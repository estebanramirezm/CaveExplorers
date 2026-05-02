using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Names")]
    public string LobbySceneName = "Lobby";

    [Header("UI References")]
    public GameObject OptionsPanel;

    void Start()
    {
        // Hide options panel on start
        OptionsPanel?.SetActive(false);
    }

    void Update()
    {
        
    }

    // ── Button callbacks ──────────────────────────────────────────────

    public void OnStartClicked()
    {
        SceneManager.LoadScene(LobbySceneName);
    }

    public void OnOptionsClicked()
    {
        OptionsPanel?.SetActive(true);
    }

    public void OnBackClicked()
    {
        OptionsPanel?.SetActive(false);
    }
}
