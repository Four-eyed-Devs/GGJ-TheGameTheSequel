using UnityEngine;

/// <summary>
/// Manages sound effects for the game.
/// Handles mask interactions, UI sounds, and general SFX.
/// </summary>
public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource sfxSource;
    
    [Header("Mask Sounds")]
    [Tooltip("Sound when hovering over a mask")]
    [SerializeField] private AudioClip maskHover;
    
    [Tooltip("Sound when picking up/clicking a mask (first click)")]
    [SerializeField] private AudioClip maskPickup;
    
    [Tooltip("Sound when confirming mask selection (second click)")]
    [SerializeField] private AudioClip maskSelect;
    
    [Tooltip("Sound when deselecting a mask")]
    [SerializeField] private AudioClip maskDeselect;
    
    [Tooltip("Sound when a mask breaks/is depleted")]
    [SerializeField] private AudioClip maskBreak;

    [Header("UI Sounds")]
    [Tooltip("Generic button click sound")]
    [SerializeField] private AudioClip buttonClick;
    
    [Tooltip("Sound for correct choice")]
    [SerializeField] private AudioClip correctChoice;
    
    [Tooltip("Sound for wrong choice")]
    [SerializeField] private AudioClip wrongChoice;

    [Header("Tension Sounds")]
    [Tooltip("Sound when tension increases")]
    [SerializeField] private AudioClip tensionUp;
    
    [Tooltip("Sound when tension decreases")]
    [SerializeField] private AudioClip tensionDown;

    [Header("Ambient/Other")]
    [Tooltip("Table slam sound effect")]
    [SerializeField] private AudioClip tableSlam;

    [Header("Settings")]
    [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private bool enableSFX = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Create audio source if not assigned
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
    }

    private void Start()
    {
        // Subscribe to dialogue events for automatic SFX
        if (Interrogation.Dialogue.DialogueManager.Instance != null)
        {
            Interrogation.Dialogue.DialogueManager.Instance.OnCorrectMaskSelected += PlayCorrectChoice;
            Interrogation.Dialogue.DialogueManager.Instance.OnWrongMaskSelected += PlayWrongChoice;
        }

        // Subscribe to tension changes
        if (Interrogation.Dialogue.TensionMeter.Instance != null)
        {
            Interrogation.Dialogue.TensionMeter.Instance.OnTensionChanged += HandleTensionChanged;
        }
    }

    private void OnDestroy()
    {
        if (Interrogation.Dialogue.DialogueManager.Instance != null)
        {
            Interrogation.Dialogue.DialogueManager.Instance.OnCorrectMaskSelected -= PlayCorrectChoice;
            Interrogation.Dialogue.DialogueManager.Instance.OnWrongMaskSelected -= PlayWrongChoice;
        }

        if (Interrogation.Dialogue.TensionMeter.Instance != null)
        {
            Interrogation.Dialogue.TensionMeter.Instance.OnTensionChanged -= HandleTensionChanged;
        }
    }

    private void HandleTensionChanged(int newTension, int delta)
    {
        if (delta > 0 && tensionUp != null)
        {
            PlaySFX(tensionUp);
        }
        else if (delta < 0 && tensionDown != null)
        {
            PlaySFX(tensionDown);
        }
    }

    // === General Play Methods ===

    /// <summary>
    /// Play a sound effect
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (!enableSFX || clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume * volumeMultiplier);
    }

    /// <summary>
    /// Play a sound effect with pitch variation
    /// </summary>
    public void PlaySFXWithPitch(AudioClip clip, float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        if (!enableSFX || clip == null || sfxSource == null) return;
        
        float originalPitch = sfxSource.pitch;
        sfxSource.pitch = Random.Range(minPitch, maxPitch);
        sfxSource.PlayOneShot(clip, sfxVolume);
        sfxSource.pitch = originalPitch;
    }

    // === Mask Sounds ===

    public void PlayMaskHover()
    {
        PlaySFX(maskHover, 0.5f); // Quieter for hover
    }

    public void PlayMaskPickup()
    {
        PlaySFX(maskPickup);
    }

    public void PlayMaskSelect()
    {
        PlaySFX(maskSelect);
    }

    public void PlayMaskDeselect()
    {
        PlaySFX(maskDeselect);
    }

    public void PlayMaskBreak()
    {
        PlaySFX(maskBreak);
    }

    // === UI Sounds ===

    public void PlayButtonClick()
    {
        PlaySFX(buttonClick);
    }

    public void PlayCorrectChoice()
    {
        PlaySFX(correctChoice);
    }

    public void PlayWrongChoice()
    {
        PlaySFX(wrongChoice);
    }

    // === Other Sounds ===

    public void PlayTableSlam()
    {
        PlaySFX(tableSlam);
    }

    // === Volume Control ===

    public void SetVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    public void SetEnabled(bool enabled)
    {
        enableSFX = enabled;
    }
}
