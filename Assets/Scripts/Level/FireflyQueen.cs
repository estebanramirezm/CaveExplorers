using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// The Firefly Queen NPC in the forest.
/// Trades WHITE fireflies only for currency.
/// Colored fireflies are permanent abilities and not tradeable.
/// Press E to interact.
/// </summary>
public class FireflyQueen : MonoBehaviour
{
    [Header("Trade Rate")]
    public int WhiteRate = 2; // currency per white firefly

    [Header("Audio")]
    public AudioClip TradeSound;

    [Header("UI")]
    public GameObject TradeUI;
    public TextMeshProUGUI WhiteCountText;
    public TextMeshProUGUI EarningsText;
    public TextMeshProUGUI ColoredNoticeText;
    public TextMeshProUGUI InteractPromptText;
    public Button TradeButton;

    private bool playerNearby;
    private FireflySwarm swarm;
    private Coroutine pulseCoroutine;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        audioSource.ignoreListenerPause = true;
        TradeUI?.SetActive(false);
        InteractPromptText?.gameObject.SetActive(false);
    }

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (TradeUI && !TradeUI.activeSelf) OpenTradeUI();
            else CloseTradeUI();
        }
        if (Input.GetKeyDown(KeyCode.Escape)) CloseTradeUI();

        // DEBUG ONLY - remove before final build
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Q))
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("[DEBUG] No GameObject with tag 'Player' found!");
                return;
            }

            swarm = player.GetComponent<FireflySwarm>();
            if (swarm == null)
                Debug.LogWarning("[DEBUG] FireflySwarm not found on Player!");
            else
                Debug.Log($"[DEBUG] Swarm found! White fireflies: {swarm.GetCount(FireflyType.White)}");

            if (TradeUI && !TradeUI.activeSelf) OpenTradeUI();
            else CloseTradeUI();
        }
        if (Input.GetKeyDown(KeyCode.T))
{
    Debug.Log("[DEBUG] T pressed, calling OnTradeClicked");
    OnTradeClicked();
}
#endif
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerNearby = true;
        swarm = other.GetComponent<FireflySwarm>() ?? other.GetComponentInParent<FireflySwarm>();
        InteractPromptText?.gameObject.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerNearby = false;
        InteractPromptText?.gameObject.SetActive(false);
        CloseTradeUI();
    }

    private void OpenTradeUI()
    {
        TradeUI?.SetActive(true);
        InteractPromptText?.gameObject.SetActive(false);
        FindObjectOfType<HeartsUI>()?.Hide();
        Time.timeScale = 0f;
        RefreshUI();
        if (TradeButton != null)
            pulseCoroutine = StartCoroutine(PulseTradeButton());
    }

    private void CloseTradeUI()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
        TradeUI?.SetActive(false);
        if (playerNearby) InteractPromptText?.gameObject.SetActive(true);
        FindObjectOfType<HeartsUI>()?.Show();
        Time.timeScale = 1f;
    }

    private IEnumerator PulseTradeButton()
    {
        float t = 0f;
        Vector3 baseScale = TradeButton.transform.localScale;
        while (true)
        {
            t += Time.unscaledDeltaTime * 3f;
            float s = 1f + Mathf.Sin(t) * 0.08f;
            TradeButton.transform.localScale = baseScale * s;
            yield return null;
        }
    }

    private void RefreshUI()
    {
        if (swarm == null)
        {
            Debug.LogWarning("[FireflyQueen] RefreshUI called but swarm is null!");
            return;
        }

        int whites = swarm.GetCount(FireflyType.White);
        int total  = whites * WhiteRate;

        if (WhiteCountText)    WhiteCountText.text   = $"White fireflies: {whites} × {WhiteRate}g";
        if (EarningsText)      EarningsText.text      = $"Total: {total}g";
        if (ColoredNoticeText) ColoredNoticeText.text = "Colored fireflies grant abilities\nand cannot be traded.";
    }

    public void OnTradeClicked()
    {
        Debug.Log($"[FireflyQueen] OnTradeClicked called! Swarm null: {swarm == null}");
        if (swarm == null)
        {
            Debug.LogWarning("[FireflyQueen] OnTradeClicked but swarm is null!");
            return;
        }

        int whites = swarm.GetCount(FireflyType.White);
        int total  = whites * WhiteRate;

        if (TradeSound != null) audioSource.PlayOneShot(TradeSound);
        GameManager.Instance?.AddCurrency(total);

        for (int i = 0; i < whites; i++)
            swarm.RemoveFirefly(FireflyType.White);

        Debug.Log($"[FireflyQueen] Traded {whites} white fireflies for {total}g!");
        CloseTradeUI();
    }
}