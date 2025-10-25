using UnityEngine;
using DG.Tweening;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    // Why: Singleton pattern - only one AudioManager exists
    public static AudioManager Instance;

    [Header("Audio Sources - DO NOT ASSIGN MANUALLY")]
    [SerializeField] private AudioSource musicSource1;
    [SerializeField] private AudioSource musicSource2;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Tracks")]
    public AudioClip menuMusic;
    public AudioClip setupMusic;
    public AudioClip[] gamePlaylist; // Multiple tracks for game scene

    [Header("UI Sound Effects")]
    public AudioClip buttonClick;
    public AudioClip buttonHover;
    public AudioClip characterSelect;
    public AudioClip bandComplete;

    [Header("Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    [Range(0.5f, 3f)] public float crossfadeDuration = 1.5f;

    // Why: Track which source is currently active for crossfading
    private AudioSource activeSource;
    private AudioSource inactiveSource;
    private bool isPlayingPlaylist = false;
    private int currentPlaylistIndex = 0;

    private void Awake()
    {
        // Why: Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Why: Create the two music sources for crossfading
        SetupAudioSources();
    }

    public void OnSceneActivated(string sceneName)
    {
        // Why: Called by SceneLoader when a scene is activated
        Debug.Log($"🎬 Scene activated: {sceneName}");

        if (sceneName == "MainMenu" && menuMusic != null)
        {
            PlayMusic(menuMusic);
        }
        else if (sceneName == "Setup" && setupMusic != null)
        {
            PlayMusic(setupMusic);
        }
        else if (sceneName == "Game" && gamePlaylist != null && gamePlaylist.Length > 0)
        {
            PlayPlaylist(gamePlaylist);
        }
    }

    private void SetupAudioSources()
    {
        // Why: Create music source 1
        if (musicSource1 == null)
        {
            GameObject source1 = new GameObject("MusicSource1");
            source1.transform.SetParent(transform);
            musicSource1 = source1.AddComponent<AudioSource>();
            musicSource1.loop = true;
            musicSource1.playOnAwake = false;
            musicSource1.volume = 0f; // Start silent
        }

        // Why: Create music source 2
        if (musicSource2 == null)
        {
            GameObject source2 = new GameObject("MusicSource2");
            source2.transform.SetParent(transform);
            musicSource2 = source2.AddComponent<AudioSource>();
            musicSource2.loop = true;
            musicSource2.playOnAwake = false;
            musicSource2.volume = 0f; // Start silent
        }

        // Why: Create SFX source
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        // Why: Start with source1 as active
        activeSource = musicSource1;
        inactiveSource = musicSource2;

        sfxSource.volume = sfxVolume;
    }

    public void PlayMusic(AudioClip clip)
    {
        // Why: Stop playlist mode if it was playing
        isPlayingPlaylist = false;
        StopAllCoroutines();

        if (clip == null)
        {
            Debug.LogWarning("AudioManager: Tried to play null music clip");
            return;
        }

        // Why: If same clip is already playing, don't crossfade
        if (activeSource.clip == clip && activeSource.isPlaying)
        {
            Debug.Log($"🎵 Already playing: {clip.name}");
            return;
        }

        Debug.Log($"🎵 Crossfading to: {clip.name}");

        // Why: Swap sources - inactive becomes active
        AudioSource newSource = inactiveSource;
        AudioSource oldSource = activeSource;

        // Why: Setup new source
        newSource.clip = clip;
        newSource.volume = 0f;
        newSource.loop = true;
        newSource.Play();

        // Why: Crossfade
        newSource.DOFade(musicVolume, crossfadeDuration).SetEase(Ease.InOutQuad);
        oldSource.DOFade(0f, crossfadeDuration).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            oldSource.Stop();
            oldSource.clip = null;
        });

        // Why: Update which source is active
        activeSource = newSource;
        inactiveSource = oldSource;
    }

    public void PlayPlaylist(AudioClip[] playlist)
    {
        // Why: Start playing a playlist with crossfade between tracks
        if (playlist == null || playlist.Length == 0)
        {
            Debug.LogWarning("AudioManager: Playlist is empty or null");
            return;
        }

        isPlayingPlaylist = true;
        currentPlaylistIndex = 0;
        StartCoroutine(PlaylistCoroutine(playlist));
    }

    private IEnumerator PlaylistCoroutine(AudioClip[] playlist)
    {
        while (isPlayingPlaylist)
        {
            AudioClip currentClip = playlist[currentPlaylistIndex];

            // Why: Play current track
            PlayMusic(currentClip);

            // Why: Wait for track to finish (minus crossfade time so we overlap)
            float waitTime = currentClip.length - crossfadeDuration;
            yield return new WaitForSeconds(waitTime);

            // Why: Move to next track (loop back to start)
            currentPlaylistIndex = (currentPlaylistIndex + 1) % playlist.Length;
        }
    }

    public void StopMusic()
    {
        // Why: Fade out all music
        isPlayingPlaylist = false;
        StopAllCoroutines();

        activeSource.DOFade(0f, crossfadeDuration).OnComplete(() =>
        {
            activeSource.Stop();
            activeSource.clip = null;
        });
    }

    public void PlaySFX(AudioClip clip)
    {
        // Why: Play a one-shot sound effect
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);

        // Why: Update currently playing source
        if (activeSource.isPlaying)
        {
            activeSource.volume = musicVolume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }

    // Why: Convenience methods for UI buttons
    public void PlayButtonClick() => PlaySFX(buttonClick);
    public void PlayButtonHover() => PlaySFX(buttonHover);
    public void PlayCharacterSelect() => PlaySFX(characterSelect);
    public void PlayBandComplete() => PlaySFX(bandComplete);
}