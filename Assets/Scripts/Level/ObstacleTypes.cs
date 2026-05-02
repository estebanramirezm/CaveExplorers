using UnityEngine;

// ── Spikes (Cave biome) ───────────────────────────────────────────────────────

/// <summary>Static spike tiles. Player takes damage on contact.</summary>
public class Spikes : Obstacle
{
    // No extra logic needed beyond the base class for simple spikes.
    // BypassAbility could be "Roll" if rolling over spikes should be safe.
}

// ── Falling Rock (Cave biome) ────────────────────────────────────────────────

/// <summary>
/// Triggered rock that falls when the player walks underneath.
/// Attach to a rock GameObject with a Rigidbody2D (initially kinematic).
/// Add a separate "trigger zone" child collider above the rock.
/// </summary>
public class FallingRock : Obstacle
{
    [Header("Falling Rock")]
    public float TriggerDelay = 0.3f;
    public float ResetTime    = 5f;    // 0 = never resets

    private Rigidbody2D rb;
    private Vector3 startPos;
    private bool triggered;

    void Start()
    {
        rb       = GetComponent<Rigidbody2D>();
        startPos = transform.position;
    }

    // The rock uses a separate child trigger to detect the player underneath.
    // Wire that child's OnTriggerEnter2D → this method via a small relay component,
    // or override here if this is the trigger itself.
    public void TriggerFall()
    {
        if (triggered) return;
        triggered = true;
        Invoke(nameof(Fall), TriggerDelay);
        if (ResetTime > 0) Invoke(nameof(ResetRock), ResetTime);
    }

    private void Fall()
    {
        if (rb) rb.isKinematic = false;
    }

    private void ResetRock()
    {
        triggered            = false;
        transform.position   = startPos;
        if (rb) rb.isKinematic = true;
        if (rb) rb.linearVelocity   = Vector2.zero;
    }
}

// ── Ice Patch (Snow biome) ────────────────────────────────────────────────────

/// <summary>
/// Slippery surface: lowers friction while player stands on it.
/// Attach to the ice tile GameObject with a PhysicsMaterial2D for full effect,
/// or use this script to manually adjust velocity.
/// </summary>
public class IcePatch : MonoBehaviour
{
    [Header("Ice Settings")]
    public float SlipFactor = 0.95f;    // Velocity retained each FixedUpdate

    void OnCollisionStay2D(Collision2D col)
    {
        if (!col.gameObject.CompareTag("Player")) return;
        var rb = col.rigidbody;
        if (rb != null)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * SlipFactor, rb.linearVelocity.y);
    }
}

// ── Lava (Volcano biome) ──────────────────────────────────────────────────────

/// <summary>Instant-death lava pit.</summary>
public class Lava : Obstacle
{
    void Awake() => DamageAmount = 3; // kills in one hit
}
