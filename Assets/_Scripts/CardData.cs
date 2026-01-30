using UnityEngine;

/// <summary>
/// Represents the type of psychological mask the player can use.
/// Each type is effective against specific detective attack patterns.
/// </summary>
public enum MaskType
{
    Logic,      // Cold, analytical deflection
    Emotion,    // Sympathy play, vulnerability
    Aggression, // Intimidation, outbursts
    Charm       // Manipulation, flattery
}

/// <summary>
/// ScriptableObject defining a psychological mask card.
/// Masks are the player's defense against detective interrogation.
/// </summary>
[CreateAssetMenu(fileName = "NewMask", menuName = "Interrogation/CardData")]
public class CardData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Display name of the mask (e.g., 'The Stoic')")]
    public string maskName;
    
    [TextArea(2, 4)]
    [Tooltip("Flavor text describing this persona")]
    public string description;
    
    [Header("Mechanics")]
    [Tooltip("The psychological archetype this mask represents")]
    public MaskType maskType;
    
    [Tooltip("Starting durability - how many times this mask can be used before breaking")]
    [Range(1, 5)]
    public int maxDurability = 3;
}
