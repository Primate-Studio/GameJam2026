using UnityEngine;

public class ClientBodyCreator : MonoBehaviour
{
   // [Header("")]
    public ClientVisualPool visualPool;
    private GameObject face;
    private GameObject head;
    private GameObject clothes;
    private GameObject bodyAccessory;
    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            //for testing purposes
            Destroy(head.gameObject);
            Destroy(face.gameObject);
            Destroy(clothes.gameObject);
            Destroy(bodyAccessory.gameObject);
            
            //reapply random look
            ApplyRandomLook(visualPool);
        }   
    }

    public void ApplyRandomLook(ClientVisualPool visualPool)
    {
        if (visualPool == null) { Debug.LogWarning("VisualPool es null"); return; }

        head = SpawnAndReset(visualPool.headAccessories, transform);
        face = SpawnAndReset(visualPool.faces, transform);
        clothes = SpawnAndReset(visualPool.clothes, transform);
        bodyAccessory = SpawnAndReset(visualPool.bodyAccessories, transform);
    }

    private GameObject SpawnAndReset(GameObject[] pool, Transform parent)
    {
        //reset pool
        if (pool == null || pool.Length == 0) return null;
        var prefab = pool[Random.Range(0, pool.Length)];
        var go = Instantiate(prefab, parent, false);
        var t = go.transform;
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;

        return go;
    }
}

