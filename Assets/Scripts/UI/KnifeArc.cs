using System.Collections;
using UnityEngine;

public class KnifeArc : MonoBehaviour
{
    [Header("Arc Settings")]
    public float Radius     = 1.5f;
    public float ArcDegrees = 200f;
    public float Duration   = 0.25f;
    public int   Segments   = 30;
    public Color ArcColor   = new Color(1f, 0.95f, 0.5f, 1f);
    public float LineWidth  = 0.1f;

    private LineRenderer lr;
    private bool slashing = false;

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.K) || slashing) return;
        if (GameManager.Instance == null || !GameManager.Instance.HasEquipment("Knife")) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, Radius);
        Bat hitBat = null;
        foreach (var col in hits)
        {
            var bat = col.GetComponent<Bat>();
            if (bat != null) { hitBat = bat; break; }
        }

        StartCoroutine(SlashRoutine(hitBat));
    }

    void Awake()
    {
        // Create a dedicated child GameObject for the arc
        var arcObj = new GameObject("KnifeArcRenderer");
        arcObj.transform.SetParent(transform);
        arcObj.transform.localPosition = Vector3.zero;

        lr = arcObj.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.startWidth    = LineWidth;
        lr.endWidth      = LineWidth * 0.2f;
        lr.positionCount = Segments + 1;
        lr.sortingOrder  = 10;
        lr.startColor    = ArcColor;
        lr.endColor      = new Color(ArcColor.r, ArcColor.g, ArcColor.b, 0f);
        lr.enabled       = false;
    }

    public bool TryBlockSteal(Bat bat)
    {
        if (GameManager.Instance == null) return false;
        if (!GameManager.Instance.HasEquipment("Knife")) return false;
        if (!slashing) StartCoroutine(SlashRoutine(bat));
        return true;
    }

    private IEnumerator SlashRoutine(Bat bat)
    {
        slashing   = true;
        lr.enabled = true;

        bat?.OnKnifeHit();

        float elapsed    = 0f;
        float startAngle = -90f;
        float endAngle   = startAngle + ArcDegrees;

        while (elapsed < Duration)
        {
            elapsed += Time.deltaTime;
            float t          = elapsed / Duration;
            float currentEnd = Mathf.Lerp(startAngle, endAngle, t);
            DrawArc(startAngle, currentEnd);

            float alpha   = Mathf.Lerp(1f, 0f, t * t);
            lr.startColor = new Color(ArcColor.r, ArcColor.g, ArcColor.b, alpha);
            lr.endColor   = new Color(ArcColor.r, ArcColor.g, ArcColor.b, 0f);

            yield return null;
        }

        lr.enabled = false;
        slashing   = false;
    }

    private void DrawArc(float fromDeg, float toDeg)
    {
        float step = (toDeg - fromDeg) / Segments;
        for (int i = 0; i <= Segments; i++)
        {
            float angle = fromDeg + step * i;
            float rad   = angle * Mathf.Deg2Rad;
            lr.SetPosition(i, transform.position + new Vector3(
                Mathf.Cos(rad) * Radius,
                Mathf.Sin(rad) * Radius,
                0f));
        }
    }
}