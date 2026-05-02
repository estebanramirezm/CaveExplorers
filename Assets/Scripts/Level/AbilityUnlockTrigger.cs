using UnityEngine;
using TMPro;

/// <summary>
/// Place in the scene as a trigger zone. When the player walks in,
/// it unlocks an ability and shows a popup message.
/// 
/// Setup:
///   1. Create an empty GameObject, add BoxCollider2D (Is Trigger)
///   2. Attach this script
///   3. Set Ability Name to match exactly: "Run", "Roll", "Crawl", "WallClimb", "Grapple"
///   4. Optionally assign a PopupText (world space TMP) to show a message
/// </summary>
public class AbilityUnlockTrigger : MonoBehaviour
{
    [Header("Ability")]
    public string AbilityName;
    public string DisplayMessage = "New Ability Unlocked!";

    [Header("One Time")]
    public bool DestroyAfterUnlock = true;

    [Header("Optional Popup")]
    public GameObject PopupText;        // World space TMP object
    public float PopupDuration = 2.5f;

    private bool triggered;

    void Start()
    {
        if (PopupText) PopupText.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        GameManager.Instance?.UnlockAbility(AbilityName);

        // Hide everything immediately
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            sr.enabled = false;
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        // Show popup
        if (PopupText)
        {
            PopupText.SetActive(true);
            var tmp = PopupText.GetComponent<TMPro.TextMeshPro>();
            if (tmp) tmp.text = DisplayMessage;
            Invoke(nameof(HidePopup), PopupDuration);
        }

        if (DestroyAfterUnlock)
            Invoke(nameof(DestroySelf), PopupDuration + 0.1f);
    }

    private void HidePopup() 
    { 
        if (PopupText) PopupText.SetActive(false); 
    }

    private void DestroySelf() 
    { 
        gameObject.SetActive(false);
        Destroy(gameObject, 0.1f);
    }
}
