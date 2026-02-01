using UnityEngine;
using Interrogation.Dialogue;

/// <summary>
/// Controls the BlackMan character animations based on TensionMeter.
/// Animation states:
/// - Relaxed on chair: tension < 50
/// - Stressed on chair: tension >= 50 (default state)
/// - Over the table idle: tension > 80
/// - Table Slam / Table Hump: triggered randomly during high tension
/// </summary>
public class BlackManAnimator : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Animator component on the BlackMan character. Uses BlackManController2.")]
    [SerializeField] private Animator animator;

    [Header("Tension Thresholds")]
    [Tooltip("Below this tension, character is relaxed")]
    [SerializeField] private int relaxedThreshold = 50;
    [Tooltip("Above this tension, character goes over the table")]
    [SerializeField] private int overTableThreshold = 80;

    [Header("Random Table Actions")]
    [Tooltip("Minimum time between random table slam/hump actions")]
    [SerializeField] private float minActionInterval = 5f;
    [Tooltip("Maximum time between random table slam/hump actions")]
    [SerializeField] private float maxActionInterval = 15f;
    [Tooltip("Chance (0-1) to trigger table action when conditions are met")]
    [SerializeField] private float tableActionChance = 0.3f;
    [Tooltip("Enable random table slam/hump actions")]
    [SerializeField] private bool enableRandomActions = true;

    // Animator parameter names (must match BlackManController2)
    private static readonly int StressedMeterParam = Animator.StringToHash("StressedMeter");
    private static readonly int TableSlamTrigger = Animator.StringToHash("TableSlam");
    private static readonly int TableHumpTrigger = Animator.StringToHash("TableHump");

    private float nextActionTime;
    private int lastTension = -1;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError("[BlackManAnimator] No Animator found on " + gameObject.name);
            enabled = false;
            return;
        }

        ScheduleNextAction();
    }

    private void Start()
    {
        // Set initial state
        UpdateAnimatorState();
    }

    private void Update()
    {
        UpdateAnimatorState();

        // Check for random table actions
        if (enableRandomActions && Time.time >= nextActionTime)
        {
            TryRandomTableAction();
            ScheduleNextAction();
        }
    }

    /// <summary>
    /// Updates the animator based on current tension level
    /// </summary>
    private void UpdateAnimatorState()
    {
        int tension = GetCurrentTension();

        // Only update if tension changed
        if (tension == lastTension) return;
        lastTension = tension;

        // Set the StressedMeter float parameter - the animator controller handles transitions
        animator.SetFloat(StressedMeterParam, tension);

        Debug.Log($"[BlackManAnimator] Tension updated to {tension}");
    }

    /// <summary>
    /// Gets the current tension from TensionMeter
    /// </summary>
    private int GetCurrentTension()
    {
        if (TensionMeter.Instance != null)
        {
            return TensionMeter.Instance.CurrentTension;
        }
        return 50; // Default to stressed state
    }

    /// <summary>
    /// Tries to trigger a random table action (slam or hump) based on chance
    /// </summary>
    private void TryRandomTableAction()
    {
        int tension = GetCurrentTension();

        // Only trigger table actions when tension is high enough
        if (tension < overTableThreshold) return;

        // Check random chance
        if (Random.value > tableActionChance) return;

        // Randomly choose between table slam and table hump
        if (Random.value > 0.5f)
        {
            TriggerTableSlam();
        }
        else
        {
            TriggerTableHump();
        }
    }

    /// <summary>
    /// Schedule the next random action check
    /// </summary>
    private void ScheduleNextAction()
    {
        nextActionTime = Time.time + Random.Range(minActionInterval, maxActionInterval);
    }

    /// <summary>
    /// Manually trigger the Table Slam animation
    /// </summary>
    public void TriggerTableSlam()
    {
        if (animator != null)
        {
            animator.SetTrigger(TableSlamTrigger);
            Debug.Log("[BlackManAnimator] Triggered Table Slam");
        }
    }

    /// <summary>
    /// Manually trigger the Table Hump animation
    /// </summary>
    public void TriggerTableHump()
    {
        if (animator != null)
        {
            animator.SetTrigger(TableHumpTrigger);
            Debug.Log("[BlackManAnimator] Triggered Table Hump");
        }
    }

    /// <summary>
    /// Force update the animation state (useful after scene loads)
    /// </summary>
    public void ForceUpdate()
    {
        lastTension = -1;
        UpdateAnimatorState();
    }
}
