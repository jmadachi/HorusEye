# Guia de Pruebas - Asistente del Cliente

**Rol:** Asistente del Cliente
**URL:** https://horuseye.mauricioadachi.dev
**Credenciales:** Crear desde el Admin del Sistema, Admin del Proveedor o Admin del Cliente

---

## Alcance del Rol

El Asistente del Cliente tiene **solo lectura** estricta dentro de SU cliente. Es el rol con menos permisos del sistema. **NO** puede crear, editar ni eliminar nada. **NO** puede crear usuarios.

### Funcionalidades Disponibles

| Modulo | Ver | Crear | Editar | Eliminar |
|--------|-----|-------|--------|----------|
| Dashboard | Si | - | - | - |
| Activos | Si | No | No | No |
| Tags RFID | Si | No | No | No |
| Autorizaciones | Si | No | No | No |
| Reportes | Si | - | - | - |
| Usuarios | No | - | - | - |
| Proveedores | No | - | - | - |
| Clientes | No | - | - | - |
| Dispositivos | Si (los de su cliente) | No | No | No |
| Ubicaciones | Si (las de su cliente) | No | No | No |

---

## Prueba 1: Login

1. Abrir https://horuseye.mauricioadachi.dev
2. Ingresar credenciales del Asistente del Cliente
3. Iniciar Sesion
4. **Esperado:** Dashboard se carga. Menu muestra: Dashboard, Activos, Tags, Reportes, Autorizaciones, Dispositivos, Ubicaciones. **NO** muestra "Usuarios", "Proveedores", "Clientes", "Fabricantes".

---

## Prueba 2: Dashboard

1. Verificar KPIs
2. **Esperado:** Datos solo de SU cliente

---

## Prueba 3: Dispositivos - Solo Lectura

1. Ir a "Dispositivos"
2. **Esperado:** Solo ve dispositivos de SU cliente
3. Verificar que NO hay boton "Nuevo Dispositivo"
4. Verificar que NO hay acciones de editar/eliminar
5. **Esperado:** Solo lectura

---

## Prueba 4: Ubicaciones - Solo Lectura

1. Ir a "Ubicaciones"
2. SU cliente se selecciona automaticamente
3. **Esperado:** Arbol de ubicaciones visible
4. Verificar que NO hay boton "Nueva Ubicacion"
5. Verificar que NO hay acciones de agregar/eliminar
6. **Esperado:** Solo lectura

---

## Prueba 5: Activos - Solo Lectura

1. Ir a "Activos"
2. **Esperado:** Tabla visible
3. Verificar que NO hay boton "Registrar Activo"
4. Verificar que NO hay acciones de editar/eliminar
5. **Esperado:** Solo lectura

---

## Prueba 6: Tags - Solo Lectura

1. Ir a "Tags RFID"
2. **Esperado:** Tabla visible
3. Verificar que NO hay boton "Registrar Tag"
4. Verificar que NO hay acciones de cambiar estado
5. **Esperado:** Solo lectura

---

## Prueba 7: Autorizaciones - Solo Lectura

1. Ir a "Autorizaciones"
2. **Esperado:** Tabla visible
3. Verificar que NO hay boton "Nueva Autorizacion"
4. Verificar que NO hay acciones de revocar/eliminar
5. **Esperado:** Solo lectura

---

## Prueba 8: Reportes

1. Ir a "Reportes"
2. Generar reporte y exportar
3. **Esperado:** Funciona

---

## Prueba 9: Modulos NO Visibles

1. Verificar que NO hay menu "Usuarios"
2. Verificar que NO hay menu "Proveedores"
3. Verificar que NO hay menu "Clientes"
4. Verificar que NO hay menu "Fabricantes"
5. **Esperado:** Estos menus no aparecen en la navegacion

---

## Prueba 10: Verificar Aislamiento

1. Con otro Asistente de otro cliente
2. Login
3. Ir a Dispositivos
4. **Esperado:** NO ve dispositivos del primer cliente
5. Ir a Ubicaciones
6. **Esperado:** NO ve ubicaciones del primer cliente

---

## Prueba 11: Tema Oscuro y Cerrar Sesion

1. Cambiar tema
2. Cerrar sesion
3. **Esperado:** Funciona correctamente
