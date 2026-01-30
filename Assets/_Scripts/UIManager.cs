using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Manages all UI elements for the interrogation scene.
/// Subscribes to GameManager events and updates displays.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private TextMeshProUGUI detectiveDialogueText;
    [SerializeField] private TextMeshProUGUI innerMonologueText;
    
    [Header("Suspicion Meter")]
    [SerializeField] private Image suspicionFill;
    [SerializeField] private TextMeshProUGUI suspicionPercentText;
    [SerializeField] private Gradient suspicionGradient;
    
    [Header("Mask Buttons")]
    [SerializeField] private Transform maskButtonContainer;
    [SerializeField] private MaskButton[] maskButtons;
    
    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Button restartButton;
    
    [Header("Animation Settings")]
    [SerializeField] private float typewriterSpeed = 0.03f;
    [SerializeField] private float suspicionAnimDuration = 0.5f;
    
    private Coroutine typewriterCoroutine;
    private float targetSuspicionFill;
    
    private void Start()
    {
        // Subscribe to GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDialogueChanged += HandleDialogueChanged;
            GameManager.Instance.OnSuspicionChanged += HandleSuspicionChanged;
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            GameManager.Instance.OnGameOver += HandleGameOver;
            
            // Initialize displays
            InitializeUI();
        }
        else
        {
            Debug.LogError("[UIManager] GameManager not found!");
        }
        
        // Setup restart button
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }
        
        // Hide game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
    
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDialogueChanged -= HandleDialogueChanged;
            GameManager.Instance.OnSuspicionChanged -= HandleSuspicionChanged;
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            GameManager.Instance.OnGameOver -= HandleGameOver;
        }
    }
    
    /// <summary>
    /// Sets up initial UI state.
    /// </summary>
    private void InitializeUI()
    {
        // Initialize suspicion meter
        UpdateSuspicionDisplay(GameManager.Instance.SuspicionMeter, animate: false);
        
        // Refresh mask buttons (only if they don't already have CardData assigned)
        if (maskButtons != null)
        {
            for (int i = 0; i < maskButtons.Length; i++)
            {
                if (maskButtons[i] != null && maskButtons[i].cardData != null)
                {
                    // Re-initialize to refresh durability display from GameManager
                    maskButtons[i].Initialize(maskButtons[i].cardData);
                }
            }
        }
        
        // Clear dialogue initially
        if (detectiveDialogueText != null)
        {
            detectiveDialogueText.text = "";
        }
        if (innerMonologueText != null)
        {
            innerMonologueText.text = "";
        }
    }
    
    /// <summary>
    /// Handles new dialogue node presentation.
    /// </summary>
    private void HandleDialogueChanged(DialogueNode node)
    {
        if (node == null) return;
        
        // Typewriter effect for detective dialogue
        if (detectiveDialogueText != null)
        {
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
            }
            typewriterCoroutine = StartCoroutine(TypewriterEffect(detectiveDialogueText, node.dialogueText));
        }
        
        // Set inner monologue hint (if available)
        if (innerMonologueText != null)
        {
            innerMonologueText.text = string.IsNullOrEmpty(node.innerMonologueHint) 
                ? "" 
                : $"<i>{node.innerMonologueHint}</i>";
        }
    }
    
    /// <summary>
    /// Typewriter effect for dialogue text.
    /// </summary>
    private IEnumerator TypewriterEffect(TextMeshProUGUI textComponent, string fullText)
    {
        textComponent.text = "";
        foreach (char c in fullText)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }
    
    /// <summary>
    /// Handles suspicion meter changes.
    /// </summary>
    private void HandleSuspicionChanged(int newSuspicion)
    {
        UpdateSuspicionDisplay(newSuspicion, animate: true);
    }
    
    /// <summary>
    /// Updates the suspicion meter visual.
    /// </summary>
    private void UpdateSuspicionDisplay(int suspicion, bool animate)
    {
        float normalizedValue = suspicion / 100f;
        
        if (suspicionPercentText != null)
        {
            suspicionPercentText.text = $"{suspicion}%";
        }
        
        if (suspicionFill != null)
        {
            if (animate)
            {
                StartCoroutine(AnimateSuspicionFill(normalizedValue));
            }
            else
            {
                suspicionFill.fillAmount = normalizedValue;
                if (suspicionGradient != null)
                {
                    suspicionFill.color = suspicionGradient.Evaluate(normalizedValue);
                }
            }
        }
    }
    
    /// <summary>
    /// Animates suspicion bar fill.
    /// </summary>
    private IEnumerator AnimateSuspicionFill(float targetValue)
    {
        float startValue = suspicionFill.fillAmount;
        float elapsed = 0f;
        
        while (elapsed < suspicionAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / suspicionAnimDuration;
            float easedT = 1f - Mathf.Pow(1f - t, 3f); // Ease out cubic
            
            float currentFill = Mathf.Lerp(startValue, targetValue, easedT);
            suspicionFill.fillAmount = currentFill;
            
            if (suspicionGradient != null)
            {
                suspicionFill.color = suspicionGradient.Evaluate(currentFill);
            }
            
            yield return null;
        }
        
        suspicionFill.fillAmount = targetValue;
    }
    
    /// <summary>
    /// Handles game state transitions.
    /// </summary>
    private void HandleGameStateChanged(GameManager.GameState newState)
    {
        // Update button interactability based on state
        bool buttonsActive = newState == GameManager.GameState.WaitingForInput;
        
        if (maskButtonContainer != null)
        {
            // Visual indicator that input is expected
            // Could add glow, pulse, etc.
        }
    }
    
    /// <summary>
    /// Handles game over display.
    /// </summary>
    private void HandleGameOver(bool survived)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        if (gameOverText != null)
        {
            if (survived)
            {
                gameOverText.text = "YOU SURVIVED THE INTERROGATION\n\n<size=60%>Your masks held. Your secrets remain your own.</size>";
                gameOverText.color = new Color(0.5f, 0.8f, 0.5f);
            }
            else
            {
                gameOverText.text = "CONFESSION\n\n<size=60%>The masks have shattered. The truth spills out.</size>";
                gameOverText.color = new Color(0.8f, 0.3f, 0.3f);
            }
        }
    }
    
    /// <summary>
    /// Handles restart button click.
    /// </summary>
    private void OnRestartClicked()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        GameManager.Instance?.StartNewGame();
        InitializeUI();
    }
    
    /// <summary>
    /// Sets a custom inner monologue message (for resolution feedback).
    /// </summary>
    public void SetInnerMonologue(string message)
    {
        if (innerMonologueText != null)
        {
            innerMonologueText.text = $"<i>{message}</i>";
        }
    }
    
    /// <summary>
    /// Flashes the suspicion meter for impact feedback.
    /// </summary>
    public void FlashSuspicionMeter()
    {
        StartCoroutine(FlashCoroutine());
    }
    
    private IEnumerator FlashCoroutine()
    {
        if (suspicionFill == null) yield break;
        
        Color originalColor = suspicionFill.color;
        suspicionFill.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        suspicionFill.color = originalColor;
    }
}
