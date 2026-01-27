using UnityEngine;

public class PlayerRaycast : MonoBehaviour
{
    [Header("Settings")]
    public float rayDistance = 5f;
    public LayerMask interactableLayer;
    
    [Header("References")]
    [SerializeField] private Camera playerCamera; // Referència a la càmera
    
    private Outline currentOutline;
    private GameObject lastHighlighted;

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

            if (currentObject.CompareTag("Items")) // Assegura't que l'objecte tingui aquest Tag
            {
                if (lastHighlighted != currentObject)
                {
                    ClearHighlight();
                    lastHighlighted = currentObject;
                    // Canviem la layer per activar l'outline del post-process i als children
                    lastHighlighted.layer = LayerMask.NameToLayer("Highlighted");
                    foreach (Transform child in lastHighlighted.transform)
                    {
                        child.gameObject.layer = LayerMask.NameToLayer("Highlighted");
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
            lastHighlighted.layer = LayerMask.NameToLayer("Items");
            foreach (Transform child in lastHighlighted.transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Items");
            }
            lastHighlighted = null;
        }
    }


    private void SetOutlineState(Outline outline, bool state)
    {
        outline.enabled = state; 
        
        if (state) outline.EnableOutline(); else outline.DisableOutline();
    }

    private void ClearCurrentOutline()
    {
        if (currentOutline != null)
        {
            SetOutlineState(currentOutline, false);
            currentOutline = null;
        }
    }
}