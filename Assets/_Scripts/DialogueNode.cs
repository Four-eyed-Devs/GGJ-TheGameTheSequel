using UnityEngine;

/// <summary>
/// ScriptableObject representing a single detective statement/accusation.
/// Forms the building blocks of the interrogation narrative.
/// </summary>
[CreateAssetMenu(fileName = "NewDialogue", menuName = "Interrogation/DialogueNode")]
public class DialogueNode : ScriptableObject
{
    [Header("Narrative")]
    [TextArea(3, 6)]
    [Tooltip("What the detective says to the player")]
    public string dialogueText;
    
    [TextArea(2, 4)]
    [Tooltip("Optional hint shown in the inner monologue area")]
    public string innerMonologueHint;
    
    [Header("Mechanics")]
    [Tooltip("The type of mask that will successfully deflect this statement")]
    public MaskType damageType;
    
    [Tooltip("How much suspicion increases if the player uses the wrong mask")]
    [Range(5, 30)]
    public int suspicionDamage = 10;
    
    [Header("Flow")]
    [Tooltip("Possible next nodes (for branching dialogue). If empty, interrogation ends.")]
    public DialogueNode[] nextNodes;
}
