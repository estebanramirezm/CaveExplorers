using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Attach to Player. Handles Flare Gun equipment.
/// One use per level — permanently illuminates the room.
/// Resets on level reload.
/// Press F to activate (only if owned and not used this level).
/// </summary>
public class FlareGun : MonoBehaviour
{
    [Header("Flare Settings")]
    public float FlareDuration  = 3f;
    public float FlareIntensity = 1.5f;

    [Header("References")]
    public Light2D RoomLight;

    private bool usedThisLevel = false;

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.F)) return;

        Debug.Log("[FlareGun] F pressed.");

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[FlareGun] GameManager.Instance is null!");
            return;
        }
        if (!GameManager.Instance.HasEquipment("Flare Gun"))
        {
            Debug.LogWarning("[FlareGun] Player does not own 'Flare Gun'. Check GameManager.BuyEquipment was called with exact string 'Flare Gun'.");
            return;
        }
        if (usedThisLevel)
        {
            Debug.LogWarning("[FlareGun] Already used this level.");
            return;
        }

        TriggerFlare();
    }

    public void TriggerFlare()
    {
        if (usedThisLevel) return;
        if (RoomLight == null)
        {
            Debug.LogWarning("[FlareGun] No RoomLight assigned!");
            return;
        }

        usedThisLevel = true;
        StartCoroutine(FlareRoutine());
        Debug.Log("[FlareGun] Fired! Room permanently illuminated.");
    }

    public void ResetForNewLevel()
    {
        usedThisLevel = false;
    }

    public bool CanUse => GameManager.Instance != null
                       && GameManager.Instance.HasEquipment("Flare Gun")
                       && !usedThisLevel;

    private IEnumerator FlareRoutine()
    {
        RoomLight.enabled   = true;
        RoomLight.intensity = 0f;

        float elapsed = 0f;
        while (elapsed < FlareDuration)
        {
            elapsed            += Time.deltaTime;
            RoomLight.intensity = Mathf.Lerp(0f, FlareIntensity, elapsed / FlareDuration);
            yield return null;
        }

        RoomLight.intensity = FlareIntensity;
        Debug.Log("[FlareGun] Room now permanently lit.");
    }
}