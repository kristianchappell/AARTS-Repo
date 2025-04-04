using NUnit.Framework.Internal;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
using System.IO;

public class ScannedObjectManagerScript : MonoBehaviour
{
    public GameObject VideoUI;

    public GameObject ObjectNameUI;

    private VideoClip videoClip;

    public VideoPlayer videoPlayer;

    //public static string objectName;

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

        VideoUI.GetComponent<VideoPlayer>().Play();
    }

    private string CapitalizeFirstLetter(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        return char.ToUpper(name[0]) + name.Substring(1);
    }

    public void Update()
    {
        //UpdateUI();
    }
}
