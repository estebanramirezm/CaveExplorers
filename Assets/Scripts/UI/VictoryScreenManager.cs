using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryScreenManager : MonoBehaviour
{
    [Tooltip("The exact name of your lobby scene")]
    public string lobbySceneName = "Lobby";
    
    [Tooltip("Drag the AudioSource component here")]
    public AudioSource victoryMusic;

    void Start()
    {
        // FAILSAFE: Find all audio sources in the scene
        AudioSource[] allAudio = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audio in allAudio)
        {
            // Just STOP the old music, don't destroy the object!
            if (audio.isPlaying && audio != victoryMusic)
            {
                audio.Stop();
            }
        }

        // Play the victory track!
        if (victoryMusic != null)
        {
            victoryMusic.Play();
        }
    }

    public void ReturnToLobby()
    {
        Debug.Log("Returning to Lobby...");
        SceneManager.LoadScene(lobbySceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit(); 
    }
}