# Guia de Pruebas - Administrador del Proveedor

**Rol:** Administrador del Proveedor
**URL:** https://horuseye.mauricioadachi.dev
**Credenciales:** Crear desde el Admin del Sistema

---

## Alcance del Rol

El Administrador del Proveedor puede gestionar usuarios asociados a SU proveedor: crear Asistentes del Proveedor, Administradores del Cliente y Asistentes del Cliente. Tambien puede gestionar clientes y dispositivos dentro de su proveedor. **NO** puede crear usuarios del Sistema ni gestionar otros proveedores.

### Funcionalidades Disponibles

| Modulo | Ver | Crear | Editar | Eliminar |
|--------|-----|-------|--------|----------|
| Dashboard | Si | - | - | - |
| Activos | Si | No | No | No |
| Tags RFID | Si | No | No | No |
| Autorizaciones | Si | No | No | No |
| Reportes | Si | - | - | - |
| Usuarios | Si | Si (AsistProv, AdminCli, AsistCli) | Si | Si |
| Proveedores | Si (el suyo) | No | No | No |
| Clientes | Si (los suyos) | Si | Si | No |
| Dispositivos | Si (los suyos) | Si | Si | No |
| Ubicaciones | Si (las de sus clientes) | Si | Si | No |

### Datos Asociados

- **Proveedor:** Solo ve y gestiona su propio proveedor
- **Clientes:** Solo ve los clientes asociados a su proveedor
- **Dispositivos:** Solo ve los dispositivos de sus clientes

---

## Prueba 1: Login

1. Abrir https://horuseye.mauricioadachi.dev
2. Ingresar credenciales del Admin del Proveedor
3. Iniciar Sesion
4. **Esperado:** Dashboard se carga. Menu muestra: Dashboard, Activos, Tags, Reportes, Autorizaciones, Usuarios, Clientes, Dispositivos, Ubicaciones.

---

## Prueba 2: Dashboard

1. Verificar KPIs y graficos
2. **Esperado:** Los datos mostrados son solo de los clientes de SU proveedor (aislamiento de datos)

---

## Prueba 3: Crear Usuarios - Roles Permitidos

### 3.1 Crear Asistente del Proveedor
1. Ir a "Usuarios"
2. Clic en "Nuevo Usuario"
3. Completar campos
4. Seleccionar rol "Asistente del Proveedor"
5. Seleccionar SU proveedor
6. Guardar
7. **Esperado:** Usuario creado, asociado a su proveedor

### 3.2 Crear Administrador del Cliente
1. Seleccionar rol "Administrador del Cliente"
2. Seleccionar un Cliente de SU proveedor
3. Guardar
4. **Esperado:** Usuario creado, asociado al cliente seleccionado

### 3.3 Crear Asistente del Cliente
1. Seleccionar rol "Asistente del Cliente"
2. Seleccionar un Cliente de SU proveedor
3. Guardar
4. **Esperado:** Usuario creado

---

## Prueba 4: Crear Usuarios - Roles NO Permitidos

### 4.1 Intentar Crear Administrador del Sistema
1. Ir a "Usuarios" > "Nuevo Usuario"
2. Verificar el dropdown de roles
3. **Esperado:** "Administrador del Sistema" NO aparece en las opciones

### 4.2 Intentar Crear Asistente del Administrador del Sistema
1. Verificar dropdown
2. **Esperado:** "Asistente del Administrador del Sistema" NO aparece

### 4.3 Intentar Crear Soporte del Sistema
1. Verificar dropdown
2. **Esperado:** "Soporte del Sistema" NO aparece

### 4.4 Intentar Crear Administrador del Proveedor
1. Verificar dropdown
2. **Esperado:** "Administrador del Proveedor" NO aparece (ya tiene uno)

---

## Prueba 5: Ver Proveedor Asociado

1. Ir a "Proveedores"
2. **Esperado:** Solo ve SU proveedor (no ve otros proveedores)
3. Verificar que NO hay boton "Nuevo Proveedor"
4. Verificar que NO hay acciones de editar/eliminar
5. **Esperado:** Solo lectura de su proveedor

---

## Prueba 6: Gestion de Clientes

### 6.1 Listar Clientes
1. Ir a "Clientes"
2. **Esperado:** Solo ve los clientes asociados a SU proveedor

### 6.2 Crear Cliente
1. Clic en "Nuevo Cliente"
2. Completar campos
3. El proveedor se asigna automaticamente al suyo
4. Guardar
5. **Esperado:** Cliente creado, asociado a su proveedor

### 6.3 Editar Cliente
1. Clic en editar un cliente propio
2. Modificar datos
3. Guardar
4. **Esperado:** Datos actualizados

### 6.4 Intentar Eliminar Cliente
1. Verificar que NO hay boton de eliminar en la tabla
2. **Esperado:** Solo AdminSistema puede eliminar clientes

---

## Prueba 7: Gestion de Dispositivos

### 7.1 Listar Dispositivos
1. Ir a "Dispositivos"
2. **Esperado:** Solo ve dispositivos de sus clientes

### 7.2 Crear Dispositivo
1. Clic en "Nuevo Dispositivo"
2. Completar campos
3. Seleccionar uno de SUS clientes
4. Guardar
5. **Esperado:** Dispositivo creado, asociado a su cliente

### 7.3 Editar Dispositivo
1. Editar un dispositivo propio
2. Guardar
3. **Esperado:** Datos actualizados

### 7.4 Intentar Eliminar Dispositivo
1. Verificar que NO hay boton de eliminar (solo AdminSistema puede)
2. **Esperado:** Sin opcion de eliminar

---

## Prueba 8: Gestion de Ubicaciones

### 8.1 Ver Ubicaciones
1. Ir a "Ubicaciones"
2. Seleccionar un cliente de SU proveedor
3. **Esperado:** Se muestra el arbol

### 8.2 Crear Ubicaciones
1. Clic en "Nueva Ubicacion"
2. Crear nodo raiz e hijos
3. **Esperado:** Arbol creado

### 8.3 Eliminar Ubicaciones
1. Verificar que NO hay boton de eliminar (solo AdminSistema)
2. **Esperado:** Sin opcion de eliminar

---

## Prueba 9: Configuracion de Antena para un Cliente

El Admin del Proveedor puede configurar antenas para sus clientes. Los fabricantes ya deben estar creados por el Admin del Sistema.

### 9.1 Crear Cliente (si no existe)
1. Ir a "Clientes" > "Nuevo Cliente"
2. Nombre: "Logistica Express", RUC: 1111222233334
3. Guardar
4. **Esperado:** Cliente creado, asociado a SU proveedor

### 9.2 Registrar Dispositivo RFID
1. Ir a "Dispositivos" > "Nuevo Dispositivo"
2. Completar:
   - **Nombre:** Puerta de Entrada
   - **Fabricante:** Chainway (ya configurado por AdminSistema)
   - **Modelo:** U300
   - **Direccion IP:** 192.168.1.50
   - **Cliente:** Logistica Express
   - **Tipo:** Fijo
   - **Endpoint:** `https://horuseye-api.mauricioadachi.dev/api/eventos-rfid/chainway`
3. Guardar
4. **Esperado:** Dispositivo registrado para el cliente

### 9.3 Crear Ubicaciones para el Cliente
1. Ir a "Ubicaciones", seleccionar "Logistica Express"
2. Crear: Bodega → Entrada
3. **Esperado:** Arbol de ubicaciones creado

### 9.4 Asociar Dispositivo a Ubicacion
1. Editar el dispositivo "Puerta de Entrada"
2. Seleccionar ubicacion "Entrada"
3. Guardar
4. **Esperado:** Dispositivo con ubicacion asignada

### 9.5 Verificar que el Dispositivo Aparece
1. Ir a "Dispositivos"
2. **Esperado:** El dispositivo "Puerta de Entrada" aparece en la lista, solo se ven dispositivos de los clientes de SU proveedor

---

## Prueba 10: Modulos de Solo Lectura

### 9.1 Activos
1. Ir a "Activos"
2. **Esperado:** Solo lectura, sin botones de CRUD

### 9.2 Tags
1. Ir a "Tags RFID"
2. **Esperado:** Solo lectura

### 9.3 Autorizaciones
1. Ir a "Autorizaciones"
2. **Esperado:** Solo lectura

### 9.4 Reportes
1. Generar y exportar reportes
2. **Esperado:** Funciona (los reportes incluyen datos de sus clientes)

### 9.5 Fabricantes
1. Ir a "Fabricantes"
2. **Esperado:** Solo lectura

---

## Prueba 10: Verificar Aislamiento

1. Con otro AdminProveedor creado por el AdminSistema
2. Hacer login con ese otro AdminProveedor
3. Ir a Clientes
4. **Esperado:** NO ve los clientes del primer proveedor
5. Ir a Usuarios
6. **Esperado:** NO ve los usuarios del primer proveedor

---

## Prueba 11: Tema Oscuro y Cerrar Sesion

1. Cambiar tema
2. Cerrar sesion
3. **Esperado:** Funciona correctamente
