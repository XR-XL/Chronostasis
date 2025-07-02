using UnityEngine;

public struct CameraInput
{
    public Vector2 Look;
}

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private float sensitivity;
    private Vector3 _eulerAngles;


    // camera follows the transform position given by input map (Vector2)
    public void Initialize(Transform target)
    {
        transform.position = target.position;
        transform.eulerAngles = _eulerAngles = target.eulerAngles;
    }

    public void UpdateRotation(CameraInput input)
    {
        _eulerAngles += new Vector3(-input.Look.y, input.Look.x) * sensitivity;
        _eulerAngles.x = Mathf.Clamp(_eulerAngles.x, -90, 90);
        transform.eulerAngles = _eulerAngles;
    }

    public void UpdateLocation(Transform target)
    {
        transform.position = target.position;
    }

}
