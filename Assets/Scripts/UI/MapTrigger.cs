using UnityEngine;

/// <summary>
/// Place this on the boat object in the lobby.
/// Player walks into it and presses E to open the world map.
/// 
/// Setup:
///   1. Add BoxCollider2D (Is Trigger) to the boat
///   2. Attach this script
///   3. Assign WorldMapCanvas to the WorldMap slot
///   4. Optionally assign an InteractPrompt ("Press E")
/// </summary>
public class MapTrigger : MonoBehaviour
{
    [Header("References")]
    public WorldMap WorldMapUI;
    public GameObject InteractPrompt;

    private bool playerNearby;

    void Start()
    {
        if (InteractPrompt) InteractPrompt.SetActive(false);
    }

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (WorldMapUI == null) return;
            if (WorldMapUI.IsOpen) WorldMapUI.CloseMap();
            else WorldMapUI.OpenMap();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerNearby = true;
        if (InteractPrompt) InteractPrompt.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerNearby = false;
        if (InteractPrompt) InteractPrompt.SetActive(false);
    }
}