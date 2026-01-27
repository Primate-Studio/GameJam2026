using System;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEditor.UIElements;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Unity.Android.Gradle.Manifest;
using NUnit.Framework;

public class TutorialManager : MonoBehaviour
{
    
    public static TutorialManager Instance { get; private set; }

    [Header("Tutorial Dog")]
    public GameObject tutorialDog;
    private TutorialDog dogController;

    [Header("Tutorial UI")]
    public Canvas tutorialUI;
    public UnityEngine.UI.Image tutorialImage;
    public TextMeshProUGUI tutorialText;
    public TextMeshProUGUI tutorialDebtText;
    public Slider timeSlider;
    public Button continueButton;

    [Header("Tutorial Sprites")]
    public Sprite movementSprite;
    public Sprite cameraSprite;
    public Sprite interactionSprite;
    public Sprite inventorySprite;
    public Sprite orderSprite;
    public Sprite orderNoteSprite;
    public Sprite desperationSprite;
    public Sprite manualSprite;
    public Sprite manualPageSprite;
    public Sprite objectTypeSprite;
    public Sprite qualityObjectSprite;
    public Sprite debtSprite;
    public Sprite resultSceneSprite;


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
        
        // Activar UI del tutorial
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
        if (isWaitingContinueButton && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
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
        
        // Desbloquear cursor para poder hacer clic
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
        Debug.Log("<color=cyan>━━━ INICIANDO FIRST TUTORIAL PASS ━━━</color>");
        SetTutorialState(TutorialState.Introduction);
        
        tutorialText.text = "¡Arriba, gandul! Bienvenido al Oasis, el imperio viral de Ulises. Anoche te bebiste hasta el agua de los floreros.";
        yield return StartCoroutine(TypeWritterEffect.TypeText(tutorialText, tutorialText.text, 0.05f));
        Debug.Log("<color=yellow>Mensaje 1 mostrado, esperando continuar...</color>");
        yield return StartCoroutine(WaitForContinueButton());

        tutorialText.text = "Ahora mismo estas trabajando para él en su tienda de objetos para las odiseas. La Odisea de TONI, para ser exactos.";
        Debug.Log("<color=yellow>Mensaje 2 mostrado, esperando continuar...</color>");
        yield return StartCoroutine(WaitForContinueButton());
        
        tutorialText.text = "Pero antes de nada, hablemos de tu deuda con Ulises...";
        Debug.Log("<color=yellow>Mensaje 3 mostrado, esperando continuar...</color>");
        yield return StartCoroutine(WaitForContinueButton());

        tutorialText.text = "La broma te sale por 250 monedas. Ulises es un tipo razonable, trabaja en la Agencia para pagarla o serás ejecutado al acabar el día. Tú eliges.";
        Debug.Log("<color=yellow>Mensaje 4 mostrado, esperando continuar...</color>");
        yield return StartCoroutine(WaitForContinueButton());

        tutorialText.text = "Arriba a la izquierda siempre puedes observar lo que te queda por pagar. ¡Suerte!";
        Debug.Log("<color=yellow>Mensaje 5 mostrado, esperando continuar...</color>");
        yield return StartCoroutine(WaitForContinueButton());

        Debug.Log("<color=cyan>━━━ FIRST TUTORIAL PASS COMPLETADO ━━━</color>");
    }

    public IEnumerator SecondTutorialPass()
    {
        SetTutorialState(TutorialState.PlayerMovement);
        
          // El perro vuela detrás del jugador
        tutorialText.text = "No me pierdas de vista o estaremos aquí todo el día. Mueve el cuello y búscame, estoy volando detrás de ti.";
        if (dogController != null && dogTransforms.Length > 0)
        {
            dogController.MoveTo(dogTransforms[0].position);
            yield return new WaitUntil (() => isDoginPlace(dogTransforms[0]) == true); // Posición detrás del jugador
        }
        dogController.LookAt(playerPosition.position);
        
        yield return StartCoroutine(WaitForContinueButton());
        
        // Pop Up Imagen de controles de cámara
        if (cameraSprite != null)
        {
            tutorialImage.sprite = cameraSprite;
            tutorialImage.gameObject.SetActive(true);
        }
        
        canPlayerMoveCamera = true;
        yield return new WaitUntil(() => isPlayerLooking(tutorialDog) == true); 

        // Pop Up Imagen de controles de movimiento
        if (movementSprite != null)
        {
            tutorialImage.sprite = movementSprite;
            tutorialImage.gameObject.SetActive(true);
        }

        if (dogController != null && dogTransforms.Length > 0)
        {
            dogController.MoveTo(dogTransforms[1].position);
            yield return new WaitUntil (() => isDoginPlace(dogTransforms[1]) == true); // Posición detrás del jugador
        }

        canPlayerMoveCamera = false;
        tutorialText.text = "Empecemos por lo básico para que te acostumbres al lugar. Acércate a mí.";
        yield return StartCoroutine(WaitForContinueButton());
        canPlayerMoveCamera = true;

        tutorialImage.gameObject.SetActive(false);
        
        canPlayerMove = true;
        yield return new WaitUntil(() => playerInZone(playerTransforms[0].position));
        canPlayerMove = false;
        
      
        tutorialImage.gameObject.SetActive(false);
    }

    public IEnumerator ThirdTutorialPass()
    {
        SetTutorialState(TutorialState.Interaccion);
        
        canPlayerMoveCamera = false;
        tutorialText.text = "Empecemos por lo básico. Acércate a ese estante y agarra el Odre.";
        yield return StartCoroutine(WaitForContinueButton());
        // Pop Up Imagen de interacción
        if (interactionSprite != null)
        {
            tutorialImage.sprite = interactionSprite;
            tutorialImage.gameObject.SetActive(true);
        }
        
        canPlayerMoveCamera = true;
        canPlayerMove = true;
        canPlayerInteract = true;
        
        yield return new WaitUntil(() => playerTakeObject(ObjectType.Odre));
        canPlayerInteract = false;
        tutorialImage.gameObject.SetActive(false);
        canPlayerMoveCamera = false;
        tutorialText.text = "Bien. Escucha que esto es importante. En la Agencia vendemos imitaciones baratas de armas divinas.";
        yield return StartCoroutine(WaitForContinueButton());

        tutorialText.text = "Se dividen en tres tipos: Ataque para los monstruos, Adaptabilidad para las condiciones raras y Utilidad para el entorno.";
        yield return StartCoroutine(WaitForContinueButton());
        
        // Pop Up Imagen de tipos de objetos
        if (objectTypeSprite != null)
        {
            tutorialImage.sprite = objectTypeSprite;
            tutorialImage.gameObject.SetActive(true);
        }

        tutorialText.text = "Cada uno de los tipos tiene su propio estante. Los puedes diferenciar por los Iconos.";
        yield return StartCoroutine(WaitForContinueButton());
        tutorialImage.gameObject.SetActive(false);

        // Pop Up Imagen del inventario
        if (inventorySprite != null)
        {
            tutorialImage.sprite = inventorySprite;
            tutorialImage.gameObject.SetActive(true);
        }
        

        tutorialText.text = "Ahora, fíjate en la parte de abajo. Eso de ahí son tus bolsillos, cada objeto ocupa una ranura en tus bolsillos.";
        yield return StartCoroutine(WaitForContinueButton());

        canPlayerMove = false;
        canPlayerMoveCamera = false;
        tutorialText.text = "Puedes intercambiar de ranuras, vamos, pruébalo.";
        yield return StartCoroutine(WaitForContinueButton());
        
        canPlayerUseInventory = true;
        yield return new WaitUntil(() => usedWheelInInventory());
        
        tutorialImage.gameObject.SetActive(false);
        
        canPlayerMove = false;
        canPlayerMoveCamera = false;
        
        InstanceClient(0);
        tutorialText.text = "¡Bien! Ahora ve a por ese otro objeto, poniendolo en otro de tus bolsillos.";
        // El perro se mueve al segundo objeto
        if (dogController != null && dogTransforms.Length > 2)
        {
            dogController.MoveTo(dogTransforms[2].position);
        }
        
        yield return StartCoroutine(WaitForContinueButton());

        canPlayerMoveCamera = true;
        canPlayerMove = true;
        yield return new WaitUntil(() => playerInZone(playerTransforms[1].position));
        canPlayerInteract = true;
        yield return new WaitUntil(() => playerTakeObject(ObjectType.Arco));

        canPlayerMove = false;
        canPlayerMoveCamera = false;
        tutorialText.text = "Ojo, aquí las cosas parecen infinitas, pero reponerlas cuesta dinero. Ten en cuenta que una vez cojas un objeto este tardará en aparecer.";
        yield return StartCoroutine(WaitForContinueButton());

    }


    public IEnumerator FifthTutorialPass()
    {
        SetTutorialState(TutorialState.PrimerCliente);
        // sonido de campana
        tutorialText.text = "¿Oyes eso? Ha llegado tu primer cliente. Ve a atenderle.";
        yield return StartCoroutine(WaitForContinueButton());
        
        canPlayerMove = true;
        canPlayerMoveCamera = true;

        dogController.MoveTo(dogTransforms[3].position);

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
        
        // PAUSAR EL TIEMPO para que el bocadillo no desaparezca y se pueda ver bien
        PauseGameTime();
        
        isWaitingForFirstClientOrder = false;

        // Pop Up Imagen del bocadillo de pedido
        if (orderSprite != null)
        {
            tutorialImage.sprite = orderSprite;
            tutorialImage.gameObject.SetActive(true);
        }
        
        tutorialText.text = "No te va a decir, quiero una espada. Te dirá qué Monstruo quiere matar, qué Condición hay y dónde está. Recuerda los iconos de cada categoría.";
        isWaitingContinueButton = true;
        yield return StartCoroutine(WaitForContinueButton());
        
        tutorialText.text = "Fíjate bien en el pedido encima de su cabeza.";
        isPlayerLookingAt = false;
        canPlayerMoveCamera = true;
        yield return new WaitUntil(() => isPlayerLooking(orderBocadillo) == true);
        tutorialImage.gameObject.SetActive(false);
        
        // REANUDAR EL TIEMPO después de explicar el bocadillo
        ResumeGameTime();

        // Pop Up Imagen de la nota de pedido
        if (orderNoteSprite != null)
        {
            tutorialImage.sprite = orderNoteSprite;
            tutorialImage.gameObject.SetActive(true);
        }
        
        tutorialText.text = "Siempre que el pedido esté activo verás las condiciones específicas en una nota en la derecha.";
        yield return StartCoroutine(WaitForContinueButton());
        tutorialImage.gameObject.SetActive(false);

        // Pop Up Imagen de la desesperación
        if (desperationSprite != null)
        {
            tutorialImage.sprite = desperationSprite;
            tutorialImage.gameObject.SetActive(true);
        }
        
        tutorialText.text = "Ese reloj de colores es su nivel de desesperación. Si llega a rojo, se enfadan. Si llega a cero, se enfrentará a una muerte segura.";
        yield return StartCoroutine(WaitForContinueButton());

        tutorialText.text = "Eso solo significa una cosa: perder dinero. Así que date prisa en atenderles.";
        yield return StartCoroutine(WaitForContinueButton());
        tutorialImage.gameObject.SetActive(false);

        tutorialText.text = "Ten en cuenta que solo puedes atender a un máximo de tres clientes a la vez.";
        yield return StartCoroutine(WaitForContinueButton());

        tutorialText.text = "Así que no te emociones que no eres un pulpo.";
        yield return StartCoroutine(WaitForContinueButton());

    }

    public IEnumerator SixthTutorialPass()
    {
        SetTutorialState(TutorialState.Manual);
        
        tutorialText.text = "No te asustes todavía, para poder saber qué es lo mejor para cada situación tienes el manual. Abrelo.";
        yield return StartCoroutine(WaitForContinueButton());
        isWaitingForManualOpen = true;
        yield return new WaitUntil(() => playerOpenManual() == true);
        isWaitingForManualOpen = false;
        canPlayerInteract = false;
        canPlayerCloseManual = true;
        isWaitingForManualClose = true;
        tutorialText.text = "Ten en cuenta que mientras tengas el manual abierto el tiempo seguirá corriendo igual, por lo que cuando antes te acostumbre mejor.";
        yield return StartCoroutine(WaitForContinueButton());

        tutorialText.text = "Cada entrada del manual está dividida por la categoría que te he explicado antes.";
        yield return StartCoroutine(WaitForContinueButton());

        tutorialText.text = "Prueba a pasar de página y verlo por ti mismo.";
        yield return StartCoroutine(WaitForContinueButton());
        
        canPlayerChangePage = true;
        yield return new WaitUntil(() => manualPageChanged());

        tutorialText.text = "Aquí verás que items son exactamente los necesarios para las condiciones específicas de la aventura que te está pidiendo el cliente.";
        yield return StartCoroutine(WaitForContinueButton());

        tutorialText.text = "Si te fijas la nota sigue activa incluso con el manual abierto.";
        yield return StartCoroutine(WaitForContinueButton()); 

        tutorialText.text = "Para cada situación específica te saldrán tres objetos debajo con su utilidad a la derecha.";
        yield return StartCoroutine(WaitForContinueButton());

        tutorialText.text = "Dependiendo cual elijas el cliente tendrá más o menos probabilidades de salir victorioso.";
        yield return StartCoroutine(WaitForContinueButton());

        tutorialText.text = "Recuerda que los objetos no son eternos. Si cojes uno tardará en aparecer otro igual.";
        yield return StartCoroutine(WaitForContinueButton());
        canPlayerChangePage = true;
        canPlayerCloseManual= true;

        tutorialText.text = "Cuando termines cierra el manual.";
        isWaitingForManualClose = true;
        yield return new WaitUntil(() => playerCloseManual() == true);
        isWaitingForManualClose = false;

        canPlayerMove = true;
        canPlayerInteract = true;
        canPlayerMoveCamera = true;
        tutorialText.text = "Casualmente los objetos de tus bolsillos son justo lo que el cliente quiere. Ve a entregárselos.";
        yield return StartCoroutine(WaitForContinueButton());

    }

    public IEnumerator SeventhTutorialPass()
    {
        SetTutorialState(TutorialState.EntregaPedido);
        tutorialText.text = "Bien, fijate que el cliente cuando ha hecho el pedido ha dejado una mochila. Es en esa mochila donde pondremos los objetos.";
        isPlayerLookingAt = false;
        canPlayerMoveCamera = true;
        yield return new WaitUntil(() => isPlayerLooking(bag) == true);

        tutorialText.text = "Coge uno de los objetos de tus bolsillos y colócalo dentro. Ten en cuenta que una vez colocado el objeto no hay vuelta atrás.";
        canPlayerInteract = true;
        canPlayerMove = true;

        // Esperar a que se entregue el primer objeto (cualquiera de los dos)
        yield return new WaitUntil(() => GetDeliveredItemsCount() >= 1);
        
        canPlayerInteract = false;
        canPlayerMove = false;

        tutorialText.text = "Para que el pedido sea completado tienes que colocar los mismos objetos que especificaciones te pida el cliente.";
        yield return StartCoroutine(WaitForContinueButton());
        
        tutorialText.text = "En este caso eran dos así que con dos objetos basta. Ten en cuenta que te pueden venir pedidos de tres especificaciones también.";
        yield return StartCoroutine(WaitForContinueButton());
        
        tutorialText.text = "¡Perfecto! Ahora entrega el segundo objeto así acabamos.";
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
    
    tutorialText.text = "Ahora que ya sabes como va el tema, atiende a tu segundo cliente que ya está llegando.";
    yield return StartCoroutine(WaitForContinueButton());

    // Instanciar el segundo cliente en el slot 1
    InstanceClient(1);
    
    // ESPERAR A QUE EL CLIENTE ESTÉ EN SU POSICIÓN
    tutorialText.text = "Espera un momento mientras el cliente llega a su posición...";
    yield return new WaitUntil(() => IsClientInPosition(1));
    
    canPlayerMove = true;
    canPlayerMoveCamera = true;
    canPlayerOpenManual = true;
    canPlayerChangePage = true;

    
    tutorialText.text = "Acércate al cliente para ver su pedido.";
    yield return StartCoroutine(WaitForContinueButton());
    
    // Esperar a que el jugador se acerque al trigger del cliente
    yield return new WaitUntil(() => playerInZone(playerTransforms[2].position));
    
    canPlayerMove = false;
    canPlayerMoveCamera = false;

    // Ahora sí, crear el pedido cuando tanto el cliente como el jugador están en posición
    isWaitingForSecondClientOrder = true;
    orderHasBeenShown = false;
    CreateClientOrder(1, ciclopeBebe, muchoPolvo, interiorCueva);
    
    // Esperar a que el pedido se haya creado y mostrado completamente
    yield return new WaitUntil(() => orderHasBeenShown);
    yield return new WaitForSeconds(1f);
    
    tutorialText.text = "Este pedido tiene tres especificaciones. Necesitarás entregar tres objetos diferentes.";
    yield return StartCoroutine(WaitForContinueButton());
    
    tutorialText.text = "Consulta el manual, encuentra los objetos correctos y completa este pedido por tu cuenta.";
    yield return StartCoroutine(WaitForContinueButton());
    
    isWaitingForSecondClientOrder = false;
    
    canPlayerMove = true;
    canPlayerMoveCamera = true;
    canPlayerInteract = true;
    canPlayerOpenManual = true;
    canPlayerUseInventory = true;
    
    // Esperar a que se complete el segundo pedido
    bool secondOrderCompleted = false;
    StartCoroutine(WaitForOrderCompletion(() => secondOrderCompleted = true));
    yield return new WaitUntil(() => secondOrderCompleted);
    
    canPlayerMove = false;
    canPlayerMoveCamera = false;
    canPlayerInteract = false;

    tutorialText.text = "Bien hecho, has sido capaz de completar el pedido por tu cuenta.";
    yield return StartCoroutine(WaitForContinueButton());

    tutorialText.text = "Ten en cuenta que el día se dará por terminado si te quedas sin clientes o si se acaba el día. Puedes ver cuánto queda debajo de la deuda a la izquierda.";
    yield return StartCoroutine(WaitForContinueButton());

    Debug.Log("<color=green>✓ EighthTutorialPass completado</color>");
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
    bool isInPosition = distance < 1.5f; // Ajusta este valor según el tamaño de tus slots
    
    if (isInPosition)
    {
        Debug.Log($"<color=green>✓ Cliente en slot {slotIndex} ha llegado a su posición</color>");
    }
    
    return isInPosition;
}

    public IEnumerator NinthTutorialPass()
    {
        SetTutorialState(TutorialState.FacturaDiaria);
        
        tutorialText.text = "¡Felicidades! Has completado tu primer día en la Agencia. Ahora veamos cómo te ha ido.";
        yield return StartCoroutine(WaitForContinueButton());


        //Enseñamos imagen de la factura diaria
        tutorialImage.sprite = resultSceneSprite;
        tutorialText.text = "Esto de aquí es la factura del día. Aquí podrás ver los frutos de tu rendimiento durante el día. ";
        yield return StartCoroutine(WaitForContinueButton());

        tutorialText.text = "En la columna izquierda tienes los ingresos. Aquí afectará cuantos objetos hayas vendido a los clientes. Y cuántos de esos clientes han tenido éxito.";
        yield return StartCoroutine(WaitForContinueButton());

        tutorialText.text = "Ten en cuenta que cuanto mejores sean los objetos para la misión del cliente más te pagarán por ellos.";
        yield return StartCoroutine(WaitForContinueButton());

        tutorialText.text = "En la columna del medio tienes los gastos. Aquí se tienen en cuenta cuántos clientes han fallado su misión y el coste por reponer cada objeto vendido.";
        yield return StartCoroutine(WaitForContinueButton());

        tutorialText.text = "Por último en la Columna de la derecha tienes el total. Siempre que el total sea positivo una parte de él irá a pagar tu deuda. ";
        yield return StartCoroutine(WaitForContinueButton());

        tutorialText.text = "Recuerda que si al final del día no has pagado toda tu deuda... bueno, digamos que Ulises no es muy tolerante con los impagos.";
        yield return StartCoroutine(WaitForContinueButton());

    }

    public IEnumerator TenthTutorialPass()
    {
        SetTutorialState(TutorialState.FinTutorial);
        
        tutorialText.text = "En fin, este es todo mi trabajo por hoy, que Ulises no paga a las niñeros.";
        yield return StartCoroutine(WaitForContinueButton());

        dogController.MoveTo(dogTransforms[4].position);

        tutorialText.text = "Si me ves por aquí será en mi caseta que está en la pared del fondo. Aunque más te vale no verme porque si aparezco será para avisarte de que uno de los clientes ha muerto.";
        yield return StartCoroutine(WaitForContinueButton());

        tutorialText.text = "Para pasar al siguiente día dale al botón de abajo que pone ir al siguiente día.";
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

    public bool playerDropObject( ObjectType item)
    {
        // Verificar si el objeto ha sido entregado al pedido activo
        if (OrderSystem.Instance != null)
        {
            var activeOrders = OrderSystem.Instance.GetActiveClientOrders();
            foreach (var clientOrder in activeOrders)
            {
                if (clientOrder.order.deliveredItems.Contains(item))
                {
                    return true;
                }
            }
        }
        return false;
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
        // Detecta si el manual está cerrado
        if (manualUI != null && !manualUI.manualPanel.activeSelf)
        {
            return true;
        }
        return false;
    }


    public bool isClientOrderDone(RequirementData monster, RequirementData condition, RequirementData environment)
    {
        // Verificar si existe un pedido activo que coincida con los requisitos
        if (OrderSystem.Instance != null)
        {
            var activeOrders = OrderSystem.Instance.GetActiveClientOrders();
            foreach (var clientOrder in activeOrders)
            {
                if (clientOrder.order.monster == monster && 
                    clientOrder.order.condition == condition && 
                    clientOrder.order.environment == environment)
                {
                    return true;
                }
            }
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
    public bool isDoginPlace(Transform dogTransform)
    {
        float distance = Vector3.Distance(tutorialDog.transform.position, dogTransform.position);
        return distance < 1.5f; // Ajusta el umbral según sea necesario
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
    
    /// <summary>
    /// Pausa el tiempo del juego (útil para mostrar animaciones o bocadillos)
    /// </summary>
    public void PauseGameTime()
    {
        Time.timeScale = 0f;
        Debug.Log("<color=yellow>⏸ Tiempo del juego pausado</color>");
    }
    
    /// <summary>
    /// Reanuda el tiempo del juego
    /// </summary>
    public void ResumeGameTime()
    {
        Time.timeScale = 1f;
        Debug.Log("<color=green>▶ Tiempo del juego reanudado</color>");
    }


}
