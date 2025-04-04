using UnityEngine;
using UnityEngine.UI;


public class CloseUIOverlay : MonoBehaviour
{
    public Button myButton; // Assign the button in the Inspector
    public GameObject uIOverlay; // Assign the UI element in the Inspector

    void Start()
    {
        // Add a listener to the button's onClick event
        myButton.onClick.AddListener(closeUI);
    }

    void closeUI()
    {
        // Set the UI element's active property to false
        uIOverlay.SetActive(false);

    }
}
