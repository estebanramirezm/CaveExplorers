using UnityEngine;

public class SkeletonArcher : MonoBehaviour
{
    enum State { Patrol, Frozen }

    [Header("Patrol")]
    public Transform[] Waypoints;
    public float WaypointThreshold = 0.3f;
    public float PatrolSpeed       = 1.5f;
    public float PauseMin          = 1f;
    public float PauseMax          = 3f;

    [Header("Light Stun")]
    public float LightDetectRadius = 6f;

    [Header("Shooting")]
    public GameObject ArrowPrefab;
    public Transform  ArrowSpawnPoint;
    public float ShootRange    = 10f;
    public float ShootCooldown = 2.5f;

    private State        state = State.Patrol;
    private Rigidbody2D  rb;
    private Transform    player;
    private int          waypointIndex;
    private float        pauseTimer;
    private float        shootTimer;

    void Start()
    {
        rb                = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        state = IsInLight() ? State.Frozen : State.Patrol;

        switch (state)
        {
            case State.Patrol: UpdatePatrol(); break;
            case State.Frozen: SetVelocityX(0f); return;
        }

        HandleShooting();
    }

    // ── States ────────────────────────────────────────────────────────────

    void UpdatePatrol()
    {
        if (Waypoints == null || Waypoints.Length == 0)
        {
            SetVelocityX(0f);
            return;
        }

        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;
            SetVelocityX(0f);
            return;
        }

        float dx = Waypoints[waypointIndex].position.x - transform.position.x;

        if (Mathf.Abs(dx) <= WaypointThreshold)
        {
            waypointIndex = (waypointIndex + 1) % Waypoints.Length;
            pauseTimer    = Random.Range(PauseMin, PauseMax);
            SetVelocityX(0f);
            return;
        }

        MoveX(Mathf.Sign(dx), PatrolSpeed);
    }

    // ── Shooting ──────────────────────────────────────────────────────────

    void HandleShooting()
    {
        shootTimer -= Time.deltaTime;
        if (shootTimer > 0f) return;
        if (Vector2.Distance(transform.position, player.position) > ShootRange) return;

        SpawnProjectile();
        shootTimer = ShootCooldown;
    }

    public void SpawnProjectile()
    {
        if (ArrowPrefab == null || player == null) return;

        Vector3 spawnPos = ArrowSpawnPoint != null ? ArrowSpawnPoint.position : transform.position;
        Vector2 dir      = ((Vector2)player.position - (Vector2)spawnPos).normalized;

        Instantiate(ArrowPrefab, spawnPos, Quaternion.identity)
            .GetComponent<Arrow>()?.Launch(dir, transform);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    void MoveX(float dirX, float speed)
    {
        rb.linearVelocity        = new Vector2(dirX * speed, rb.linearVelocity.y);
        transform.localScale     = new Vector3(dirX, 1f, 1f);
    }

    void SetVelocityX(float x)
    {
        rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);
    }

    bool IsInLight()
    {
        foreach (var torch in Torch.All)
        {
            if (torch.IsLit && Vector2.Distance(transform.position, torch.transform.position) <= LightDetectRadius)
                return true;
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, LightDetectRadius);

        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, ShootRange);

        if (Waypoints == null || Waypoints.Length == 0) return;

        Gizmos.color = new Color(0f, 1f, 0.5f, 0.8f);
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
