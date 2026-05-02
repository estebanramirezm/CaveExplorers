using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityTutorialUI : MonoBehaviour
{
    [Header("References")]
    public GameObject Panel;
    public Image AbilityIcon;
    public TextMeshProUGUI InstructionsText;
    public TextMeshProUGUI PromptText;

    [Header("Ability Entries")]
    public AbilityEntry[] Abilities;

    [Header("Settings")]
    public float InputDelay = 0.4f;

    [System.Serializable]
    public struct AbilityEntry
    {
        public string Name;
        public Sprite Icon;
        [TextArea] public string Instructions;
    }

    private bool isShowing;

    void Awake()
    {
        Panel?.SetActive(false);
    }

    void OnEnable()  { GameManager.OnAbilityUnlocked += Show; }
    void OnDisable() { GameManager.OnAbilityUnlocked -= Show; }

    private void Show(string abilityName)
    {
        Debug.Log($"[AbilityTutorialUI] Show called for: {abilityName}");

        AbilityEntry entry;
        if (!TryGetEntry(abilityName, out entry)) return;

        Debug.Log($"[AbilityTutorialUI] Entry found. Panel={Panel}, Icon={AbilityIcon}, Text={InstructionsText}");

        if (AbilityIcon)      AbilityIcon.sprite    = entry.Icon;
        if (InstructionsText) InstructionsText.text  = entry.Instructions;

        Panel?.SetActive(true);
        isShowing = true;
        Time.timeScale = 0f;

        StartCoroutine(WaitForDismiss());
    }

    private IEnumerator WaitForDismiss()
    {
        yield return new WaitForSecondsRealtime(InputDelay);

        while (isShowing)
        {
            if (Input.anyKeyDown) { Dismiss(); yield break; }
            yield return null;
        }
    }

    public void Dismiss()
    {
        isShowing = false;
        Panel?.SetActive(false);
        Time.timeScale = 1f;
    }

    private bool TryGetEntry(string abilityName, out AbilityEntry result)
    {
        foreach (var entry in Abilities)
        {
            if (entry.Name == abilityName) { result = entry; return true; }
        }
        Debug.LogWarning($"[AbilityTutorialUI] No entry for ability: {abilityName}");
        result = default;
        return false;
    }
}
