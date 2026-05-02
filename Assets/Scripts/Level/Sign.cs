using UnityEngine;
using TMPro;

/// <summary>
/// Attach to any sign GameObject.
/// Requires a CircleCollider2D (Is Trigger = true) and a Canvas popup panel.
///
/// Setup:
///   1. Add this component to the sign sprite/object.
///   2. Add a CircleCollider2D, check "Is Trigger".
///   3. Create a UI Canvas panel with a title TMP text, body TMP text, and a
///      "Press E to close" dismiss label. Assign each in the Inspector.
///   4. Fill in Title and Message in the Inspector — each sign can have its own text.
/// </summary>
public class Sign : MonoBehaviour
{
    [Header("Sign Content")]
    public string Title = "Notice";
    [TextArea(3, 8)]
    public string Message = "Enter your sign text here.";

    [Header("UI References")]
    public GameObject PopupPanel;
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI MessageText;
    public TextMeshProUGUI InteractPromptText;

    private bool playerNearby;
    private bool isOpen;

    void Start()
    {
        PopupPanel?.SetActive(false);
        InteractPromptText?.gameObject.SetActive(false);
    }

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (!isOpen) Open();
            else Close();
        }

        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerNearby = true;
        InteractPromptText?.gameObject.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerNearby = false;
        InteractPromptText?.gameObject.SetActive(false);
        Close();
    }

    private void Open()
    {
        if (TitleText)   TitleText.text   = Title;
        if (MessageText) MessageText.text = Message;

        PopupPanel?.SetActive(true);
        InteractPromptText?.gameObject.SetActive(false);
        FindObjectOfType<HeartsUI>()?.Hide();
        isOpen = true;
    }

    private void Close()
    {
        if (!isOpen) return;
        PopupPanel?.SetActive(false);
        if (playerNearby) InteractPromptText?.gameObject.SetActive(true);
        FindObjectOfType<HeartsUI>()?.Show();
        isOpen = false;
    }
}
