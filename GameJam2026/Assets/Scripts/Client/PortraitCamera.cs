using UnityEngine;
using UnityEngine.AI;

public class PortraitCamera : MonoBehaviour
{
    public static PortraitCamera Instance { get; private set; }

    [Header("Configuració")]
    [SerializeField] private Camera portraitCam;
    [SerializeField] private Transform spawnPoint;
    
    // CANVI: Usem un int per al número de la capa o el busquem per nom
    [SerializeField] private string portraitLayerName = "Portrait"; 

    void Awake() => Instance = this;

    public Sprite TakePortrait(GameObject originalClient)
    {
        if (spawnPoint == null) return null;

        // --- TRUC PER EVITAR L'ERROR DEL NAVMESH ---
        // Busquem l'agent al client original
        NavMeshAgent agent = originalClient.GetComponent<NavMeshAgent>();
        bool agentWasEnabled = false;
        
        if (agent != null) 
        {
            agentWasEnabled = agent.enabled;
            agent.enabled = false; // Desactivem l'agent de l'original un segon
        }

        // Fem la còpia (el clone naixerà amb l'agent desactivat i no donarà error)
        GameObject clone = Instantiate(originalClient, spawnPoint.position, spawnPoint.rotation);
        clone.GetComponentInChildren<OrderUIItem>()?.gameObject.SetActive(false); // Assegurem que la UI del client no es vegi al retrat
        
        // Tornem a activar l'agent de l'original perquè pugui seguir la seva vida a la botiga
        if (agent != null) agent.enabled = agentWasEnabled;
        // -------------------------------------------

        // Assegurem que el clone no tingui scripts de moviment actius
        if (clone.TryGetComponent(out ClientMovement mover)) mover.enabled = false;

        int layerIndex = LayerMask.NameToLayer(portraitLayerName);
        SetLayerRecursive(clone, layerIndex);

        portraitCam.Render();

        RenderTexture currentRT = portraitCam.targetTexture;
        RenderTexture.active = currentRT;
        Texture2D texture = new Texture2D(currentRT.width, currentRT.height, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0, 0, currentRT.width, currentRT.height), 0, 0);
        texture.Apply();

        Destroy(clone);
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform) SetLayerRecursive(child.gameObject, layer);
    }
}