using System.Collections;
using UnityEngine;

/// <summary>
/// Handles player sprite color changes.
/// - Base color: set freely in Inspector
/// - Ability flash: brief color tint when an ability activates
/// - Damage flash: red flash when hit
/// 
/// Attach to Player alongside PlayerController.
/// </summary>
public class PlayerVisuals : MonoBehaviour
{
    [Header("Base Color")]
    public Color BaseColor = Color.white;

    [Header("Ability Flash Colors")]
    public Color RunColor    = new Color(1f, 0.8f, 0f);     // gold
    public Color RollColor   = new Color(0f, 0.8f, 1f);     // cyan
    public Color CrawlColor  = new Color(0.6f, 0.4f, 1f);   // purple
    public Color ClimbColor  = new Color(0.4f, 1f, 0.4f);   // green
    public Color GrappleColor= new Color(1f, 0.5f, 0f);     // orange

    [Header("Damage Flash")]
    public Color DamageColor = Color.red;
    public float DamageFlashDuration = 0.15f;
    public int   DamageFlashCount    = 3;

    [Header("Flash Settings")]
    public float AbilityFlashDuration = 0.12f;

    private SpriteRenderer sr;
    private Coroutine activeFlash;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = BaseColor;
    }

    // ── Called by AbilitySystem ───────────────────────────────────────

    public void FlashAbility(string abilityName)
    {
        Color c = abilityName switch
        {
            "Run"       => RunColor,
            "Roll"      => RollColor,
            "Crawl"     => CrawlColor,
            "WallClimb" => ClimbColor,
            "Grapple"   => GrappleColor,
            _           => BaseColor
        };
        StartFlash(c, AbilityFlashDuration);
    }

    // ── Called by HealthManager / PlayerController on damage ──────────

    public void FlashDamage()
    {
        if (activeFlash != null) StopCoroutine(activeFlash);
        activeFlash = StartCoroutine(DamageFlash());
    }

    // ── Coroutines ────────────────────────────────────────────────────

    private void StartFlash(Color flashColor, float duration)
    {
        if (activeFlash != null) StopCoroutine(activeFlash);
        activeFlash = StartCoroutine(Flash(flashColor, duration));
    }

    private IEnumerator Flash(Color flashColor, float duration)
    {
        sr.color = flashColor;
        yield return new WaitForSeconds(duration);
        sr.color = BaseColor;
        activeFlash = null;
    }

    private IEnumerator DamageFlash()
    {
        for (int i = 0; i < DamageFlashCount; i++)
        {
            sr.color = DamageColor;
            yield return new WaitForSeconds(DamageFlashDuration);
            sr.color = BaseColor;
            yield return new WaitForSeconds(DamageFlashDuration);
        }
        activeFlash = null;
    }
}