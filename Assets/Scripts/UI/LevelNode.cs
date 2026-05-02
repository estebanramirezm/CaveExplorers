using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// A clickable level node on the world map.
/// Attach to a UI Button positioned on the map image.
/// </summary>
[RequireComponent(typeof(Button))]
public class LevelNode : MonoBehaviour
{
    [Header("Level")]
    public LevelData Data;

    [Header("UI References")]
    public Image NodeImage;
    public TextMeshProUGUI NameText;
    public GameObject LockIcon;
    public GameObject CompletedIcon;

    [Header("Colors")]
    public Color UnlockedColor  = new Color(1f, 0.2f, 0.2f);
    public Color LockedColor    = new Color(0.4f, 0.4f, 0.4f);
    public Color CompletedColor = new Color(0.2f, 0.8f, 0.2f);

    [Header("Pulse")]
    public bool PulseWhenUnlocked = true;
    public float PulseSpeed  = 2f;
    public float PulseAmount = 0.08f;

    private Button btn;
    private Vector3 baseScale;
    private bool isUnlocked;

    void Awake()
    {
        btn       = GetComponent<Button>();
        baseScale = transform.localScale;
        btn.onClick.AddListener(OnClicked);
    }

    void Update()
    {
        if (PulseWhenUnlocked && isUnlocked)
        {
            float s = 1f + Mathf.Sin(Time.unscaledTime * PulseSpeed) * PulseAmount;
            transform.localScale = baseScale * s;
        }
    }

    public void Refresh()
    {
        if (Data == null) { Debug.LogWarning($"[LevelNode] {name} has no LevelData assigned!"); return; }

        bool completed = GameManager.Instance.IsLevelComplete(Data.LevelId);
        isUnlocked     = CheckUnlocked();

        if (NodeImage)
            NodeImage.color = completed  ? CompletedColor :
                              isUnlocked ? UnlockedColor  : LockedColor;

        if (NameText)      NameText.text = Data.LevelName;
        if (LockIcon)      LockIcon.SetActive(!isUnlocked);
        if (CompletedIcon) CompletedIcon.SetActive(completed);

        btn.interactable = isUnlocked;
    }

    private bool CheckUnlocked()
    {
        var gm = GameManager.Instance;
        if (gm == null) return false;
        if (!gm.IsBiomeUnlocked(Data.Biome)) return false;
        if (gm.TotalArtifactsCollected < Data.RequiredArtifacts) return false;
        if (Data.PrerequisiteLevelId >= 0 && !gm.IsSceneUnlocked(Data.SceneName)) return false;
        return true;
    }

    private void OnClicked()
    {
        WorldMap.Instance?.SelectLevel(Data);
    }
}