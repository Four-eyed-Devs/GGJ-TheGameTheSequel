using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component for a single mask button in the player's hand.
/// Displays mask info and handles selection.
/// </summary>
[RequireComponent(typeof(Button))]
public class MaskButton : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The CardData this button represents")]
    public CardData cardData;
    
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI durabilityText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image maskIcon;
    
    [Header("Visual States")]
    [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
    [SerializeField] private Color brokenColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
    [SerializeField] private Color logicColor = new Color(0.3f, 0.5f, 0.8f);
    [SerializeField] private Color emotionColor = new Color(0.8f, 0.4f, 0.6f);
    [SerializeField] private Color aggressionColor = new Color(0.8f, 0.3f, 0.3f);
    [SerializeField] private Color charmColor = new Color(0.8f, 0.7f, 0.3f);
    
    private Button button;
    private int currentDurability;
    
    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);
    }
    
    private void Start()
    {
        if (cardData != null)
        {
            Initialize(cardData);
        }
        
        // Subscribe to durability changes
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMaskDurabilityChanged += HandleDurabilityChanged;
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }
    }
    
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMaskDurabilityChanged -= HandleDurabilityChanged;
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }
    
    /// <summary>
    /// Sets up the button with card data.
    /// </summary>
    public void Initialize(CardData data)
    {
        cardData = data;
        
        if (nameText != null)
        {
            nameText.text = data.maskName;
        }
        
        // Get initial durability from GameManager if available
        if (GameManager.Instance != null)
        {
            currentDurability = GameManager.Instance.GetMaskDurability(data.maskType);
        }
        else
        {
            currentDurability = data.maxDurability;
        }
        
        UpdateDurabilityDisplay();
        ApplyTypeColor();
    }
    
    /// <summary>
    /// Updates the durability text and visual state.
    /// </summary>
    private void UpdateDurabilityDisplay()
    {
        if (durabilityText != null)
        {
            durabilityText.text = $"{currentDurability}/{cardData.maxDurability}";
        }
        
        bool isBroken = currentDurability <= 0;
        button.interactable = !isBroken;
        
        if (backgroundImage != null)
        {
            backgroundImage.color = isBroken ? brokenColor : normalColor;
        }
        
        // Fade text when broken
        if (nameText != null)
        {
            nameText.alpha = isBroken ? 0.5f : 1f;
        }
    }
    
    /// <summary>
    /// Applies color coding based on mask type.
    /// </summary>
    private void ApplyTypeColor()
    {
        if (maskIcon == null) return;
        
        Color typeColor = cardData.maskType switch
        {
            MaskType.Logic => logicColor,
            MaskType.Emotion => emotionColor,
            MaskType.Aggression => aggressionColor,
            MaskType.Charm => charmColor,
            _ => Color.white
        };
        
        maskIcon.color = typeColor;
    }
    
    /// <summary>
    /// Handles button click - notifies GameManager.
    /// </summary>
    private void OnButtonClicked()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[MaskButton] GameManager not found!");
            return;
        }
        
        if (currentDurability <= 0)
        {
            Debug.LogWarning($"[MaskButton] {cardData.maskName} is broken!");
            return;
        }
        
        Debug.Log($"[MaskButton] Selected: {cardData.maskName}");
        GameManager.Instance.SelectMask(cardData);
    }
    
    /// <summary>
    /// Responds to durability changes from GameManager.
    /// </summary>
    private void HandleDurabilityChanged(MaskType type, int newDurability)
    {
        if (cardData != null && cardData.maskType == type)
        {
            currentDurability = newDurability;
            UpdateDurabilityDisplay();
            
            if (newDurability <= 0)
            {
                PlayBreakAnimation();
            }
        }
    }
    
    /// <summary>
    /// Responds to game state changes.
    /// </summary>
    private void HandleGameStateChanged(GameManager.GameState newState)
    {
        // Only allow clicking during input phase
        bool canInteract = newState == GameManager.GameState.WaitingForInput && currentDurability > 0;
        button.interactable = canInteract;
    }
    
    /// <summary>
    /// Visual feedback when mask breaks.
    /// </summary>
    private void PlayBreakAnimation()
    {
        // Simple scale punch - could be expanded with DOTween or Animation
        StartCoroutine(BreakPunchCoroutine());
    }
    
    private System.Collections.IEnumerator BreakPunchCoroutine()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.2f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = originalScale * 0.9f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = originalScale;
    }
}
