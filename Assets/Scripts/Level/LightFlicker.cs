using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFlicker : MonoBehaviour
{
    public float BaseIntensity  = 1.5f;
    public float FlickerAmount  = 0.4f;
    public float FlickerSpeed   = 8f;

    private Light2D light2D;

    void Awake() => light2D = GetComponent<Light2D>();

    void Update()
    {
        if (light2D == null) return;
        light2D.intensity = BaseIntensity + Mathf.PerlinNoise(Time.time * FlickerSpeed, 0f) * FlickerAmount - FlickerAmount * 0.5f;
    }
}
