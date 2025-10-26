using UnityEngine;
using DG.Tweening;
using System.Collections;

/// <summary>
/// Singleton AudioManager - handles all music and SFX
/// Now includes event music system with pause/resume
/// </summary>
public class AudioManager : MonoBehaviour
{
    // Why: Singleton pattern - only one AudioManager exists
    public static AudioManager Instance;

    [Header("Audio Sources - DO NOT ASSIGN MANUALLY")]
    [SerializeField] private AudioSource musicSource1;
    [SerializeField] private AudioSource musicSource2;
    [SerializeField] private AudioSource eventMusicSource; // NEW: Dedicated source for event music
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Tracks")]
    public AudioClip menuMusic;
    public AudioClip setupMusic;
    public AudioClip[] gamePlaylist; // Multiple tracks for game scene
    public AudioClip gameOverMusic; // NEW: Music for GameOver scene

    [Header("UI Sound Effects")]
    public AudioClip buttonClick;
    public AudioClip buttonHover;
    public AudioClip characterSelect;
    public AudioClip bandComplete;

    [Header("Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    [Range(0.5f, 3f)] public float crossfadeDuration = 1.5f;
    [Range(0.3f, 2f)] public float eventFadeDuration = 1f; // Faster fade for events

    // Why: Track which source is currently active for crossfading
    private AudioSource activeSource;
    private AudioSource inactiveSource;
    private bool isPlayingPlaylist = false;
    private int currentPlaylistIndex = 0;

    // Why: Store paused music state so we can resume it
    private AudioClip pausedClip;
    private float pausedTime;
    private bool wasPausedDuringPlaylist;
    private AudioClip[] pausedPlaylist;
    private int pausedPlaylistIndex;

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

        // Why: Create the audio sources
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
        else if (sceneName == "GameOver" && gameOverMusic != null)
        {
            PlayMusic(gameOverMusic);
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

        // Why: Create event music source (separate from main music)
        if (eventMusicSource == null)
        {
            GameObject eventSource = new GameObject("EventMusicSource");
            eventSource.transform.SetParent(transform);
            eventMusicSource = eventSource.AddComponent<AudioSource>();
            eventMusicSource.loop = true;
            eventMusicSource.playOnAwake = false;
            eventMusicSource.volume = 0f; // Start silent
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

    // ============================================
    // EVENT MUSIC SYSTEM
    // ============================================

    /// <summary>
    /// Pauses current music and prepares to play event music
    /// Stores the paused state so it can be resumed later
    /// </summary>
    public void PauseMusic()
    {
        Debug.Log("🎵 Pausing music for event...");

        // Why: Store what was playing so we can resume it
        pausedClip = activeSource.clip;
        pausedTime = activeSource.time;
        wasPausedDuringPlaylist = isPlayingPlaylist;

        if (isPlayingPlaylist)
        {
            pausedPlaylist = gamePlaylist; // Store reference to playlist
            pausedPlaylistIndex = currentPlaylistIndex;
        }

        // Why: Stop playlist coroutine if running
        isPlayingPlaylist = false;
        StopAllCoroutines();

        // Why: Fade out current music quickly
        activeSource.DOKill(); // Kill any ongoing tweens
        activeSource.DOFade(0f, eventFadeDuration).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            activeSource.Pause(); // Pause instead of stop to keep position
        });
    }

    /// <summary>
    /// Plays special event music on dedicated event source
    /// This plays on top of the paused regular music
    /// </summary>
    public void PlayEventMusic(AudioClip eventMusic)
    {
        if (eventMusic == null)
        {
            Debug.Log("🎵 No event music provided");
            return;
        }

        Debug.Log($"🎵 Playing event music: {eventMusic.name}");

        // Why: Setup event music source
        eventMusicSource.clip = eventMusic;
        eventMusicSource.volume = 0f;
        eventMusicSource.loop = true;
        eventMusicSource.Play();

        // Why: Fade in event music
        eventMusicSource.DOFade(musicVolume, eventFadeDuration).SetEase(Ease.InQuad);
    }

    /// <summary>
    /// Resumes the music that was playing before the event
    /// Fades out event music and fades in regular music
    /// </summary>
    public void ResumeMusic()
    {
        Debug.Log("🎵 Resuming music after event...");

        // Why: Fade out event music if it's playing
        if (eventMusicSource.isPlaying)
        {
            eventMusicSource.DOKill();
            eventMusicSource.DOFade(0f, eventFadeDuration).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                eventMusicSource.Stop();
                eventMusicSource.clip = null;
            });
        }

        // Why: Resume regular music
        if (pausedClip != null)
        {
            // Resume from where we paused
            activeSource.time = pausedTime;
            activeSource.Play();

            activeSource.DOKill();
            activeSource.DOFade(musicVolume, eventFadeDuration).SetEase(Ease.InQuad);

            // Why: If we were in a playlist, resume it
            if (wasPausedDuringPlaylist && pausedPlaylist != null)
            {
                isPlayingPlaylist = true;
                currentPlaylistIndex = pausedPlaylistIndex;
                StartCoroutine(PlaylistCoroutine(pausedPlaylist));
            }
        }

        // Why: Clear paused state
        pausedClip = null;
        pausedTime = 0f;
        wasPausedDuringPlaylist = false;
        pausedPlaylist = null;
    }

    // ============================================
    // SFX SYSTEM (unchanged - always works)
    // ============================================

    public void PlaySFX(AudioClip clip)
    {
        // Why: Play a one-shot sound effect
        // SFX use separate AudioSource so they NEVER interrupt music
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