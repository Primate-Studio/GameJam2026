using UnityEngine;
using System.Collections.Generic;

public class ClientManager : MonoBehaviour
{
    public static ClientManager Instance { get; private set; }

    [Header("Configuració")]
    public GameObject clientPrefab;
    public Transform spawnPoint;
    public Transform[] targetPoints; // Els 3 llocs de la botiga

    [Header("Estat")]
    private GameObject[] activeClients; // Guardem el client de cada slot (null si buit)
    private float spawnTimer = 0f;
    public float spawnInterval = 15f;

    [Header("Configuració dels Clients")]
    public int maxClientsPerDay = 5;
    public int clientsCount = 0;
    public float orderDuration = 60f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        activeClients = new GameObject[targetPoints.Length];
        spawnTimer = 20f;
    }

    void Start()
    {
        // En modo tutorial, resetear el timer para evitar spawns inmediatos
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Tutorial)
        {
            orderDuration = 300f; // Tiempo muy alto para que nunca se acabe el pedido
            spawnTimer = 99999f; // Timer muy alto para que nunca se ejecute
        }
    }

    void Update()
    {
        HandleSpawning();

        // Debug: Si premem G, fem fora el primer client que trobem
        if (Input.GetKeyDown(KeyCode.G))
        {
            DismissFirstClient();
        }
    }

    private void HandleSpawning()
    {
        // En tutorial, no spawnear clientes automáticamente
        if (GameManager.Instance.CurrentState == GameState.Tutorial)
        {
            return;
        }

        // Verificar que no se ha alcanzado el límite de clientes del día
        if (clientsCount >= maxClientsPerDay)
        {
            return; // No spawnear más clientes
        }

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0)
        {
            int freeSlot = GetFreeSlotIndex();
            if (freeSlot != -1)
            {
                SpawnClientInSlot(freeSlot);
                spawnInterval = Random.Range(10f, 20f);
                spawnTimer = spawnInterval; // Reset del timer
            }
        }
    }

    private int GetFreeSlotIndex()
    {
        for (int i = 0; i < activeClients.Length; i++)
        {
            if (activeClients[i] == null) return i;
        }
        return -1;
    }

    public void SpawnClientInSlot(int slotIndex)
    {
        //Debug.Log($"<color=lime>👤 ClientManager: Spawneando cliente en slot {slotIndex}</color>");
        
        GameObject client = Instantiate(clientPrefab, spawnPoint.position, spawnPoint.rotation);
        activeClients[slotIndex] = client;
        clientsCount++;

        // Visuals (El que ja tenies)
        ClientBodyCreator bodyCreator = client.GetComponent<ClientBodyCreator>();
        if (bodyCreator != null) bodyCreator.ApplyRandomLook();

        // Moviment al slot corresponent
        ClientMovement mover = client.GetComponent<ClientMovement>();
        
        // Cuando el cliente llegue a su posición, generar el pedido
        mover.OnArrival = () =>
        {
            //Debug.Log($"<color=lime>👤 Cliente llegó al slot {slotIndex}, generando pedido...</color>");
            
            if (OrderSystem.Instance != null)
            {
                OrderSystem.Instance.GenerateOrderForClient(client, slotIndex);
            }
            else
            {
                Debug.LogError("<color=red>OrderSystem.Instance es null!</color>");
            }
        };
        
        mover.MoveTo(targetPoints[slotIndex].position, false, slotIndex);
    }

    public void DismissFirstClient()
    {
        for (int i = 0; i < activeClients.Length; i++)
        {
            if (activeClients[i] != null)
            {
                GameObject client = activeClients[i];
                activeClients[i] = null; // Alliberem el slot immediatament

                ClientMovement mover = client.GetComponent<ClientMovement>();
                mover.MoveTo(spawnPoint.position, true);
                
                // Quan arribi al spawn, que es destrueixi
                mover.OnArrival = () => mover.Despawn();
                return;
            }
        }
    }
    public void DismissClientInSlot(int slotIndex)
    {
        // 1. Seguretat: mirem si realment hi ha algú en aquell slot
        if (slotIndex < 0 || slotIndex >= activeClients.Length || activeClients[slotIndex] == null)
        {
            Debug.LogWarning($"Intentant fer fora un client del slot {slotIndex} però està buit!");
            return;
        }

        // 2. Agafem la referència (NO alliberem el slot encara)
        GameObject client = activeClients[slotIndex];

        // 3. Ordenem al client que marxi
        ClientMovement mover = client.GetComponent<ClientMovement>();
        if (mover != null)
        {
            mover.MoveTo(spawnPoint.position, true);
            
            // Li assignem la destrucció quan arribi al final
            // IMPORTANT: Alliberem el slot DESPRÉS que el client s'hagi destruït
            mover.OnArrival = () => 
            {
                //Debug.Log($"<color=orange>👋 Cliente del slot {slotIndex} llegó al spawn, liberando slot...</color>");
                activeClients[slotIndex] = null; // Ara sí alliberem el slot
                mover.Despawn();
            };
        }
        else
        {
            // Si no té ClientMovement, alliberem immediatament
            activeClients[slotIndex] = null;
            Destroy(client);
        }
    }
    public void CalculateTimer()
    {
        MoneyManager.Instance.CalculateDebtLevel();
        switch (MoneyManager.Instance.DebtLevel)
        {
            case DebtLevel.High:
                orderDuration = 70f;
                break;
            case DebtLevel.Medium:
                orderDuration = 62f;
                break;
            case DebtLevel.Low:
                orderDuration = 51f;
                break;
            case DebtLevel.LowLow:
                orderDuration = 37f;
                break;
            case DebtLevel.None:
                orderDuration = 25f;
                break;
        }
    }
    
    public GameObject GetClientInSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= activeClients.Length)
        {
            Debug.LogWarning($"Slot index {slotIndex} fuera de rango!");
            return null;
        }
        return activeClients[slotIndex];
    }

    /// <summary>
    /// Obtiene la Transform de posición de un slot específico
    /// </summary>
    public Transform GetClientSlotPosition(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < targetPoints.Length)
        {
            return targetPoints[slotIndex];
        }
        
        Debug.LogError($"<color=red>Slot index {slotIndex} fuera de rango!</color>");
        return null;
    }
}
