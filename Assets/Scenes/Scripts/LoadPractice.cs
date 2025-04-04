using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;


public class LoadPractice : MonoBehaviour
{
    public Button practice;
    public GameObject aslObject;

    public GameObject videoCanvas;

    public Button practiceButton;

    public ARCameraManager arCameraManager;
    private bool facingUser = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        practice.onClick.AddListener(() => loadPractice());
    }

    void loadPractice()
    {
        //PlayerPracticeManagerScript.signName = aslObject.GetComponent<TMP_Text>().text;
        flipCamera();
        videoCanvas.SetActive(false);
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
