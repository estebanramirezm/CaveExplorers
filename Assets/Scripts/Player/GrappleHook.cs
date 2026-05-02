using UnityEngine;

/// <summary>
/// Attach to Player. Finds the nearest GrapplePoint in range,
/// connects a DistanceJoint2D, and lets the player swing.
/// Press G to attach, G again to release. Hold W to reel in.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(Animator))]
public class GrappleHook : MonoBehaviour
{
    [Header("Settings")]
    public float GrappleRange = 8f;
    public float ReelSpeed    = 2f;
    public KeyCode GrappleKey = KeyCode.G;

    [Header("Swing")]
    public float SwingImpulse     = 2f;
    public float MinSwingVelocity = 0.5f;

    [Header("Line")]
    public int LineSegments = 12;

    private Rigidbody2D rb;
    private Animator animator;
    private LineRenderer line;
    private DistanceJoint2D joint;
    private GrapplePoint currentPoint;
    public bool IsGrappling { get; private set; }

    void Awake()
    {
        rb       = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        line     = GetComponent<LineRenderer>();
        line.positionCount = 0;
    }

    void Update()
    {
        if (Input.GetKeyDown(GrappleKey))
        {
            if (!GameManager.Instance.HasAbility("Grapple")) return;
            if (!IsGrappling) TryGrapple();
            else              ReleaseGrapple();
        }

        if (IsGrappling && joint != null && Input.GetKey(KeyCode.W))
            joint.distance = Mathf.Max(1.5f, joint.distance - ReelSpeed * Time.deltaTime);

        DrawRope();
    }

    // ── Attach ────────────────────────────────────────────────────────

    private void TryGrapple()
    {
        GrapplePoint nearest = FindNearest();
        if (nearest == null) return;

        currentPoint = nearest;
        IsGrappling  = true;
        animator?.SetBool("IsGrappling", true);

        joint = gameObject.AddComponent<DistanceJoint2D>();
        joint.connectedAnchor = nearest.transform.position;
        joint.distance        = Vector2.Distance(transform.position, nearest.transform.position);
        joint.enableCollision = false;
        joint.maxDistanceOnly = false;

        nearest.SetActive(true);
        line.positionCount = LineSegments;

        float hVel = rb.linearVelocity.x;
        if (Mathf.Abs(hVel) >= MinSwingVelocity)
            rb.AddForce(new Vector2(hVel * SwingImpulse, 0f), ForceMode2D.Impulse);
        else
        {
            float dir = nearest.transform.position.x > transform.position.x ? 1f : -1f;
            rb.AddForce(new Vector2(dir * SwingImpulse, 0f), ForceMode2D.Impulse);
        }
    }

    private void ReleaseGrapple()
    {
        if (joint != null) Destroy(joint);
        if (currentPoint != null) currentPoint.SetActive(false);

        currentPoint       = null;
        IsGrappling        = false;
        animator?.SetBool("IsGrappling", false);
        line.positionCount = 0;
    }

    // ── Nearest Point ─────────────────────────────────────────────────

    private GrapplePoint FindNearest()
    {
        GrapplePoint[] points = FindObjectsOfType<GrapplePoint>();
        GrapplePoint nearest  = null;
        float minDist         = GrappleRange;

        foreach (var p in points)
        {
            float d = Vector2.Distance(transform.position, p.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = p;
            }
        }
        return nearest;
    }

    // ── Draw Rope ─────────────────────────────────────────────────────

    private void DrawRope()
    {
        if (!IsGrappling || currentPoint == null)
        {
            line.positionCount = 0;
            return;
        }

        Vector3 start = transform.position;
        Vector3 end   = currentPoint.transform.position;

        for (int i = 0; i < LineSegments; i++)
        {
            float t   = i / (float)(LineSegments - 1);
            float sag = Mathf.Sin(t * Mathf.PI) * 0.3f;
            Vector3 pos = Vector3.Lerp(start, end, t) + Vector3.down * sag;
            line.SetPosition(i, pos);
        }
    }
}