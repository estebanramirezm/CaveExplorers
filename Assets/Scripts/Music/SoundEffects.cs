using UnityEngine;

public class TriggerSound : MonoBehaviour
{
    public AudioClip Clip;
    [Range(0f, 1f)]
    public float Volume = 1f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var ability = other.GetComponentInParent<AbilitySystem>();
        if (ability != null && ability.IsRolling) return;

        if (Clip == null) return;
        var go = new GameObject("OneShotAudio");
        var src = go.AddComponent<AudioSource>();
        src.clip         = Clip;
        src.volume       = Volume;
        src.spatialBlend = 0f;
        src.Play();
        Destroy(go, Clip.length + 0.1f);
    }
}
