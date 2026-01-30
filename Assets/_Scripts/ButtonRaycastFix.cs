using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach to a Button to disable Raycast Target on all child graphics.
/// This ensures clicks reach the Button component.
/// </summary>
[ExecuteInEditMode]
public class ButtonRaycastFix : MonoBehaviour
{
    [ContextMenu("Fix Child Raycasts")]
    public void FixChildRaycasts()
    {
        // Disable raycast on all child graphics so button receives clicks
        foreach (var graphic in GetComponentsInChildren<Graphic>(true))
        {
            // Keep raycast enabled only on the button's own image
            if (graphic.gameObject == gameObject) continue;
            
            graphic.raycastTarget = false;
            Debug.Log($"[ButtonRaycastFix] Disabled raycast on {graphic.gameObject.name}");
        }
    }
    
    private void Start()
    {
        if (Application.isPlaying)
        {
            FixChildRaycasts();
        }
    }
}
