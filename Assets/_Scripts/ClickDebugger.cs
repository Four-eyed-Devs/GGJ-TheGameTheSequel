using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Debug component to trace UI pointer events.
/// Attach to buttons to see if they receive any pointer events.
/// </summary>
public class ClickDebugger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"[ClickDebugger] POINTER ENTER: {gameObject.name}");
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"[ClickDebugger] POINTER EXIT: {gameObject.name}");
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"[ClickDebugger] POINTER DOWN: {gameObject.name}");
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log($"[ClickDebugger] POINTER UP: {gameObject.name}");
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[ClickDebugger] POINTER CLICK: {gameObject.name}");
    }
}
