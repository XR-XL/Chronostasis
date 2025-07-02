using KinematicCharacterController;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.VFX;

public struct CharacterInput
{
    public Quaternion Rotation;
    public Vector2 Move;
    public bool Jump;
    public bool JumpSustain;
    public bool Dash;
    public CrouchInput Crouch;
    public bool Timestop;
    public bool Attack;
}

public enum CrouchInput
{
    None, Toggle, Pressed, Released
}

public enum Stance
{
    Stand, Crouch, Slide
}

public struct CharacterState
{
    public bool Grounded;
    public Stance Stance;
    public Vector3 Velocity;

}

public class PlayerCharacter : MonoBehaviour, ICharacterController
{
    // exposes the parameter to add object references and add adjustable values
    [Header("Object references")]
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Transform root;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private VisualEffect vfx_slash;
    [SerializeField] private PlayerCamera playerCamera;
    [Space]
    [Header("Speed responces")]
    [SerializeField] private float walkSpeed = 14f;
    [SerializeField] private float crouchSpeed = 7f;
    [SerializeField] private float walkResponse = 25f;
    [SerializeField] private float crouchResponse = 20f;
    [Space]
    [Header("Dash system")]
    public float dashCooldown = 2f;
    [SerializeField] private float dashSpeed = 700f;
    [SerializeField] private float dashGroundMultiplier = 10f;
    public float dashCounter = 2;
    [SerializeField] private float dashGravityDelayTime = 0.5f;
    [Space]
    [Header("Air movement")]
    [SerializeField] private float airSpeed = 30f;
    [Range(0f, 1000f)]
    [SerializeField] private float airAcceleration = 70f;
    [Space]
    [Header("Jump system")]
    [SerializeField] private float jumpSpeed = 50f;
    [SerializeField] private float jumpCounter = 2;
    [Range(0f, 1f)] // when you hold jump, you should jump a bit higher to respect player intention
    [SerializeField] private float jumpSustainGravity = 0.4f; // reduce gravity when button held
    [SerializeField] private float gravity = -98.1f;
    [Space]
    [Header("Slide system")]
    [SerializeField] private float slideStartSpeed = 25f;
    [SerializeField] private float slideEndSpeed = 15f;
    [SerializeField] private float slideFriction = 0.8f; 
    [SerializeField] private float slideSteerAcceleration = 5f;
    [SerializeField] private float slideGravity = -98.1f;
    [Space]
    [Header("Crouch control")]
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchHeightResponce = 15f;
    // % based height, adjust with slider
    [Range(0f, 1f)] 
    [SerializeField] private float standCameraTargetHeight = 0.9f;
    [Range(0f, 1f)]
    [SerializeField] private float crouchCameraTargetHeight = 0.7f;
    [Space]
    [Header("Timestop system")]
    [SerializeField] private float timestopSeconds = 5f;
    [Range(0f, 1f)]
    [SerializeField] private float timestopRecoveryDelayMultiplier = 0.5f;
    [Space]
    [Header("Attack attributes")]
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float attackTime = 0.4f;
    [Space]
    [Header("Public data")]
    public DashBar dashBar;
    public TimestopBar timestopBar;
    [Space]
    [Header("Misc.")]
    public float _currentSpeedX;
    public float _currentSpeedY;
    public float _currentSpeedZ;
    public Vector3 _currentSpeed;


    private CharacterState _state;
    private CharacterState _lastState;
    private CharacterState _tempState; // placehold for last state

    private Color _debugColor; // debugging use

    private Quaternion _requestedRotation;
    private Vector3 _requestedMovement;
    private bool _requestedJump;
    private bool _requestedSustainedJump;
    private bool _requestedCrouch;
    private bool _requestedCrouchInAir;
    private bool _requestedDash;
    private bool _requestedTimestop;
    private bool _requestedAttack;

    private bool _dashReady;
    private bool _applyGravity;

    private bool _attackReady;

    private Vector3 lastMovementDirection;

    private Collider[] _uncrouchOverlapResults;

    private RaycastHit _meleeHitRegister;
    private bool _meleeHitConfirmation;


    public void Initialize()
    {
        _state.Stance = Stance.Stand;
        _lastState = _state;
        _uncrouchOverlapResults = new Collider[8];
        _applyGravity = true;

        motor.CharacterController = this;


    }

    // updates input, called per tick
    public void UpdateInput(CharacterInput input)
    {
        _requestedRotation = input.Rotation;
        _requestedMovement = new Vector3(input.Move.x, 0f, input.Move.y);
        // clamp movement such that if you move diagonally the magnetude is still 1
        // using vector.normalise here forces the vector to be 1 so we use a clamp
        _requestedMovement = Vector3.ClampMagnitude(_requestedMovement, 1f);

        // dash handling
        _requestedDash = _requestedDash || input.Dash;

        // now, we match the movement with the camera rotation
        _requestedMovement = input.Rotation * _requestedMovement;

        // jumping handling
        _requestedJump = _requestedJump || input.Jump;

        _requestedSustainedJump = input.JumpSustain;

        // crouch handling
        // side not to self: consider adding a toggle to swtich between hold/ toggle crouch

        var wasRequestingCrouch = _requestedCrouch;
        _requestedCrouch = input.Crouch switch
        {
            CrouchInput.Toggle => !_requestedCrouch,
            CrouchInput.None => _requestedCrouch,
            _ => _requestedCrouch 
        };
        if (_requestedCrouch && !wasRequestingCrouch)
            _requestedCrouchInAir = !_state.Grounded;
        else if (!_requestedCrouch && wasRequestingCrouch)
            _requestedCrouchInAir = false;

        // timestop system
        // logs and keep track of timestop; calls gamemanager singleton object
        // simulate timestop by stopping all movement when gamemanager returns bool is true
        // done in external scripts
        if (input.Timestop)
        {
            _requestedTimestop = !_requestedTimestop;

        }

        if (_requestedTimestop && timestopSeconds > 0)
        {
            GameManager.Instance.UpdateTimestopStatus(_requestedTimestop);
        }
        else
        {
            _requestedTimestop = false;
            GameManager.Instance.UpdateTimestopStatus(_requestedTimestop);
        }

        if (GameManager.Instance.timestopTriggered)
        {
            timestopSeconds -= Time.deltaTime;
            Debug.Log(timestopSeconds);
        }

        if (timestopSeconds < 5f && !GameManager.Instance.timestopTriggered)
        {
            timestopSeconds += Time.deltaTime * timestopRecoveryDelayMultiplier;
        }
        if (timestopSeconds > 5f)
        {
            timestopSeconds = 5;
        }

        timestopBar.SetTimestopBar(Mathf.Clamp01(timestopSeconds/5));
        
        // attack handling
        _requestedAttack = _requestedAttack || input.Attack;

        if (!_attackReady)
        {
            attackCooldown -= Time.deltaTime;
        }

        if (attackCooldown <= 0)
        {
            _attackReady = true;
            attackCooldown = 0.5f;
        }

        if (_requestedAttack) 
        {
            AttackEnemy();
            AttackTimeWait(attackTime);
        }

        if (dashCounter == 0)
        {
            dashBar.SetDashOne(Mathf.Clamp01((2 - dashCooldown) / 2));
            dashBar.SetDashTwo(0);
        }

        else if (dashCounter == 1)
        {
            dashBar.SetDashTwo(Mathf.Clamp01((2 - dashCooldown) / 2));
        }

    }

    public void UpdateBody(float deltaTime)
    {
        var currentHeight = motor.Capsule.height;
        var normalisedHeight = currentHeight / standHeight; // gives a num range [0,1]

        var cameraTargetHeight = currentHeight * 
            (
                _state.Stance is Stance.Stand
                    ? standCameraTargetHeight
                    : crouchCameraTargetHeight
            );

        var rootTargetScale = new Vector3(1f, normalisedHeight, 1f);
        // when the current height drops because of this update function, it will shrink the root of the character
        // in which, is the base model

        cameraTarget.localPosition = Vector3.Lerp
        (
            // side note: damp the lerp function using some maths
            // this returns a more stable interpolation and is frame rate independent
            a: cameraTarget.localPosition,
            b: new Vector3(0f, cameraTargetHeight, 0f),
            t: 1f - Mathf.Exp(-crouchHeightResponce * deltaTime)

        );
        root.localScale = Vector3.Lerp
        (

            a: root.localScale,
            b: rootTargetScale,
            t: 1f - Mathf.Exp(-crouchHeightResponce * deltaTime)

        );

    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        // dash cooldown system
        // count for cooldown if dash less than 2
        if (_lastState.Velocity != Vector3.zero)
        {
            lastMovementDirection = _lastState.Velocity.normalized;
        }

        if (dashCounter < 2)
        {
            dashCooldown -= Time.deltaTime;

        }

        if (dashCooldown <= 0)
        {
            if (dashCounter < 2)
            {
                dashCounter += 1;
            }
            dashCooldown = 5f;
        }

        // dashing system
        if (_requestedDash && _dashReady)
        {

            {
                _requestedDash = false;
                _requestedCrouch = false;

                // placeholder for requested movement to avoid null case
                var dashDirection = (_requestedMovement.sqrMagnitude != 0)
                    // returns the normalised direction of movement if there exist a _requestedMovement
                    ? Vector3.Normalize(Vector3.ProjectOnPlane(vector: _requestedMovement, planeNormal: motor.CharacterUp))
                    // gives the forward vector otherwise, catching both cases
                    : lastMovementDirection;
                if (motor.GroundingStatus.IsStableOnGround) // dashing on ground significantly dampens the speed
                { 
                currentVelocity += dashDirection * dashSpeed * dashGroundMultiplier;
                dashCounter -= 1;
                }
                else
                {
                    currentVelocity += dashDirection * dashSpeed;
                    dashCounter -= 1;
                    StartCoroutine(GravityOnDelay(dashGravityDelayTime));
                }
            }
        }
        else
        {
            _requestedDash = false;
        }

        // checking dashing readiness
        if (dashCounter > 0) {_dashReady = true;}
        else{ _dashReady = false;}

        // grounding check
        if (motor.GroundingStatus.IsStableOnGround)
        {
            jumpCounter = 2;

            // get direction of motion tangent to the player
            var groundedMovement = motor.GetDirectionTangentToSurface // using the motor component from the import
            (

                    direction: _requestedMovement,
                    surfaceNormal: motor.GroundingStatus.GroundNormal

            ) * _requestedMovement.magnitude; // grounded movement returns unit vector

            // sliding
            // side note to self: I need to edit this code later if I want to implement crouch slam

            {
                var moving = groundedMovement.sqrMagnitude > 0f;
                var crouching = _state.Stance is Stance.Crouch;
                var wasStanding = _lastState.Stance is Stance.Stand;
                var wasInAir = !_lastState.Grounded;

                if (moving && crouching && (wasStanding || wasInAir))
                {
                    _state.Stance = Stance.Slide;

                    // Velocity is projected on flat ground plane's normal
                    // to remedy the negation of speed/momentum presevation, we use last frame's vector to calculate speed

                    if (wasInAir)
                    {
                        currentVelocity = Vector3.ProjectOnPlane
                        (
                            vector: _lastState.Velocity,
                            planeNormal: motor.GroundingStatus.GroundNormal
                        );
                    }

                    // check to see if manual input was given in air
                    var effectiveStartSlideSpeed = slideStartSpeed;
                    if (!_lastState.Grounded && !_requestedCrouchInAir)
                    {
                        effectiveStartSlideSpeed = 0f;
                        _requestedCrouchInAir = false;
                    }
                    var slideSpeed = Mathf.Max(effectiveStartSlideSpeed, currentVelocity.magnitude);
                    currentVelocity = motor.GetDirectionTangentToSurface
                    (
                        direction: currentVelocity,
                        surfaceNormal: motor.GroundingStatus.GroundNormal
                    ) * slideSpeed;
                }
            }


            // grounded movement 
            if (_state.Stance is Stance.Stand or Stance.Crouch)
            {

                // calculate the responce and movement speed to smooth out movement
                // it also respects a lot more player intention
                var speed = _state.Stance is Stance.Stand
                    ? walkSpeed
                    : crouchSpeed;
                var response = _state.Stance is Stance.Stand
                    ? walkResponse
                    : crouchResponse;

                // move along with the direction then multiply by the walkspeed
                var targetVelocity = groundedMovement * speed;
                currentVelocity = Vector3.Lerp
                (

                    a: currentVelocity,
                    b: targetVelocity,
                    t: 1f - Mathf.Exp(-response * deltaTime)

                );
            }
            else
            {
                // sliding friction
                currentVelocity -= currentVelocity * (slideFriction * deltaTime);

                // calculate slope and add force
                {
                    var force = Vector3.ProjectOnPlane
                    (
                        vector: motor.CharacterUp,
                        planeNormal: motor.GroundingStatus.GroundNormal
                    ) * slideGravity;

                    currentVelocity += force * deltaTime;
                }

                // steering on slide

                {
                    // find target velocity, which we determine by directional input from player
                    var currentSpeed = currentVelocity.magnitude;
                    var targetVelocity = groundedMovement * currentSpeed;
                    var steerForce = (targetVelocity - currentVelocity) * slideSteerAcceleration * deltaTime;

                    currentVelocity += steerForce;
                    currentVelocity = Vector3.ClampMagnitude(currentVelocity, currentSpeed);
                }

                // stop sliding when speed drops below the minimum
                if (currentVelocity.magnitude < slideEndSpeed)
                {
                    _state.Stance = Stance.Crouch;
                }
            }
        }
        // air controls and movements
        else
        {
            // air strafing/moving
            if (_requestedMovement.sqrMagnitude > 0f)
            {
                var planarMovement = Vector3.Normalize(
                Vector3.ProjectOnPlane
                (
                    vector: _requestedMovement,
                    planeNormal: motor.CharacterUp
                )

                ) * _requestedMovement.magnitude;

                // current velocity and preservation
                var currentPlanarVelocity = Vector3.ProjectOnPlane
                (
                    vector: currentVelocity,
                    planeNormal: motor.CharacterUp
                );

                // calculate final movement
                var movementForce = planarMovement * airAcceleration * deltaTime;

                if (currentPlanarVelocity.magnitude < airSpeed)
                {
                    // create a target velocity so speed does not increase indefinitely
                    var targetPlanerVelocity = currentPlanarVelocity + movementForce;

                    // limit the target velocity to the preset air speed
                    targetPlanerVelocity = Vector3.ClampMagnitude(targetPlanerVelocity, airSpeed);

                    // steering (strafing)
                    movementForce = targetPlanerVelocity - currentPlanarVelocity;
                }
                // Slow movement force to stop infinite acceleration
                else if (Vector3.Dot(movementForce, currentPlanarVelocity) > 0f)
                {
                    // project movement onto plane normal to velocity
                    var constrainedMovementForce = Vector3.ProjectOnPlane
                    (
                        vector: movementForce,
                        planeNormal: currentPlanarVelocity.normalized
                    );

                    movementForce = constrainedMovementForce;
                }

                // prevent climbing slopes
                if (motor.GroundingStatus.FoundAnyGround)
                {
                    // check to see if player is in same direction as the resultant velocity
                    if (Vector3.Dot(movementForce, currentVelocity + movementForce) > 0f)
                    {
                        // get the normal of the obstruction
                        var obstructionNormal = Vector3.Cross
                        (
                            motor.CharacterUp,
                            Vector3.Cross
                            (
                                motor.CharacterUp,
                                motor.GroundingStatus.GroundNormal
                            )

                        ).normalized;

                        // prevent movement force that would boost the player
                        movementForce = Vector3.ProjectOnPlane(movementForce, obstructionNormal);
                    }
                }

                currentVelocity += movementForce;
            }

            if (_applyGravity) // allows manipulation of gravity
            {
                // apply gravity (which we set in the headers)
                // minor note: whenever we need to change gravity application I can always modify here
                var effectiveGravity = gravity;
                var verticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
                if (_requestedSustainedJump && verticalSpeed > 0f)
                    effectiveGravity *= jumpSustainGravity;
                currentVelocity += motor.CharacterUp * effectiveGravity * deltaTime;
            }
        }

        if (_requestedJump)
        {
            if (jumpCounter > 0)
            {
                _requestedJump = false; // sets the variable back to false once we begin to process the code
                _requestedCrouch = false; // reset crouch when jumping
                _requestedCrouchInAir = false;

                // unstick the character from ground
                motor.ForceUnground(time: 0.1f);

                // set a minimum velocity to the jump (which makes it so you always jump the same height)
                // so compare the current vs the targeted speed (which is the raw jumpSpeed var)
                var currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
                var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);

                // add the difference to the character's velocity, making it so it always apply a set velocity up
                currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
                jumpCounter -= 1;
            }
            else
            {
                _requestedJump = false;
            }
        }

        #if UNITY_EDITOR
        Debug.DrawLine(root.transform.localPosition, currentVelocity, _debugColor = Color.red);
        #endif

        _currentSpeedX = currentVelocity.x;
        _currentSpeedY = currentVelocity.y;
        _currentSpeedZ = currentVelocity.z;
        _currentSpeed = currentVelocity;
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        // rotation is apllied only in 2d, with no vertical rotation

        var forward = Vector3.ProjectOnPlane
        (
            _requestedRotation * Vector3.forward,
            motor.CharacterUp
        );

        if (forward != Vector3.zero) // math breaks when look straight up, so we amend that
        {
            currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
        }
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
        _tempState = _state;
        // Checks crouch
        if (_requestedCrouch && _state.Stance is Stance.Stand)
        {
            _state.Stance = Stance.Crouch;
            motor.SetCapsuleDimensions
            (
                radius: motor.Capsule.radius,
                height: crouchHeight,
                yOffset: crouchHeight * 0.5f
            );
        }
    }

    public void PostGroundingUpdate(float deltaTime)
    {
        if (!motor.GroundingStatus.IsStableOnGround && _state.Stance is Stance.Slide) 
        {
            _state.Stance = Stance.Crouch;
        }
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        // Checks uncrouch
        if (!_requestedCrouch && _state.Stance is not Stance.Stand)
        {
            _state.Stance = Stance.Stand;
            motor.SetCapsuleDimensions
            (
                radius: motor.Capsule.radius,
                height: standHeight,
                yOffset: standHeight * 0.5f
            );
        }
        // check for colliding overlaps for uncrouching
        var pos = motor.TransientPosition;
        var rot = motor.TransientRotation;
        var mask = motor.CollidableLayers;
        if (motor.CharacterOverlap
            (
                pos, 
                rot, 
                _uncrouchOverlapResults, 
                mask, 
                QueryTriggerInteraction.Ignore
            ) > 0) // this tells us that if the overlap object exists (>0)
        {
            // recrouch if collider tells us there are collisions
            _requestedCrouch = true;
            motor.SetCapsuleDimensions
            (
                radius: motor.Capsule.radius,
                height: crouchHeight,
                yOffset: crouchHeight * 0.5f
            );
        }

        // sync state with motor properties
        _state.Grounded = motor.GroundingStatus.IsStableOnGround;
        _state.Velocity = motor.Velocity;
        // update the last state with the snapshot we took during the earlier character update
        _lastState = _tempState;
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        return coll != null && coll.gameObject != null && coll.gameObject.GetComponent<Collider>() != null;
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
        
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
        
    }

    /// <summary>
    /// Below are misc. functions that works outside of the normal KCC interface
    /// Most non-movement related stuff (e.g. attack, health etc.) will be included here
    /// </summary>


    public Transform GetCameraTarget() => cameraTarget;

    // debug only - remind myself to delete later
    public void SetPosition(Vector3 position, bool killvelocity = true)
    {
        motor.SetPosition(position);
        if (killvelocity) 
            motor.BaseVelocity = Vector3.zero;
    }

    // Attack settings

    public void AttackEnemy() 
    {
        // I just calculated center via local transform

        var castCenterOffset = new Vector3(0, 0, 0.62f);
        var castCameraLocation = playerCamera.transform.position;

        var castCenter = playerCamera.transform.TransformPoint(castCenterOffset);

        // manually set - need to change if change vfx

        var vector3HalfExtents = new Vector3(1.25f, 1f, 1.25f);

        // in a nutshell: VFX slash is a prefab - I spawn in an instance of it when i attack
        // the prefab instance also has a collider
        // parent the transform if slashing in non timestop
        // removes the transform if slashing in timestop, also delays the slash to make a cool effect

        // as for attacks: boxcast - if in timestop, store the hit, apply it after timestop ends
        if (_requestedAttack && _attackReady)
        {
            _attackReady = false;
            _requestedAttack = false;
            var updirection = motor.CharacterUp;
            var slashDirection = castCenter - cameraTarget.position;
            var slashDirectionQuaternion = Quaternion.LookRotation(slashDirection, updirection);

            if (!GameManager.Instance.timestopTriggered)
            {
                // this casts a hitbox, collider is in the prefab

                    VisualEffect slashvfxinstance = Instantiate
                    (
                    vfx_slash,
                    position: castCenter,
                    rotation: slashDirectionQuaternion,
                    parent: root

                    );
                
                Destroy(slashvfxinstance.gameObject, 1);

            }
            else
            {
                VisualEffect slashvfxinstance = Instantiate
                (
                    vfx_slash,
                    position: castCenter,
                    rotation: slashDirectionQuaternion

                );
                // includes hit detection in coroutine
                StartCoroutine(SuspendSlashes(slashvfxinstance));
                Destroy(slashvfxinstance.gameObject, 8);
                
                
            }
            

        }
        else 
        { 
            _requestedAttack = false; 
        }
    }

    // gravity delay function, coroutine run over seconds in scaled time
    IEnumerator GravityOnDelay(float gravityDelayTime)
    {
        _applyGravity = false;
        yield return new WaitForSeconds(gravityDelayTime);
        _applyGravity = true;
    }

    // slash delay effect, just to make it look cool
    // variables pass when coroutine called - hitbox should be the exact same when after time has been unpaused, regardless of new player position
    IEnumerator SuspendSlashes(VisualEffect visualEffect)
    {
        Debug.Log("Suspend triggered");
        visualEffect.playRate = 0.0005f;
        visualEffect.transform.localScale = 1.2f * Vector3.one;

        yield return new WaitUntil(() => !GameManager.Instance.timestopTriggered);

        visualEffect.playRate = 1f;
        
    }

    IEnumerator AttackTimeWait(float attackTime)
    {
        yield return new WaitForSeconds(attackTime);
    }

}
