using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Equipment store in the lobby.
/// Player spends currency on permanent equipment.
/// Press E to open when nearby.
/// 
/// Equipment:
///   Lantern   - increases base light radius
///   Rope      - increases grapple range
///   Knife     - recover stolen fireflies instantly from bat roosts
///   Flare Gun - one use per level, lights entire room permanently
/// </summary>
public class EquipmentStore : MonoBehaviour
{
    [System.Serializable]
    public class EquipmentItem
    {
        public string Name;
        public string Description;
        public int Cost;
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI DescText;
        public TextMeshProUGUI CostText;
        public Button BuyButton;
    }

    [Header("Items")]
    public EquipmentItem Lantern;
    public EquipmentItem Rope;
    public EquipmentItem Knife;
    public EquipmentItem FlareGun;

    [Header("Audio")]
    public AudioClip BuySound;

    [Header("UI")]
    public GameObject StorePanel;
    public TextMeshProUGUI CurrencyText;
    public TextMeshProUGUI InteractPromptText;

    private bool playerNearby;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        audioSource.ignoreListenerPause = true;
        StorePanel?.SetActive(false);
        InteractPromptText?.gameObject.SetActive(false);

        // Set default item data
        SetItemData(Lantern,  "Lantern Upgrade", "Increases base light radius",         7);
        SetItemData(Rope,     "Rope",             "Increases grapple range",             8);
        SetItemData(Knife,    "Knife",            "Press K to recover stolen fireflies", 5);
        SetItemData(FlareGun, "Flare Gun",        "Lights room permanently (1/level)\nPress F to activate", 10);
    }

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (StorePanel && !StorePanel.activeSelf) OpenStore();
            else CloseStore();
        }
        if (Input.GetKeyDown(KeyCode.Escape)) CloseStore();
        if (StorePanel && StorePanel.activeSelf) RefreshUI();
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
        CloseStore();
    }

    private void OpenStore()
    {
        StorePanel?.SetActive(true);
        FindObjectOfType<HeartsUI>()?.Hide();
        Time.timeScale = 0f;
        RefreshUI();
    }

    private void CloseStore()
    {
        StorePanel?.SetActive(false);
        FindObjectOfType<HeartsUI>()?.Show();
        Time.timeScale = 1f;
    }

    private void RefreshUI()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        if (CurrencyText) CurrencyText.text = $"Currency: {gm.Currency}g";

        RefreshItem(Lantern);
        RefreshItem(Rope);
        RefreshItem(Knife);
        RefreshItem(FlareGun);
    }

    private void RefreshItem(EquipmentItem item)
    {
        if (item == null) return;
        var gm    = GameManager.Instance;
        bool owns = gm != null && gm.HasEquipment(item.Name);
        bool can  = gm != null && gm.Currency >= item.Cost;

        if (item.CostText)  item.CostText.text  = owns ? "Owned" : $"{item.Cost}g";
        if (item.BuyButton) item.BuyButton.interactable = !owns && can;
    }

    private void SetItemData(EquipmentItem item, string name, string desc, int cost)
    {
        if (item == null) return;
        item.Name        = name;
        item.Description = desc;
        item.Cost        = cost;
        if (item.NameText) item.NameText.text = name;
        if (item.DescText) item.DescText.text = desc;
    }

    // ── Buy button callbacks ──────────────────────────────────────────

    public void OnBuyLantern()  => TryBuy(Lantern);
    public void OnBuyRope()     => TryBuy(Rope);
    public void OnBuyKnife()    => TryBuy(Knife);
    public void OnBuyFlareGun() => TryBuy(FlareGun);

    private void TryBuy(EquipmentItem item)
    {
        var gm = GameManager.Instance;
        if (gm == null || item == null) return;
        if (gm.HasEquipment(item.Name)) return;

        if (gm.SpendCurrency(item.Cost))
        {
            gm.BuyEquipment(item.Name);
            ApplyEquipmentEffect(item.Name);
            if (BuySound != null) audioSource.PlayOneShot(BuySound);
            RefreshUI();
        }
    }

    private void ApplyEquipmentEffect(string name)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        switch (name)
        {
            case "Lantern Upgrade":
                var swarm = player.GetComponent<FireflySwarm>();
                if (swarm) swarm.BaseRadius += 2f;
                Debug.Log($"[Store] New light radius: {swarm.BaseRadius}");
                break;
            case "Rope":
                var grapple = player.GetComponent<GrappleHook>();
                if (grapple) grapple.GrappleRange += 4f;
                Debug.Log($"[Store] New grapple range: {grapple.GrappleRange}");
                break;
            case "Knife":
                // just enable it — TriggerSlash() called at runtime
                break;
            case "Flare Gun":
                // FlareGun activates via F key, nothing to do here
                break;
            // Knife and FlareGun effects handled in their respective systems
        }
    }
}