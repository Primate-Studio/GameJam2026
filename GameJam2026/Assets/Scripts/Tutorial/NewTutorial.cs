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
    
    [Header("Item Hints - Cliente 1 (2 items)")]
    public TutorialHint itemHint1_Client1;
    public TutorialHint itemHint2_Client1;
    
    [Header("Item Hints - Cliente 2 (3 items)")]
    public TutorialHint itemHint1_Client2;
    public TutorialHint itemHint2_Client2;
    public TutorialHint itemHint3_Client2;

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

    [Header("Result Panel")]
    public GameObject resultPanel;

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
            "Posa't dret gandul! Benvingut a l'Agència de Venda d'Odissea, l'imperi viral d'Ulisses.",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));
        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Ahir vas beure més que una esponja a l'Oasi, i ara tens un deute a pagar.",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));

        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Tens dues opcions: treballar o ser executat en acabar el dia. Tu tries.",
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
            "Molt bé! Prova de moure la càmera amb el ratolí.",
            dialogueSystem.dogSprite,
            dialogueSystem.mouseSprite,
            false,
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
            false,
            tutorialDog.transform
        ));
        
        yield return new WaitForSeconds(0.3f);
        
        playerRestrictions.DisableAll();
        playerRestrictions.EnableManual();
        
        yield return StartCoroutine(WaitForManualOpen());
        
        dialogueSystem.HideDialogue();
        
        yield return new WaitForSeconds(0.5f);
        
        playerRestrictions.DisableAll();
        
        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Ara tanca el manual amb TAB de nou.",
            dialogueSystem.dogSprite,
            dialogueSystem.tabSprite,
            false,
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
            false,
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
            "Tens clients esperant, tria quin vols atendre primer.",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));

        dialogueSystem.HideDialogue();

        if (client1Hint != null) client1Hint.ShowHint();
        else Debug.LogError("<color=red>client1Hint es NULL!</color>");

        if (client2Hint != null) client2Hint.ShowHint();
        else Debug.LogError("<color=red>client2Hint es NULL!</color>");

        // Mover al perro a una posición neutral
        if (dogController != null && dogPositions.Length > 0)
        {
            dogController.MoveTo(dogPositions[0]);
        }

        yield return new WaitForSeconds(0.5f);
        
        playerRestrictions.EnableAll();
    }

    /// <summary>
    /// Espera a que el jugador se acerque a uno de los dos clientes
    /// </summary>
    private IEnumerator WaitForClientChoice()
    {
        playerRestrictions.DisableInteraction();
        isCheckingConditions = true;

        while (true)
        {
            if (client1 != null && client1.IsPlayerInZone() && !stateManager.hasApproachedClient1)
            {
                stateManager.hasApproachedClient1 = true;
                stateManager.SetActiveClient(client1);
                if (client1Hint != null) client1Hint.HideHint();
                if (client2Hint != null) client2Hint.HideHint();
                isCheckingConditions = false;
                yield break;
            }

            if (client2 != null && client2.IsPlayerInZone() && !stateManager.hasApproachedClient2)
            {
                stateManager.hasApproachedClient2 = true;
                stateManager.SetActiveClient(client2);
                if (client2Hint != null) client2Hint.HideHint();
                if (client1Hint != null) client1Hint.HideHint();
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
        playerRestrictions.EnableInteraction();
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
            dogController.MoveTo(dogPositions[1]);
            yield return new WaitForSeconds(1f);
        }
        else
        {
            Debug.LogWarning("dogPositions[1] no está asignado en el Inspector.");
        }

        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Molt bé! Si vols acabar el tutorial, has de fer el mateix amb l'altre client. Ves-hi!",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));

        dialogueSystem.HideDialogue();
        playerRestrictions.EnableAll();

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
            Debug.LogWarning("No hay cliente restante para el segundo pedido.");
            yield break;
        }

        // Mostrar hint del cliente restante
        if (remainingClient.clientID == 1 && client1Hint != null)
        {
            client1Hint.ShowHint();
        }
        else if (remainingClient.clientID == 2 && client2Hint != null)
        {
            client2Hint.ShowHint();
        }

        isCheckingConditions = true;
        while (!remainingClient.IsPlayerInZone())
        {
            yield return null;
        }
        isCheckingConditions = false;

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
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));
        
        dialogueSystem.HideDialogue();
        
        // PASO 3: Generar pedido directamente (sin ir a ver items primero)
        GenerateTutorialOrder(remainingClient);
        
        yield return new WaitForSeconds(1.5f);
        
        // Mostrar hints de items del cliente restante
        TutorialHint[] remainingItemHints = GetItemHintsForClient(remainingClient);
        foreach (TutorialHint hint in remainingItemHints)
        {
            if (hint != null) hint.ShowHint();
        }
        
        // Mostrar hints de objetos
        remainingClient.ShowObjectHints();
        
        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Ves a buscar els objectes que compleixin les condicions!",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));
        
        dialogueSystem.HideDialogue();
        playerRestrictions.EnableAll();
        
        // PASO 4: Ir a recoger los items (con restricciones)
        yield return StartCoroutine(GoToItemsToCollect(remainingClient));
        
        // PASO 5: Entregar
        yield return StartCoroutine(ReturnToClientAndDeliver(remainingClient));

        stateManager.CompleteClient(remainingClient);
        remainingClient.CompleteOrder();
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

        // PASO NUEVO: Mostrar panel de resultados
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Aquesta és la factura del dia. Aquí podràs saber si continues viu un dia més o si...",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));

        dialogueSystem.HideDialogue();

        // Ocultar panel de resultados
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Enhorabona! Has completat el tutorial. Ara estàs preparat per gestionar la botiga sense ajuda.",
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
        playerRestrictions.DisableInteraction();
        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Cada client et donarà una comanda amb dues o tres condicions. Per a cada condició hauràs de lliurar un objecte.",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));

        stateManager.hasLearnedOrders = true;
        dialogueSystem.HideDialogue();

        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Si hi ha dues situacions, has de trobar els dos millors objectes per a cada una.",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));

        stateManager.hasLearnedOrders = true;
        dialogueSystem.HideDialogue();
        playerRestrictions.EnableInteraction();
    }


    
    /// <summary>
    /// Explica el manual
    /// </summary>
    private IEnumerator ExplainManual(TutorialClient client)
    {
        playerRestrictions.DisableInteraction();
        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Per saber quins objectes són els millors, consulta el manual.",
            dialogueSystem.dogSprite,
            dialogueSystem.tabSprite,
            true,
            tutorialDog.transform
        ));

        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Les pàgines estan separades per activitats. Cada fila representa una condició diferent i es troben en prestatgeries diferents.",
            dialogueSystem.dogSprite,
            dialogueSystem.manualSprite,
            true,
            tutorialDog.transform
        ));

        stateManager.hasSeenManualExplanation = true;
        stateManager.hasLearnedManual = true;
        dialogueSystem.HideDialogue();
        playerRestrictions.EnableInteraction();
    }

    /// <summary>
    /// Envía al jugador a los items para que los vea (primera vez - solo explicación)
    /// </summary>
    private IEnumerator GoToItemsFirstTime(TutorialClient client)
    {
        playerRestrictions.DisableAll();
        client.ShowObjectHints();
        
        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Gira't i mira els objectes que estan ressaltats. El manual et dirà quins són els millors per cada situació.",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));
        
        dialogueSystem.HideDialogue();
        playerRestrictions.EnableAll();

        foreach (TutorialHint hint in GetItemHintsForClient(client))
            if (hint != null) hint.ShowHint();

        yield return StartCoroutine(WaitForPlayerNearItems(client));

        foreach (TutorialHint hint in GetItemHintsForClient(client))
            if (hint != null) hint.HideHint();

        playerRestrictions.DisableAll();

        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Aquests són els objectes. Cada objecte té una qualitat, un preu i un temps de reaparició.",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));

        dialogueSystem.HideDialogue();
        client.HideObjectHints();
    }
    
    /// <summary>
    /// Espera a que el jugador esté cerca de los items
    /// </summary>
    private IEnumerator WaitForPlayerNearItems(TutorialClient client)
    {
        TutorialHint[] hints = GetItemHintsForClient(client);
        bool isNearAnyItem = false;
        
        while (!isNearAnyItem)
        {
            foreach (TutorialHint hint in hints)
            {
                if (hint != null && IsPlayerNearTransform(hint.transform, 1.8f))
                {
                    isNearAnyItem = true;

                    break;
                }
            }
            yield return null;
        }
    }
    
    /// <summary>
    /// Devuelve los hints de items según el cliente activo
    /// </summary>
    private TutorialHint[] GetItemHintsForClient(TutorialClient client)
    {
        if (client.clientID == 1)
        {
            return new TutorialHint[] { itemHint1_Client1, itemHint2_Client1 };
        }
        else
        {
            return new TutorialHint[] { itemHint1_Client2, itemHint2_Client2, itemHint3_Client2 };
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
        
        // Mostrar hint del cliente activo, ocultar el otro
        if (client.clientID == 1)
        {
            if (client1Hint != null) client1Hint.ShowHint();
            if (client2Hint != null) client2Hint.HideHint();
        }
        else
        {
            if (client2Hint != null) client2Hint.ShowHint();
            if (client1Hint != null) client1Hint.HideHint();
        }
        
        // Esperar a que vuelva con el cliente
        while (!client.IsPlayerInZone())
        {
            yield return null;
        }
        
        // Ocultar hint del cliente al llegar
        if (client.clientID == 1) { if (client1Hint != null) client1Hint.HideHint(); }
        else { if (client2Hint != null) client2Hint.HideHint(); }
        
        playerRestrictions.DisableAll();
        
        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Ara et donarà la seva comanda. Mira la nota que apareix a la dreta.",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));
        
        dialogueSystem.HideDialogue();

        GenerateTutorialOrder(client);

        foreach (TutorialHint hint in GetItemHintsForClient(client))
            if (hint != null) hint.ShowHint();

        yield return new WaitForSeconds(1.5f);
        
        playerRestrictions.DisableAll();
        
        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Perfecte! Ara porta els objectes a la motxilla del taulell per lliurar la comanda.",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));
        
        dialogueSystem.HideDialogue();
        playerRestrictions.EnableAll();
    }
    
    /// <summary>
    /// Genera el pedido del tutorial en la UI
    /// </summary>
    private void GenerateTutorialOrder(TutorialClient client)
    {
        int slotIndex = client.clientID - 1;

        if (OrderSystem.Instance != null)
        {
            OrderSystem.Instance.GenerateTutorialOrderForClient(
                client.gameObject,
                slotIndex,
                client.requirement1,
                client.requirement2,
                client.requirement3
            );
        }
        else
        {
            Debug.LogError("OrderSystem.Instance es null!");
        }

        client.hasReceivedOrder = true;
        stateManager.hasLearnedOrders = true;
    }
    
    /// <summary>
    /// Envía al jugador a recoger los items (segunda vez - para completar el pedido)
    /// </summary>
    private IEnumerator GoToItemsToCollect(TutorialClient client)
    {
        playerRestrictions.EnableAll();
        playerRestrictions.EnableInteraction();
        playerRestrictions.EnableInventory();

        client.ShowObjectHints();
        SetObjectTypeRestrictionForClient(client);

        yield return StartCoroutine(WaitForItemsCollected(client));

        client.HideObjectHints();
        RemoveObjectTypeRestriction();
        playerRestrictions.DisableInteraction();
    }
    
    /// <summary>
    /// Configura qué tipos de objetos puede recoger según el cliente
    /// </summary>
    private void SetObjectTypeRestrictionForClient(TutorialClient client)
    {
        if (playerRestrictions == null)
        {
            Debug.LogError("playerRestrictions es NULL!");
            return;
        }
        playerRestrictions.SetAllowedObjectTypes(GetObjectTypesFromRequirements(client), true);
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
        int requiredItems = client.requirement3 != null ? 3 : 2;

        while (GetItemsInInventoryCount() < requiredItems)
            yield return new WaitForSeconds(0.5f);

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
        playerRestrictions.DisableAll();

        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Perfecte! Ara porta els objectes a la motxilla del taulell per lliurar la comanda.",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));

        dialogueSystem.HideDialogue();
        playerRestrictions.EnableAll();
        playerRestrictions.EnableInteraction();
        playerRestrictions.EnableInventory();

        yield return StartCoroutine(WaitForOrderCompletion());

        playerRestrictions.DisableAll();

        yield return StartCoroutine(dialogueSystem.ShowDialogue(
            "Molt bé! Comanda completada!",
            dialogueSystem.dogSprite,
            null,
            true,
            tutorialDog.transform
        ));

        dialogueSystem.HideDialogue();
    }
    
    /// <summary>
    /// Espera a que se complete el pedido (versión simplificada)
    /// </summary>
    private IEnumerator WaitForOrderCompletion()
    {
        int initialCount = OrderSystem.Instance != null ? OrderSystem.Instance.GetActiveOrdersCount() : 0;
        float elapsedTime = 0f;
        float timeoutDuration = 60f;

        while (OrderSystem.Instance != null && OrderSystem.Instance.GetActiveOrdersCount() >= initialCount)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= timeoutDuration)
            {
                Debug.LogError($"TIMEOUT: El pedido no se completó en {timeoutDuration}s.");
                break;
            }
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
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
