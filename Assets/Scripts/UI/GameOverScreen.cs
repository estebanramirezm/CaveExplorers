using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Game Over screen. Stays visible until player clicks Retry.
/// Retry teleports player to last checkpoint — no scene reload.
/// Wire HealthManager.OnPlayerDeath → GameOverScreen.ShowGameOver
/// </summary>
public class GameOverScreen : MonoBehaviour
{
    [Header("References")]
    public GameObject Panel;

    [Header("Scene Names")]
    public string LobbySceneName = "Lobby";

    void Awake()
    {
        Panel?.SetActive(false);
    }

    // ── Called by HealthManager.OnPlayerDeath ─────────────────────────

    public void ShowGameOver()
    {
        Panel?.SetActive(true);
    }

    // ── Button callbacks ──────────────────────────────────────────────

    public void OnRetryClicked()
    {
        Panel?.SetActive(false);
        HealthManager.Instance?.Respawn();
    }

    public void OnLobbyClicked()
    {
        Time.timeScale = 1f;
        Checkpoint.Reset();
        SceneManager.LoadScene(LobbySceneName);
    }
}