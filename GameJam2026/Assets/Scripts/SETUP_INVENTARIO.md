# Sistema de Inventario e Interacción - Job Simulator Style

## 📋 Resumen del Sistema

Sistema completo de inventario con 3 bolsillos, interacción con objetos del mundo, y mecánicas de coger/cambiar/entregar objetos.

---

## 🎯 Características Implementadas

### ✅ PlayerMovement conectado con InputManager
- Movimiento con WASD usando `InputManager.Instance.Horizontal/Vertical`
- Rotación de cámara con mouse usando `InputManager.Instance.MouseX/MouseY`
- Todo centralizado para facilitar cambio de teclas

### ✅ Sistema de Inventario (3 bolsillos)
- Cambio de bolsillo con **rueda del mouse**
- Cada bolsillo puede contener 1 objeto
- Visualización automática del objeto en la mano según bolsillo seleccionado
- Sistema de eventos para UI futura

### ✅ Sistema de Interacción
- Trigger delante de las manos para detectar objetos
- **Tecla E** para:
  - **Coger** objeto si no tienes nada
  - **Cambiar** objeto si tienes uno y tocas otro
  - **Entregar** objeto si tocas zona de entrega
- Detección por **enum ObjectType** (mejor que tags)

---

## 🛠️ Configuración en Unity

### 1. **Configurar el Player**

#### A. GameObject Principal del Player
1. Asegúrate de que tu Player tiene:
   - `PlayerMovement.cs` (ya actualizado)
   - `Rigidbody` (con Freeze Rotation activado)
   - `Camera` como hijo

#### B. Crear el InteractTrigger
1. Crea un **GameObject hijo** del Player llamado `InteractTrigger`
2. Posiciónalo **delante de las manos** del jugador (por ejemplo, 0.5 unidades adelante)
3. Añade un **Box Collider** o **Sphere Collider**:
   - ✅ Marca "**Is Trigger**"
   - Ajusta el tamaño para el área de alcance
4. Añade el script **`InteractionController.cs`**

#### C. Crear el HandTransform
1. Crea otro **GameObject hijo** del Player llamado `HandTransform`
2. Posiciónalo donde quieres que aparezcan los objetos en la mano
3. Este será la referencia para instanciar objetos

#### D. Configurar InventoryManager
1. Crea un **GameObject vacío** en la escena llamado `InventoryManager`
2. Añade el script **`InventoryManager.cs`**
3. En el Inspector:
   - Arrastra `HandTransform` al campo **Hand Transform**

### 2. **Crear Objetos Interactuables (Prefabs)**

#### A. Crear Prefab de Objeto (Ejemplo: Espada)
1. Crea un GameObject con el modelo 3D de la espada
2. Añade un **Collider** (Box/Capsule/Mesh según el objeto)
   - ⚠️ NO marques "Is Trigger"
3. Añade un **Rigidbody** si quieres física
4. Añade el script **`InteractableObject.cs`**
5. En el Inspector de InteractableObject:
   - **Object Type**: Selecciona `Espada`
   - **Hand Prefab**: Arrastra el prefab que se verá en la mano
   - **Is Delivery Zone**: Deja en `false`

#### B. Crear el HandPrefab
1. Crea una versión más pequeña o ajustada del objeto para la mano
2. Guárdala como prefab separado
3. Asigna este prefab al campo **Hand Prefab** del objeto interactuable

#### C. Añadir más objetos
Para cada nuevo objeto (Arco, Lanza, etc.):
1. Añade el tipo al **enum ObjectType** en `InteractableObject.cs`:
```csharp
public enum ObjectType
{
    None,
    Espada,
    Arco,
    Lanza,
    Hacha,      // ← Añade aquí
    Escudo,     // ← Y aquí
}
```
2. Repite los pasos A y B

### 3. **Crear Zona de Entrega**

1. Crea un GameObject (puede ser un cubo, área marcada, etc.)
2. Añade un **Collider** marcado como **Trigger**
3. Añade el script **`InteractableObject.cs`**
4. En el Inspector:
   - **Object Type**: Deja en `None`
   - **Is Delivery Zone**: Marca como `true` ✅

---

## 🎮 Controles

| Acción | Control |
|--------|---------|
| Movimiento | **WASD** |
| Mirar | **Mouse** |
| Cambiar bolsillo | **Rueda del Mouse** ↑↓ |
| Interactuar | **E** |
| Pausa | **Escape** |

---

## 🔄 Flujo de Interacción

```
┌─────────────────────────────────────────┐
│ ¿Tengo objeto en el bolsillo actual?   │
└──────────┬───────────────┬──────────────┘
           │ NO            │ SÍ
           ▼               ▼
    ┌──────────┐    ┌──────────────┐
    │ ¿Toco    │    │ ¿Qué toco?   │
    │ objeto?  │    │              │
    └────┬─────┘    └──┬───────┬───┘
         │ SÍ          │       │
         ▼             ▼       ▼
    [COGER]      [CAMBIAR]  [ENTREGAR]
                            (si es zona)
```

### Escenarios:
1. **Sin objeto + Cerca de objeto** → Presiona E = **COGER**
2. **Con objeto + Cerca de otro objeto** → Presiona E = **CAMBIAR**
3. **Con objeto + Cerca de zona entrega** → Presiona E = **ENTREGAR**

---

## 📝 Scripts Creados

### 1. **InteractableObject.cs**
- Define qué es un objeto (espada, arco, lanza)
- Guarda referencia al prefab de la mano
- Puede ser zona de entrega
- Métodos: `PickUp()`, `Drop()`

### 2. **InventoryManager.cs** (Singleton)
- Gestiona 3 bolsillos
- Cambio con scroll
- Instancia objetos en la mano
- Métodos principales:
  - `TryAddToCurrentSlot()` - Añadir objeto
  - `SwapCurrentSlot()` - Cambiar objeto
  - `DeliverCurrentSlot()` - Entregar objeto
  - `IsCurrentSlotEmpty()` - Verificar si está vacío

### 3. **InteractionController.cs**
- Detecta objetos con trigger
- Maneja input de E
- Lógica de coger/cambiar/entregar
- Debug visual con Gizmos

### 4. **InputManager.cs** (Actualizado)
- Añadidas propiedades:
  - `MouseX` / `MouseY`
  - `MouseScrollDelta`
  - `Horizontal` / `Vertical`
  - `InteractPressed`

### 5. **PlayerMovement.cs** (Actualizado)
- Ahora usa `InputManager.Instance` en lugar de `Input` directo
- Listo para cambio de teclas centralizado

---

## 🎨 Mejoras Futuras

### UI
- [ ] Mostrar visualmente los 3 bolsillos en pantalla
- [ ] Indicador del bolsillo actual seleccionado
- [ ] Iconos de los objetos en cada bolsillo

### Gameplay
- [ ] Sistema de puntos al entregar
- [ ] Diferentes zonas de entrega para diferentes objetos
- [ ] Efectos de sonido
- [ ] Animaciones de coger/soltar

### Objetos
- [ ] Más tipos de objetos
- [ ] Objetos que ocupan múltiples bolsillos
- [ ] Objetos combinables

---

## 🐛 Debug

### Consola
El sistema imprime información útil en la consola:
- 🟢 Verde: Objeto recogido
- 🔵 Cyan: Objeto cambiado
- 🟡 Amarillo: Objeto entregado
- ⚪ Gris: Objeto fuera de rango

### Vista de Escena
Con `showDebugInfo = true` en InteractionController:
- **Amarillo**: Trigger sin objeto cerca
- **Verde**: Trigger con objeto detectado

---

## ❓ Preguntas Frecuentes

**P: ¿Por qué enum en lugar de tags?**  
R: Enums son más eficientes, type-safe, autocompletan en código y evitan errores de typos.

**P: ¿Cómo añado más bolsillos?**  
R: Cambia el tamaño del array en InventoryManager (línea 26) y ajusta la lógica del scroll.

**P: ¿Cómo cambio las teclas?**  
R: Modifica los KeyCode en el Inspector del InputManager.

**P: ¿El objeto desaparece del mundo al cogerlo?**  
R: Sí, se desactiva con `SetActive(false)`. Se reactiva al cambiar o soltar.

---

## 🤝 Integración con tu amigo

El sistema está diseñado para trabajar con el **InputManager** de tu amigo. Todos los inputs pasan por ahí, así que podéis:
1. Añadir nuevos controles en InputManager
2. Cambiar keybindings desde el Inspector
3. Crear un menú de opciones para remapear teclas

---

## 📞 Soporte

Si algo no funciona:
1. Verifica que todos los GameObjects tienen los scripts asignados
2. Revisa que el InteractTrigger tiene "Is Trigger" activado
3. Asegúrate de que HandTransform está asignado en InventoryManager
4. Mira la consola - hay mensajes de debug útiles

---

**¡Listo para crear tu Job Simulator!** 🎮✨
