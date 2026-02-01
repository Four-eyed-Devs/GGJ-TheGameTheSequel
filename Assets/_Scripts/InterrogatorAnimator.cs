using UnityEngine;
using Interrogation.Dialogue;

/// <summary>
/// Controls the interrogator character's animations based on tension and dialogue events.
/// Attach this to the interrogator character GameObject that has an Animator component.
/// </summary>
[RequireComponent(typeof(Animator))]
public class InterrogatorAnimator : MonoBehaviour
{
    [Header("Animation Parameters")]
    [Tooltip("Parameter name for stressed/relaxed state (Bool)")]
    [SerializeField] private string stressedParam = "IsStressed";
    
    [Tooltip("Parameter name for table slam trigger")]
    [SerializeField] private string tableSlamTrigger = "TableSlam";

    [SerializeField] private string tableHumpTrigger = "TableHump";
    
    [Header("Tension Thresholds")]
    [Tooltip("Tension value above which interrogator appears stressed")]
    [SerializeField] private int stressedThreshold = 60;

    [SerializeField] private int veryStressedTreshold = 90;
    
    [Tooltip("Trigger table slam when tension increases by this much in one change")]
    [SerializeField] private int slamOnTensionSpike = 10;
    
    [Header("Settings")]
    [Tooltip("Chance to play table slam on wrong mask selection (0-1)")]
    [Range(0f, 1f)]
    [SerializeField] private float tableSlamChance = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float tableHumpChance = 0.2f;


    private Animator animator;
    
    // Cache parameter hashes for performance
    private int stressedHash;
    private int verStressedHash;
    private int tableSlamHash;
    private int tableHumpHash;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        
        // Cache parameter hashes
        stressedHash = Animator.StringToHash(stressedParam);
        veryStressedTreshold = Animator.StringToHash(veryStressedParam);
        tableSlamHash = Animator.StringToHash(tableSlamTrigger);
        tableHumpHash = Animator.StringToHash(tableHumpTrigger)
    }
    
    private void Start()
    {
        // Subscribe to tension changes
        if (TensionMeter.Instance != null)
        {
                TensionMeter.Instance.OnTensionChanged += HandleTensionChanged;
                
                // Set initial state
                UpdateStressedState(TensionMeter.Instance.CurrentTension);
        }
        else
        {
            Debug.LogWarning("InterrogatorAnimator: TensionMeter not found. Animation will not respond to tension.");
        }
        
        // Subscribe to dialogue events if DialogueManager exists
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnWrongMaskSelected += HandleWrongMaskSelected;
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (TensionMeter.Instance != null)
        {
            TensionMeter.Instance.OnTensionChanged -= HandleTensionChanged;
        }
        
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnWrongMaskSelected -= HandleWrongMaskSelected;
        }
    }
    
    /// <summary>
    /// Called when tension value changes.
    /// </summary>
    private void HandleTensionChanged(int newTension, int delta)
    {
        UpdateStressedState(newTension);
        
        // Check for tension spike (wrong answer increases tension)
        if (delta >= slamOnTensionSpike)
        {
            // Tension spiked - consider playing table slam
            if (Random.value < tableHumpChance)
            {
                PlayTableHump();
            }
            else if(Random.Value < tableSlamChance)
            {
                PlayTableSlam();
            }
        }
    }
    
    /// <summary>
    /// Called when player selects a wrong mask.
    /// </summary>
    private void HandleWrongMaskSelected()
    {
        // Random chance to slam table on wrong answer
        if (Random.value < tableSlamChance)
        {
            PlayTableSlam();
        }
    }
    
    /// <summary>
    /// Updates the stressed/relaxed animation state based on tension.
    /// </summary>
    private void UpdateStressedState(int tension)
    {
        bool isStressed = tension >= stressedThreshold;
        animator.SetBool(stressedHash, isStressed);
    }
    
    /// <summary>
    /// Triggers the table slam animation.
    /// </summary>
    public void PlayTableSlam()
    {
        animator.SetTrigger(tableSlamHash);
    }

    public void PlayTableHump()
    {
        animator.SetTrigger(tableHumpHash);
    }

    /// <summary>
    /// Force set stressed state (useful for cutscenes/scripted moments).
    /// </summary>
    public void SetStressed(bool stressed)
    {
        animator.SetBool(stressedHash, stressed);
    }
    
    /// <summary>
    /// Play a specific animation state by name.
    /// </summary>
    public void PlayState(string stateName)
    {
        animator.Play(stateName);
    }
}
