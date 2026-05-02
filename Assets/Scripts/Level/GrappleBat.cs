using UnityEngine;

public class GrappleBat : MonoBehaviour
{
    enum State { Idle, Attracted, Carrying, Returning }

    [Header("Detection")]
    public float DetectRange  = 8f;
    public float GrabDistance = 0.6f;

    [Header("Movement")]
    public float FlySpeed    = 4f;
    public float ReturnSpeed = 2f;

    [Header("Carry Path")]
    public Transform[] Waypoints;
    public float WaypointThreshold = 0.4f;

    [Header("Escape")]
    public KeyCode EscapeKey             = KeyCode.Space;
    public int     EscapePressesRequired = 12;
    public float   ShakeDuration         = 0.15f;
    public float   ShakeMagnitude        = 0.12f;
    public GameObject EscapePrompt;

    [Header("Visuals")]
    public float FlapSpeed = 8f;

    private State            state = State.Idle;
    private Vector3          homePos;
    private Rigidbody2D      rb;
    private Transform        player;
    private PlayerController playerController;
    private Rigidbody2D      playerRb;
    private int              waypointIndex;
    private int              escapePresses;
    private float            shakeTimer;
    private float            playerOriginalGravity;
    private CanvasGroup      escapePromptGroup;

    void Start()
    {
        homePos = transform.position;

        rb                = GetComponent<Rigidbody2D>();
        rb.gravityScale   = 0;
        rb.freezeRotation = true;

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player           = playerObj.transform;
            playerController = playerObj.GetComponent<PlayerController>();
            playerRb         = playerObj.GetComponent<Rigidbody2D>();
        }

        if (EscapePrompt != null)
        {
            escapePromptGroup = EscapePrompt.GetComponent<CanvasGroup>();
            EscapePrompt.SetActive(false);
        }
    }

    void Update()
    {
        if (player == null) return;

        switch (state)
        {
            case State.Idle:      UpdateIdle();      break;
            case State.Attracted: UpdateAttracted(); break;
            case State.Carrying:  UpdateCarrying();  break;
            case State.Returning: UpdateReturning(); break;
        }

        Flap();
    }

    void FixedUpdate()
    {
        if (state == State.Carrying && player != null && playerRb != null)
        {
            Vector3 pos = new Vector3(transform.position.x, transform.position.y, player.position.z);
            if (shakeTimer > 0f)
            {
                shakeTimer -= Time.fixedDeltaTime;
                pos += (Vector3)(Random.insideUnitCircle * ShakeMagnitude);
            }
            playerRb.MovePosition(pos);
        }
    }

    // ── States ────────────────────────────────────────────────────────────

    void UpdateIdle()
    {
        if (Vector2.Distance(transform.position, homePos) > 0.1f)
            MoveToward(homePos, ReturnSpeed);
        else
            rb.linearVelocity = Vector2.zero;

        if (Vector2.Distance(transform.position, player.position) <= DetectRange)
            state = State.Attracted;
    }

    void UpdateAttracted()
    {
        MoveToward(player.position, FlySpeed);

        if (Vector2.Distance(transform.position, player.position) <= GrabDistance)
            GrabPlayer();
    }

    void UpdateCarrying()
    {
        if (Input.GetKeyDown(EscapeKey))
        {
            shakeTimer = ShakeDuration;
            escapePresses++;
            if (escapePresses >= EscapePressesRequired)
            {
                ReleasePlayer();
                return;
            }
        }

        if (escapePromptGroup != null)
            escapePromptGroup.alpha = 0.5f + Mathf.Sin(Time.time * 5f) * 0.5f;

        if (Waypoints == null || Waypoints.Length == 0) return;

        MoveToward(Waypoints[waypointIndex].position, FlySpeed);

        if (Vector2.Distance(transform.position, Waypoints[waypointIndex].position) <= WaypointThreshold)
            waypointIndex = (waypointIndex + 1) % Waypoints.Length;
    }

    void UpdateReturning()
    {
        MoveToward(homePos, ReturnSpeed);

        if (Vector2.Distance(transform.position, homePos) <= 0.1f)
        {
            rb.linearVelocity = Vector2.zero;
            state = State.Idle;
        }
    }

    // ── Grab / Release ────────────────────────────────────────────────────

    void GrabPlayer()
    {
        rb.linearVelocity = Vector2.zero;
        if (playerRb != null)
        {
            playerOriginalGravity    = playerRb.gravityScale;
            playerRb.gravityScale    = 0;
            playerRb.linearVelocity  = Vector2.zero;
            playerRb.isKinematic     = true;
        }
        if (playerController != null) playerController.IsGrabbed = true;

        escapePresses = 0;
        waypointIndex = 0;
        state         = State.Carrying;

        if (EscapePrompt != null) EscapePrompt.SetActive(true);
    }

    void ReleasePlayer()
    {
        if (playerRb != null)
        {
            playerRb.isKinematic  = false;
            playerRb.gravityScale = playerOriginalGravity;
        }
        if (playerController != null) playerController.IsGrabbed = false;
        state = State.Returning;

        if (EscapePrompt != null) EscapePrompt.SetActive(false);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    void MoveToward(Vector3 target, float speed)
    {
        Vector2 dir = ((Vector2)target - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * speed;

        if (Mathf.Abs(dir.x) > 0.01f)
            transform.localScale = new Vector3(Mathf.Sign(dir.x), transform.localScale.y, 1f);
    }

    void Flap()
    {
        float flap = 1f + Mathf.Sin(Time.time * FlapSpeed) * 0.15f;
        transform.localScale = new Vector3(transform.localScale.x, flap, 1f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, DetectRange);

        if (Waypoints == null || Waypoints.Length == 0) return;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.9f);
        for (int i = 0; i < Waypoints.Length; i++)
        {
            if (Waypoints[i] == null) continue;
            Gizmos.DrawWireSphere(Waypoints[i].position, 0.3f);
            int next = (i + 1) % Waypoints.Length;
            if (Waypoints[next] != null)
                Gizmos.DrawLine(Waypoints[i].position, Waypoints[next].position);
        }
    }
}
