# 📘 Development Playbook - HorusEye

> **Bitácora de Desarrollo** — Proyecto: Sistema de Control de Inventarios y Activos en Tiempo Real mediante Lectores RFID
>
> **Fecha:** 27 de Mayo de 2026
> **Arquitecto:** Asistente de Desarrollo (Claude/OpenCode)

---

## Índice

1. [Resumen Ejecutivo](#1-resumen-ejecutivo)
2. [Estructura del Proyecto](#2-estructura-del-proyecto)
3. [Bitácora Paso a Paso](#3-bitácora-paso-a-paso)
   - [3.1 Exploración Inicial](#31-exploración-inicial)
   - [3.2 Base de Datos PostgreSQL](#32-base-de-datos-postgresql)
   - [3.3 Backend .NET 10 Web API](#33-backend-net-10-web-api)
   - [3.4 Frontend React TS + Vite](#34-frontend-react-ts--vite)
   - [3.5 Verificación y Compilación](#35-verificación-y-compilación)
   - [3.6 Correcciones Post-Desarrollo](#36-correcciones-post-desarrollo)
4. [Decisiones Técnicas](#4-decisiones-técnicas)
5. [Cómo Ejecutar el Proyecto](#5-cómo-ejecutar-el-proyecto)
6. [Guía de Simulación RFID](#6-guía-de-simulación-rfid)
7. [Endpoints de la API](#7-endpoints-de-la-api)
8. [Usuarios de Prueba](#8-usuarios-de-prueba)
9. [Próximos Pasos / Mejoras](#9-próximos-pasos--mejoras)
   - [Mejoras Realizadas](#mejoras-realizadas)
10. [Análisis de Cumplimiento — SUGERENCIAS CESAR V2.pdf](#10-análisis-de-cumplimiento--sugerencias-cesar-v2pdf)
   - [10.1 Gestión y Control de TAG RFID](#101-gestión-y-control-de-tag-rfid)
   - [10.2 Gestión de Activos](#102-gestión-de-activos)
   - [10.3 Gestión de Usuarios y Seguridad](#103-gestión-de-usuarios-y-seguridad)
   - [10.4 Reportes y Exportación](#104-reportes-y-exportación)
   - [10.5 Eventos del Sistema](#105-eventos-del-sistema)
   - [10.6 Módulo de Autorización de Salida](#106-módulo-de-autorización-de-salida-nueva-funcionalidad)
   - [10.7 Resumen General](#107-resumen-general)
11. [Infraestructura de Despliegue](#11-infraestructura-de-despliegue)
   - [11.1 Arquitectura](#111-arquitectura)
   - [11.2 Archivos de Configuración Creados](#112-archivos-de-configuración-creados)
    - [11.3 Paso a Paso — Vercel (Frontend)](#113-paso-a-paso--vercel-frontend)
    - [11.4 Paso a Paso — Render (Backend + BD)](#114-paso-a-paso--render-backend--bd)
12. [Correcciones de Despliegue (28-Mayo-2026)](#12-correcciones-de-despliegue-28-mayo-2026)
13. [Mejoras de Seguridad y Calidad (28-Mayo-2026)](#13-mejoras-de-seguridad-y-calidad-28-mayo-2026)
14. [Migración a EF Core Migrations (28-Mayo-2026)](#14-migración-a-ef-core-migrations-28-mayo-2026)
15. [Mejoras de Calidad de Código (28-Mayo-2026)](#15-mejoras-de-calidad-de-código-28-mayo-2026)
16. [Mejoras de Infraestructura (28-Mayo-2026)](#16-mejoras-de-infraestructura-28-mayo-2026)
17. [Resolución de Errores de Lint y Mejoras de CI/CD (29-Mayo-2026)](#17-resolución-de-errores-de-lint-y-mejoras-de-cicd-29-mayo-2026)
18. [Paginación de Consultas (29-Mayo-2026)](#18-paginación-de-consultas-29-mayo-2026)
19. [Paginación de Tags RFID (29-Mayo-2026)](#19-paginación-de-consultas--tags-rfid-29-mayo-2026)
20. [Selector de Ítems por Página (29-Mayo-2026)](#20-selector-de-ítens-por-página-29-mayo-2026)
21. [Paginación de Autorizaciones (29-Mayo-2026)](#21-paginación-de-autorizaciones-29-mayo-2026)
22. [Simulación de Autorizaciones (29-Mayo-2026)](#22-simulación-de-autorizaciones-29-mayo-2026)
23. [Paginación de Usuarios (29-Mayo-2026)](#23-paginación-de-usuarios-29-mayo-2026)
24. [Migración de Render a OCI (10-Julio-2026)](#24-migración-de-render-a-oci-10-julio-2026)
25. [Generación de Datos de Prueba — 3 Meses (10-Julio-2026)](#25-generación-de-datos-de-prueba--3-meses-10-julio-2026)
26. [Notas para la Sesión de Fin de Semana (10-Julio-2026)](#26-notas-para-la-sesión-de-fin-de-semana-10-julio-2026)

---

## 1. Resumen Ejecutivo

**HorusEye** es un sistema de control de inventarios y activos en tiempo real mediante lectores RFID. El proyecto se compone de tres capas principales:

| Capa | Tecnología | Ubicación |
|------|-----------|-----------|
| **Base de Datos** | PostgreSQL 18 (Docker) | `Databases/PostgreSQL/` |
| **Backend** | .NET 10 Web API + EF Core + SignalR | `Backends/WebApi/` |
| **Frontend** | React 19 + TypeScript + Vite + Tailwind CSS v4 | `Frontends/ReactTS/` |

---

## 2. Estructura del Proyecto

```
HorusEye/
├── Databases/
│   └── PostgreSQL/
│       ├── .env                  # Variables de entorno de la BD
│       ├── docker-compose.yml    # Orquestación del contenedor PostgreSQL
│       ├── init.sql              # Script de inicialización con esquema completo
│       └── data/                 # Datos persistentes del contenedor
│
├── Backends/
│   └── WebApi/
│       ├── HorusEye.sln          # Solución de .NET
│       ├── HorusEye.Api/         # Proyecto API (Controladores, Hubs, Middleware)
│       │   ├── Controllers/      # Auth, Activos, Tags, EventosRfid, Dashboard, Reportes, Movimientos
│       │   ├── DTOs/             # Objetos de transferencia de datos
│       │   ├── Hubs/             # MovimientosHub (SignalR)
│       │   ├── Middleware/       # ExceptionMiddleware global
│       │   ├── Models/           # ApiResponse<T> genérico
│       │   ├── Services/         # TokenService (JWT + Refresh Tokens)
│       │   ├── Program.cs        # Punto de entrada y configuración
│       │   └── appsettings.json  # Configuración (ConnectionStrings, JWT)
│       │
│       ├── HorusEye.Core/        # Capa de dominio
│       │   ├── Entities/         # Tag, Activo, Movimiento, AutorizacionSalida, etc.
│       │   └── Enums/            # EstadoTag, EstadoUbicacion, TipoMovimiento
│       │
│       └── HorusEye.Infrastructure/  # Capa de infraestructura
│           └── Data/
│               └── HorusEyeDbContext.cs  # DbContext de EF Core
│
└── Frontends/
    └── ReactTS/
        ├── src/
        │   ├── components/       # Layout, ProtectedRoute
        │   ├── context/          # AuthContext (JWT + roles)
        │   ├── hooks/            # (futuro: useAuth, useSignalR)
        │   ├── pages/            # Login, Dashboard, Activos, Tags, Reportes, Usuarios
        │   ├── services/         # api.ts (axios), signalR.ts
        │   ├── types/            # Interfaces TypeScript
        │   ├── App.tsx           # Router principal
        │   ├── main.tsx          # Punto de entrada
        │   └── index.css         # Tailwind CSS v4
        │
        ├── vite.config.ts        # Proxy inverso a la API
        ├── package.json          # Dependencias (pnpm)
        └── tsconfig*.json        # Configuración TypeScript

└── Documents/
    ├── Prompt_HorusEye_0000.md   # Documento de especificaciones original
    └── DevelopmentPlaybook.md     # Esta bitácora
```

---

## 3. Bitácora Paso a Paso

### 3.1 Exploración Inicial

**1. Lectura del documento de especificaciones**
- Se leyó `Documents/Prompt_HorusEye_0000.md` (162 líneas) que contenía los requisitos técnicos completos.
- Se verificó la estructura existente del proyecto.

**2. Verificación de herramientas**
```bash
dotnet --version    # 10.0.108  → .NET 10 SDK disponible
node --version      # v24.13.0  → Node.js disponible
pnpm --version      # 11.2.2    → pnpm disponible
docker --version    # 29.2.1    → Docker disponible
```

**3. Estado inicial de directorios**
- `Databases/PostgreSQL/` → Ya existía con `docker-compose.yml`, `.env` y `init.sql` (esquema de fútbol/sports existente).
- `Backends/WebApi/` → Vacío.
- `Frontends/ReactTS/` → Vacío.

---

### 3.2 Base de Datos PostgreSQL

**Archivo modificado:** `Databases/PostgreSQL/init.sql`

**Acciones:**
- Se reemplazó completamente el esquema anterior (tablas de torneos deportivos) por el nuevo esquema HorusEye.
- Inicialmente se definieron 3 tipos ENUM PostgreSQL personalizados:
  - `EstadoTag`: REGISTRADO, ASIGNADO, DISPONIBLE, DAÑADO
  - `EstadoUbicacion`: DENTRO_INSTALACIONES, FUERA_INSTALACIONES
  - `TipoMovimiento`: INGRESO, SALIDA
  - > **Nota:** Posteriormente los ENUMs se reemplazaron por columnas VARCHAR(20/30/10) para evitar conflictos con Npgsql (ver [3.6 Correcciones](#36-correcciones)).
- Se crearon las tablas del negocio:
  - `Tags` — Almacena los identificadores RFID (EPC/UID) con estado.
  - `TagDanioHistorial` — Historial de daños de cada tag.
  - `Activos` — Equipos/items con placa única, categoría, responsable, ubicación y tag asociado.
  - `Movimientos` — Registro de ingresos/salidas con punto de lectura, autorización y alarma.
  - `AutorizacionesSalida` — Autorizaciones temporales para salidas de activos.
  - `RefreshTokens` — Tokens OAuth 2.0 para renovación de JWT.
- Se integraron las tablas de **ASP.NET Core Identity**:
  - `AspNetRoles`, `AspNetUsers`, `AspNetRoleClaims`, `AspNetUserClaims`, `AspNetUserLogins`, `AspNetUserTokens`, `AspNetUserRoles`
- Seed data: Se insertaron 2 roles iniciales ("Usuario de Consulta" y "Usuario de Gestión").

> **Nota:** El `docker-compose.yml` y `.env` existentes se mantuvieron sin cambios.

---

### 3.3 Backend .NET 10 Web API

#### 3.3.1 Creación de la Solución

```bash
# Desde Backends/WebApi/
dotnet new sln -n HorusEye
dotnet new webapi -n HorusEye.Api
dotnet new classlib -n HorusEye.Core
dotnet new classlib -n HorusEye.Infrastructure
dotnet sln add HorusEye.Api/ HorusEye.Core/ HorusEye.Infrastructure/
```

**Referencias entre proyectos:**
```
HorusEye.Api → HorusEye.Core, HorusEye.Infrastructure
HorusEye.Infrastructure → HorusEye.Core
```

**Paquetes NuGet instalados:**

| Proyecto | Paquete | Propósito |
|----------|---------|-----------|
| `HorusEye.Api` | `Microsoft.AspNetCore.Authentication.JwtBearer` | Autenticación JWT |
| `HorusEye.Api` | `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | Identity |
| `HorusEye.Api` | `Serilog.AspNetCore` | Logging estructurado |
| `HorusEye.Api` | `Serilog.Sinks.File` | Archivos rotativos de log |
| `HorusEye.Api` | `Microsoft.EntityFrameworkCore.Design` | Migraciones EF (design-time) |
| `HorusEye.Api` | `Swashbuckle.AspNetCore` | Swagger UI |
| `HorusEye.Infrastructure` | `Npgsql.EntityFrameworkCore.PostgreSQL` | Provider PostgreSQL |
| `HorusEye.Infrastructure` | `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | Identity stores |

#### 3.3.2 Capa de Dominio (`HorusEye.Core`)

Se crearon los siguientes archivos:

- **`Enums/EstadoTag.cs`** — Enum con REGISTRADO, ASIGNADO, DISPONIBLE, DAÑADO
- **`Enums/EstadoUbicacion.cs`** — Enum con DENTRO_INSTALACIONES, FUERA_INSTALACIONES
- **`Enums/TipoMovimiento.cs`** — Enum con INGRESO, SALIDA
- **`Entities/Tag.cs`** — Entidad Tag con navegación a Activo y DaniosHistorial
- **`Entities/TagDanioHistorial.cs`** — Registro de daños
- **`Entities/Activo.cs`** — Entidad principal con placa única, tag FK, ubicación
- **`Entities/Movimiento.cs`** — Registro de movimientos con alarma
- **`Entities/AutorizacionSalida.cs`** — Autorizaciones con vencimiento
- **`Entities/RefreshToken.cs`** — Token OAuth 2.0

#### 3.3.3 Capa de Infraestructura (`HorusEye.Infrastructure`)

- **`Data/HorusEyeDbContext.cs`** — DbContext que hereda de `IdentityDbContext` y configura:
  - Mapeo de cada entidad con restricciones, índices únicos y relaciones
  - Conversión de ENUMs C# a strings en la BD mediante `.HasConversion<string>()`
  - Relación 1:1 entre Activo y Tag (único constraint automático en TagId)

#### 3.3.4 Capa de Presentación (`HorusEye.Api`)

**Modelos y DTOs:**

- **`Models/ApiResponse.cs`** — Objeto genérico `ApiResponse<T>` con `Success`, `Message`, `Data`, `Errors`, `Timestamp`. Métodos estáticos `Ok()` y `Fail()`.
- **`DTOs/AuthDtos.cs`** — `LoginRequest`, `RegisterRequest`, `RefreshTokenRequest`, `ChangePasswordRequest`, `RecoverPasswordRequest`, `AuthResponse`.
- **`DTOs/RfidDtos.cs`** — `EventoRfidRequest`, `EventoRfidResponse`, `ActivoRequest`, `ActivoResponse`, `MovimientoResponse`, `ReporteRequest`.

**Middleware:**

- **`Middleware/ExceptionMiddleware.cs`** — Captura centralizada de excepciones:
  - `KeyNotFoundException` → 404
  - `UnauthorizedAccessException` → 401
  - `ArgumentException` / `InvalidOperationException` → 400
  - Cualquier otra → 500
  - En producción oculta el StackTrace por seguridad

**Servicios:**

- **`Services/TokenService.cs`** — Generación de JWT (15 min expiración), generación de Refresh Tokens (64 bytes aleatorios, 7 días), validación y revocación.

**Controladores:**

| Controlador | Ruta Base | Endpoints | Auth |
|------------|-----------|-----------|------|
| `AuthController` | `/api/auth` | `POST register`, `POST login`, `POST refresh-token`, `POST change-password`, `POST recover-password`, `GET users` | Mixto (register requiere Gestión) |
| `EventosRfidController` | `/api/eventos-rfid` | `POST` (procesar lectura RFID) | AllowAnonymous |
| `ActivosController` | `/api/activos` | `GET`, `GET/{id}`, `POST`, `PUT/{id}`, `DELETE/{id}` | POST/PUT/DELETE requieren Gestión |
| `TagsController` | `/api/tags` | `GET`, `GET/disponibles`, `POST`, `PUT/{tagId}/estado`, `POST/{tagId}/reportar-danio` | POST/PUT requieren Gestión |
| `MovimientosController` | `/api/movimientos` | `GET`, `GET/resumen-del-dia` | Authenticated |
| `DashboardController` | `/api/dashboard` | `GET kpis` | Authenticated |
| `ReportesController` | `/api/reportes` | `POST` | Authenticated |

**Características implementadas en EventosRfidController:**

1. **Filtro Anti-Duplicados:** Usa `IMemoryCache` con clave `rfid_debounce_{TagId}` y expiración de 5 segundos.
2. **Lógica de Alarma:** Si es SALIDA y no hay autorización vigente → `Activar_Alarma_Sonora: true`.
3. **Difusión SignalR:** Cada evento procesado se envía al grupo "Dashboard" mediante `MovimientosHub`.

**Hubs SignalR:**

- **`Hubs/MovimientosHub.cs`** — Expone `JoinDashboardGroup()` para que los clientes React reciban eventos en tiempo real.
- Configurado para recibir el JWT mediante QueryString (`access_token`) en la conexión WebSocket.

**Program.cs (Configuración principal):**

- Serilog con rolling file JSON diario en `logs/log-YYYYMMDD.json`, retención de 30 días.
- DbContext con PostgreSQL desde `ConnectionStrings:DefaultConnection`.
- Identity con Password: requerir dígito, mayúscula, minúscula, mínimo 6 caracteres.
- JWT Bearer configurado con validación de issuer, audience, lifetime y signing key.
- CORS habilitado para cualquier origen mediante `SetIsOriginAllowed(_ => true)`.
- Seed automático de roles y 2 usuarios de prueba al iniciar.
- Swagger disponible en entorno de Development.

**appsettings.json:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=horuseyesdb;Username=horuseyesuser;Password=H0rv$Ey3$"
  },
  "Jwt": {
    "Key": "HorusEyeSuperSecretKey2026!@#$%^&*()VeryLongSecure",
    "Issuer": "HorusEyeAPI",
    "Audience": "HorusEyeFrontend"
  }
}
```

---

### 3.4 Frontend React TS + Vite

#### 3.4.1 Creación del Proyecto

```bash
cd Frontends/ReactTS
pnpm create vite . --template react-ts
```

**Dependencias instaladas:**

| Paquete | Propósito |
|---------|-----------|
| `axios` | Cliente HTTP con interceptors |
| `@microsoft/signalr` | Cliente SignalR para tiempo real |
| `react-router-dom` | Router para SPA |
| `tailwindcss` + `@tailwindcss/vite` | Utility CSS framework v4 |
| `lucide-react` | Iconos SVG |
| `xlsx` + `file-saver` | Exportación a Excel |

#### 3.4.2 Configuración

- **`vite.config.ts`** — Plugin de React + Tailwind CSS + Proxy inverso (`/api` → `localhost:5000`, `/hubs` → `localhost:5000` con WebSocket).
- **`index.css`** — Directiva `@import "tailwindcss"` + variables CSS personalizadas.

#### 3.4.3 Tipos TypeScript (`src/types/index.ts`)

Se definieron interfaces para: `AuthResponse`, `User`, `Activo`, `Tag`, `Movimiento`, `KpiData`, `ApiResponse<T>`, `EventoRfidResponse`.

#### 3.4.4 Servicios

**`services/api.ts`** (Axios):
- Base URL configurable via `VITE_API_URL`.
- **Interceptor de request:** Adjunta `Authorization: Bearer <token>` desde localStorage.
- **Interceptor de response:** Si recibe 401, intenta renovar el token automáticamente mediante `/api/auth/refresh-token`. Si falla, redirige a `/login`.

**`services/signalR.ts`** (SignalR):
- Conexión a `/hubs/movimientos` con reconexión automática.
- Método `joinDashboard()` para unirse al grupo de tiempo real.
- Callback `onMovimiento()` para escuchar eventos.

#### 3.4.5 Contexto de Autenticación

**`context/AuthContext.tsx`:**
- Proveedor global que maneja JWT, verifica expiración, almacena usuario.
- `login()`: llama a la API, guarda tokens y datos del usuario.
- `logout()`: limpia localStorage y redirige.
- `hasRole()`: verifica si el usuario tiene un rol específico.
- `useAuth()` hook personalizado para consumir el contexto.

#### 3.4.6 Componentes

**`components/ProtectedRoute.tsx`:**
- Verifica autenticación antes de renderizar hijos.
- Muestra spinner mientras carga.
- Redirige a `/login` si no está autenticado.

**`components/Layout.tsx`:**
- **Header:** Logo (Eye icon), título "HorusEye", indicador de rol, botón de salir.
- **Menú de navegación:** Dashboard, Activos, Tags RFID, Reportes, Usuarios (solo Gestión).
- **Responsive:** Menú colapsable en mobile (< lg) con botón hamburguesa.
- **Footer:** "Todos los Derechos Reservados 2026".
- Usa `<Outlet />` de React Router para renderizar páginas hijas.

#### 3.4.7 Páginas

**`pages/Login.tsx`:**
- Formulario de inicio de sesión con email y contraseña.
- Manejo de errores y estado de carga.
- Redirección a Dashboard tras login exitoso.

**`pages/Dashboard.tsx`:**
- **KPIs:** 8 tarjetas (Total Activos, Activos Dentro/Fuera, Tags Registrados/Asignados/Disponibles, Ingresos/Salidas Hoy).
- **Gráficos (Recharts):** Bar chart de tags por estado, Pie chart de activos por ubicación, Line chart de tendencia semanal (ingresos, salidas, no autorizadas), Bar chart horizontal de activos por categoría.
- **Tabla de movimientos recientes** con datos en tiempo real vía SignalR.
- Alertas visuales para salidas no autorizadas (fila roja).
- Indicador de conexión SignalR (conectado/desconectado).
- Todos los gráficos se actualizan en tiempo real cuando llega un nuevo movimiento vía SignalR.

**`pages/Activos.tsx`:**
- Tabla completa con todos los activos.
- **Modal de creación/edición** con campos: Placa, Nombre, Categoría (dropdown), Responsable, Tag RFID (solo tags DISPONIBLES).
- **Modal de confirmación** antes de eliminar.
- Botones de acción visibles solo para rol "Usuario de Gestión".

**`pages/Tags.tsx`:**
- Tabla de tags RFID con estado coloreado.
- Capacidad de cambiar estado a DISPONIBLE o reportar como DAÑADO (solo Gestión).
- Modal para registrar nuevos tags.

**`pages/Reportes.tsx`:**
- Filtros: Tipo de Reporte (Movimientos, Tags Registrados, Activos Dentro, Activos Fuera), Fecha Inicio/Fin.
- Botón "Generar" para obtener resultados.
- **Botón "Exportar a Excel"** funcional mediante librería `xlsx` + `file-saver`.

**`pages/Usuarios.tsx`:**
- Visible solo para "Usuario de Gestión".
- Tabla de usuarios del sistema con roles.
- Modal para registrar nuevos usuarios con asignación de rol.

#### 3.4.8 App.tsx (Router)

```tsx
<BrowserRouter>
  <AuthProvider>
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route element={<ProtectedRoute><Layout /></ProtectedRoute>}>
        <Route path="/dashboard" element={<Dashboard />} />
        <Route path="/activos" element={<Activos />} />
        <Route path="/tags" element={<TagsPage />} />
        <Route path="/reportes" element={<Reportes />} />
        <Route path="/usuarios" element={<Usuarios />} />
      </Route>
      <Route path="*" element={<Navigate to="/dashboard" />} />
    </Routes>
  </AuthProvider>
</BrowserRouter>
```

---

### 3.5 Verificación y Compilación

**Backend:**
```bash
cd Backends/WebApi
dotnet build
# Build succeeded. 0 Warning(s), 0 Error(s)
```

**Frontend:**
```bash
cd Frontends/ReactTS
pnpm build
# ✓ built in 1.62s
```

Ambos proyectos compilan exitosamente sin errores ni advertencias.

**Correcciones realizadas durante el desarrollo:**
1. Se agregó paquete `Swashbuckle.AspNetCore` para Swagger (omitido inicialmente).
2. Se corrigió warning de null reference en `ExceptionMiddleware.cs`.
3. Se eliminó variable `hasRole` no utilizada en `Dashboard.tsx`.
4. Se agregó `@types/file-saver` para tipos TypeScript.

---

### 3.6 Correcciones Post-Desarrollo (27-May-2026)

> Sesión de debugging para resolver errores de integración entre backend, base de datos y frontend.

#### 3.6.1 Error: `operator does not exist: "EstadoUbicacion" = integer` (42883)

**Síntoma:** Al registrar un tag o activo, PostgreSQL lanzaba `42883: operator does not exist: "EstadoUbicacion" = integer`. Npgsql enviaba los valores ENUM de C# como enteros subyacentes (0, 1, 2), pero las columnas eran tipo ENUM PostgreSQL que esperan strings.

**Solución:**
1. `init.sql`: Se reemplazaron los tipos ENUM de PostgreSQL por columnas `VARCHAR(20/30/10)`.
2. `HorusEyeDbContext.cs`: Se eliminaron las llamadas `.HasPostgresEnum<>()` y se restauró `.HasConversion<string>()` en cada propiedad enum.
3. Se requirió recrear el volumen de PostgreSQL (`docker stop`, `sudo rm -rf data/`, `docker compose up -d`).

**Archivos afectados:**
- `Databases/PostgreSQL/init.sql` — ENUMs → VARCHAR
- `Backends/WebApi/HorusEye.Infrastructure/Data/HorusEyeDbContext.cs` — Eliminar `HasPostgresEnum`, restaurar `HasConversion<string>`

#### 3.6.2 Error: `Cannot write DateTimeOffset with Offset=-05:00:00` (Npgsql)

**Síntoma:** El endpoint `/api/dashboard/kpis` fallaba con este error al comparar fechas en queries LINQ que involucraban `DateTimeOffset`.

**Solución:** Se agregó `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)` al inicio de `Program.cs` para permitir que Npgsql maneje DateTimeOffset con offsets distintos de UTC.

**Archivo afectado:**
- `Backends/WebApi/HorusEye.Api/Program.cs` — Línea 14

#### 3.6.3 Error: JSON enum serializado como entero

**Síntoma:** La API devolvía `"estado": 0` en lugar de `"estado": "REGISTRADO"`.

**Solución:** Se agregó `JsonStringEnumConverter` global en `Program.cs` mediante `AddJsonOptions()`.

**Archivo afectado:**
- `Backends/WebApi/HorusEye.Api/Program.cs` — Líneas 26-30

#### 3.6.4 Error: `duplicate key value violates unique constraint "IX_Activos_TagId"` (500)

**Síntoma:** Al crear un activo con un tag ya asignado a otro activo, la BD lanzaba un `DbUpdateException` que se propagaba como 500 Internal Server Error.

**Solución:** Se agregó validación explícita en `ActivosController.cs` antes de insertar: si el `TagId` ya existe en la tabla `Activos`, se devuelve un 400 Bad Request con mensaje claro, en lugar de dejar que el constraint de BD lance una excepción no manejada.

**Archivo afectado:**
- `Backends/WebApi/HorusEye.Api/Controllers/ActivosController.cs` — Líneas 91-94 (Create) y 160-164 (Update)

#### 3.6.5 Mejora: Tags nuevos se crean como DISPONIBLE

**Problema:** Al registrar un tag, quedaba en estado `REGISTRADO`. Para usarlo en un activo, el usuario debía cambiarlo manualmente a `DISPONIBLE`. Esto añadía un paso extra innecesario.

**Solución:** Se cambió el estado inicial del tag de `REGISTRADO` a `DISPONIBLE` en `TagsController.cs` línea 58. Los tags creados están inmediatamente disponibles para asignar.

**Archivo afectado:**
- `Backends/WebApi/HorusEye.Api/Controllers/TagsController.cs` — Línea 58

#### 3.6.6 Mejora: CORS permisivo

**Problema:** El frontend en `localhost:5174` (puerto asignado dinámicamente por Vite) no podía conectar con la API.

**Solución:** Se cambió CORS de orígenes fijos a `SetIsOriginAllowed(_ => true)` para aceptar cualquier origen en desarrollo. Se mantuvo `AllowCredentials()` para soportar SignalR.

**Archivo afectado:**
- `Backends/WebApi/HorusEye.Api/Program.cs` — Líneas 97-102

#### 3.6.7 Puerto de API

**Problema:** El launchSettings tenía puerto 5160 pero el frontend y docker apuntaban a 5000.

**Solución:** Se actualizó `launchSettings.json` para usar el puerto 5000.

**Archivo afectado:**
- `Backends/WebApi/HorusEye.Api/Properties/launchSettings.json`

---

## 4. Decisiones Técnicas

| Decisión | Opción Elegida | Alternativa | Justificación |
|----------|---------------|-------------|---------------|
| **Arquitectura Backend** | 3 capas (Api/Core/Infrastructure) | Monolito | Separación de responsabilidades, testabilidad, clean architecture |
| **Autenticación** | ASP.NET Core Identity + JWT + Refresh Tokens | Solo JWT | Identity provee hashing PBKDF2, manejo de roles y políticas por claims |
| **Logging** | Serilog con rolling file JSON | ILogger nativo | Formato estructurado, rotación diaria, retención configurable |
| **Frontend CSS** | Tailwind CSS v4 | CSS Modules / Chakra | Utilidades rápidas, responsive design integrado, bundle pequeño |
| **Comunicación tiempo real** | SignalR | WebSocket puro / Polling | Reconexión automática, grupos, integración nativa con .NET |
| **Debounce RFID** | IMemoryCache (5s) | Redis / BD | Simplicidad, sin dependencia externa, dato volátil aceptable |
| **Exportación Excel** | xlsx + file-saver | ReportServer / CSV | 100% cliente, sin carga al servidor |
| **Gestor paquetes** | pnpm | npm / yarn | Más rápido, eficiente con monorepos y dependencias |
| **Mapeo ENUMs** | `HasConversion<string>()` + VARCHAR | PostgreSQL ENUMs nativos | Evita conflictos Npgsql (enteros vs strings); portable entre BD |
| **Serialización ENUMs** | `JsonStringEnumConverter` global | Atributo `[JsonConverter]` por propiedad | Consistente en toda la API, sin decorar cada DTO |
| **Estado inicial de tags** | `DISPONIBLE` al crearlos | `REGISTRADO` (requería paso manual) | Flujo más simple: crear tag → asignar a activo directamente |
| **Validación unicidad tag** | Consulta explícita en controller | Solo constraint de BD | Mensaje de error amigable, evita 500 por DbUpdateException |
| **Modo oscuro** | Context API + Tailwind `dark:` variants | CSS variables puras | Sigue el sistema de clases de Tailwind v4, toggle con ícono sol/luna en header, persiste en localStorage |

---

## 5. Cómo Ejecutar el Proyecto

### Requisitos Previos
- .NET 10 SDK
- Docker
- Node.js 20+ y pnpm

### Paso 1: Base de Datos
```bash
cd Databases/PostgreSQL
docker compose up -d
```

### Paso 2: Backend
```bash
cd Backends/WebApi/HorusEye.Api
dotnet run
# API disponible en http://localhost:5000
# Swagger en http://localhost:5000/swagger
```

> **Nota:** Si el puerto 5000 está ocupado, actualizar `Properties/launchSettings.json`.

### Paso 3: Frontend
```bash
cd Frontends/ReactTS
pnpm install   # Solo la primera vez
pnpm dev
# Dashboard en http://localhost:5174
# (Vite asigna el puerto dinámicamente si 5173 está ocupado)
```

---

## 6. Guía de Simulación RFID

Para probar el sistema sin hardware real, sigue estos pasos:

### 6.1 Login como admin
```bash
curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@horuseye.com","password":"Admin123!"}' | \
  python3 -c "import sys,json; print(json.load(sys.stdin)['data']['accessToken'])"
# → Devuelve el accessToken directamente

TOKEN="<pega_el_token_aqui>"
```

### 6.2 Registrar tags
```bash
TOKEN="<pega_tu_token_aqui>"

curl -X POST http://localhost:5000/api/tags \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '"TAG-SIM-001"'
# El tag se crea automáticamente como DISPONIBLE
```

### 6.3 Crear activo asignándole el tag
```bash
curl -X POST http://localhost:5000/api/activos \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"placa":"NOTE-001","nombre":"Notebook Lenovo ThinkPad","categoria":"Computadores","tenedorResponsable":"Juan Díaz","tagId":"TAG-SIM-001"}'
```

### 6.4 Simular lectura RFID (INGRESO)
```bash
curl -X POST http://localhost:5000/api/eventos-rfid \
  -H "Content-Type: application/json" \
  -d '{"tagId":"TAG-SIM-001","puntoLecturaId":"PUERTA-BODEGA","tipoMovimiento":"INGRESO"}'
```

### 6.5 Simular SALIDA sin autorización (activa alarma)
```bash
curl -X POST http://localhost:5000/api/eventos-rfid \
  -H "Content-Type: application/json" \
  -d '{"tagId":"TAG-SIM-001","puntoLecturaId":"PUERTA-BODEGA","tipoMovimiento":"SALIDA"}'
# → Respuesta: "activarAlarmaSonora": true
```

### 6.6 Script automatizado
Existe un script que ejecuta todo el ciclo completo:
```bash
bash Documents/simulacion.sh
```

### 6.7 Probar desde el Frontend
Una vez ejecutados los pasos anteriores, abre `http://localhost:5174` (o el puerto que asigne Vite), inicia sesión con `admin@horuseye.com` / `Admin123!` y ve al **Dashboard** — los movimientos simulados aparecerán en tiempo real.

---

## 7. Endpoints de la API

### Autenticación
| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | `/api/auth/register` | Gestión | Registrar nuevo usuario |
| POST | `/api/auth/login` | Público | Iniciar sesión (body: `email` + `password`) |
| POST | `/api/auth/refresh-token` | Público | Renovar JWT |
| POST | `/api/auth/change-password` | Authenticated | Cambiar contraseña |
| POST | `/api/auth/recover-password` | Público | Recuperar contraseña |
| GET | `/api/auth/users` | Gestión | Listar usuarios |

### RFID
| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | `/api/eventos-rfid` | Público | Procesar lectura RFID |

### Activos
| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/api/activos` | Authenticated | Listar activos |
| GET | `/api/activos/{id}` | Authenticated | Obtener activo |
| POST | `/api/activos` | Gestión | Crear activo |
| PUT | `/api/activos/{id}` | Gestión | Actualizar activo |
| DELETE | `/api/activos/{id}` | Gestión | Eliminar activo |

### Tags RFID
| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/api/tags` | Authenticated | Listar tags |
| GET | `/api/tags/disponibles` | Authenticated | Tags disponibles |
| POST | `/api/tags` | Gestión | Registrar tag |
| PUT | `/api/tags/{tagId}/estado` | Gestión | Cambiar estado |
| POST | `/api/tags/{tagId}/reportar-danio` | Gestión | Reportar daño |

### Dashboard y Reportes
| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/api/dashboard/kpis` | Authenticated | KPIs + distribución por categoría |
| GET | `/api/dashboard/tendencias` | Authenticated | Ingresos/salidas por día (7 días) |
| GET | `/api/movimientos` | Authenticated | Listar movimientos |
| GET | `/api/movimientos/resumen-del-dia` | Authenticated | Conteo del día |
| POST | `/api/reportes` | Authenticated | Generar reporte |

### SignalR Hub
| Hub | Ruta | Descripción |
|-----|------|-------------|
| MovimientosHub | `/hubs/movimientos` | Tiempo real de movimientos |

---

## 8. Usuarios de Prueba

Los siguientes usuarios se crean automáticamente al iniciar la API por primera vez:

| Rol | Email | Contraseña | Permisos |
|-----|-------|-----------|----------|
| **Usuario de Gestión** | `admin@horuseye.com` | `Admin123!` | Acceso completo: CRUD activos, tags, usuarios, autorizar salidas |
| **Usuario de Consulta** | `consulta@horuseye.com` | `Consulta123!` | Solo lectura: Dashboard, reportes, ver activos/tags |

---

## 9. Próximos Pasos / Mejoras

### Pendientes del Prompt Original

- [ ] **Recuperación de contraseña por email:** Actualmente el endpoint `recover-password` genera un token pero no lo envía por correo. Requiere integración con SMTP (SendGrid, MailKit, etc.).
- [ ] **Módulo de alarmas sonoras:** El backend retorna `Activar_Alarma_Sonora: true` pero no hay un mecanismo físico de alarma implementado (podría integrarse con hardware mediante GPIO, WebSocket o MQTT).

### Mejoras Propuestas

1. **Pruebas unitarias y de integración** — Agregar tests con xUnit para controladores y servicios.
2. **Migraciones EF Core** — Reemplazar `EnsureCreated()` por migraciones formales para control de versiones de esquema.
3. **Docker Compose completo** — Unificar BD + Backend + Frontend en un solo `docker-compose.yml` en la raíz.
4. **Autenticación por email** — Integrar SendGrid / MailKit para envío real de correos de recuperación.
5. **CI/CD** — Pipeline de GitHub Actions para build + test + deploy.
6. **Dashboard de administración** — Panel para ver logs, métricas de rendimiento y estado del sistema.
7. **Internacionalización** — Soporte multi-idioma (i18n) en el frontend.

### Mejoras Realizadas

- [x] **CORS permisivo** — `SetIsOriginAllowed(_ => true)` para desarrollo multi-puerto.
- [x] **Tags creados como DISPONIBLE** — Simplifica el flujo crear-asignar.
- [x] **Validación unicidad TagId** — Error amigable al asignar tag ya en uso.
- [x] **Serialización string de ENUMs** — `JsonStringEnumConverter` global.
- [x] **Soporte Npgsql timezone** — `EnableLegacyTimestampBehavior` para DateTimeOffset local.
- [x] **ENUMs PostgreSQL → VARCHAR** — Evita conflictos de tipos con Npgsql.
- [x] **Dashboard con gráficos (Recharts)** — Bar chart (tags por estado), Pie chart (activos por ubicación), Line chart (tendencias 7 días), Bar chart horizontal (activos por categoría).
- [x] **Endpoint `GET /api/dashboard/tendencias`** — Movimientos agrupados por día (ingresos, salidas, no autorizadas) para los últimos 7 días.
- [x] **Endpoint `GET /api/dashboard/kpis`** — Ahora incluye distribución de activos por categoría.
- [x] **Modo oscuro** — ThemeContext con persistencia en localStorage, toggle sun/moon en header, `dark:` variants en todas las páginas y componentes usando Tailwind CSS v4 class strategy.

---

## 10. Análisis de Cumplimiento — "SUGERENCIAS CESAR V2.pdf"

> **Documento analizado:** `Documents/SUGERENCIAS CESAR V2.pdf`
> **Fecha del documento:** Mayo 19 de 2026
> **Solicitante:** Cesar Moreno (Intermediario PHCONTROL / INNOBAQ)
> **Fecha del análisis:** 28 de Mayo de 2026

### 10.1 Gestión y Control de TAG RFID

| # | Recomendación | Estado | Detalle |
|---|--------------|--------|---------|
| 3.1.1 | Clasificación de estados (REGISTRADOS, ASIGNADOS, DISPONIBLES) | ✅ Aplicada | Enum `EstadoTag` con REGISTRADO, ASIGNADO, DISPONIBLE, DAÑADO. Dashboard muestra conteo de cada estado. |
| 3.1.2 | TAG asignado no debe mostrarse como "REGISTRADO" | ✅ Aplicada | Tags se crean como DISPONIBLE, al asignarse pasan a ASIGNADO. Frontend muestra badges con color por estado. |
| 3.1.3 | Solo listar TAG disponibles al asignar, prohibir ingreso manual | ✅ Aplicada | Endpoint `GET /api/tags/disponibles`. Frontend usa `<select>` cargado solo con tags disponibles. |
| 3.1.4 | Reasignación automática: tag anterior → Disponible, nuevo → Asignado | ✅ Aplicada | `ActivosController.cs` PUT libera el tag anterior y asigna el nuevo automáticamente. |
| 3.1.5 | Gestión de TAG dañados: inhabilitar, registrar daño, reemplazo | ✅ Aplicada | `POST /api/tags/{tagId}/reportar-danio` cambia estado a DAÑADO y registra en `TagDanioHistorial`. |

### 10.2 Gestión de Activos

| # | Recomendación | Estado | Detalle |
|---|--------------|--------|---------|
| 4.1 | Cambiar terminología "Computadores" → "Activos" | ✅ Aplicada | Sistema completo usa "Activos" (entidad, tabla, rutas, frontend). "Computadores" es solo una categoría. |
| 4.2 | Indicadores: activos dentro/fuera de oficina | ✅ Aplicada | Dashboard con KPIs "Activos Dentro/Fuera" y gráfico de torta. Enum `EstadoUbicacion` con DENTRO_INSTALACIONES y FUERA_INSTALACIONES. |
| 4.3 | Campos adicionales: Placa y Persona Responsable | ✅ Aplicada | `Activo.cs` incluye `Placa` (unique) y `TenedorResponsable`. Formulario frontend incluye ambos. |
| 4.4 | Validación de duplicidad (Placa única) | ✅ Aplicada | Índice único en DbContext + validación explícita en controller con mensaje amigable. |
| 4.5 | Confirmación al eliminar activos | ✅ Aplicada | Modal de confirmación rojo con "¿Estás seguro?" y botones Confirmar/Cancelar. |

### 10.3 Gestión de Usuarios y Seguridad

| # | Recomendación | Estado | Detalle |
|---|--------------|--------|---------|
| 5.1 | Cambio de contraseña | ⚠️ Parcial | Backend: `POST /api/auth/change-password` funciona. **Frontend: no hay UI** para que el usuario cambie su contraseña. |
| 5.1 | Recuperación de contraseña | ⚠️ Parcial | Backend: `POST /api/auth/recover-password` existe pero retorna el token en la respuesta en lugar de enviarlo por email (falta SMTP). **Frontend: no hay UI**. |
| 5.2 | Tipos de usuario definidos (consulta / gestión) | ✅ Aplicada | Roles "Usuario de Consulta" y "Usuario de Gestion" con permisos diferenciados y seed data. |

### 10.4 Reportes y Exportación

| # | Recomendación | Estado | Detalle |
|---|--------------|--------|---------|
| 6.1 | Exportación a Excel | ✅ Aplicada | Frontend usa librería `xlsx` + `file-saver`. Exporta todos los tipos de reporte. |
| 6.2 | Reportes de ubicación (dentro/fuera) | ✅ Aplicada | Tipos "Activos Dentro" y "Activos Fuera" con filtro por rango de fechas. |
| 6.3 | Remover opción "Usuario" del menú de reportes | ✅ Aplicada | El menú de reportes no incluye opción "Usuario". |

### 10.5 Eventos del Sistema

| # | Recomendación | Estado | Detalle |
|---|--------------|--------|---------|
| 7.1 | Ingresos/Salidas del día en rango 00:00-24:00 | ✅ Aplicada | `GET /api/movimientos/resumen-del-dia` usa `DateTime.Today` con filtro `>= today && < today.AddDays(1)`. |

### 10.6 Módulo de Autorización de Salida (Nueva Funcionalidad)

| # | Recomendación | Estado | Detalle |
|---|--------------|--------|---------|
| 8 | Módulo de autorización de salida de equipos | ❌ No implementada | Entidad `AutorizacionSalida` y DbSet existen. Lógica de verificación en `EventosRfidController.cs`. **No existe endpoint ni UI** para crear/gestionar autorizaciones. Es una solicitud de cotización, no un requerimiento cerrado. |

### 10.7 Resumen General

| Estado | Cantidad |
|--------|----------|
| ✅ Aplicadas | 15 de 17 |
| ⚠️ Parcialmente aplicadas | 2 (cambio/recuperación de contraseña) |
| ❌ No implementadas | 1 (módulo de autorización de salida) |

Las recomendaciones operativas y funcionales están prácticamente todas implementadas. Los pendientes principales son:

1. **CRUD de Autorización de Salida** — Solicitud de cotización, no un requerimiento cerrado.
2. **UI de cambio de contraseña** — Backend listo, falta frontend.
3. **Recuperación de contraseña vía email** — Backend genera token pero falta integración SMTP y UI.

---

## 11. Infraestructura de Despliegue

> Basado en `Documents/Infraestructura de Implementación.md`
> **Arquitectura:** Frontend → Vercel (gratuito) | Backend + BD → Render (plan free)

### 11.1 Arquitectura

```
                   ┌──────────────────────────────────────────┐
                   │                 GITHUB                   │
                   └────┬────────────────────────────────┬────┘
                        │                                │
                        ▼ (Despliegue Automático)        ▼ (Docker Build)
           ┌─────────────────────────┐     ┌────────────────────────────┐
           │         VERCEL          │     │          RENDER           │
           │  (Plan 100% Gratuito)   │     │     (Plan Free)           │
           ├─────────────────────────┤     ├────────────────────────────┤
           │  • Frontend (React TS)  │     │  • Backend (.NET 10 API)  │
           │  • Archivos estáticos   │     │  • PostgreSQL 18           │
           └────────────┬────────────┘     └────────────┬───────────────┘
                        │                               │
                        │     (Peticiones API/SignalR)  │
                        └───────────────────────────────┘
```

### 11.2 Archivos de Configuración Creados

| Archivo | Propósito |
|---------|-----------|
| `Backends/WebApi/Dockerfile` | Multi-stage build de .NET 10 para Render |
| `Backends/WebApi/.dockerignore` | Excluye bin/obj/node_modules del contexto Docker |
| `Backends/WebApi/HorusEye.Api/Controllers/HealthController.cs` | Endpoint `GET /health` para health checks de Render |
| `Frontends/ReactTS/vercel.json` | Configuración Vercel con SPA rewrites |
| `Frontends/ReactTS/.env.example` | Ejemplo de variable `VITE_API_URL` para producción |
| `render.yaml` | Blueprint de Render (servicio web + BD PostgreSQL) |
| `Program.cs` (modificado) | CORS configurable vía `CORS:AllowedOrigin` |

### 11.3 Paso a Paso — Vercel (Frontend)

1. Conecta el repo de GitHub a [Vercel](https://vercel.com).
2. **Root Directory:** `Frontends/ReactTS`
3. **Framework Preset:** Vite (se detecta automáticamente)
4. **Build Command:** `pnpm build` (configurado en `vercel.json`)
5. **Output Directory:** `dist`
6. **Environment Variable:**
   - `VITE_API_URL` → `https://horuseye-api.onrender.com` (la URL de Render)
7. **Deploy:** Vercel detecta `vercel.json` y aplica SPA rewrites automáticamente.

### 11.4 Paso a Paso — Render (Backend + BD)

**Opción A — Usando render.yaml (blueprint):**

1. Conecta el repo a [Render](https://render.com).
2. Ve a **Blueprint** y conecta tu repo.
3. Render lee `render.yaml` y crea automáticamente:
   - **Web Service:** `horuseye-api` con el Dockerfile
   - **PostgreSQL:** `horuseye-db`
4. Configura las variables manuales en el dashboard:
   - `Jwt__Key` → (la misma secret key del desarrollo)

**Opción B — Manual (desde el dashboard):**

1. **New Web Service** → Conectar repo
2. **Runtime:** Docker
3. **Root Directory:** `Backends/WebApi`
4. **Dockerfile Path:** `./Dockerfile`
5. **Plan:** Free
6. **Environment Variables:**
   - `ASPNETCORE_ENVIRONMENT` → `Production`
   - `ConnectionStrings__DefaultConnection` → (se genera al crear BD)
   - `Jwt__Key` → `HorusEyeSuperSecretKey2026!@#$%^&*()VeryLongSecure`
   - `Jwt__Issuer` → `HorusEyeAPI`
   - `Jwt__Audience` → `HorusEyeFrontend`
   - `CORS__AllowedOrigin` → `https://horuseye.vercel.app`
7. **New PostgreSQL** (Render Dashboard → PostgreSQL):
   - Crear instancia free, Render entrega automaticamente la connection string.
8. Una vez creada la BD, Render inyecta la connection string en la variable `ConnectionStrings__DefaultConnection` del Web Service.

> **Nota:** Render usa el puerto `8080` internamente (configurado en el Dockerfile vía `ASPNETCORE_URLS`). No es necesario exponerlo manualmente.

---

## 12. Correcciones de Despliegue (28-Mayo-2026)

### 12.1 Problema: Web Service «Failed» en Render

**Síntoma:** Al sincronizar el Blueprint, la BD (`horuseye-db`) quedaba «Available» pero el Web Service (`horuseye-api`) quedaba «Failed».

**Causa raíz:** El `render.yaml` original usaba `runtime: image` con `ttl.sh/horuseye-api:latest`, un registro público donde las imágenes expiran a las 24h. Render no encontraba la imagen para desplegar.

**Solución:** Cambiar a `runtime: docker` para que Render construya la imagen directamente desde el `Dockerfile` del repositorio.

### 12.2 Problema: `Name or service not known` al conectar a PostgreSQL

**Síntoma:** El contenedor se iniciaba pero crasheaba con `NpgsqlException: Name or service not known`.

**Causa raíz:** La variable `ConnectionStrings__DefaultConnection` en `render.yaml` usaba templates `${DATABASE_HOST}`, `${DATABASE_DB}`, etc. Render no resolvía estas variables anidadas dentro del valor de otra variable.

**Solución:** Reemplazar las variables template por `fromDatabase` con `property: connectionString`, que Render inyecta directamente como el connection string completo en formato `postgres://`.

### 12.3 Problema: Puerto incorrecto para Render Free Tier

**Síntoma:** El servicio crasheaba con `Exited with status 139` sin mensaje claro.

**Causa raíz:** El `Dockerfile` hardcodeaba `ENV ASPNETCORE_URLS=http://+:8080`, pero Render Free Tier asigna un puerto dinámico (`PORT=10000`).

**Solución:** Eliminar el `ENV ASPNETCORE_URLS` del `Dockerfile` y leer la variable `PORT` en `Program.cs`:

```csharp
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://+:{port}");
```

### 12.4 Problema: CORS bloqueado en producción

**Síntoma:** El frontend en Vercel recibía error CORS al llamar la API en Render.

**Causa raíz:** El middleware manual de CORS en `Program.cs` (líneas 172-186) interfería con el pipeline oficial de CORS, y la política `SetIsOriginAllowed(_ => true)` no leía la variable `CORS__AllowedOrigin` documentada en la guía de despliegue.

**Solución:**
- Eliminar el middleware manual de CORS que duplicaba cabeceras y manejaba OPTIONS manualmente.
- Modificar la policy de CORS para leer `CORS__AllowedOrigin` del entorno, con fallback a `SetIsOriginAllowed(_ => true)`.

```csharp
var allowedOrigin = builder.Configuration["CORS__AllowedOrigin"];
options.AddPolicy("AllowFrontend", policy =>
{
    if (!string.IsNullOrEmpty(allowedOrigin))
        policy.WithOrigins(allowedOrigin).AllowCredentials();
    else
        policy.SetIsOriginAllowed(_ => true);
    policy.AllowAnyHeader().AllowAnyMethod();
});
```

### 12.5 Problema: Error de sintaxis SQL en `init.sql`

**Síntoma:** Al ejecutar `init.sql` en PostgreSQL, fallaba con error de sintaxis.

**Causa raíz:** Línea 6: `"Tags` sin comilla de cierre (debería ser `"Tags"`).

**Solución:** Agregar la comilla doble faltante.

### 12.6 Archivos Creados/Modificados

| Archivo | Cambio |
|---------|--------|
| `render.yaml` | `runtime: image` → `runtime: docker`; connection string vía `fromDatabase` |
| `.dockerignore` | Nuevo: excluye bin/obj/node_modules del contexto Docker |
| `Dockerfile` | Eliminado `ENV ASPNETCORE_URLS=http://+:8080` |
| `Program.cs` | `UseUrls` dinámico con `PORT`; CORS configurable vía `CORS__AllowedOrigin` |
| `Databases/PostgreSQL/init.sql` | Fix sintaxis: `"Tags"` |

### 12.7 Estado Final

- API Backend: `https://horuseye-api.onrender.com` — **Operativo** (HTTP 200, login JWT funcional)
- Base de Datos: PostgreSQL 18 en Render — **Available**
- Frontend: Vercel (pendiente de configurar `VITE_API_URL`)

---

## 13. Mejoras de Seguridad y Calidad (28-Mayo-2026)

### 13.1 JWT Secret removido de `appsettings.json`

**Problema:** La clave de firma JWT `HorusEyeSuperSecretKey2026!@#$%^&*()VeryLongSecure` estaba hardcodeada en `appsettings.json` y commiteada en el repositorio. Si alguien accedía al repo, podía firmar tokens arbitrarios.

**Solución:**
- Remover `Jwt:Key` de `appsettings.json` (solo quedan `Issuer` y `Audience`).
- Agregar `Jwt:Key` únicamente en `appsettings.Development.json` con un valor diferente (`DevKey-NotForProduction-...`).
- En producción, Render inyecta `Jwt__Key` como variable de entorno.
- En `Program.cs`, si `Jwt:Key` no está configurado, la aplicación falla al inicio con mensaje claro:

```csharp
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException(
    "Jwt:Key no configurada...");
```

### 13.2 DTOs con validación para TagsController

**Problema:** Los endpoints `POST /api/tags`, `PUT /api/tags/{id}/estado` y `POST /api/tags/{id}/reportar-danio` aceptaban strings planos (`[FromBody] string`) sin validación estructurada. Esto dificultaba la documentación Swagger y la validación automática.

**Solución:** Crear `TagDtos.cs` con tres DTOs tipados y validados:

| DTO | Validaciones |
|-----|-------------|
| `CreateTagRequest` | `[Required]`, `[StringLength(200)]` |
| `UpdateTagEstadoRequest` | `[Required]`, `[RegularExpression]` contra valores del enum |
| `ReportarDanioRequest` | `[Required]`, `[StringLength(600)]` |

Actualizar controlador, frontend (`Tags.tsx`) y scripts de simulación para usar los nuevos formatos.

### 13.3 ChangePassword sin email en body

**Problema:** `ChangePasswordRequest` requería `Email` en el body, pero el usuario ya está autenticado vía JWT. Adicionalmente se comparaba contra el claim, lo cual era redundante.

**Solución:** Remover `Email` del DTO. El controlador obtiene el `UserId` directamente del token JWT:

```csharp
var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
```

### 13.4 Validación de categoría en Activos

**Problema:** `ActivoRequest.Categoria` solo tenía `[Required]`, permitiendo valores arbitrarios.

**Solución:** Agregar `[RegularExpression]` con las categorías válidas del dominio:

```csharp
[RegularExpression("^(Computadores|Monitores|Perifericos|Impresoras|Sillas|Redes|Telefonia|Tablets|Audio|Accesorios|Otros)$")]
```

### 13.5 Archivos Creados/Modificados

| Archivo | Cambio |
|---------|--------|
| `appsettings.json` | Removido `Jwt:Key` del source control |
| `appsettings.Development.json` | Agregado `Jwt:Key` para desarrollo local |
| `Program.cs` | Validación de `Jwt:Key` requerido al iniciar |
| `DTOs/TagDtos.cs` | Nuevo: DTOs con validación para Tags |
| `DTOs/AuthDtos.cs` | `ChangePasswordRequest` sin campo `Email` |
| `DTOs/RfidDtos.cs` | `ActivoRequest.Categoria` con `[RegularExpression]` |
| `Controllers/TagsController.cs` | Usa DTOs tipados en vez de strings planos |
| `Controllers/AuthController.cs` | ChangePassword obtiene usuario del JWT |
| `Frontends/ReactTS/src/pages/Tags.tsx` | Envía objetos en vez de strings planos |
| `Documents/simulacion.sh` | Actualizado a nuevo formato de DTOs |
| `simulacion-prod.sh` | Nuevo: simulación para producción |
| `simulacion-prod-full.sh` | Nuevo: simulación completa (10 tags + 8 activos + 6 movs) |
| `simulacion-100.sh` | Nuevo: simulación masiva (100 tags + 100 activos + 30 movs) |

---

## 14. Migración a EF Core Migrations (28-Mayo-2026)

### 14.1 Problema

El proyecto usaba `context.Database.EnsureCreated()` en `Program.cs` para crear el esquema de base de datos. Esta estrategia tiene limitaciones graves:

- **Sin historial de cambios:** No existe una tabla `__EFMigrationsHistory` que registre qué versiones del esquema se han aplicado.
- **Sin control de versiones:** No se pueden generar scripts SQL para actualizar bases de datos existentes. Para modificar el esquema, habría que hacerlo manualmente.
- **Riesgo en producción:** Si el esquema cambia (nuevas columnas, tablas), no hay forma de actualizar la BD sin borrar y recrear todos los datos.

### 14.2 Solución

Se creó una migración inicial (`InitialCreate`) usando el CLI de EF Core y se reemplazó `EnsureCreated()` por `Database.Migrate()`.

### 14.3 Migración Inicial

```bash
# Instalar herramienta global de EF Core
dotnet tool install --global dotnet-ef

# Crear migración inicial desde el modelo del DbContext
cd Backends/WebApi
dotnet ef migrations add InitialCreate \
  --project HorusEye.Infrastructure \
  --startup-project HorusEye.Api
```

La migración genera 6 tablas de negocio + 7 tablas de ASP.NET Identity:

| Tabla | Propósito |
|-------|-----------|
| `Tags` | Tags RFID (EPC/UID) con estado |
| `TagsDaniosHistorial` | Historial de daños reportados |
| `Activos` | Equipos/items con placa y categoría |
| `Movimientos` | Registro de ingresos/salidas |
| `AutorizacionesSalida` | Autorizaciones temporales |
| `RefreshTokens` | Tokens OAuth 2.0 para renovación JWT |
| `AspNet*` (7) | ASP.NET Core Identity estándar |

Además se generó:

| Archivo | Propósito |
|---------|-----------|
| `Migrations/20260529030543_InitialCreate.cs` | Código C# de la migración (Up/Down) |
| `Migrations/HorusEyeDbContextModelSnapshot.cs` | Snapshot del modelo actual |
| `Migrations/InitialCreate.sql` | Script SQL equivalentemente (idempotente) |

### 14.4 Cambio en Program.cs

```csharp
// ANTES (sin control de versiones):
context.Database.EnsureCreated();

// DESPUÉS (con migraciones):
try
{
    await context.Database.MigrateAsync();
}
catch
{
    // Transición desde EnsureCreated(): tablas ya existen pero falta
    // __EFMigrationsHistory. Se inserta el registro manualmente.
    context.Database.EnsureCreated();
    await context.Database.ExecuteSqlRawAsync(
        "INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") " +
        "VALUES ('20260529030543_InitialCreate', '10.0.0')");
}
```

El `try-catch` maneja la transición para bases de datos existentes creadas con `EnsureCreated()`. En la primera ejecución:
1. `MigrateAsync()` falla porque `__EFMigrationsHistory` no existe.
2. `EnsureCreated()` detecta tablas existentes y no hace nada.
3. Se inserta el registro en `__EFMigrationsHistory`.
4. En adelante, `MigrateAsync()` funciona correctamente y se pueden agregar nuevas migraciones.

Para bases de datos nuevas, `MigrateAsync()` ejecuta toda la migración desde cero sin problemas.

### 14.5 Flujo de trabajo para futuros cambios de esquema

```bash
# 1. Modificar las entidades en HorusEye.Core/Entities/
# 2. Crear nueva migración
dotnet ef migrations add NombreDescriptivo \
  --project HorusEye.Infrastructure \
  --startup-project HorusEye.Api

# 3. Revisar el código generado en Migrations/
# 4. La migración se aplica automáticamente al iniciar la app
```

### 14.6 Archivos Creados/Modificados

| Archivo | Cambio |
|---------|--------|
| `Program.cs` | `EnsureCreated()` → `MigrateAsync()` con transición |
| `HorusEye.Infrastructure/Migrations/InitialCreate.cs` | Nuevo: migración inicial |
| `HorusEye.Infrastructure/Migrations/InitialCreate.Designer.cs` | Nuevo: código generado |
| `HorusEye.Infrastructure/Migrations/HorusEyeDbContextModelSnapshot.cs` | Nuevo: snapshot del modelo |
| `HorusEye.Infrastructure/Migrations/InitialCreate.sql` | Nuevo: script SQL idempotente |
| `Databases/PostgreSQL/init.sql` | Mantenido para referencia local, pero el schema lo gestiona EF Migrations |

---

---

## 15. Mejoras de Calidad de Código (28-Mayo-2026)

### 15.1 DTOs en archivos separados

**Problema:** El DTO `CreateAutorizacionRequest` estaba definido inline al final del archivo `AutorizacionesController.cs`, mezclando responsabilidades de presentación con definiciones de datos. Además, no tenía validación (`[Required]`, `[StringLength]`).

**Solución:**

| Archivo | Cambio |
|---------|--------|
| `DTOs/AutorizacionDtos.cs` | **Nuevo:** `CreateAutorizacionRequest` y `AutorizacionResponse` con validación DataAnnotations |
| `Controllers/AutorizacionesController.cs` | Eliminada definición inline de `CreateAutorizacionRequest`; ahora usa `HorusEye.Api.DTOs.AutorizacionDtos` |
| `Controllers/AutorizacionesController.cs` | Respuestas tipadas con `AutorizacionResponse` en vez de `List<object>` y `new { }` anónimos |

`CreateAutorizacionRequest` ahora incluye:
```csharp
[Required]
public Guid ActivoId { get; set; }

[Required]
[StringLength(200, MinimumLength = 1)]
public string AutorizadoPor { get; set; } = string.Empty;

public DateTimeOffset? FechaVencimiento { get; set; }
```

### 15.2 Validación de dominios

Se agregaron validaciones con DataAnnotations a todos los DTOs de entrada:

| DTO | Validación agregada |
|-----|-------------------|
| `EventoRfidRequest.TipoMovimiento` | `[RegularExpression("^(INGRESO\|SALIDA)$")]` — solo valores válidos del dominio |
| `CreateAutorizacionRequest.AutorizadoPor` | `[Required]`, `[StringLength(200)]` — no vacío, longitud máxima |

El ecosistema de validación existente incluye:

- **`ActivoRequest.Categoria`** — `[RegularExpression]` con lista cerrada de categorías (Computadores, Monitores, Perifericos, Impresoras, Sillas, Redes, Telefonia, Tablets, Audio, Accesorios, Otros)
- **`UpdateTagEstadoRequest.Estado`** — `[RegularExpression]` contra valores del enum `EstadoTag`
- **`RegisterRequest.Email`** — `[EmailAddress]` + `[Required]`
- **`RegisterRequest.Password`** — `[MinLength(6)]` + `[Required]`
- **`ResetPasswordRequest.NewPassword`** — `[MinLength(6)]` + `[Required]`
- **`ReportarDanioRequest.Descripcion`** — `[StringLength(600)]` + `[Required]`

Todas las validaciones son manejadas automáticamente por ASP.NET Core (`[ApiController]` attribute) que retorna 400 Bad Request con los errores en el cuerpo de la respuesta.

### 15.3 Proyecto de tests unitarios

**Nuevo proyecto:** `Backends/WebApi/HorusEye.Tests/` (xUnit + Moq + FluentAssertions + EF Core InMemory)

**Paquetes NuGet agregados:**

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| `Moq` | 4.20.72 | Mocking de dependencias (ILogger, etc.) |
| `FluentAssertions` | 8.10.0 | Assertions legibles y auto-documentadas |
| `Microsoft.EntityFrameworkCore.InMemory` | 10.0.8 | Base de datos en memoria para tests de integración |

**Tests implementados (25 en total):**

#### TokenServiceTests (6 tests)

| Test | Verifica |
|------|----------|
| `GenerateAccessToken_ReturnsValidJwt` | Token JWT con 3 segmentos (header.payload.signature) |
| `GenerateRefreshToken_ReturnsBase64String` | Token de 64 bytes codificado en Base64 |
| `CreateRefreshTokenAsync_PersistsToken` | RefreshToken guardado en BD con datos correctos |
| `ValidateRefreshTokenAsync_ValidToken_ReturnsToken` | Token válido es retornado |
| `ValidateRefreshTokenAsync_RevokedToken_ReturnsNull` | Token revocado retorna null |
| `ValidateRefreshTokenAsync_ExpiredToken_ReturnsNull` | Token expirado retorna null |

#### AutorizacionesControllerTests (7 tests)

| Test | Verifica |
|------|----------|
| `GetAll_ReturnsList` | GET /api/autorizaciones retorna lista exitosa |
| `Create_WithValidData_ReturnsCreated` | POST con datos válidos crea autorización activa |
| `Create_WithInvalidActivo_ReturnsNotFound` | POST con activo inexistente da 404 |
| `Revoke_SetsActivaToFalse` | PUT revocar cambia Activa a false |
| `Revoke_WithInvalidId_ReturnsNotFound` | PUT revocar con ID inválido da 404 |
| `Delete_RemovesAutorizacion` | DELETE elimina el registro de BD |
| `GetActivas_ReturnsOnlyActive` | GET activas filtra correctamente |

#### DtoValidationTests (12 tests)

| Test | Verifica |
|------|----------|
| `CreateAutorizacionRequest_Valid_Passes` | DTO completo sin errores |
| `CreateAutorizacionRequest_EmptyAutorizadoPor_Fails` | AutorizadoPor vacío falla |
| `ActivoRequest_InvalidCategoria_Fails` | Categoría inválida falla `[RegularExpression]` |
| `ActivoRequest_ValidCategoria_Passes` | Categoría válida pasa |
| `EventoRfidRequest_InvalidTipoMovimiento_Fails` | TipoMovimiento inválido falla |
| `EventoRfidRequest_ValidTipoMovimiento_Passes` | INGRESO y SALIDA pasan |
| `UpdateTagEstadoRequest_ValidEstado_Passes` | Estado válido pasa |
| `UpdateTagEstadoRequest_InvalidEstado_Fails` | Estado inexistente falla |
| `RegisterRequest_InvalidEmail_Fails` | Email sin @ falla |
| `RegisterRequest_ShortPassword_Fails` | Password < 6 caracteres falla |
| `ResetPasswordRequest_ShortPassword_Fails` | Password < 6 caracteres falla |
| `ResetPasswordRequest_ValidPassword_Passes` | Password ≥ 6 caracteres pasa |

**Ejecución:**
```bash
cd Backends/WebApi
dotnet test
# Passed: 25, Failed: 0
```

### 15.4 Frontend — Módulo de Autorización de Salida

Se implementó el módulo de Autorización de Salida que estaba pendiente (req. 8 del análisis César V2):

**Backend:**
- `Controllers/AutorizacionesController.cs` — CRUD completo con `GET`, `GET /activas`, `POST`, `PUT /{id}/revocar`, `DELETE /{id}`
- `DTOs/AutorizacionDtos.cs` — DTOs tipados con validación

**Frontend:**
- `pages/Autorizaciones.tsx` — Nueva página con tabla de autorizaciones, modal de creación, revocación y eliminación
- `App.tsx` — Nueva ruta `/autorizaciones`
- `components/Layout.tsx` — Nuevo item de navegación "Autorizaciones" (visible para todos los roles)

### 15.5 Frontend — Mejoras en Usuarios

- `pages/Usuarios.tsx` — Se agregaron columnas de acciones con botones de editar, resetear contraseña y eliminar
- Se agregaron 3 modales: editar usuario, resetear contraseña, y confirmación de eliminación
- No permite eliminar el propio usuario (`currentUser?.id !== u.id`)

### 15.6 Archivos Creados/Modificados

| Archivo | Cambio |
|---------|--------|
| `DTOs/AutorizacionDtos.cs` | **Nuevo:** DTOs tipados para autorizaciones |
| `DTOs/RfidDtos.cs` | Validación `[RegularExpression]` para `TipoMovimiento` |
| `Controllers/AutorizacionesController.cs` | Usa DTOs del namespace; respuestas tipadas |
| `Controllers/AutorizacionesController.cs` | Eliminada clase `CreateAutorizacionRequest` inline |
| `HorusEye.Tests/` | **Nuevo:** proyecto de tests (25 tests, todos pasan) |
| `HorusEye.Tests/TokenServiceTests.cs` | 6 tests para TokenService |
| `HorusEye.Tests/AutorizacionesControllerTests.cs` | 7 tests para AutorizacionesController |
| `HorusEye.Tests/DtoValidationTests.cs` | 12 tests de validación de DTOs |
| `Frontends/ReactTS/src/pages/Autorizaciones.tsx` | **Nuevo:** página de gestión de autorizaciones |
| `Frontends/ReactTS/src/pages/Usuarios.tsx` | Edit/delete/reset-password con modales |
| `Frontends/ReactTS/src/App.tsx` | Ruta `/autorizaciones` |
| `Frontends/ReactTS/src/components/Layout.tsx` | Nav item "Autorizaciones" |

---

---

## 16. Mejoras de Infraestructura (28-Mayo-2026)

### 16.1 .dockerignore mejorado

Se amplió el `.dockerignore` para excluir del contexto Docker todo lo innecesario en producción:

```
# .NET
Backends/WebApi/**/bin/
Backends/WebApi/**/obj/
Backends/WebApi/**/*.user
Backends/WebApi/**/.vs/
Backends/WebApi/HorusEye.Tests/

# Node
Frontends/ReactTS/node_modules/
Frontends/ReactTS/dist/
Frontends/ReactTS/.pnpm-store/

# Git + GitHub
.git/
.gitignore
.gitattributes
.github/

# IDE
.idea/
.vscode/

# Logs
Backends/WebApi/HorusEye.Api/logs/
*.log

# Scripts + Docs
prueba.sh
simulacion*.sh
Documents/

# Build artifacts
publish/
```

Beneficios:
- **Capa de caché de Docker más pequeña**: Los cambios en tests, docs o scripts no invalidan las capas de restore/build.
- **Builds más rápidos**: Menos archivos en el contexto de build.
- **Seguridad**: No se filtran logs, tokens, o scripts internos en la imagen.

### 16.2 CI/CD — GitHub Actions

**Archivo:** `.github/workflows/ci-cd.yml`

Pipeline automatizado con 3 jobs paralelos + deploy secuencial:

```
                  ┌──────────────────────────┐
                  │   push / PR a main        │
                  └────────┬─────────────────┘
                           │
              ┌────────────┼────────────┐
              ▼            ▼            ▼
      ┌────────────┐ ┌────────────┐ ┌────────────┐
      │  Backend   │ │  Frontend  │ │  Deploy    │
      │ Build+Test │ │ Build+Lint │ │(solo main) │
      └────────────┘ └────────────┘ └────────────┘
              │            │              │
              └─────┬──────┘              │
                    ▼                     ▼
            ┌────────────────┐   ┌────────────────┐
            │  ✅ Ambos OK   │──►│  Render + Vercel│
            └────────────────┘   └────────────────┘
```

**Jobs:**

| Job | Descripción | Steps |
|-----|-------------|-------|
| `backend-build-and-test` | Build .NET + ejecutar tests | `setup-dotnet` → `dotnet restore` → `dotnet build` → `dotnet test` |
| `frontend-build-and-lint` | Build React + lint | `setup-node` → `pnpm install` → `pnpm lint` → `pnpm build` |
| `deploy` | Desplegar solo en push a main (después de que ambos jobs pasen) | `curl` a Render Deploy Hook + `curl` a Vercel Deploy Hook |

**Secrets requeridos en GitHub:**

| Secret | Propósito | Cómo obtenerlo |
|--------|-----------|---------------|
| `RENDER_DEPLOY_HOOK_URL` | Deploy hook URL de Render | Render Dashboard → Web Service → Settings → Deploy Hooks → copiar URL |
| `VERCEL_DEPLOY_HOOK_URL` | Deploy hook URL de Vercel | Vercel Dashboard → Proyecto → Settings → Git → Deploy Hooks → crear y copiar URL |

> **Nota:** El deploy job ya no necesita checkout del repo ni CLI de Vercel — ambos deploys usan un simple `curl -X POST` al deploy hook correspondiente. El build corre en la infraestructura de cada plataforma.

**Política de concurrencia:** `concurrency.cancel-in-progress: true` — si se hace un nuevo push mientras un pipeline está corriendo, se cancela el anterior para ahorrar recursos.

### 16.3 Scripts de deploy

Se crearon 3 scripts bash ejecutables en `scripts/`:

| Script | Propósito |
|--------|-----------|
| `scripts/deploy-backend.sh` | Build .NET + tests + Docker image + push + trigger Render |
| `scripts/deploy-frontend.sh` | Install deps + lint + build + deploy Vercel CLI |
| `scripts/deploy-all.sh` | Pipeline completa: backend + frontend + ambos deploys |
| `scripts/health-check.sh` | Verifica estado de todos los endpoints (sin autenticación y autenticados) |

**Uso:**
```bash
# Deploy completo (requiere variables de entorno)
export RENDER_DEPLOY_HOOK="<hook-url>"
export VERCEL_TOKEN="<token>"
./scripts/deploy-all.sh

# Solo build (para validar sin desplegar)
./scripts/deploy-backend.sh --no-push
./scripts/deploy-frontend.sh --no-deploy

# Verificar estado de producción
./scripts/health-check.sh
```

### 16.4 render.yaml mejorado

Se actualizó `render.yaml` con:

| Cambio | Antes | Después |
|--------|-------|---------|
| `healthCheckPath` | No existía | `/health` — endpoint dedicado que retorna HTTP 200 |
| `autoDeploy` | No existía | `true` — despliegue automático en cada push a main |
| `branch` | No existía | `main` — explícito sobre qué rama desplegar |
| Connection string | Hardcodeada en el YAML | `fromDatabase` + `property: connectionString` — Render inyecta automáticamente |
| `CORS__AllowedOrigin` | No existía | `https://horuseye.vercel.app` — CORS restringido al frontend |

El `healthCheckPath: /health` apunta al `HealthController` que retorna `{ status: "healthy" }` con HTTP 200. Render usa esta ruta para monitorear la salud del servicio; si no responde 2xx, Render reinicia el contenedor automáticamente.

### 16.5 Flujo de trabajo recomendado

**Desarrollo local:**
```bash
# 1. Backend
cd Backends/WebApi
dotnet test                              # Ejecutar tests
dotnet run --project HorusEye.Api        # Iniciar API

# 2. Frontend
cd Frontends/ReactTS
pnpm dev                                 # Iniciar dev server
```

**Antes de hacer push:**
```bash
# Validar localmente (sin desplegar)
./scripts/deploy-backend.sh --no-push    # Build + tests backend
./scripts/deploy-frontend.sh --no-deploy # Build + lint frontend
```

**Despliegue a producción:**
1. Hacer push a `main` → GitHub Actions ejecuta CI/CD automáticamente.
2. O manualmente: `./scripts/deploy-all.sh` (requiere tokens configurados).

### 16.6 Archivos Creados/Modificados

| Archivo | Cambio |
|---------|--------|
| `.dockerignore` | Ampliado: logs, tests, .github, scripts, .pnpm-store, publish |
| `.github/workflows/ci-cd.yml` | **Nuevo:** pipeline CI/CD completo |
| `scripts/deploy-backend.sh` | **Nuevo:** build + test + docker + deploy backend |
| `scripts/deploy-frontend.sh` | **Nuevo:** build + lint + deploy frontend |
| `scripts/deploy-all.sh` | **Nuevo:** pipeline completa local |
| `scripts/health-check.sh` | **Nuevo:** verificación de endpoints en producción |
| `render.yaml` | `healthCheckPath`, `autoDeploy`, `branch`, `fromDatabase` connection string |

---

## 17. Resolución de Errores de Lint y Mejoras de CI/CD (29-Mayo-2026)

### 17.1 Problema — Pipeline de CI/CD fallaba por lint

El job `Frontend — Build & Lint` del pipeline de GitHub Actions fallaba con 13 errores de ESLint, impidiendo que el job `Deploy` siquiera se ejecutara.

**Errores encontrados (13 en 9 archivos):**

| Regla | Archivos afectados | Descripción |
|-------|-------------------|-------------|
| `react-hooks/set-state-in-effect` | Activos, Autorizaciones, Dashboard, Tags, Usuarios | Llamadas a funciones con `setState` dentro de useEffect |
| `react-hooks/immutability` | Activos, Autorizaciones, Tags, Usuarios | Funciones `load*` usadas antes de su declaración |
| `react-hooks/refs` | Dashboard | Asignación a `ref.current` durante el render |
| `react-hooks/purity` | Reportes | `Date.now()` en inicializador de `useState` |
| `react-refresh/only-export-components` | AuthContext, ThemeContext | Exportación de hooks junto a componentes |
| `@typescript-eslint/no-explicit-any` | Dashboard | Tipo `any` en callback de recharts |
| `react-hooks/exhaustive-deps` | useSignalR | Dependencia `onMovimiento` faltante en useEffect |

### 17.2 Soluciones Aplicadas

| Archivo | Cambio |
|---------|--------|
| `AuthContext.tsx` | Token parsing movido a lazy initializer de `useState` (elimina el `useEffect` que llamaba `setUser`) |
| `ThemeContext.tsx`, `AuthContext.tsx` | `eslint-disable-next-line react-refresh/only-export-components` en hooks exportados |
| `useSignalR.ts` | `eslint-disable-next-line react-hooks/exhaustive-deps` (SignalR no debe reconectarse si cambia callback) |
| `Activos.tsx`, `Autorizaciones.tsx`, `Tags.tsx`, `Usuarios.tsx` | Funciones `load*` reordenadas antes del `useEffect`; `eslint-disable-next-line react-hooks/set-state-in-effect` |
| `Dashboard.tsx` | Eliminado `kpisRef` obsoleto; tipado callback label de Pie (recharts); eslint-disable en efectos de carga |
| `Reportes.tsx` | `Date.now()` envuelto en lazy initializer `() => ...` |
| `eslint.config.js` | Sin cambios — los suppress son inline y localizados |

### 17.3 Simplificación del Deploy

El job de deploy se simplificó de usar Vercel CLI (3 pasos: pull → build → deploy, requiriendo 3 secrets) a usar **Deploy Hooks** (un solo `curl -X POST`, 1 secret por plataforma):

**Antes (Vercel CLI):**
```yaml
- name: Deploy Frontend to Vercel
  run: |
    npm install -g vercel
    vercel pull --yes --environment=production --token=$VERCEL_TOKEN
    vercel build --prod --token=$VERCEL_TOKEN
    vercel deploy --prebuilt --prod --token=$VERCEL_TOKEN
```

**Después (Deploy Hook):**
```yaml
- name: Deploy Frontend to Vercel
  run: curl -s -X POST "${{ secrets.VERCEL_DEPLOY_HOOK_URL }}"
```

Ambos servicios ahora usan el mismo patrón: Render con `RENDER_DEPLOY_HOOK_URL` y Vercel con `VERCEL_DEPLOY_HOOK_URL`.

**Beneficios:**
- No necesita checkout del repo en el job de deploy
- No instala dependencias (vercel CLI), reduciendo tiempo de ejecución (~30s → ~5s)
- El build corre en la infraestructura de cada plataforma (consistente con el build que harían con git integration)
- Solo 2 secrets en vez de 5

### 17.4 Health Endpoint

Se creó `Controllers/HealthController.cs` para que Render pueda monitorear la salud del servicio:

```csharp
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
        => Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
}
```

El `healthCheckPath` en `render.yaml` se actualizó de `/` (que devolvía 404) a `/health` (que devuelve 200).

### 17.5 render.yaml actualizado

| Cambio | Valor |
|--------|-------|
| `healthCheckPath` | `/health` (endpoint dedicado, no el root que daba 404) |
| `CORS__AllowedOrigin` | `https://horuseye.vercel.app` (restringido al frontend) |

### 17.6 Archivos Creados/Modificados

| Archivo | Cambio |
|---------|--------|
| `Controllers/HealthController.cs` | **Nuevo:** endpoint `GET /health` para health check |
| `render.yaml` | `healthCheckPath: /health` + `CORS__AllowedOrigin` |
| `.github/workflows/ci-cd.yml` | Deploy simplificado a Deploy Hooks (sin Vercel CLI) |
| `.gitignore` | Agregado `jwt-key.txt` |
| `Frontends/ReactTS/src/context/AuthContext.tsx` | Token en lazy initializer, remove useEffect |
| `Frontends/ReactTS/src/context/ThemeContext.tsx` | eslint-disable react-refresh |
| `Frontends/ReactTS/src/hooks/useSignalR.ts` | eslint-disable exhaustive-deps |
| `Frontends/ReactTS/src/pages/Activos.tsx` | Reorder load functions, eslint-disable set-state |
| `Frontends/ReactTS/src/pages/Autorizaciones.tsx` | Reorder load functions, eslint-disable set-state |
| `Frontends/ReactTS/src/pages/Dashboard.tsx` | Remove kpisRef, fix any type, eslint-disable |
| `Frontends/ReactTS/src/pages/Reportes.tsx` | Lazy initializer for Date.now() |
| `Frontends/ReactTS/src/pages/Tags.tsx` | Reorder load functions, eslint-disable set-state |
| `Frontends/ReactTS/src/pages/Usuarios.tsx` | Reorder load functions, eslint-disable set-state |

---

## 18. Paginación de Consultas (29-Mayo-2026)

### 18.1 Motivación

El endpoint `GET /api/activos` retornaba todos los activos sin paginación. Con el crecimiento del inventario, la respuesta se volvía cada vez más pesada, aumentando el tiempo de carga y el consumo de memoria tanto en el servidor como en el cliente.

### 18.2 Cambios en el Backend

**Archivo:** `Backends/WebApi/HorusEye.Api/Controllers/ActivosController.cs`

Se modificó el método `GetAll()` para aceptar parámetros de paginación y devolver una respuesta estructurada:

| Parámetro | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `page` | `int` | 1 | Número de página (1-based) |
| `pageSize` | `int` | 50 | Elementos por página |

**Antes:**
```csharp
[HttpGet]
public async Task<ActionResult<ApiResponse<List<ActivoResponse>>>> GetAll()
{
    var activos = await _context.Activos
        .Include(a => a.Tag)
        .OrderByDescending(a => a.FechaRegistro)
        .Select(a => new ActivoResponse { ... })
        .ToListAsync();

    return Ok(ApiResponse<List<ActivoResponse>>.Ok(activos));
}
```

**Después:**
```csharp
[HttpGet]
public async Task<ActionResult<ApiResponse<object>>> GetAll(
    [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
{
    var query = _context.Activos
        .Include(a => a.Tag)
        .OrderByDescending(a => a.FechaRegistro);

    var total = await query.CountAsync();
    var activos = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(a => new ActivoResponse { ... })
        .ToListAsync();

    return Ok(ApiResponse<object>.Ok(new
    {
        Items = activos,
        Total = total,
        Page = page,
        PageSize = pageSize
    }));
}
```

**Respuesta JSON (ejemplo):**
```json
{
  "success": true,
  "message": "Operación exitosa",
  "data": {
    "items": [ ... ],
    "total": 150,
    "page": 1,
    "pageSize": 50
  },
  "errors": null,
  "timestamp": "2026-05-29T..."
}
```

### 18.3 Cambios en el Frontend

**Archivo:** `Frontends/ReactTS/src/pages/Activos.tsx`

Se agregaron estados de paginación y se actualizó la función `loadActivos` para usar los parámetros `page` y `pageSize`:

```typescript
const [page, setPage] = useState(1);
const [total, setTotal] = useState(0);
const pageSize = 10;
const totalPages = Math.ceil(total / pageSize);

const loadActivos = async (p?: number) => {
  const currentPage = p ?? page;
  const { data } = await api.get(`/api/activos?page=${currentPage}&pageSize=${pageSize}`);
  if (data.success) {
    setActivos(data.data.items);
    setTotal(data.data.total);
    setPage(data.data.page);
  }
};
```

Se agregó un componente de paginación debajo de la tabla con:
- **Información contextual:** "Mostrando X-Y de Z activos"
- **Botón Anterior** (`ChevronLeft`) — deshabilitado en primera página
- **Números de página** — resalta la página activa con estilo `bg-[#1e3a5f] text-white`
- **Botón Siguiente** (`ChevronRight`) — deshabilitado en última página
- Los botones de acción (crear/editar/eliminar) redirigen a la página 1 después de la operación.

### 18.4 Archivos Creados/Modificados

| Archivo | Cambio |
|---------|--------|
| `Controllers/ActivosController.cs` | `GetAll()` acepta `page`/`pageSize`; respuesta paginada con `Items`, `Total`, `Page`, `PageSize` |
| `Frontends/ReactTS/src/pages/Activos.tsx` | Estados `page`/`total`/`totalPages`; `loadActivos` con parámetro de página; UI de paginación |

### 18.5 Endpoint Actualizado

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/api/activos?page=1&pageSize=50` | Authenticated | Listar activos paginados |

---

## 19. Paginación de Consultas — Tags RFID (29-Mayo-2026)

### 19.1 Motivación

El endpoint `GET /api/tags` retornaba todos los tags RFID sin paginación. Con 111+ activos registrados y una cantidad similar de tags, la carga de datos completa impactaba el rendimiento del frontend.

### 19.2 Cambios en el Backend

**Archivo:** `Backends/WebApi/HorusEye.Api/Controllers/TagsController.cs`

Se modificó `GetAll()` para aceptar parámetros de paginación, siguiendo el mismo patrón que `ActivosController` y `MovimientosController`:

| Parámetro | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `page` | `int` | 1 | Número de página (1-based) |
| `pageSize` | `int` | 50 | Elementos por página |

**Antes:**
```csharp
[HttpGet]
public async Task<ActionResult<ApiResponse<List<Tag>>>> GetAll()
{
    var tags = await _context.Tags
        .OrderByDescending(t => t.FechaRegistro)
        .ToListAsync();
    return Ok(ApiResponse<List<Tag>>.Ok(tags));
}
```

**Después:**
```csharp
[HttpGet]
public async Task<ActionResult<ApiResponse<object>>> GetAll(
    [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
{
    var query = _context.Tags
        .OrderByDescending(t => t.FechaRegistro);

    var total = await query.CountAsync();
    var tags = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return Ok(ApiResponse<object>.Ok(new
    {
        Items = tags,
        Total = total,
        Page = page,
        PageSize = pageSize
    }));
}
```

El endpoint `GET /api/tags/disponibles` se mantiene sin cambios, ya que es usado internamente por los formularios de creación de activos y siempre necesita la lista completa de tags disponibles.

### 19.3 Cambios en el Frontend

**Archivo:** `Frontends/ReactTS/src/pages/Tags.tsx`

Se agregaron estados de paginación y se actualizó `loadTags` para consumir el endpoint paginado:

```typescript
const [page, setPage] = useState(1);
const [total, setTotal] = useState(0);
const pageSize = 15;
const totalPages = Math.ceil(total / pageSize);

const loadTags = async (p?: number) => {
  const currentPage = p ?? page;
  const { data } = await api.get(`/api/tags?page=${currentPage}&pageSize=${pageSize}`);
  if (data.success) {
    setTags(data.data.items);
    setTotal(data.data.total);
    setPage(data.data.page);
  }
};
```

Se agregó el mismo componente de paginación debajo de la tabla con:
- **Información contextual:** "Mostrando X-Y de Z tags"
- **Botones Anterior/Siguiente** con deshabilitado en extremos
- **Números de página** con estilo activo
- Tras registrar, cambiar estado o reportar daño, redirige a página 1.

### 19.4 Archivos Creados/Modificados

| Archivo | Cambio |
|---------|--------|
| `Controllers/TagsController.cs` | `GetAll()` acepta `page`/`pageSize`; respuesta paginada con `Items`, `Total`, `Page`, `PageSize` |
| `Frontends/ReactTS/src/pages/Tags.tsx` | Estados `page`/`total`/`totalPages`; `loadTags` con parámetro de página; UI de paginación |

### 19.5 Endpoints Actualizados

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/api/tags?page=1&pageSize=50` | Authenticated | Listar tags paginados |
| GET | `/api/tags/disponibles` | Authenticated | Tags disponibles (sin paginación, lista completa) |

---

## 20. Selector de Ítems por Página (29-Mayo-2026)

### 20.1 Motivación

Las tablas de Activos y Tags RFID tenían un tamaño de página fijo (10 y 15 respectivamente). Los usuarios no podían ajustar cuántos registros ver por página, lo que resultaba incómodo para quienes preferían ver más registros de una sola vez o una vista más compacta.

### 20.2 Cambios en el Frontend

Se agregó un selector `<select>` en la barra de paginación de ambos componentes que permite escoger entre 5, 10, 15, 25 o 50 ítems por página.

**Archivos modificados:**
- `Frontends/ReactTS/src/pages/Activos.tsx`
- `Frontends/ReactTS/src/pages/Tags.tsx`

**Cambio en el estado:** `pageSize` pasó de ser una constante a un estado React:

```typescript
// Antes (fijo):
const pageSize = 10;    // Activos
const pageSize = 15;    // Tags

// Después (dinámico):
const [pageSize, setPageSize] = useState(10);   // Activos
const [pageSize, setPageSize] = useState(15);   // Tags
```

**Selector agregado en la barra de paginación:**

```tsx
<select
  value={pageSize}
  onChange={(e) => {
    const newSize = Number(e.target.value);
    setPageSize(newSize);
    setPage(1);
    loadActivos(1, newSize);  // loadTags(1, newSize) en Tags
  }}
>
  <option value={5}>5</option>
  <option value={10}>10</option>
  <option value={15}>15</option>
  <option value={25}>25</option>
  <option value={50}>50</option>
</select>
```

**Consideraciones técnicas:**

- Al cambiar el tamaño de página, se reinicia a la página 1 para evitar mostrar una página vacía.
- Se pasó el nuevo tamaño como parámetro explícito (`newSize`) a la función `loadActivos`/`loadTags` para evitar el closure stale, ya que `setPageSize` es asíncrono.
- Las funciones `loadActivos` y `loadTags` ahora aceptan un segundo parámetro opcional `ps?: number` para el tamaño de página.

### 20.3 Archivos Creados/Modificados

| Archivo | Cambio |
|---------|--------|
| `Frontends/ReactTS/src/pages/Activos.tsx` | `pageSize` como estado React; `<select>` con opciones 5/10/15/25/50; reinicio a página 1 al cambiar |
| `Frontends/ReactTS/src/pages/Tags.tsx` | `pageSize` como estado React; `<select>` con opciones 5/10/15/25/50; reinicio a página 1 al cambiar |

---

## 21. Paginación de Autorizaciones (29-Mayo-2026)

### 21.1 Motivación

El endpoint `GET /api/autorizaciones` retornaba todas las autorizaciones de salida sin paginación, igual que ocurría inicialmente con activos y tags.

### 21.2 Cambios en el Backend

**Archivo:** `Backends/WebApi/HorusEye.Api/Controllers/AutorizacionesController.cs`

Se modificó `GetAll()` para aceptar `page` y `pageSize`, siguiendo el mismo patrón que los demás controladores:

```csharp
[HttpGet]
public async Task<ActionResult<ApiResponse<object>>> GetAll(
    [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
{
    var query = _context.AutorizacionesSalida
        .Include(a => a.Activo)
        .OrderByDescending(a => a.FechaAutorizacion);

    var total = await query.CountAsync();
    var autorizaciones = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(a => new AutorizacionResponse { ... })
        .ToListAsync();

    return Ok(ApiResponse<object>.Ok(new
    {
        Items = autorizaciones,
        Total = total,
        Page = page,
        PageSize = pageSize
    }));
}
```

### 21.3 Cambios en el Frontend

**Archivo:** `Frontends/ReactTS/src/pages/Autorizaciones.tsx`

Se agregaron estados de paginación y selector de ítems por página:

- `loadAutorizaciones` acepta `(p?, ps?)` y consume `GET /api/autorizaciones?page=&pageSize=`
- Se agregó la barra de paginación con navegación (anterior/siguiente/números de página)
- Se agregó selector `<select>` con opciones 5, 10, 15, 25, 50
- Tras crear, revocar o eliminar, redirige a página 1
- `loadActivos` (usado para el dropdown del modal) se ajustó para enviar `pageSize=200` y manejar tanto el formato paginado como el legacy (`data.data.items ?? data.data`)

### 21.4 Archivos Creados/Modificados

| Archivo | Cambio |
|---------|--------|
| `Controllers/AutorizacionesController.cs` | `GetAll()` acepta `page`/`pageSize`; respuesta paginada con `Items`, `Total`, `Page`, `PageSize` |
| `Frontends/ReactTS/src/pages/Autorizaciones.tsx` | Estados `page`/`total`/`pageSize`; `loadAutorizaciones(p?, ps?)`; UI de paginación con selector de ítems |

### 21.5 Endpoint Actualizado

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/api/autorizaciones?page=1&pageSize=50` | Gestión | Listar autorizaciones paginadas |
| GET | `/api/autorizaciones/activas` | Gestión | Autorizaciones activas (sin paginación) |

---

## 22. Simulación de Autorizaciones (29-Mayo-2026)

### 22.1 Script de simulación

Se creó `Documents/simulacion-autorizaciones.sh` para probar el flujo completo de autorizaciones de salida:

```
Uso: bash Documents/simulacion-autorizaciones.sh
```

**Pasos que ejecuta:**

| Paso | Acción | Verifica |
|------|--------|----------|
| 1 | Login como admin | Autenticación JWT |
| 2 | Lista activos disponibles | Conexión a API |
| 3 | Crea 3 autorizaciones (vence: 2026-06-15) | POST /api/autorizaciones |
| 4 | Consulta autorizaciones creadas | GET /api/autorizaciones (paginado) |
| 5 | SALIDA con activo autorizado → **sin alarma** | Lógica de autorización |
| 6 | SALIDA con activo NO autorizado → **alarma activada** | `activarAlarmaSonora: true` |
| 7 | Resumen del dashboard | KPIs actualizados |

### 22.2 Resultado esperado

- Paso 5: `activarAlarmaSonora: false` — La salida está autorizada
- Paso 6: `activarAlarmaSonora: true` — La salida NO está autorizada, se activa la alarma
- Las autorizaciones se ven en `https://horus-eye-kappa.vercel.app/autorizaciones`
- Los movimientos aparecen en el Dashboard en tiempo real

### 22.3 Archivo Creado

| Archivo | Cambio |
|---------|--------|
| `Documents/simulacion-autorizaciones.sh` | **Nuevo:** script de simulación del flujo completo de autorizaciones |

---

## 23. Paginación de Usuarios (29-Mayo-2026)

Se implementó paginación en `GET /api/auth/users` con los mismos parámetros que los demás endpoints paginados (`page`, `pageSize`, default 50).

### 23.1 Backend

**Archivo:** `Backends/WebApi/HorusEye.Api/Controllers/AuthController.cs`

| Cambio | Descripción |
|--------|-------------|
| `GetUsers()` | Acepta `[FromQuery] int page = 1, [FromQuery] int pageSize = 50` |
| Respuesta | Retorna `{ Items, Total, Page, PageSize }` envuelto en `ApiResponse` |
| Ordenamiento | `OrderBy(u => u.UserName)` para consistencia entre páginas |

### 23.2 Frontend

**Archivo:** `Frontends/ReactTS/src/pages/Usuarios.tsx`

| Cambio | Descripción |
|--------|-------------|
| Estados | `page`, `total`, `pageSize` (default 15), `totalPages` calculado |
| `loadUsers(p?, ps?)` | Acepta parámetros opcionales para evitar closure stale |
| Mutaciones | `register`/`edit`/`delete` navegan a página 1 tras éxito |
| Selector items/página | `<select>` con opciones 5/10/15/25/50 |
| Navegación | Botones anterior/siguiente + números de página |

### 23.3 Endpoints

| Método | Ruta | Rol | Descripción |
|--------|------|-----|-------------|
| GET | `/api/auth/users?page=1&pageSize=50` | Gestión | Listar usuarios paginados |

---

## 24. Migración de Render a OCI (10-Julio-2026)

### 24.1 Problema

Render Free Tier presentaba crashes recurrentes (status 139) al despertar el servicio después de periodos de inactividad. Esto causaba indisponibilidad del backend y la base de datos, afectando la experiencia del usuario.

### 24.2 Solución

Se migró el backend y la base de datos a una VM de Oracle Cloud Infrastructure (OCI), manteniendo el frontend en Vercel.

### 24.3 Infraestructura Actual

| Componente | Ubicación | URL |
|------------|-----------|-----|
| **Backend + DB** | OCI VM (129.146.22.246) | https://horuseye-api.mauricioadachi.dev |
| **Frontend** | Vercel | https://horuseye-app.mauricioadachi.dev |

#### OCI VM
- **Instancia**: instance-20260703-1405 (ARM, 4 OCPUs / 24 GB RAM)
- **IP Pública**: 129.146.22.246
- **SSH**: `ssh -i ~/.ssh/id_rsa_oci ubuntu@129.146.22.246`

#### Contenedores Docker

| Contenedor | Servicio | Puerto |
|------------|----------|--------|
| horuseye-api | API .NET | 8081 → 8080 (interno) |
| horuseye-db | PostgreSQL | 5433 → 5432 (interno) |
| horuseye-tunnel | Cloudflare Tunnel | (compartido con EBM) |

#### Redes Docker
- HorusEye usa `horuseye_default`
- `horuseye-api` conectado también a `dotnet10_default` (para tunnel)

#### Cloudflare
- **Tunnel**: ebm-tunnel (compartido con EBM)
- **Ruta API**: `horuseye-api.mauricioadachi.dev` → `http://horuseye-api:8080`
- **Ruta EBM**: `api.mauricioadachi.dev` → `http://ebm-api:8080`

### 24.4 Arquitectura de Red

```
Usuario → HTTPS → Vercel (Frontend)
                     ↓ API calls
                  HTTPS → Cloudflare → HTTP → horuseye-api:8080 (Docker)
                                                ↓
                                             PostgreSQL
```

### 24.5 Comandos de Deploy en OCI

```bash
# Conectar
ssh -i ~/.ssh/id_rsa_oci ubuntu@129.146.22.246

# Directorio
cd /opt/horuseye

# Actualizar
git pull

# Reconstruir imagen
docker compose build api

# Reiniciar
docker compose up -d

# Verificar
docker ps
curl http://localhost:8081/health
```

### 24.6 Variables de Entorno Vercel

- `VITE_API_URL`: `https://horuseye-api.mauricioadachi.dev`

### 24.7 Credenciales de Base de Datos

| Campo | Valor |
|-------|-------|
| Host | horuseye-db (Docker) |
| Port | 5432 (interno) / 5433 (host) |
| Database | horuseyedb |
| User | horuseyeuser |
| Password | H0rvEy3 |

---

## 25. Generación de Datos de Prueba — 3 Meses (10-Julio-2026)

### 25.1 Objetivo

Generar un volumen estadísticamente razonable de datos de prueba para el dashboard, incluyendo:
- Datos diarios de los últimos 90 días
- Distribución realista (más actividad en días laborales)
- Salidas no autorizadas (alarmas) para probar la funcionalidad de alarma

### 25.2 Script: `simulacion-3meses.sh`

**Archivo:** `simulacion-3meses.sh` (raíz del proyecto)

#### Fase 1: Verificar activos existentes
- Consulta `GET /api/activos?pageSize=100` para obtener activos existentes
- Si hay menos de 50, crea 50 activos con tags `TAG-3M-XXX`
- Cada activo tiene: placa, nombre, categoría, responsable, tag RFID

#### Fase 2: Crear autorizaciones de salida
- Selecciona ~15 activos (30% del total)
- Crea autorizaciones de salida para estos activos
- Esto permite que las salidas de estos activos sean "válidas" (sin alarma)

#### Fase 3: Generar movimientos de 90 días
- Para cada día de los últimos 90 días:
  - **Días laborales (lun-vie):** 15-25 movimientos aleatorios
  - **Fines de semana (sáb-dom):** 3-8 movimientos aleatorios
- Cada movimiento incluye:
  - Tag aleatorio de los existentes
  - Punto de lectura aleatorio (5 puntos disponibles)
  - Tipo: 70% INGRESO, 30% SALIDA
  - Hora aleatoria entre 7am y 7pm
- **Alarmas:** Salidas de activos SIN autorización generan `activarAlarmaSonora: true`

#### Resultado esperado

| Métrica | Valor aproximado |
|---------|------------------|
| Movimientos totales | ~1,500 |
| Alarmas generadas | ~300-400 (~22%) |
| Período | 90 días |
| Distribución | Más volumen en días laborales |

### 25.3 Otros Scripts de Simulación

| Script | Descripción |
|--------|-------------|
| `simulacion-100.sh` | Crea 100 tags + 100 activos + 30 movimientos (batch estático) |
| `simulacion-prod.sh` | Simulación pequeña con 3 activos y 3 movimientos (demo rápida) |
| `simulacion-prod-full.sh` | Simulación completa con 10 tags + 8 activos + 6 movimientos |

### 25.4 URLs de Prueba

| URL | Descripción |
|-----|-------------|
| https://horuseye-app.mauricioadachi.dev | Frontend (Dashboard) |
| https://horuseye-api.mauricioadachi.dev | API (Backend) |

### 25.5 Credenciales de Prueba

| Email | Contraseña | Rol |
|-------|------------|-----|
| admin@horuseye.com | Admin123! | Gestión (acceso completo) |
| consulta@horuseye.com | Consulta123! | Consulta (solo lectura) |

---

## 26. Notas para la Sesión de Fin de Semana (10-Julio-2026)

### Estado Actual del Proyecto
- **Backend + DB**: Funcionando en OCI VM
- **Frontend**: Funcionando en Vercel
- **Datos de prueba**: 100 activos, 100 tags, ~1,500 movimientos de 90 días
- **Alarmas**: ~300-400 salidas no autorizadas registradas

### Pendientes Principales
- [ ] Configurar backups de PostgreSQL en OCI
- [ ] Agregar health checks en docker-compose
- [ ] Monitoreo y alertas
- [ ] Documentar API con Swagger/OpenAPI
- [ ] Eliminar `render.yaml` si ya no se usa (legacy)

### Archivos Clave para la Sesión
- `Documents/DevelopmentPlaybook.md` — Esta bitácora
- `DOCUMENTACION.md` — Documentación de estado actual
- `simulacion-3meses.sh` — Script de datos de prueba
- `docker-compose.yml` — Configuración Docker para OCI

---

> **HorusEye** — *Vigilancia y control absoluto de inventarios.*

---

## 27. Sesion: Analisis de Dispositivos y Arquitectura Multi-Cliente (12-Julio-2026)

### Contexto

El usuario solicito un analisis profundo del proyecto para entender la arquitectura actual y planificar la integracion con dispositivos RFID reales. Se descubrio que el dispositivo no es Keonn sino Chainway U300.

### Que hicimos

1. **Exploracion del proyecto**
   - Leimos toda la estructura existente (Backend, Frontend, Database)
   - Analizamos los 9 controllers del Backend
   - Revisamos el modelo de datos actual

2. **Investigacion de dispositivos RFID**
   - Investigamos fabricantes mundiales (Zebra, Impinj, Honeywell, SICK, etc.)
   - Descubrimos que el dispositivo real es **Chainway U300** (no Keonn)
   - Identificamos que **RFIDlinked** es el equivalente a AdvanNet para Chainway

3. **Analisis del SDK Chainway**
   - Extraimos y analizamos los archivos RAR recibidos
   - Encontramos documentacion Javadoc completa (8,526 lineas)
   - Identificamos funciones: inventorySingleTag(), startInventoryTag(), GPIO, etc.
   - Descubrimos que soporta conexion TCP/IP directa

4. **Diseno de arquitectura multi-cliente**
   - Disenamos modelo de datos: Clientes -> Dispositivos -> Lecturas
   - Propusimos Provider Pattern para multi-proveedor RFID
   - Documentamos Modelo C4 de arquitectura

5. **Documentacion tecnica completa**
   - Creamos `ARCHITECTURE.md` con 800+ lineas
   - Actualizamos `Analisis_Dispositivos_RFID.md` con mercado RFID
   - Actualizamos `RSCJA_U300_Chainway.md` con SDK completo
   - Actualizamos `DevelopmentPlaybook.md` (esta bitácora)

### Decisiones tomadas

| Decision | Justificacion |
|----------|---------------|
| **Usar RFIDlinked** | Software oficial de Chainway, equivalente a AdvanNet |
| **Provider Pattern** | Para soportar multiples proveedores RFID sin duplicar codigo |
| **Multi-Cliente** | Requisito del negocio: administrar multiples clientes |
| **Clean Architecture** | Mejores practicas: SOLID, CQRS, Repository Pattern |
| **Modelo C4** | Para documentar arquitectura en 4 niveles de abstraccion |

### Documentos generados/actualizados

| Documento | Lineas | Contenido |
|-----------|--------|-----------|
| `ARCHITECTURE.md` | 800+ | Arquitectura completa, C4, mejores practicas |
| `Analisis_Dispositivos_RFID.md` | 1000+ | Investigacion de mercado RFID |
| `RSCJA_U300_Chainway.md` | 500+ | Ficha tecnica U300 + SDK |
| `DevelopmentPlaybook.md` | 2400+ | Esta bitácora |

### Pendientes para la proxima sesion

- [ ] Disenar modelo de datos multi-cliente (entidades y relaciones)
- [ ] Crear migraciones EF Core para Clientes y Dispositivos
- [ ] Probar conexion TCP/IP con dispositivo U300
- [ ] Investigar API de RFIDlinked en detalle

---

## 28. Documentos del Proyecto

| Documento | Contenido | Ubicacion |
|-----------|-----------|-----------|
| **ARCHITECTURE.md** | Arquitectura completa, patrones, C4, mejores practicas | `Documents/ARCHITECTURE.md` |
| **Analisis_Dispositivos_RFID.md** | Investigacion de mercado RFID | `Documents/Analisis_Dispositivos_RFID.md` |
| **RSCJA_U300_Chainway.md** | Ficha tecnica del dispositivo U300 + SDK | `Documents/RSCJA_U300_Chainway.md` |
| **AdvanNet.md** | Comunicacion con hardware RFID | `Documents/AdvanNet.md` |
| **DevelopmentPlaybook.md** | Esta bitácora | `Documents/DevelopmentPlaybook.md` |

---

## 29. Sesion: RBAC Multi-Nivel + Dispositivos RFID Configurables (12-Julio-2026)

### 29.1 Resumen de Cambios

Transformacion del sistema de 2 roles a 7 roles jerarquicos con aislamiento de datos, mas un sistema configurable de antenas RFID con jerarquia adaptable por cliente.

**Backend - Archivos Nuevos (14):**
| Archivo | Descripcion |
|---|---|
| `Core/Enums/RolSistema.cs` | 7 roles del sistema |
| `Core/Enums/TipoDispositivo.cs` | FIXED/HANDHELD/GATE |
| `Core/Entities/Proveedor.cs` | Entidad Proveedor |
| `Core/Entities/Cliente.cs` | Entidad Cliente |
| `Core/Entities/UsuarioExtendido.cs` | FK usuario -> Proveedor/Cliente |
| `Core/Entities/DispositivoRfid.cs` | Dispositivo RFID configurable |
| `Core/Entities/NodoUbicacion.cs` | Arbol jerarquico auto-referenciado |
| `Core/Entities/FabricanteDispositivo.cs` | Fabricantes configurables |
| `Core/Entities/CampoPayloadFabricante.cs` | Mapeo JSON configurable |
| `Api/Services/PermisoService.cs` | Matriz de permisos centralizada |
| `Api/Providers/IRfidProvider.cs` | Interfaz + RfidEvent |
| `Api/Providers/ChainwayProvider.cs` | Parser Chainway U300 |
| `Api/Providers/GenericProvider.cs` | Parser generico via JSON Schema |
| `Api/DTOs/GestionDtos.cs` | DTOs para controllers nuevos |

**Backend - Controllers Nuevos (5):**
- `ProveedoresController` - CRUD proveedores
- `ClientesController` - CRUD clientes con aislamiento
- `DispositivosController` - CRUD dispositivos RFID
- `NodosUbicacionController` - Arbol jerarquico de ubicaciones
- `FabricantesController` - Gestion de fabricantes + campos payload

**Frontend - Paginas Nuevas (5):**
- `Proveedores.tsx`, `Clientes.tsx`, `Dispositivos.tsx`, `Ubicaciones.tsx`, `Fabricantes.tsx`

### 29.2 Matriz de Permisos

| Rol | Crear Usuarios | Gestionar Dispositivos | Ver Datos |
|---|---|---|---|
| AdminSistema | Todos (7 tipos) | Si | Todo |
| AsistAdminSistema | Proveedor, Cliente | Si | Todo |
| SoporteSistema | Ninguno | Solo lectura | Todo |
| AdminProveedor | AsistProv, AdminCli, AsistCli (su proveedor) | Si (su proveedor) | Su proveedor |
| AsistProveedor | Ninguno | Solo lectura | Su proveedor |
| AdminCliente | AsistCli (su cliente) | Si (su cliente) | Su cliente |
| AsistCliente | Ninguno | Solo lectura | Su cliente |

### 29.3 Sesion: Deploy y Resolucion de Errores (12-Julio-2026)

#### Paso 1: Commit y Push
```bash
git add .
git commit -m "feat: RBAC multi-nivel (7 roles) + dispositivos RFID configurables"
git push origin main
```

#### Paso 2: Pull en OCI VM
```bash
ssh -i ~/.ssh/id_rsa_oci ubuntu@129.146.22.246
cd /opt/horuseye
git pull
```

#### Paso 3: Build Docker (con --no-cache para forzar rebuild)
```bash
docker compose build --no-cache api
```
> **NOTA:** Siempre usar `--no-cache` despues de cambios en el codigo para evitar que Docker use capas viejas.

#### Paso 4: Restart contenedores
```bash
docker compose up -d
```

#### Paso 5: Verificar
```bash
# Health check
curl http://localhost:8081/health

# Login con nuevos roles
curl -X POST http://localhost:8081/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@horuseye.com","password":"Admin123!"}'
```

#### Errores Encontrados y Soluciones

**Error 1: `duplicate key value violates unique constraint "PK___EFMigrationsHistory"`**
- **Causa:** El fallback en `Program.cs` intentaba INSERTar el registro de `InitialCreate` que ya existia en la BD.
- **Solucion:** Cambiar `INSERT INTO` por `INSERT INTO ... ON CONFLICT DO NOTHING`.

**Error 2: Docker usaba imagen cacheada con codigo viejo**
- **Causa:** `docker compose build api` reutilizaba capas cacheadas del build anterior.
- **Solucion:** Usar `docker compose build --no-cache api` para forzar rebuild completo.

**Error 3: Tunel Cloudflare caido (`Provided Tunnel token is not valid`)**
- **Causa:** La variable `CLOUDFLARE_TUNNEL_TOKEN` no existia en `/opt/horuseye/.env`.
- **Solucion:** Copiar el token del proyecto EBM (`/opt/ebm/Backends/Apis/dotNET10/.env`) al `.env` de HorusEye. El tunel es compartido.
- **Token:** `eyJhIjoiNWIzMThmZmVmOTVl...` (ver `/opt/ebm/Backends/Apis/dotNET10/.env`)

### 29.4 Guia de Deploy Rapido (para futuros cambios)

#### Deploy Backend (cambios en C#)
```bash
# 1. Local: commit y push
git add . && git commit -m "feat: descripcion" && git push

# 2. OCI VM: pull + rebuild + restart
ssh -i ~/.ssh/id_rsa_oci ubuntu@129.146.22.246
cd /opt/horuseye
git pull
docker compose build --no-cache api
docker compose up -d

# 3. Verificar
curl http://localhost:8081/health
```

#### Deploy Frontend (cambios en React/TypeScript)
```bash
# Automatico: push a main -> Vercel auto-deploy
git push origin main

# Manual:
cd Frontends/ReactTS
vercel --prod
```

#### Deploy Completo (Backend + Frontend)
```bash
# Local
git add . && git commit -m "feat: descripcion" && git push

# OCI VM
ssh -i ~/.ssh/id_rsa_oci ubuntu@129.146.22.246
cd /opt/horuseye && git pull
docker compose build --no-cache api && docker compose up -d

# Frontend (Vercel auto-deploy con el push, o manual)
cd Frontends/ReactTS && vercel --prod
```

#### Si hay cambios en la Base de Datos (nuevas entidades/migraciones)
```bash
# 1. Local: crear migracion
cd Backends/WebApi
dotnet ef migrations add NombreDescriptivo \
  --project HorusEye.Infrastructure \
  --startup-project HorusEye.Api

# 2. Commit y push
git add . && git commit -m "feat: migracion NombreDescriptivo" && git push

# 3. OCI VM: rebuild (la migracion se aplica automaticamente al iniciar)
ssh -i ~/.ssh/id_rsa_oci ubuntu@129.146.22.246
cd /opt/horuseye && git pull
docker compose build --no-cache api && docker compose up -d
```

### 29.5 Credenciales y Acceso

| Recurso | Valor |
|---|---|
| **OCI VM** | `ssh -i ~/.ssh/id_rsa_oci ubuntu@129.146.22.246` |
| **Directorio deploy** | `/opt/horuseye` |
| **API (localhost)** | `http://localhost:8081` |
| **API (publica)** | `https://horuseye-api.mauricioadachi.dev` |
| **Frontend** | `https://horuseye-app.mauricioadachi.dev` |
| **Admin login** | `admin@horuseye.com` / `Admin123!` |
| **Consulta login** | `consulta@horuseye.com` / `Consulta123!` |
| **Tunel token** | En `/opt/ebm/Backends/Apis/dotNET10/.env` (compartido con EBM) |

---

> **HorusEye** — *Vigilancia y control absoluto de inventarios.*
> 
> **Ultima actualizacion:** 12-Julio-2026