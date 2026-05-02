using UnityEngine;

/// <summary>
/// Place checkpoint objects throughout the level.
/// Player activates by walking through.
/// Stores the last activated checkpoint for respawn.
/// First checkpoint should be placed at level start.
/// </summary>
public class Checkpoint : MonoBehaviour
{
    public static Checkpoint Current { get; private set; }

    [Header("Visuals")]
    public SpriteRenderer SR;
    public Color InactiveColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    public Color ActiveColor   = new Color(0.2f, 1f, 0.8f, 1f);

    void Start()
    {
        if (SR) SR.color = InactiveColor;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Current == this) return;

        // Deactivate old checkpoint visually
        if (Current != null && Current.SR)
            Current.SR.color = InactiveColor;

        Current = this;
        if (SR) SR.color = ActiveColor;

        Debug.Log($"[Checkpoint] Activated: {gameObject.name}");
    }

    public static Vector3 GetRespawnPosition(Vector3 levelStart)
    {
        return Current != null ? Current.transform.position : levelStart;
    }

    // Call when scene loads to clear checkpoint
    public static void Reset() => Current = null;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}