using Model;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

public class PlayerPracticeManagerScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public GameObject miniVideoCanvas;
    public GameObject VideoUI;
    public GameObject ObjectNameUI;

    public GameObject oldVideoCanvas;
    public GameObject oldObjectNameUI;
    public GameObject practiceSetCanvas;

    private VideoClip videoClip;
    public VideoPlayer videoPlayer;

    public Button exitButton;

    public Button practiceButton;
    private bool facingUser = false;

    public GameObject homeButton;

    public XROrigin origin;

    //[SerializeField] private ARSession arSession;
    //[SerializeField] private GameObject mainCamera;

    [SerializeField] private VisionEngine visionEngine;


    void Start()
    {
        //arCameraManager.requestedFacingDirection = CameraFacingDirection.User;
        exitButton.onClick.AddListener(() => exit());
        practiceButton.onClick.AddListener(() => loadPractice());
    }

    void loadPractice()
    {
        //PlayerPracticeManagerScript.signName = aslObject.GetComponent<TMP_Text>().text;
        visionEngine.StopAllCameras();
        visionEngine.StartFrontCamera();
        //flipCamera();

        homeButton.SetActive(false);
        practiceSetCanvas.SetActive(false);
        oldVideoCanvas.SetActive(false);

        miniVideoCanvas.SetActive(true);

        UpdateMiniUI();

    }

    //private void flipCamera()
    //{
    //    //arSession.enabled = false;

    //    if (facingUser)
    //    {
    //        origin.GetComponent<ARPlaneManager>().enabled = true;
    //        origin.GetComponent<ARFaceManager>().enabled = false;

    //        //mainCamera.GetComponent<ARCameraManager>().requestedFacingDirection = CameraFacingDirection.World;
    //        //Debug.Log(mainCamera.GetComponent<ARCameraManager>().requestedFacingDirection);


    //        facingUser = false;
    //    }
    //    else
    //    {
    //        origin.GetComponent<ARPlaneManager>().enabled = false;
    //        origin.GetComponent<ARFaceManager>().enabled = true;

    //        //mainCamera.GetComponent<ARCameraManager>().requestedFacingDirection = CameraFacingDirection.User;
    //        //Debug.Log(mainCamera.GetComponent<ARCameraManager>().requestedFacingDirection);

    //        facingUser = true;
    //    }

    //    //arSession.enabled = true;
    //    arSession.Reset();
    //    TouchManagerScript.facingUser = facingUser;

    //}

    void exit()
    {
        practiceSetCanvas.SetActive(true);
        oldVideoCanvas.SetActive(true);
        homeButton.SetActive(true);

        miniVideoCanvas.SetActive(false);
        visionEngine.StopAllCameras();
        visionEngine.StartBackCamera();

    }

    // Update is called once per frame
    void Update()
    {
        if (visionEngine.handResult != null)
        {
            List<string> classes = visionEngine.handResult.Classes;
            if (classes[0] == oldObjectNameUI.GetComponent<TMP_Text>().text)
            {
                correct();
            }
        }
    }

    public void UpdateMiniUI()
    {
        string name = oldObjectNameUI.GetComponent<TMP_Text>().text;

        if (name == null)
        {
            name = "dog";
        }
        //name = objectName;
        ObjectNameUI.GetComponent<TMP_Text>().text = CapitalizeFirstLetter(name);

        videoPlayer = VideoUI.GetComponent<VideoPlayer>();

        string videoPath = "SigningVideos/dpan_source_videos/" + name;

        videoClip = Resources.Load<VideoClip>(videoPath);

        VideoUI.GetComponent<VideoPlayer>().clip = videoClip;

        if (videoClip == null)
        {
            Debug.Log("Empty");
        }

        VideoUI.GetComponent<VideoPlayer>().Play();
    }

    private string CapitalizeFirstLetter(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        return char.ToUpper(name[0]) + name.Substring(1);
    }

    void correct()
    {
        string correctWord = oldObjectNameUI.GetComponent<TMP_Text>().text;
        PlayerPrefs.SetInt(correctWord, 1);
        PlayerPrefs.SetInt("totalCorrect", PlayerPrefs.GetInt("totalCorrect"));

        exit();
        
    }
}
