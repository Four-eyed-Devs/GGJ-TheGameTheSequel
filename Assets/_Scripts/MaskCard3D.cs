using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Unity.VisualScripting;
using Interrogation.Dialogue;

/// <summary>
/// 3D interactable mask card that sits on the interrogation table.
/// Requires a Collider component for mouse/raycast detection.
/// Integrates with DialogueManager for interrogation flow.
/// </summary>
[RequireComponent(typeof(Collider))]
public class MaskCard3D : MonoBehaviour
{
    [Header("Card Data")]
    public CardData cardData;
    
    [Header("New Dialogue System")]
    [Tooltip("The mask type for the new dialogue system")]
    [SerializeField] private Interrogation.Dialogue.MaskType dialogueMaskType;
    
    [Header("Visual References")]
    [Tooltip("TextMeshPro for displaying mask name (optional)")]
    [SerializeField] private TextMeshPro nameText;
    
    [Tooltip("TextMeshPro for displaying durability (optional)")]
    [SerializeField] private TextMeshPro durabilityText;
    
    [Tooltip("Renderer for visual feedback")]
    [SerializeField] private Renderer cardRenderer;
    
    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.25f);
    [SerializeField] private Color hoverColor = new Color(0.3f, 0.3f, 0.4f);
    [SerializeField] private Color disabledColor = new Color(0.1f, 0.1f, 0.1f);
    [SerializeField] private Color selectedColor = new Color(0.4f, 0.5f, 0.6f);
    
    [Header("Animation")]
    [SerializeField] private float hoverLiftHeight = 0.1f;
    [SerializeField] private float animationSpeed = 8f;
    [SerializeField] private float clickRotation = 60f;
    [SerializeField] private float clickLift = 0.15f;
    
    [Header("Type Colors (for accent)")]
    [SerializeField] private Color logicAccent = new Color(0.3f, 0.5f, 0.8f);
    [SerializeField] private Color emotionAccent = new Color(0.8f, 0.4f, 0.6f);
    [SerializeField] private Color aggressionAccent = new Color(0.8f, 0.3f, 0.3f);
    [SerializeField] private Color charmAccent = new Color(0.8f, 0.7f, 0.3f);

    [Header("Particle Effect")]
    [SerializeField] private GameObject particleEffect;
    
    // Runtime state
    private int currentDurability;
    private int usesRemaining = 2; // New dialogue system: max 2 uses per mask
    private bool isHovered = false;
    private bool isSelected = false;
    private bool isInteractable = true;
    private bool isDepleted = false; // For new dialogue system
    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private Quaternion originalRotation;
    private Quaternion targetRotation;
    private Material cardMaterial;
    private bool isSubscribed = false;
    private bool isDialogueSubscribed = false;
    private bool firstCLick = true;
    
    // Properties
    public Interrogation.Dialogue.MaskType DialogueMaskType => dialogueMaskType;
    public int UsesRemaining => usesRemaining;
    public bool IsDepleted => isDepleted;
    
    private void Awake()
    {
        originalPosition = transform.localPosition;
        targetPosition = originalPosition;
        originalRotation = transform.localRotation;
        targetRotation = originalRotation;

        // Get or create material instance
        if (cardRenderer != null)
        {
            cardMaterial = cardRenderer.material; // Creates instance
        }
    }
    
    private void Start()
    {
        StartCoroutine(DelayedInitialize());
    }
    
    private System.Collections.IEnumerator DelayedInitialize()
    {
        yield return null; // Wait one frame for managers to initialize
        
        if (cardData != null)
        {
            Initialize(cardData);
            
            // Auto-assign dialogue mask type from CardData if not set
            if (cardData.maskType == global::MaskType.Logic)
                dialogueMaskType = Interrogation.Dialogue.MaskType.Logic;
            else if (cardData.maskType == global::MaskType.Emotion)
                dialogueMaskType = Interrogation.Dialogue.MaskType.Emotion;
            else if (cardData.maskType == global::MaskType.Aggression)
                dialogueMaskType = Interrogation.Dialogue.MaskType.Aggression;
            else if (cardData.maskType == global::MaskType.Charm)
                dialogueMaskType = Interrogation.Dialogue.MaskType.Charm;
        }
        
        // Subscribe to new DialogueManager first (preferred system)
        SubscribeToDialogueEvents();
        
        // Only subscribe to old GameManager if DialogueManager is not present
        SubscribeToEvents();
        
        // Initialize interactable state based on which system is active
        if (DialogueManager.Instance != null)
        {
            // New system: cards start non-interactable, enabled when mask selection is requested
            isInteractable = false;
            isDepleted = false;
            usesRemaining = 2;
            Debug.Log($"[MaskCard3D] {cardData?.maskName} initialized for new DialogueManager system");
        }
        else if (GameManager.Instance != null)
        {
            // Old system: use GameManager state
            HandleGameStateChanged(GameManager.Instance.CurrentState);
        }
    }
    
    private void SubscribeToEvents()
    {
        // Skip old GameManager subscription if new DialogueManager is active
        if (DialogueManager.Instance != null)
        {
            Debug.Log($"[MaskCard3D] {(cardData != null ? cardData.maskName : name)} using new DialogueManager - skipping old GameManager subscription");
            return;
        }
        
        if (isSubscribed || GameManager.Instance == null) return;
        
        GameManager.Instance.OnMaskDurabilityChanged += HandleDurabilityChanged;
        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        isSubscribed = true;
        Debug.Log($"[MaskCard3D] {(cardData != null ? cardData.maskName : name)} subscribed to GameManager");
    }
    
    private void SubscribeToDialogueEvents()
    {
        if (isDialogueSubscribed || DialogueManager.Instance == null) return;
        
        DialogueManager.Instance.OnMaskSelectionEnabled += HandleMaskSelectionEnabled;
        DialogueManager.Instance.OnMaskSelectionDisabled += HandleMaskSelectionDisabled;
        isDialogueSubscribed = true;
        Debug.Log($"[MaskCard3D] {(cardData != null ? cardData.maskName : name)} subscribed to DialogueManager");
    }
    
    private void OnDestroy()
    {
        if (isSubscribed && GameManager.Instance != null)
        {
            GameManager.Instance.OnMaskDurabilityChanged -= HandleDurabilityChanged;
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
        
        if (isDialogueSubscribed && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnMaskSelectionEnabled -= HandleMaskSelectionEnabled;
            DialogueManager.Instance.OnMaskSelectionDisabled -= HandleMaskSelectionDisabled;
        }
        
        // Clean up material instance
        if (cardMaterial != null)
        {
            Destroy(cardMaterial);
        }
    }
    
    private void Update()
    {
        // Smooth position animation
        if (transform.localPosition != targetPosition)
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition, 
                targetPosition, 
                Time.deltaTime * animationSpeed
            );
        }

        if (transform.localRotation != targetRotation)
        {
            transform.localRotation = Quaternion.Lerp(
                transform.localRotation,
                targetRotation,
                Time.deltaTime * animationSpeed
            );
        }
    }
    
    /// <summary>
    /// Initialize card with data.
    /// </summary>
    public void Initialize(CardData data)
    {
        cardData = data;
        
        if (nameText != null)
        {
            nameText.text = data.maskName;
        }
        
        // Get durability from GameManager or default to max
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
    
    private void UpdateDurabilityDisplay()
    {
        if (durabilityText != null)
        {
            // Check if using new dialogue system
            if (DialogueManager.Instance != null)
            {
                int remaining = DialogueManager.Instance.GetMaskRemainingUses(dialogueMaskType);
                durabilityText.text = $"{remaining}/2";
                
                bool isBroken = remaining <= 0;
                if (isBroken)
                {
                    SetCardColor(disabledColor);
                    isInteractable = false;
                    isDepleted = true;
                }
            }
            else if (cardData != null)
            {
                // Legacy system
                durabilityText.text = $"{currentDurability}/{cardData.maxDurability}";
                
                bool isBroken = currentDurability <= 0;
                if (isBroken)
                {
                    SetCardColor(disabledColor);
                    isInteractable = false;
                }
            }
        }
    }
    
    private void ApplyTypeColor()
    {
        if (cardData == null) return;
        
        Color accent = cardData.maskType switch
        {
            MaskType.Logic => logicAccent,
            MaskType.Emotion => emotionAccent,
            MaskType.Aggression => aggressionAccent,
            MaskType.Charm => charmAccent,
            _ => Color.white
        };
        
        // Apply accent color (could be to emission, secondary material, etc.)
        if (cardMaterial != null && cardMaterial.HasProperty("_EmissionColor"))
        {
            cardMaterial.SetColor("_EmissionColor", accent * 0.3f);
            cardMaterial.EnableKeyword("_EMISSION");
        }
    }
    
    private void SetCardColor(Color color)
    {
        if (cardMaterial != null)
        {
            cardMaterial.color = color;
        }
    }
    
    // === Mouse Interaction (OnMouse callbacks - may not work in all setups) ===
    
    private void OnMouseEnter()
    {
        OnHoverEnter();
    }
    
    private void OnMouseExit()
    {
        OnHoverExit();
    }
    
    private void OnMouseDown()
    {
        OnCardClicked();
    }
    
    // === Public methods for CardInputHandler ===
    
    public void OnHoverEnter()
    {
        // Check if card can be interacted with
        bool canInteract = isInteractable && !isSelected;
        
        // For new dialogue system, check isDepleted; for old system, check currentDurability
        if (DialogueManager.Instance != null)
        {
            canInteract = canInteract && !isDepleted;
        }
        else
        {
            canInteract = canInteract && currentDurability > 0;
        }
        
        if (!canInteract) return;
        
        isHovered = true;
        targetPosition = originalPosition + Vector3.up * hoverLiftHeight;
        SetCardColor(hoverColor);
        
        Debug.Log($"[MaskCard3D] Hover Enter: {cardData?.maskName}");
    }
    
    public void OnHoverExit()
    {
        isHovered = false;

        if (!isSelected)
        {
            targetPosition = originalPosition;
            targetRotation = originalRotation;
        }

        // Check appropriate durability based on which system is active
        bool hasUsesLeft = DialogueManager.Instance != null ? !isDepleted : currentDurability > 0;
        
        if (isInteractable && hasUsesLeft && !isSelected)
        {
            SetCardColor(normalColor);
        }
        
        Debug.Log($"[MaskCard3D] Hover Exit: {cardData?.maskName}");
    }
    
    public void OnCardClicked()
    {
        // Check appropriate durability based on which system is active
        bool hasUsesLeft = DialogueManager.Instance != null ? !isDepleted : currentDurability > 0;
        
        if (!isInteractable || !hasUsesLeft)
            return;

        isSelected = true;

        Debug.Log($"[MaskCard3D] Clicked: {cardData?.maskName}");
        SelectCard();
    }

    public void SetHover()
    {
        targetPosition = originalPosition + Vector3.up * clickLift;

        targetRotation = originalRotation * Quaternion.Euler(clickRotation, 0f, 0f);

        particleEffect.SetActive(true);
    }

    /// <summary>
    /// Called when player selects this card.
    /// </summary>
    private void SelectCard()
    {
        if (firstCLick)
        {
            firstCLick = false;

            CardInputHandler.Instance.UpdateCardPos(this);

            return;
        }

        // Check if using new DialogueManager system
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsWaitingForMask)
        {
            SelectCardForDialogue();
            return;
        }

        // Legacy GameManager support
        if (cardData == null)
        {
            Debug.LogError($"[MaskCard3D] {name} has no CardData assigned!");
            return;
        }
        
        if (GameManager.Instance == null)
        {
            Debug.LogError("[MaskCard3D] GameManager not found!");
            return;
        }
        
        if (GameManager.Instance.CurrentState != GameManager.GameState.WaitingForInput)
        {
            Debug.LogWarning($"[MaskCard3D] Cannot select - game state is {GameManager.Instance.CurrentState}");
            return;
        }
        
        // Visual feedback
        SetCardColor(selectedColor);

        CardInputHandler.Instance.UpdateCardPos(this);

        firstCLick = true;
    }
    
    /// <summary>
    /// Called when card is selected during new dialogue system flow
    /// </summary>
    private void SelectCardForDialogue()
    {
        // Check if mask can still be used (2 uses max)
        if (!DialogueManager.Instance.CanUseMask(dialogueMaskType))
        {
            Debug.LogWarning($"[MaskCard3D] {cardData?.maskName} is depleted and cannot be used!");
            return;
        }
        
        Debug.Log($"[MaskCard3D] Selected mask for dialogue: {dialogueMaskType}");
        
        // Visual feedback
        SetCardColor(selectedColor);
        
        // Update uses remaining
        usesRemaining = DialogueManager.Instance.GetMaskRemainingUses(dialogueMaskType) - 1;
        
        // Notify DialogueManager
        DialogueManager.Instance.OnMaskSelected(dialogueMaskType);
        
        // Check if this was the last use
        if (usesRemaining <= 0)
        {
            isDepleted = true;
            PlayBreakEffect();
        }
        
        // Update display
        UpdateDurabilityDisplay();
        
        CardInputHandler.Instance?.UpdateCardPos(this);
        firstCLick = true;
    }

    public void DeselectCard()
    {
        isSelected = false;
        isHovered = false;

        targetPosition = originalPosition;
        targetRotation = originalRotation;

        particleEffect.SetActive(false);

        if (isInteractable && currentDurability > 0)
        {
            SetCardColor(normalColor);
        }
    }

    // === Event Handlers ===

    private void HandleDurabilityChanged(MaskType type, int newDurability)
    {
        if (cardData != null && cardData.maskType == type)
        {
            currentDurability = newDurability;
            UpdateDurabilityDisplay();
            
            if (newDurability <= 0)
            {
                PlayBreakEffect();
            }
        }
    }
    
    private void HandleGameStateChanged(GameManager.GameState newState)
    {
        isInteractable = newState == GameManager.GameState.WaitingForInput && currentDurability > 0;
        
        if (!isInteractable && !isHovered)
        {
            SetCardColor(currentDurability > 0 ? normalColor : disabledColor);
        }
        
        Debug.Log($"[MaskCard3D] {cardData?.maskName} - State: {newState}, Interactable: {isInteractable}");
    }
    
    // === New Dialogue System Event Handlers ===
    
    private void HandleMaskSelectionEnabled()
    {
        // Check if this mask can still be used
        if (DialogueManager.Instance != null)
        {
            bool canUse = DialogueManager.Instance.CanUseMask(dialogueMaskType);
            isInteractable = canUse;
            isDepleted = !canUse;
            
            if (!canUse)
            {
                SetCardColor(disabledColor);
            }
            else
            {
                SetCardColor(normalColor);
            }
            
            UpdateDurabilityDisplay();
        }
        
        Debug.Log($"[MaskCard3D] {cardData?.maskName} - Mask selection enabled, Interactable: {isInteractable}");
    }
    
    private void HandleMaskSelectionDisabled()
    {
        isInteractable = false;
        
        if (!isHovered && !isSelected)
        {
            SetCardColor(isDepleted ? disabledColor : normalColor);
        }
        
        Debug.Log($"[MaskCard3D] {cardData?.maskName} - Mask selection disabled");
    }
    
    private void PlayBreakEffect()
    {
        // Visual/audio feedback when mask breaks
        Debug.Log($"[MaskCard3D] {cardData?.maskName} BROKEN!");
        
        // Could add particle effect, sound, animation here
        StartCoroutine(BreakAnimation());
    }
    
    private System.Collections.IEnumerator BreakAnimation()
    {
        Vector3 startScale = transform.localScale;
        
        // Quick shake
        for (int i = 0; i < 5; i++)
        {
            transform.localPosition = originalPosition + Random.insideUnitSphere * 0.05f;
            yield return new WaitForSeconds(0.05f);
        }
        
        transform.localPosition = originalPosition;
        
        // Shrink slightly
        transform.localScale = startScale * 0.9f;
    }
}
