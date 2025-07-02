using Unity.VisualScripting;
using UnityEngine;

public class PauseCamera : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Camera cameraObject;
    [SerializeField] private Camera pauseCamera;

    private void Awake()
    {
        pauseCamera.enabled = false;
    }

    private void Update()
    {
        if (!cameraObject.enabled)
        {
            pauseCamera.enabled = true;
            pauseCamera.transform.rotation = cameraTransform.rotation;
            pauseCamera.transform.position = cameraTransform.position;
        }
        else
        {
            pauseCamera.enabled = false;
        }
    }


}

