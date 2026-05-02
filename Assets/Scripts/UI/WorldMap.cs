using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Manages the world map screen.
/// Attach to WorldMapCanvas.
/// </summary>
public class WorldMap : MonoBehaviour
{
    public static WorldMap Instance { get; private set; }

    [Header("UI")]
    public GameObject MapPanel;
    public TextMeshProUGUI MessageText;
    public float MessageDuration = 2f;

    [Header("Scene")]
    public string LobbySceneName = "Lobby";

    public bool IsOpen { get; private set; }

    void Awake()
    {
        Instance = this;
        MapPanel.SetActive(false);
    }

    public void OpenMap()
    {
        IsOpen = true;
        MapPanel.SetActive(true);
        Time.timeScale = 0f;
        foreach (var node in FindObjectsOfType<LevelNode>())
            node.Refresh();
    }

    public void CloseMap()
    {
        IsOpen = false;
        MapPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OnBackClicked() => CloseMap();

    public void SelectLevel(LevelData data)
    {
        if (data == null) return;

        if (!GameManager.Instance.IsBiomeUnlocked(data.Biome))
        { ShowMessage($"{data.Biome} biome locked!"); return; }

        if (GameManager.Instance.TotalArtifactsCollected < data.RequiredArtifacts)
        { ShowMessage($"Need {data.RequiredArtifacts} artifacts!"); return; }

        if (data.PrerequisiteLevelId >= 0 &&
            !GameManager.Instance.IsSceneUnlocked(data.SceneName))
        { ShowMessage("Complete the previous level first!"); return; }

        Time.timeScale = 1f;
        SceneManager.LoadScene(data.SceneName);
    }

    public void ShowMessage(string msg)
    {
        if (MessageText == null) return;
        MessageText.text = msg;
        CancelInvoke(nameof(ClearMessage));
        Invoke(nameof(ClearMessage), MessageDuration);
    }

    private void ClearMessage()
    {
        if (MessageText) MessageText.text = "";
    }
}