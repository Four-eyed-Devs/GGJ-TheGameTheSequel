using UnityEngine;
using UnityEditor;
using TMPro;

/// <summary>
/// Editor utility to create 3D mask card objects for the interrogation table.
/// </summary>
public class MaskCard3DSetup : EditorWindow
{
    [MenuItem("Tools/Interrogation/Create 3D Mask Cards")]
    public static void CreateMaskCards()
    {
        // Create parent container
        GameObject cardsParent = new GameObject("MaskCards");
        
        // Card positions on table (adjust based on your table size)
        Vector3[] positions = new Vector3[]
        {
            new Vector3(-0.45f, 0, 0.1f),
            new Vector3(-0.15f, 0, 0.1f),
            new Vector3(0.15f, 0, 0.1f),
            new Vector3(0.45f, 0, 0.1f)
        };
        
        string[] cardNames = { "Card_Stoic", "Card_Victim", "Card_Hothead", "Card_Charmer" };
        
        for (int i = 0; i < 4; i++)
        {
            GameObject card = CreateCardObject(cardNames[i], positions[i]);
            card.transform.SetParent(cardsParent.transform);
        }
        
        Debug.Log("[MaskCard3DSetup] Created 4 mask cards. Remember to:");
        Debug.Log("  1. Position 'MaskCards' parent on your table");
        Debug.Log("  2. Assign CardData assets to each card's MaskCard3D component");
        Debug.Log("  3. Adjust card size/rotation to fit your scene");
        
        Selection.activeGameObject = cardsParent;
    }
    
    private static GameObject CreateCardObject(string name, Vector3 position)
    {
        // Create card as a simple cube (replace with your own model later)
        GameObject card = GameObject.CreatePrimitive(PrimitiveType.Cube);
        card.name = name;
        card.transform.localPosition = position;
        card.transform.localScale = new Vector3(0.25f, 0.02f, 0.35f); // Card-like proportions
        
        // Add MaskCard3D component
        MaskCard3D maskCard = card.AddComponent<MaskCard3D>();
        
        // Set up renderer reference
        SerializedObject so = new SerializedObject(maskCard);
        so.FindProperty("cardRenderer").objectReferenceValue = card.GetComponent<Renderer>();
        so.ApplyModifiedProperties();
        
        // Create card face with text
        GameObject cardFace = new GameObject("CardFace");
        cardFace.transform.SetParent(card.transform);
        cardFace.transform.localPosition = new Vector3(0, 0.51f, 0); // Slightly above card surface
        cardFace.transform.localRotation = Quaternion.Euler(90, 0, 0); // Face up
        cardFace.transform.localScale = new Vector3(1f, 1f, 1f);
        
        // Add name text
        GameObject nameObj = new GameObject("NameText");
        nameObj.transform.SetParent(cardFace.transform);
        nameObj.transform.localPosition = new Vector3(0, 0.05f, 0);
        nameObj.transform.localRotation = Quaternion.identity;
        nameObj.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
        
        TextMeshPro nameTmp = nameObj.AddComponent<TextMeshPro>();
        nameTmp.text = name.Replace("Card_", "The ");
        nameTmp.fontSize = 4;
        nameTmp.alignment = TextAlignmentOptions.Center;
        nameTmp.color = Color.white;
        
        // Add durability text
        GameObject durabilityObj = new GameObject("DurabilityText");
        durabilityObj.transform.SetParent(cardFace.transform);
        durabilityObj.transform.localPosition = new Vector3(0, -0.08f, 0);
        durabilityObj.transform.localRotation = Quaternion.identity;
        durabilityObj.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
        
        TextMeshPro durabilityTmp = durabilityObj.AddComponent<TextMeshPro>();
        durabilityTmp.text = "3/3";
        durabilityTmp.fontSize = 3;
        durabilityTmp.alignment = TextAlignmentOptions.Center;
        durabilityTmp.color = new Color(0.7f, 0.7f, 0.7f);
        
        // Wire up text references
        so = new SerializedObject(maskCard);
        so.FindProperty("nameText").objectReferenceValue = nameTmp;
        so.FindProperty("durabilityText").objectReferenceValue = durabilityTmp;
        so.ApplyModifiedProperties();
        
        // Set material color
        Renderer rend = card.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = new Color(0.15f, 0.15f, 0.2f);
        }
        
        return card;
    }
    
    [MenuItem("Tools/Interrogation/Create Simple Card Prefab")]
    public static void CreateSimplePrefab()
    {
        // Just creates one card you can duplicate/modify
        GameObject card = GameObject.CreatePrimitive(PrimitiveType.Cube);
        card.name = "MaskCard_Template";
        card.transform.localScale = new Vector3(0.25f, 0.02f, 0.35f);
        
        card.AddComponent<MaskCard3D>();
        
        // Set up renderer reference
        MaskCard3D maskCard = card.GetComponent<MaskCard3D>();
        SerializedObject so = new SerializedObject(maskCard);
        so.FindProperty("cardRenderer").objectReferenceValue = card.GetComponent<Renderer>();
        so.ApplyModifiedProperties();
        
        Debug.Log("[MaskCard3DSetup] Created template card. Add TextMeshPro 3D objects as children for name/durability display.");
        
        Selection.activeGameObject = card;
    }
}
