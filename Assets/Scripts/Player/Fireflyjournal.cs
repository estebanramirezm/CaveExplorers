using UnityEngine;
using TMPro;

/// <summary>
/// Press J anytime to open/close the firefly journal.
/// Shows lifetime firefly counts, abilities, currency, and artifacts.
/// Attach to a Canvas in the scene. Set up journal panel in Inspector.
/// </summary>
public class FireflyJournal : MonoBehaviour
{
    [Header("Input")]
    public KeyCode JournalKey = KeyCode.J;

    [Header("Panel")]
    public GameObject JournalPanel;
    public GameObject HudRoot;

    [Header("Firefly Counts")]
    public TextMeshProUGUI WhiteCountText;

    [Header("Abilities")]
    public TextMeshProUGUI GrappleText;
    public TextMeshProUGUI RollText;
    public TextMeshProUGUI WallClimbText;
    public TextMeshProUGUI RunText;
    public TextMeshProUGUI CrawlText;
    public TextMeshProUGUI LureText;

    [Header("Stats")]
    public TextMeshProUGUI CurrencyText;
    public TextMeshProUGUI ArtifactsText;

    [Header("Current Level")]
    public TextMeshProUGUI LevelProgressText;

    private bool isOpen;

    void Start()
    {
        JournalPanel?.SetActive(false);
    }

    void Update()
    {
         if (Input.GetKeyDown(JournalKey))
    {
        isOpen = !isOpen;
        JournalPanel?.SetActive(isOpen);
        HudRoot?.SetActive(!isOpen);
        Time.timeScale = isOpen ? 0f : 1f;
        if (isOpen) Refresh();
    }

    // Keep refreshing while open
    if (isOpen) Refresh();
    }

   private void Refresh()
{
    var gm = GameManager.Instance;
    if (gm == null) return;

    // Live swarm count (decreases when bat steals)
    var swarm = GameObject.FindGameObjectWithTag("Player")?.GetComponent<FireflySwarm>();
    int currentWhites = swarm != null ? swarm.GetCount(FireflyType.White) : 0;
    int lifetimeWhites = gm.GetLifetimeCount(FireflyType.White);

    SetText(WhiteCountText,  $"White fireflies — Carrying: {currentWhites}  |  Total found: {lifetimeWhites}");

    // Abilities
    SetText(GrappleText,   gm.HasAbility("Grapple")
        ? "Grapple: UNLOCKED\n  G — throw hook\n  W — reel in"
        : "Grapple: locked");

    SetText(RollText,      gm.HasAbility("Roll")
        ? "Roll: UNLOCKED\n  C — roll/dash"
        : "Roll: locked");

    SetText(WallClimbText, gm.HasAbility("WallClimb")
        ? "Wall Climb: UNLOCKED\n  Up Arrow — climb wall"
        : "Wall Climb: locked");

    SetText(RunText,       gm.HasAbility("Run")
        ? "Run: UNLOCKED\n  Hold Shift — run"
        : "Run: locked");

    SetText(CrawlText,     gm.HasAbility("Crawl")
        ? "Crawl: UNLOCKED\n  Down Arrow — crawl"
        : "Crawl: locked");

    SetText(LureText,      gm.HasAbility("Lure")
        ? "Lure: UNLOCKED\n  L — drop lure (distracts bats)"
        : "Lure: locked\n  Find the purple firefly");

    // Stats
    SetText(CurrencyText,  $"Currency:  {gm.Currency}g");
    SetText(ArtifactsText, $"Artifacts: {gm.TotalArtifactsCollected}");

    var levelExit = FindObjectOfType<LevelExit>();
    if (levelExit != null)
    {
        int required = gm.GetWhiteRequirementForScene(levelExit.NextSceneName);
        SetText(LevelProgressText, $"Fireflies needed to exit: {required}");
    }
    else
    {
        SetText(LevelProgressText, "");
    }
}

    private void SetText(TextMeshProUGUI tmp, string text)
    {
        if (tmp) tmp.text = text;
    }

    public void OnCloseClicked()
    {
        isOpen = false;
        JournalPanel?.SetActive(false);
        HudRoot?.SetActive(true);
        Time.timeScale = 1f;
    }
}