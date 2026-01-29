using UnityEngine;
using UnityEngine.AI;

public class PortraitCamera : MonoBehaviour
{
    public static PortraitCamera Instance { get; private set; }

    [Header("Configuració")]
    [SerializeField] private Camera portraitCam;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private string portraitLayerName = "Portrait"; 

    private int portraitLayer = -1;

    void Awake()
    {
        Instance = this;
        portraitLayer = LayerMask.NameToLayer(portraitLayerName);
        
        if (portraitCam != null)
        {
            portraitCam.cullingMask = 1 << portraitLayer;
            portraitCam.clearFlags = CameraClearFlags.SolidColor;
            portraitCam.backgroundColor = new Color(0, 0, 0, 0);
            portraitCam.enabled = false; 
        }
    }

    public Sprite TakePortrait(GameObject client)
    {
        if (spawnPoint == null || portraitCam == null) return null;

        // 1. GUARDEM L'ESTAT ORIGINAL DEL CLIENT
        Vector3 originalPosition = client.transform.position;
        Quaternion originalRotation = client.transform.rotation;
        Transform originalParent = client.transform.parent;
        int originalLayer = client.layer;

        // Desactivem el NavMeshAgent temporalment per poder-lo moure lliurement
        NavMeshAgent agent = client.GetComponent<NavMeshAgent>();
        bool agentWasEnabled = agent != null && agent.enabled;
        if (agent != null) agent.enabled = false;

        // 2. MOVEM EL CLIENT A LA ZONA DE FOTOS
        client.transform.position = spawnPoint.position;
        client.transform.rotation = spawnPoint.rotation;
        
        // Li canviem la capa a ell i a tots els seus fills
        SetLayerRecursive(client, portraitLayer);

        // 3. CAPTUREM LA FOTO
        RenderTexture rt = portraitCam.targetTexture;
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;
        
        // Netegem el fons abans de disparar
        GL.Clear(true, true, Color.clear);
        portraitCam.Render();

        Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();

        // 4. RESTAUREM EL CLIENT AL SEU ESTAT ORIGINAL
        client.transform.position = originalPosition;
        client.transform.rotation = originalRotation;
        client.transform.SetParent(originalParent);
        SetLayerRecursive(client, originalLayer);

        if (agent != null) agent.enabled = agentWasEnabled;
        RenderTexture.active = prev;

        // Retornem el Sprite
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursive(child.gameObject, layer);
    }
}