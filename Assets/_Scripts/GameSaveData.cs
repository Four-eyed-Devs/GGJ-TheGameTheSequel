using UnityEngine;
using System;

/// <summary>
/// Serializable data container for game state persistence.
/// Handles auto-saving after each resolved turn.
/// </summary>
[Serializable]
public class GameSaveData
{
    private const string SAVE_KEY = "InterrogationSaveData";
    
    [Header("Game State")]
    public int suspicionMeter;
    public int[] maskDurabilities; // Indexed by MaskType enum
    public int currentNodeIndex;
    public bool gameInProgress;
    
    /// <summary>
    /// Creates a fresh save with default values.
    /// </summary>
    public GameSaveData()
    {
        suspicionMeter = 0;
        maskDurabilities = new int[4]; // One for each MaskType
        currentNodeIndex = 0;
        gameInProgress = false;
    }
    
    /// <summary>
    /// Initializes durabilities from an array of CardData assets.
    /// </summary>
    public void InitializeFromCards(CardData[] masks)
    {
        maskDurabilities = new int[4];
        foreach (var mask in masks)
        {
            maskDurabilities[(int)mask.maskType] = mask.maxDurability;
        }
        gameInProgress = true;
    }
    
    /// <summary>
    /// Saves the current game state to PlayerPrefs.
    /// </summary>
    public static void Save(GameSaveData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
        Debug.Log($"[GameSaveData] Saved: Suspicion={data.suspicionMeter}, Node={data.currentNodeIndex}");
    }
    
    /// <summary>
    /// Loads game state from PlayerPrefs. Returns null if no save exists.
    /// </summary>
    public static GameSaveData Load()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY))
        {
            Debug.Log("[GameSaveData] No save data found.");
            return null;
        }
        
        string json = PlayerPrefs.GetString(SAVE_KEY);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
        Debug.Log($"[GameSaveData] Loaded: Suspicion={data.suspicionMeter}, Node={data.currentNodeIndex}");
        return data;
    }
    
    /// <summary>
    /// Clears saved data (for new game or after game over).
    /// </summary>
    public static void ClearSave()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
        Debug.Log("[GameSaveData] Save data cleared.");
    }
    
    /// <summary>
    /// Checks if a saved game exists.
    /// </summary>
    public static bool HasSave()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }
}
