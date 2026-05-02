using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float Speed    = 9f;
    public float Lifetime = 4f;
    public int   Damage   = 1;

    private Rigidbody2D rb;
    private Transform   shooter;

    void Awake()
    {
        rb              = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    public void Launch(Vector2 direction, Transform source)
    {
        shooter          = source;
        rb.linearVelocity = direction * Speed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Destroy(gameObject, Lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponentInParent<PlayerController>()?.ReceiveDamage(Damage, shooter);
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}
