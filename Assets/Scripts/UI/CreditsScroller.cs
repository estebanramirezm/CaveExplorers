using UnityEngine;
using TMPro;

public class CreditsScroller : MonoBehaviour
{
    [Header("Scrolling Settings")]
    public float scrollSpeed = 75f;
    public RectTransform creditsTextRect;
    
    [Header("End Sequence Settings")]
    public GameObject buttonContainer;
    public float timeUntilButtonsAppear = 12f; 

    private bool buttonsActive = false;

    void Start()
    {
        // Make sure buttons are hidden when the scene starts
        if (buttonContainer != null)
        {
            buttonContainer.SetActive(false);
        }
        
        // Start the timer to show the buttons
        Invoke("ShowButtons", timeUntilButtonsAppear);
    }

    void Update()
    {
        // Move the text UP every frame
        if (creditsTextRect != null)
        {
            creditsTextRect.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
        }

        // Minecraft/Arcade feature: Press 'Space' or 'X' to skip the scrolling!
        if (!buttonsActive && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.X)))
        {
            CancelInvoke("ShowButtons"); // Stop the timer
            ShowButtons();
            creditsTextRect.gameObject.SetActive(false); // Instantly hide the text
        }
    }

    private void ShowButtons()
    {
        buttonsActive = true;
        if (buttonContainer != null)
        {
            buttonContainer.SetActive(true);
        }
    }
}