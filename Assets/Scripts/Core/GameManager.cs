using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Persistent singleton. Tracks all save-worthy game state.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    //Test Currency
     void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            GameManager.Instance?.AddCurrency(50);

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.V)) 
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("VictoryScreen");
        }
    }

    // ── Currency & Artifacts ──────────────────────────────────────────
    public int Currency                { get; private set; }
    public int TotalArtifactsCollected { get; private set; }

    // ── Firefly Journal (lifetime totals per type) ────────────────────
    private Dictionary<FireflyType, int> lifetimeFireflies = new Dictionary<FireflyType, int>();

    // ── Collected white firefly IDs per scene ─────────────────────────
    private Dictionary<string, HashSet<int>> collectedWhites = new Dictionary<string, HashSet<int>>();

    // ── Per-scene white firefly count (includes un-ID'd fireflies) ────
    private Dictionary<string, int> sceneWhiteCounts = new Dictionary<string, int>();

    // ── Collected artifact IDs per scene ──────────────────────────────
    private Dictionary<string, HashSet<int>> collectedArtifacts = new Dictionary<string, HashSet<int>>();

    // ── Opened doors (permanent) ──────────────────────────────────────
    private HashSet<string> openedDoors = new HashSet<string>();

    // ── Level completion ──────────────────────────────────────────────
    public Dictionary<int, bool> CompletedLevels { get; private set; } = new Dictionary<int, bool>();

    // ── Unlocked Abilities ────────────────────────────────────────────
    public HashSet<string> UnlockedAbilities { get; private set; } = new HashSet<string>();

    // ── Equipment ─────────────────────────────────────────────────────
    public HashSet<string> OwnedEquipment { get; private set; } = new HashSet<string>();

    // ── Biomes ────────────────────────────────────────────────────────
    private Dictionary<string, int> biomeUnlockRequirements = new Dictionary<string, int>
    {
        { "Cave",       0  },
        { "Snow",       10 },
        { "Rainforest", 20 },
        { "Volcano",    30 },
    };
    public HashSet<string> UnlockedBiomes { get; private set; } = new HashSet<string>();

    // ── Level white firefly requirements ──────────────────────────────
    public Dictionary<string, int> LevelWhiteRequirements = new Dictionary<string, int>
    {
        { "Level_Cave_01",       8  },
        { "Level_Cave_02",       5  },
        { "Level_Snow_01",       12 },
        { "Level_Rainforest_01", 15 },
        { "Level_Volcano_01",    18 },
    };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        UnlockedBiomes.Add("Cave");

        foreach (FireflyType t in System.Enum.GetValues(typeof(FireflyType)))
            lifetimeFireflies[t] = 0;
    }

    // ── Currency ──────────────────────────────────────────────────────

    public void AddCurrency(int amount) => Currency += amount;

    public bool SpendCurrency(int amount)
    {
        if (Currency < amount) return false;
        Currency -= amount;
        return true;
    }


    // ── Artifacts ─────────────────────────────────────────────────────

    public void CollectArtifact(string sceneName, int artifactId)
    {
        if (!collectedArtifacts.ContainsKey(sceneName))
            collectedArtifacts[sceneName] = new HashSet<int>();
        collectedArtifacts[sceneName].Add(artifactId);
        TotalArtifactsCollected++;
        CheckBiomeUnlocks();
        Debug.Log($"[GameManager] Artifact {artifactId} collected in {sceneName}. Total: {TotalArtifactsCollected}");
    }

    public bool IsArtifactCollected(string sceneName, int artifactId)
    {
        return collectedArtifacts.ContainsKey(sceneName) &&
               collectedArtifacts[sceneName].Contains(artifactId);
    }

    // ── White Fireflies ───────────────────────────────────────────────

    public void CollectWhiteFirefly(string sceneName, int fireflyId)
    {
        if (!collectedWhites.ContainsKey(sceneName))
            collectedWhites[sceneName] = new HashSet<int>();

        sceneWhiteCounts[sceneName] = (sceneWhiteCounts.ContainsKey(sceneName) ? sceneWhiteCounts[sceneName] : 0) + 1;
        lifetimeFireflies[FireflyType.White]++;

        if (fireflyId > 0)
            collectedWhites[sceneName].Add(fireflyId);

        Debug.Log($"[GameManager] White firefly {fireflyId} collected in {sceneName}. Scene total: {sceneWhiteCounts[sceneName]}");
    }

    public bool IsWhiteFireflyCollected(string sceneName, int fireflyId)
    {
        return collectedWhites.ContainsKey(sceneName) &&
               collectedWhites[sceneName].Contains(fireflyId);
    }

    public int GetWhiteCountForScene(string sceneName)
    {
        return sceneWhiteCounts.ContainsKey(sceneName) ? sceneWhiteCounts[sceneName] : 0;
    }

    public int GetWhiteRequirementForScene(string sceneName)
    {
        return LevelWhiteRequirements.ContainsKey(sceneName)
            ? LevelWhiteRequirements[sceneName] : 5;
    }

    // ── Colored Fireflies (permanent abilities) ───────────────────────

    public void CollectColoredFirefly(FireflyType type)
    {
        lifetimeFireflies[type]++;
        string ability = FireflyTypeToAbility(type);
        if (!string.IsNullOrEmpty(ability))
            UnlockAbility(ability);
        Debug.Log($"[GameManager] Colored firefly {type} collected. Ability: {ability}");
    }

    public bool HasColoredFirefly(FireflyType type) => lifetimeFireflies[type] > 0;

    private string FireflyTypeToAbility(FireflyType t) => t switch
    {
        FireflyType.Blue   => "Grapple",
        FireflyType.Red    => "Roll",
        FireflyType.Green  => "WallClimb",
        FireflyType.Yellow => "Run",
        FireflyType.Orange => "Crawl",
        FireflyType.Purple => "Lure",
        _                  => ""
    };

    // ── Journal ───────────────────────────────────────────────────────

    public int GetLifetimeCount(FireflyType t) =>
        lifetimeFireflies.ContainsKey(t) ? lifetimeFireflies[t] : 0;

    // ── Doors ─────────────────────────────────────────────────────────

    public void OpenDoor(string doorId) => openedDoors.Add(doorId);
    public bool IsDoorOpen(string doorId) => openedDoors.Contains(doorId);

    // ── Abilities ─────────────────────────────────────────────────────

    public static event System.Action<string> OnAbilityUnlocked;

    public void UnlockAbility(string abilityName)
    {
        if (UnlockedAbilities.Add(abilityName))
        {
            Debug.Log($"[GameManager] Ability unlocked: {abilityName}");
            OnAbilityUnlocked?.Invoke(abilityName);
        }
    }

    public bool HasAbility(string abilityName) => UnlockedAbilities.Contains(abilityName);

    // ── Equipment ─────────────────────────────────────────────────────

    public void BuyEquipment(string name) => OwnedEquipment.Add(name);
    public bool HasEquipment(string name) => OwnedEquipment.Contains(name);

    // ── Level Completion ──────────────────────────────────────────────

    public void MarkLevelComplete(int levelId) => CompletedLevels[levelId] = true;

    public bool IsLevelComplete(int levelId) =>
        CompletedLevels.TryGetValue(levelId, out bool done) && done;

    // ── Scene unlock (firefly exit condition met at least once) ──────
    private HashSet<string> unlockedScenes = new HashSet<string>();

    public void MarkSceneUnlocked(string sceneName)
    {
        if (unlockedScenes.Add(sceneName))
            Debug.Log($"[GameManager] Scene unlocked: {sceneName}");
    }

    public bool IsSceneUnlocked(string sceneName) => unlockedScenes.Contains(sceneName);

    // ── Biomes ────────────────────────────────────────────────────────

    private void CheckBiomeUnlocks()
    {
        foreach (var kv in biomeUnlockRequirements)
        {
            if (!UnlockedBiomes.Contains(kv.Key) && TotalArtifactsCollected >= kv.Value)
            {
                UnlockedBiomes.Add(kv.Key);
                Debug.Log($"[GameManager] Biome unlocked: {kv.Key}");
            }
        }
    }

    public bool IsBiomeUnlocked(string biomeName) => UnlockedBiomes.Contains(biomeName);

    // ── Bat stolen fireflies (persistent across scene loads) ──────────

    private Dictionary<string, HashSet<int>> batsWithFireflies = new Dictionary<string, HashSet<int>>();

    public void RecordBatSteal(string sceneName, int batId)
    {
        if (!batsWithFireflies.ContainsKey(sceneName))
            batsWithFireflies[sceneName] = new HashSet<int>();
        batsWithFireflies[sceneName].Add(batId);
    }

    public void ClearBatSteal(string sceneName, int batId)
    {
        if (batsWithFireflies.ContainsKey(sceneName))
            batsWithFireflies[sceneName].Remove(batId);
    }

    public bool BatHasFirefly(string sceneName, int batId)
    {
        return batsWithFireflies.ContainsKey(sceneName) &&
               batsWithFireflies[sceneName].Contains(batId);
    }

    public int GetBatStolenCount(string sceneName)
    {
        return batsWithFireflies.ContainsKey(sceneName) ? batsWithFireflies[sceneName].Count : 0;
    }

    // ── Per-scene firefly totals (set by LevelManager on scene load) ─────
    private Dictionary<string, int> sceneFireflyTotals = new Dictionary<string, int>();
    public string LastLevelScene { get; private set; } = "";

    public void SetSceneFireflyTotal(string sceneName, int total)
    {
        sceneFireflyTotals[sceneName] = total;
        LastLevelScene = sceneName;
    }

    public int GetSceneFireflyTotal(string sceneName)
    {
        return sceneFireflyTotals.ContainsKey(sceneName) ? sceneFireflyTotals[sceneName] : 0;
    }

    // ── Carried fireflies (persists across scene loads) ───────────────

    public int CarriedWhiteFireflies { get; private set; }

    public void SetCarriedWhiteFireflies(int count) => CarriedWhiteFireflies = Mathf.Max(0, count);
    public void IncrementCarried()                  => CarriedWhiteFireflies++;
    public void DecrementCarried()                  => CarriedWhiteFireflies = Mathf.Max(0, CarriedWhiteFireflies - 1);

    // ── Firefly (swarm visual tracking) ──────────────────────────────

    public void AddFirefly(FireflyType type, int count = 1)
    {
        Debug.Log($"[GameManager] {type} fireflies added: {count}");
    }

    public void SpendFireflies(FireflyType type, int count) { }
}