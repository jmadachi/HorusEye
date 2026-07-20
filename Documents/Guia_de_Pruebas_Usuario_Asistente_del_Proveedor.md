# Guia de Pruebas - Asistente del Proveedor

**Rol:** Asistente del Proveedor
**URL:** https://horuseye.mauricioadachi.dev
**Credenciales:** Crear desde el Admin del Sistema o Admin del Proveedor

---

## Alcance del Rol

El Asistente del Proveedor tiene **solo lectura** dentro del ambito de SU proveedor. Puede ver clientes, dispositivos y ubicaciones de su proveedor, pero **NO** puede crear, editar ni eliminar nada. **NO** puede crear usuarios.

### Funcionalidades Disponibles

| Modulo | Ver | Crear | Editar | Eliminar |
|--------|-----|-------|--------|----------|
| Dashboard | Si | - | - | - |
| Activos | Si | No | No | No |
| Tags RFID | Si | No | No | No |
| Autorizaciones | Si | No | No | No |
| Reportes | Si | - | - | - |
| Usuarios | Si (solo los de su proveedor) | No | No | No |
| Proveedores | Si (el suyo) | No | No | No |
| Clientes | Si (los de su proveedor) | No | No | No |
| Dispositivos | Si (los de su proveedor) | No | No | No |
| Ubicaciones | Si (las de sus clientes) | No | No | No |

---

## Prueba 1: Login

1. Abrir https://horuseye.mauricioadachi.dev
2. Ingresar credenciales del Asistente del Proveedor
3. Iniciar Sesion
4. **Esperado:** Dashboard se carga. Menu muestra modulos de lectura.

---

## Prueba 2: Dashboard

1. Verificar KPIs
2. **Esperado:** Datos de los clientes de SU proveedor

---

## Prueba 3: Proveedor - Solo Lectura

1. Ir a "Proveedores"
2. **Esperado:** Solo ve SU proveedor
3. Verificar que NO hay acciones de crear/editar/eliminar
4. **Esperado:** Solo lectura

---

## Prueba 4: Clientes - Solo Lectura

1. Ir a "Clientes"
2. **Esperado:** Solo ve los clientes de SU proveedor
3. Verificar que NO hay boton "Nuevo Cliente"
4. Verificar que NO hay acciones de editar/eliminar
5. **Esperado:** Solo lectura

---

## Prueba 5: Dispositivos - Solo Lectura

1. Ir a "Dispositivos"
2. **Esperado:** Solo ve dispositivos de los clientes de SU proveedor
3. Verificar que NO hay boton "Nuevo Dispositivo"
4. Verificar que NO hay acciones de editar/eliminar
5. **Esperado:** Solo lectura

---

## Prueba 6: Ubicaciones - Solo Lectura

1. Ir a "Ubicaciones"
2. Seleccionar un cliente de SU proveedor
3. **Esperado:** Se muestra el arbol
4. Verificar que NO hay boton "Nueva Ubicacion"
5. Verificar que NO hay acciones de agregar hijo/eliminar
6. **Esperado:** Solo lectura

---

## Prueba 7: Usuarios - Solo Lectura

1. Ir a "Usuarios"
2. **Esperado:** Ve los usuarios asociados a SU proveedor
3. Verificar que NO hay boton "Nuevo Usuario"
4. Verificar que NO hay acciones de editar/eliminar
5. **Esperado:** Solo lectura

---

## Prueba 8: Modulos Restantes - Solo Lectura

### 8.1 Activos
1. Ir a "Activos"
2. **Esperado:** Solo lectura

### 8.2 Tags
1. Ir a "Tags RFID"
2. **Esperado:** Solo lectura

### 8.3 Autorizaciones
1. Ir a "Autorizaciones"
2. **Esperado:** Solo lectura

### 8.4 Reportes
1. Generar reportes y exportar
2. **Esperado:** Funciona

### 8.5 Fabricantes
1. Verificar que NO hay menu "Fabricantes"
2. **Esperado:** No visible para este rol

---

## Prueba 9: Verificar Aislamiento

1. Con otro Asistente de otro proveedor
2. Login
3. Ir a Clientes
4. **Esperado:** NO ve los clientes del primer proveedor
5. Ir a Dispositivos
6. **Esperado:** NO ve dispositivos de otros proveedores

---

## Prueba 10: Tema Oscuro y Cerrar Sesion

1. Cambiar tema
2. Cerrar sesion
3. **Esperado:** Funciona correctamente
