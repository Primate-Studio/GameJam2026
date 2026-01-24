# 📦 Sistema de UI de Inventario - Guía de Configuración

## 🎯 Archivos Creados

1. **InventoryUI.cs** - Script principal que gestiona la visualización de los 3 slots
2. **InteractableObjectDatabase.cs** - Base de datos de sprites/iconos para cada objeto

---

## 🛠️ Configuración en Unity

### **1. Crear la Jerarquía de UI**

En tu Canvas, crea la siguiente estructura:

```
Canvas
└── InventoryPanel
    ├── Slot1
    │   ├── Border (Image - borde/fondo del slot)
    │   ├── Icon (Image - icono del objeto)
    │   └── Number (Text - opcional, muestra "1")
    ├── Slot2
    │   ├── Border (Image)
    │   ├── Icon (Image)
    │   └── Number (Text - opcional, muestra "2")
    └── Slot3
        ├── Border (Image)
        ├── Icon (Image)
        └── Number (Text - opcional, muestra "3")
```

### **2. Configurar los Componentes**

#### **A) InventoryPanel**
- Añade el componente `InventoryUI.cs`
- Layout sugerido: Horizontal Layout Group con spacing de 10-20

#### **B) Cada Slot (Slot1, Slot2, Slot3)**
- Tamaño recomendado: 80x80 o 100x100 píxeles
- Añade un componente **Layout Element** si usas Layout Groups

#### **C) Border (Image)**
- Sprite: Un cuadrado con borde (puedes usar el sprite default "UISprite")
- Color: Gris (se cambiará automáticamente por el script)
- Image Type: Sliced (si el sprite tiene bordes)

#### **D) Icon (Image)**
- Sprite: Ninguno inicialmente
- Color: Blanco
- Preserve Aspect: ✅ Activado
- **⚠️ IMPORTANTE**: Configura el Anchor/Pivot para que quede centrado y con margen
- Tamaño sugerido: 60x60 (10-20px más pequeño que el slot)

#### **E) Number (Text - opcional)**
- Text: "1", "2", "3" respectivamente
- Font Size: 20-24
- Alignment: Bottom-Right (esquina inferior derecha)
- Color: Blanco con outline negro

---

### **3. Asignar Referencias en el Inspector**

#### **En InventoryUI:**

1. **Slot UIs (Size: 3)** - Arrastra cada slot:
   - **Element 0 (Slot1):**
     - Transform: Slot1
     - Icon Image: Slot1 → Icon (Image)
     - Border Image: Slot1 → Border (Image)
     - Slot Number Text: Slot1 → Number (Text) [opcional]
   
   - **Element 1 (Slot2):**
     - Transform: Slot2
     - Icon Image: Slot2 → Icon (Image)
     - Border Image: Slot2 → Border (Image)
     - Slot Number Text: Slot2 → Number (Text) [opcional]
   
   - **Element 2 (Slot3):**
     - Transform: Slot3
     - Icon Image: Slot3 → Icon (Image)
     - Border Image: Slot3 → Border (Image)
     - Slot Number Text: Slot3 → Number (Text) [opcional]

2. **Visual Settings:**
   - Selected Color: Amarillo (#FFFF00) o dorado
   - Normal Color: Gris (#808080)
   - Selected Scale: 1.1 (hace el slot 10% más grande cuando está seleccionado)

---

### **4. Crear la Base de Datos de Objetos**

1. Crea un **GameObject vacío** en tu escena llamado `GameDatabase` o `ObjectDatabase`
2. Añade el componente `InteractableObjectDatabase.cs`
3. En el inspector, configura la lista **Object Icons**:

```
Element 0:
  - Object Type: Hilo
  - Icon: [Arrastra el sprite del hilo]

Element 1:
  - Object Type: Red
  - Icon: [Arrastra el sprite de la red]

Element 2:
  - Object Type: Espejo
  - Icon: [Arrastra el sprite del espejo]

... y así sucesivamente para todos tus objetos
```

**💡 TIP:** Puedes usar los mismos sprites que tengas en los objetos 3D, o crear iconos específicos para el UI.

---

### **5. Preparar los Sprites de Iconos**

Si aún no tienes sprites para los objetos:

1. **Opción A - Usar fotos/renders de los objetos 3D:**
   - En Unity, selecciona el objeto 3D
   - Toma un screenshot o usa una cámara ortográfica
   - Importa la imagen como Sprite (Texture Type: Sprite 2D/UI)

2. **Opción B - Crear iconos simples:**
   - Usa formas básicas en Photoshop/GIMP
   - O descarga iconos gratuitos de sitios como [Flaticon](https://www.flaticon.com/)
   - Importa con fondo transparente (PNG)

3. **Configuración del sprite en Unity:**
   - Texture Type: **Sprite (2D and UI)**
   - Sprite Mode: Single
   - Pixels Per Unit: 100
   - Filter Mode: Bilinear
   - Max Size: 256 o 512

---

## ✅ Verificación

Para probar que todo funciona:

1. ▶️ **Ejecuta el juego**
2. Los 3 slots deben aparecer vacíos (sin iconos)
3. El **Slot 1** debe estar resaltado (color amarillo y ligeramente más grande)
4. Recoge un objeto → debe aparecer su icono en el slot actual
5. Usa la rueda del mouse para cambiar de slot:
   - El borde debe cambiar de color
   - El slot debe cambiar de escala
6. Recoge más objetos → deben aparecer en los slots correspondientes

---

## 🎨 Personalización Visual

### **Estilo Moderno/Minimalista:**
```
- Border: Fondo oscuro (#1E1E1E) con borde fino blanco
- Selected: Borde brillante (#00FFFF) cyan
- Selected Scale: 1.15
```

### **Estilo Fantasía/RPG:**
```
- Border: Marco dorado ornamentado
- Selected: Efecto de brillo/glow amarillo
- Selected Scale: 1.2
- Añadir sombra (Shadow component)
```

### **Posición en pantalla:**
- **Bottom-Center:** Típico para inventarios rápidos
- **Bottom-Right:** Estilo survival/acción
- **Top-Right:** Mini-mapa/inventario compacto

---

## 🐛 Troubleshooting

**❌ Los iconos no aparecen:**
- Verifica que `InteractableObjectDatabase` esté en la escena
- Comprueba que los sprites estén asignados en la base de datos
- Mira la consola por warnings de "No se encontró icono para el objeto"

**❌ El slot seleccionado no se resalta:**
- Verifica que las referencias a Border Image estén asignadas
- Comprueba que Selected Color y Normal Color sean diferentes

**❌ Error de "Instance is null":**
- Asegúrate de que `InventoryManager` esté en la escena antes que `InventoryUI`
- Verifica que el script tenga el patrón Singleton correctamente

---

## 🚀 Mejoras Futuras (Opcionales)

- ✨ Animaciones al cambiar de slot (DOTween)
- 🎵 Sonidos al cambiar de slot
- 💡 Tooltip que muestre el nombre del objeto al pasar el mouse
- 🔢 Contador de cantidad (si tienes objetos stackeables)
- ⌨️ Atajos de teclado (1, 2, 3) para cambiar de slot directo
- 🌟 Efecto de partículas al recoger un objeto
