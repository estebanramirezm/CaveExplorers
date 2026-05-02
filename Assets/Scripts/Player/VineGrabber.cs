using UnityEngine;

/// <summary>
/// Attach to Player. Handles grabbing, swinging, and climbing vines.
/// 
/// Controls while on vine:
///   Up Arrow / W  = grab vine / climb up
///   Down Arrow / S= slide down (drops if at the bottom)
///   Left/Right / A/D = actively pump the swing
///   Jump        = launch off (preserves momentum)
///   G           = drop gently
///
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class VineGrabber : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────

    [Header("Vine Movement")]
    public float ClimbDelay  = 0.15f;    // metres/sec rope change when climbing
    public float SwingForce  = 30f;      // lateral force while swinging
    public float LaunchForce = 8f;       // impulse on jump-release

    // ── Private state ──────────────────────────────────────────────────

    private Rigidbody2D     rb;
    private DistanceJoint2D joint;      
    private Vine            currentVine;
    private bool            onVine;

    private int             currentSegIndex;
    private float           climbTimer;

    // Nearby vine data
    private Vine            nearbyVine;
    private Rigidbody2D     nearbySegment;
    private int             nearbySegIndex;

    // ── Unity lifecycle ────────────────────────────────────────────────

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Grab
        if (!onVine && nearbyVine != null && (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)))
            Grab();

        if (!onVine) return;

        HandleClimb();
        HandleActiveSwing();
        HandleRelease();
    }

    void FixedUpdate()
    {
        if (!onVine || joint == null) return;
    }

    // ── Input handlers ─────────────────────────────────────────────────

    private void HandleClimb()
    {
        if (climbTimer > 0) climbTimer -= Time.deltaTime;

        if (climbTimer <= 0f)
        {
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                MoveToSegment(currentSegIndex - 1);
            }
            else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                if (currentVine != null && currentSegIndex >= currentVine.GetSegmentCount() - 1)
                {
                    Release(launch: false);
                }
                else
                {
                    MoveToSegment(currentSegIndex + 1);
                }
            }
        }
    }

    private void MoveToSegment(int newIndex)
    {
        if (currentVine == null || newIndex < 0 || newIndex >= currentVine.GetSegmentCount())
            return;

        currentSegIndex = newIndex;
        var segRb = currentVine.GetSegmentRb(currentSegIndex);

        if (joint != null)
        {
            joint.connectedBody = segRb;
            joint.distance = 0f; 
        }
        
        transform.position = segRb.transform.position;
        climbTimer = ClimbDelay;
    }

    /// <summary>
    /// Applies a horizontal impulse when the player holds Left/Right.
    /// </summary>
    private void HandleActiveSwing()
    {
        float h = Input.GetAxisRaw("Horizontal");
        if (Mathf.Approximately(h, 0f)) return;

        rb.AddForce(Vector2.right * (h * SwingForce), ForceMode2D.Force);
    }

    private void HandleRelease()
    {
        if (Input.GetButtonDown("Jump"))  Release(launch: true);
        if (Input.GetKeyDown(KeyCode.G))  Release(launch: false);
    }

    // ── Grab / Release ─────────────────────────────────────────────────

    private void Grab()
    {
        if (nearbyVine == null) return;
        if (!GameManager.Instance.HasAbility("WallClimb")) return;

        currentVine = nearbyVine;
        onVine      = true;
        currentSegIndex = nearbySegIndex;

        var segRb = currentVine.GetSegmentRb(currentSegIndex);

        joint = gameObject.AddComponent<DistanceJoint2D>();
        joint.connectedBody   = segRb;
        joint.connectedAnchor = Vector2.zero;
        joint.anchor          = Vector2.zero;
        joint.autoConfigureDistance = false;
        joint.distance        = 0f;
        joint.maxDistanceOnly = false;
        joint.enableCollision = false;  

        transform.position = segRb.transform.position;

        currentVine.OnPlayerGrab(this, currentSegIndex);
    }

    private void Release(bool launch)
    {
        if (!onVine) return;

        onVine = false;
        
        if (joint != null) { Destroy(joint); joint = null; }
        currentVine?.OnPlayerRelease();
        currentVine = null;

        if (launch)
        {
            // Use the current physics velocity as the launch direction so the player flies out along their natural swing arc.
            Vector2 vel = rb.linearVelocity;
            Vector2 dir = vel.sqrMagnitude > 0.01f ? vel.normalized : Vector2.up;
            rb.AddForce(dir * LaunchForce, ForceMode2D.Impulse);
        }
    }

    // ── Trigger Detection ──────────────────────────────────────────────

    void OnTriggerEnter2D(Collider2D other)
    {
        if (onVine) return;
        var seg = other.GetComponent<VineSegment>();
        if (seg == null) return;

        nearbyVine     = seg.ParentVine;
        nearbySegment  = other.GetComponent<Rigidbody2D>();
        nearbySegIndex = other.transform.GetSiblingIndex();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (onVine) return;
        var seg = other.GetComponent<VineSegment>();
        if (seg == null) return;

        nearbyVine    = null;
        nearbySegment = null;
    }

    // ── Public API ─────────────────────────────────────────────────────

    public bool IsOnVine => onVine;
}