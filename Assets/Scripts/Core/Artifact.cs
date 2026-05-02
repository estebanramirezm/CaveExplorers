using UnityEngine;

public class Artifact : MonoBehaviour
{
    [Header("Artifact Settings")]
    public int ArtifactId;

    [Header("Visuals")]
    public float BobSpeed  = 1.2f;
    public float BobAmount = 0.2f;

    private Vector3 startPos;
    private bool collected;

    void Start()
    {
        startPos = transform.position;

        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (GameManager.Instance != null && GameManager.Instance.IsArtifactCollected(scene, ArtifactId))
        {
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        if (collected) return;
        float newY = startPos.y + Mathf.Sin(Time.time * BobSpeed) * BobAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        GameManager.Instance?.CollectArtifact(scene, ArtifactId);
        LevelManager.Instance?.OnArtifactCollected(this);

        gameObject.SetActive(false);
        Destroy(gameObject, 0.1f);
    }
}
