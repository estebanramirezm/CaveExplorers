using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FireflySwarm : MonoBehaviour
{
    [Header("Light")]
    public float LightPerWhiteFirefly = 0.8f;
    public float BaseRadius = 1f;
    public Light2D PlayerLight;
    public Light2D AmbientLight;

    [Header("Lure Ability")]
    public KeyCode LureKey = KeyCode.L;
    public GameObject LurePrefab;
    public float LureDuration = 7f;

    [Header("Visual Firefly Prefab")]
    public GameObject FireflyPrefab;

    [Header("UI")]
    public HeartsUI HeartsUI;

    private Dictionary<FireflyType, List<Firefly>> collected = new Dictionary<FireflyType, List<Firefly>>();
    private List<Firefly> allFireflies = new List<Firefly>();

    private Animator animator;
    private FireflyLure activeLure;
    private Firefly hiddenPurpleFirefly;
    private Coroutine lureRoutine;
    private bool isLuring;

    public bool IsLuring => isLuring;

    public int GetCount(FireflyType t) => collected.ContainsKey(t) ? collected[t].Count : 0;
    public int WhiteCount => GetCount(FireflyType.White);
    public int PurpleCount => GetCount(FireflyType.Purple);
    public int FireflyCount => allFireflies.Count;
    public float LightRadius => BaseRadius + WhiteCount * LightPerWhiteFirefly;

    void Awake()
    {
        foreach (FireflyType t in System.Enum.GetValues(typeof(FireflyType)))
            collected[t] = new List<Firefly>();

        animator = GetComponent<Animator>();

        if (AmbientLight)
            AmbientLight.enabled = true;
    }

    void Start()
    {
        if (GameManager.Instance == null) return;

        int carried = GameManager.Instance.CarriedWhiteFireflies;

        if (carried > 0)
            RespawnVisuals(carried);
    }

    void Update()
    {
        if (PlayerLight)
            PlayerLight.pointLightOuterRadius = LightRadius;

        if (Input.GetKeyDown(LureKey) &&
            GameManager.Instance != null &&
            GameManager.Instance.HasAbility("Lure"))
        {
            if (!isLuring)
            {
                if (!IsGrounded())
                {
                    Debug.Log("[FireflySwarm] Must be grounded to use Lure.");
                    return;
                }

                StartLure();
            }
            else
            {
                EndLure();
            }
        }

        if (isLuring && !IsGrounded())
        {
            EndLure();
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.P))
        {
            for (int i = 0; i < 5; i++)
                AddDebugWhiteFirefly();

            Debug.Log("[DEBUG] Added 5 white fireflies");
        }
#endif
    }

    // ── Lure ───────────────────────────────────────

    private void StartLure()
    {
        if (PurpleCount <= 0)
        {
            Debug.Log("[FireflySwarm] Need a purple firefly to use Lure.");
            return;
        }

        if (LurePrefab == null)
        {
            Debug.LogWarning("[FireflySwarm] LurePrefab not assigned!");
            return;
        }

        if (!HidePurpleFireflyForLure())
        {
            Debug.Log("[FireflySwarm] Could not hide purple firefly.");
            return;
        }

        GameObject go = Instantiate(LurePrefab, transform.position, Quaternion.identity);
        activeLure = go.GetComponentInChildren<FireflyLure>();

        if (activeLure == null)
        {
            Debug.LogWarning("[FireflySwarm] LurePrefab has no FireflyLure component!");
            Destroy(go);
            RestorePurpleFireflyAfterLure();
            return;
        }

        isLuring = true;

        animator.SetBool("IsLuring", true);
        animator.SetTrigger("StartLure");

        float facing = Mathf.Sign(transform.localScale.x);
        activeLure.Launch(new Vector2(facing, 0f));

        if (lureRoutine != null)
            StopCoroutine(lureRoutine);

        lureRoutine = StartCoroutine(LureTimerRoutine());

        Debug.Log("[FireflySwarm] Purple lure started.");
    }

    private IEnumerator LureTimerRoutine()
    {
        yield return new WaitForSeconds(LureDuration);
        EndLure();
    }

    private void EndLure()
    {
        if (!isLuring) return;

        isLuring = false;

        animator.SetBool("IsLuring", false);
        animator.SetTrigger("EndLure");

        if (activeLure != null)
            Destroy(activeLure.gameObject);

        activeLure = null;

        RestorePurpleFireflyAfterLure();

        if (lureRoutine != null)
        {
            StopCoroutine(lureRoutine);
            lureRoutine = null;
        }

        Debug.Log("[FireflySwarm] Purple lure ended.");
    }

    private bool HidePurpleFireflyForLure()
    {
        if (!collected.ContainsKey(FireflyType.Purple) || collected[FireflyType.Purple].Count == 0)
            return false;

        hiddenPurpleFirefly = collected[FireflyType.Purple][collected[FireflyType.Purple].Count - 1];

        if (hiddenPurpleFirefly != null)
            hiddenPurpleFirefly.gameObject.SetActive(false);

        return true;
    }

    private void RestorePurpleFireflyAfterLure()
    {
        if (hiddenPurpleFirefly != null)
        {
            hiddenPurpleFirefly.gameObject.SetActive(true);
            hiddenPurpleFirefly.SetOrbiting(true);
        }

        hiddenPurpleFirefly = null;
    }

    private bool IsGrounded()
    {
        PlayerController pc = GetComponent<PlayerController>();

        if (pc == null || pc.GroundCheck == null)
            return true;

        return Physics2D.OverlapCircle(
            pc.GroundCheck.position,
            pc.GroundCheckRadius,
            pc.GroundLayer
        );
    }

#if UNITY_EDITOR
    private void AddDebugWhiteFirefly()
    {
        collected[FireflyType.White].Add(null);
        allFireflies.Add(null);
        RefreshUI();
    }
#endif

    public void AddFirefly(Firefly f)
    {
        collected[f.Type].Add(f);
        allFireflies.Add(f);
        f.gameObject.SetActive(false);

        Debug.Log($"[FireflySwarm] Collected {f.Type}. Total: {allFireflies.Count}, Light radius: {LightRadius}");

        switch (f.Type)
        {
            case FireflyType.Blue:
            case FireflyType.Red:
            case FireflyType.Green:
                NotifySecretDoors(f.Type);
                break;
        }

        if (f.Type == FireflyType.White)
            GameManager.Instance?.IncrementCarried();

        GameManager.Instance?.AddFirefly(f.Type, 1);
        RefreshUI();
    }

    // ── Scout / White Fireflies ─────────────────────

    public bool CanSendScout() => GetCount(FireflyType.White) > 0;

    public void RemoveFireflyVisualOnly(FireflyType type)
    {
        if (type != FireflyType.White)
        {
            Debug.LogWarning("[FireflySwarm] Only white fireflies can be sent as scouts!");
            return;
        }

        if (!collected.ContainsKey(type) || collected[type].Count == 0) return;

        var f = collected[type][collected[type].Count - 1];

        collected[type].RemoveAt(collected[type].Count - 1);
        allFireflies.Remove(f);

        if (f)
            Destroy(f.gameObject);

        RefreshUI();
    }

    public void AddFireflyVisualOnly(FireflyType type)
    {
        SpawnOrbitFirefly(FireflyType.White);

        if (type == FireflyType.White)
            GameManager.Instance?.IncrementCarried();

        Debug.Log("[FireflySwarm] Scout returned.");
        RefreshUI();
    }

    // ── Bat stealing ─────────────────────────────────

    public void RemoveFirefly(FireflyType type)
    {
        if (!collected.ContainsKey(type) || collected[type].Count == 0) return;

        var f = collected[type][collected[type].Count - 1];

        collected[type].RemoveAt(collected[type].Count - 1);
        allFireflies.Remove(f);

        if (f)
            Destroy(f.gameObject);

        if (type == FireflyType.White)
            GameManager.Instance?.DecrementCarried();

        Debug.Log($"[FireflySwarm] Lost {type} firefly to bat!");
        RefreshUI();
    }

    // ── Respawn ──────────────────────────────────────

    public void RespawnVisuals(int whiteCount)
    {
        ClearAll();

        for (int i = 0; i < whiteCount; i++)
            SpawnOrbitFirefly(FireflyType.White);

        Debug.Log($"[FireflySwarm] Respawned {whiteCount} white firefly visuals.");
        RefreshUI();
    }

    private void SpawnOrbitFirefly(FireflyType type)
    {
        if (FireflyPrefab == null) return;

        var go = Instantiate(FireflyPrefab, transform.position, Quaternion.identity);

        var existing = go.GetComponent<Firefly>();

        if (existing != null)
            Destroy(existing);

        var col = go.GetComponent<Collider2D>();

        if (col)
            col.enabled = false;

        var f = go.AddComponent<Firefly>();
        f.Type = type;
        f.SetOrbiting(true);

        go.SetActive(false);

        collected[type].Add(f);
        allFireflies.Add(f);
    }

    private void NotifySecretDoors(FireflyType type)
    {
        foreach (var door in FindObjectsOfType<SecretDoor>())
        {
            if (door.RequiredColor == type)
                door.Unlock();
        }
    }

    public Dictionary<FireflyType, int> GetAllCounts()
    {
        var counts = new Dictionary<FireflyType, int>();

        foreach (var kvp in collected)
            counts[kvp.Key] = kvp.Value.Count;

        return counts;
    }

    public void ClearAll()
    {
        foreach (var f in allFireflies)
            if (f) Destroy(f.gameObject);

        allFireflies.Clear();

        foreach (var key in new List<FireflyType>(collected.Keys))
            collected[key].Clear();

        if (PlayerLight)
            PlayerLight.pointLightOuterRadius = BaseRadius;

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (HeartsUI == null)
            HeartsUI = FindObjectOfType<HeartsUI>();

        if (HeartsUI != null)
            HeartsUI.UpdateFireflyCount(WhiteCount);
    }
}