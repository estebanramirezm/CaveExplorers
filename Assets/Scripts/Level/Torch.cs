using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Torch : MonoBehaviour
{
    public static List<Torch> All { get; } = new List<Torch>();

    [Header("Interaction")]
    public KeyCode InteractKey  = KeyCode.E;
    public float   InteractRange = 2f;
    public GameObject InteractPrompt;

    [Header("State")]
    public bool StartsLit = false;

    [Header("Components")]
    public ParticleSystem FireParticles;
    public Light2D        FireLight;

    public bool IsLit { get; private set; }

    private Transform player;
    private bool      promptVisible;

    void OnEnable()  { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (InteractPrompt != null) InteractPrompt.SetActive(false);

        IsLit = StartsLit;
        ApplyState();
    }

    void Update()
    {
        if (player == null) return;

        bool inRange = Vector2.Distance(transform.position, player.position) <= InteractRange;

        if (inRange != promptVisible)
        {
            promptVisible = inRange;
            InteractPrompt?.SetActive(inRange);
        }

        if (inRange && Input.GetKeyDown(InteractKey))
        {
            IsLit = !IsLit;
            ApplyState();
        }
    }

    void ApplyState()
    {
        if (FireParticles != null)
        {
            if (IsLit) FireParticles.Play();
            else       FireParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (FireLight != null)
            FireLight.enabled = IsLit;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, InteractRange);
    }
}
