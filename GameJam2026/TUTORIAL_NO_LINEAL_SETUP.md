# Sistema de Tutorial No Lineal - Guía de Configuración

## 📋 Descripción General

Este sistema implementa un tutorial no lineal donde el jugador puede explorar libremente mientras aprende las mecánicas del juego. El tutorial guía al jugador a través de explicaciones progresivas sin forzar un orden estricto después de la introducción básica.

## 🏗️ Arquitectura del Sistema

El sistema está dividido en varios componentes modulares:

### Scripts Principales

1. **NewTutorial.cs** - Controlador principal del tutorial
2. **TutorialDialogueSystem.cs** - Gestiona todos los diálogos y UI
3. **TutorialPlayerRestrictions.cs** - Controla qué puede hacer el jugador
4. **TutorialStateManager.cs** - Rastrea el progreso del jugador
5. **TutorialClient.cs** - Componente para cada cliente del tutorial
6. **TutorialOrderSystem.cs** - Gestiona los pedidos del tutorial
7. **TutorialHint.cs** - Sistema de hints visuales para objetos

## 🎮 Flujo del Tutorial

### Fase 1: Introducción (Lineal)
1. Explicación de movimiento (WASD)
2. Explicación de cámara (Mouse)
3. Explicación del manual (TAB)
4. Mensaje: "Ve a atender a los clientes"

### Fase 2: Exploración Libre (No Lineal)
- El jugador elige qué cliente atender primero
- Sistema detecta automáticamente la elección

### Fase 3: Primer Cliente (Completo)
1. Explicación de pedidos (si no la vio)
2. Explicación del manual (si no lo abrió antes)
3. Explicación de objetos (calidad, dinero, tiempo)
4. Recoger objetos
5. Volver con el cliente
6. Entregar pedido

### Fase 4: Entre Clientes
- Mensaje del perro: "Haz el segundo cliente"

### Fase 5: Segundo Cliente (Simplificado)
- Mensaje breve: "Hazme mi pedido"
- No repite explicaciones ya vistas
- Genera pedido directamente
- Bloquea objetos no ideales

### Fase 6: Finalización
- Mensaje de felicitación
- Transición al juego normal

## 🛠️ Configuración en Unity

### 1. Preparar la Escena

#### a) GameObject Principal: TutorialManager
- Crear un GameObject vacío llamado "TutorialManager"
- Añadir los siguientes componentes:
  - `NewTutorial`
  - `TutorialDialogueSystem`
  - `TutorialPlayerRestrictions`
  - `TutorialStateManager`
  - `TutorialOrderSystem`

#### b) Configurar el Perro Tutorial
- GameObject con el perro
- Componentes necesarios:
  - `TutorialDog` (del tutorial anterior)
  - `Animator`
- Crear transforms para las posiciones del perro:
  - Posición inicial (frente al jugador)
  - Posición neutral (observando)
  - Posición final (centro)

#### c) Configurar Clientes
- Dos GameObjects para los clientes
- Cada uno debe tener:
  - Componente `TutorialClient`
  - Transform para la zona de interacción
  - Mochila para entregas (GameObject)
  - Animator (opcional)

#### d) Configurar Objetos Interactuables
- Para cada objeto que el jugador debe recoger:
  - Añadir componente `TutorialHint`
  - Configurar el tipo de objeto
  - Asignar a la lista del cliente correspondiente

### 2. Configurar NewTutorial.cs

```
[Tutorial Dog]
- Tutorial Dog: GameObject del perro
- Dog Positions: Array de 3 transforms
  [0] Posición neutral
  [1] Posición entre clientes
  [2] Posición final

[Tutorial Clients]
- Client 1: TutorialClient del primer cliente
- Client 2: TutorialClient del segundo cliente

[Tutorial Systems]
- Dialogue System: Referencia al TutorialDialogueSystem
- Player Restrictions: Referencia al TutorialPlayerRestrictions
- State Manager: Referencia al TutorialStateManager

[Player Reference]
- Player Transform: Transform del jugador

[Manual Reference]
- Manual UI: Referencia al ManualUI del juego

[Requirement Data - Cliente 1]
- Client 1 Requirement 1: RequirementData (ScriptableObject)
- Client 1 Requirement 2: RequirementData (ScriptableObject)

[Requirement Data - Cliente 2]
- Client 2 Requirement 1: RequirementData (ScriptableObject)
- Client 2 Requirement 2: RequirementData (ScriptableObject)
```

### 3. Configurar TutorialDialogueSystem.cs

```
[UI Elements]
- Dialogue Text: TextMeshProUGUI para el texto
- Character Image: Image para el sprite del personaje
- Tutorial Image: Image para sprites instructivos
- Continue Button: Botón de continuar
- Dialogue Panel: Panel contenedor del diálogo

[Character Sprites]
- Dog Sprite: Sprite del perro
- Client Sprite: Sprite genérico de cliente

[Tutorial Images]
- WASD Sprite: Imagen de las teclas WASD
- Mouse Sprite: Imagen del ratón
- Tab Sprite: Imagen de la tecla TAB
- Manual Sprite: Imagen del manual
- Desperation Sprite: Imagen de la desesperación
- Interaction Sprite: Imagen de interacción
```

### 4. Configurar TutorialClient.cs (para cada cliente)

```
[Client Info]
- Client ID: 1 o 2
- Client Name: Nombre del cliente
- Client Transform: Transform del cliente
- Interaction Zone: Transform de la zona de interacción

[Order Data]
- Requirement 1: Asignado automáticamente desde NewTutorial
- Requirement 2: Asignado automáticamente desde NewTutorial
- Requirement 3: Opcional, null si solo tiene 2

[Visual References]
- Backpack: GameObject de la mochila
- Object Hints: Array de TutorialHint (objetos iluminados)

[Animator]
- Client Animator: Animator del cliente (opcional)
```

### 5. Configurar TutorialHint.cs (para cada objeto)

```
[Visual Effects]
- Highlight Effect: GameObject con luz/partículas (opcional)
- Highlight Color: Color del highlight (amarillo por defecto)

[Object Reference]
- Target Object: GameObject del objeto a resaltar
- Object Type: Tipo del objeto (enum ObjectType)
```

## 🔧 Integración con Sistemas Existentes

### InputManager
El InputManager debe verificar las flags de TutorialPlayerRestrictions:

```csharp
if (TutorialPlayerRestrictions.Instance != null && !TutorialPlayerRestrictions.Instance.canMove)
{
    // Bloquear input de movimiento
    return;
}
```

### InventoryManager
Verificar restricciones de objetos:

```csharp
if (TutorialPlayerRestrictions.Instance != null && 
    TutorialPlayerRestrictions.Instance.restrictObjectTypes)
{
    if (!TutorialPlayerRestrictions.Instance.IsObjectAllowed(objectType))
    {
        // No permitir recoger este objeto
        return;
    }
}
```

### ManualUI
Detectar cuando se abre el manual:

```csharp
public void OpenManual()
{
    if (TutorialStateManager.Instance != null)
    {
        TutorialStateManager.Instance.hasOpenedManual = true;
    }
    // ... resto del código
}
```

## 📊 Sistema de Detección de Manual

El sistema detecta automáticamente si el jugador abrió el manual antes de hablar con los clientes:

- Si abre el manual antes → Se salta la explicación del manual con el cliente
- Si NO lo abrió → Se explica el manual cuando hable con el cliente

## 🎯 TODO: Implementaciones Pendientes

Las siguientes funcionalidades están marcadas con `// TODO:` en el código:

1. **Detección de proximidad a objetos** (NewTutorial.cs, línea ~493)
   - Implementar verificación de que el jugador se acerca a los objetos iluminados

2. **Verificación de objetos recogidos** (NewTutorial.cs, línea ~508)
   - Detectar qué objetos el jugador ha recogido del inventario

3. **Verificación de pedido completado** (NewTutorial.cs, líneas ~550 y ~577)
   - Implementar detección de cuándo se entrega un pedido completo

4. **Lógica de objetos ideales** (TutorialOrderSystem.cs, línea ~120)
   - Implementar verificación de qué objetos son ideales para los requisitos

5. **Bloqueo de objetos no ideales** (NewTutorial.cs, línea ~339)
   - Implementar restricción de objetos que no son del pedido del segundo cliente

## 🐛 Debug y Testing

### Flags de Estado
Puedes verificar el estado del tutorial en tiempo real inspeccionando:
- `TutorialStateManager.Instance` - Ver todas las flags de progreso
- `TutorialPlayerRestrictions.Instance` - Ver restricciones activas
- `TutorialOrderSystem.Instance` - Ver pedidos activos

### Reset del Tutorial
Para resetear el tutorial durante testing:
```csharp
TutorialStateManager.Instance.ResetTutorial();
```

### Logs de Debug
El sistema genera logs automáticos en las transiciones de fase y eventos importantes.

## 📝 Notas Adicionales

### Ventajas de este Sistema

1. **Modular**: Cada componente tiene una responsabilidad clara
2. **Reutilizable**: Los scripts pueden usarse en otros tutoriales
3. **Flexible**: Fácil de expandir con nuevas fases o explicaciones
4. **Detección Automática**: El sistema detecta el progreso del jugador sin input manual
5. **No Lineal**: El jugador tiene libertad después de las explicaciones básicas

### Personalización

Para añadir nuevas explicaciones:
1. Añadir flag en `TutorialStateManager`
2. Crear método `Explain[NombreDelConcepto]` en `NewTutorial`
3. Llamarlo en el momento apropiado según el flujo

### Diálogos en Catalán

Todos los diálogos están en catalán como solicitado. Para cambiar el idioma, simplemente modifica las strings en las llamadas a `ShowDialogue()`.

## 🎨 Recomendaciones Visuales

1. **Highlights de Objetos**: Usar luces puntuales amarillas con intensidad pulsante
2. **Panel de Diálogo**: Fondo semitransparente negro con texto blanco
3. **Botón Continuar**: Visible y con feedback visual claro
4. **UI de Pedidos**: Las órdenes se muestran en el panel de la derecha de la pantalla

---

**Autor**: Sistema de Tutorial No Lineal v1.0  
**Fecha**: Enero 2026  
**Compatible con**: Unity 2022.3+
