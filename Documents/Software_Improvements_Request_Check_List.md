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
| 2607160003 | 2026-07-16 | 🔵 Pendiente | Backend / Auth + Frontend | **Dashboard muestra datos globales a todos los usuarios** — Al ingressar con usuario tipo Administrador del Proveedor, el Dashboard muestra la misma información que el Administrador del Sistema. Debería filtrar datos por el Proveedor/Cliente asociado al usuario. Un usuario nuevo no debería ver información. | `Backends/WebApi/HorusEye.Api/Controllers/DashboardController.cs`, `Frontends/ReactTS/src/pages/Dashboard.tsx` | — | **Problema grave de aislamiento de datos.** El endpoint `/api/dashboard/kpis` retorna datos globales sin filtrar por `proveedorId` o `clienteId` del usuario autenticado. |
| 2607160004 | 2026-07-16 | 🔵 Pendiente | Frontend / UX | **Formulario de creación de usuario debe solicitar Proveedor/Cliente obligatorio** — Al seleccionar un rol de tipo Proveedor (Administrador/Asistente del Proveedor) se debe exigir seleccionar un Proveedor. Al seleccionar un rol de tipo Cliente se debe exigir seleccionar un Proveedor y un Cliente. Actualmente el formulario muestra dropdowns pero no valida que se seleccionen antes de guardar. | `Frontends/ReactTS/src/pages/Usuarios.tsx` | — | Los dropdowns existen pero no hay validación client-side para impedir el envío si no se seleccionan. |

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
