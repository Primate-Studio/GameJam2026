using UnityEngine;

public class PlayerRaycast : MonoBehaviour
{
    [Header("Settings")]
    public float rayDistance = 5f;
    public LayerMask interactableLayer;
    
    [Header("References")]
    [SerializeField] private Camera playerCamera; // Referència a la càmera
    
    private Outline currentOutline;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        if (playerCamera == null) return;

        // Creem el llamp des del centre de la càmera (el centre de la pantalla)
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Dibuixa el llamp a l'editor per poder veure si està anant on vols
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red);

        if (Physics.Raycast(ray, out hit, rayDistance, interactableLayer))
        {
            // Busquem el component Outline a l'objecte impactat
            Outline outline = hit.collider.GetComponent<Outline>();
            
            if (outline != null)
            {
                // Si mirem un objecte nou, desactivem l'anterior
                if (currentOutline != null && currentOutline != outline)
                {
                    SetOutlineState(currentOutline, false);
                }
                
                // Activem el nou
                SetOutlineState(outline, true);
                currentOutline = outline;
            }
            else
            {
                // Si l'objecte té la Layer però no té Outline component
                ClearCurrentOutline();
            }
        }
        else
        {
            ClearCurrentOutline();
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