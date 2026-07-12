# Guia de Pruebas - Asistente del Administrador del Sistema

**Rol:** Asistente del Administrador del Sistema
**URL:** https://horuseye-app.mauricioadachi.dev
**Credenciales:** Crear desde el Admin del Sistema

---

## Alcance del Rol

El Asistente del Administrador del Sistema puede crear usuarios de los tipos: Proveedor y Cliente (Administrador/Asistente). Tambien puede gestionar dispositivos, ubicaciones y fabricantes. **NO** puede crear otros usuarios del Sistema ni usuarios Asistentes del Administrador del Sistema.

### Funcionalidades Disponibles

| Modulo | Ver | Crear | Editar | Eliminar |
|--------|-----|-------|--------|----------|
| Dashboard | Si | - | - | - |
| Activos | Si | Si | Si | Si |
| Tags RFID | Si | Si | Si | Si |
| Autorizaciones | Si | Si | Si | Si |
| Reportes | Si | - | - | - |
| Usuarios | Si | Si (solo roles de Proveedor/Cliente) | Si | Si |
| Proveedores | Si | Si | Si | Si |
| Clientes | Si | Si | Si | Si |
| Dispositivos | Si | Si | Si | Si |
| Ubicaciones | Si | Si | Si | Si |
| Fabricantes | Si | Si | Si | Si |

### Funcionalidades NO Disponibles

- **NO** puede crear Administrador del Sistema
- **NO** puede crear Asistente del Administrador del Sistema
- **NO** puede crear Soporte del Sistema

---

## Prueba 1: Login

1. Abrir https://horuseye-app.mauricioadachi.dev
2. Ingresar credenciales del Asistente del Admin (creado previamente)
3. Hacer clic en "Iniciar Sesion"
4. **Esperado:** Redirige al Dashboard. En el header se muestra el rol correcto.

---

## Prueba 2: Crear Usuarios - Roles Permitidos

### 2.1 Crear Administrador del Proveedor
1. Ir a "Usuarios"
2. Clic en "Nuevo Usuario"
3. Completar campos
4. Seleccionar rol "Administrador del Proveedor"
5. Seleccionar Proveedor
6. Guardar
7. **Esperado:** Usuario creado exitosamente

### 2.2 Crear Asistente del Proveedor
1. Repetir proceso con rol "Asistente del Proveedor"
2. **Esperado:** Usuario creado

### 2.3 Crear Administrador del Cliente
1. Repetir proceso con rol "Administrador del Cliente"
2. Seleccionar Cliente
3. **Esperado:** Usuario creado

### 2.4 Crear Asistente del Cliente
1. Repetir proceso con rol "Asistente del Cliente"
2. Seleccionar Cliente
3. **Esperado:** Usuario creado

---

## Prueba 3: Crear Usuarios - Roles NO Permitidos

### 3.1 Intentar Crear Administrador del Sistema
1. Ir a "Usuarios" > "Nuevo Usuario"
2. Intentar seleccionar rol "Administrador del Sistema"
3. **Esperado:** El rol NO aparece en el dropdown (solo se muestran los roles que puede crear)

### 3.2 Intentar Crear Asistente del Administrador del Sistema
1. Intentar seleccionar rol "Asistente del Administrador del Sistema"
2. **Esperado:** El rol NO aparece en el dropdown

### 3.3 Intentar Crear Soporte del Sistema
1. Intentar seleccionar rol "Soporte del Sistema"
2. **Esperado:** El rol NO aparece en el dropdown

---

## Prueba 4: Gestion de Proveedores

1. Ir a "Proveedores"
2. Crear, editar y eliminar proveedores
3. **Esperado:** CRUD completo funciona

---

## Prueba 5: Gestion de Clientes

1. Ir a "Clientes"
2. Crear, editar y eliminar clientes
3. Asociar clientes a proveedores
4. **Esperado:** CRUD completo funciona

---

## Prueba 6: Gestion de Dispositivos

1. Ir a "Dispositivos"
2. Crear, editar y eliminar dispositivos RFID
3. Asociar a clientes y proveedores
4. **Esperado:** CRUD completo funciona

---

## Prueba 7: Gestion de Ubicaciones

1. Ir a "Ubicaciones"
2. Seleccionar un cliente
3. Crear nodos raiz e hijos
4. **Esperado:** Arbol jerarquico funciona correctamente

---

## Prueba 8: Gestion de Fabricantes

1. Ir a "Fabricantes"
2. Crear fabricante
3. Agregar campos de payload
4. Editar y eliminar
5. **Esperado:** CRUD completo funciona

---

## Prueba 9: Modulos de Solo Lectura/Edicion

### 9.1 Activos
1. Ir a "Activos"
2. Verificar que puede crear, editar y eliminar activos
3. **Esperado:** CRUD completo

### 9.2 Tags
1. Ir a "Tags RFID"
2. Crear tag, cambiar estado
3. **Esperado:** CRUD completo

### 9.3 Autorizaciones
1. Ir a "Autorizaciones"
2. Crear, revocar y eliminar autorizaciones
3. **Esperado:** CRUD completo

### 9.4 Reportes
1. Ir a "Reportes"
2. Generar reporte y exportar a Excel
3. **Esperado:** Funciona correctamente

---

## Prueba 10: Configuracion de Antena (Flujo Completo)

El Asistente del Admin puede configurar antenas completas excepto eliminar fabricantes.

> **Recordar:** Fabricante = marca del hardware (Chainway, Zebra). Proveedor = empresa que presta el servicio (concepto separado).

### 10.1 Crear Fabricante de Hardware
1. Ir a "Fabricantes" > "Nuevo Fabricante"
2. Nombre: "Zebra", Descripcion: "Lectores Zebra FX9600"
3. Guardar
4. **Esperado:** Fabricante creado

### 10.2 Configurar Campos del Payload
1. Expandir "Zebra"
2. Agregar campos: `epc` → EPC, `antenna` → Antenna, `rssi` → RSSI, `timestamp` → Timestamp
3. **Esperado:** 4 campos configurados

### 10.3 Crear Cliente
1. Ir a "Clientes" > "Nuevo Cliente"
2. Completar datos
3. Guardar
4. **Esperado:** Proveedor creado

### 10.4 Crear Cliente
1. Ir a "Clientes" > "Nuevo Cliente"
2. Completar datos, asociar al proveedor
3. Guardar
4. **Esperado:** Cliente creado

### 10.5 Registrar Dispositivo
1. Ir a "Dispositivos" > "Nuevo Dispositivo"
2. Nombre: "Muelle de Carga", Fabricante: Zebra, Modelo: FX9600
3. IP: 192.168.1.200, Cliente: (el creado)
4. Endpoint: `https://horuseye-api.mauricioadachi.dev/api/eventos-rfid/zebra`
5. Guardar
6. **Esperado:** Dispositivo registrado

### 10.6 Crear Ubicaciones
1. Ir a "Ubicaciones", seleccionar el cliente
2. Crear: Bodega → Zona → Muelle
3. **Esperado:** Arbol creado

### 10.7 Asociar Dispositivo a Ubicacion
1. Editar el dispositivo, seleccionar ubicacion "Muelle"
2. Guardar
3. **Esperado:** Dispositivo con ubicacion asignada

---

## Prueba 11: Verificar Restriccion de Eliminacion

1. Intentar eliminar un usuario que es "Administrador del Sistema"
2. **Esperado:** El sistema lo permite (el AsistenteAdmin tiene permisos de eliminacion sobre usuarios que el creo)

---

## Prueba 11: Tema Oscuro y Cerrar Sesion

1. Cambiar tema oscuro/claro
2. Cerrar sesion
3. **Esperado:** Funciona correctamente
