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
        while (isWaitingContinueButton)
        {
            yield return null; // Esperar un frame
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
        tutorialText.text = "Empecemos por lo básico. Acércate a ese estante y agarra eso.";
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

        yield return new WaitUntil(() => playerInZone(playerTransforms[2].position));
        canPlayerMove = false;

        // Generar pedido específico del tutorial
        canGenerateOrder = true;
        isWaitingForFirstClientOrder = true;
        CreateClientOrder(ciclopeIntellectual, estampidaOvejas, null);
        yield return new WaitUntil(() => isClientOrderDone(ciclopeIntellectual, estampidaOvejas, null));

        // Pop Up Imagen del bocadillo de pedido
        if (orderSprite != null)
        {
            tutorialImage.sprite = orderSprite;
            tutorialImage.gameObject.SetActive(true);
        }
        
        tutorialText.text = "No te va a decir, quiero una espada. Te dirá qué Monstruo quiere matar, qué Condición hay y dónde está. Recuerda los iconos de cada categoría.";
        isPlayerLookingAt = false;
        yield return new WaitUntil(() => isPlayerLooking(orderBocadillo) == true);
        tutorialImage.gameObject.SetActive(false);

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

        tutorialText.text = "Cuando termines cierra el manual.";
        isWaitingForManualClose = true;
        yield return new WaitUntil(() => playerCloseManual() == true);
        isWaitingForManualClose = false;

        canPlayerMove = true;
        canPlayerMoveCamera = true;
        tutorialText.text = "Casualmente los objetos de tus bolsillos son justo lo que el cliente quiere. Ve a entregárselos.";
        yield return StartCoroutine(WaitForContinueButton());

    }

    public IEnumerator SeventhTutorialPass()
    {
        SetTutorialState(TutorialState.EntregaPedido);

        tutorialText.text = "Bien, fijate que el cliente cuando ha hecho el pedido ha dejado una mochila. Es en esa mochila donde pondremos los objetos.";
        isPlayerLookingAt = false;
        yield return new WaitUntil(() => isPlayerLooking(bag) == true);

        tutorialText.text = "Cojo uno de los objetos de tus bolsillos y colócalo dentro. Ten en cuenta que una vez colocado el objeto no hay vuelta atrás. Ahora pon el siguiente.";
        yield return StartCoroutine(WaitForContinueButton());
        canPlayerInteract = true;
        yield return new WaitUntil(() => playerDropObject(ObjectType.Odre) || playerDropObject(ObjectType.Arco));
        canPlayerInteract = false;

        tutorialText.text = "Para que el pedido sea completado tienes que colocar los mismos objetos que especificaciones te pida el cliente.";
        yield return StartCoroutine(WaitForContinueButton());
        tutorialText.text = "En este caso eran dos así que con dos objetos basta. Ten en cuenta que te pueden venir pedidos de tres especificaciones también.";
        yield return StartCoroutine(WaitForContinueButton());
        tutorialText.text = "¡Perfecto! Ahora entrega el segundo objeto asi acabamos.";
        yield return StartCoroutine(WaitForContinueButton());
        canPlayerInteract = true;

        yield return new WaitUntil(() => playerDropObject(ObjectType.Arco) || playerDropObject(ObjectType.Odre));

    }

    public IEnumerator EighthTutorialPass()
    {
        SetTutorialState(TutorialState.SegundoCliente);
        
        tutorialText.text = "Ahora que ya sabes como va el tema, atiende a tu segundo cliente que ya está llegando.";
        yield return StartCoroutine(WaitForContinueButton());

        tutorialText.text = "Para este pedido, intenta usar todos los objetos ideales para que el cliente quede satisfecho.";
        canPlayerMove = true;
        canPlayerMoveCamera = true;
        canPlayerOpenManual = true;
        canPlayerChangePage = true;

        isWaitingForSecondClientOrder = true;
        yield return new WaitUntil(() => isClientOrderDone(ciclopeBebe, muchoPolvo, interiorCueva));
        isWaitingForSecondClientOrder = false;
        yield return new WaitUntil(() => playerDropObject(ObjectType.CascoA) && playerDropObject(ObjectType.Mascaras) && playerDropObject(ObjectType.Espejo));

        tutorialText.text = "Bien hecho, has sido capaz de completar el pedido por tu cuenta.";
        yield return StartCoroutine(WaitForContinueButton());

        tutorialText.text = "Ten en cuenta que el día se dará por terminado si te quedas sin clientes o si se acaba el día. Puedes ver cuánto queda debajo de la deuda a la izquierda.";
        yield return StartCoroutine(WaitForContinueButton());

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

    public bool playerDropObject( ObjectType item)
    {
        if (InventoryManager.Instance.GetCurrentObjectType() != item)
        {
            return false;
        }
        else
            return true;
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
       // OrderGenerator.Instance.GenerateSpecificOrder(monster, condition, environment);
        return true;
    }
    public void CreateClientOrder(RequirementData monster, RequirementData condition, RequirementData environment)
    {
        OrderGenerator.Instance.GenerateSpecificOrder(monster, condition, environment);
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


}
