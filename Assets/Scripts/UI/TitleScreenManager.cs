using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class TitleScreenManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Exact name of the scene to load")]
    public string firstLevelName = "Lobby"; 

    [Header("UI Elements")]
    public TextMeshProUGUI promptText;
    public float flashSpeed = 0.5f;

    void Start()
    {
        // Start the flashing text effect
        if (promptText != null)
        {
            StartCoroutine(FlashText());
        }
    }

    void Update()
    {
        // Listen for the Enter key
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadScene(firstLevelName);
        }
    }

    // Classic arcade flashing effect
    private IEnumerator FlashText()
    {
        while (true)
        {
            promptText.enabled = !promptText.enabled;
            yield return new WaitForSeconds(flashSpeed);
        }
    }
}