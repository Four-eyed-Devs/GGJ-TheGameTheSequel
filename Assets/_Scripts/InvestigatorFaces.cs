using UnityEngine;
using Interrogation.Dialogue;

[ExecuteAlways] // Makes this run in the Editor and Play mode
public class ManualBaseMapOffset : MonoBehaviour
{
    [Header("Assign your Material here")]
    public Material material; // Drag your material here

    [Header("Texture Offset (U, V)")]
    public Vector2 offset;    // Editable in Inspector

    [Header("Tension-Based Expression")]
    [Tooltip("Enable automatic Y offset based on tension")]
    public bool useTensionBasedOffset = true;
    
    [Tooltip("How fast the offset changes (higher = faster)")]
    public float transitionSpeed = 8f;

    [Header("Y Offset Values")]
    [Tooltip("Default/neutral expression (tension 41-59)")]
    public float neutralY = -0.16f;
    [Tooltip("Expression when tension >= 60")]
    public float at60Y = 0f;
    [Tooltip("Expression when tension >= 70")]
    public float at70Y = 0.16f;
    [Tooltip("Expression when tension >= 80")]
    public float at80Y = 0.32f;
    [Tooltip("Expression when tension <= 40")]
    public float at40Y = 0f;
    [Tooltip("Expression when tension <= 30")]
    public float at30Y = 0.48f;
    [Tooltip("Expression when tension <= 20")]
    public float at20Y = 0.63f;

    private float targetY = -0.16f;
    private float currentY = -0.16f;

    void Start()
    {
        currentY = neutralY;
        targetY = neutralY;
        offset.y = neutralY;

        // Subscribe to tension changes
        if (Application.isPlaying && TensionMeter.Instance != null)
        {
            TensionMeter.Instance.OnTensionChanged += OnTensionChanged;
            // Initialize based on current tension
            UpdateTargetFromTension(TensionMeter.Instance.CurrentTension);
        }
    }

    void OnDestroy()
    {
        if (TensionMeter.Instance != null)
        {
            TensionMeter.Instance.OnTensionChanged -= OnTensionChanged;
        }
    }

    private void OnTensionChanged(int tension, int delta)
    {
        if (useTensionBasedOffset)
        {
            UpdateTargetFromTension(tension);
        }
    }

    private void UpdateTargetFromTension(int tension)
    {
        // Determine target Y based on tension thresholds
        // High tension (composed) - check from highest first
        if (tension >= 80)
        {
            targetY = at80Y;
        }
        else if (tension >= 70)
        {
            targetY = at70Y;
        }
        else if (tension >= 60)
        {
            targetY = at60Y;
        }
        // Low tension (stressed) - check from lowest first
        else if (tension <= 20)
        {
            targetY = at20Y;
        }
        else if (tension <= 30)
        {
            targetY = at30Y;
        }
        else if (tension <= 40)
        {
            targetY = at40Y;
        }
        else
        {
            // Neutral zone (41-59)
            targetY = neutralY;
        }

        Debug.Log($"[ManualBaseMapOffset] Tension: {tension} -> Target Y: {targetY}");
    }

    void Update()
    {
        if (material == null) return;

        if (Application.isPlaying && useTensionBasedOffset)
        {
            // Rapidly interpolate towards target
            if (Mathf.Abs(currentY - targetY) > 0.001f)
            {
                currentY = Mathf.Lerp(currentY, targetY, Time.deltaTime * transitionSpeed);
                offset.y = currentY;
            }
            else
            {
                currentY = targetY;
                offset.y = targetY;
            }
        }

        material.SetTextureOffset("_BaseMap", offset);
    }

    /// <summary>
    /// Manually set the expression (for testing or cutscenes)
    /// </summary>
    public void SetExpression(float yOffset)
    {
        targetY = yOffset;
    }

    /// <summary>
    /// Force immediate expression change (no lerp)
    /// </summary>
    public void SetExpressionImmediate(float yOffset)
    {
        targetY = yOffset;
        currentY = yOffset;
        offset.y = yOffset;
    }
}