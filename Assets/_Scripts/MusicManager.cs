using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent music manager that plays background music across all scenes.
/// Uses DontDestroyOnLoad to survive scene transitions.
/// </summary>
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Music Tracks")]
    [Tooltip("Main background music that loops")]
    [SerializeField] private AudioClip mainTheme;
    
    [Tooltip("Optional: Tense music for high tension moments")]
    [SerializeField] private AudioClip tenseTheme;
    
    [Tooltip("Optional: Menu/intro music")]
    [SerializeField] private AudioClip menuTheme;

    [Header("Settings")]
    [SerializeField] private float musicVolume = 0.5f;
    [SerializeField] private float crossfadeDuration = 2f;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loopMusic = true;

    [Header("Tension-Based Music")]
    [Tooltip("Automatically switch to tense music at high tension")]
    [SerializeField] private bool autoSwitchOnTension = false;
    [SerializeField] private int tensionThresholdForTenseMusic = 70;

    private AudioSource musicSourceA;
    private AudioSource musicSourceB;
    private AudioSource activeSource;
    private bool isCrossfading;

    private void Awake()
    {
        // Singleton with persistence
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create two audio sources for crossfading
        musicSourceA = gameObject.AddComponent<AudioSource>();
        musicSourceA.playOnAwake = false;
        musicSourceA.loop = loopMusic;
        musicSourceA.volume = 0f;

        musicSourceB = gameObject.AddComponent<AudioSource>();
        musicSourceB.playOnAwake = false;
        musicSourceB.loop = loopMusic;
        musicSourceB.volume = 0f;

        activeSource = musicSourceA;
    }

    private void Start()
    {
        if (playOnStart && mainTheme != null)
        {
            PlayMusic(mainTheme);
        }

        // Subscribe to scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        // Unsubscribe from tension if we subscribed
        if (Interrogation.Dialogue.TensionMeter.Instance != null)
        {
            Interrogation.Dialogue.TensionMeter.Instance.OnTensionChanged -= HandleTensionChanged;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-subscribe to tension meter in new scene if it exists
        if (autoSwitchOnTension)
        {
            StartCoroutine(SubscribeToTensionDelayed());
        }
    }

    private System.Collections.IEnumerator SubscribeToTensionDelayed()
    {
        // Wait a frame for singletons to initialize
        yield return null;
        
        if (Interrogation.Dialogue.TensionMeter.Instance != null)
        {
            Interrogation.Dialogue.TensionMeter.Instance.OnTensionChanged -= HandleTensionChanged;
            Interrogation.Dialogue.TensionMeter.Instance.OnTensionChanged += HandleTensionChanged;
        }
    }

    private void HandleTensionChanged(int tension, int delta)
    {
        if (!autoSwitchOnTension || tenseTheme == null) return;

        if (tension >= tensionThresholdForTenseMusic && activeSource.clip != tenseTheme)
        {
            CrossfadeTo(tenseTheme);
        }
        else if (tension < tensionThresholdForTenseMusic && activeSource.clip != mainTheme)
        {
            CrossfadeTo(mainTheme);
        }
    }

    /// <summary>
    /// Play a music track immediately
    /// </summary>
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        activeSource.Stop();
        activeSource.clip = clip;
        activeSource.volume = musicVolume;
        activeSource.Play();
    }

    /// <summary>
    /// Crossfade to a new music track
    /// </summary>
    public void CrossfadeTo(AudioClip newClip)
    {
        if (newClip == null || isCrossfading) return;
        if (activeSource.clip == newClip) return;

        StartCoroutine(CrossfadeCoroutine(newClip));
    }

    private System.Collections.IEnumerator CrossfadeCoroutine(AudioClip newClip)
    {
        isCrossfading = true;

        AudioSource fadingOut = activeSource;
        AudioSource fadingIn = (activeSource == musicSourceA) ? musicSourceB : musicSourceA;

        fadingIn.clip = newClip;
        fadingIn.volume = 0f;
        fadingIn.Play();

        float elapsed = 0f;
        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / crossfadeDuration;

            fadingOut.volume = Mathf.Lerp(musicVolume, 0f, t);
            fadingIn.volume = Mathf.Lerp(0f, musicVolume, t);

            yield return null;
        }

        fadingOut.Stop();
        fadingOut.volume = 0f;
        fadingIn.volume = musicVolume;

        activeSource = fadingIn;
        isCrossfading = false;
    }

    /// <summary>
    /// Set the music volume (0-1)
    /// </summary>
    public void SetVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (activeSource != null && activeSource.isPlaying)
        {
            activeSource.volume = musicVolume;
        }
    }

    /// <summary>
    /// Pause the music
    /// </summary>
    public void PauseMusic()
    {
        activeSource?.Pause();
    }

    /// <summary>
    /// Resume the music
    /// </summary>
    public void ResumeMusic()
    {
        activeSource?.UnPause();
    }

    /// <summary>
    /// Stop the music
    /// </summary>
    public void StopMusic()
    {
        musicSourceA?.Stop();
        musicSourceB?.Stop();
    }

    /// <summary>
    /// Play main theme
    /// </summary>
    public void PlayMainTheme()
    {
        if (mainTheme != null)
            CrossfadeTo(mainTheme);
    }

    /// <summary>
    /// Play tense theme
    /// </summary>
    public void PlayTenseTheme()
    {
        if (tenseTheme != null)
            CrossfadeTo(tenseTheme);
    }

    /// <summary>
    /// Play menu theme
    /// </summary>
    public void PlayMenuTheme()
    {
        if (menuTheme != null)
            CrossfadeTo(menuTheme);
    }
}
