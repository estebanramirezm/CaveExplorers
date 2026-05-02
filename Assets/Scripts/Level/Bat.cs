using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class Bat : MonoBehaviour
{
    public enum BatState { Idle, Swarm, Scatter, ReturnToRoost, HasFirefly }

    [Header("Speed")]
    public float MoveSpeed = 3f;
    public float WanderSpeed = 1f;

    [Header("Detection")]
    public float LightDetectRadius  = 6f;
    public int   MinFirefliesNeeded = 1;

    [Header("Visuals")]
    public Light2D FireflyGlow;

    [Header("Persistence")]
    [SerializeField] private int batId = -1;

    [Header("Scatter")]
    public float ScatterDuration = 2f;
    private string sceneName;

    private Rigidbody2D  rb;
    private Vector3      scatterDir;
    private BatState     state = BatState.Idle;
    private Transform    player;
    private FireflySwarm swarm;
    private Vector3      homePos;
    private Vector3      wanderTarget;
    private float        scatterTimer;
    private float        wanderTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        homePos = transform.position;
        sceneName = SceneManager.GetActiveScene().name;

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            swarm = playerObj.GetComponent<FireflySwarm>() ??
                    playerObj.GetComponentInChildren<FireflySwarm>();
        }

        rb.gravityScale = 0;
        rb.freezeRotation = true;

        if (player == null)
            Debug.LogWarning("[Bat] Could not find Player tag!");
        else if (swarm == null)
            Debug.LogWarning("[Bat] Found Player but no FireflySwarm component!");

        if (batId >= 0 && GameManager.Instance != null && GameManager.Instance.BatHasFirefly(sceneName, batId))
            state = BatState.HasFirefly;

        if (FireflyGlow != null)
            FireflyGlow.enabled = state == BatState.HasFirefly;
    }

    // ---------------- COLLISION ----------------

    void OnTriggerEnter2D(Collider2D other)
    {
        if (state == BatState.Swarm && FireflyLure.Active == null && other.CompareTag("Player"))
            TrySteal();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (state == BatState.Swarm && FireflyLure.Active == null && other.CompareTag("Player"))
            TrySteal();
    }

    // ---------------- DEBUG ----------------

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, LightDetectRadius);
    }

    // ---------------- STATE ----------------

    private void SetState(BatState newState)
    {
        state = newState;

        if (FireflyGlow != null)
            FireflyGlow.enabled = state == BatState.HasFirefly;

        if (state == BatState.Scatter)
        {
            scatterTimer = ScatterDuration;
            scatterDir = (transform.position - player.position).normalized;
        }
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        int fireflyCount = swarm != null ? swarm.FireflyCount : 0;

        // -------- STATE LOGIC --------

        var lure = FireflyLure.Active;
        bool lureActive = lure != null && lure.IsLanded;

        switch (state)
        {
            case BatState.Idle:
                if (lureActive)
                {
                    float lureDist = Vector2.Distance(transform.position, lure.transform.position);
                    if (lureDist < lure.LureRadius)
                        SetState(BatState.Swarm);
                }
                else if (fireflyCount >= MinFirefliesNeeded && dist < LightDetectRadius)
                {
                    SetState(BatState.Swarm);
                }
                break;

            case BatState.Swarm:
                if (!lureActive && fireflyCount == 0)
                    SetState(BatState.Idle);
                break;

            // HasFirefly: wanders until knife hit — no stealing, no chasing

            case BatState.Scatter:
                scatterTimer -= Time.deltaTime;
                if (scatterTimer <= 0)
                    SetState(BatState.Idle);
                break;
        }

        // -------- MOVEMENT (ONE SPEED ONLY) --------

        Vector2 moveDir = Vector2.zero;

        float speed;
        switch (state)
        {
            case BatState.Idle:
            case BatState.HasFirefly:
                moveDir = Wander();
                speed = WanderSpeed;
                break;

            case BatState.Swarm:
                Vector3 swarmTarget = lureActive ? lure.transform.position : player.position;
                moveDir = (swarmTarget - transform.position).normalized;
                speed = MoveSpeed;
                break;

            case BatState.Scatter:
                moveDir = scatterDir;
                speed = MoveSpeed;
                break;

            default:
                speed = WanderSpeed;
                break;
        }

        rb.linearVelocity = moveDir * speed;
    }

    // ---------------- BEHAVIOUR ----------------

    private Vector2 Wander()
    {
        wanderTimer -= Time.deltaTime;

        if (wanderTimer <= 0)
        {
            Vector2 o = Random.insideUnitCircle * 2f;
            wanderTarget = homePos + new Vector3(o.x, o.y, 0);
            wanderTimer = 2f;
        }

        return (wanderTarget - transform.position).normalized;
    }

    private void TrySteal()
    {
        if (swarm == null || swarm.FireflyCount <= 0) return;

        swarm.RemoveFirefly(FireflyType.White);
        Debug.Log("[Bat] Stole firefly");

        if (batId >= 0)
            GameManager.Instance?.RecordBatSteal(sceneName, batId);

        SetState(BatState.HasFirefly);
    }

    public void OnKnifeHit()
    {
        if (state == BatState.HasFirefly)
        {
            swarm?.AddFireflyVisualOnly(FireflyType.White);
            Debug.Log("[Bat] Firefly returned by knife hit");

            if (batId >= 0)
                GameManager.Instance?.ClearBatSteal(sceneName, batId);
        }

        SetState(BatState.Scatter);
    }
}