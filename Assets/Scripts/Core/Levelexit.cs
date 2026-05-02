using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelExit : MonoBehaviour
{
    [Header("Settings")]
    public string NextSceneName;
    public string LobbySceneName = "Lobby";
    public string DisplayName;   // e.g. "Cave - Level 1"

    [Header("References")]
    public GameObject LockedVisual;
    public GameObject UnlockedVisual;
    public GameObject ExitUI;
    public Button ContinueButton;
    public TextMeshProUGUI ProgressText;
    public TextMeshProUGUI RequirementText;

    private string sceneName;
    private bool unlocked;
    private bool playerInside;

    void Start()
    {
        sceneName = SceneManager.GetActiveScene().name;
        if (ExitUI != null) ExitUI.SetActive(false);
        Refresh();
    }

    void Update()
    {
        Refresh();

        if (playerInside && Input.GetKeyDown(KeyCode.E))
            ShowExitUI();
    }

    private void Refresh()
    {
        if (GameManager.Instance == null) return;

        int collected    = GameManager.Instance.GetWhiteCountForScene(sceneName);
        int stolenByBats = GameManager.Instance.GetBatStolenCount(sceneName);
        int effective    = Mathf.Max(0, collected - stolenByBats);
        int nextRequired = GameManager.Instance.GetWhiteRequirementForScene(NextSceneName);
        unlocked = effective >= nextRequired;
        if (unlocked && !string.IsNullOrEmpty(NextSceneName))
        {
            GameManager.Instance?.MarkSceneUnlocked(NextSceneName);
            Debug.Log($"[LevelExit] Exit unlocked → next scene '{NextSceneName}' marked accessible.");
        }

        if (ProgressText)
            ProgressText.text = string.IsNullOrEmpty(DisplayName) ? sceneName : DisplayName;

        if (RequirementText)
            RequirementText.text = $"Next level requires: {nextRequired} fireflies";

        if (LockedVisual)   LockedVisual.SetActive(!unlocked);
        if (UnlockedVisual) UnlockedVisual.SetActive(unlocked);

        if (ContinueButton) ContinueButton.interactable = unlocked;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;
        ShowExitUI();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
        HideExitUI();
    }

    private void ShowExitUI()
    {
        if (ExitUI != null) ExitUI.SetActive(true);
        Time.timeScale = 0f;
    }

    private void HideExitUI()
    {
        if (ExitUI != null) ExitUI.SetActive(false);
        Time.timeScale = 1f;
    }

    // ── Button callbacks ──────────────────────────────────────────────

    public void OnContinueClicked()
    {
        if (!unlocked) return;
        HideExitUI();
        if (!string.IsNullOrEmpty(NextSceneName))
            SceneManager.LoadScene(NextSceneName);
        else
            Debug.LogWarning("[LevelExit] No next scene assigned!");
    }

    public void OnLobbyClicked()
    {
        HideExitUI();
        SceneManager.LoadScene(LobbySceneName);
    }
}