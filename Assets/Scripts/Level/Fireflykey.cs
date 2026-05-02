using UnityEngine;

/// <summary>
/// A colored key hidden in the level.
/// Collecting it permanently opens the matching SecretDoor.
/// Key is gone on replay if door is already open.
/// Assign a unique DoorId matching the SecretDoor's DoorId.
/// </summary>
public class FireflyKey : MonoBehaviour
{
    [Header("Key Settings")]
    public FireflyType KeyColor;
    public string DoorId; // must match SecretDoor.DoorId exactly

    [Header("Visuals")]
    public float BobSpeed   = 1.5f;
    public float BobAmount  = 0.15f;
    public float RotateSpeed = 60f;

    private Vector3 startPos;
    private SpriteRenderer sr;

    void Start()
    {
        startPos = transform.position;
        sr       = GetComponent<SpriteRenderer>();

        // Set color to match firefly type
        if (sr) sr.color = Firefly.GetColor(KeyColor);

        // If door already open, destroy key immediately
        if (GameManager.Instance != null && GameManager.Instance.IsDoorOpen(DoorId))
        {
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        // Bob and spin
        float newY = startPos.y + Mathf.Sin(Time.time * BobSpeed) * BobAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        transform.Rotate(0, 0, RotateSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Open the door permanently
        GameManager.Instance?.OpenDoor(DoorId);

        // Find and open matching door in scene
        foreach (var door in FindObjectsOfType<SecretDoor>())
        {
            if (door.DoorId == DoorId)
                door.Unlock();
        }

        Debug.Log($"[FireflyKey] {KeyColor} key collected. Door {DoorId} opened!");
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Firefly.GetColor(KeyColor);
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
}