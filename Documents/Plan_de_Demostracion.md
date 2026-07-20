# Plan de Demostración — HorusEye
## Control de Inventarios y Activos en Tiempo Real

---

## Objetivo

Demostrar a potenciales clientes cómo se controlan inventarios y activos en tiempo real usando RFID.

---

## Configuración Inicial (Una sola vez)

Estos pasos se ejecutan una sola vez y quedan guardados para todas las demostraciones posteriores.

### 1.1 Login como Administrador
- Abrir `https://horuseye.mauricioadachi.dev`
- Login: `admin@horuseye.com` / `Admin123!`

### 1.2 Crear Usuario Proveedor
- Ir a `/usuarios`
- Crear usuario con rol **Administrador del Proveedor**
- Este proveedor se usará en todas las demostraciones

---

## Flujo de Demostración (Repetible)

### PASO 1: Login
- Login con `admin@horuseye.com`
- "Bienvenidos. Hoy vamos a demostrar cómo se controlan los activos en tiempo real."

### PASO 2: Crear Cliente
- Ir a `/clientes`
- Crear un cliente nuevo y asociarlo al Proveedor creado en la configuración inicial
- "Cada cliente tiene su propio inventario aislado"

### PASO 3: Configurar Jerarquía de Ubicaciones
- Ir a `/ubicaciones`
- Seleccionar el cliente creado
- Crear la jerarquía de ubicaciones del cliente. Ejemplo:

```
Constructora ABC [Cliente]
├── Atlántico [Departamento]
│   ├── Barranquilla [Ciudad]
│   │   ├── Vía 40 [Bodega]
│   │   └── Centro [Bodega]
│   └── Soledad [Ciudad]
│       └── Hipódromo [Bodega]
├── Bolívar [Departamento]
│   └── Cartagena [Ciudad]
│       ├── Crespo [Bodega]
│       └── Bocagrande [Bodega]
└── Magdalena [Departamento]
    ├── Santa Marta [Ciudad]
    │   ├── Mamatoco [Bodega]
    │   └── Bavaria [Bodega]
    └── Fundación [Ciudad]
        ├── Chimila [Bodega]
        └── La Esmeralda [Bodega]
```

- Para cada nodo: escribir el **Nombre** (ej: "Barranquilla") y el **Tipo de nodo** (ej: "Ciudad")
- "La estructura es completamente configurable. Cada cliente define sus propios niveles."

### PASO 4: Registrar Dispositivos por Ubicación
- Ir a `/dispositivos`
- Seleccionar el cliente
- Seleccionar una ubicación (nodo hoja del árbol, ej: "Bodega Vía 40")
- Al seleccionar la ubicación se muestran dos listados:
  - **Lectores registrados** en esa ubicación
  - **Tags registrados** para esa ubicación
- Crear los lectores necesarios (ej: "Lector A" y "Lector B")
- Registrar tags (tarjetas NFC) con su UID
- "Cada ubicación tiene sus propios lectores y tags."

### PASO 4.1: Definir Eventos por Ubicación
- Ir a la configuración de eventos de la ubicación
- Crear eventos definiendo nombre y secuencia de lectores
- Ejemplo para "Bodega Vía 40":
  - **Evento "Entrada"**: secuencia = [Lector B] (uno solo)
  - **Evento "Salida"**: secuencia = [Lector A] (uno solo)
- "El sistema detecta automáticamente qué evento ocurrió según el orden en que los lectores leyeron el tag."
- "No se puede repetir una secuencia de lectores en la misma ubicación."

### PASO 5: Crear Activos y Asignar Tags
- Ir a `/activos`
- Crear activos y asignarles los tags registrados:
  - Ej: "Laptop Dell" → tag de la tarjeta 1
  - Ej: "Monitor Samsung" → tag de la tarjeta 2
- "Cada activo tiene su placa, nombre y está vinculado a una etiqueta RFID"

### PASO 6: Simular Movimiento con el Celular
- Abrir NFC Tester en los celulares (uno por lector)
- Seleccionar el dispositivo lecorrespondiente
- Acercar la tarjeta a los celulares en el orden definido por el evento
- El sistema detecta automáticamente qué evento ocurrió según la secuencia
- "No seleccionamos si es entrada o salida. El sistema lo sabe por el orden de los lectores."

### PASO 7: Verificar en Dashboard y Consulta
- Ir a `/dashboard`
- Ver el movimiento registrado en tiempo real
- Ir a `/reportes` o consultar movimientos
- "Aquí está el registro: fecha, hora, activo, tipo de movimiento, y si estuvo autorizado"

---

## Checklist Pre-Demo

### Configuración inicial (una vez)
- [ ] Proveedor creado y activo

### Por cada demostración
- [ ] Cliente creado y asociado al Proveedor
- [ ] Jerarquía de ubicaciones configurada
- [ ] Dispositivos registrados por ubicación (lectores + tags)
- [ ] Eventos definidos por ubicación (nombre + secuencia de lectores)
- [ ] Activos creados con tags asignados
- [ ] Celulares con NFC activado y NFC Tester funcionando (uno por lector)
- [ ] 2-3 tarjetas bancarias con chip contactless
- [ ] Frontend y backend respondiendo

### Verificación
- [ ] Escanear tarjeta → movimiento aparece en Dashboard
- [ ] Escanear sin autorización → alarma se activa
- [ ] Consultar reportes → movimientos registrados
