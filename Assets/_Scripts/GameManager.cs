using UnityEngine;
using System;
using System.Collections.Generic;
using Interrogation.Dialogue;

/// <summary>
/// Core game controller managing the interrogation flow.
/// Singleton pattern for easy access from UI components.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game Configuration")]
    [Tooltip("All available mask cards")]
    public CardData[] masks;
    
    [Tooltip("Ordered list of dialogue nodes for the interrogation")]
    public DialogueNode[] dialogueSequence;
    
    [Tooltip("Suspicion threshold for game over (confession)")]
    public int maxSuspicion = 100;
    
    [Header("Runtime State (Debug)")]
    [SerializeField] private int suspicionMeter;
    [SerializeField] private int[] maskDurabilities;
    [SerializeField] private int currentNodeIndex;
    [SerializeField] private GameState currentState;
    
    // Properties for external access
    public int SuspicionMeter => suspicionMeter;
    public GameState CurrentState => currentState;
    public DialogueNode CurrentNode => 
        (currentNodeIndex >= 0 && currentNodeIndex < dialogueSequence.Length) 
            ? dialogueSequence[currentNodeIndex] 
            : null;
    
    // Events for UI updates
    public event Action<DialogueNode> OnDialogueChanged;
    public event Action<int> OnSuspicionChanged;
    public event Action<MaskType, int> OnMaskDurabilityChanged;
    public event Action<GameState> OnGameStateChanged;
    public event Action<bool> OnGameOver; // true = survived, false = confession
    
    /// <summary>
    /// Represents the current phase of a turn.
    /// </summary>
    public enum GameState
    {
        NotStarted,
        DetectiveSpeaking,
        WaitingForInput,
        ResolvingOutcome,
        GameOver
    }
    
    private void Awake()
    {
        Debug.Log("[GameManager] Awake called");
        
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Debug.Log("[GameManager] Duplicate instance destroyed");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log("[GameManager] Singleton instance set");
    }
    
    private void Start()
    {
        Debug.Log($"[GameManager] Start called. Masks: {(masks != null ? masks.Length : 0)}, Nodes: {(dialogueSequence != null ? dialogueSequence.Length : 0)}");
        
        // Check if new DialogueManager system is being used - if so, skip old system
        if (Interrogation.Dialogue.DialogueManager.Instance != null)
        {
            Debug.Log("[GameManager] New DialogueManager detected - disabling old GameManager flow");
            currentState = GameState.NotStarted;
            return;
        }
        
        // Always start fresh - no loading saved games
        StartNewGame();
    }
    
    /// <summary>
    /// Initializes a fresh interrogation session.
    /// </summary>
    public void StartNewGame()
    {
        // Validate configuration
        if (masks == null || masks.Length == 0)
        {
            Debug.LogError("[GameManager] No masks assigned! Drag CardData assets into the Masks array.");
            return;
        }
        
        if (dialogueSequence == null || dialogueSequence.Length == 0)
        {
            Debug.LogError("[GameManager] No dialogue nodes assigned! Drag DialogueNode assets into the Dialogue Sequence array.");
            return;
        }
        
        GameSaveData.ClearSave();
        
        suspicionMeter = 0;
        currentNodeIndex = 0;
        
        // Initialize mask durabilities from card data
        maskDurabilities = new int[4];
        foreach (var mask in masks)
        {
            if (mask != null)
            {
                maskDurabilities[(int)mask.maskType] = mask.maxDurability;
                Debug.Log($"[GameManager] Initialized {mask.maskName} with durability {mask.maxDurability}");
            }
        }
        
        Debug.Log("[GameManager] Starting new interrogation...");
        TransitionToState(GameState.DetectiveSpeaking);
    }
    
    /// <summary>
    /// Loads game state from persistent storage.
    /// </summary>
    public void LoadGame()
    {
        GameSaveData saveData = GameSaveData.Load();
        if (saveData == null || !saveData.gameInProgress)
        {
            StartNewGame();
            return;
        }
        
        suspicionMeter = saveData.suspicionMeter;
        currentNodeIndex = saveData.currentNodeIndex;
        maskDurabilities = saveData.maskDurabilities;
        
        Debug.Log($"[GameManager] Loaded game at node {currentNodeIndex}");
        TransitionToState(GameState.DetectiveSpeaking);
    }
    
    /// <summary>
    /// Saves current game state (called automatically after each turn).
    /// </summary>
    private void AutoSave()
    {
        GameSaveData saveData = new GameSaveData
        {
            suspicionMeter = this.suspicionMeter,
            maskDurabilities = this.maskDurabilities,
            currentNodeIndex = this.currentNodeIndex,
            gameInProgress = currentState != GameState.GameOver
        };
        GameSaveData.Save(saveData);
    }
    
    /// <summary>
    /// Transitions the game to a new state and triggers appropriate events.
    /// </summary>
    private void TransitionToState(GameState newState)
    {
        currentState = newState;
        OnGameStateChanged?.Invoke(newState);
        
        switch (newState)
        {
            case GameState.DetectiveSpeaking:
                PresentDialogue();
                break;
            case GameState.WaitingForInput:
                // UI will handle showing mask buttons
                break;
            case GameState.ResolvingOutcome:
                // Outcome is resolved immediately in SelectMask
                break;
            case GameState.GameOver:
                // Event already fired in CheckGameOver
                break;
        }
    }
    
    /// <summary>
    /// Displays the current dialogue node.
    /// </summary>
    private void PresentDialogue()
    {
        if (CurrentNode == null)
        {
            // No more dialogue - player survived!
            EndGame(survived: true);
            return;
        }
        
        Debug.Log($"[Detective] {CurrentNode.dialogueText}");
        OnDialogueChanged?.Invoke(CurrentNode);
        
        // Brief pause before allowing input (could be animated)
        TransitionToState(GameState.WaitingForInput);
    }
    
    /// <summary>
    /// Called by MaskButton when player selects a mask.
    /// </summary>
    public void SelectMask(CardData selectedMask)
    {
        if (currentState != GameState.WaitingForInput)
        {
            Debug.LogWarning("[GameManager] Cannot select mask - not waiting for input!");
            return;
        }
        
        if (GetMaskDurability(selectedMask.maskType) <= 0)
        {
            Debug.LogWarning($"[GameManager] {selectedMask.maskName} is broken!");
            return;
        }
        
        TransitionToState(GameState.ResolvingOutcome);
        ResolveOutcome(selectedMask);
    }
    
    /// <summary>
    /// Compares selected mask type against detective's attack type.
    /// </summary>
    private void ResolveOutcome(CardData selectedMask)
    {
        DialogueNode node = CurrentNode;
        bool successfulBlock = selectedMask.maskType == node.damageType;
        
        if (successfulBlock)
        {
            // Correct mask - deflect but take durability damage
            Debug.Log($"[Resolution] Blocked with {selectedMask.maskName}! Mask takes wear.");
            DecrementMaskDurability(selectedMask.maskType);
        }
        else
        {
            // Wrong mask - suspicion increases
            Debug.Log($"[Resolution] Wrong mask! Suspicion +{node.suspicionDamage}");
            AddSuspicion(node.suspicionDamage);
        }
        
        // Check for game over conditions
        if (CheckGameOver())
        {
            return;
        }
        
        // Advance to next dialogue
        AdvanceDialogue();
        
        // Auto-save after resolution
        AutoSave();
    }
    
    /// <summary>
    /// Reduces a mask's durability by 1.
    /// </summary>
    private void DecrementMaskDurability(MaskType type)
    {
        int index = (int)type;
        maskDurabilities[index] = Mathf.Max(0, maskDurabilities[index] - 1);
        OnMaskDurabilityChanged?.Invoke(type, maskDurabilities[index]);
        
        if (maskDurabilities[index] <= 0)
        {
            Debug.Log($"[GameManager] {type} mask has BROKEN!");
        }
    }
    
    /// <summary>
    /// Adds to the suspicion meter.
    /// </summary>
    private void AddSuspicion(int amount)
    {
        suspicionMeter = Mathf.Min(maxSuspicion, suspicionMeter + amount);
        OnSuspicionChanged?.Invoke(suspicionMeter);
    }
    
    /// <summary>
    /// Gets current durability for a mask type.
    /// </summary>
    public int GetMaskDurability(MaskType type)
    {
        return maskDurabilities[(int)type];
    }
    
    /// <summary>
    /// Checks if game over conditions are met.
    /// </summary>
    private bool CheckGameOver()
    {
        // Suspicion maxed - confession
        if (suspicionMeter >= maxSuspicion)
        {
            EndGame(survived: false);
            return true;
        }
        
        // All masks broken - defenseless
        bool allBroken = true;
        for (int i = 0; i < maskDurabilities.Length; i++)
        {
            if (maskDurabilities[i] > 0)
            {
                allBroken = false;
                break;
            }
        }
        
        if (allBroken)
        {
            Debug.Log("[GameManager] All masks broken - defenseless!");
            EndGame(survived: false);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Moves to the next dialogue node.
    /// </summary>
    private void AdvanceDialogue()
    {
        currentNodeIndex++;
        
        if (currentNodeIndex >= dialogueSequence.Length)
        {
            // Survived the interrogation!
            EndGame(survived: true);
            return;
        }
        
        TransitionToState(GameState.DetectiveSpeaking);
    }
    
    /// <summary>
    /// Ends the game with the specified outcome.
    /// </summary>
    private void EndGame(bool survived)
    {
        TransitionToState(GameState.GameOver);
        GameSaveData.ClearSave();
        
        if (survived)
        {
            Debug.Log("[GameManager] === INTERROGATION SURVIVED ===");
        }
        else
        {
            Debug.Log("[GameManager] === CONFESSION - GAME OVER ===");
        }
        
        OnGameOver?.Invoke(survived);
    }
}
