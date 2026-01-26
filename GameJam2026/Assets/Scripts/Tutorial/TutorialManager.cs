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
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
  
    }

    public void SetTutorialState(TutorialState newState)
    {
        currentState = newState;
    }

    public void ContinueButtonPressed()
    {
        if (isWaitingContinueButton)
        {
            isWaitingContinueButton = false;
        }
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
        
        tutorialText.text = "¡Arriba, gandul! Bienvenido al Oasis, el imperio viral de Ulises. Anoche te bebiste hasta el agua de los floreros.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

        tutorialText.text = "Ahora mismo estas trabajando para él en su tienda de objetos para las odiseas. La Odisea de TONI, para ser exactos.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);
        
        tutorialText.text = "Pero antes de nada, hablemos de tu deuda con Ulises...";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

        tutorialText.text = "La broma te sale por 250 monedas. Ulises es un tipo razonable, trabaja en la Agencia para pagarla o serás ejecutado al acabar el día. Tú eliges.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

        tutorialText.text = "Arriba a la izquierda siempre puedes observar lo que te queda por pagar. ¡Suerte!";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

    }

    public IEnumerator SecondTutorialPass()
    {
        SetTutorialState(TutorialState.PlayerMovement);
        
        tutorialText.text = "No me pierdas de vista o estaremos aquí todo el día. Mueve el cuello y búscame, estoy volando detrás de ti.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);
        canPlayerMoveCamera = true;

        tutorialText.text = "Empecemos por lo básico para que te acostumbres al lugar. Acércate a mí.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);
        canPlayerMove = true;
        yield return new WaitUntil(() => playerInZone(playerTransforms[0].position));
        canPlayerMove = false;
        
    }

    public IEnumerator ThirdTutorialPass()
    {
        SetTutorialState(TutorialState.Interaccion);
        
        tutorialText.text = "Empecemos por lo básico. Acércate a ese estante y agarra eso.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);
        canPlayerInteract = true;
        yield return new WaitUntil(() => playerTakeObject(ObjectType.Odre));
        canPlayerInteract = false;

        tutorialText.text = "Bien. Escucha que esto es importante. En la Agencia vendemos imitaciones baratas de armas divinas.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

        tutorialText.text = "Se dividen en tres tipos: Ataque para los monstruos, Adaptabilidad para las condiciones raras y Utilidad para el entorno.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

        tutorialText.text = "Cada uno de los tipos tiene su propio estante. Los puedes diferenciar por los Iconos.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

        tutorialText.text = "¡Bien! Ahora ve a por ese otro objeto.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

        canPlayerMove = true;
        yield return new WaitUntil(() => playerInZone(playerTransforms[1].position));
        canPlayerInteract = true;
        yield return new WaitUntil(() => playerTakeObject(ObjectType.Arco));

        tutorialText.text = "Ojo, aquí las cosas parecen infinitas, pero reponerlas cuesta dinero. Ten en cuenta que una vez cojas un objeto este tardará en aparecer.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

    }

    public IEnumerator FourthTutorialPass()
    {
        SetTutorialState(TutorialState.Inventario);
        
        tutorialText.text = "Fíjate en la parte de abajo . Eso de ahí son tus bolsillos, cada objeto ocupa una ranura en tus bolsillos. ";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);


        InstanceClient(0);
        canPlayerMove = false;
        canPlayerMoveCamera = false;
        tutorialText.text = "Puedes intercambiar de ranuras, vamos, pruébalo.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);
        canPlayerUseInventory = true;
        yield return new WaitUntil(() => usedWheelInInventory());



    }

    public IEnumerator FifthTutorialPass()
    {
        SetTutorialState(TutorialState.PrimerCliente);
        // sonido de campana
        tutorialText.text = "¿Oyes eso? Ha llegado tu primer cliente. Ve a atenderle.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);
        canPlayerMove = true;
        canPlayerMoveCamera = true;

        yield return new WaitUntil(() => playerInZone(playerTransforms[2].position));
        canPlayerMove = false;

        canGenerateOrder = true;
        isWaitingForFirstClientOrder = true;
        yield return new WaitUntil(() => isClientOrderDone(ciclopeIntellectual, estampidaOvejas, null));

        tutorialText.text = "No te va a decir, quiero una espada. Te dirá qué Monstruo quiere matar, qué Condición hay y dónde está. Recuerda los iconos de cada categoría.";
        isPlayerLookingAt = false;
        yield return new WaitUntil(() => isPlayerLooking(orderBocadillo) == true);

        // señalar las notas de pedido
        tutorialText.text = "Siempre que el pedido esté activo verás las condiciones específicas en una nota en la derecha.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

        //Popear imagen de la desesperacion 
        tutorialText.text = "Ese reloj de colores es su nivel de desesperación. Si llega a rojo, se enfadan. Si llega a cero, se enfrentará a una muerte segura ";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

        tutorialText.text = "Eso solo significa una cosa: perder dinero. Así que date prisa en atenderles.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

        tutorialText.text = "Ten en cuenta que solo puedes atender a un máximo de tres clientes a la vez. ";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

        tutorialText.text = "Así que no te emociones que no eres un pulpo.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

    }

    public IEnumerator SixthTutorialPass()
    {
        SetTutorialState(TutorialState.Manual);
        
        tutorialText.text = "No te asustes todavía, para poder saber qué es lo mejor para cada situación tienes el manual. Abrelo.";
        isWaitingContinueButton = true;
 
        yield return new WaitUntil(() => isWaitingContinueButton == false);
        isWaitingForManualOpen = true;
        yield return new WaitUntil(() => playerOpenManual() == true);
        isWaitingForManualOpen = false;
        isWaitingForManualClose = true;
        tutorialText.text = "Ten en cuenta que mientras tengas el manual abierto el tiempo seguirá corriendo igual, por lo que cuando antes te acostumbre mejor.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

        tutorialText.text = "Cada entrada del manual está dividida por la categoría que te he explicado antes.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

        tutorialText.text = "Prueba a pasar de página y verlo por ti mismo.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);
        
        canPlayerChangePage = true;
        yield return new WaitUntil(() => manualPageChanged());

        tutorialText.text = "Aquí verás que items son exactamente los necesarios para las condiciones específicas de la aventura que te está pidiendo el cliente.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

        tutorialText.text = "Si te fijas la nota sigue activa incluso con el manual abierto.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false); 

        tutorialText.text = "Para cada situación específica te saldrán tres objetos debajo con su utilidad a la derecha.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

        tutorialText.text = "Dependiendo cual elijas el cliente tendrá más o menos probabilidades de salir victorioso.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

        tutorialText.text = "Recuerda que los objetos no son eternos. Si cojes uno tardará en aparecer otro igual.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

        tutorialText.text = "Cuando termines cierra el manual.";
        isWaitingForManualClose = true;
        yield return new WaitUntil(() => playerCloseManual() == true);
        isWaitingForManualClose = false;

        canPlayerMove = true;
        canPlayerMoveCamera = true;
        tutorialText.text = "Casualmente los objetos de tus bolsillos son justo lo que el cliente quiere. Ve a entregárselos.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

    }

    public IEnumerator SeventhTutorialPass()
    {
        SetTutorialState(TutorialState.EntregaPedido);

        tutorialText.text = "Bien, fijate que el cliente cuando ha hecho el pedido ha dejado una mochila. Es en esa mochila donde pondremos los objetos.";
        isPlayerLookingAt = false;
        yield return new WaitUntil(() => isPlayerLooking(bag) == true);

        tutorialText.text = "Cojo uno de los objetos de tus bolsillos y colócalo dentro. Ten en cuenta que una vez colocado el objeto no hay vuelta atrás. Ahora pon el siguiente.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);
        canPlayerInteract = true;
        yield return new WaitUntil(() => playerDropObject(ObjectType.Odre) || playerDropObject(ObjectType.Arco));
        canPlayerInteract = false;

        tutorialText.text = "Para que el pedido sea completado tienes que colocar los mismos objetos que especificaciones te pida el cliente.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);
        tutorialText.text = "En este caso eran dos así que con dos objetos basta. Ten en cuenta que te pueden venir pedidos de tres especificaciones también.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);
        tutorialText.text = "¡Perfecto! Ahora entrega el segundo objeto asi acabamos.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);
        canPlayerInteract = true;

        yield return new WaitUntil(() => playerDropObject(ObjectType.Arco) || playerDropObject(ObjectType.Odre));

    }

    public IEnumerator EighthTutorialPass()
    {
        SetTutorialState(TutorialState.SegundoCliente);
        
        tutorialText.text = "Ahora que ya sabes como va el tema, atiende a tu segundo cliente que ya está llegando.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

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
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

        tutorialText.text = "Ten en cuenta que el día se dará por terminado si te quedas sin clientes o si se acaba el día. Puedes ver cuánto queda debajo de la deuda a la izquierda.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

    }

    public IEnumerator NinthTutorialPass()
    {
        SetTutorialState(TutorialState.FacturaDiaria);
        
        tutorialText.text = "¡Felicidades! Has completado tu primer día en la Agencia. Ahora veamos cómo te ha ido.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);


        //Enseñamos imagen de la factura diaria
        tutorialImage.sprite = resultSceneSprite;
        tutorialText.text = "Esto de aquí es la factura del día. Aquí podrás ver los frutos de tu rendimiento durante el día. ";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

        tutorialText.text = "En la columna izquierda tienes los ingresos. Aquí afectará cuantos objetos hayas vendido a los clientes. Y cuántos de esos clientes han tenido éxito.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

        tutorialText.text = "Ten en cuenta que cuanto mejores sean los objetos para la misión del cliente más te pagarán por ellos.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

        tutorialText.text = "En la columna del medio tienes los gastos. Aquí se tienen en cuenta cuántos clientes han fallado su misión y el coste por reponer cada objeto vendido.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

        tutorialText.text = "Por último en la Columna de la derecha tienes el total. Siempre que el total sea positivo una parte de él irá a pagar tu deuda. ";
        isWaitingContinueButton = true; 
        yield return new WaitUntil(() => isWaitingContinueButton == false);

        tutorialText.text = "Recuerda que si al final del día no has pagado toda tu deuda... bueno, digamos que Ulises no es muy tolerante con los impagos.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

    }

    public IEnumerator TenthTutorialPass()
    {
        SetTutorialState(TutorialState.FinTutorial);
        
        tutorialText.text = "En fin, este es todo mi trabajo por hoy, que Ulises no paga a las niñeros.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

        tutorialText.text = "Si me ves por aquí será en mi caseta que está en la pared del fondo. Aunque más te vale no verme porque si aparezco será para avisarte de que uno de los clientes ha muerto.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

        tutorialText.text = "Para pasar al siguiente día dale al botón de abajo que pone ir al siguiente día.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

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
        OrderGenerator.Instance.GenerateSpecificOrder(monster, condition, environment);
        return isWaitingForFirstClientOrder == false;
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
            if (hit.collider.gameObject == target || hit.collider.transform.IsChildOf(target.transform))
            {
                return true;
            }
        }

        return false;
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
