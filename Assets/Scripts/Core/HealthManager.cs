using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance { get; private set; }

    [Header("Settings")]
    public int MaxHearts = 3;

    [Header("Invincibility")]
    public float InvincibilityDuration = 1.5f;
    private bool isInvincible;

    [Header("Knockback")]
    public float KnockbackForce   = 6f;
    public float KnockbackUpForce = 4f;

    [Header("UI")]
    public HeartsUI HeartsUI;

    public int CurrentHearts { get; private set; }

    public UnityEvent<int> OnHeartsChanged;

    private Rigidbody2D rb;

    void Awake()
    {
        Instance = this;
        rb       = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        ResetHearts();

    }

    public void TakeDamage(int amount = 1) => TakeDamageFrom(amount, null);

    public void TakeDamageFrom(int amount, Transform source)
    {
        if (isInvincible) return;

        CurrentHearts = Mathf.Max(0, CurrentHearts - amount);
        UpdateUI();

        if (CurrentHearts <= 0)
            Respawn();
        else
        {
            ApplyKnockback(source);
            StartCoroutine(InvincibilityFrames());
        }
    }

    // ── Respawn at checkpoint ─────────────────────────────────────────

    public void Respawn()
    {
        Vector3 respawnPos = Checkpoint.GetRespawnPosition(transform.position);
        respawnPos.z = 0f;

        // Move player
        transform.position = respawnPos;
        if (rb) rb.linearVelocity = Vector2.zero;

        // Snap camera so it doesn't pan across the level revealing missing platforms
        var cam = FindObjectOfType<CameraFollow>();
        if (cam != null)
        {
            Vector3 snap = respawnPos + cam.Offset;
            if (cam.LockY) snap.y = cam.LockedY;
            cam.transform.position = snap;
        }

        // Reset hearts
        ResetHearts();

        // Brief invincibility so player doesn't immediately take damage
        StartCoroutine(InvincibilityFrames());

        Debug.Log($"[HealthManager] Respawned at {respawnPos}");
    }

    // ── Helpers ───────────────────────────────────────────────────────

    private void UpdateUI()
    {
        HeartsUI?.UpdateHearts(CurrentHearts);
        OnHeartsChanged?.Invoke(CurrentHearts);
    }

    private void ApplyKnockback(Transform source)
    {
        if (rb == null || source == null) return;
        Vector2 dir = (transform.position - source.position).normalized;
        dir.y = 0;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(dir.x * KnockbackForce, KnockbackUpForce), ForceMode2D.Impulse);
    }

    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        yield return new WaitForSeconds(InvincibilityDuration);
        isInvincible = false;
    }

    public void Heal(int amount = 1)
    {
        CurrentHearts = Mathf.Min(MaxHearts, CurrentHearts + amount);
        UpdateUI();
    }

    public void ResetHearts()
    {
        CurrentHearts = MaxHearts;
        UpdateUI();
    }

    public void SetSafePosition(Vector3 pos) { } // kept for compatibility
}