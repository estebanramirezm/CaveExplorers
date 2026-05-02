using UnityEngine;

/// <summary>
/// Place these in your level wherever the player can grapple.
/// Add a SpriteRenderer (circle sprite) so they're visible.
/// GrappleHook finds these automatically by type — no wiring needed.
/// </summary>
public class GrapplePoint : MonoBehaviour
{
    [Header("Visuals")]
    public Color IdleColor   = new Color(1f, 1f, 1f, 0.4f);   // dim when inactive
    public Color ActiveColor = new Color(0f, 1f, 1f, 1f);     // bright cyan when hooked

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr) sr.color = IdleColor;
    }

    // Called by GrappleHook when attached/released
    public void SetActive(bool active)
    {
        if (sr) sr.color = active ? ActiveColor : IdleColor;
    }

    // Draw range indicator in Scene view
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}