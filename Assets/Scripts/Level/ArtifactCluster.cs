using UnityEngine;

/// <summary>
/// The artifact cluster at the end of each level.
/// Contains the main artifact + yellow firefly + white fireflies.
/// Player walks into it to collect everything.
/// Wire OnClusterCollected to LevelManager.OnArtifactCollected if needed.
/// </summary>
public class ArtifactCluster : MonoBehaviour
{
    [Header("Contents")]
    public int WhiteFireflyCount = 5;
    public bool ContainsYellowFirefly = true;
    public bool ContainsArtifact = true;

    [Header("Spawning")]
    public GameObject FireflyPrefab;    // assign Firefly prefab
    public float SpawnRadius = 0.8f;

    [Header("Visuals")]
    public float BobSpeed   = 1.5f;
    public float BobAmount  = 0.2f;
    public float RotateSpeed = 45f;

    private Vector3 startPos;
    private bool collected;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        if (collected) return;
        float newY = startPos.y + Mathf.Sin(Time.time * BobSpeed) * BobAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        transform.Rotate(0, 0, RotateSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;
        var swarm = other.GetComponentInParent<FireflySwarm>();

        // Spawn and collect white fireflies
        if (FireflyPrefab != null)
        {
            for (int i = 0; i < WhiteFireflyCount; i++)
            {
                Vector2 offset = Random.insideUnitCircle * SpawnRadius;
                var go = Instantiate(FireflyPrefab,
                    transform.position + new Vector3(offset.x, offset.y, 0),
                    Quaternion.identity);
                var f = go.GetComponent<Firefly>();
                if (f != null)
                {
                    f.Type = FireflyType.White;
                    swarm?.AddFirefly(f);
                    go.SetActive(false); // collected immediately
                }
            }

            // Spawn yellow firefly
            if (ContainsYellowFirefly)
            {
                var go = Instantiate(FireflyPrefab, transform.position, Quaternion.identity);
                var f  = go.GetComponent<Firefly>();
                if (f != null)
                {
                    f.Type = FireflyType.Yellow;
                    swarm?.AddFirefly(f);
                    go.SetActive(false);
                }
            }
        }

        // Count as artifact
        if (ContainsArtifact)
        {
            string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            GameManager.Instance?.CollectArtifact(scene, 0); // 0 = cluster artifact ID
            LevelManager.Instance?.OnArtifactCollected(null);
        }

        gameObject.SetActive(false);
        Debug.Log("[ArtifactCluster] Collected!");
    }
}
