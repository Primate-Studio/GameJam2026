using UnityEngine;

public class PlayerRaycast : MonoBehaviour
{
    [Header("Settings")]
    public float rayDistance = 5f;
    public LayerMask interactableLayer;
    
    [Header("References")]
    [SerializeField] private Camera playerCamera; // Referència a la càmera
    private GameObject lastHighlighted;
    public InteractableObject CurrentTarget { get; private set; }

    void Start()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        if (playerCamera == null) return;

        // Creem el llamp des del centre de la càmera (el centre de la pantalla)
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 3f)) // Distància de 3 metres
        {
            GameObject currentObject = hit.collider.gameObject;
            InteractableObject interactable = currentObject.GetComponent<InteractableObject>();
            if (currentObject.CompareTag("Items") || currentObject.CompareTag("Bag")) // Assegura't que l'objecte tingui aquest Tag
            {
                if(interactable != null) CurrentTarget = interactable;
                else CurrentTarget = null;
                if(currentObject.CompareTag("Items"))
                {
                    CurrentTarget.isDeliveryZone = false;
                }
                else if(currentObject.CompareTag("Bag"))
                {
                    CurrentTarget.isDeliveryZone = true;
                }
                
                if (lastHighlighted != currentObject)
                {
                    ClearHighlight();
                    lastHighlighted = currentObject;
                    // Canviem la layer per activar l'outline del post-process i als children
                    lastHighlighted.layer = LayerMask.NameToLayer("Highlighted");
                    foreach (Transform child in lastHighlighted.transform)
                    {
                        if(child.gameObject.layer != LayerMask.NameToLayer("IgnoreOutline")) child.gameObject.layer = LayerMask.NameToLayer("Highlighted");
                    }
                }
            }
            else
            {
                ClearHighlight();
            }
        }
        else
        {
            ClearHighlight();
        }
    }

    void ClearHighlight()
    {
        if (lastHighlighted != null)
        {
            CurrentTarget = null; 
            lastHighlighted.layer = LayerMask.NameToLayer("Items");
            foreach (Transform child in lastHighlighted.transform)
            {
                if(child.gameObject.layer != LayerMask.NameToLayer("IgnoreOutline")) child.gameObject.layer = LayerMask.NameToLayer("Items");
            }
            lastHighlighted = null;
        }
    }
}