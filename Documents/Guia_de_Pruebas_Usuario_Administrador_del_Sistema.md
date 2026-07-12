# Guia de Pruebas - Administrador del Sistema

**Rol:** Administrador del Sistema
**URL:** https://horuseye-app.mauricioadachi.dev
**Credenciales:** admin@horuseye.com / Admin123!

---

## Alcance del Rol

El Administrador del Sistema tiene **acceso total** a todas las funcionalidades de la aplicacion. Puede crear todos los tipos de usuarios, gestionar todos los modulos y ver todos los datos sin restricciones.

### Funcionalidades Disponibles

| Modulo | Ver | Crear | Editar | Eliminar |
|--------|-----|-------|--------|----------|
| Dashboard | Si | - | - | - |
| Activos | Si | Si | Si | Si |
| Tags RFID | Si | Si | Si | Si |
| Autorizaciones | Si | Si | Si | Si |
| Reportes | Si | - | - | - |
| Usuarios | Si | Si (todos los roles) | Si | Si |
| Proveedores | Si | Si | Si | Si |
| Clientes | Si | Si | Si | Si |
| Dispositivos | Si | Si | Si | Si |
| Ubicaciones | Si | Si | Si | Si |
| Fabricantes | Si | Si | Si | Si |

---

## Prueba 1: Login

1. Abrir https://horuseye-app.mauricioadachi.dev
2. Ingresar email: `admin@horuseye.com`
3. Ingresar contrasena: `Admin123!`
4. Hacer clic en "Iniciar Sesion"
5. **Esperado:** Redirige al Dashboard. En el header se muestra el nombre "Administrador" y el rol "Administrador del Sistema".

---

## Prueba 2: Dashboard

1. Una vez logueado, verificar que se muestra el Dashboard
2. **Esperado:** Se muestran las tarjetas de KPIs (Total Activos, Activos Dentro/Fuera, Tags, Ingresos/Salidas Hoy)
3. Verificar que los graficos se cargan (barras, torta, tendencias)
4. Verificar que la tabla de movimientos recientes muestra datos
5. Verificar el indicador de conexion SignalR (punto verde "Conectado")

---

## Prueba 3: Gestion de Usuarios

### 3.1 Listar Usuarios
1. Hacer clic en "Usuarios" en el menu lateral
2. **Esperado:** Se muestra la tabla con los usuarios existentes (al menos 2: admin y consulta)
3. Verificar que se muestra el rol de cada usuario
4. Probar la paginacion y el selector de items por pagina

### 3.2 Crear Usuario - Administrador del Proveedor
1. Hacer clic en "Nuevo Usuario"
2. Completar: Nombre de Usuario, Email, Contrasena
3. En "Rol", seleccionar "Administrador del Proveedor"
4. Seleccionar un Proveedor existente (o crear uno primero)
5. Hacer clic en "Guardar"
6. **Esperado:** Mensaje de exito, el usuario aparece en la lista

### 3.3 Crear Usuario - Asistente del Proveedor
1. Hacer clic en "Nuevo Usuario"
2. Completar campos
3. Seleccionar rol "Asistente del Proveedor"
4. Seleccionar Proveedor
5. Guardar
6. **Esperado:** Usuario creado exitosamente

### 3.4 Crear Usuario - Administrador del Cliente
1. Hacer clic en "Nuevo Usuario"
2. Completar campos
3. Seleccionar rol "Administrador del Cliente"
4. Seleccionar Cliente (y Proveedor si aplica)
5. Guardar
6. **Esperado:** Usuario creado exitosamente

### 3.5 Crear Usuario - Asistente del Cliente
1. Hacer clic en "Nuevo Usuario"
2. Completar campos
3. Seleccionar rol "Asistente del Cliente"
4. Seleccionar Cliente
5. Guardar
6. **Esperado:** Usuario creado exitosamente

### 3.6 Crear Usuario - Asistente del Administrador del Sistema
1. Hacer clic en "Nuevo Usuario"
2. Completar campos
3. Seleccionar rol "Asistente del Administrador del Sistema"
4. Guardar
5. **Esperado:** Usuario creado exitosamente

### 3.7 Crear Usuario - Soporte del Sistema
1. Hacer clic en "Nuevo Usuario"
2. Completar campos
3. Seleccionar rol "Soporte del Sistema"
4. Guardar
5. **Esperado:** Usuario creado exitosamente

### 3.8 Editar Usuario
1. En la tabla de usuarios, hacer clic en el icono de editar (lapiz) de un usuario
2. Modificar el nombre de usuario o el email
3. Guardar cambios
4. **Esperado:** Mensaje de exito, datos actualizados

### 3.9 Resetear Contrasena
1. Hacer clic en el icono de llave de un usuario
2. Ingresar nueva contrasena
3. Confirmar
4. **Esperado:** Contrasena actualizada

### 3.10 Eliminar Usuario
1. Hacer clic en el icono de eliminar (papelera) de un usuario (que no sea el propio)
2. Confirmar la eliminacion
3. **Esperado:** Usuario eliminado de la lista

---

## Prueba 4: Gestion de Proveedores

### 4.1 Listar Proveedores
1. Hacer clic en "Proveedores" en el menu
2. **Esperado:** Tabla vacia o con proveedores existentes

### 4.2 Crear Proveedor
1. Hacer clic en "Nuevo Proveedor"
2. Completar: Nombre, Razon Social, RUC (13 digitos), Direccion, Telefono, Email
3. Guardar
4. **Esperado:** Proveedor creado, aparece en la tabla

### 4.3 Editar Proveedor
1. Clic en editar (lapiz) del proveedor
2. Modificar datos
3. Guardar
4. **Esperado:** Datos actualizados

### 4.4 Eliminar Proveedor
1. Clic en eliminar (papelera)
2. Confirmar
3. **Esperado:** Proveedor eliminado

---

## Prueba 5: Gestion de Clientes

### 5.1 Listar Clientes
1. Hacer clic en "Clientes"
2. **Esperado:** Tabla de clientes

### 5.2 Crear Cliente
1. Hacer clic en "Nuevo Cliente"
2. Completar: Nombre, Razon Social, RUC, Direccion, Telefono, Email
3. Seleccionar Proveedor asociado (opcional)
4. Guardar
5. **Esperado:** Cliente creado

### 5.3 Editar Cliente
1. Clic en editar
2. Modificar datos
3. Guardar
4. **Esperado:** Datos actualizados

### 5.4 Eliminar Cliente
1. Clic en eliminar
2. Confirmar
3. **Esperado:** Cliente eliminado

---

## Prueba 6: Gestion de Dispositivos RFID

### 6.1 Listar Dispositivos
1. Hacer clic en "Dispositivos"
2. **Esperado:** Tabla de dispositivos (vacia inicialmente)

### 6.2 Crear Dispositivo
1. Hacer clic en "Nuevo Dispositivo"
2. Completar: Nombre, Fabricante (ej: Chainway), Modelo (ej: U300), Direccion IP
3. Seleccionar Cliente asociado
4. Seleccionar Tipo (Fijo/Portatil/Arco)
5. Guardar
6. **Esperado:** Dispositivo registrado

### 6.3 Editar Dispositivo
1. Clic en editar
2. Modificar datos (ej: cambiar Endpoint API)
3. Guardar
4. **Esperado:** Datos actualizados

### 6.4 Eliminar Dispositivo
1. Clic en eliminar
2. Confirmar
3. **Esperado:** Dispositivo eliminado

---

## Prueba 7: Gestion de Ubicaciones (Arbol Jerarquico)

### 7.1 Ver Arbol
1. Hacer clic en "Ubicaciones"
2. Seleccionar un Cliente en el dropdown
3. **Esperado:** Se muestra el arbol de ubicaciones (vacio si es nuevo)

### 7.2 Crear Nodo Raiz
1. Clic en "Nueva Ubicacion"
2. Completar: Nombre (ej: "Bodega Principal"), Tipo de nodo (ej: "Bodega")
3. Guardar
4. **Esperado:** Nodo raiz creado en el arbol

### 7.3 Crear Nodo Hijo
1. En el arbol, hacer clic en el icono de "+" junto al nodo padre
2. Completar: Nombre (ej: "Sector Norte"), Tipo (ej: "Sector")
3. Guardar
4. **Esperado:** Nodo hijo anidado bajo el padre

### 7.4 Crear Sub-hijo
1. Repetir el proceso sobre el nodo hijo
2. **Esperado:** Arbol de 3 niveles

### 7.5 Expandir/Colapsar Nodos
1. Clic en la flecha del nodo padre
2. **Esperado:** Se expanden/colapsan los hijos

### 7.6 Eliminar Nodo
1. Clic en eliminar (papelera) de un nodo
2. **Esperado:** Nodo eliminado (y sus hijos si existen)

---

## Prueba 8: Gestion de Fabricantes

### 8.1 Listar Fabricantes
1. Hacer clic en "Fabricantes"
2. **Esperado:** Lista de fabricantes (vacia inicialmente)

### 8.2 Crear Fabricante
1. Hacer clic en "Nuevo Fabricante"
2. Completar: Nombre (ej: "Chainway"), Descripcion
3. Guardar
4. **Esperado:** Fabricante creado

### 8.3 Agregar Campo de Payload
1. Expandir el fabricante creado
2. Clic en "+ Agregar campo"
3. Completar: Campo Externo (ej: "epc"), Campo Interno (seleccionar "EPC"), Tipo (String), Requerido (Si)
4. Guardar
5. **Esperado:** Campo aparece en la tabla de campos

### 8.4 Agregar Mas Campos
1. Agregar campos para: antenna (-> Antenna, int), rssi (-> RSSI, int), timestamp (-> Timestamp, datetime)
2. **Esperado:** 4 campos configurados

### 8.5 Editar Fabricante
1. Clic en editar (lapiz)
2. Modificar descripcion
3. Guardar
4. **Esperado:** Datos actualizados

### 8.6 Eliminar Campo
1. Clic en eliminar (papelera) de un campo
2. **Esperado:** Campo eliminado

### 8.7 Eliminar Fabricante
1. Clic en eliminar fabricante
2. Confirmar
3. **Esperado:** Fabricante y sus campos eliminados

---

## Prueba 9: Activos

### 9.1 Listar Activos
1. Hacer clic en "Activos"
2. **Esperado:** Tabla de activos existentes (100 activos de prueba)

### 9.2 Crear Activo
1. Clic en "Registrar Activo"
2. Completar: Placa, Nombre, Categoria, Responsable, Tag RFID (seleccionar disponible)
3. Guardar
4. **Esperado:** Activo creado

### 9.3 Editar Activo
1. Clic en editar
2. Modificar datos
3. Guardar
4. **Esperado:** Datos actualizados

### 9.4 Eliminar Activo
1. Clic en eliminar
2. Confirmar
3. **Esperado:** Activo eliminado

---

## Prueba 10: Tags RFID

### 10.1 Listar Tags
1. Hacer clic en "Tags RFID"
2. **Esperado:** Tabla de tags

### 10.2 Crear Tag
1. Clic en "Registrar Tag"
2. Ingresar ID del tag
3. Guardar
4. **Esperado:** Tag creado como DISPONIBLE

### 10.3 Cambiar Estado
1. Clic en el icono de estado de un tag
2. Cambiar a DISPONIBLE o reportar DANADO
3. **Esperado:** Estado actualizado

---

## Prueba 11: Autorizaciones

### 11.1 Listar Autorizaciones
1. Hacer clic en "Autorizaciones"
2. **Esperado:** Tabla de autorizaciones

### 11.2 Crear Autorizacion
1. Clic en "Nueva Autorizacion"
2. Seleccionar Activo
3. Ingresar "Autorizado por"
4. Guardar
5. **Esperado:** Autorizacion activa creada

### 11.3 Revocar Autorizacion
1. Clic en "Revocar" de una autorizacion activa
2. **Esperado:** Autorizacion marcada como inactiva

### 11.4 Eliminar Autorizacion
1. Clic en eliminar
2. Confirmar
3. **Esperado:** Autorizacion eliminada

---

## Prueba 12: Reportes

### 12.1 Generar Reporte
1. Hacer clic en "Reportes"
2. Seleccionar tipo de reporte (ej: Movimientos)
3. Seleccionar rango de fechas
4. Clic en "Generar"
5. **Esperado:** Se muestran los resultados

### 12.2 Exportar a Excel
1. Con un reporte generado, clic en "Exportar a Excel"
2. **Esperado:** Se descarga un archivo .xlsx

---

## Prueba 13: Verificar Aislamiento de Datos

1. Con el admin, crear un Proveedor "Proveedor A"
2. Crear un Cliente "Cliente A" asociado a Proveedor A
3. Crear un usuario "AdminProveedor" con rol "Administrador del Proveedor" asociado a Proveedor A
4. Crear un usuario "AdminCliente" con rol "Administrador del Cliente" asociado a Cliente A
5. **Cerrar sesion** del admin
6. **Ingresar** con AdminProveedor
7. Ir a Clientes
8. **Esperado:** Solo ve "Cliente A" (no ve Clientes de otros proveedores)
9. Ir a Dispositivos
10. **Esperado:** Solo ve dispositivos de Cliente A
11. **Cerrar sesion**
12. **Ingresar** con AdminCliente
13. Ir a Dispositivos
14. **Esperado:** Solo ve dispositivos de Cliente A (no ve dispositivos de otros clientes)

---

## Prueba 14: Configuracion Completa de Antena (Flujo End-to-End)

Esta prueba demuestra el proceso completo para que el sistema este listo para recibir y registrar datos de una antena RFID nueva. Es el flujo principal del sistema.

### Paso 1: Configurar Fabricante (que antenas soporta el sistema)

1. Ir a "Fabricantes"
2. Clic en "Nuevo Fabricante"
3. Completar:
   - **Nombre:** Chainway
   - **Descripcion:** Lectores RFID UHF de la marca Chainway
   - **URL Documentacion:** (opcional) URL a la documentacion del fabricante
4. Clic en "Guardar"
5. **Esperado:** Fabricante creado en la lista

### Paso 2: Configurar Campos del Payload (que envia la antena)

1. En la lista de fabricantes, hacer clic en la flecha para expandir "Chainway"
2. Verificar que aparece "Campos del Payload" (vacio)
3. Clic en "+ Agregar campo"
4. Agregar campo 1:
   - **Campo Externo:** `epc` (nombre del campo en el JSON que envia la antena)
   - **Campo Interno:** Seleccionar `EPC` (campo interno del sistema)
   - **Tipo:** String
   - **Requerido:** Si
   - **Orden:** 0
   - Guardar
5. Agregar campo 2:
   - **Campo Externo:** `antenna`
   - **Campo Interno:** `Antenna`
   - **Tipo:** Int
   - **Requerido:** Si
   - **Orden:** 1
   - Guardar
6. Agregar campo 3:
   - **Campo Externo:** `rssi`
   - **Campo Interno:** `RSSI`
   - **Tipo:** Int
   - **Requerido:** Si
   - **Orden:** 2
   - Guardar
7. Agregar campo 4:
   - **Campo Externo:** `timestamp`
   - **Campo Interno:** `Timestamp`
   - **Tipo:** DateTime
   - **Requerido:** Si
   - **Orden:** 3
   - Guardar
8. **Esperado:** 4 campos configurados en la tabla. El sistema ahora sabe como interpretar el JSON que envia una antena Chainway.

> **Nota:** Si se necesita soportar otro fabricante (ej: Zebra, Impinj), se crea otro Fabricante con sus propios campos de payload. Cada fabricante puede tener una estructura JSON diferente.

### Paso 3: Crear Proveedor (quien instala/mantiene las antenas)

1. Ir a "Proveedores"
2. Clic en "Nuevo Proveedor"
3. Completar:
   - **Nombre:** Keonn Technologies
   - **Razon Social:** Keonn Technologies SL
   - **RUC:** 1234567890123
   - **Direccion:** (direccion del proveedor)
   - **Telefono:** (telefono)
   - **Email:** (email de contacto)
4. Guardar
5. **Esperado:** Proveedor creado

### Paso 4: Crear Cliente (quien usa las antenas)

1. Ir a "Clientes"
2. Clic en "Nuevo Cliente"
3. Completar:
   - **Nombre:** Empresa ABC
   - **Razon Social:** Empresa ABC SA
   - **RUC:** 9876543210123
   - **Proveedor:** Seleccionar "Keonn Technologies" (el proveedor creado)
4. Guardar
5. **Esperado:** Cliente creado, asociado al proveedor

### Paso 5: Registrar Dispositivo RFID (la antena fisica)

1. Ir a "Dispositivos"
2. Clic en "Nuevo Dispositivo"
3. Completar:
   - **Nombre:** Puerta Principal Bodega 1
   - **Fabricante:** Chainway
   - **Modelo:** U300
   - **Direccion IP:** 192.168.1.100 (IP del dispositivo en la red local)
   - **Ubicacion:** (texto libre, ej: "Entrada principal")
   - **Cliente:** Seleccionar "Empresa ABC"
   - **Tipo:** Fijo (para portales) o Portatil (para pistolas)
   - **Endpoint API:** `https://horuseye-api.mauricioadachi.dev/api/eventos-rfid/chainway`
4. Guardar
5. **Esperado:** Dispositivo registrado. El sistema ahora tiene toda la informacion para procesar eventos de esta antena.

> **Importante:** El **Endpoint API** es la URL a donde el dispositivo fisico enviara los eventos RFID. Debe coincidir con el formato configurado en el Fabricante.

### Paso 6: Configurar Ubicacion (donde esta la antena)

1. Ir a "Ubicaciones"
2. Seleccionar el cliente "Empresa ABC" en el dropdown
3. Clic en "Nueva Ubicacion"
4. Crear nodo raiz:
   - **Nombre:** Bodega 1
   - **Tipo de nodo:** Bodega
   - Guardar
5. Crear nodo hijo (haciendo clic en "+" sobre "Bodega 1"):
   - **Nombre:** Zona de Carga
   - **Tipo de nodo:** Zona
   - Guardar
6. Crear sub-hijo:
   - **Nombre:** Puerta Principal
   - **Tipo de nodo:** Puerta
   - Guardar
7. **Esperado:** Arbol de 3 niveles: Bodega 1 → Zona de Carga → Puerta Principal

### Paso 7: Asociar Dispositivo a Ubicacion (opcional pero recomendado)

1. Ir a "Dispositivos"
2. Editar el dispositivo "Puerta Principal Bodega 1"
3. Seleccionar la ubicacion "Puerta Principal" en el arbol
4. Guardar
5. **Esperado:** El dispositivo ahora tiene su ubicacion fisica registrada

### Paso 8: Verificar que el Dispositivo esta Listo

1. Ir a "Dispositivos"
2. Verificar que el dispositivo muestra:
   - Fabricante: Chainway
   - Modelo: U300
   - IP: 192.168.1.100
   - Cliente: Empresa ABC
   - Endpoint: https://horuseye-api.mauricioadachi.dev/api/eventos-rfid/chainway
3. **Esperado:** Todo configurado correctamente

### Paso 9: Probar que el Sistema Recibe Eventos

1. Abrir una terminal
2. Ejecutar (simula lo que hace la antena fisica):
   ```bash
   curl -X POST https://horuseye-api.mauricioadachi.dev/api/eventos-rfid/chainway \
     -H "Content-Type: application/json" \
     -d '{"epc":"TAG-TEST-001","antenna":1,"rssi":-45,"timestamp":"2026-07-12T23:00:00Z","reader_id":"U300-001"}'
   ```
3. **Esperado:** Respuesta `"success": true` (si el tag esta registrado) o `"TAG no registrado"` (si el tag aun no existe en el sistema)

### Paso 10: Verificar en el Dashboard

1. Ir a "Dashboard"
2. Verificar que el contador de "Ingresos Hoy" aumento en 1
3. Verificar que el movimiento aparece en la tabla de "Movimientos Recientes"
4. **Esperado:** El movimiento muestra el TAG, el activo asociado, tipo INGRESO, y el punto de lectura

### Resumen del Flujo

```
┌─────────────────────────────────────────────────────────────┐
│  FLUJO DE CONFIGURACION DE ANTENA EN LA APLICACION          │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  1. FABRICANTES → Definir que antenas soporta el sistema     │
│     └── Configurar campos del payload JSON                    │
│                                                               │
│  2. PROVEEDORES → Quien instala/mantiene el hardware         │
│                                                               │
│  3. CLIENTES → Quien usa las antenas                          │
│     └── Asociado a un proveedor                               │
│                                                               │
│  4. DISPOSITIVOS → Registrar la antena fisica                 │
│     └── IP, modelo, cliente, endpoint API                     │
│                                                               │
│  5. UBICACIONES → Donde esta la antena (arbol jerarquico)    │
│     └── Bodega → Zona → Puerta                                │
│                                                               │
│  6. El dispositivo fisico envia eventos al endpoint           │
│     └── POST /api/eventos-rfid/chainway                       │
│                                                               │
│  7. El sistema procesa y registra el movimiento               │
│     └── Aparece en el Dashboard en tiempo real                │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

---

## Prueba 15: Tema Oscuro

1. En el header, hacer clic en el icono de luna/sol
2. **Esperado:** La interfaz cambia a modo oscuro
3. Recargar la pagina
4. **Esperado:** El tema se mantiene (persiste en localStorage)
5. Volver a cambiar al modo claro
6. **Esperado:** La interfaz vuelve al tema claro

---

## Prueba 15: Cerrar Sesion

1. Hacer clic en "Salir" en el header
2. **Esperado:** Redirige al login, no se puede acceder a /dashboard sin autenticacion
