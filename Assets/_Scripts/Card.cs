using UnityEngine;

public class Card : MonoBehaviour
{
    public string cardName;
    public string cardDescription;
    public int cardDurability;
    public CardType cardType;

    void Start()
    {
        
    }

    
    void Update()
    {
        
    }


    public enum CardType
    {
        Logic,
        Emotion,
        Aggresion
    }
}
