using UnityEngine;

public class FireflyLure : MonoBehaviour
{
    public static FireflyLure Active { get; private set; }

    [Header("Settings")]
    public float LureRadius = 10f;

    [Header("Projectile")]
    public float ProjectileSpeed = 10f;
    public float ProjectileDistance = 6f;

    public bool IsLanded { get; private set; }

    private SpriteRenderer sr;
    private bool inFlight;
    private Vector2 flyDir;
    private float flownDist;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();

        if (sr)
            sr.color = Firefly.GetColor(FireflyType.Purple);
    }

    void OnEnable()
    {
        if (Active != null && Active != this)
            Destroy(Active.gameObject);

        Active = this;
    }

    void OnDisable()
    {
        if (Active == this)
            Active = null;
    }

    public void Launch(Vector2 direction)
    {
        flyDir = direction.normalized;
        flownDist = 0f;
        inFlight = true;
        IsLanded = false;
    }

    void Update()
    {
        if (inFlight)
        {
            float step = ProjectileSpeed * Time.deltaTime;
            transform.position += (Vector3)(flyDir * step);
            flownDist += step;

            if (flownDist >= ProjectileDistance)
            {
                inFlight = false;
                IsLanded = true;
            }

            return;
        }

        if (sr)
        {
            Color c = Firefly.GetColor(FireflyType.Purple);
            float alpha = 0.6f + Mathf.Sin(Time.time * 5f) * 0.3f;
            sr.color = new Color(c.r, c.g, c.b, alpha);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.7f, 0.2f, 1f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, LureRadius);
    }
}