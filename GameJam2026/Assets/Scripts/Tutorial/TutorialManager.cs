using System;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEditor.UIElements;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Unity.Android.Gradle.Manifest;

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
    public bool canPlayerUseInventory = false;
    public bool isWaitingForPlayerAction = false;
    public bool canGenerateOrder = false;
    public bool isWaitingForFirstClientOrder = false;
    public bool isWaitingForSecondClientOrder = false;
    public bool isWaitingContinueButton = false;
    public bool isWaitingForManualOpen = false;
    public bool isWaitingForManualClose = false;
    public bool tutorialIsPaused = false;
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



    
    public enum TutorialState
    {
        StartTutorial,
        Introduction,
        PlayerMovement,
        Interaccion, // agafar els dos objectes ideals de la primera comanda
        Inventario,
        PrimerCliente, // la seva comanda es ciclope intelectual y estampida de ovejas
        MoverseMostrador,
        TomarPedido, //explicacio de comanda
        SistemaDesesperacion, //explicacio de sistema de desesperacion
        GestionPedidos, // explicacio de 3 comandes
        Manual,
        SistemaPuntuacion,
        FinManual,
        EntregaPedido, // explicacio de entrega de comanda y entrega dels dos primers objectes
        JornadaLaboral, // explicacio de jornada laboral de les seves condicions de finalitzacio
        SegundoCliente, // la seva comanda es ciclope bebe mucho polvo y interior cueva
        FinJornada, // resum de la jornada laboral,
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

        tutorialText.text = "No te va a decir, quiero una espada. Te dirá qué Monstruo quiere matar, qué Condición hay y dónde está. Recuerda los iconos de cada categoría. ";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

        tutorialText.text = "Siempre que el pedido esté activo verás las condiciones específicas en una nota en la derecha.";
        isWaitingContinueButton = true;
        yield return new WaitUntil(() => isWaitingContinueButton == false);

        canGenerateOrder = true;
        isWaitingForFirstClientOrder = true;
        yield return new WaitUntil(() => isClientOrderDone(ciclopeIntellectual, estampidaOvejas, null));

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

    public bool usedWheelInInventory()
    {
        if(InputManager.Instance.MouseScrollDelta == 0)
        {
            return false;
        }
        else
        return true;
    }

    public void InstanceClient(int slotIndex)
    {
        ClientManager.Instance.SpawnClientInSlot(slotIndex);
    }

    public bool isClientOrderDone(RequirementData monster, RequirementData condition, RequirementData environment)
    {
        OrderGenerator.Instance.GenerateSpecificOrder(monster, condition, environment);
        return isWaitingForFirstClientOrder == false;
    }


}
