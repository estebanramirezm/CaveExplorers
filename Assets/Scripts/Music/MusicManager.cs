using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;

    [System.Serializable]
    public struct SceneMusic
    {
        public string SceneName;
        public AudioClip Clip;
    }

    [Header("Tracks")]
    public SceneMusic[] Tracks;

    [Header("Audio")]
    [Range(0f, 1f)]
    public float Volume = 1f;

    private AudioSource audioSource;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource           = gameObject.AddComponent<AudioSource>();
        audioSource.loop      = true;
        audioSource.volume    = Volume;
        audioSource.playOnAwake = false;
    }

    void OnEnable()  { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;

        foreach (var entry in Tracks)
        {
            if (entry.SceneName == scene.name)
            {
                PlayClip(entry.Clip);
                return;
            }
        }
    }

    void PlayClip(AudioClip clip)
    {
        if (clip == null || audioSource.clip == clip) return;

        audioSource.clip = clip;
        audioSource.Play();
    }
}
