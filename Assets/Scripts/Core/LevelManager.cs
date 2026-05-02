using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Per-scene manager: tracks artifact collection and level completion.
/// Attach to a "LevelManager" GameObject in each level scene.
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Info")]
    public LevelData Data;

    [Header("Counts (auto-filled on Start)")]
    public int TotalArtifacts;

    [Header("Events")]
    public UnityEvent OnLevelComplete;
    public UnityEvent<int, int> OnArtifactChanged;

    public int Collected { get; private set; }

    void Awake()
    {
        Instance = this;

        TotalArtifacts = FindObjectsOfType<Artifact>().Length;

        int whiteTotal = 0;
        foreach (var f in FindObjectsOfType<Firefly>())
            if (f.Type == FireflyType.White) whiteTotal++;

        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        GameManager.Instance?.SetSceneFireflyTotal(scene, whiteTotal);
    }

    public void OnArtifactCollected(Artifact artifact)
    {
        Collected++;
        OnArtifactChanged?.Invoke(Collected, TotalArtifacts);

        if (Collected >= TotalArtifacts)
            CompleteLevel();
    }

    private void CompleteLevel()
    {
        if (Data != null)
            GameManager.Instance?.MarkLevelComplete(Data.LevelId);

        OnLevelComplete?.Invoke();
        Debug.Log("[LevelManager] Level complete!");
    }
}
