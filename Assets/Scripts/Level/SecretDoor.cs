using UnityEngine;
using TMPro;

/// <summary>
/// A door opened by collecting the matching FireflyKey.
/// Assign a unique DoorId. On scene load checks if already opened.
/// </summary>
public class SecretDoor : MonoBehaviour
{
    [Header("Door Settings")]
    public string DoorId;               // unique ID, must match FireflyKey.DoorId
    public FireflyType RequiredColor;   // just for visual color

    [Header("References")]
    public GameObject DoorSprite;
    public GameObject UnlockEffect;
    public TextMeshPro HintText;

    private bool unlocked;

    void Start()
    {
        if (DoorSprite)
        {
            var sr = DoorSprite.GetComponent<SpriteRenderer>();
            if (sr) sr.color = Firefly.GetColor(RequiredColor);
        }

        if (HintText)
            HintText.text = $"Find the {RequiredColor} key";

        // Already opened in a previous run?
        if (GameManager.Instance != null && GameManager.Instance.IsDoorOpen(DoorId))
            Unlock();
    }

    public void Unlock()
    {
        if (unlocked) return;
        unlocked = true;

        if (DoorSprite)   DoorSprite.SetActive(false);
        if (UnlockEffect) UnlockEffect.SetActive(true);
        if (HintText)     HintText.gameObject.SetActive(false);

        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        Debug.Log($"[SecretDoor] {DoorId} unlocked!");
    }
}