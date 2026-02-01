using UnityEngine;

public class OutroController : MonoBehaviour
{
    [Tooltip("Reference to the TextController in the scene")]
    public TextControllerOutro textControllerOutro;

    [Tooltip("If true, the shown text will auto-hide after displayDuration")]
    public bool autoHide = false;

    private void Awake()
    {
        if (textControllerOutro == null)
        {
            textControllerOutro = FindObjectOfType<TextControllerOutro>();
        }
    }

    // Call this when the outcome is known
    public void ShowOutcome(bool playerWon)
    {
        if (textControllerOutro == null)
        {
            Debug.LogWarning("OutroController: TextController not assigned.");
            return;
        }

        textControllerOutro.ShowTextByOutcome(playerWon, autoHide);
    }
}
