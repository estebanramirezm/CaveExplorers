using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Equipment system: tracks inventory, equips/uses items.
/// Attach to the Player.
/// </summary>
public class EquipmentSystem : MonoBehaviour
{
    // Items currently in the player's inventory (must be unlocked in GameManager)
    public List<string> Inventory { get; private set; } = new List<string>();
    public string EquippedItem { get; private set; }

    [Header("Prefabs (optional – assign in Inspector)")]
    public GameObject LanternPrefab;
    public GameObject RopePrefab;
    public GameObject FlareGunPrefab;

    public UnityEvent<string> OnItemEquipped;
    public UnityEvent<string> OnItemUsed;

    // ── Add to inventory ──────────────────────────────────────────────

    public bool PickUp(string itemName)
    {
        if (!GameManager.Instance.HasEquipment(itemName))
        {
            Debug.LogWarning($"[EquipmentSystem] {itemName} not unlocked in GameManager.");
            return false;
        }
        if (!Inventory.Contains(itemName))
            Inventory.Add(itemName);
        return true;
    }

    // ── Equip ─────────────────────────────────────────────────────────

    public void Equip(string itemName)
    {
        if (!Inventory.Contains(itemName)) return;
        EquippedItem = itemName;
        OnItemEquipped?.Invoke(itemName);
        Debug.Log($"[EquipmentSystem] Equipped: {itemName}");
    }

    // ── Use ───────────────────────────────────────────────────────────

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !string.IsNullOrEmpty(EquippedItem) && EquippedItem != "Knife")
            UseEquipped();
        if (Input.GetKeyDown(KeyCode.K) && GameManager.Instance != null && GameManager.Instance.HasEquipment("Knife"))
            KnifeSlash();
    }

    private void UseEquipped()
    {
        OnItemUsed?.Invoke(EquippedItem);

        switch (EquippedItem)
        {
            case "Lantern":
                ToggleLantern();
                break;
            case "Rope":
                PlaceRope();
                break;
            case "FlareGun":
                FireFlare();
                break;
            case "Knife":
                KnifeSlash();
                break;
            default:
                Debug.Log($"[EquipmentSystem] Used {EquippedItem}");
                break;
        }
    }

    // ── Equipment Implementations (stubs – flesh out per item) ────────

    private GameObject activeLantern;
    private void ToggleLantern()
    {
        if (LanternPrefab == null) return;
        if (activeLantern == null)
            activeLantern = Instantiate(LanternPrefab, transform);
        else
        {
            Destroy(activeLantern);
            activeLantern = null;
        }
    }

    private void PlaceRope()
    {
        if (RopePrefab != null)
            Instantiate(RopePrefab, transform.position, Quaternion.identity);
    }

    private void FireFlare()
    {
        if (FlareGunPrefab != null)
        {
            float dir = Mathf.Sign(transform.localScale.x);
            var flare = Instantiate(FlareGunPrefab, transform.position, Quaternion.identity);
            flare.GetComponent<Rigidbody2D>()?.AddForce(Vector2.right * dir * 10f, ForceMode2D.Impulse);
        }
    }

    private void KnifeSlash()
    {
        // Overlap circle in front of player; any enemy/obstacle tagged "Breakable" gets hit
        float dir = Mathf.Sign(transform.localScale.x);
        Vector2 attackPos = (Vector2)transform.position + Vector2.right * dir * 0.6f;
        Collider2D hit = Physics2D.OverlapCircle(attackPos, 0.5f);
        if (hit != null && hit.CompareTag("Breakable"))
            Destroy(hit.gameObject);
    }
}
