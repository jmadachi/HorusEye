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

## Prueba 14: Tema Oscuro

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
