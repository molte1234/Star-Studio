using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Singleton pattern - only one AudioManager exists
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music Tracks")]
    public AudioClip menuMusic;
    public AudioClip setupMusic;
    public AudioClip gameMusic;

    [Header("UI Sound Effects")]
    public AudioClip buttonClick;
    public AudioClip buttonHover;
    public AudioClip characterSelect;
    public AudioClip bandComplete;

    [Header("Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;
    [Range(0f, 1f)]
    public float sfxVolume = 0.8f;

    void Awake()
    {
        // Why: Singleton setup
        if (Instance == null)
        {
            Instance = this;
            // No need for DontDestroyOnLoad - Bootstrap scene never unloads!
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Why: Create audio sources if they don't exist
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
    }

    // ===== MUSIC CONTROL =====

    public void PlayMusic(AudioClip clip)
    {
        // Why: Don't restart if same track is already playing
        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            return;
        }

        musicSource.clip = clip;
        musicSource.Play();
        Debug.Log("🎵 Playing music: " + clip.name);
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    // ===== SFX CONTROL =====

    public void PlaySFX(AudioClip clip)
    {
        // Why: Play one-shot sound effects
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }

    // ===== CONVENIENCE METHODS =====

    public void PlayButtonClick()
    {
        PlaySFX(buttonClick);
    }

    public void PlayButtonHover()
    {
        PlaySFX(buttonHover);
    }

    public void PlayCharacterSelect()
    {
        PlaySFX(characterSelect);
    }

    public void PlayBandComplete()
    {
        PlaySFX(bandComplete);
    }
}