using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARCameraManagerController : MonoBehaviour
{
    public GameObject videoCanvas;

    public Button practiceButton;

    public ARCameraManager arCameraManager;
    private bool facingUser = false;

    void Start()
    {
        if (arCameraManager != null)
        {
            arCameraManager.requestedFacingDirection = CameraFacingDirection.User;
        }
    }


    private void flipCamera()
    {
        if (facingUser)
        {
            arCameraManager.requestedFacingDirection = CameraFacingDirection.World;
            facingUser = false;
        }
        else
        {
            arCameraManager.requestedFacingDirection = CameraFacingDirection.User;
            facingUser = true;
        }

    }

    private void Update()
    {
        
    }
}
