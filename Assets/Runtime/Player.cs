using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    // exposes the parameter to add object references
    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private PlayerCamera playerCamera;
    [Space]

    private PlayerInputActions _inputActions;
    private CameraInput cameraInput;

    void Start()
    {
        playerCharacter.Initialize();
        playerCamera.Initialize(playerCharacter.GetCameraTarget());

        _inputActions = new PlayerInputActions();
        _inputActions.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    void OnDestroy()
    {
        _inputActions?.Disable();
        _inputActions?.Dispose(); 
    }

    void Update()
    {

        var input = _inputActions.Gameplay;
        var deltaTime = Time.deltaTime;

        // Get camera Input and update rotation
        if (!GameManager.Instance.gamePaused)
        {
            cameraInput = new CameraInput {Look = input.Look.ReadValue<Vector2>()};
        }
        else 
        {
            cameraInput = new CameraInput {Look = Vector2.zero };
        }

        playerCamera.UpdateRotation(cameraInput);

        // get input form the character
        var characterInput = new CharacterInput
        {
            Rotation = playerCamera.transform.rotation,
            Move = input.Move.ReadValue<Vector2>(),
            Jump = input.Jump.WasPressedThisFrame(),
            JumpSustain = input.Jump.IsPressed(),
            Dash = input.Dash.WasPressedThisFrame(),
            Crouch = input.Crouch.WasPressedThisFrame()
                ? CrouchInput.Toggle
                : CrouchInput.None,
            Timestop = input.Timestop.WasPressedThisFrame(),
            Attack = input.Attack.WasPressedThisFrame()
        };
        playerCharacter.UpdateInput(characterInput);
        playerCharacter.UpdateBody(deltaTime);
        playerCamera.UpdateLocation(playerCharacter.GetCameraTarget());


        #if UNITY_EDITOR
        if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                var ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
                if (Physics.Raycast(ray, out var hit))
                {
                    Teleport(hit.point);
                }
            }
        #endif
    }

    // debug only - remind myself to delete this
    public void Teleport(Vector3 position)
    {
        playerCharacter.SetPosition(position);
    }
 
}
