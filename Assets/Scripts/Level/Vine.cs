using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates a swinging vine made of connected Rigidbody2D segments.
/// Setup:
///   1. Create empty GameObject named "Vine"
///   2. Attach this script
///   3. Set Segment Count and Segment Length in Inspector
///   4. The vine hangs from this object's position downward
/// </summary>
public class Vine : MonoBehaviour
{
    [Header("Vine Settings")]
    public int SegmentCount = 8;
    public float SegmentLength = 0.4f;
    public float SegmentWidth = 0.1f;
    public float GrabWidth = 0.8f;   // width of the invisible grab trigger — increase to make grabbing easier
    public Color VineColor = new Color(0.2f, 0.6f, 0.1f);

    [Header("Physics")]
    public float SwayForce = 2f;
    public float MaxSegmentSpeed = 3f;

    private List<GameObject> segments = new List<GameObject>();
    private List<Rigidbody2D> segRbs  = new List<Rigidbody2D>();
    private VineGrabber currentGrabber;

    void Start()
    {
        BuildVine();
        InvokeRepeating(nameof(ApplySway), 1f, 3f);
    }

    void FixedUpdate()
    {
        foreach (var rb in segRbs)
        {
            if (rb != null && rb.linearVelocity.magnitude > MaxSegmentSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * MaxSegmentSpeed;
            }
        }
    }

    private void BuildVine()
    {
        GameObject prev = null;

        for (int i = 0; i < SegmentCount; i++)
        {
            GameObject seg = new GameObject($"Segment_{i}");
            seg.transform.parent   = transform;
            seg.transform.position = transform.position + Vector3.down * i * SegmentLength;
            seg.tag = "Vine";

            var sr = seg.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite();
            sr.color  = VineColor;
            seg.transform.localScale = new Vector3(SegmentWidth, SegmentLength, 1);

            var rb = seg.AddComponent<Rigidbody2D>();
            rb.mass = 0.1f;
            rb.constraints = RigidbodyConstraints2D.None;

            var col = seg.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(GrabWidth / SegmentWidth, 1f); // wider than the visual

            var joint = seg.AddComponent<HingeJoint2D>();
            if (prev == null)
            {
                joint.connectedAnchor = transform.position;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
            else
            {
                joint.connectedBody   = prev.GetComponent<Rigidbody2D>();
                joint.connectedAnchor = Vector2.up * SegmentLength * 0.5f;
            }

            // Limit rotation to ±2 degrees with damping
            joint.useLimits = true;
            joint.limits = new JointAngleLimits2D { min = -2f, max = 2f };
            joint.useMotor = true;
            joint.motor = new JointMotor2D { maxMotorTorque = 0.5f, motorSpeed = 0f };

            var vs = seg.AddComponent<VineSegment>();
            vs.ParentVine = this;

            segments.Add(seg);
            segRbs.Add(rb);
            prev = seg;
        }
    }

    private void ApplySway()
    {
        float dir = Random.value > 0.5f ? 1f : -1f;
        foreach (var rb in segRbs)
            rb?.AddForce(Vector2.right * dir * SwayForce, ForceMode2D.Impulse);
    }

    public Rigidbody2D GetSegmentRb(int index) => segRbs[index];

    public void OnPlayerGrab(VineGrabber grabber, int segmentIndex)
    {
        currentGrabber = grabber;
    }

    public void OnPlayerRelease()
    {
        currentGrabber = null;
    }

    public bool IsGrabbed => currentGrabber != null;
    public int GetSegmentCount() => SegmentCount;

    private Sprite CreateSquareSprite()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
    }
}

/// <summary>
/// Identifies each vine segment for grab detection.
/// </summary>
public class VineSegment : MonoBehaviour
{
    public Vine ParentVine;
}