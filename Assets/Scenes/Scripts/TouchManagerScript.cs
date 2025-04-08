using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Model;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine.Video;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using Unity.XR.CoreUtils;
using System.Linq;
public class TouchManagerScript : MonoBehaviour
{

    private PlayerInput playerInput;

    private InputAction touchPositionAction;
    private InputAction touchPressAction;
    [SerializeField] private TextAsset detectionModel;
    private Model.MPObjectDetection model;

    public GameObject videoCanvas;
    public GameObject VideoUI;
    public GameObject ObjectNameUI;

    public Button videoButtonExit;

    public GameObject homeCanvas;
    public Button homeButton;

    public GameObject practiceSet;

    private VideoClip videoClip;
    public VideoPlayer videoPlayer;

    [SerializeField] private GameObject totalComplete;

    public static bool facingUser = false;

    public GameObject background;


    public void UpdateUI(string name)
    {
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
        else
        {
            VideoUI.GetComponent<VideoPlayer>().Play();
        }
    }

    private string CapitalizeFirstLetter(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        return char.ToUpper(name[0]) + name.Substring(1);
    }

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        touchPressAction = playerInput.actions["TouchPress"];
        touchPositionAction = playerInput.actions["TouchPosition"];
        model = new Model.MPObjectDetection(detectionModel);

        homeButton.onClick.AddListener(openHome);
        videoButtonExit.onClick.AddListener(closeVideo);

        int completed = PlayerPrefs.GetInt("totalCorrect");
        int total = Resources.LoadAll<VideoClip>("SigningVideos/dpan_source_videos").Length;

        totalComplete.GetComponent<TMP_Text>().text = "Words Learned: " + completed + "/" + total;
    }

    void openHome()
    {
        // Set the UI element's active property to true
        homeCanvas.SetActive(true);
        videoCanvas.SetActive(false);

    }

    void closeVideo()
    {
        videoCanvas.SetActive(false);
        background.SetActive(false);

    }

    private void OnEnable()
    {
        touchPressAction.performed += TouchPressed;
    }

    private void OnDisable()
    {
        touchPressAction.performed -= TouchPressed;
    }

    private int frame;

    private bool ScanActive(Vector2 position)
    {
        if (position[0] > 0 && position[0] < 200 && position[1] > 1750 && position[1] < 1950)
        {
            return false;
        }

        return !videoCanvas.activeSelf && !homeCanvas.activeSelf && !practiceSet.activeSelf && !facingUser;
    }

    private void TouchPressed(InputAction.CallbackContext context)
    {
        Vector2 position = touchPositionAction.ReadValue<Vector2>();

        if (ScanActive(position)) {

            Texture2D image = TakeScreenshot();
            ArrayList classes = model.ProcessScreen(image, position);


            //Object Recong model
            if (classes.Count > 0)
            {
                string objectName = (string)classes[0];
                UpdateUI(objectName);

                videoCanvas.SetActive(true);

            }

            //setBackground(image);
        }

        //string path = Path.Combine(Application.persistentDataPath, "${frame}.png");
        //Debug.Log(path);

        //File.WriteAllBytes(path, image.EncodeToPNG());

        //string filename = "Assets/Scenes/Sprites/car.png";
        //Texture2D LoadPNG(string fileName)
        //{
        //    string path = fileName;

        //    if (!File.Exists(path))
        //    {
        //        Debug.LogError("File not found at: " + path);
        //        return null;
        //    }

        //    byte[] fileData = File.ReadAllBytes(path);
        //    Texture2D texture = new Texture2D(2, 2); // Dummy size, LoadImage will resize it
        //    if (texture.LoadImage(fileData)) // Load PNG data
        //    {
        //        return texture;
        //    }

        //    Debug.LogError("Failed to load texture.");
        //    return null;
        //}

        // LoadPNG(filename); // TakeScreenshot();

        //var rawData = System.IO.File.ReadAllBytes(filename);
        //if (!(image.LoadImage(rawData))) {
        //    Debug.Log(image.LoadImage(rawData));   
        //}


        //string path = Path.Combine(Application.persistentDataPath, "${frame}.png");
        //// Debug.Log(path);
        //++frame;

        //File.WriteAllBytes(path, image.EncodeToPNG());
    }

    private void setBackground(Texture2D image)
    {
        Image screenShot = background.GetComponent<Image>();

        screenShot.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), new Vector2(0.5f, 0.5f));
        background.SetActive(true);
    }

    private Texture2D TakeScreenshot()
    {
        UnityEngine.Camera camera = UnityEngine.Camera.main;
        int width = Screen.width;
        int height = Screen.height;
        RenderTexture rt = new RenderTexture(width, height, 24);
        camera.targetTexture = rt;

        // The Render Texture in RenderTexture.active is the one
        // that will be read by ReadPixels.
        var currentRT = RenderTexture.active;
        RenderTexture.active = rt;

        // Render the camera's view.
        camera.Render();

        Texture2D image = new Texture2D(width, height);
        image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        image.Apply();

        camera.targetTexture = null;

        // Replace the original active Render Texture.
        RenderTexture.active = currentRT;

        return image;
    }
}
