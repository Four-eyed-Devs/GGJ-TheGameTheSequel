using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles 3D card selection via raycasting from the camera.
/// Attach to the Main Camera or a dedicated Input Manager object.
/// Uses the new Input System.
/// </summary>
public class CardInputHandler : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask cardLayerMask = -1; // -1 = Everything
    [SerializeField] private float raycastDistance = 100f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugRay = true;
    
    private MaskCard3D currentHoveredCard;
    private Mouse mouse;
    
    private void Start()
    {
        mouse = Mouse.current;
        
        if (mouse == null)
        {
            Debug.LogError("[CardInputHandler] No mouse found!");
        }
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        if (mainCamera == null)
        {
            Debug.LogError("[CardInputHandler] No camera found! Assign one or tag your camera as 'MainCamera'");
        }
        else
        {
            Debug.Log($"[CardInputHandler] Using camera: {mainCamera.name}");
        }
    }
    
    private void Update()
    {
        if (mainCamera == null || mouse == null) return;
        
        HandleRaycast();
        HandleClick();
    }
    
    private void HandleRaycast()
    {
        Vector2 mousePosition = mouse.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        
        if (showDebugRay)
        {
            Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.yellow);
        }
        
        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, cardLayerMask))
        {
            MaskCard3D card = hit.collider.GetComponent<MaskCard3D>();
            
            if (card != null)
            {
                // New card hovered
                if (currentHoveredCard != card)
                {
                    // Exit previous card
                    if (currentHoveredCard != null)
                    {
                        currentHoveredCard.OnHoverExit();
                    }
                    
                    // Enter new card
                    currentHoveredCard = card;
                    currentHoveredCard.OnHoverEnter();
                }
            }
            else
            {
                // Hit something that's not a card
                ClearHover();
            }
        }
        else
        {
            // Hit nothing
            ClearHover();
        }
    }
    
    private void ClearHover()
    {
        if (currentHoveredCard != null)
        {
            currentHoveredCard.OnHoverExit();
            currentHoveredCard = null;
        }
    }
    
    private void HandleClick()
    {
        if (mouse.leftButton.wasPressedThisFrame)
        {
            Debug.Log("[CardInputHandler] Mouse clicked");
            
            if (currentHoveredCard != null)
            {
                Debug.Log($"[CardInputHandler] Clicking card: {currentHoveredCard.name}");
                currentHoveredCard.OnCardClicked();
            }
            else
            {
                // Debug raycast to see what we're hitting
                Vector2 mousePosition = mouse.position.ReadValue();
                Ray ray = mainCamera.ScreenPointToRay(mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, cardLayerMask))
                {
                    Debug.Log($"[CardInputHandler] Hit object: {hit.collider.gameObject.name}, but no MaskCard3D component");
                }
                else
                {
                    Debug.Log("[CardInputHandler] Raycast hit nothing");
                }
            }
        }
    }
}
