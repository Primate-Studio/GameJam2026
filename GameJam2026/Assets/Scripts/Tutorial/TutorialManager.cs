using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    
    public static TutorialManager Instance { get; private set; }

    [Header("Tutorial Dog")]
    public GameObject tutorialDog;
    private TutorialDog dogController;
    private Animator dogAnimator;

    [Header("Tutorial UI")]
    public Canvas tutorialUI;
    public UnityEngine.UI.Image tutorialImage;
    public GameObject inventoryUI;
    public TextMeshProUGUI tutorialText;
    public TextMeshProUGUI tutorialDebtText;
    public Slider timeSlider;
    public Button continueButton;
    public GameObject textPanel;
    public GameObject resultCanvas;

    [Header("Tutorial Sprites")]
    public Sprite movementSprite;
    public Sprite cameraSprite;
    public Sprite interactionSprite; 
    public Sprite manualSprite; 
    public Sprite inventorySprite;
    public Sprite desperationSprite;


    [Header("Tutorial Conditions")]
    public bool canPlayerMove = false;
    public bool waitingForDog = false;
    public bool canPlayerMoveCamera = false;
    public bool canPlayerInteract = false;
    public bool canPlayerOpenManual = false;
    public bool canPlayerCloseManual = false;
    public bool canPlayerChangePage = false;
    public bool canPlayerUseInventory = false;
    public bool isWaitingForPlayerAction = false;
    public bool canGenerateOrder = false;
    public bool isWaitingForFirstClientOrder = false;
    public bool isWaitingForSecondClientOrder = false;
    public bool isWaitingContinueButton = false;
    public bool isWaitingForManualOpen = false;
    public bool isWaitingForManualClose = false;
    public bool tutorialIsPaused = false;
    public bool isPlayerLookingAt = false;
    public bool playerHasDoneTutorial = false;
    public bool orderHasBeenShown = false;
    public ObjectType allowedObjectType = ObjectType.Odre;  // Nuevo: tipo de objeto permitido
    public bool isObjectTypeRestricted = false;  // Nuevo: si está restringido o no
    public ObjectType[] allowedObjectTypes;

    [Header("Transforms")]

    public Transform playerPosition;
    public Transform[] playerTransforms;
    public Transform[] dogTransforms;

    [Header("Requirement Data")]
    public RequirementData ciclopeIntellectual;
    public RequirementData estampidaOvejas;
    public RequirementData ciclopeBebe;
    public RequirementData muchoPolvo;
    public RequirementData interiorCueva;
    
    [Header("GameObjects References")]
    public GameObject orderBocadillo;
    public GameObject bag;
    [SerializeField] private TutorialHint objectHint1;
    [SerializeField] private TutorialHint objectHint2;



    public ManualUI manualUI;

    private int lastManualPageIndex = 0;

    
    public enum TutorialState
    {
        StartTutorial,
        Introduction,
        PlayerMovement,
        Interaccion, // agafar els dos objectes ideals de la primera comanda
        Inventario,
        PrimerCliente, // la seva comanda es ciclope intelectual y estampida de ovejas, explicacio de com funciona el temps de desesperacio
        EntregaPedido, // explicacio de entrega de comanda y entrega dels dos primers objectes
        Manual,
        SegundoCliente, // la seva comanda es ciclope bebe mucho polvo y interior cueva
        FacturaDiaria, // foto resultscene  
        FinTutorial // final del tutorial
    }
    public TutorialState currentState = TutorialState.StartTutorial;

    public void StartTalking() => SetTalkingState(true);
    public void StopTalking() => SetTalkingState(false);

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // IMPORTANTE: Forzar el estado a Tutorial INMEDIATAMENTE
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameState.Tutorial);
            Debug.Log("<color=cyan>✓ TutorialManager: Estado cambiado a Tutorial</color>");
        }
        
        // Obtener referencia al controlador del perro
        if (tutorialDog != null)
        {
            dogController = tutorialDog.GetComponent<TutorialDog>();
            dogAnimator = tutorialDog.GetComponent<Animator>();
            if(dogAnimator == null)
            {
                Debug.LogError("<color=red>✗ ERROR: El Tutorial Dog NO tiene un Animator asignado!</color>");
            }
            if (dogController == null)
            {
                dogController = tutorialDog.AddComponent<TutorialDog>();
            }
        }
    }

    void Start()
    {
        Debug.Log("<color=cyan>✓ TutorialManager Start()</color>");
        
        // Conectar el botón de continuar
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueButtonPressed);
            continueButton.gameObject.SetActive(false); // Ocultar al inicio
            Debug.Log("<color=green>✓ Botón de continuar conectado y ocultado</color>");
        }
        else
        {
            Debug.LogError("<color=red>✗ ERROR: continueButton NO está asignado en el Inspector!</color>");
        }
        
        // Ocultar la imagen al inicio
        if (tutorialImage != null)
        {
            tutorialImage.gameObject.SetActive(false);
        }
        
        // Inicializar el tutorial
        InitializeTutorial();
    }

    /// <summary>
    /// Inicializa el tutorial y comienza la secuencia
    /// </summary>
    public void InitializeTutorial()
    {
        Debug.Log("<color=cyan>✓ Iniciando Tutorial...</color>");
        
        // Desactivar todos los sistemas del juego normal
        DisableNormalGameSystems();
        
        // Activar UI deasdasdl tutorial
        if (tutorialUI != null)
        {
            tutorialUI.gameObject.SetActive(true);
        }
        
        // Mostrar el perro
        if (tutorialDog != null)
        {
            tutorialDog.SetActive(true);
        }
        
        // Desactivar Cursor para el modo primera persona
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Comenzar la secuencia del tutorial
        StartCoroutine(RunCompleteTutorial());
    }

    public void SetTalkingState(bool talking)
    {
        if (dogAnimator != null)
        {
            // IMPORTANT: El nom "isTalking" ha de ser idèntic al de l'Animator
            dogAnimator.SetBool("isTalking", talking); 
        }
    }
    /// <summary>
    /// Desactiva los sistemas normales del juego durante el tutorial
    /// </summary>
    private void DisableNormalGameSystems()
    {
        // El ClientManager ya no spawneará automáticamente (verificado en HandleSpawning)
        // El OrderGenerator no generará pedidos automáticos (verificado en Update)
        // Los controles están limitados por las flags canPlayerMove, canPlayerInteract, etc.
    }

    /// <summary>
    /// Corrutina principal que ejecuta todo el tutorial en secuencia
    /// </summary>
    private IEnumerator RunCompleteTutorial()
    {
        yield return StartCoroutine(FirstTutorialPass());
        yield return StartCoroutine(SecondTutorialPass());
        yield return StartCoroutine(ThirdTutorialPass());
        yield return StartCoroutine(FifthTutorialPass());
        yield return StartCoroutine(SixthTutorialPass());
        yield return StartCoroutine(SeventhTutorialPass());
        yield return StartCoroutine(EighthTutorialPass());
        yield return StartCoroutine(NinthTutorialPass());
        yield return StartCoroutine(TenthTutorialPass());
        
        // Tutorial completado
        CompleteTutorial();
    }

    /// <summary>
    /// Finaliza el tutorial y activa el modo de juego normal
    /// </summary>
    private void CompleteTutorial()
    {
        playerHasDoneTutorial = true;
        
        // Ocultar UI del tutorial
        if (tutorialUI != null)
        {
            tutorialUI.gameObject.SetActive(false);
        }
        
        // Ocultar el perro
        if (tutorialDog != null)
        {
            tutorialDog.SetActive(false);
        }
        
        // Habilitar todos los controles
        canPlayerMove = true;
        canPlayerMoveCamera = true;
        canPlayerInteract = true;
        canPlayerUseInventory = true;
        canPlayerOpenManual = true;
        
        // Cambiar al modo de juego normal
        GameManager.Instance.ChangeState(GameState.Playing);
        
        Debug.Log("<color=green>✓ Tutorial completado!</color>");
    }

    // Update is called once per frame
    void Update()
    {
        // Permitir continuar con tecla ESPACIO o ENTER como alternativa
        if (isWaitingContinueButton && Input.GetKeyDown(KeyCode.Return))
        {
            ContinueButtonPressed();
        }
    }

    public void SetTutorialState(TutorialState newState)
    {
        currentState = newState;
    }

    public void ContinueButtonPressed()
    {
        Debug.Log("<color=green>✓ Botón de continuar presionado</color>");
        if (isWaitingContinueButton)
        {
            isWaitingContinueButton = false;
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Muestra el botón de continuar y espera a que se presione
    /// </summary>
    private IEnumerator WaitForContinueButton()
    {
        Debug.Log("<color=yellow>⏸ WaitForContinueButton: Esperando input del jugador...</color>");
        
        // Desbloquear cursor para poder hacer clic, ponerlo donde esta el continueButton
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        canPlayerMove = false;
        canPlayerMoveCamera = false;

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
            Debug.Log("<color=yellow>✓ Botón de continuar activado</color>");
        }
        else
        {
            Debug.LogError("<color=red>✗ ERROR: continueButton es NULL! Asígnalo en el Inspector</color>");
        }
        
        isWaitingContinueButton = true;
        
        // IMPORTANTE: Esperar hasta que isWaitingContinueButton sea false
        // Usar WaitForSecondsRealtime para que funcione aunque Time.timeScale = 0
        while (isWaitingContinueButton)
        {
            yield return null; // Esperar un frame (funciona incluso con timeScale = 0)
        }
        
        Debug.Log("<color=green>✓ Jugador continuó, siguiente paso...</color>");
        
        // Bloquear cursor de nuevo para el gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(ContinueButtonPressed);
        }
    }

    
    public IEnumerator FirstTutorialPass()
    {
        SetTutorialState(TutorialState.Introduction);
        
        StartTalking();
        tutorialText.text = "A dalt, gandul! Benvingut a l'Agència de Venda d'Odissees, l'imperi viral d'Ulisses. Anit et vas beure fins a l'aigua dels florers en l'Oasi.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        yield return StartCoroutine(WaitForContinueButton());

        StartTalking();
        tutorialText.text = "La broma et surt per 250 monedes. Ulisses és un tiu raonable, treballa en l'Agència per a pagar-la o seràs executat en acabar el dia. Tu tries.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        yield return StartCoroutine(WaitForContinueButton());

        StartTalking();
        tutorialText.text = "A dalt a l'esquerra sempre pots observar el que et queda per pagar.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        yield return StartCoroutine(WaitForContinueButton());

    }

    public IEnumerator SecondTutorialPass()
    {
        SetTutorialState(TutorialState.PlayerMovement);
        
          // El perro vuela detrás del jugador
        StartTalking();
        tutorialText.text = "Comencem pel bàsic perquè t'acostumis al lloc. Segueix-me.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        if (dogController != null && dogTransforms.Length > 0)
        {
            dogController.MoveTo(dogTransforms[0].position);
            yield return new WaitUntil (() => isDoginPlace(dogTransforms[0]) == true); // Posición detrás del jugador
        }
        dogController.LookAt(playerPosition);
        
        //yield return StartCoroutine(WaitForContinueButton());
        
        // Pop Up Imagen de controles de cámara
        if (cameraSprite != null)
        {
            tutorialImage.sprite = cameraSprite;
            tutorialImage.gameObject.SetActive(true);
        }
        
        canPlayerMoveCamera = true;
        yield return new WaitUntil(() => isPlayerLooking(tutorialDog) == true); 

        if (dogController != null && dogTransforms.Length > 0)
        {
            dogController.MoveTo(dogTransforms[1].position);
            yield return new WaitUntil (() => isDoginPlace(dogTransforms[1]) == true);
        }
        dogController.LookAt(playerPosition);

        // canPlayerMoveCamera = false;
        // StartTalking();
        // tutorialText.text = "Empecemos por lo básico para que te acostumbres al lugar. Acércate a mí.";
        // yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        // StopTalking();
        // //yield return StartCoroutine(WaitForContinueButton());
        // // Pop Up Imagen de controles de movimiento
        if (movementSprite != null)
        {
            tutorialImage.sprite = movementSprite;
            tutorialImage.gameObject.SetActive(true);
        }
        canPlayerMoveCamera = true;
        
        canPlayerMove = true;
        yield return new WaitUntil(() => playerInZone(playerTransforms[0].position));
        canPlayerMove = false;
        
      
        tutorialImage.gameObject.SetActive(false);
    }

    public IEnumerator ThirdTutorialPass()
    {
        SetTutorialState(TutorialState.Interaccion);
        
        if (interactionSprite != null)
        {
            tutorialImage.sprite = interactionSprite;
            tutorialImage.gameObject.SetActive(true);
        }

        canPlayerMoveCamera = false;
        StartTalking();
        tutorialText.text = "Acosta't a aquest prestatge i agarra l'Odre.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        tutorialImage.gameObject.SetActive(false);
        //yield return StartCoroutine(WaitForContinueButton());
        // Pop Up Imagen de interacción
        objectHint1.ShowHint();
        if (interactionSprite != null)
        {
            tutorialImage.sprite = interactionSprite;
            tutorialImage.gameObject.SetActive(true);
        }

        SetAllowedObjectTypes(new ObjectType[] { ObjectType.Odre }, true);
        
        canPlayerMoveCamera = true;
        canPlayerMove = true;
        canPlayerInteract = true;


        
        yield return new WaitUntil(() => playerTakeObject(ObjectType.Odre));
        objectHint1.HideHint();
        tutorialImage.gameObject.SetActive(false);

        RemoveObjectTypeRestriction();

        canPlayerInteract = false;
        tutorialImage.gameObject.SetActive(false);
        canPlayerMoveCamera = false;
        StartTalking();
        tutorialText.text = "Bé. Escolta que això és important. Les eines es divideixen en tres tipus";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        yield return StartCoroutine(WaitForContinueButton());

        StartTalking();
        tutorialText.text = "Cadascun dels tipus té el seu propi prestatge. Els pots diferenciar per les Icones.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        yield return StartCoroutine(WaitForContinueButton());
        tutorialImage.gameObject.SetActive(false);

        // Pop Up Imagen del inventario
        if (inventorySprite != null)
        {
            tutorialImage.sprite = inventorySprite;
            tutorialImage.gameObject.SetActive(true);
        }
        
        StartTalking();
        tutorialText.text = "A baix a la dreta tens les butxaques, cada objecte ocupa una ranura. A més pots intercanviar de ranures, anem, prova-ho.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        yield return StartCoroutine(WaitForContinueButton());
        tutorialImage.gameObject.SetActive(false);
        canPlayerMove = false;
        canPlayerMoveCamera = false;
        //yield return StartCoroutine(WaitForContinueButton());
        
        canPlayerUseInventory = true;
        yield return new WaitUntil(() => usedWheelInInventory());
        
        tutorialImage.gameObject.SetActive(false);
        
        canPlayerMove = false;
        canPlayerMoveCamera = false;
        
        if (interactionSprite != null)
        {
            tutorialImage.sprite = interactionSprite;
            tutorialImage.gameObject.SetActive(true);
        }
        
        InstanceClient(0);
        StartTalking();
        tutorialText.text = "Ara agafa l'Arc. Els objectes trigaran a tornar a estar disponibles una vegada agafats, tingues-ho en compte.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();


        objectHint2.ShowHint();
        
        // El perro se mueve al segundo objeto
        if (dogController != null && dogTransforms.Length > 2)
        {
            dogController.MoveTo(dogTransforms[2].position);
            yield return new WaitUntil (() => isDoginPlace(dogTransforms[2]) == true);
        }
        dogController.LookAt(playerPosition);
        
        SetAllowedObjectTypes(new ObjectType[] { ObjectType.Arco }, true);
                tutorialImage.gameObject.SetActive(false);
        //yield return StartCoroutine(WaitForContinueButton());

        canPlayerMoveCamera = true;
        canPlayerMove = true;
        canPlayerUseInventory = true;
        yield return new WaitUntil(() => playerInZone(playerTransforms[1].position));   
        canPlayerInteract = true;
        
        // NUEVA CONDICIÓN: Solo permitir coger el objeto si NO está en el slot 1 (índice 0)
        yield return new WaitUntil(() => playerTakeObject(ObjectType.Arco) && InventoryManager.Instance.currentSlotIndex > 0);
        
        objectHint2.HideHint();
        RemoveObjectTypeRestriction();

        canPlayerMove = false;
        canPlayerMoveCamera = false;

    }

    public IEnumerator FifthTutorialPass()
    {
        SetTutorialState(TutorialState.PrimerCliente);
        // sonido de campana
        canPlayerInteract = false;
        StartTalking();
        tutorialText.text = "Ha arribat el teu primer client. Ves a atendre-li. El client sempre et demanarà de dos a tres objectes de categories diferents.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        //yield return StartCoroutine(WaitForContinueButton());
        
        canPlayerMove = true;
        canPlayerMoveCamera = true;

        dogController.MoveTo(dogTransforms[3].position);
        yield return new WaitUntil (() => isDoginPlace(dogTransforms[3]) == true);
        dogController.LookAt(playerPosition);

        yield return new WaitUntil(() => playerInZone(playerTransforms[2].position));
        canPlayerMove = false;
        canPlayerMoveCamera = false;

        // Generar pedido específico del tutorial
        canGenerateOrder = true;
        isWaitingForFirstClientOrder = true;
        orderHasBeenShown = false;
        CreateClientOrder(0, ciclopeIntellectual, estampidaOvejas, null);
        
        // Esperar a que el pedido se haya creado y mostrado completamente
        yield return new WaitUntil(() => orderHasBeenShown);
        yield return new WaitForSeconds(0.5f); // Breve espera para que aparezca el bocadillo

        
        isWaitingForFirstClientOrder = false;
        
        StartTalking();
        tutorialText.text = "Sempre que la comanda estigui activa veuràs les condicions específiques en una nota en la dreta.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        yield return StartCoroutine(WaitForContinueButton());
        tutorialImage.gameObject.SetActive(false);

        // Pop Up Imagen de la desesperación
        if (desperationSprite != null)
        {
            tutorialImage.sprite = desperationSprite;
            tutorialImage.gameObject.SetActive(true);
        }
        
        StartTalking();
        tutorialText.text = "Aquesta roda de colors és el seu nivell de desesperació. Si arriba a vermell, s'enfaden. Si arriba a zero, es van sense cap mena d'equip.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        yield return StartCoroutine(WaitForContinueButton());
        tutorialImage.gameObject.SetActive(false);

        StartTalking();
        tutorialText.text = "Tingues en compte que només pots atendre un màxim de tres clients alhora.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        yield return StartCoroutine(WaitForContinueButton());

    }

        public IEnumerator SixthTutorialPass()
    {
        SetTutorialState(TutorialState.Manual);
        
        if(manualSprite != null)
        {
            tutorialImage.sprite = manualSprite;
            tutorialImage.gameObject.SetActive(true);
        }

        canPlayerInteract = false;
        StartTalking();
        tutorialText.text = "Per a poder saber què és el millor per a cada situació tens el manual. Obre-ho.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        
        canPlayerOpenManual = true;  
        isWaitingForManualOpen = true;
        yield return new WaitUntil(() => playerOpenManual() == true);

        tutorialImage.gameObject.SetActive(false);
        isWaitingForManualOpen = false;
        canPlayerCloseManual = false;  
        
        StartTalking();
        tutorialText.text = "Cada entrada del manual està dedicada a una activitat, amb les seves tres categories. Pots diferenciar les activitats ràpidament pel Monstre a derrotar.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        yield return StartCoroutine(WaitForContinueButton());

        StartTalking();
        tutorialText.text = "Aquí veuràs que objectes són exactament els necessaris per a les condicions que et demana el client.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        yield return StartCoroutine(WaitForContinueButton());


        StartTalking();
        tutorialText.text = "Per a cada situació específica et sortiran tres objectes amb la seva utilitat a la dreta.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        yield return StartCoroutine(WaitForContinueButton());

        StartTalking();
        tutorialText.text = "Depenent quin triïs el client tindrà més o menys probabilitats de sortir victoriós.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        yield return StartCoroutine(WaitForContinueButton()); 

        if(manualSprite != null)
        {
            tutorialImage.sprite = manualSprite;
            tutorialImage.gameObject.SetActive(true);
        }

        StartTalking();
        tutorialText.text = "Casualment, els objectes de les teves butxaques són just el que el client vol. Tanca el manual.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        
        canPlayerCloseManual = true; 
        isWaitingForManualClose = true;
        yield return new WaitUntil(() => playerCloseManual() == true);
        tutorialImage.gameObject.SetActive(false);
        isWaitingForManualClose = false;

        canPlayerMove = true;
        canPlayerInteract = false;
        canPlayerMoveCamera = true;
    }

    public IEnumerator SeventhTutorialPass()
    {
        SetTutorialState(TutorialState.EntregaPedido);
        StartTalking();
        tutorialText.text = "El client ha deixat una motxilla.En aquesta motxilla on posaràs els objectes.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        isPlayerLookingAt = false;
        canPlayerMoveCamera = true;
        yield return new WaitUntil(() => isPlayerLooking(bag) == true);

        StartTalking();
        tutorialText.text = "Agafa un dels objectes de les teves butxaques i col·loca-ho dins. Tingues en compte que una vegada col·locat l'objecte no hi ha marxa enrere.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        canPlayerInteract = true;
        canPlayerMove = true;

        // Esperar a que se entregue el primer objeto (cualquiera de los dos)
        yield return new WaitUntil(() => GetDeliveredItemsCount() >= 1);
        
        canPlayerInteract = false;
        canPlayerMove = false;

        StartTalking();
        tutorialText.text = "Perquè la comanda sigui completada has de col·locar els mateixos objectes que especificacions et demani el client.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        yield return StartCoroutine(WaitForContinueButton());

        canPlayerInteract = true;
        canPlayerMoveCamera = true;
        canPlayerMove = true;

        // Esperar a que el pedido se complete (el cliente desaparezca o el OrderSystem lo procese)
        bool orderCompleted = false;
        StartCoroutine(WaitForOrderCompletion(() => orderCompleted = true));
        yield return new WaitUntil(() => orderCompleted);
        
        canPlayerInteract = false;
        canPlayerMove = false;
        
        Debug.Log("<color=green>✓ SeventhTutorialPass completado - Pedido completado</color>");
    }

    public IEnumerator EighthTutorialPass()
    {
        SetTutorialState(TutorialState.SegundoCliente);

        // Instanciar el segundo cliente en el slot 1
        InstanceClient(1);
        
        StartTalking();
        tutorialText.text = "Espera un moment! Ja que ve un altre client aprofita per practicar i fes la seva comanfa pel teu compte.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();

        yield return StartCoroutine(WaitForContinueButton());

        yield return new WaitUntil(() => IsClientInPosition(1));
        
        isWaitingForManualOpen = true;
        canPlayerMove = true;
        canPlayerMoveCamera = true;
        canPlayerOpenManual = true;
        canPlayerChangePage = true;

        // NUEVO: Mostrar el mensaje
        StartTalking();
        tutorialText.text = "Apropat per rebre la comanda";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        // NUEVO: Esperar a que el jugador se acerque
        yield return new WaitUntil(() => playerInZone(playerTransforms[3].position));
        textPanel.SetActive(false);
        
        canPlayerMove = false;
        canPlayerInteract = false;

        // Ahora sí, crear el pedido cuando tanto el cliente como el jugador están en posición
        isWaitingForSecondClientOrder = true;
        orderHasBeenShown = false;
        CreateClientOrder(1, ciclopeBebe, muchoPolvo, interiorCueva);
        

        // Esperar a que el pedido se haya creado y mostrado completamente
        yield return new WaitUntil(() => orderHasBeenShown);
        yield return new WaitForSeconds(3f);

        SetAllowedObjectTypes(new ObjectType[] { 
            ObjectType.Espejo, 
            ObjectType.Mascaras, 
            ObjectType.CascoA 
        }, true);
        
        isWaitingForSecondClientOrder = false;
        
        canPlayerMove = true;




        canPlayerMoveCamera = true;
        canPlayerInteract = true;
        canPlayerChangePage = true;
        canPlayerOpenManual = true;
        canPlayerUseInventory = true;
        
        // Esperar a que se complete el segundo pedido
        bool secondOrderCompleted = false;
        StartCoroutine(WaitForOrderCompletion(() => secondOrderCompleted = true));
        yield return new WaitUntil(() => secondOrderCompleted);
        
        RemoveObjectTypeRestriction();

        canPlayerMove = false;
        canPlayerMoveCamera = false;
        canPlayerInteract = false;
        textPanel.SetActive(true);

        StartTalking();
        tutorialText.text = "Ben fet, has estat capaç de completar la comanda pel teu compte.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        yield return StartCoroutine(WaitForContinueButton());
        isWaitingForManualClose = true;

        StartTalking();
        tutorialText.text = "El dia es donarà per acabat si et quedes sense clients o si s'acaba el dia. Pots veure quant queda sota el deute a l'esquerra.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        yield return StartCoroutine(WaitForContinueButton());

        Debug.Log("<color=green>✓ EighthTutorialPass completado</color>");
    }





    public IEnumerator NinthTutorialPass()
    {
        SetTutorialState(TutorialState.FacturaDiaria);
        canPlayerMoveCamera = true;
        StartTalking();
        tutorialText.text = "Felicitats! Has completat el teu primer día en la Agencia. A ver com t'ha anat.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        yield return StartCoroutine(WaitForContinueButton());

        //Enseñamos imagen de la factura diaria
        tutorialDebtText.gameObject.SetActive(false);
        tutorialImage.gameObject.SetActive(false);
        timeSlider.gameObject.SetActive(false);
        inventoryUI.gameObject.SetActive(false);  
        resultCanvas.SetActive(true);
        
        StartTalking();
        tutorialText.text = "Això d'aquí és la factura del dia. Aquí podràs veure els fruits del teu rendiment durant el dia.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        yield return StartCoroutine(WaitForContinueButton());

        StartTalking();
        tutorialText.text = "En la columna esquerra tens els ingressos. Aquí afectarà quants objectes hagis venut als clients. I quants d'aquests clients han tingut èxit.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        yield return StartCoroutine(WaitForContinueButton());

        StartTalking();
        tutorialText.text = "Tingues en compte que quant millors siguin els objectes per a la missió del client més et pagaran per ells.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        yield return StartCoroutine(WaitForContinueButton());

        StartTalking();
        tutorialText.text = "En la columna del mitjà tens les despeses. Aquí es tenen en compte quants clients han fallat la seva missió i el cost per reposar cada objecte venut.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        yield return StartCoroutine(WaitForContinueButton());

        StartTalking();
        tutorialText.text = "Finalment en la Columna de la dreta tens el total. Sempre que el total sigui positiu una part d'ell anirà a pagar el teu deute. En cas que sigui negatiu, bo, ja saps el que passarà.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        yield return StartCoroutine(WaitForContinueButton());

        resultCanvas.SetActive(false);
        tutorialDebtText.gameObject.SetActive(true);
        timeSlider.gameObject.SetActive(true);
        inventoryUI.gameObject.SetActive(true); 
        canPlayerMoveCamera = true;
    }

    public IEnumerator TenthTutorialPass()
    {
        SetTutorialState(TutorialState.FinTutorial);
        
        canPlayerMoveCamera = true;
        StartTalking();
        tutorialText.text = "En fi, aquest és tot el meu treball per avui, que Ulisses no paga a les mainaders.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        yield return StartCoroutine(WaitForContinueButton());

        dogController.MoveTo(dogTransforms[4].position);
        yield return new WaitUntil (() => isDoginPlace(dogTransforms[4]) == true);
        dogController.LookAt(playerPosition);

        StartTalking();
        tutorialText.text = "Si em veus per aquí serà en la meva caseta que està en la paret del fons. Encara que més et val no veure'm perquè si aparec serà per a avisar-te que un dels clients ha mort.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        yield return StartCoroutine(WaitForContinueButton());

        StartTalking();
        tutorialText.text = "Per a passar al següent dia dona-li al botó de baix que posa anar al següent dia.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        StopTalking();
        yield return StartCoroutine(WaitForContinueButton());

        playerHasDoneTutorial = true;

    }

    public bool playerInZone( Vector3 position)
    {
        return Vector3.Distance(playerPosition.position, position) < 2.0f;
    }

    public bool playerTakeObject( ObjectType item)
    {
        if (InventoryManager.Instance.GetCurrentObjectType() != item)
        {
            return false;
        }else
        return true;
    }

    private IEnumerator WaitForOrderCompletion(System.Action onComplete)
    {
        int initialActiveOrders = GetActiveOrdersCount();
        
        // Esperar a que la cantidad de pedidos activos disminuya (significa que uno se completó)
        yield return new WaitUntil(() => GetActiveOrdersCount() < initialActiveOrders);
        
        Debug.Log("<color=green>✓ Pedido completado detectado</color>");
        onComplete?.Invoke();
    }

    

    /// <summary>
    /// Cuenta cuántos pedidos activos hay actualmente
    /// </summary>
    private int GetActiveOrdersCount()
    {
        if (OrderSystem.Instance != null)
        {
            return OrderSystem.Instance.GetActiveClientOrders().Count;
        }
        return 0;
    }

    
    /// <summary>
    /// Cuenta cuántos objetos se han entregado al pedido activo
    /// </summary>
    private int GetDeliveredItemsCount()
    {
        if (OrderSystem.Instance != null)
        {
            var activeOrders = OrderSystem.Instance.GetActiveClientOrders();
            foreach (var clientOrder in activeOrders)
            {
                return clientOrder.order.deliveredItems.Count;
            }
        }
        return 0;
    }

    public bool usedWheelInInventory()
    {
        if(InputManager.Instance.MouseScrollDelta == 0)
        {
            return false;
        }
        else
        return true;
    }

    /// <summary>
    /// Verifica si un cliente está en su posición asignada en un slot específico
    /// </summary>
    public bool IsClientInPosition(int slotIndex)
    {
        GameObject client = ClientManager.Instance.GetClientInSlot(slotIndex);
        
        if (client == null)
        {
            Debug.LogWarning($"<color=yellow>Cliente en slot {slotIndex} no existe aún</color>");
            return false;
        }
        
        // Obtener la posición objetivo del slot desde el ClientManager
        Transform targetPosition = ClientManager.Instance.GetClientSlotPosition(slotIndex);
        
        if (targetPosition == null)
        {
            Debug.LogError($"<color=red>No se pudo obtener la posición del slot {slotIndex}</color>");
            return false;
        }
        
        // Verificar si el cliente está cerca de su posición objetivo
        float distance = Vector3.Distance(client.transform.position, targetPosition.position);
        bool isInPosition = distance < 3f; // Ajusta este valor según el tamaño de tus slots
        
        if (isInPosition)
        {
            Debug.Log($"<color=green>✓ Cliente en slot {slotIndex} ha llegado a su posición</color>");
        }
        
        return isInPosition;
    }
    public bool playerOpenManual()
    {
        // Detecta si el manual está abierto
        if (manualUI != null && manualUI.manualPanel.activeSelf)
        {
            return true;
        }
        return false;
    }

    public bool playerCloseManual()
    {
        if (!canPlayerCloseManual)
        {
            return false;
        }
        // Detecta si el manual está cerrado
        if (manualUI != null && !manualUI.manualPanel.activeSelf)
        {
            return true;
        }
        return false;
    }

    public void CreateClientOrder(int slotIndex, RequirementData monster, RequirementData condition, RequirementData environment)
    {
        // Obtener el cliente del slot desde el ClientManager
        GameObject client = ClientManager.Instance.GetClientInSlot(slotIndex);
        
        if (client == null)
        {
            Debug.LogError($"<color=red>No hay cliente en el slot {slotIndex}!</color>");
            return;
        }
        
        // Generar el pedido a través del OrderSystem para que se muestre correctamente
        if (OrderSystem.Instance != null)
        {
            OrderSystem.Instance.GenerateTutorialOrderForClient(client, slotIndex, monster, condition, environment);
            // El flag orderHasBeenShown se activará después de que el pedido se muestre
            StartCoroutine(WaitForOrderToShow());
        }
        else
        {
            Debug.LogError("<color=red>OrderSystem.Instance es null!</color>");
        }
    }

     /// <summary>
    /// Permite solo coger múltiples tipos de objetos específicos
    /// </summary>
    public void SetAllowedObjectTypes(ObjectType[] objectTypes, bool restricted = true)
    {
        allowedObjectTypes = objectTypes;
        isObjectTypeRestricted = restricted;
        string objectNames = string.Join(", ", allowedObjectTypes);
        Debug.Log($"<color=yellow>📌 Solo se permite coger: {objectNames}</color>");
    }

    /// <summary>
    /// Verifica si el jugador puede coger un objeto específico (versión mejorada)
    /// </summary>
    public bool CanPickupObjectType(ObjectType objectType)
    {
        if (!isObjectTypeRestricted)
        {
            return true; // Si no hay restricción, puede coger cualquiera
        }

        // Si hay un array de tipos permitidos, verificar contra el array
        if (allowedObjectTypes != null && allowedObjectTypes.Length > 0)
        {
            foreach (ObjectType allowed in allowedObjectTypes)
            {
                if (objectType == allowed)
                {
                    return true;
                }
            }
            
            string allowedNames = string.Join(", ", allowedObjectTypes);
            Debug.Log($"<color=red>✗ No puedes coger {objectType}. Solo puedes coger: {allowedNames}</color>");
            return false;
        }

        // Fallback al sistema anterior (un solo objeto permitido)
        if (objectType == allowedObjectType)
        {
            return true;
        }

        Debug.Log($"<color=red>✗ No puedes coger {objectType}. Solo puedes coger: {allowedObjectType}</color>");
        return false;
    }

    /// <summary>
    /// Quita la restricción de tipo de objeto
    /// </summary>
    public void RemoveObjectTypeRestriction()
    {
        isObjectTypeRestricted = false;
        Debug.Log("<color=green>✓ Restricción de objeto removida</color>");
    }

    
    private IEnumerator WaitForOrderToShow()
    {
        // Esperar un frame para que el pedido se cree
        yield return null;
        orderHasBeenShown = true;
    }

    public bool isPlayerLooking(GameObject target)
    {
        // Verificar primero si el ángulo es razonable
        Vector3 directionToTarget = target.transform.position - Camera.main.transform.position;
        float angle = Vector3.Angle(Camera.main.transform.forward, directionToTarget);
    
        if (angle > 30f) // Si está fuera del ángulo, no está mirando
        {
            return false;
        }

        // Hacer raycast desde la cámara hacia el centro de la pantalla
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(UnityEngine.Screen.width / 2, UnityEngine.Screen.height / 2, 0));
        RaycastHit hit;

        // Realizar el raycast (ajusta la distancia según necesites)
        if (Physics.Raycast(ray, out hit, 100f))
        {
            // Verificar si el objeto golpeado es el target o un hijo del target
            if (hit.collider.gameObject == target || hit.collider.transform.IsChildOf(target.transform)||hit.collider.gameObject.layer==target.layer)
            {
                return true;
            }
        }

        return false;
    }

    public bool CanChangeToSlot(int slotIndex)
    {
        // Si no estamos en tutorial, permitir cambio normal
        if (GameManager.Instance.CurrentState != GameState.Tutorial)
        {
            return true;
        }
        
        // Si estamos en el paso del segundo objeto del tutorial
        if (currentState == TutorialState.Interaccion && !playerTakeObject(ObjectType.Arco))
        {
            // BLOQUEAR el slot 1 (índice 0) hasta que se recoja el Arco
            if (slotIndex == 0 && playerTakeObject(ObjectType.Odre))
            {
                Debug.Log("<color=red>⚠ No puedes usar el bolsillo 1 para recoger el segundo objeto. Usa el bolsillo 2 o 3.</color>");
                return false;
            }
        }
        
        return true;
    }
    
    public bool isDoginPlace(Transform dogTransform)
    {
        float distance = Vector3.Distance(tutorialDog.transform.position, dogTransform.position);
        return distance < 1f; // Ajusta el umbral según sea necesario
    }

    public bool manualPageChanged()
    {
        if (manualUI != null && manualUI.pageHasChanged)
        {
            manualUI.pageHasChanged = false; 
            canPlayerChangePage = false;
            return true;
        }
        
        return false;
    }

    // voids
    public void InstanceClient(int slotIndex)
    {
        ClientManager.Instance.SpawnClientInSlot(slotIndex);
    }
    

}
