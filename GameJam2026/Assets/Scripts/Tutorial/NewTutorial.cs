using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Sistema de tutorial no lineal que permite al jugador explorar libremente
/// mientras guía progresivamente a través de las mecánicas del juego
/// </summary>
public class NewTutorial : MonoBehaviour
{
    public static NewTutorial Instance { get; private set; }

    [Header("Tutorial Dog")]
    public GameObject tutorialDog;
    private TutorialDog dogController;
    private Animator dogAnimator;
    public Transform[] dogPositions; // Posiciones donde el perro se moverá durante el tutorial

    [Header("Tutorial Clients")]
    public TutorialClient client1;
    public TutorialClient client2;
    
    [Header("Client Hints")]
    public TutorialHint client1Hint; // Hint que ilumina al cliente 1
    public TutorialHint client2Hint; // Hint que ilumina al cliente 2
    
    [Header("Item Triggers - Cliente 1 (2 items)")]
    public Transform itemTrigger1_Client1;
    public Transform itemTrigger2_Client1;
    
    [Header("Item Triggers - Cliente 2 (3 items)")]
    public Transform itemTrigger1_Client2;
    public Transform itemTrigger2_Client2;
    public Transform itemTrigger3_Client2;

    [Header("Tutorial Systems")]
    public TutorialDialogueSystem dialogueSystem;
    public TutorialPlayerRestrictions playerRestrictions;
    public TutorialStateManager stateManager;

    [Header("Player Reference")]
    public Transform playerTransform;

    [Header("Manual Reference")]
    public ManualUI manualUI;

    [Header("Inventory Reference")]
    public GameObject inventoryUI;
    public Sprite inventorySprite;

    [Header("Requirement Data - Cliente 1")]
    public RequirementData client1Requirement1;
    public RequirementData client1Requirement2;

    [Header("Requirement Data - Cliente 2")]
    public RequirementData client2Requirement1;
    public RequirementData client2Requirement2;
    public RequirementData client2Requirement3;

    private bool tutorialRunning = false;
    private bool isCheckingConditions = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Obtener componentes del perro
        if (tutorialDog != null)
        {
            dogController = tutorialDog.GetComponent<TutorialDog>();
            dogAnimator = tutorialDog.GetComponent<Animator>();
        }

        // Obtener referencias a los sistemas
        if (dialogueSystem == null)
            dialogueSystem = FindObjectOfType<TutorialDialogueSystem>();
        
        if (playerRestrictions == null)
            playerRestrictions = FindObjectOfType<TutorialPlayerRestrictions>();
        
        if (stateManager == null)
            stateManager = FindObjectOfType<TutorialStateManager>();
    }   
    
    void Start()
    {
        // Asignar los requirements a los clientes
        if (client1 != null)
        {
            client1.requirement1 = client1Requirement1;
            client1.requirement2 = client1Requirement2;
        }

        if (client2 != null)
        {
            client2.requirement1 = client2Requirement1;
            client2.requirement2 = client2Requirement2;
        }

        // Iniciar el tutorial
        StartTutorial();
    }

    void Update()
    {
        if (!tutorialRunning || isCheckingConditions) return;

        // Verificar condiciones según la fase actual
        CheckPhaseConditions();
    }

    /// <summary>
    /// Inicia el tutorial
    /// </summary>
    public void StartTutorial()
    {
        tutorialRunning = true;
        StartCoroutine(RunTutorial());
    }

    /// <summary>
    /// Corrutina principal del tutorial no lineal
    /// </summary>
    private IEnumerator RunTutorial()
    {
        // FASE 1: Introducción básica (lineal)
        yield return StartCoroutine(IntroductionPhase());

        // FASE 2: Exploración libre - esperar a que elija un cliente
        stateManager.SetPhase(TutorialStateManager.TutorialPhase.FreeExploration);
        yield return StartCoroutine(WaitForClientChoice());

        // FASE 3: Interacción con el primer cliente
        yield return StartCoroutine(FirstClientPhase());

        // FASE 4: Mensaje del perro - hacer el segundo cliente
        yield return StartCoroutine(BetweenClientsPhase());

        // FASE 5: Segundo cliente (simplificado)
        yield return StartCoroutine(SecondClientPhase());

        // FASE 6: Finalizar tutorial
        yield return StartCoroutine(CompleteTutorial());
    }

    /// <summary>
    /// FASE 1: Introducción - Explicaciones básicas lineales
    /// </summary>
    private IEnumerator IntroductionPhase()
    {
        stateManager.SetPhase(TutorialStateManager.TutorialPhase.Introduction);
        
        // Bloquear todo al inicio
        playerRestrictions.DisableAll();

        // Hacer que el perro mire al jugador con transición suave
        if (dogController != null && playerTransform != null)
        {
            dogController.LookAt(playerTransform);
        }
        
        yield return new WaitForSeconds(1f); // Pausa inicial

        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Posa't dret gandul! Benvingut a l'Àgencia de Venda d'Oidssees, l'imperi viral d'Ulisses. Anit et vas beure fins a l'aigua dels florers en l'Oasi i ara tens un deute a pagar.",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));

        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Ten dues opcions, treballa o ser executat a l'acabar el dia. Tu tries.",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));

        // ===== 1. MOVIMIENTO =====
        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Primer de tot, aprèn a moure't amb WASD.",
            dialogueSystem.dogSprite,
            dialogueSystem.wasdSprite,
            false, 
            tutorialDog.transform
        ));
        
        yield return new WaitForSeconds(0.3f);
        
        // Permitir solo movimiento
        playerRestrictions.DisableAll();
        playerRestrictions.EnableMovement();
        
        // ESPERAR a que el jugador se mueva
        yield return StartCoroutine(WaitForPlayerMovement());
        
        // AHORA sí ocultar el diálogo después de que se mueva
        dialogueSystem.HideDialogue();
        
        stateManager.hasLearnedMovement = true;
        yield return new WaitForSeconds(0.5f);

        // ===== 2. CÁMARA =====
        playerRestrictions.DisableAll();
        
        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Molt bé! Prova a moure la càmera amb el ratolí.",
            dialogueSystem.dogSprite,
            dialogueSystem.mouseSprite,
            false, // No esperar botón - texto permanece hasta que mueva la cámara
            tutorialDog.transform
        ));
        
        yield return new WaitForSeconds(0.3f);
        
        // Permitir SOLO cámara
        playerRestrictions.DisableAll();
        playerRestrictions.EnableCameraMovement();
        
        // ESPERAR a que el jugador mueva la cámara
        yield return StartCoroutine(WaitForCameraMovement());
        
        // AHORA sí ocultar el diálogo
        dialogueSystem.HideDialogue();
        
        stateManager.hasLearnedCamera = true;
        yield return new WaitForSeconds(0.5f);

        // ===== 3. MANUAL =====
        playerRestrictions.DisableAll();
        
        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Perfecte! Ara, obre el manual amb TAB. El necessitaràs per atendre els clients.",
            dialogueSystem.dogSprite,
            dialogueSystem.tabSprite,
            false, // No esperar botón - texto permanece hasta que abra el manual
            tutorialDog.transform
        ));
        
        yield return new WaitForSeconds(0.3f);
        
        // Permitir SOLO abrir manual
        playerRestrictions.DisableAll();
        playerRestrictions.EnableManual();
        
        // ESPERAR a que abra el manual
        yield return StartCoroutine(WaitForManualOpen());
        
        // AHORA sí ocultar el diálogo
        dialogueSystem.HideDialogue();
        
        yield return new WaitForSeconds(0.5f);
        
        // Mensaje para cerrar el manual
        playerRestrictions.DisableAll();
        
        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Ara tanca el manual amb TAB de nou.",
            dialogueSystem.dogSprite,
            dialogueSystem.tabSprite,
            false, // No esperar botón - texto permanece hasta que cierre el manual
            tutorialDog.transform
        ));
        
        yield return new WaitForSeconds(0.3f);
        
        // Permitir SOLO cerrar manual
        playerRestrictions.EnableManual();
        
        // ESPERAR a que cierre el manual
        yield return StartCoroutine(WaitForManualClose());
        
        // AHORA sí ocultar el diálogo
        dialogueSystem.HideDialogue();
        
        stateManager.hasLearnedManual = true;
        yield return new WaitForSeconds(0.5f);

        // ===== 4. INVENTARIO =====
        playerRestrictions.DisableAll();
        
        // Activar el panel del inventario
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(true);
        }
        
        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "A l'esquerra tens l'inventari, prova de canviar les posicions amb la roda del ratolí.",
            dialogueSystem.dogSprite,
            inventorySprite,
            false, // No esperar botón - texto permanece hasta que use el inventario
            tutorialDog.transform
        ));
        
        yield return new WaitForSeconds(0.3f);
        
        // Permitir SOLO usar inventario
        playerRestrictions.DisableAll();
        playerRestrictions.EnableInventory();
        
        // ESPERAR a que mueva los slots del inventario
        yield return StartCoroutine(WaitForInventoryUse());
        
        // AHORA sí ocultar el diálogo
        dialogueSystem.HideDialogue();
        
        stateManager.hasLearnedInventory = true;
        yield return new WaitForSeconds(0.5f);

        // ===== 5. MENSAJE FINAL =====
        playerRestrictions.DisableAll();
        
        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Ten clientes esperant, tria a quin vols atendre primer.",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));

        // Mostrar hints de clientes para que el jugador los vea iluminados
        if (client1Hint != null) client1Hint.ShowHint();
        if (client2Hint != null) client2Hint.ShowHint();

        // Mover al perro a una posición neutral
        if (dogController != null && dogPositions.Length > 0)
        {
            dogController.MoveTo(dogPositions[0]);
        }

        dialogueSystem.HideDialogue();
        yield return new WaitForSeconds(0.5f);
        
        playerRestrictions.EnableAll();
    }

    /// <summary>
    /// Espera a que el jugador se acerque a uno de los dos clientes
    /// </summary>
    private IEnumerator WaitForClientChoice()
    {
        isCheckingConditions = true;

        while (true)
        {
            // Verificar si se acerca al cliente 1
            if (client1 != null && client1.IsPlayerInZone() && !stateManager.hasApproachedClient1)
            {
                stateManager.hasApproachedClient1 = true;
                stateManager.SetActiveClient(client1);
                if (client1Hint != null) client1Hint.HideHint(); // Ocultar hint del cliente 1
                isCheckingConditions = false;
                yield break;
            }

            // Verificar si se acerca al cliente 2
            if (client2 != null && client2.IsPlayerInZone() && !stateManager.hasApproachedClient2)
            {
                stateManager.hasApproachedClient2 = true;
                stateManager.SetActiveClient(client2);
                if (client2Hint != null) client2Hint.HideHint(); // Ocultar hint del cliente 2
                isCheckingConditions = false;
                yield break;
            }

            yield return null;
        }
    }

    /// <summary>
    /// Verifica las condiciones según la fase actual
    /// </summary>
    private void CheckPhaseConditions()
    {
        // Esta función se puede expandir para verificar condiciones específicas
        // durante diferentes fases del tutorial
    }

    /// <summary>
    /// FASE 3: Primer cliente - Interacción completa con explicaciones
    /// </summary>
    private IEnumerator FirstClientPhase()
    {
        TutorialClient activeClient = stateManager.activeClient;
        if (activeClient == null) yield break;

        stateManager.SetPhase(TutorialStateManager.TutorialPhase.ClientInteraction);
        playerRestrictions.DisableAll();

        // 1. CLIENTE - Explicación de pedidos
        activeClient.SetTalkedTo();
        
        if (stateManager.ShouldExplainOrders())
        {
            yield return StartCoroutine(ExplainOrders(activeClient));
        }

        if (stateManager.ShouldExplainManual())
        {
            yield return StartCoroutine(ExplainManual(activeClient));
        }

        // 2. IR A LOS ITEMS - Primera vez para explicación
        yield return StartCoroutine(GoToItemsFirstTime(activeClient));

        // 3. VOLVER AL CLIENTE - Generar el pedido
        yield return StartCoroutine(ReturnToClientAndGenerateOrder(activeClient));

        // 4. IR A LOS ITEMS - Segunda vez para recogerlos
        yield return StartCoroutine(GoToItemsToCollect(activeClient));

        // 5. VOLVER AL CLIENTE - Entregar el pedido
        yield return StartCoroutine(ReturnToClientAndDeliver(activeClient));

        // Marcar cliente como completado
        stateManager.CompleteClient(activeClient);
        activeClient.CompleteOrder();
    }

    /// <summary>
    /// FASE 4: Entre clientes - Mensaje del perro
    /// NOTA: Necesitas configurar dogPositions[1] en el Inspector para que el perro camine aquí
    /// </summary>
    private IEnumerator BetweenClientsPhase()
    {
        playerRestrictions.DisableAll();

        // Mover al perro cerca del jugador
        if (dogController != null && dogPositions.Length > 1)
        {
            Debug.Log($"<color=cyan>Moviendo perro a dogPositions[1]: {dogPositions[1].position}</color>");
            dogController.MoveTo(dogPositions[1]);
            yield return new WaitForSeconds(1f);
        }
        else
        {
            Debug.LogWarning("<color=orange>dogPositions[1] no está asignado! Asigna un Transform en el Inspector</color>");
        }

        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Molt bé! Si vols acabar el tutorial, has de fer el mateix amb l'altre client. Ves-hi!",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));

        playerRestrictions.EnableAll();
        dialogueSystem.HideDialogue();

        stateManager.SetPhase(TutorialStateManager.TutorialPhase.SecondClient);
    }

    /// <summary>
    /// FASE 5: Segundo cliente - Versión simplificada sin repetir explicaciones
    /// </summary>
    private IEnumerator SecondClientPhase()
    {
        // Determinar cuál es el cliente restante
        TutorialClient remainingClient = null;

        if (!stateManager.IsClientCompleted(1))
        {
            remainingClient = client1;
        }
        else if (!stateManager.IsClientCompleted(2))
        {
            remainingClient = client2;
        }

        if (remainingClient == null)
        {
            Debug.LogWarning("<color=orange>No hay cliente restante para el segundo pedido</color>");
            yield break;
        }
        
        Debug.Log($"<color=cyan>===== SEGUNDO CLIENTE: Cliente {remainingClient.clientID} =====</color>");
        
        // Mostrar hint del cliente restante
        if (remainingClient.clientID == 1 && client1Hint != null)
        {
            client1Hint.ShowHint();
        }
        else if (remainingClient.clientID == 2 && client2Hint != null)
        {
            client2Hint.ShowHint();
        }

        // PASO 1: Esperar a que se acerque al cliente restante
        isCheckingConditions = true;
        Debug.Log($"<color=yellow>Esperando a que el jugador se acerque al cliente {remainingClient.clientID}...</color>");
        
        while (!remainingClient.IsPlayerInZone())
        {
            yield return null;
        }
        
        isCheckingConditions = false;
        Debug.Log($"<color=green>✓ Jugador cerca del cliente {remainingClient.clientID}</color>");
        
        // Ocultar hint del cliente
        if (remainingClient.clientID == 1 && client1Hint != null)
        {
            client1Hint.HideHint();
        }
        else if (remainingClient.clientID == 2 && client2Hint != null)
        {
            client2Hint.HideHint();
        }

        stateManager.SetActiveClient(remainingClient);
        remainingClient.SetTalkedTo();
        
        // PASO 2: Mensaje simplificado del cliente
        playerRestrictions.DisableAll();
        
        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Ara que ja saps com funciona tot, et donaré la meva comanda. Mira les notes a la dreta!",
            dialogueSystem.clientSprite,
            null,
            true,
            remainingClient.clientTransform
        ));
        
        dialogueSystem.HideDialogue();
        
        // PASO 3: Generar pedido directamente (sin ir a ver items primero)
        GenerateTutorialOrder(remainingClient);
        
        yield return new WaitForSeconds(1.5f);
        
        // Mostrar hints de objetos
        remainingClient.ShowObjectHints();
        
        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Ves a buscar els objectes que compleixin les condicions!",
            dialogueSystem.clientSprite,
            null,
            true,
            remainingClient.clientTransform
        ));
        
        dialogueSystem.HideDialogue();
        playerRestrictions.EnableAll();
        
        // PASO 4: Ir a recoger los items (con restricciones)
        yield return StartCoroutine(GoToItemsToCollect(remainingClient));
        
        // PASO 5: Entregar
        yield return StartCoroutine(ReturnToClientAndDeliver(remainingClient));

        stateManager.CompleteClient(remainingClient);
        remainingClient.CompleteOrder();
        
        Debug.Log($"<color=green>✓✓✓ SEGUNDO CLIENTE COMPLETADO ✓✓✓</color>");
    }

    /// <summary>
    /// FASE 6: Completar tutorial
    /// </summary>
    private IEnumerator CompleteTutorial()
    {
        stateManager.SetPhase(TutorialStateManager.TutorialPhase.Completed);
        playerRestrictions.DisableAll();

        // Mover al perro al centro
        if (dogController != null && dogPositions.Length > 2)
        {
            dogController.MoveTo(dogPositions[2]);
            yield return new WaitForSeconds(1f);
        }

        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Enhorabona! Has completat el tutorial. Ara estàs preparat per gestionar la botiga!",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));

        dialogueSystem.HideDialogue();
        playerRestrictions.EnableAll();

        tutorialRunning = false;

        // Transición al juego normal
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameState.Playing);
        }
    }

    // ============= MÉTODOS AUXILIARES =============

    /// <summary>
    /// Explica qué son los pedidos
    /// </summary>
    private IEnumerator ExplainOrders(TutorialClient client)
    {
        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Cada client et donarà una comanda amb dues o tres condicions. Per a cada condició hauràs d'entregar un objecte.",
            dialogueSystem.clientSprite,
            null,
            true,
            client.clientTransform
        ));

        stateManager.hasLearnedOrders = true;
        dialogueSystem.HideDialogue();

        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Si son dos situacions has de trobar els dos millors objectes per aquestes.",
            dialogueSystem.clientSprite,
            null,
            true,
            client.clientTransform
        ));

        stateManager.hasLearnedOrders = true;
        dialogueSystem.HideDialogue();
    }


    
    /// <summary>
    /// Explica el manual
    /// </summary>
    private IEnumerator ExplainManual(TutorialClient client)
    {
        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Per saber quins objectes són els millors, consulta el manual",
            dialogueSystem.dogSprite,
            dialogueSystem.tabSprite,
            true,
            client.clientTransform
        ));

        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Les pàgines estan separades per activitats. Cada fila representa una condició diferent i estan ubicades en prestatgeries diferents.",
            dialogueSystem.dogSprite,
            dialogueSystem.manualSprite,
            true,
            tutorialDog.transform
        ));

        stateManager.hasSeenManualExplanation = true;
        stateManager.hasLearnedManual = true;
        dialogueSystem.HideDialogue();
    }

    /// <summary>
    /// Envía al jugador a los items para que los vea (primera vez - solo explicación)
    /// </summary>
    private IEnumerator GoToItemsFirstTime(TutorialClient client)
    {
        playerRestrictions.DisableAll();
        
        // Iluminar los objetos
        client.ShowObjectHints();
        
        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Fixat en els objectes que estan ressaltats. Consulta el manual per saber quins objectes són els millors per cada situació.",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));
        
        dialogueSystem.HideDialogue();
        playerRestrictions.EnableAll();
        
        // Esperar a que llegue a los items
        yield return StartCoroutine(WaitForPlayerNearItems(client));
        
        // Explicar los items
        playerRestrictions.DisableAll();
        
        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Aquests són els objectes. Cada objecte té una qualitat, un preu i un temps de reaparició.",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));
        
        // yield return StartCoroutine(dialogueSystem.ShowDialogue(
        //     "Consulta el manual per saber quins objectes són els millors per cada situació.",
        //     dialogueSystem.dogSprite,
        //     dialogueSystem.tabSprite,
        //     true,
        //     tutorialDog.transform
        // ));
        
        dialogueSystem.HideDialogue();
        
        // Ocultar hints temporalmente
        client.HideObjectHints();
    }
    
    /// <summary>
    /// Espera a que el jugador esté cerca de los items
    /// </summary>
    private IEnumerator WaitForPlayerNearItems(TutorialClient client)
    {
        Transform[] triggers = GetItemTriggersForClient(client);
        bool isNearAnyItem = false;
        
        while (!isNearAnyItem)
        {
            foreach (Transform trigger in triggers)
            {
                if (trigger != null && IsPlayerNearTransform(trigger, 1f))
                {
                    isNearAnyItem = true;
                    break;
                }
            }
            yield return null;
        }
    }
    
    /// <summary>
    /// Devuelve los triggers de items según el cliente activo
    /// </summary>
    private Transform[] GetItemTriggersForClient(TutorialClient client)
    {
        if (client.clientID == 1)
        {
            return new Transform[] { itemTrigger1_Client1, itemTrigger2_Client1 };
        }
        else
        {
            return new Transform[] { itemTrigger1_Client2, itemTrigger2_Client2, itemTrigger3_Client2 };
        }
    }
    
    /// <summary>
    /// Verifica si el jugador está cerca de un transform
    /// </summary>
    private bool IsPlayerNearTransform(Transform target, float distance)
    {
        if (target == null || playerTransform == null) return false;
        return Vector3.Distance(playerTransform.position, target.position) < distance;
    }
    
    /// <summary>
    /// Vuelve al cliente y genera el pedido oficial
    /// </summary>
    private IEnumerator ReturnToClientAndGenerateOrder(TutorialClient client)
    {
        playerRestrictions.DisableAll();
        
        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Ara torna amb el client per rebre la comanda oficial.",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));
        
        dialogueSystem.HideDialogue();
        playerRestrictions.EnableAll();
        
        // Esperar a que vuelva con el cliente
        while (!client.IsPlayerInZone())
        {
            yield return null;
        }
        
        playerRestrictions.DisableAll();
        
        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Ara et donaré la meva comanda. Mira la nota que apareix a la dreta.",
            dialogueSystem.clientSprite,
            null,
            true,
            client.clientTransform
        ));
        
        dialogueSystem.HideDialogue();
        
        // GENERAR EL PEDIDO REAL (mostrar en UI)
        GenerateTutorialOrder(client);
        
        // Verificar que la mochila se creó
        yield return new WaitForSeconds(0.5f);
        DeliveryBox[] deliveryBoxes = FindObjectsByType<DeliveryBox>(FindObjectsSortMode.None);
        Debug.Log($"<color=yellow>DeliveryBoxes encontradas en la escena: {deliveryBoxes.Length}</color>");
        foreach (var box in deliveryBoxes)
        {
            Debug.Log($"<color=yellow>  - DeliveryBox en posición: {box.transform.position}</color>");
        }
        
        yield return new WaitForSeconds(1.5f);
        
        playerRestrictions.DisableAll();
        
        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Perfecte! Ara porta els objectes a la motxilla del taulell per lliurar la comanda.",
            dialogueSystem.clientSprite,
            null,
            true,
            client.clientTransform
        ));
        
        dialogueSystem.HideDialogue();
        playerRestrictions.EnableAll();
    }
    
    /// <summary>
    /// Genera el pedido del tutorial en la UI
    /// </summary>
    private void GenerateTutorialOrder(TutorialClient client)
    {
        // Obtener el GameObject del cliente
        GameObject clientObj = client.gameObject;
        
        // IMPORTANTE: OrderSystem usa índices 0, 1, 2 para spawnBox1, spawnBox2, spawnBox3
        // Así que si clientID es 1 o 2, debemos restar 1
        int slotIndex = client.clientID - 1;
        
        Debug.Log($"<color=yellow>Generando pedido: clientID={client.clientID}, slotIndex={slotIndex}</color>");
        
        // Usar OrderSystem directamente como el tutorial antiguo
        if (OrderSystem.Instance != null)
        {
            OrderSystem.Instance.GenerateTutorialOrderForClient(
                clientObj,
                slotIndex,  // Usar 0, 1 o 2
                client.requirement1,
                client.requirement2,
                client.requirement3
            );
            
            Debug.Log($"<color=cyan>✓ Pedido generado para cliente {client.clientID} en slot {slotIndex}</color>");
            Debug.Log($"<color=cyan>  La mochila (DeliveryBox) se ha instanciado en el mostrador automáticamente</color>");
        }
        else
        {
            Debug.LogError("<color=red>OrderSystem.Instance es null!</color>");
        }
        
        // Marcar que ha recibido la orden
        client.hasReceivedOrder = true;
        stateManager.hasLearnedOrders = true;
    }
    
    /// <summary>
    /// Envía al jugador a recoger los items (segunda vez - para completar el pedido)
    /// </summary>
    private IEnumerator GoToItemsToCollect(TutorialClient client)
    {
        Debug.Log($"<color=yellow>GoToItemsToCollect iniciado para cliente {client.clientID}</color>");
        
        playerRestrictions.EnableAll();
        
        // IMPORTANTE: Activar interacción y uso de inventario SOLO durante la recolección de items
        playerRestrictions.EnableInteraction();
        playerRestrictions.EnableInventory();
        
        // Iluminar objetos de nuevo
        client.ShowObjectHints();
        
        // Habilitar restricción de objetos - solo puede coger los del pedido
        SetObjectTypeRestrictionForClient(client);
        
        Debug.Log($"<color=yellow>Esperando a que recoja {(client.requirement3 != null ? 3 : 2)} items...</color>");
        
        // Esperar a que recoja los objetos necesarios
        yield return StartCoroutine(WaitForItemsCollected(client));
        
        Debug.Log($"<color=green>Items recogidos! Ocultando hints...</color>");
        
        // Ocultar hints
        client.HideObjectHints();
        
        // Quitar restricción
        RemoveObjectTypeRestriction();
        
        // IMPORTANTE: Desactivar interacción después de recoger los items
        playerRestrictions.DisableInteraction();
        // Mantener inventario activo para que pueda entregar
        
        Debug.Log($"<color=green>GoToItemsToCollect completado</color>");
    }
    
    /// <summary>
    /// Configura qué tipos de objetos puede recoger según el cliente
    /// </summary>
    private void SetObjectTypeRestrictionForClient(TutorialClient client)
    {
        if (playerRestrictions == null)
        {
            Debug.LogError("<color=red>playerRestrictions es NULL!</color>");
            return;
        }
        
        // Obtener los tipos de objetos del pedido
        ObjectType[] allowedTypes = GetObjectTypesFromRequirements(client);
        
        Debug.Log($"<color=yellow>Restringiendo objetos para cliente {client.clientID}: {string.Join(", ", allowedTypes)}</color>");
        
        // Configurar restricción
        playerRestrictions.SetAllowedObjectTypes(allowedTypes, true);
    }
    
    /// <summary>
    /// Obtiene los tipos de objetos de los requirements del cliente
    /// </summary>
    private ObjectType[] GetObjectTypesFromRequirements(TutorialClient client)
    {
        // TODO: Esto dependerá de cómo estén configurados tus RequirementData
        // Por ahora devuelvo un array vacío, debes adaptarlo a tu estructura
        if (client.clientID == 1)
        {
            // Cliente 1: 2 objetos
            return new ObjectType[] { ObjectType.Odre, ObjectType.Arco }; // EJEMPLO
        }
        else
        {
            // Cliente 2: 3 objetos
            return new ObjectType[] { ObjectType.Mascaras, ObjectType.CascoA, ObjectType.Espejo }; // EJEMPLO
        }
    }
    
    /// <summary>
    /// Elimina la restricción de tipos de objetos
    /// </summary>
    private void RemoveObjectTypeRestriction()
    {
        if (playerRestrictions != null)
        {
            playerRestrictions.SetAllowedObjectTypes(null, false);
        }
    }
    
    /// <summary>
    /// Espera a que el jugador recoja los items necesarios
    /// </summary>
    private IEnumerator WaitForItemsCollected(TutorialClient client)
    {
        // Determinar cuántos items necesita
        int requiredItems = client.requirement3 != null ? 3 : 2;
        
        Debug.Log($"<color=cyan>Esperando {requiredItems} items en el inventario...</color>");
        
        // Esperar hasta que tenga los items en el inventario
        while (GetItemsInInventoryCount() < requiredItems)
        {
            // Log cada 2 segundos para ver el progreso
            int currentCount = GetItemsInInventoryCount();
            Debug.Log($"<color=cyan>Items en inventario: {currentCount}/{requiredItems}</color>");
            
            yield return new WaitForSeconds(2f);
        }
        
        Debug.Log($"<color=green>¡{requiredItems} items recogidos!</color>");
        yield return new WaitForSeconds(0.5f);
    }
    
    /// <summary>
    /// Cuenta cuántos items tiene el jugador en el inventario
    /// </summary>
    private int GetItemsInInventoryCount()
    {
        if (InventoryManager.Instance == null) return 0;
        
        int count = 0;
        foreach (var slot in InventoryManager.Instance.slots)
        {
            if (!slot.IsEmpty())
            {
                count++;
            }
        }
        return count;
    }
    
    /// <summary>
    /// Vuelve al cliente para entregar el pedido
    /// </summary>
    private IEnumerator ReturnToClientAndDeliver(TutorialClient client)
    {
        Debug.Log($"<color=yellow>ReturnToClientAndDeliver iniciado para cliente {client.clientID}</color>");
        
        playerRestrictions.DisableAll();
        
        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Perfecte! Ara porta els objectes a la motxilla del mostrador per lliurar la comanda.",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));
        
        dialogueSystem.HideDialogue();
        
        // NOTA: La mochila (DeliveryBox) ya fue creada por OrderSystem en el mostrador
        Debug.Log($"<color=cyan>La mochila del cliente {client.clientID} está en el mostrador (creada por OrderSystem)</color>");
        
        playerRestrictions.EnableAll();
        
        // IMPORTANTE: Activar interacción e inventario para poder entregar en la DeliveryBox
        playerRestrictions.EnableInteraction();
        playerRestrictions.EnableInventory();
        
        Debug.Log($"<color=cyan>Esperando a que complete el pedido en la DeliveryBox del mostrador...</color>");
        
        // Esperar a que complete el pedido
        yield return StartCoroutine(WaitForOrderCompletion());
        
        // IMPORTANTE: Desactivar interacción e inventario después de entregar
        playerRestrictions.DisableInteraction();
        playerRestrictions.DisableInventory();
        
        playerRestrictions.DisableAll();
        
        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Molt bé! Comanda completada!",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));
        
        dialogueSystem.HideDialogue();
        
        Debug.Log($"<color=green>ReturnToClientAndDeliver completado</color>");
    }
    
    /// <summary>
    /// Espera a que se complete el pedido (versión simplificada)
    /// </summary>
    private IEnumerator WaitForOrderCompletion()
    {
        // Obtener el count inicial de pedidos
        int initialCount = 0;
        if (OrderSystem.Instance != null)
        {
            initialCount = OrderSystem.Instance.GetActiveOrdersCount();
        }
        
        // Esperar hasta que disminuya el número de pedidos (se completó uno)
        while (OrderSystem.Instance != null && OrderSystem.Instance.GetActiveOrdersCount() >= initialCount)
        {
            yield return null;
        }
        
        // Esperar un momento adicional para que termine las animaciones
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// Explica los objetos, su calidad, dinero y tiempo de reaparición
    /// </summary>
    private IEnumerator ExplainObjects(TutorialClient client)
    {
        // Iluminar los objetos del cliente
        client.ShowObjectHints();

        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Aquests objectes estan ressaltats perquè són els que necessites. Ves a agafar-los!",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));

        dialogueSystem.HideDialogue();
        playerRestrictions.EnableAll();

        // Esperar a que se acerque a los objetos
        // TODO: Implementar detección de proximidad a objetos

        yield return new WaitForSeconds(1f); // Temporal

        playerRestrictions.DisableAll();

        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Cada objecte té una qualitat, un preu i un temps de reaparició. Tria saviament!",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));

        stateManager.hasLearnedItemQuality = true;
        dialogueSystem.HideDialogue();
    }

    /// <summary>
    /// Espera a que el jugador recoja los objetos necesarios
    /// </summary>
    private IEnumerator WaitForObjectCollection(TutorialClient client)
    {
        playerRestrictions.EnableAll();

        // TODO: Implementar verificación de objetos recogidos
        // Por ahora, esperar unos segundos
        yield return new WaitForSeconds(5f);

        client.HideObjectHints();
    }

    /// <summary>
    /// Entrega el pedido al cliente
    /// </summary>
    private IEnumerator DeliverOrder(TutorialClient client)
    {
        playerRestrictions.DisableAll();

        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Ara torna amb el client per entregar el pedido!",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));

        dialogueSystem.HideDialogue();
        playerRestrictions.EnableAll();

        // Esperar a que vuelva con el cliente
        isCheckingConditions = true;
        while (!client.IsPlayerInZone())
        {
            yield return null;
        }
        isCheckingConditions = false;

        playerRestrictions.DisableAll();

        // Mostrar pedido en la UI de la derecha y mochila
        client.ShowBackpack();

        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "El client ha deixat una motxilla. Posa els objectes dins. Recorda que no hi ha marxa enrere!",
            dialogueSystem.dogSprite,
            null,
            true,
            client.backpack.transform
        ));

        dialogueSystem.HideDialogue();
        playerRestrictions.EnableAll();

        // Esperar a que complete el pedido
        yield return StartCoroutine(WaitForOrderCompletion(client));
    }

    /// <summary>
    /// Espera a que se complete un pedido
    /// </summary>
    private IEnumerator WaitForOrderCompletion(TutorialClient client)
    {
        // TODO: Implementar verificación de pedido completado
        // Por ahora, esperar unos segundos
        yield return new WaitForSeconds(5f);

        playerRestrictions.DisableAll();

        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Molt bé! Pedido completat!",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));

        dialogueSystem.HideDialogue();
    }

    // ============= MÉTODOS DE ESPERA PARA ACCIONES DEL JUGADOR =============

    /// <summary>
    /// Espera a que el jugador se mueva (WASD)
    /// </summary>
    private IEnumerator WaitForPlayerMovement()
    {
        bool hasMoved = false;
        
        while (!hasMoved)
        {
            if (InputManager.Instance != null)
            {
                float horizontal = Mathf.Abs(InputManager.Instance.Horizontal);
                float vertical = Mathf.Abs(InputManager.Instance.Vertical);
                
                if (horizontal > 0.1f || vertical > 0.1f)
                {
                    hasMoved = true;
                }
            }
            
            yield return null;
        }
        
        // Esperar un poco más para que vea que funciona
        yield return new WaitForSeconds(1.5f);
    }

    /// <summary>
    /// Espera a que el jugador mueva la cámara (Mouse)
    /// </summary>
    private IEnumerator WaitForCameraMovement()
    {
        float totalMouseMovement = 0f;
        float requiredMovement = 50f; // Cantidad de movimiento requerido
        
        while (totalMouseMovement < requiredMovement)
        {
            if (InputManager.Instance != null)
            {
                float mouseX = Mathf.Abs(InputManager.Instance.MouseX);
                float mouseY = Mathf.Abs(InputManager.Instance.MouseY);
                
                totalMouseMovement += mouseX + mouseY;
            }
            
            yield return null;
        }
        
        yield return new WaitForSeconds(1f);
    }

    /// <summary>
    /// Espera a que el jugador abra el manual
    /// </summary>
    private IEnumerator WaitForManualOpen()
    {
        ManualUI manualUI = FindObjectOfType<ManualUI>();
        if (manualUI == null) yield break;
        
        while (!manualUI.manualPanel.activeSelf)
        {
            yield return null;
        }
        
        stateManager.hasOpenedManual = true;
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// Espera a que el jugador cierre el manual
    /// </summary>
    private IEnumerator WaitForManualClose()
    {
        ManualUI manualUI = FindObjectOfType<ManualUI>();
        if (manualUI == null)
        {
            Debug.LogWarning("<color=yellow>ManualUI no encontrado</color>");
            yield break;
        }
        
        // Esperar hasta que el manual esté cerrado
        while (manualUI.manualPanel.activeSelf)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// Espera a que el jugador use el inventario (cambio de slot)
    /// </summary>
    private IEnumerator WaitForInventoryUse()
    {
        if (InventoryManager.Instance == null) yield break;
        
        int initialSlot = InventoryManager.Instance.currentSlotIndex;
        bool hasChanged = false;
        
        while (!hasChanged)
        {
            if (InventoryManager.Instance.currentSlotIndex != initialSlot)
            {
                hasChanged = true;
            }
            
            yield return null;
        }
        
        yield return new WaitForSeconds(1f);
    }
}
