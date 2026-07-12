# Guia de Pruebas - Soporte del Sistema

**Rol:** Soporte del Sistema
**URL:** https://horuseye-app.mauricioadachi.dev
**Credenciales:** Crear desde el Admin del Sistema

---

## Alcance del Rol

El Soporte del Sistema tiene **solo lectura** en todo el sistema. Puede ver todos los datos de todos los clientes y proveedores, pero **NO** puede crear, editar ni eliminar nada. No puede crear usuarios.

### Funcionalidades Disponibles

| Modulo | Ver | Crear | Editar | Eliminar |
|--------|-----|-------|--------|----------|
| Dashboard | Si | - | - | - |
| Activos | Si | No | No | No |
| Tags RFID | Si | No | No | No |
| Autorizaciones | Si | No | No | No |
| Reportes | Si | - | - | - |
| Usuarios | Si | No | No | No |
| Proveedores | Si | No | No | No |
| Clientes | Si | No | No | No |
| Dispositivos | Si | No | No | No |
| Ubicaciones | Si | No | No | No |
| Fabricantes | Si | No | No | No |

---

## Prueba 1: Login

1. Abrir https://horuseye-app.mauricioadachi.dev
2. Ingresar credenciales del Soporte (creado previamente)
3. Iniciar Sesion
4. **Esperado:** Redirige al Dashboard. Se muestra el rol "Soporte del Sistema".

---

## Prueba 2: Dashboard

1. Verificar que el Dashboard carga correctamente
2. **Esperado:** KPIs, graficos y tabla de movimientos visibles
3. Verificar que NO hay botones de accion (no hay "Registrar" ni "Editar")

---

## Prueba 3: Activos - Solo Lectura

1. Ir a "Activos"
2. **Esperado:** Se muestra la tabla de activos
3. Verificar que NO hay boton "Registrar Activo"
4. Verificar que NO hay columnas de acciones (no hay botones de editar/eliminar)
5. Probar paginacion
6. **Esperado:** Paginacion funciona, pero sin opciones de modificacion

---

## Prueba 4: Tags RFID - Solo Lectura

1. Ir a "Tags RFID"
2. **Esperado:** Tabla de tags visible
3. Verificar que NO hay boton "Registrar Tag"
4. Verificar que NO hay acciones de cambiar estado o reportar dano
5. **Esperado:** Solo lectura

---

## Prueba 5: Autorizaciones - Solo Lectura

1. Ir a "Autorizaciones"
2. **Esperado:** Tabla visible
3. Verificar que NO hay boton "Nueva Autorizacion"
4. Verificar que NO hay botones de revocar o eliminar
5. **Esperado:** Solo lectura

---

## Prueba 6: Reportes

1. Ir a "Reportes"
2. Generar un reporte
3. Exportar a Excel
4. **Esperado:** Funciona (el Soporte SI puede generar reportes)

---

## Prueba 7: Usuarios - Solo Lectura

1. Ir a "Usuarios"
2. **Esperado:** Se muestra la tabla de usuarios
3. Verificar que NO hay boton "Nuevo Usuario"
4. Verificar que NO hay acciones de editar, resetear contrasena o eliminar
5. **Esperado:** Solo lectura, puede ver todos los usuarios del sistema

---

## Prueba 8: Proveedores - Solo Lectura

1. Ir a "Proveedores"
2. **Esperado:** Tabla visible
3. Verificar que NO hay boton "Nuevo Proveedor"
4. Verificar que NO hay acciones de editar/eliminar
5. **Esperado:** Solo lectura

---

## Prueba 9: Clientes - Solo Lectura

1. Ir a "Clientes"
2. **Esperado:** Tabla visible (ve TODOS los clientes, sin aislamiento)
3. Verificar que NO hay boton "Nuevo Cliente"
4. Verificar que NO hay acciones de editar/eliminar
5. **Esperado:** Solo lectura

---

## Prueba 10: Dispositivos - Solo Lectura

1. Ir a "Dispositivos"
2. **Esperado:** Tabla visible (ve TODOS los dispositivos)
3. Verificar que NO hay boton "Nuevo Dispositivo"
4. Verificar que NO hay acciones de editar/eliminar
5. **Esperado:** Solo lectura

---

## Prueba 11: Ubicaciones - Solo Lectura

1. Ir a "Ubicaciones"
2. Seleccionar un cliente
3. **Esperado:** Se muestra el arbol de ubicaciones
4. Verificar que NO hay boton "Nueva Ubicacion"
5. Verificar que NO hay acciones de agregar hijo o eliminar
6. **Esperado:** Solo lectura

---

## Prueba 12: Fabricantes - Solo Lectura

1. Ir a "Fabricantes"
2. **Esperado:** Lista de fabricantes visible
3. Verificar que NO hay boton "Nuevo Fabricante"
4. Verificar que NO hay acciones de editar/eliminar
5. Verificar que NO hay opcion de agregar campos
6. **Esperado:** Solo lectura

---

## Prueba 13: Verificar Acceso Global (sin aislamiento)

1. Ir a Clientes
2. **Esperado:** Ve TODOS los clientes (no solo los de un proveedor especifico)
3. Ir a Dispositivos
4. **Esperado:** Ve TODOS los dispositivos (no solo los de un cliente especifico)
5. Ir a Usuarios
6. **Esperado:** Ve TODOS los usuarios del sistema

---

## Prueba 14: Tema Oscuro y Cerrar Sesion

1. Cambiar tema
2. Cerrar sesion
3. **Esperado:** Funciona correctamente
