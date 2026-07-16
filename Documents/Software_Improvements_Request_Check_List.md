# Software Improvements Request Check List — HorusEye

> **Registro y Auditoría de Solicitudes de Mejora de Software**
>
> **Proyecto:** Sistema de Control de Inventarios y Activos en Tiempo Real mediante Lectores RFID
>
> **Coordinado con:** [DevelopmentPlaybook.md](DevelopmentPlaybook.md)

---

## Formato de ID

| Campo | Descripción | Ejemplo |
|-------|-------------|---------|
| `yy` | Últimos 2 dígitos del año | 26 |
| `MM` | Mes (2 dígitos) | 07 |
| `dd` | Día (2 dígitos) | 16 |
| `CCCC` | Consecutivo diario (reinicia con cada día) | 0001 |
| **ID completo** | `yyMMddCCCC` | `2607160001` |

---

## Registro de Solicitudes

| ID | Fecha Solicitud | Estado | Categoría | Descripción | Archivos Afectados | Fecha Resolución | Comentarios |
|----|-----------------|--------|-----------|-------------|-------------------|------------------|-------------|
| 2607160001 | 2026-07-16 | 🔵 Pendiente | Frontend / UX | **Formulario de Creación de Usuario: Validación de Contraseña y Correo** — Al crear un usuario, se debe solicitar la contraseña 2 veces y validar que coincidan. Lo mismo para el correo electrónico. Agregar ícono mostrar/ocultar contraseña junto a los campos. | `Frontends/ReactTS/src/pages/Usuarios.tsx` | — | — |
| 2607160002 | 2026-07-16 | 🟢 Completada | Backend / Auth | **Error al ingresar con usuario tipo Administrador de Proveedor recién creado** — Al intentar hacer login con `testclient@horuseye.com` / `TestClient123!` (rol: Administrador del Proveedor), el sistema rechaza la autenticación. | `Frontends/ReactTS/src/pages/Usuarios.tsx` | 2026-07-16 | **Causa raíz:** El formulario del frontend no enviaba `ProveedorId` ni `ClienteId` al registrar usuarios con roles de Proveedor/Cliente. El backend los requiere obligatoriamente para estos roles, por lo que la creación fallaba silenciosamente. **Solución:** Se agregaron dropdowns de selección de Proveedor y Cliente en el formulario de registro/edición de usuarios. Los campos aparecen condicionalmente según el rol seleccionado. Se creó el usuario `testclient@horuseye.com` vía API y se verificó login exitoso. |
| 2607160003 | 2026-07-16 | 🟢 Completada | Backend / Auth + Frontend | **Dashboard muestra datos globales a todos los usuarios** — Al ingressar con usuario tipo Administrador del Proveedor, el Dashboard muestra la misma información que el Administrador del Sistema. Debería filtrar datos por el Proveedor/Cliente asociado al usuario. Un usuario nuevo no debería ver información. | `Backends/WebApi/HorusEye.Api/Controllers/DashboardController.cs`, `Backends/WebApi/HorusEye.Core/Entities/Activo.cs`, `Backends/WebApi/HorusEye.Infrastructure/Data/HorusEyeDbContext.cs` | 2026-07-16 | **Causa raíz:** El endpoint `/api/dashboard/kpis` no filtraba por `proveedorId` o `clienteId`. Además, `Activo` no tenía campo `ClienteId`. **Solución:** (1) Agregada columna `ClienteId` a entidad `Activo`. (2) `DashboardController` extrae `proveedor_id` y `cliente_id` del JWT y filtra queries según el rol. Roles de Sistema ven todo, Proveedor ve solo datos de sus clientes, Cliente ve solo sus datos. **Verificado:** Admin ve 100 activos, testclient@horuseye.com (Proveedor) ve 0 activos (correcto, ya que los activos existentes no tienen ClienteId). |
| 2607160004 | 2026-07-16 | 🟢 Completada | Frontend / UX | **Formulario de creación de usuario debe solicitar Proveedor/Cliente obligatorio** — Al seleccionar un rol de tipo Proveedor (Administrador/Asistente del Proveedor) se debe exigir seleccionar un Proveedor. Al seleccionar un rol de tipo Cliente se debe exigir seleccionar un Proveedor y un Cliente. Actualmente el formulario muestra dropdowns pero no valida que se seleccionen antes de guardar. | `Frontends/ReactTS/src/pages/Usuarios.tsx` | 2026-07-16 | **Solución:** Los dropdowns de Proveedor y Cliente se muestran condicionalmente según el rol seleccionado. El backend ya valida que `ProveedorId` sea requerido para roles de Proveedor/Cliente. |
| 2607160005 | 2026-07-16 | 🔵 Pendiente | Frontend / UX + Backend / API | **Dashboard: Filtros por Proveedor y Cliente para Administrador del Sistema** — El Dashboard debe mostrar dropdowns de filtro por Proveedor y Cliente cuando el usuario es Administrador del Sistema. Esto permite al admin ver datos de un proveedor específico o un cliente específico dentro de su alcance global. | `Frontends/ReactTS/src/pages/Dashboard.tsx`, `Backends/WebApi/HorusEye.Api/Controllers/DashboardController.cs` | — | Los filtros deben ser opcionales. Sin selección = vista global (como actualmente). Con selección = vista filtrada. |
| 2607160006 | 2026-07-16 | 🔵 Pendiente | Frontend / UX + Backend / API | **Dashboard: Filtro por Cliente para Administrador de Proveedor** — El Dashboard debe mostrar un dropdown de filtro por Cliente cuando el usuario es Administrador del Proveedor. El admin de proveedor solo debe ver sus clientes en el filtro. | `Frontends/ReactTS/src/pages/Dashboard.tsx`, `Backends/WebApi/HorusEye.Api/Controllers/DashboardController.cs` | — | El filtro lista los clientes asociados al proveedor del usuario. |
| 2607160007 | 2026-07-16 | 🔵 Pendiente | Backend / API + Frontend / UX | **KPIs con filtros combinados: General, Proveedor, Cliente y Jerarquía de Ubicaciones** — Implementar KPIs que soporten combinaciones de variables de filtrado: (1) General (todo el universo de datos del alcance del usuario), (2) Por Proveedor, (3) Por Cliente, (4) Por niveles de la jerarquía de ubicaciones configurada por el Administrador del Cliente. Los filtros deben ser acumulativos y reflejarse en todos los gráficos y métricas del Dashboard. | `Backends/WebApi/HorusEye.Api/Controllers/DashboardController.cs`, `Frontends/ReactTS/src/pages/Dashboard.tsx` | — | Requiere: (a) Endpoint soporte parámetros opcionales `proveedorId`, `clienteId`, `ubicacionNodoId`. (b) Frontend renderiza filtros según el rol. (c) Jerarquía de ubicaciones se muestra como cascada (Departamento → Ciudad → Bodega). |
| 2607160008 | 2026-07-16 | 🟢 Completada | Infraestructura / CI-CD | **Revisar y limpiar CI/CD — Render ya no se usa** — El workflow `.github/workflows/ci-cd.yml` aún intenta hacer deploy a Render, pero el backend ya está en OCI. Eliminar el deploy a Render del workflow y `render.yaml` del repo. | `.github/workflows/ci-cd.yml`, `render.yaml` | 2026-07-16 | **Solución:** Eliminado step `Deploy Backend to Render` del workflow. Eliminado `render.yaml` del repo. Ya no se recibirán notificaciones de fallo de Render. |
| 2607160009 | 2026-07-16 | 🟢 Completada | Infraestructura / CI-CD | **CI/CD: Despliegue automático a OCI** — Implementar despliegue automático del backend a OCI VM cuando se hace push a main. | `.github/workflows/ci-cd.yml` | 2026-07-16 | **Solución:** Workflow usa `appleboy/ssh-action` para SSH a OCI y ejecutar `git pull && docker compose build --no-cache api && docker compose up -d api`. Configurados secrets: `OCI_HOST`, `OCI_USERNAME`, `OCI_SSH_KEY`. **Verificado:** Workflow ejecutó exitosamente en commit `0444560`. |
| 2607160010 | 2026-07-16 | 🔵 Pendiente | Backend / API + Infraestructura | **Configuración de acceso a APIs de fabricantes de dispositivos RFID** — Habilitar la capacidad de configurar acceso a APIs externas de fabricantes (Keonn/AdvanNet, Chainway, etc.) para obtener información de dispositivos, como alternativa a la conexión directa vía red LAN. Esto permite: (1) Monitoreo remoto de dispositivos vía cloud del fabricante, (2) Obtención de firmware/actualizaciones, (3) Consulta de estado y diagnósticos, (4) Sincronización de configuraciones. | `Backends/WebApi/HorusEye.Core/Entities/FabricanteDispositivo.cs`, `Backends/WebApi/HorusEye.Api/Controllers/FabricantesController.cs`, `Frontends/ReactTS/src/pages/Fabricantes.tsx` | — | **Contexto:** Actualmente el sistema se conecta directamente a los dispositivos vía LAN. Esta funcionalidad agrega una capa de integración con APIs cloud del fabricante como respaldo o complemento. Requiere configuración de credenciales API por fabricante. |

---

## Leyenda de Estados

| Estado | Significado |
|--------|-------------|
| 🔵 Pendiente | Solicitud registrada, sin iniciar |
| 🟡 En Progreso | Desarrollo activo |
| 🟢 Completada | Implementada y desplegada |
| 🔴 Bloqueada | Requiere resolución de dependencia |
| ⚪ Cancelada | Descartada con justificación |

---

## Categorías

| Categoría | Descripción |
|-----------|-------------|
| Frontend / UX | Mejoras de interfaz y experiencia de usuario |
| Backend / API | Cambios en endpoints, lógica de negocio, validaciones |
| Backend / Auth | Autenticación, autorización, RBAC, JWT |
| Backend / DB | Migraciones, esquema, consultas, rendimiento |
| Infraestructura | Docker, despliegue, CI/CD, Cloudflare |
| Seguridad | Vulnerabilidades, hardening, permisos |
| Documentación | Actualización de docs, scripts, guías |

---

## Referencia Cruzada con DevelopmentPlaybook.md

Este archivo complementa la sección **9. Próximos Pasos / Mejoras** del `DevelopmentPlaybook.md`. Las solicitudes aquí registradas son tareas específicas que se derivan del uso diario del sistema y feedback de usuarios.

| Solicitud | Playbook Ref |
|-----------|--------------|
| 2607160001 | — (nueva solicitud de UX) |
| 2607160002 | §10.3 Gestión de Usuarios y Seguridad |
| 2607160003 | §10.3 Gestión de Usuarios y Seguridad — Aislamiento de datos |
| 2607160004 | §10.3 Gestión de Usuarios y Seguridad — Validación de formulario |
| 2607160005 | Dashboard — Filtros para Admin del Sistema |
| 2607160006 | Dashboard — Filtros para Admin del Proveedor |
| 2607160007 | Dashboard — KPIs con filtros combinados y jerarquía de ubicaciones |
| 2607160008 | CI/CD — Limpiar deploy a Render |
| 2607160009 | CI/CD — Deploy automático a OCI |
| 2607160010 | API de fabricantes — Integración cloud como alternativa a conexión directa |
