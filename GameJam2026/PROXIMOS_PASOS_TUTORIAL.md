# Tutorial No Lineal - Próximos Pasos

## ✅ Scripts Creados

1. **NewTutorial.cs** - Controlador principal ✓
2. **TutorialDialogueSystem.cs** - Sistema de diálogos ✓
3. **TutorialPlayerRestrictions.cs** - Restricciones del jugador ✓
4. **TutorialStateManager.cs** - Gestión de estado no lineal ✓
5. **TutorialClient.cs** - Componente para clientes ✓
6. **TutorialOrderSystem.cs** - Sistema de pedidos ✓
7. **TutorialHint.cs** - Hints visuales ✓
8. **TutorialIntegrationExamples.cs** - Ejemplos de integración ✓
9. **TutorialDebugger.cs** - Herramienta de debugging ✓

## 📋 Tareas Pendientes

### 1. Configuración en Unity (IMPORTANTE)

#### A. Crear GameObjects en la Escena
- [ ] GameObject "TutorialManager" con todos los componentes
- [ ] Configurar perro tutorial con TutorialDog + Animator
- [ ] Crear 3 transforms para posiciones del perro
- [ ] Configurar Cliente 1 con TutorialClient
- [ ] Configurar Cliente 2 con TutorialClient
- [ ] Crear zonas de interacción (triggers) para cada cliente
- [ ] Asignar mochilas para entregas (sin bocadillos)

#### B. Configurar UI del Tutorial
- [ ] Panel de diálogo con:
  - TextMeshProUGUI para el texto
  - Image para el personaje (perro/cliente)
  - Image para imágenes instructivas (WASD, mouse, etc.)
  - Botón de "Continuar"
- [ ] Asignar todos los sprites necesarios

#### C. Objetos Interactuables
- [ ] Añadir TutorialHint a los objetos del Cliente 1
- [ ] Añadir TutorialHint a los objetos del Cliente 2
- [ ] Configurar luces/efectos de highlight

### 2. Integración con Sistemas Existentes

Usar **TutorialIntegrationExamples.cs** como referencia:

#### A. InputManager.cs
```csharp
// Añadir verificaciones:
- canMove antes de procesar input de movimiento
- canMoveCamera antes de procesar input de cámara
- canOpenManual antes de abrir el manual
- Notificar hasOpenedManual = true cuando se abre el manual
```

#### B. InventoryManager.cs
```csharp
// Añadir verificaciones:
- canUseInventory antes de cambiar slots
- restrictObjectTypes + IsObjectAllowed() al recoger objetos
```

#### C. ClientManager.cs
```csharp
// Modificar:
- No spawnear clientes si CurrentState == GameState.Tutorial
```

#### D. OrderGenerator.cs
```csharp
// Modificar:
- No generar pedidos si CurrentState == GameState.Tutorial
```

#### E. DeliveryBox.cs (o sistema de entregas)
```csharp
// Añadir:
- Detectar si estamos en tutorial
- Usar TutorialOrderSystem.DeliverItem() en tutorial
- Detectar cuando se completa un pedido
```

#### F. ManualUI.cs
```csharp
// Añadir al método Open():
if (TutorialStateManager.Instance != null)
{
    TutorialStateManager.Instance.hasOpenedManual = true;
}
```

### 3. Implementar TODOs en el Código

#### En NewTutorial.cs:

**TODO 1: Detección de proximidad a objetos (línea ~493)**
```csharp
private IEnumerator WaitForPlayerNearObjects(TutorialClient client)
{
    bool isNear = false;
    while (!isNear)
    {
        foreach (TutorialHint hint in client.objectHints)
        {
            if (hint != null && hint.targetObject != null)
            {
                float distance = Vector3.Distance(
                    playerTransform.position,
                    hint.targetObject.transform.position
                );
                
                if (distance < 3f) // Radio ajustable
                {
                    isNear = true;
                    break;
                }
            }
        }
        yield return null;
    }
}
```

**TODO 2: Verificación de objetos recogidos (línea ~508)**
```csharp
private IEnumerator WaitForObjectCollection(TutorialClient client)
{
    playerRestrictions.EnableAll();
    
    // Obtener los tipos de objetos necesarios
    int requiredCount = client.GetRequirementCount();
    int collectedCount = 0;
    
    // Esperar hasta que el jugador tenga los objetos necesarios
    while (collectedCount < requiredCount)
    {
        // Verificar inventario
        // Ejemplo: collectedCount = InventoryManager.Instance.GetFilledSlotsCount();
        
        yield return new WaitForSeconds(0.5f);
    }
    
    client.HideObjectHints();
}
```

**TODO 3: Verificación de pedido completado (líneas ~550 y ~577)**
```csharp
private IEnumerator WaitForOrderCompletion(TutorialClient client)
{
    // Esperar a que se complete el pedido
    while (!TutorialOrderSystem.Instance.IsCurrentOrderComplete())
    {
        yield return new WaitForSeconds(0.5f);
    }
    
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
```

#### En TutorialOrderSystem.cs:

**TODO 4: Lógica de objetos ideales (línea ~120)**
```csharp
public bool IsItemIdealForOrder(ObjectType itemType)
{
    if (currentDeliveryOrder == null) return false;
    
    // Verificar si el objeto es ideal para alguno de los requisitos
    // Esto depende de cómo tengas configurados tus RequirementData
    
    // Ejemplo básico:
    // - Verificar si el objeto está en la lista de objetos ideales del requirement1
    // - Verificar si el objeto está en la lista de objetos ideales del requirement2
    // - Verificar si el objeto está en la lista de objetos ideales del requirement3
    
    return true; // Implementar lógica real
}
```

### 4. Testing con el Debugger

1. **Añadir TutorialDebugger a la escena**
   - Crear un GameObject vacío "TutorialDebugger"
   - Añadir el componente TutorialDebugger

2. **Teclas de debugging:**
   - `F1` - Toggle ventana de debug
   - `R` - Reset tutorial
   - `N` - Skip a siguiente fase
   - `T` - Toggle restricciones
   - `C` - Completar cliente actual

3. **Ventana de debug muestra:**
   - Estado actual del tutorial
   - Todas las flags de progreso
   - Restricciones activas
   - Pedidos activos

### 5. Crear Prefabs de los Sprites

Necesitas crear/asignar estos sprites:

- **wasdSprite** - Imagen de las teclas WASD
- **mouseSprite** - Imagen del ratón
- **tabSprite** - Imagen de la tecla TAB
- **manualSprite** - Imagen del manual
- **desperationSprite** - Imagen de la rueda de desesperación
- **interactionSprite** - Imagen de interacción (E/F)
- **dogSprite** - Sprite del perro
- **clientSprite** - Sprite de cliente genérico

### 6. Configurar Requirements Data

Crear ScriptableObjects para los pedidos:

**Cliente 1:**
- Requirement 1: (por ejemplo, CiclopsIntelectual - Environment)
- Requirement 2: (por ejemplo, EstampidaOvejas - Attack)

**Cliente 2:**
- Requirement 1: (diferente del Cliente 1)
- Requirement 2: (diferente del Cliente 1)

### 7. Testing Paso a Paso

1. **Fase Introducción:**
   - [ ] Verifica que aparezcan los 3 mensajes (movimiento, cámara, manual)
   - [ ] Verifica que las restricciones funcionen
   - [ ] Verifica que el perro se mueva correctamente

2. **Fase Exploración Libre:**
   - [ ] Verifica que puedas elegir cualquier cliente
   - [ ] Verifica que el sistema detecte correctamente a qué cliente te acercas

3. **Fase Cliente 1:**
   - [ ] Verifica explicación de pedidos
   - [ ] Verifica explicación del manual (si no lo abriste antes)
   - [ ] Verifica que se iluminen los objetos correctos
   - [ ] Verifica que puedas recoger los objetos
   - [ ] Verifica que puedas entregar el pedido

4. **Fase Entre Clientes:**
   - [ ] Verifica mensaje del perro

5. **Fase Cliente 2:**
   - [ ] Verifica que NO repita explicaciones
   - [ ] Verifica que genere el pedido directamente
   - [ ] Verifica que complete correctamente

6. **Fase Final:**
   - [ ] Verifica mensaje de felicitación
   - [ ] Verifica transición al juego normal

## 🎯 Prioridades

1. **ALTA**: Configurar la escena con todos los GameObjects
2. **ALTA**: Integrar InputManager con las restricciones
3. **ALTA**: Integrar sistema de entregas con TutorialOrderSystem
4. **MEDIA**: Implementar TODOs de detección de objetos
5. **MEDIA**: Configurar UI y sprites
6. **BAJA**: Ajustes visuales y polish

## 📝 Notas Finales

- El sistema está **completamente funcional** en estructura
- Solo necesita **configuración en Unity** y las **integraciones mencionadas**
- Usa el **TutorialDebugger** para testear rápidamente
- Todos los diálogos están en **catalán** como solicitaste
- El sistema es **modular y expandible**

## 🆘 Si Necesitas Ayuda

Si tienes dudas con alguna integración específica, pregúntame sobre:
- Cómo integrar con InputManager
- Cómo detectar objetos recogidos
- Cómo configurar los Requirements
- Cualquier otra implementación específica

¡Buena suerte con el tutorial! 🎮
