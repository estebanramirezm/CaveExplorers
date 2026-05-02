using System.Collections;
using UnityEngine;

/// <summary>
/// A firefly collectible in the level.
/// White  = common light source, counts toward level exit requirement, can be sent as scout
/// Blue   = unlocks Grapple (permanent)
/// Red    = unlocks Roll (permanent)
/// Green  = unlocks WallClimb (permanent)
/// Yellow = unlocks Run (permanent)
/// Orange = unlocks Crawl (permanent)
/// Purple = end of level radar, illuminates room temporarily
/// </summary>
public class Firefly : MonoBehaviour
{
    [Header("Type")]
    public FireflyType Type = FireflyType.White;

    [Header("Unique ID (White fireflies only)")]
    public int FireflyId;

    [Header("Idle Movement")]
    public float WanderRadius = 1.5f;
    public float WanderSpeed  = 0.8f;
    public float WanderPause  = 1.5f;

    [Header("Visuals")]
    public float PulseSpeed  = 3f;
    public float PulseAmount = 0.3f;

    [Header("Audio")]
    public AudioClip CollectSound;

    private Vector3 homePos;
    private Vector3 targetPos;
    private SpriteRenderer sr;
    private bool collected;
    private bool orbiting;

    public static Color GetColor(FireflyType t) => t switch
    {
        FireflyType.White  => new Color(1f,   1f,   1f,   1f),
        FireflyType.Yellow => new Color(1f,   0.95f,0.2f, 1f),
        FireflyType.Blue   => new Color(0.2f, 0.6f, 1f,   1f),
        FireflyType.Red    => new Color(1f,   0.2f, 0.2f, 1f),
        FireflyType.Green  => new Color(0.2f, 1f,   0.3f, 1f),
        FireflyType.Orange => new Color(1f,   0.6f, 0.2f, 1f),  // Crawl
        FireflyType.Purple => new Color(0.7f, 0.2f, 1f,   1f),
        _                  => Color.white
    };

    void Start()
    {
        homePos   = transform.position;
        targetPos = homePos;
        sr        = GetComponent<SpriteRenderer>();
        if (sr) sr.color = GetColor(Type);

        if (Type == FireflyType.White && FireflyId > 0)
        {
            string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (GameManager.Instance != null && GameManager.Instance.IsWhiteFireflyCollected(scene, FireflyId))
            {
                Destroy(gameObject);
                return;
            }
        }

        if (Type == FireflyType.Yellow || Type == FireflyType.Blue   ||
            Type == FireflyType.Red    || Type == FireflyType.Green  ||
            Type == FireflyType.Orange || Type == FireflyType.Purple)
        {
            if (GameManager.Instance != null && GameManager.Instance.HasColoredFirefly(Type))
            {
                Destroy(gameObject);
                return;
            }
        }

        StartCoroutine(Wander());
    }

    void Update()
    {
        if (collected || orbiting) return;

        transform.position = Vector3.MoveTowards(
            transform.position, targetPos, WanderSpeed * Time.deltaTime);

        if (sr)
        {
            Color c     = GetColor(Type);
            float alpha = 0.6f + Mathf.Sin(Time.time * PulseSpeed) * PulseAmount;
            sr.color    = new Color(c.r, c.g, c.b, alpha);
        }
    }

    private IEnumerator Wander()
    {
        while (!collected && !orbiting)
        {
            Vector2 offset = Random.insideUnitCircle * WanderRadius;
            targetPos      = homePos + new Vector3(offset.x, offset.y, 0);
            yield return new WaitForSeconds(WanderPause + Random.Range(-0.5f, 0.5f));
        }
    }

    public void SetOrbiting(bool value)
    {
        orbiting = value;
        if (orbiting) StopAllCoroutines();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Firefly] OnTriggerEnter2D hit by: {other.name} tag={other.tag} type={Type}");
        if (collected || orbiting) return;
        if (!other.CompareTag("Player")) return;
        Collect(other);
    }

    public void Collect(Collider2D playerCol)
    {
        collected = true;
        StopAllCoroutines();

        if (CollectSound)
            AudioSource.PlayClipAtPoint(CollectSound, transform.position);

        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        switch (Type)
        {
            case FireflyType.White:
                GameManager.Instance?.CollectWhiteFirefly(scene, FireflyId);
                playerCol.GetComponentInParent<FireflySwarm>()?.AddFirefly(this);
                break;

            case FireflyType.Purple:
            case FireflyType.Yellow:
            case FireflyType.Blue:
            case FireflyType.Red:
            case FireflyType.Green:
            case FireflyType.Orange:
                GameManager.Instance?.CollectColoredFirefly(Type);
                gameObject.SetActive(false);
                Destroy(gameObject, 0.5f);
                break;
        }
    }

    void OnRoomIlluminated()
    {
        if (sr) StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        if (sr) sr.color = Color.white;
        yield return new WaitForSeconds(0.5f);
        if (sr) sr.color = GetColor(Type);
    }
}