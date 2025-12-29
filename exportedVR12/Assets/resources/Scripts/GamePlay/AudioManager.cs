using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Source")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("Clips")]
    public AudioClip clickSound;
    public AudioClip hoverSound;
    public AudioClip battleStartSound;
    public AudioClip victorySound;
    public AudioClip defeatSound;

    private void Awake()
    {
        // Singleton Pattern (Like GameFlowManager)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayClick()
    {
        if (clickSound) sfxSource.PlayOneShot(clickSound);
    }

    public void PlayHover()
    {
        if (hoverSound) sfxSource.PlayOneShot(hoverSound);
    }

    public void PlayBattleStart()
    {
        if (battleStartSound) sfxSource.PlayOneShot(battleStartSound);
    }

    public void PlayVictory()
    {
        // Stop music to emphasize victory
        if (musicSource) musicSource.Stop(); 
        if (victorySound) sfxSource.PlayOneShot(victorySound);
    }
}