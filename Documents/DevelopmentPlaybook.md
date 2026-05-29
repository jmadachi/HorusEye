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
| `deploy` | Desplegar solo en push a main (después de que ambos jobs pasen) | Render deploy hook + Vercel CLI |

**Secrets requeridos en GitHub:**

| Secret | Propósito | Cómo obtenerlo |
|--------|-----------|---------------|
| `RENDER_DEPLOY_HOOK` | Trigger de deploy en Render | Render Dashboard → Web Service → Deploy Hook |
| `RENDER_SERVICE_ID` | ID del servicio en Render | Render Dashboard → URL del servicio |
| `VERCEL_ORG_ID` | ID de la organización en Vercel | `vercel whoami --scope` |
| `VERCEL_PROJECT_ID` | ID del proyecto en Vercel | `vercel link` → ver `.vercel/project.json` |
| `VERCEL_TOKEN` | Token de API de Vercel | Vercel Dashboard → Settings → Tokens |

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
| `healthCheckPath` | No existía | `/api/auth/login` — Render monitorea la salud del servicio |
| `autoDeploy` | No existía | `true` — despliegue automático en cada push a main |
| `branch` | No existía | `main` — explícito sobre qué rama desplegar |
| Connection string | Hardcodeada en el YAML | `fromDatabase` + `property: connectionString` — Render inyecta automáticamente |

El `healthCheckPath` permite que Render detecte si la API está funcionando correctamente. Si la ruta responde HTTP 200, el servicio se considera saludable. Caso contrario, Render reinicia el contenedor automáticamente.

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

> **HorusEye** — *Vigilancia y control absoluto de inventarios.*
