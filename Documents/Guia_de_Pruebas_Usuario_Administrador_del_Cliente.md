# Guia de Pruebas - Administrador del Cliente

**Rol:** Administrador del Cliente
**URL:** https://horuseye-app.mauricioadachi.dev
**Credenciales:** Crear desde el Admin del Sistema o Admin del Proveedor

---

## Alcance del Rol

El Administrador del Cliente puede gestionar usuarios Asistentes del Cliente asociados a SU cliente. Tambien puede gestionar dispositivos y ubicaciones dentro de su cliente. **NO** puede crear otros tipos de usuarios, ni gestionar proveedores ni clientes.

### Funcionalidades Disponibles

| Modulo | Ver | Crear | Editar | Eliminar |
|--------|-----|-------|--------|----------|
| Dashboard | Si | - | - | - |
| Activos | Si | No | No | No |
| Tags RFID | Si | No | No | No |
| Autorizaciones | Si | No | No | No |
| Reportes | Si | - | - | - |
| Usuarios | Si | Si (solo Asistente del Cliente) | Si | Si |
| Proveedores | No | - | - | - |
| Clientes | Si (el suyo) | No | No | No |
| Dispositivos | Si (los suyos) | Si | Si | No |
| Ubicaciones | Si (las suyas) | Si | Si | No |

### Datos Asociados

- **Cliente:** Solo ve su propio cliente
- **Dispositivos:** Solo ve los dispositivos de SU cliente
- **Ubicaciones:** Solo ve las ubicaciones de SU cliente

---

## Prueba 1: Login

1. Abrir https://horuseye-app.mauricioadachi.dev
2. Ingresar credenciales del Admin del Cliente
3. Iniciar Sesion
4. **Esperado:** Dashboard se carga. Menu muestra: Dashboard, Activos, Tags, Reportes, Autorizaciones, Usuarios, Dispositivos, Ubicaciones. **NO** muestra "Proveedores" ni "Clientes".

---

## Prueba 2: Dashboard

1. Verificar KPIs
2. **Esperado:** Los datos son solo de SU cliente

---

## Prueba 3: Crear Usuarios - Solo Asistente del Cliente

### 3.1 Crear Asistente del Cliente
1. Ir a "Usuarios"
2. Clic en "Nuevo Usuario"
3. Completar campos
4. Seleccionar rol "Asistente del Cliente"
5. Seleccionar SU cliente
6. Guardar
7. **Esperado:** Usuario creado, asociado a su cliente

### 3.2 Verificar que NO puede crear otros roles
1. Verificar dropdown de roles
2. **Esperado:** Solo aparece "Asistente del Cliente" como opcion

---

## Prueba 4: Ver Cliente Asociado

1. Verificar que NO hay menu "Clientes" (o si lo hay, es solo lectura)
2. **Esperado:** Solo ve informacion de SU cliente

---

## Prueba 5: Gestion de Dispositivos

### 5.1 Listar Dispositivos
1. Ir a "Dispositivos"
2. **Esperado:** Solo ve dispositivos de SU cliente

### 5.2 Crear Dispositivo
1. Clic en "Nuevo Dispositivo"
2. Completar campos
3. SU cliente se asigna automaticamente
4. Guardar
5. **Esperado:** Dispositivo creado

### 5.3 Editar Dispositivo
1. Editar un dispositivo propio
2. Guardar
3. **Esperado:** Datos actualizados

### 5.4 Eliminar Dispositivo
1. Verificar que NO hay boton de eliminar
2. **Esperado:** Solo AdminSistema puede eliminar

---

## Prueba 6: Gestion de Ubicaciones

### 6.1 Ver Ubicaciones
1. Ir a "Ubicaciones"
2. SU cliente se selecciona automaticamente
3. **Esperado:** Se muestra el arbol de ubicaciones de SU cliente

### 6.2 Crear Ubicaciones
1. Clic en "Nueva Ubicacion"
2. Crear nodos
3. **Esperado:** Arbol creado

### 6.3 Editar Ubicaciones
1. Editar un nodo
2. **Esperado:** Datos actualizados

### 6.4 Eliminar Ubicaciones
1. Verificar que NO hay boton de eliminar
2. **Esperado:** Solo AdminSistema puede eliminar

---

## Prueba 7: Modulos de Solo Lectura

### 7.1 Activos
1. Ir a "Activos"
2. **Esperado:** Solo lectura

### 7.2 Tags
1. Ir a "Tags RFID"
2. **Esperado:** Solo lectura

### 7.3 Autorizaciones
1. Ir a "Autorizaciones"
2. **Esperado:** Solo lectura

### 7.4 Reportes
1. Generar reportes
2. **Esperado:** Funciona con datos de SU cliente

### 7.5 Fabricantes
1. Verificar que NO hay menu "Fabricantes"
2. **Esperado:** No visible para este rol

---

## Prueba 8: Verificar Aislamiento

1. Con otro AdminCliente creado para un cliente diferente
2. Login con ese otro AdminCliente
3. Ir a Dispositivos
4. **Esperado:** NO ve dispositivos del primer cliente
5. Ir a Ubicaciones
6. **Esperado:** NO ve ubicaciones del primer cliente
7. Ir a Usuarios
8. **Esperado:** Solo ve usuarios de SU cliente

---

## Prueba 9: Tema Oscuro y Cerrar Sesion

1. Cambiar tema
2. Cerrar sesion
3. **Esperado:** Funciona correctamente
