using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Attach to a ground/platform object to diagnose why its sprite disappears.
/// Check the Console after the bug occurs.
/// </summary>
public class PlatformDebug : MonoBehaviour
{
    private SpriteRenderer sr;
    private bool wasEnabled;
    private bool wasActive;
    private Color lastColor;
    private float lastAlpha;
    private bool wasInView;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError($"[PlatformDebug] No SpriteRenderer on {gameObject.name}!");
            return;
        }

        wasEnabled = sr.enabled;
        wasActive  = gameObject.activeInHierarchy;
        lastColor  = sr.color;
        lastAlpha  = sr.color.a;

        Debug.Log($"[PlatformDebug] START — {gameObject.name} | enabled={sr.enabled} | active={wasActive} | alpha={lastAlpha:F2} | material={sr.material?.name} | sortLayer={sr.sortingLayerName} | sortOrder={sr.sortingOrder}");

        // Log all lights in the scene
        var lights = FindObjectsOfType<Light2D>();
        foreach (var l in lights)
            Debug.Log($"[PlatformDebug] Light2D: {l.gameObject.name} | type={l.lightType} | enabled={l.enabled} | intensity={l.intensity} | radius={l.pointLightOuterRadius}");
    }

    void Update()
    {
        if (sr == null) return;

        bool  nowEnabled = sr.enabled;
        bool  nowActive  = gameObject.activeInHierarchy;
        float nowAlpha   = sr.color.a;

        if (nowEnabled != wasEnabled)
        {
            Debug.LogWarning($"[PlatformDebug] {gameObject.name} SpriteRenderer.enabled changed: {wasEnabled} → {nowEnabled}", gameObject);
            wasEnabled = nowEnabled;
        }

        if (nowActive != wasActive)
        {
            Debug.LogWarning($"[PlatformDebug] {gameObject.name} activeInHierarchy changed: {wasActive} → {nowActive}", gameObject);
            wasActive = nowActive;
        }

        if (Mathf.Abs(nowAlpha - lastAlpha) > 0.01f)
        {
            Debug.LogWarning($"[PlatformDebug] {gameObject.name} alpha changed: {lastAlpha:F2} → {nowAlpha:F2}", gameObject);
            lastAlpha = nowAlpha;
        }

        var cam = Camera.main;
        if (cam != null)
        {
            Vector3 vp = cam.WorldToViewportPoint(transform.position);
            bool inView = vp.z > 0 && vp.x >= 0 && vp.x <= 1 && vp.y >= 0 && vp.y <= 1;
            if (inView != wasInView)
            {
                Debug.Log($"[PlatformDebug] {gameObject.name} camera visibility: {wasInView} → {inView} | world={transform.position} | viewport={vp}", gameObject);
                wasInView = inView;
            }
        }
    }

    void OnDestroy()
    {
        var trace = new System.Diagnostics.StackTrace(true);
        Debug.LogError($"[PlatformDebug] {gameObject.name} was DESTROYED!\n{trace}", gameObject);
    }
}
