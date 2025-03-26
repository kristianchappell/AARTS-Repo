using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

public class StartLiveFeed : MonoBehaviour
{
    public void StartLive()
    {
        SceneManager.LoadSceneAsync("ARScene");
        LoaderUtility.Deinitialize();
        LoaderUtility.Initialize();
    }
}
