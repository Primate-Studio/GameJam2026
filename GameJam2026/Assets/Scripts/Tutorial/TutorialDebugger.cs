using UnityEngine;

/// <summary>
/// Utilidades de debugging para el tutorial no lineal
/// Añade este componente a un GameObject en la escena para facilitar el testing
/// </summary>
public class TutorialDebugger : MonoBehaviour
{
    [Header("Debug Options")]
    public bool showDebugUI = true;
    public KeyCode resetTutorialKey = KeyCode.R;
    public KeyCode skipToPhaseKey = KeyCode.N;
    public KeyCode toggleRestrictionsKey = KeyCode.T;
    public KeyCode completeCurrentClientKey = KeyCode.C;

    [Header("Phase Skip")]
    public TutorialStateManager.TutorialPhase phaseToSkipTo = TutorialStateManager.TutorialPhase.FreeExploration;

    private Rect debugWindowRect = new Rect(10, 10, 400, 600);
    private Vector2 scrollPosition = Vector2.zero;
    private bool showDebugWindow = true;

    void Update()
    {
        // Resetear tutorial
        if (Input.GetKeyDown(resetTutorialKey))
        {
            ResetTutorial();
        }

        // Skip a fase
        if (Input.GetKeyDown(skipToPhaseKey))
        {
            SkipToPhase();
        }

        // Toggle restricciones
        if (Input.GetKeyDown(toggleRestrictionsKey))
        {
            ToggleAllRestrictions();
        }

        // Completar cliente actual
        if (Input.GetKeyDown(completeCurrentClientKey))
        {
            CompleteCurrentClient();
        }

        // Toggle ventana de debug
        if (Input.GetKeyDown(KeyCode.F1))
        {
            showDebugWindow = !showDebugWindow;
        }
    }

    void OnGUI()
    {
        if (!showDebugUI || !showDebugWindow) return;

        debugWindowRect = GUI.Window(0, debugWindowRect, DrawDebugWindow, "Tutorial Debugger (F1 to toggle)");
    }

    void DrawDebugWindow(int windowID)
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        // === STATE MANAGER ===
        GUILayout.Label("=== TUTORIAL STATE ===", GUI.skin.box);
        
        if (TutorialStateManager.Instance != null)
        {
            var state = TutorialStateManager.Instance;
            
            GUILayout.Label($"Current Phase: {state.currentPhase}");
            GUILayout.Label($"Clients Completed: {state.clientsCompleted}/2");
            GUILayout.Space(10);

            GUILayout.Label("--- Knowledge State ---");
            DrawBoolField("Learned Movement", state.hasLearnedMovement);
            DrawBoolField("Learned Camera", state.hasLearnedCamera);
            DrawBoolField("Learned Manual", state.hasLearnedManual);
            DrawBoolField("Learned Orders", state.hasLearnedOrders);
            DrawBoolField("Learned Item Quality", state.hasLearnedItemQuality);
            GUILayout.Space(5);

            GUILayout.Label("--- Manual State ---");
            DrawBoolField("Opened Manual", state.hasOpenedManual);
            DrawBoolField("Seen Manual Explanation", state.hasSeenManualExplanation);
            GUILayout.Space(5);

            GUILayout.Label("--- Client Interaction ---");
            DrawBoolField("Approached Client 1", state.hasApproachedClient1);
            DrawBoolField("Approached Client 2", state.hasApproachedClient2);
            DrawBoolField("Talked with Client 1", state.hasTalkedWithClient1);
            DrawBoolField("Talked with Client 2", state.hasTalkedWithClient2);
            GUILayout.Space(5);

            GUILayout.Label("--- Objects ---");
            DrawBoolField("Seen Client 1 Objects", state.hasSeenClient1Objects);
            DrawBoolField("Seen Client 2 Objects", state.hasSeenClient2Objects);
            GUILayout.Space(5);

            GUILayout.Label("--- Orders ---");
            DrawBoolField("Received Client 1 Order", state.hasReceivedClient1Order);
            DrawBoolField("Received Client 2 Order", state.hasReceivedClient2Order);
            DrawBoolField("Completed Client 1", state.hasCompletedClient1Order);
            DrawBoolField("Completed Client 2", state.hasCompletedClient2Order);
        }
        else
        {
            GUILayout.Label("TutorialStateManager not found!");
        }

        GUILayout.Space(10);

        // === PLAYER RESTRICTIONS ===
        GUILayout.Label("=== PLAYER RESTRICTIONS ===", GUI.skin.box);
        
        if (TutorialPlayerRestrictions.Instance != null)
        {
            var restrictions = TutorialPlayerRestrictions.Instance;
            
            DrawBoolField("Can Move", restrictions.canMove);
            DrawBoolField("Can Move Camera", restrictions.canMoveCamera);
            DrawBoolField("Can Interact", restrictions.canInteract);
            DrawBoolField("Can Open Manual", restrictions.canOpenManual);
            DrawBoolField("Can Use Inventory", restrictions.canUseInventory);
            DrawBoolField("Restrict Object Types", restrictions.restrictObjectTypes);

            if (restrictions.restrictObjectTypes && restrictions.allowedObjectTypes != null)
            {
                GUILayout.Label($"Allowed Objects: {restrictions.allowedObjectTypes.Length}");
            }
        }
        else
        {
            GUILayout.Label("TutorialPlayerRestrictions not found!");
        }

        GUILayout.Space(10);

        // === ORDER SYSTEM ===
        GUILayout.Label("=== ORDER SYSTEM ===", GUI.skin.box);
        
        if (TutorialOrderSystem.Instance != null)
        {
            int deliveredCount = TutorialOrderSystem.Instance.GetDeliveredItemsCount();
            GUILayout.Label($"Delivered Items: {deliveredCount}");
            GUILayout.Label($"Current Order Complete: {TutorialOrderSystem.Instance.IsCurrentOrderComplete()}");
        }
        else
        {
            GUILayout.Label("TutorialOrderSystem not found!");
        }

        GUILayout.Space(10);

        // === DEBUG ACTIONS ===
        GUILayout.Label("=== DEBUG ACTIONS ===", GUI.skin.box);
        
        if (GUILayout.Button($"Reset Tutorial ({resetTutorialKey})"))
        {
            ResetTutorial();
        }

        if (GUILayout.Button($"Toggle All Restrictions ({toggleRestrictionsKey})"))
        {
            ToggleAllRestrictions();
        }

        if (GUILayout.Button($"Complete Current Client ({completeCurrentClientKey})"))
        {
            CompleteCurrentClient();
        }

        GUILayout.Space(5);

        // Phase Skip
        GUILayout.Label("Skip to Phase:");
        if (GUILayout.Button("Introduction"))
        {
            SkipToSpecificPhase(TutorialStateManager.TutorialPhase.Introduction);
        }
        if (GUILayout.Button("Free Exploration"))
        {
            SkipToSpecificPhase(TutorialStateManager.TutorialPhase.FreeExploration);
        }
        if (GUILayout.Button("Client Interaction"))
        {
            SkipToSpecificPhase(TutorialStateManager.TutorialPhase.ClientInteraction);
        }
        if (GUILayout.Button("Second Client"))
        {
            SkipToSpecificPhase(TutorialStateManager.TutorialPhase.SecondClient);
        }
        if (GUILayout.Button("Completed"))
        {
            SkipToSpecificPhase(TutorialStateManager.TutorialPhase.Completed);
        }

        GUILayout.Space(10);

        // === MANUAL CONTROLS ===
        GUILayout.Label("=== MANUAL CONTROLS ===", GUI.skin.box);
        
        if (GUILayout.Button("Force Learn All Basics"))
        {
            ForceLearAllBasics();
        }

        if (GUILayout.Button("Mark Manual as Opened"))
        {
            if (TutorialStateManager.Instance != null)
            {
                TutorialStateManager.Instance.hasOpenedManual = true;
                TutorialStateManager.Instance.hasSeenManualExplanation = true;
            }
        }

        if (GUILayout.Button("Complete Client 1"))
        {
            if (TutorialStateManager.Instance != null)
            {
                TutorialStateManager.Instance.hasCompletedClient1Order = true;
                TutorialStateManager.Instance.clientsCompleted++;
            }
        }

        if (GUILayout.Button("Complete Client 2"))
        {
            if (TutorialStateManager.Instance != null)
            {
                TutorialStateManager.Instance.hasCompletedClient2Order = true;
                TutorialStateManager.Instance.clientsCompleted++;
            }
        }

        GUILayout.EndScrollView();

        GUI.DragWindow();
    }

    void DrawBoolField(string label, bool value)
    {
        Color originalColor = GUI.color;
        GUI.color = value ? Color.green : Color.red;
        GUILayout.Label($"{label}: {(value ? "✓" : "✗")}");
        GUI.color = originalColor;
    }

    void ResetTutorial()
    {
        Debug.Log("=== RESETTING TUTORIAL ===");
        
        if (TutorialStateManager.Instance != null)
        {
            TutorialStateManager.Instance.ResetTutorial();
        }

        if (TutorialPlayerRestrictions.Instance != null)
        {
            TutorialPlayerRestrictions.Instance.EnableAll();
        }

        if (TutorialOrderSystem.Instance != null)
        {
            TutorialOrderSystem.Instance.ClearAllOrders();
        }

        // Reiniciar el tutorial
        if (NewTutorial.Instance != null)
        {
            NewTutorial.Instance.StartTutorial();
        }
    }

    void SkipToPhase()
    {
        SkipToSpecificPhase(phaseToSkipTo);
    }

    void SkipToSpecificPhase(TutorialStateManager.TutorialPhase phase)
    {
        Debug.Log($"=== SKIPPING TO PHASE: {phase} ===");
        
        if (TutorialStateManager.Instance != null)
        {
            TutorialStateManager.Instance.SetPhase(phase);

            // Configurar los requisitos según la fase
            switch (phase)
            {
                case TutorialStateManager.TutorialPhase.FreeExploration:
                    ForceLearAllBasics();
                    break;

                case TutorialStateManager.TutorialPhase.ClientInteraction:
                    ForceLearAllBasics();
                    TutorialStateManager.Instance.hasApproachedClient1 = true;
                    break;

                case TutorialStateManager.TutorialPhase.SecondClient:
                    ForceLearAllBasics();
                    TutorialStateManager.Instance.hasCompletedClient1Order = true;
                    TutorialStateManager.Instance.clientsCompleted = 1;
                    break;

                case TutorialStateManager.TutorialPhase.Completed:
                    TutorialStateManager.Instance.hasCompletedClient1Order = true;
                    TutorialStateManager.Instance.hasCompletedClient2Order = true;
                    TutorialStateManager.Instance.clientsCompleted = 2;
                    break;
            }
        }

        if (TutorialPlayerRestrictions.Instance != null)
        {
            TutorialPlayerRestrictions.Instance.EnableAll();
        }
    }

    void ToggleAllRestrictions()
    {
        if (TutorialPlayerRestrictions.Instance != null)
        {
            bool currentState = TutorialPlayerRestrictions.Instance.canMove;
            
            if (currentState)
            {
                Debug.Log("=== DISABLING ALL RESTRICTIONS ===");
                TutorialPlayerRestrictions.Instance.DisableAll();
            }
            else
            {
                Debug.Log("=== ENABLING ALL RESTRICTIONS ===");
                TutorialPlayerRestrictions.Instance.EnableAll();
            }
        }
    }

    void CompleteCurrentClient()
    {
        if (TutorialStateManager.Instance == null) return;

        var activeClient = TutorialStateManager.Instance.activeClient;
        
        if (activeClient != null)
        {
            Debug.Log($"=== COMPLETING CLIENT {activeClient.clientID} ===");
            TutorialStateManager.Instance.CompleteClient(activeClient);
            activeClient.CompleteOrder();
        }
        else
        {
            Debug.LogWarning("No active client to complete!");
        }
    }

    void ForceLearAllBasics()
    {
        if (TutorialStateManager.Instance != null)
        {
            TutorialStateManager.Instance.hasLearnedMovement = true;
            TutorialStateManager.Instance.hasLearnedCamera = true;
            TutorialStateManager.Instance.hasLearnedManual = true;
            Debug.Log("=== LEARNED ALL BASICS ===");
        }
    }
}
