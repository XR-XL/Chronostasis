using Unity.VisualScripting;
using UnityEngine;

public class SlashBehaviour : MonoBehaviour
{
    [SerializeField] private int attackDamage = 1;
    public LayerMask attackLayer;
    public float hitRegTimeLeniency = 0.2f;


    private void FixedUpdate()
    {
        SlashCollision();
    }
    // in a nutshell: this checks for collisions and then deal damage to objects and other collidable stuff
    void SlashCollision()
    {
        var vector3HalfExtents = new Vector3(2f, 1f, 2f);
        if (GameManager.Instance.timestopTriggered)
        {
            vector3HalfExtents *= 2;
        }
        Collider[] hitColliders = Physics.OverlapBox(gameObject.transform.position, vector3HalfExtents, Quaternion.identity, attackLayer);
        int i = 0;
        
        if (!GameManager.Instance.timestopTriggered)
        {
            while (i < hitColliders.Length)
            {
                // Output all of the collider names
                Debug.Log("Hit : " + hitColliders[i].name + i);

                if (hitColliders[i].TryGetComponent(out Enemy T))
                {
                    T.TakeDamage(attackDamage);
                }

                // Increase the number of Colliders in the array
                i++;
            }
            Destroy(gameObject, hitRegTimeLeniency);
        }
        
    }
    // draws a box for me to refer to its hitbox in editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        var vector3HalfExtents = new Vector3(2f, 1f, 2f);
        if (GameManager.Instance.timestopTriggered)
        {
            vector3HalfExtents *= 2;
        }
        // Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode
        if (Application.isPlaying)
            // Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
            Gizmos.DrawWireCube(transform.position, vector3HalfExtents);
    }
}
