using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;


public class PracticeSetManagerScript : MonoBehaviour
{
    public TMP_InputField input;
    public GameObject object1, object2, object3, object4, object5, object6;

    public Button backwards, forwards, search;

    List<string> aslList;
    private ArrayList filterAslList;

    private int index = 0;

    private int viewAmount = 6;

    List<GameObject> objectUI;

    public GameObject videoCanvas;
    public GameObject VideoUI;
    public GameObject ObjectNameUI;
    private VideoClip videoClip;
    public VideoPlayer videoPlayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        objectUI = new List<GameObject>
        {
            object1, object2, object3, object4, object5, object6
        };

        addAslListeners();

        search.onClick.AddListener(() => searchList());
        backwards.onClick.AddListener(() => moveBackwards());
        forwards.onClick.AddListener(() => moveForward());

        VideoClip[] videoClips = Resources.LoadAll<VideoClip>("SigningVideos/dpan_source_videos");
        
        aslList = videoClips.Select(clip => clip.name).ToList();
        filterAslList = new ArrayList(aslList);

        showObjects();
    }

    void playVideo(string name)
    {
        UpdateUI(name);
        videoCanvas.SetActive(true);

    }

    void showObjects()
    {
        for (int i = 0; i < viewAmount; ++i)
        {
            if(i < filterAslList.Count)
            {
                objectUI[i].GetComponentInChildren<TMP_Text>().text = (string)filterAslList[index + i];
                objectUI[i].SetActive(true);
            }
            else
            {
                objectUI[i].SetActive(false);
            }
            
        }
    }

    void searchList()
    {
        string filter = input.text;

        filterAslList = new ArrayList();

        foreach (string name in aslList)
        {
            if (name is string str && str.Contains(filter))
            {
                filterAslList.Add(name);
            }
        }

        showObjects();
    }

    void moveBackwards()
    {
        if(index >= viewAmount)
        {
            index -= viewAmount;
        }
    }

    void moveForward()
    {
        if(index < aslList.Count - viewAmount)
        {
            index += viewAmount;
        }
    }

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

    // Update is called once per frame
    void Update()
    {
        showObjects();
    }
    void addAslListeners()
    {
        Button button1 = object1.GetComponent<Button>();
        button1.onClick.AddListener(() => playVideo(object1.GetComponentInChildren<TMP_Text>().text));

        Button button2 = object2.GetComponent<Button>();
        button2.onClick.AddListener(() => playVideo(object2.GetComponentInChildren<TMP_Text>().text));

        Button button3 = object3.GetComponent<Button>();
        button3.onClick.AddListener(() => playVideo(object3.GetComponentInChildren<TMP_Text>().text));

        Button button4 = object4.GetComponent<Button>();
        button4.onClick.AddListener(() => playVideo(object4.GetComponentInChildren<TMP_Text>().text));

        Button button5 = object5.GetComponent<Button>();
        button5.onClick.AddListener(() => playVideo(object5.GetComponentInChildren<TMP_Text>().text));

        Button button6 = object6.GetComponent<Button>();
        button6.onClick.AddListener(() => playVideo(object6.GetComponentInChildren<TMP_Text>().text));
    }
}
