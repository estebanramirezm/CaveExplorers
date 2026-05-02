using UnityEngine;

public abstract class Obstacle : MonoBehaviour
{
    [Header("Obstacle Config")]
    public int DamageAmount = 1;
    public bool DealsDamage = true;
    public string BypassAbility = "";

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        HandlePlayerContact(other);
    }

    protected virtual void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.gameObject.CompareTag("Player")) return;
        HandlePlayerContact(col.collider);
    }

    private void HandlePlayerContact(Collider2D playerCol)
    {
        // Check bypass ability
        if (!string.IsNullOrEmpty(BypassAbility) &&
            GameManager.Instance != null &&
            GameManager.Instance.HasAbility(BypassAbility))
        {
            return;
        }

        // Check if player is rolling - rolling bypasses all obstacles
        var ability = playerCol.GetComponentInParent<AbilitySystem>();
        if (ability != null && ability.IsRolling) return;

        if (DealsDamage)
        {
            var pc = playerCol.GetComponentInParent<PlayerController>();
            pc?.ReceiveDamage(DamageAmount, transform);
        }

        OnPlayerHit(playerCol);
    }

    protected virtual void OnPlayerHit(Collider2D playerCol) { }
}