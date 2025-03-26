using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Model;
using System.Collections;
using System.IO;
public class TouchManagerScript : MonoBehaviour
{

    private PlayerInput playerInput;

    private InputAction touchPositionAction;
    private InputAction touchPressAction;
    [SerializeField] private TextAsset detectionModel;
    private Model.MPObjectDetection model;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        touchPressAction = playerInput.actions["TouchPress"];
        touchPositionAction = playerInput.actions["TouchPosition"];
        model = new Model.MPObjectDetection(detectionModel);


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

    private void TouchPressed(InputAction.CallbackContext context)
    {
        Texture2D LoadPNG(string fileName)
        {
            string path = fileName;

            if (!File.Exists(path))
            {
                Debug.LogError("File not found at: " + path);
                return null;
            }

            byte[] fileData = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2); // Dummy size, LoadImage will resize it
            if (texture.LoadImage(fileData)) // Load PNG data
            {
                return texture;
            }

            Debug.LogError("Failed to load texture.");
            return null;
        }

        Vector2 position = touchPositionAction.ReadValue<Vector2>();
        string filename = "Assets/Scenes/Sprites/car.png";

        Texture2D image = TakeScreenshot();
        // LoadPNG(filename); // TakeScreenshot();

        //var rawData = System.IO.File.ReadAllBytes(filename);
        //if (!(image.LoadImage(rawData))) {
        //    Debug.Log(image.LoadImage(rawData));   
        //}


        //string path = Path.Combine(Application.persistentDataPath, "${frame}.png");
        //// Debug.Log(path);
        //++frame;

        //File.WriteAllBytes(path, image.EncodeToPNG());

        ArrayList classes = model.ProcessScreen(image, position);

        //Debug.Log(classes.Count);

        //Object Recong model
        if (classes.Count > 0)
        {
            ScannedObjectManagerScript.objectName = (string) classes[0];

            SceneManager.LoadSceneAsync("SelectedObjectVideo");
        }
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
