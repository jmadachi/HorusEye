# HorusEye — Documento de Arquitectura

> **Estado:** Activo
> **Ultima actualizacion:** 12-Julio-2026
> **Objetivo:** Documentar la arquitectura actual y las decisiones de diseno para soporte multi-proveedor RFID

---

## 1. Arquitectura Actual (Clean Architecture)

### 1.1 Estructura de Capas

```
┌─────────────────────────────────────────────────────────────┐
│                    HORUSEYE - CLEAN ARCHITECTURE             │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  CAPA 1: PRESENTATION (HorusEye.Api)                 │   │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐   │   │
│  │  │ Controllers │ │    Hubs     │ │   DTOs      │   │   │
│  │  │  (REST API) │ │ (SignalR)   │ │(Data Trans) │   │   │
│  │  └─────────────┘ └─────────────┘ └─────────────┘   │   │
│  │  ┌─────────────┐ ┌─────────────┐                   │   │
│  │  │  Services   │ │ Middleware  │                   │   │
│  │  │ (Token JWT) │ │(Exceptions)│                   │   │
│  │  └─────────────┘ └─────────────┘                   │   │
│  └─────────────────────────────────────────────────────┘   │
│                              │                               │
│                              ▼                               │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  CAPA 2: DOMAIN (HorusEye.Core)                      │   │
│  │  ┌─────────────────────┐ ┌─────────────────────┐   │   │
│  │  │      Entities       │ │       Enums         │   │   │
│  │  │  (Tag, Activo,      │ │  (EstadoTag,        │   │   │
│  │  │   Movimiento, etc.) │ │   TipoMovimiento)   │   │   │
│  │  └─────────────────────┘ └─────────────────────┘   │   │
│  └─────────────────────────────────────────────────────┘   │
│                              │                               │
│                              ▼                               │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  CAPA 3: INFRASTRUCTURE (HorusEye.Infrastructure)    │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │              HorusEyeDbContext               │   │   │
│  │  │         (EF Core + PostgreSQL + Identity)     │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │  ┌─────────────────────┐                           │   │
│  │  │    Migrations       │                           │   │
│  │  └─────────────────────┘                           │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

### 1.2 Directorios del Proyecto

```
HorusEye/
├── Backends/
│   └── WebApi/
│       ├── HorusEye.slnx                    # Solucion
│       ├── HorusEye.Api/                    # Presentacion
│       │   ├── Controllers/                 # 9 endpoints REST
│       │   ├── Hubs/                        # SignalR Hub
│       │   ├── DTOs/                        # Data Transfer Objects
│       │   ├── Services/                    # TokenService (JWT)
│       │   ├── Middleware/                  # ExceptionMiddleware
│       │   └── Models/                      # ApiResponse<T>
│       ├── HorusEye.Core/                   # Dominio
│       │   ├── Entities/                    # 6 entidades
│       │   └── Enums/                       # 3 enums
│       ├── HorusEye.Infrastructure/         # Acceso a datos
│       │   ├── Data/                        # DbContext
│       │   └── Migrations/                  # Migraciones EF
│       └── HorusEye.Tests/                  # Tests
│
├── Frontends/
│   └── ReactTS/
│       └── src/
│           ├── pages/                       # 7 paginas
│           ├── components/                  # Layout, ProtectedRoute
│           ├── context/                     # AuthContext, ThemeContext
│           ├── services/                    # api.ts, signalR.ts
│           └── hooks/                       # useSignalR
│
└── Documents/                               # Documentacion
```

---

## 2. Arquitectura de Red

```
┌─────────────────────────────────────────────────────────────┐
│                    ARQUITECTURA DE RED                        │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  [Usuario]                                                    │
│      │                                                        │
│      │ HTTPS                                                  │
│      ▼                                                        │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  VERCEL (Frontend)                                    │   │
│  │  https://horuseye.mauricioadachi.dev              │   │
│  │  React 19 + TypeScript + Vite                         │   │
│  └─────────────────────────────────────────────────────┘   │
│      │                                                        │
│      │ HTTP/HTTPS (API calls)                                │
│      ▼                                                        │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  CLOUDFLARE TUNNEL                                    │   │
│  │  horuseye-api.mauricioadachi.dev                      │   │
│  └─────────────────────────────────────────────────────┘   │
│      │                                                        │
│      │ HTTP (interno)                                         │
│      ▼                                                        │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  OCI VM (129.146.22.246)                              │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │  Docker: horuseye-api (.NET 10)               │   │   │
│  │  │  Puerto: 8081 → 8080 (interno)                │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │  Docker: horuseye-db (PostgreSQL 18)         │   │   │
│  │  │  Puerto: 5433 → 5432 (interno)               │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

---

## 3. Flujo de Datos RFID (Actual)

### 3.1 Flujo Keonn/AdvanNet (Implementado)

```
┌─────────────────────────────────────────────────────────────┐
│                    FLUJO RFID KEONN                           │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  [Lector Keonn]                                               │
│      │                                                        │
│      │ HTTP POST (Webhook)                                    │
│      │ URL: /api/eventos-rfid                                 │
│      │ Body: { TagId, TipoMovimiento, PuntoLecturaId }       │
│      ▼                                                        │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  EventosRfidController                               │   │
│  │  • De-bounce (5s)                                    │   │
│  │  • Validar tag existe                                │   │
│  │  • Verificar autorizacion                            │   │
│  │  • Crear Movimiento                                  │   │
│  │  • Broadcast SignalR                                 │   │
│  └─────────────────────────────────────────────────────┘   │
│      │                                                        │
│      ▼                                                        │
│  [Dashboard React - Tiempo Real]                              │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

### 3.2 Flujo Chainway/U300 (Propuesto)

```
┌─────────────────────────────────────────────────────────────┐
│                    FLUJO RFID CHAINWAY                        │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  [Lector U300]                                                │
│      │                                                        │
│      │ UART (interno)                                         │
│      ▼                                                        │
│  [App Android Gateway]                                        │
│      │  • Lee tags via SDK Chainway                           │
│      │  • Envia datos al servidor                             │
│      │                                                        │
│      │ HTTP POST                                              │
│      │ URL: /api/eventos-rfid/chainway                        │
│      │ Body: { EPC, Antenna, RSSI, Timestamp }               │
│      ▼                                                        │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  EventosRfidController (endpoint nuevo)              │   │
│  │  • Mapear EPC → TagId                                │   │
│  │  • Mapear Antena → PuntoLecturaId                    │   │
│  │  • Inferir TipoMovimiento                            │   │
│  │  • Reusar logica de negocio existente                │   │
│  └─────────────────────────────────────────────────────┘   │
│      │                                                        │
│      ▼                                                        │
│  [Dashboard React - Tiempo Real]                              │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

---

## 4. Decision de Arquitectura: Multi-Proveedor RFID

### 4.1 El Problema

Tenemos dos proveedores con formatos diferentes:

| Proveedor | Formato | Endpoint |
|-----------|---------|----------|
| **Keonn** | `{ TagId, TipoMovimiento, PuntoLecturaId }` | `/api/eventos-rfid` |
| **Chainway** | `{ EPC, Antenna, RSSI, Timestamp }` | `/api/eventos-rfid/chainway` |

### 4.2 Opciones Evaluadas

| Opcion | Descripcion | Pros | Contras |
|--------|-------------|------|---------|
| **A. Simple** | Endpoint por proveedor | Facil, rapido | Duplica logica |
| **B. Provider Pattern** | Interfaz + factory | Limpio, extensible | Mas codigo |
| **C. Plugin System** | Carga dinamica | Maxima flexibilidad | Complejo |

### 4.3 Decision: Provider Pattern (Opcion B)

**Justificacion:**
- Tenemos 2 proveedores actuales, potencialmente 3-5 mas
- No necesitamos carga dinamica en runtime
- El Provider Pattern es suficiente y mas mantenible
- Facil de testear cada proveedor por separado

---

## 5. Arquitectura Propuesta: RFID Provider Pattern

### 5.1 Diagrama de Componentes

```
┌─────────────────────────────────────────────────────────────┐
│                    RFID PROVIDER PATTERN                      │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌─────────────────────────────────────────────────────┐   │
│  │           CAPA DE ABSTRACCIÓN                        │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │         IRfidProvider (Interfaz)             │   │   │
│  │  │                                              │   │   │
│  │  │  + ProviderName: string                      │   │   │
│  │  │  + ParseEventAsync(): RfidEvent              │   │   │
│  │  │  + ValidatePayload(): bool                   │   │   │
│  │  │  + GetDeviceMetadata(): Dict                 │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  └─────────────────────────────────────────────────────┘   │
│                              │                               │
│         ┌────────────────────┼────────────────────┐         │
│         ▼                    ▼                    ▼         │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐   │
│  │   Keonn     │    │  Chainway   │    │   Zebra     │   │
│  │  Provider   │    │  Provider   │    │  Provider   │   │
│  │             │    │             │    │ (futuro)    │   │
│  └─────────────┘    └─────────────┘    └─────────────┘   │
│         │                    │                    │         │
│         └────────────────────┼────────────────────┘         │
│                              ▼                               │
│  ┌─────────────────────────────────────────────────────┐   │
│  │           MODELO NORMALIZADO                         │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │              RfidEvent                       │   │   │
│  │  │                                              │   │   │
│  │  │  + ProviderName: string                      │   │   │
│  │  │  + DeviceId: string                          │   │   │
│  │  │  + EPC: string                               │   │   │
│  │  │  + Antenna: int                              │   │   │
│  │  │  + RSSI: int                                 │   │   │
│  │  │  + Timestamp: DateTime                       │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  └─────────────────────────────────────────────────────┘   │
│                              │                               │
│                              ▼                               │
│  ┌─────────────────────────────────────────────────────┐   │
│  │           LOGICA DE NEGOCIO                          │   │
│  │  (Sin cambios - reutilizable)                        │   │
│  │                                                      │   │
│  │  • De-bounce                                         │   │
│  │  • Validar tag                                       │   │
│  │  • Verificar autorizacion                            │   │
│  │  • Crear movimiento                                  │   │
│  │  • Broadcast SignalR                                 │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

### 5.2 Interfaz IRfidProvider

```csharp
namespace HorusEye.Api.Providers;

public interface IRfidProvider
{
    string ProviderName { get; }
    string[] SupportedContentTypes { get; }
    Task<RfidEvent> ParseEventAsync(HttpRequest request);
    bool ValidatePayload(string payload);
    Dictionary<string, string> GetDeviceMetadata(HttpRequest request);
}
```

### 5.3 Modelo RfidEvent

```csharp
namespace HorusEye.Api.Providers;

public class RfidEvent
{
    public string ProviderName { get; set; }
    public string DeviceId { get; set; }
    public string EPC { get; set; }
    public string? TID { get; set; }
    public int Antenna { get; set; }
    public int RSSI { get; set; }
    public int ReadCount { get; set; }
    public string? PuntoLectura { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string> RawData { get; set; }
}
```

### 5.3 Endpoints Resultantes

| Endpoint | Metodo | Proveedor | Descripcion |
|----------|--------|-----------|-------------|
| `/api/eventos-rfid` | POST | Generico | Auto-detecta proveedor |
| `/api/eventos-rfid/keonn` | POST | Keonn | Compatibilidad existente |
| `/api/eventos-rfid/chainway` | POST | Chainway | Nuevo |

---

## 6. Entidades del Modelo de Datos

### 6.1 Entidades Existentes

| Entidad | Descripcion | Estado |
|---------|-------------|--------|
| `Tag` | Tag RFID con estado | ✅ Implementada |
| `Activo` | Activo/equipo inventariado | ✅ Implementada |
| `Movimiento` | Registro de entrada/salida | ✅ Implementada |
| `AutorizacionSalida` | Autorizacion para sacar activo | ✅ Implementada |
| `RefreshToken` | Token JWT de refresco | ✅ Implementada |

### 6.2 Entidad Propuesta

| Entidad | Descripcion | Estado |
|---------|-------------|--------|
| `LectorRfid` | Dispositivo lector registrado | ❌ Por crear |

```csharp
public class LectorRfid
{
    public Guid Id { get; set; }
    public string Nombre { get; set; }      // "Puerta Principal"
    public string Modelo { get; set; }      // "U300"
    public string Fabricante { get; set; }  // "Chainway"
    public string? DireccionIP { get; set; }
    public string? Ubicacion { get; set; }  // "Bodega 1"
    public bool Activo { get; set; }
    public DateTimeOffset FechaRegistro { get; set; }
}
```

---

## 7. Stack Tecnologico

| Capa | Tecnologia | Version |
|------|------------|---------|
| **Backend** | .NET 10 (C#) | 10.0 |
| **ORM** | Entity Framework Core | via .NET 10 |
| **Real-time** | SignalR | 10.0 |
| **Auth** | ASP.NET Identity + JWT | - |
| **Database** | PostgreSQL | 18 |
| **Frontend** | React + TypeScript | 19.2 / 6.0 |
| **Bundler** | Vite | 8.0 |
| **CSS** | Tailwind CSS | 4.3 |
| **Container** | Docker | - |
| **Cloud** | Oracle Cloud (VM) + Vercel | - |

---

## 8. Proveedores RFID Soportados

### 8.1 Estado Actual

| Proveedor | Dispositivo | SDK | Estado |
|-----------|-------------|-----|--------|
| **Keonn** | AdvanReader | AdvanNet | ✅ Implementado |
| **Chainway** | U300 | com.rscja.deviceapi | 🔄 En integracion |
| **Zebra** | FX9600 | RFID API | ❌ Futuro |
| **Impinj** | Speedway | Octane SDK | ❌ Futuro |

### 8.2 Cadena de Valor

```
Chip (Impinj) → Hardware (Chainway) → SDK (RSCJA) → Gateway (Android) → API (.NET)
```

---

## 9. Seguridad

### 9.1 Autenticacion

- **JWT**: Issuer=HorusEyeAPI, Audience=HorusEyeFrontend
- **Roles**: "Usuario de Consulta", "Usuario de Gestion"
- **Refresh Tokens**: Persistidos en BD

### 9.2 Endpoints RFID

- **Keonn**: AllowAnonymous (webhook del hardware)
- **Chainway**: AllowAnonymous (del Android Gateway)
- **Futuro**: Considerar API key por dispositivo

---

## 10. Proximos Pasos

### Fase 1: Documentacion (Actual)
- [x] Documentar arquitectura actual
- [x] Documentar decision de Provider Pattern
- [ ] Revisar y aprobar documento

### Fase 2: Refactorizar Backend
- [ ] Crear interfaz IRfidProvider
- [ ] Crear KeonnProvider
- [ ] Crear ChainwayProvider
- [ ] Actualizar controller

### Fase 3: Gateway Android
- [ ] Crear proyecto Android
- [ ] Integrar SDK Chainway
- [ ] Implementar envio HTTP

### Fase 4: Frontend
- [ ] Pagina de gestion de lectores
- [ ] Estado de dispositivos en Dashboard

---

> **HorusEye** — *Vigilancia y control absoluto de inventarios.*

---

## 11. ARQUITECTURA COMPLETA (Segun Documento de Especificacion Tecnica)

> **Fuente:** Documento de Especificacion Tecnica - Sistema RFID de Borde Independiente
> **Decisiones ya tomadas:** Rust (Edge) + C# (Cloud) + React (Frontend)

### 11.1 Arquitectura de 3 Capas

```
┌─────────────────────────────────────────────────────────────┐
│           ARQUITECTURA COMPLETA - 3 CAPAS                    │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  CAPA 1: BORDE EXTREMO (Edge Computing)                      │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  DISPOSITIVO KEONN (ARM, 512 MB RAM)                 │   │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐ │   │
│  │  │   Antenas   │  │  AdvanNet   │  │  API Rust   │ │   │
│  │  │  Fisicas    │  │  (Firmware) │  │  (Nuestra)  │ │   │
│  │  └─────────────┘  └─────────────┘  └─────────────┘ │   │
│  │                                                      │   │
│  │  Tecnologia: RUST (seleccionado: 94.25 pts)          │   │
│  │  Funcion: Recibe lecturas, filtra, envia a cloud     │   │
│  └─────────────────────────────────────────────────────┘   │
│                          │                                    │
│                          │ HTTP POST (JSON optimizado)        │
│                          │ WAN/LAN                            │
│                          ▼                                    │
│  CAPA 2: NUBE (Cloud / Servidor Central)                     │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  API DE NEGOCIOS (.NET 10)                           │   │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐ │   │
│  │  │  Persistencia│  │  WebSockets │  │  Sockets    │ │   │
│  │  │  (EF Core)   │  │  (SignalR)  │  │  TCP/IP     │ │   │
│  │  │  PostgreSQL  │  │  Pantallas  │  │  Perifericos│ │   │
│  │  └─────────────┘  └─────────────┘  └─────────────┘ │   │
│  │                                                      │   │
│  │  Tecnologia: C# / .NET 10 (seleccionado: 96.25 pts) │   │
│  │  Funcion: Reglas de negocio, persistencia, distribute│   │
│  └─────────────────────────────────────────────────────┘   │
│                          │                                    │
│                          │ WebSockets (SignalR)               │
│                          ▼                                    │
│  CAPA 3: FRONTEND (Consola de Control)                       │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  DASHBOARD EN TIEMPO REAL                            │   │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐ │   │
│  │  │   React     │  │  Graficos   │  │  Alertas    │ │   │
│  │  │  (Virtual   │  │  (Recharts) │  │  en Vivo    │ │   │
│  │  │   DOM)      │  │             │  │             │ │   │
│  │  └─────────────┘  └─────────────┘  └─────────────┘ │   │
│  │                                                      │   │
│  │  Tecnologia: React + TypeScript + Vite               │   │
│  │  Funcion: Visualizacion, alertas, reportes           │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

### 11.2 Flujo de Datos Completo

```
[Producto con tag RFID]
         │
         ▼
[Antenas Fisicas] → Señal RF
         │
         ▼
[AdvanNet] → Filtra y empaqueta
         │
         ▼
[API Rust (Borde)] → Recibe localhost, limpia, optimiza
         │
         │ HTTP POST (JSON optimizado)
         ▼
[API C# (Cloud)] → Valida, persiste, distribuye
         │
         ├──────────────────────────────────────┐
         ▼                    ▼                 ▼
[PostgreSQL]          [SignalR Hub]        [Sockets TCP]
 (Datos)              (Pantallas)         (Perifericos)
                           │
                           ▼
                    [Dashboard React]
                     (Tiempo Real)
```

### 11.3 Por que Rust para el Borde?

| Criterio | Rust | Alternativas |
|----------|------|--------------|
| **RAM** | <10 MB | Go: 25 MB, Python: 80 MB |
| **Latencia** | Microsegundos | Milisegundos |
| **Memoria** | Sin GC (Garbage Collector) | GC puede pausar |
| **Concurrencia** | Hilos nativos | Goroutines/Async |
| **Seguridad** | Ownership system | Manual en C++ |

**Conclusión del documento:** Rust se consolida como la decisión arquitectónica definitiva (94.25 pts).

### 11.4 Por que C# para la Nube?

| Criterio | C#/.NET 10 | Alternativas |
|----------|------------|--------------|
| **Persistencia** | EF Core (nativo) | Requiere librerías extra |
| **WebSockets** | SignalR (nativo) | Requiere configuración |
| **Rendimiento** | Kestrel (clase mundial) | Comparable |
| **Ecosistema** | Maduro, empresarial | Menos herramientas |
| **IA** | Excelente asistencia | Variable |

**Conclusión del documento:** C# consolida una arquitectura híbrida de alto rendimiento (96.25 pts).

---

## 12. INTEGRACION CON DISPOSITIVO ACTUAL (U300 Chainway)

### 12.1 Diferencia con el Documento Original

| Aspecto | Documento Original | Realidad Actual |
|---------|-------------------|-----------------|
| **Dispositivo** | Keonn AdvanReader | Chainway U300 |
| **Edge API** | Rust (corriendo en lector) | No aplica (Android) |
| **Conexion** | localhost HTTP POST | UART → Android → HTTP |
| **SDK** | AdvanNet | com.rscja.deviceapi |

### 12.2 Arquitectura Adaptada para U300

```
┌─────────────────────────────────────────────────────────────┐
│           ARQUITECTURA ADAPTADA - U300 CHAINWAY              │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  CAPA 1: BORDE (U300 + Android)                               │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  DISPOSITIVO U300 (Android 11, Chip Impinj E710)    │   │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐ │   │
│  │  │   Antena    │  │  SDK        │  │  App        │ │   │
│  │  │    UHF      │  │  Chainway   │  │  Gateway    │ │   │
│  │  │  (SMA)      │  │  (UART)     │  │  (Android)  │ │   │
│  │  └─────────────┘  └─────────────┘  └─────────────┘ │   │
│  │                                                      │   │
│  │  Tecnologia: Android + SDK Chainway                  │   │
│  │  Funcion: Recibe tags, envia a cloud via HTTP        │   │
│  └─────────────────────────────────────────────────────┘   │
│                          │                                    │
│                          │ HTTP POST                          │
│                          │ (Android Gateway)                  │
│                          ▼                                    │
│  CAPA 2: NUBE (Igual que documento original)                │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  API DE NEGOCIOS (.NET 10)                           │   │
│  │  + Provider Pattern para multi-proveedor             │   │
│  └─────────────────────────────────────────────────────┘   │
│                          │                                    │
│                          ▼                                    │
│  CAPA 3: FRONTEND (Igual que documento original)            │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  DASHBOARD REACT                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

### 12.3 Nota sobre el Dispositivo U300

El U300 **NO es un lector Keonn** - es un dispositivo Chainway con:
- **OS:** Android 11 (no Linux embebido)
- **SDK:** com.rscja.deviceapi (no AdvanNet)
- **Conexion:** UART interno (no localhost HTTP)
- **Gateway:** App Android (no API Rust)

**Por eso necesitamos el Provider Pattern** para soportar ambos formatos.

---

## 13. RESUMEN DE DECISIONES ARQUITECTONICAS

| Decision | Estado | Fuente |
|----------|--------|--------|
| **Clean Architecture** | ✅ Implementada | Proyecto actual |
| **Edge: Rust** | 📄 Documentado | Documento de Especificacion |
| **Cloud: C# .NET 10** | ✅ Implementada | Documento de Especificacion |
| **Frontend: React** | ✅ Implementada | Documento de Especificacion |
| **Multi-proveedor RFID** | 📄 Diseñado | Provider Pattern (nuevo) |
| **Dispositivo: U300** | 🔄 En integracion | Documento Tecnico |


---

## 14. REQUISITOS NUEVOS: Multi-Cliente y Multi-Dispositivo

### 14.1 Requisitos del Sistema

| Requisito | Descripcion |
|-----------|-------------|
| **Administrar Clientes** | CRUD de clientes del sistema |
| **Asociar dispositivos** | Cada cliente tiene sus dispositivos RFID |
| **Configurar endpoints** | Definir URLs/APIs de comunicacion con dispositivos |

### 14.2 Modelo de Datos Propuesto

```
┌─────────────────────────────────────────────────────────────┐
│                    MODELO DE DATOS                            │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  CLIENTE                                                      │
│  ├── Id (Guid)                                               │
│  ├── Nombre (string)                                         │
│  ├── RazonSocial (string)                                    │
│  ├── RUC/NIT (string)                                        │
│  ├── Direccion (string)                                      │
│  ├── Telefono (string)                                       │
│  ├── Email (string)                                          │
│  ├── Activo (bool)                                           │
│  └── FechaRegistro (DateTimeOffset)                          │
│                                                               │
│  DISPOSITIVO_RFID                                             │
│  ├── Id (Guid)                                               │
│  ├── ClienteId (Guid FK) ──────► CLIENTE                    │
│  ├── Nombre (string)          "Puerta Principal"             │
│  ├── Modelo (string)          "U300"                         │
│  ├── Fabricante (string)      "Chainway"                     │
│  ├── DireccionIP (string)     "192.168.1.100"               │
│  ├── Puerto (int)             5000                           │
│  ├── Ubicacion (string)      "Bodega 1 - Entrada"           │
│  ├── TipoDispositivo (enum)  FIXED/HANDHELD/GATE            │
│  ├── EndpointAPI (string)     URL del webhook                │
│  ├── Activo (bool)                                           │
│  └── FechaRegistro (DateTimeOffset)                          │
│                                                               │
│  LECTURA_RFID (opcional - historico)                         │
│  ├── Id (long)                                              │
│  ├── DispositivoId (Guid FK) ──► DISPOSITIVO_RFID           │
│  ├── ClienteId (Guid FK) ──────► CLIENTE                    │
│  ├── EPC (string)                                            │
│  ├── RSSI (int)                                              │
│  ├── Antena (int)                                            │
│  ├── Timestamp (DateTimeOffset)                              │
│  └── Procesado (bool)                                        │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

### 14.3 Endpoints Necesarios

| Endpoint | Metodo | Descripcion |
|----------|--------|-------------|
| `/api/clientes` | GET | Listar clientes |
| `/api/clientes` | POST | Crear cliente |
| `/api/clientes/{id}` | GET | Obtener cliente |
| `/api/clientes/{id}` | PUT | Actualizar cliente |
| `/api/clientes/{id}` | DELETE | Eliminar cliente |
| `/api/clientes/{id}/dispositivos` | GET | Listar dispositivos del cliente |
| `/api/dispositivos` | GET | Listar todos los dispositivos |
| `/api/dispositivos` | POST | Registrar dispositivo |
| `/api/dispositivos/{id}` | PUT | Actualizar dispositivo |
| `/api/dispositivos/{id}` | DELETE | Eliminar dispositivo |
| `/api/dispositivos/{id}/config` | PUT | Configurar endpoint API |
| `/api/eventos-rfid` | POST | Recibir eventos de cualquier dispositivo |

### 14.4 Configuracion de Endpoints

Cada dispositivo tiene un **endpoint API** que define como se comunica con el servidor:

```json
{
  "dispositivoId": "uuid-del-dispositivo",
  "endpointAPI": "https://horuseye-api.mauricioadachi.dev/api/eventos-rfid/chainway",
  "metodo": "POST",
  "headers": {
    "Authorization": "Bearer {token}",
    "Content-Type": "application/json"
  },
  "activo": true
}
```

### 14.5 Flujo de Comunicacion

```
┌─────────────────────────────────────────────────────────────┐
│                    FLUJO DE COMUNICACION                      │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  1. Dispositivo detecta tag RFID                             │
│     └── U300 lee EPC via SDK                                 │
│                                                               │
│  2. Dispositivo envia evento                                  │
│     └── POST al endpoint configurado                         │
│     └── Ejemplo: /api/eventos-rfid/chainway                  │
│                                                               │
│  3. HorusEye API recibe evento                               │
│     └── Valida dispositivo existe                            │
│     └── Valida cliente activo                                │
│     └── Procesa segun configuracion                          │
│                                                               │
│  4. HorusEye API procesa                                     │
│     └── Registra movimiento                                  │
│     └── Actualiza inventario                                 │
│     └── Notifica dashboard                                   │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

### 14.6 Arquitectura Multi-Cliente

```
┌─────────────────────────────────────────────────────────────┐
│                    ARQUITECTURA MULTI-CLIENTE                 │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  CLIENTE A                                                    │
│  ├── Dispositivo 1 (U300 - Puerta 1)                        │
│  ├── Dispositivo 2 (U300 - Puerta 2)                        │
│  └── Dispositivo 3 (U300 - Muelle)                          │
│                                                               │
│  CLIENTE B                                                    │
│  ├── Dispositivo 1 (U300 - Entrada)                         │
│  └── Dispositivo 2 (U300 - Salida)                          │
│                                                               │
│  CLIENTE C                                                    │
│  └── Dispositivo 1 (URA4 - Bodega)                          │
│                                                               │
│  TODOS ENVIAN A:                                              │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  HorusEye API (.NET 10)                              │   │
│  │  ├── Valida por ClienteId                             │   │
│  │  ├── Procesa por DispositivoId                       │   │
│  │  └── Aisla datos por cliente                         │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```


---

## 15. MEJORES PRACTICAS Y PRINCIPIOS DE ARQUITECTURA

### 15.1 Principios SOLID

| Principio | Descripcion | Aplicacion en HorusEye |
|-----------|-------------|------------------------|
| **S** - Single Responsibility | Cada clase tiene una sola responsabilidad | Controllers solo reciben peticiones, Services solo logica, Repositories solo acceso a datos |
| **O** - Open/Closed | Abierto a extension, cerrado a modificacion | Provider Pattern para agregar nuevos dispositivos sin modificar codigo existente |
| **L** - Liskov Substitution | Las subtipos deben ser sustituibles | IRfidProvider permite intercambiar Keonn/Chainway/Zebra |
| **I** - Interface Segregation | Interfaces pequenas y especificas | Separar IReadRepository, IWriteRepository en vez de IRepository grande |
| **D** - Dependency Inversion | Depender de abstracciones, no de concreciones | Controllers dependen de IServices, no de implementaciones concretas |

### 15.2 Clean Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    CLEAN ARCHITECTURE                         │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  REGLA: Las dependencias SOLO apuntan HACIA ADENTRO           │
│                                                               │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  ENTIDADES (HorusEye.Core)                           │   │
│  │  • Sin dependencias externas                         │   │
│  │  • Solo logica de negocio pura                       │   │
│  │  • Ejemplo: Tag, Activo, Movimiento                  │   │
│  └─────────────────────────────────────────────────────┘   │
│         ▲                                                    │
│         │                                                    │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  CASOS DE USO (HorusEye.Application)                 │   │
│  │  • Orquesta la logica de negocio                     │   │
│  │  • Define interfaces (abstracciones)                 │   │
│  │  • Ejemplo: CrearMovimientoUseCase                   │   │
│  └─────────────────────────────────────────────────────┘   │
│         ▲                                                    │
│         │                                                    │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  ADAPTADORES (HorusEye.Infrastructure)               │   │
│  │  • Implementa interfaces del dominio                 │   │
│  │  • Acceso a BD, APIs externas, etc.                  │   │
│  │  • Ejemplo: MovimientoRepository                     │   │
│  └─────────────────────────────────────────────────────┘   │
│         ▲                                                    │
│         │                                                    │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  FRAMEWORKS (HorusEye.Api)                           │   │
│  │  • Punto de entrada de la aplicacion                 │   │
│  │  • Configura DI, middleware, endpoints               │   │
│  │  • Ejemplo: Program.cs, Controllers                  │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

### 15.3 Principios de Diseno

| Principio | Descripcion | Ejemplo |
|-----------|-------------|---------|
| **DRY** | Don't Repeat Yourself | Servicios compartidos en vez de logica duplicada |
| **KISS** | Keep It Simple, Simple | Soluciones simples antes que complejas |
| **YAGNI** | You Aren't Gonna Need It | No construir features que no se necesitan ahora |
| **Separation of Concerns** | Separar responsabilidades | Capas claras: Presentation, Domain, Infrastructure |
| **Composition over Inheritance** | Composicion sobre herencia | Usar interfaces y composicion en vez de herencia profunda |
| **Fail Fast** | Fallar rapido | Validaciones tempranas, null checks, exceptions claras |

### 15.4 Patron Repository

```csharp
// Interfaz (en Domain)
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}

// Implementacion (en Infrastructure)
public class Repository<T> : IRepository<T> where T : class
{
    private readonly DbContext _context;
    
    public async Task<T> GetByIdAsync(Guid id)
    {
        return await _context.Set<T>().FindAsync(id);
    }
    // ...
}
```

### 15.5 Patron Unit of Work

```csharp
// Interfaz
public interface IUnitOfWork : IDisposable
{
    IClienteRepository Clientes { get; }
    IDispositivoRepository Dispositivos { get; }
    IMovimientoRepository Movimientos { get; }
    Task<int> SaveChangesAsync();
}

// Implementacion
public class UnitOfWork : IUnitOfWork
{
    private readonly HorusEyeDbContext _context;
    
    public IClienteRepository Clientes { get; }
    public IDispositivoRepository Dispositivos { get; }
    public IMovimientoRepository Movimientos { get; }
    
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
```

### 15.6 Patron CQRS (Command Query Responsibility Segregation)

```csharp
// Command (Escribir)
public class CrearClienteCommand
{
    public string Nombre { get; set; }
    public string RUC { get; set; }
    // ...
}

// Query (Leer)
public class ObtenerClienteQuery
{
    public Guid ClienteId { get; set; }
}

// Handler
public class CrearClienteHandler
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<Cliente> Handle(CrearClienteCommand command)
    {
        var cliente = new Cliente { Nombre = command.Nombre };
        await _unitOfWork.Clientes.AddAsync(cliente);
        await _unitOfWork.SaveChangesAsync();
        return cliente;
    }
}
```

### 15.7 Patron Mediator (MediatR)

```csharp
// Registrar MediatR
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Controller简化
[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly IMediator _mediator;
    
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearClienteCommand command)
    {
        var cliente = await _mediator.Send(command);
        return Ok(cliente);
    }
}
```

### 15.8 Validacion con FluentValidation

```csharp
public class CrearClienteValidator : AbstractValidator<CrearClienteCommand>
{
    public CrearClienteValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(200);
        
        RuleFor(x => x.RUC)
            .NotEmpty().WithMessage("El RUC es requerido")
            .Matches(@"^\d{13}$").WithMessage("RUC debe tener 13 digitos");
    }
}
```

### 15.9 Logging con Serilog

```csharp
// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.json", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Usar en servicios
public class MovimientoService : IMovimientoService
{
    private readonly ILogger<MovimientoService> _logger;
    
    public async Task<Movimiento> CrearAsync(CrearMovimientoCommand command)
    {
        _logger.LogInformation("Creando movimiento para activo {ActivoId}", command.ActivoId);
        // ...
    }
}
```

### 15.10 Testing

```
┌─────────────────────────────────────────────────────────────┐
│                    ESTRATEGIA DE TESTING                      │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  UNIT TESTS (70%)                                            │
│  └── Prueban logica de negocio aislada                       │
│      └── Ejemplo: Validar que tag existe                     │
│      └── Ejemplo: Calcular movimiento correcto               │
│                                                               │
│  INTEGRATION TESTS (20%)                                     │
│  └── Prueban interaccion entre componentes                   │
│      └── Ejemplo: Repository + Database                      │
│      └── Ejemplo: API + Service                              │
│                                                               │
│  E2E TESTS (10%)                                             │
│  └── Prueban flujo completo                                  │
│      └── Ejemplo: Crear cliente → Agregar dispositivo →      │
│          Recibir evento → Verificar movimiento                │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

### 15.11 Estructura de Proyecto Final

```
HorusEye/
├── src/
│   ├── HorusEye.Domain/              # Entidades y reglas de negocio
│   │   ├── Entities/                 # Cliente, Dispositivo, Movimiento
│   │   ├── Enums/                    # TipoDispositivo, EstadoTag
│   │   ├── Interfaces/               # IRepository, IUnitOfWork
│   │   └── Events/                   # Domain events
│   │
│   ├── HorusEye.Application/        # Casos de uso y logica
│   │   ├── Commands/                 # CrearCliente, RegistrarDispositivo
│   │   ├── Queries/                  # ObtenerCliente, ListarDispositivos
│   │   ├── Handlers/                 # Manejadores de commands/queries
│   │   ├── Validators/               # FluentValidation
│   │   └── Interfaces/               # IServices
│   │
│   ├── HorusEye.Infrastructure/     # Acceso a datos y servicios externos
│   │   ├── Persistence/              # DbContext, Repositories
│   │   ├── Services/                 # TokenService, EmailService
│   │   └── ExternalApis/             # Clientes API externas
│   │
│   └── HorusEye.Api/                # Punto de entrada
│       ├── Controllers/              # Endpoints REST
│       ├── Hubs/                     # SignalR
│       ├── Middleware/               # ExceptionHandler, Logging
│       ├── Filters/                  # Action filters
│       └── Program.cs               # Configuracion DI
│
├── tests/
│   ├── HorusEye.UnitTests/          # Tests unitarios
│   ├── HorusEye.IntegrationTests/   # Tests de integracion
│   └── HorusEye.E2ETests/           # Tests end-to-end
│
└── docs/                             # Documentacion
    ├── ARCHITECTURE.md
    ├── API.md
    └── DEPLOYMENT.md
```

### 15.12 Checklist de Calidad

| Area | Practica | Estado |
|------|----------|--------|
| **Codigo** | Seguir convenciones C# | Pendiente |
| **Codigo** | Usar regiones para organizar | Pendiente |
| **Codigo** | Comentarios solo lo necesario | Pendiente |
| **Testing** | Coverage minimo 70% | Pendiente |
| **Testing** | Tests antes de commits | Pendiente |
| **Seguridad** | Validar inputs | Pendiente |
| **Seguridad** | No exponeer secrets | Pendiente |
| **Performance** | Queries optimizadas | Pendiente |
| **Performance** | Usar caching cuando aplique | Pendiente |
| **Documentacion** | Swagger actualizado | Pendiente |
| **Documentacion** | README con instrucciones | Pendiente |


---

## 16. AUDITORIA, TRAZABILIDAD Y LOGGING

### 16.1 Definiciones

| Concepto | Definicion | Ejemplo |
|----------|------------|---------|
| **Auditoria** | Quien hizo que, cuando y como | "Juan creo el cliente X a las 10:30" |
| **Trazabilidad** | Seguimiento completo de una accion | "Tag EPC123 leido → Movimiento creado → Dashboard actualizado" |
| **Logging** | Registro tecnico para soporte | "ERROR: Timeout al conectar con dispositivo U300-01" |

### 16.2 Auditoria (Quien hizo que)

#### Tabla de Auditoria

```sql
CREATE TABLE Auditoria (
    Id BIGSERIAL PRIMARY KEY,
    UsuarioId VARCHAR(450),           -- Quien (IdentityUser.Id)
    UsuarioEmail VARCHAR(256),        -- Quien (email)
    Accion VARCHAR(50),               -- Que (CREATE, UPDATE, DELETE, LOGIN, etc.)
    Entidad VARCHAR(100),             -- Sobre que (Cliente, Dispositivo, Movimiento)
    EntidadId VARCHAR(100),           -- ID del registro afectado
    DatosAntes JSONB,                 -- Estado antes del cambio
    Despues JSONB,                    -- Estado despues del cambio
    IPAddress VARCHAR(45),            -- Desde donde
    UserAgent VARCHAR(500),           -- Dispositivo/navegador
    Timestamp TIMESTAMP DEFAULT NOW() -- Cuando
);
```

#### Ejemplo de Auditoria

```json
{
  "usuarioEmail": "admin@horuseye.com",
  "accion": "CREATE",
  "entidad": "Cliente",
  "entidadId": "uuid-123",
  "datosAntes": null,
  "despues": {
    "nombre": "Empresa ABC",
    "ruc": "1234567890123"
  },
  "ipAddress": "192.168.1.100",
  "timestamp": "2026-07-12T10:30:00Z"
}
```

#### Implementacion

```csharp
// Interfaz
public interface IAuditService
{
    Task RegistrarAsync(string accion, string entidad, string entidadId, 
                        object datosAntes, object despues);
}

// Implementacion
public class AuditService : IAuditService
{
    private readonly HorusEyeDbContext _context;
    private readonly IHttpContextAccessor _httpContext;
    
    public async Task RegistrarAsync(string accion, string entidad, string entidadId,
                                     object datosAntes, object despues)
    {
        var auditoria = new Auditoria
        {
            UsuarioId = _httpContext.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            UsuarioEmail = _httpContext.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value,
            Accion = accion,
            Entidad = entidad,
            EntidadId = entidadId,
            DatosAntes = datosAntes != null ? JsonSerializer.Serialize(datosAntes) : null,
            Despues = despues != null ? JsonSerializer.Serialize(despues) : null,
            IPAddress = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
            UserAgent = _httpContext.HttpContext?.Request?.Headers["User-Agent"].ToString(),
            Timestamp = DateTimeOffset.UtcNow
        };
        
        _context.Auditoria.Add(auditoria);
        await _context.SaveChangesAsync();
    }
}
```

### 16.3 Trazabilidad (Seguimiento completo)

#### Flujo de Trazabilidad RFID

```
┌─────────────────────────────────────────────────────────────┐
│                    TRAZABILIDAD RFID                          │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  1. TAG DETECTADO                                            │
│     └── Log: "Tag E280... detectado en U300-01, Antena 3"   │
│     └── Auditoria: DeviceId, EPC, Timestamp                  │
│                                                               │
│  2. TAG VALIDADO                                             │
│     └── Log: "Tag E280... validado, Activo: NB-001"         │
│     └── Auditoria: TagId, ActivoId                           │
│                                                               │
│  3. MOVIMIENTO CREADO                                        │
│     └── Log: "Movimiento #1234 creado, Tipo: INGRESO"       │
│     └── Auditoria: MovimientoId, Tipo, Autorizado            │
│                                                               │
│  4. DASHBOARD ACTUALIZADO                                    │
│     └── Log: "SignalR broadcast a 5 clientes conectados"    │
│     └── Auditoria: ClientesNotificados                       │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

#### Tabla de Trazabilidad

```sql
CREATE TABLE Trazabilidad (
    Id BIGSERIAL PRIMARY KEY,
    TraceId VARCHAR(36),               -- ID unico del evento (UUID)
    Evento VARCHAR(100),               -- Tipo de evento
    DispositivoId UUID,                -- Dispositivo origen
    ClienteId UUID,                    -- Cliente asociado
    TagEPC VARCHAR(200),               -- Tag leido
    Datos JSONB,                       -- Datos del evento
    ParentTraceId VARCHAR(36),         -- Evento padre (si aplica)
    Timestamp TIMESTAMP DEFAULT NOW()
);
```

#### Ejemplo de Trazabilidad

```json
{
  "traceId": "evt-uuid-001",
  "evento": "RFID_TAG_DETECTADO",
  "dispositivoId": "uuid-dispositivo-01",
  "clienteId": "uuid-cliente-01",
  "tagEPC": "E280116020002081104504794",
  "datos": {
    "antenna": 3,
    "rssi": -45,
    "ubicacion": "Puerta Principal"
  },
  "parentTraceId": null,
  "timestamp": "2026-07-12T10:30:00Z"
}
```

### 16.4 Logging (Soporte tecnico)

#### Niveles de Log

| Nivel | Uso | Ejemplo |
|-------|-----|---------|
| **DEBUG** | Informacion detallada para desarrollo | "Tag EPC parseado correctamente" |
| **INFO** | Eventos normales del sistema | "Cliente ABC creado exitosamente" |
| **WARNING** | Situaciones inusuales pero manejables | "Tag no encontrado en base de datos" |
| **ERROR** | Errores que requieren atencion | "Timeout al conectar con U300-01" |
| **FATAL** | Errores criticos del sistema | "Base de datos no disponible" |

#### Configuracion Serilog

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/log-.json",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        formatter: new JsonFormatter())
    .WriteTo.Seq("http://seq-server:5341")  // Opcional: Seq para visualizacion
    .CreateLogger();
```

#### Ejemplo de Uso

```csharp
public class MovimientoService : IMovimientoService
{
    private readonly ILogger<MovimientoService> _logger;
    
    public async Task<Movimiento> ProcesarEventoAsync(RfidEvent rfidEvent)
    {
        _logger.LogInformation(
            "Procesando evento RFID: DeviceId={DeviceId}, EPC={EPC}, Antena={Antena}",
            rfidEvent.DeviceId, rfidEvent.EPC, rfidEvent.Antenna);
        
        try
        {
            var movimiento = await CrearMovimientoAsync(rfidEvent);
            
            _logger.LogInformation(
                "Movimiento creado: Id={MovimientoId}, Tipo={Tipo}",
                movimiento.Id, movimiento.TipoMovimiento);
            
            return movimiento;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al procesar evento RFID: DeviceId={DeviceId}, EPC={EPC}",
                rfidEvent.DeviceId, rfidEvent.EPC);
            throw;
        }
    }
}
```

### 16.5 Health Checks

```csharp
// En Program.cs
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql")
    .AddCheck<DispositivosHealthCheck>("dispositivos")
    .AddCheck<RfidServiceHealthCheck>("rfid-service");

// Endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Health Check personalizado
public class DispositivosHealthCheck : IHealthCheck
{
    private readonly HorusEyeDbContext _context;
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken)
    {
        var dispositivosActivos = await _context.Dispositivos
            .CountAsync(d => d.Activo, cancellationToken);
        
        if (dispositivosActivos > 0)
            return HealthCheckResult.Healthy($"{dispositivosActivos} dispositivos activos");
        else
            return HealthCheckResult.Degraded("No hay dispositivos activos");
    }
}
```

### 16.6 Metricas (Opcional - Futuro)

| Metrica | Descripcion | Herramienta |
|---------|-------------|-------------|
| **Requests por segundo** | Carga de la API | Prometheus |
| **Latencia de respuesta** | Tiempo de respuesta | Prometheus |
| **Errores por minuto** | Tasa de errores | Prometheus |
| **Tags leidos por hora** | Volumen RFID | Custom |
| **Dispositivos conectados** | Estado del hardware | Custom |

### 16.7 Resumen de Tablas de Auditoria/Trazabilidad

| Tabla | Proposito | Retencion |
|-------|-----------|-----------|
| **Auditoria** | Quien hizo que | Indefinida |
| **Trazabilidad** | Seguimiento RFID | 1 ano |
| **Logs** | Soporte tecnico | 30 dias |
| **Movimientos** | Historico de negocio | Indefinida |


---

## 17. MANEJO DE ERRORES

### 17.1 Jerarquia de Excepciones

```
┌─────────────────────────────────────────────────────────────┐
│                    JERARQUIA DE EXCEPCIONES                   │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  Exception (base)                                            │
│  │                                                           │
│  ├── AppException (excepciones de negocio)                   │
│  │   ├── ClienteNoEncontradoException                        │
│  │   ├── DispositivoNoEncontradoException                    │
│  │   ├── TagNoRegistradoException                            │
│  │   ├── AutorizacionRequeridaException                      │
│  │   └── DispositivoDesconectadoException                    │
│  │                                                           │
│  ├── ValidationException (validaciones)                      │
│  │   ├── DatoRequeridoException                              │
│  │   ├── FormatoInvalidoException                            │
│  │   └── LimiteExcedidoException                             │
│  │                                                           │
│  └── InfrastructureException (infraestructura)               │
│      ├── DatabaseConnectionException                         │
│      ├── ExternalApiException                                │
│      └── TimeoutException                                    │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

### 17.2 Modelo de Respuesta de Error

```csharp
public class ErrorResponse
{
    public bool Success { get; set; } = false;
    public string Code { get; set; }         // "CLIENTE_NO_ENCONTRADO"
    public string Message { get; set; }      // Mensaje legible
    public string Detail { get; set; }       // Detalle tecnico (solo en dev)
    public string TraceId { get; set; }      // ID para trazabilidad
    public Dictionary<string, string[]> Errors { get; set; }  // Errores de validacion
    public DateTimeOffset Timestamp { get; set; }
}

// Ejemplo de respuesta
{
    "success": false,
    "code": "TAG_NO_REGISTRADO",
    "message": "El tag E280... no esta registrado en el sistema",
    "detail": null,
    "traceId": "evt-uuid-001",
    "errors": null,
    "timestamp": "2026-07-12T10:30:00Z"
}
```

### 17.3 Excepciones de Negocio

```csharp
// Excepcion base de negocio
public class AppException : Exception
{
    public string Code { get; }
    
    public AppException(string code, string message) : base(message)
    {
        Code = code;
    }
}

// Ejemplos
public class ClienteNoEncontradoException : AppException
{
    public ClienteNoEncontradoException(Guid id) 
        : base("CLIENTE_NO_ENCONTRADO", $"Cliente {id} no encontrado")
    {
    }
}

public class DispositivoNoEncontradoException : AppException
{
    public DispositivoNoEncontradoException(Guid id)
        : base("DISPOSITIVO_NO_ENCONTRADO", $"Dispositivo {id} no encontrado")
    {
    }
}

public class TagNoRegistradoException : AppException
{
    public TagNoRegistradoException(string epc)
        : base("TAG_NO_REGISTRADO", $"Tag {epc} no registrado en el sistema")
    {
    }
}

public class AutorizacionRequeridaException : AppException
{
    public AutorizacionRequeridaException(string epc)
        : base("AUTORIZACION_REQUERIDA", 
               $"Salida no autorizada para tag {epc}")
    {
    }
}
```

### 17.4 Middleware de Excepciones

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = exception switch
        {
            // Excepciones de negocio
            AppException appEx => new ErrorResponse
            {
                Code = appEx.Code,
                Message = appEx.Message,
                TraceId = context.TraceIdentifier
            },
            
            // Errores de validacion (FluentValidation)
            ValidationException validationEx => new ErrorResponse
            {
                Code = "VALIDACION_ERROR",
                Message = "Error de validacion",
                Errors = validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()),
                TraceId = context.TraceIdentifier
            },
            
            // Errores generales
            _ => new ErrorResponse
            {
                Code = "ERROR_INTERNO",
                Message = "Ha ocurrido un error interno",
                Detail = _logger.IsEnabled(LogLevel.Debug) ? exception.Message : null,
                TraceId = context.TraceIdentifier
            }
        };
        
        // Log del error
        _logger.LogError(exception, 
            "Error {ErrorCode}: {Message} | TraceId={TraceId}",
            response.Code, response.Message, response.TraceId);
        
        // Respuesta HTTP
        var statusCode = exception switch
        {
            ClienteNoEncontradoException => 404,
            DispositivoNoEncontradoException => 404,
            TagNoRegistradoException => 404,
            AutorizacionRequeridaException => 403,
            ValidationException => 400,
            _ => 500
        };
        
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(response);
    }
}
```

### 17.5 Errores Comunes y Acciones

| Error | Codigo | HTTP | Accion |
|-------|--------|------|--------|
| Cliente no encontrado | CLIENTE_NO_ENCONTRADO | 404 | Verificar ID |
| Dispositivo no encontrado | DISPOSITIVO_NO_ENCONTRADO | 404 | Verificar ID |
| Tag no registrado | TAG_NO_REGISTRADO | 404 | Registrar tag o verificar EPC |
| Salida no autorizada | AUTORIZACION_REQUERIDA | 403 | Crear autorizacion |
| Dispositivo desconectado | DISPOSITIVO_DESCONECTADO | 503 | Verificar conexion |
| Timeout de API | TIMEOUT_API | 504 | Reintentar o verificar red |
| Error de BD | ERROR_BD | 500 | Verificar conexion BD |
| Validacion fallida | VALIDACION_ERROR | 400 | Corregir datos |

### 17.6 Retry y Circuit Breaker

```csharp
// Para llamadas a dispositivos externos
services.AddHttpClient("RFIDDevice")
    .AddTransientHttpErrorPolicy(policy => 
        policy.WaitAndRetryAsync(3, retryAttempt => 
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
    .AddTransientHttpErrorPolicy(policy => 
        policy.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));
```

### 17.7 Logging de Errores

```csharp
// En el middleware
_logger.LogError(exception, 
    "Error no manejado | TraceId={TraceId} | Path={Path} | Method={Method}",
    context.TraceIdentifier,
    context.Request.Path,
    context.Request.Method);

// En servicios
try
{
    await ProcesarEventoAsync(rfidEvent);
}
catch (TagNoRegistradoException ex)
{
    _logger.LogWarning(ex, 
        "Tag no registrado: {EPC} | DispositivoId={DeviceId}",
        rfidEvent.EPC, rfidEvent.DeviceId);
    throw; // Re-lanzar para que el middleware lo maneje
}
catch (Exception ex)
{
    _logger.LogError(ex, 
        "Error inesperado al procesar evento: {EPC}",
        rfidEvent.EPC);
    throw;
}
```

### 17.8 Respuesta del Sistema

| Escenario | Respuesta |
|-----------|-----------|
| **Exito** | `200 OK` + `ApiResponse<T>.Ok(data)` |
| **Validacion fallida** | `400 Bad Request` + `ErrorResponse` con errores |
| **No encontrado** | `404 Not Found` + `ErrorResponse` |
| **No autorizado** | `401 Unauthorized` + `ErrorResponse` |
| **Prohibido** | `403 Forbidden` + `ErrorResponse` |
| **Error interno** | `500 Internal Server Error` + `ErrorResponse` |
| **Servicio no disponible** | `503 Service Unavailable` + `ErrorResponse` |


---

## 18. INTERNACIONALIZACION (i18n)

### 18.1 Estructura de Archivos de Idioma

```
src/
├── HorusEye.Api/
│   └── Resources/
│       ├── Messages.es.json          # Mensajes en español
│       └── Messages.en.json          # Mensajes en inglés
│
└── Frontend/
    └── src/
        └── locales/
            ├── es/
            │   ├── common.json       # Textos comunes
            │   ├── menu.json         # Menus
            │   ├── dashboard.json    # Dashboard
            │   ├── clientes.json     # Clientes
            │   ├── dispositivos.json # Dispositivos
            │   ├── auth.json         # Autenticacion
            │   └── errors.json       # Mensajes de error
            │
            └── en/
                ├── common.json
                ├── menu.json
                ├── dashboard.json
                ├── clientes.json
                ├── dispositivos.json
                ├── auth.json
                └── errors.json
```

### 18.2 Archivos JSON - Backend (C#)

#### Messages.es.json

```json
{
  "Cliente": {
    "NoEncontrado": "Cliente no encontrado",
    "CreadoExitosamente": "Cliente creado exitosamente",
    "ActualizadoExitosamente": "Cliente actualizado exitosamente",
    "EliminadoExitosamente": "Cliente eliminado exitosamente",
    "NombreRequerido": "El nombre del cliente es requerido",
    "RucInvalido": "El RUC debe tener 13 digitos"
  },
  "Dispositivo": {
    "NoEncontrado": "Dispositivo no encontrado",
    "CreadoExitosamente": "Dispositivo registrado exitosamente",
    "Desconectado": "Dispositivo desconectado",
    "EndpointConfigurado": "Endpoint del dispositivo configurado"
  },
  "Tag": {
    "NoRegistrado": "Tag no registrado en el sistema",
    "Danado": "Tag esta marcado como danado",
    "SinAsignar": "Tag no esta asignado a ningun activo"
  },
  "Movimiento": {
    "Registrado": "Movimiento registrado exitosamente",
    "SalidaNoAutorizada": "ALERTA: Salida no autorizada"
  }
}
```

#### Messages.en.json

```json
{
  "Cliente": {
    "NoEncontrado": "Client not found",
    "CreadoExitosamente": "Client created successfully",
    "ActualizadoExitosamente": "Client updated successfully",
    "EliminadoExitosamente": "Client deleted successfully",
    "NombreRequerido": "Client name is required",
    "RucInvalido": "RUC must have 13 digits"
  },
  "Dispositivo": {
    "NoEncontrado": "Device not found",
    "CreadoExitosamente": "Device registered successfully",
    "Desconectado": "Device disconnected",
    "EndpointConfigurado": "Device endpoint configured"
  },
  "Tag": {
    "NoRegistrado": "Tag not registered in system",
    "Danado": "Tag is marked as damaged",
    "SinAsignar": "Tag is not assigned to any asset"
  },
  "Movimiento": {
    "Registrado": "Movement registered successfully",
    "SalidaNoAutorizada": "ALERT: Unauthorized exit"
  }
}
```

### 18.3 Uso en Backend

```csharp
// En Controllers
[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IStringLocalizer<ClientesController> _localizer;
    
    [HttpGet("{id}")]
    public async Task<IActionResult> Obtener(Guid id)
    {
        var cliente = await _service.ObtenerPorIdAsync(id);
        if (cliente == null)
            return NotFound(new { message = _localizer["Cliente.NoEncontrado"] });
        
        return Ok(cliente);
    }
}

// Configuracion en Program.cs
builder.Services.AddLocalization(options => 
    options.ResourcesPath = "Resources");

// Endpoint para cambiar idioma
app.MapGet("/api/culture/{culture}", (string culture) =>
{
    Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
    Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
    return Results.Ok(new { culture });
});
```

### 18.4 Archivos JSON - Frontend (React)

#### es/common.json

```json
{
  "comun": {
    "guardar": "Guardar",
    "cancelar": "Cancelar",
    "eliminar": "Eliminar",
    "editar": "Editar",
    "crear": "Crear",
    "buscar": "Buscar",
    "cargando": "Cargando...",
    "noDatos": "No hay datos disponibles",
    "confirmar": "¿Esta seguro?",
    "exito": "Operacion exitosa",
    "error": "Ha ocurrido un error"
  },
  "menu": {
    "dashboard": "Dashboard",
    "clientes": "Clientes",
    "dispositivos": "Dispositivos",
    "activos": "Activos",
    "tags": "Tags RFID",
    "movimientos": "Movimientos",
    "reportes": "Reportes",
    "configuracion": "Configuracion",
    "usuarios": "Usuarios",
    "cerrarSesion": "Cerrar Sesion"
  },
  "dashboard": {
    "titulo": "Dashboard",
    "totalActivos": "Total Activos",
    "activosDentro": "Activos Dentro",
    "activosFuera": "Activos Fuera",
    "ingresosHoy": "Ingresos Hoy",
    "salidasHoy": "Salidas Hoy",
    "movimientosRecientes": "Movimientos Recientes",
    "conectado": "Conectado",
    "desconectado": "Desconectado"
  }
}
```

#### en/common.json

```json
{
  "comun": {
    "guardar": "Save",
    "cancelar": "Cancel",
    "eliminar": "Delete",
    "editar": "Edit",
    "crear": "Create",
    "buscar": "Search",
    "cargando": "Loading...",
    "noDatos": "No data available",
    "confirmar": "Are you sure?",
    "exito": "Operation successful",
    "error": "An error has occurred"
  },
  "menu": {
    "dashboard": "Dashboard",
    "clientes": "Clients",
    "dispositivos": "Devices",
    "activos": "Assets",
    "tags": "RFID Tags",
    "movimientos": "Movements",
    "reportes": "Reports",
    "configuracion": "Settings",
    "usuarios": "Users",
    "cerrarSesion": "Logout"
  },
  "dashboard": {
    "titulo": "Dashboard",
    "totalActivos": "Total Assets",
    "activosDentro": "Assets Inside",
    "activosFuera": "Assets Outside",
    "ingresosHoy": "Entries Today",
    "salidasHoy": "Exits Today",
    "movimientosRecientes": "Recent Movements",
    "conectado": "Connected",
    "desconectado": "Disconnected"
  }
}
```

### 18.5 Uso en Frontend (React)

#### Contexto de Idioma

```typescript
// src/context/LanguageContext.tsx
import React, { createContext, useContext, useState } from 'react';
import es from '../locales/es/common.json';
import en from '../locales/en/common.json';

type Language = 'es' | 'en';
type Translations = typeof es;

interface LanguageContextType {
  language: Language;
  t: (key: string) => string;
  setLanguage: (lang: Language) => void;
}

const translations: Record<Language, Translations> = { es, en };

const LanguageContext = createContext<LanguageContextType | null>(null);

export function LanguageProvider({ children }: { children: React.ReactNode }) {
  const [language, setLanguage] = useState<Language>(
    () => (localStorage.getItem('language') as Language) || 'es'
  );
  
  const t = (key: string): string => {
    const keys = key.split('.');
    let value: any = translations[language];
    for (const k of keys) {
      value = value?.[k];
    }
    return value || key;
  };
  
  const handleSetLanguage = (lang: Language) => {
    setLanguage(lang);
    localStorage.setItem('language', lang);
  };
  
  return (
    <LanguageContext.Provider value={{ language, t, setLanguage: handleSetLanguage }}>
      {children}
    </LanguageContext.Provider>
  );
}

export const useLanguage = () => {
  const context = useContext(LanguageContext);
  if (!context) throw new Error('useLanguage must be used within LanguageProvider');
  return context;
};
```

#### Uso en Componentes

```tsx
// En cualquier componente
import { useLanguage } from '../context/LanguageContext';

function Dashboard() {
  const { t, language, setLanguage } = useLanguage();
  
  return (
    <div>
      <h1>{t('dashboard.titulo')}</h1>
      <p>{t('dashboard.totalActivos')}: 150</p>
      
      <button onClick={() => setLanguage(language === 'es' ? 'en' : 'es')}>
        {language === 'es' ? 'English' : 'Español'}
      </button>
    </div>
  );
}
```

### 18.6 Selector de Idioma

```tsx
// src/components/LanguageSelector.tsx
import { useLanguage } from '../context/LanguageContext';

export default function LanguageSelector() {
  const { language, setLanguage } = useLanguage();
  
  return (
    <select 
      value={language} 
      onChange={(e) => setLanguage(e.target.value as 'es' | 'en')}
    >
      <option value="es">🇪🇸 Español</option>
      <option value="en">🇺🇸 English</option>
    </select>
  );
}
```

### 18.7 Traducciones por Modulo

| Modulo | Archivos JSON | Contenido |
|--------|---------------|-----------|
| **Comun** | common.json | Botones, mensajes generales |
| **Menu** | menu.json | Navegacion principal |
| **Dashboard** | dashboard.json | Metricas, graficos |
| **Clientes** | clientes.json | CRUD de clientes |
| **Dispositivos** | dispositivos.json | Gestion de dispositivos |
| **Activos** | activos.json | Gestion de activos |
| **Tags** | tags.json | Gestion de tags RFID |
| **Auth** | auth.json | Login, registro |
| **Errores** | errors.json | Mensajes de error |


---

## 19. SEGURIDAD

### 19.1 Pilares de Seguridad

| Pilar | Descripcion | Implementacion |
|-------|-------------|----------------|
| **Autenticacion** | Quien eres | JWT + ASP.NET Identity |
| **Autorizacion** | Que puedes hacer | Roles + Policies |
| **Confidencialidad** | Datos protegidos | HTTPS + Cifrado |
| **Integridad** | Datos no alterados | Validaciones + Constraints |
| **Disponibilidad** | Sistema accesible | Health checks + Redundancia |

### 19.2 Autenticacion (JWT)

#### Flujo de Autenticacion

```
┌─────────────────────────────────────────────────────────────┐
│                    FLUJO JWT                                  │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  1. LOGIN                                                     │
│     Usuario ──POST /api/auth/login──► API                    │
│     { email, password }                                       │
│                                                               │
│  2. RESPUESTA                                                 │
│     API ──200 OK──► Usuario                                  │
│     {                                                         │
│       accessToken: "eyJhbG...",     # Expira en 15 min       │
│       refreshToken: "abc123...",    # Expira en 7 dias        │
│       expiresIn: 900                                             │
│     }                                                         │
│                                                               │
│  3. USO DE TOKEN                                              │
│     Usuario ──GET /api/clientes──► API                        │
│     Header: Authorization: Bearer eyJhbG...                   │
│                                                               │
│  4. REFRESH                                                   │
│     Cuando expira el accessToken:                             │
│     Usuario ──POST /api/auth/refresh──► API                   │
│     { refreshToken }                                           │
│     API ──200 OK──► Nuevo accessToken + refreshToken         │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

#### Configuracion JWT

```csharp
// Program.cs
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ClockSkew = TimeSpan.Zero  // Sin tolerancia de tiempo
    };
});
```

#### TokenService

```csharp
public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    
    public string GenerateAccessToken(IdentityUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),  // 15 minutos
            signingCredentials: creds
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
```

### 19.3 Autorizacion (Roles y Policies)

#### Roles del Sistema

| Rol | Descripcion | Permisos |
|-----|-------------|----------|
| **Administrador** | Acceso total | CRUD completo, gestionar usuarios, configurar dispositivos |
| **Operador** | Operaciones diarias | Ver dashboard, registrar movimientos, ver reportes |
| **Consultor** | Solo lectura | Ver dashboard, ver reportes |
| **Tecnico** | Dispositivos | Configurar dispositivos, ver estado |

#### Policies Personalizadas

```csharp
// Program.cs
builder.Services.AddAuthorization(options =>
{
    // Policy basada en rol
    options.AddPolicy("SoloAdmin", policy =>
        policy.RequireRole("Administrador"));
    
    // Policy basada en claim
    options.AddPolicy("PuedeEditarDispositivos", policy =>
        policy.RequireClaim("permission", "devices.edit"));
    
    // Policy compuesta
    options.AddPolicy("OperadorOAdmin", policy =>
        policy.RequireRole("Administrador", "Operador"));
    
    // Policy basada en resource
    options.AddPolicy("PuedeAccederCliente", policy =>
        policy.AddRequirements(new ClienteAccessRequirement()));
});
```

#### Uso en Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]  // Requiere autenticacion
public class DispositivosController : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Administrador,Operador")]  // Roles permitidos
    public async Task<IActionResult> Listar()
    {
        // ...
    }
    
    [HttpPost]
    [Authorize(Policy = "SoloAdmin")]  // Solo administradores
    public async Task<IActionResult> Crear([FromBody] CrearDispositivoCommand command)
    {
        // ...
    }
    
    [HttpPut("{id}/config")]
    [Authorize(Policy = "PuedeEditarDispositivos")]  // Policy personalizada
    public async Task<IActionResult> Configurar(Guid id, [FromBody] ConfigurarEndpointCommand command)
    {
        // ...
    }
}
```

### 19.4 Password Policy

```csharp
// Configuracion de Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Requisitos de contraseña
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    
    // Bloqueo por intentos fallidos
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // Requisitos de usuario
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = 
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
})
.AddEntityFrameworkStores<HorusEyeDbContext>()
.AddDefaultTokenProviders();
```

### 19.5 Seguridad de Dispositivos (API Key)

```csharp
// Para dispositivos RFID que no pueden usar JWT
public class ApiKeyRequirement : IAuthorizationRequirement
{
    public string ApiKey { get; }
    
    public ApiKeyRequirement(string apiKey)
    {
        ApiKey = apiKey;
    }
}

public class ApiKeyHandler : AuthorizationHandler<ApiKeyRequirement>
{
    private readonly IConfiguration _config;
    
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, ApiKeyRequirement requirement)
    {
        var apiKey = context.Resource as HttpContext?.Request?.Headers["X-Api-Key"].ToString();
        
        if (apiKey == requirement.ApiKey)
        {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}

// Uso
[HttpPost("rfid-event")]
[Authorize(Policy = "ApiKeyRequired")]
public async Task<IActionResult> RecibirEventoRfid([FromBody] RfidEvent evento)
{
    // ...
}
```

### 19.6 CORS

```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "https://horuseye.mauricioadachi.dev",  // Frontend
                "http://localhost:5173"                      // Desarrollo local
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();  // Para SignalR
    });
});

app.UseCors("AllowFrontend");
```

### 19.7 Rate Limiting

```csharp
// Program.cs
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 100;  // 100 requests
        opt.Window = TimeSpan.FromMinute(1);  // por minuto
    });
    
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

app.UseRateLimiter();

// En Controllers
[HttpPost("rfid-event")]
[EnableRateLimiting("fixed")]
public async Task<IActionResult> RecibirEventoRfid([FromBody] RfidEvent evento)
{
    // ...
}
```

### 19.8 HTTPS y Seguridad de Transporte

```csharp
// Program.cs
app.UseHttpsRedirection();
app.UseHsts();

// Headers de seguridad
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'");
    
    await next();
});
```

### 19.9 Prevencion de SQL Injection

```csharp
// SIEMPRE usar parametrizacion
var clientes = await _context.Clientes
    .Where(c => c.Nombre.Contains(searchTerm))  // EF Core parametriza automaticamente
    .ToListAsync();

// NUNCA usar string concatenation
// ❌ var query = $"SELECT * FROM Clientes WHERE Nombre LIKE '%{searchTerm}%'";
// ✅ var query = _context.Clientes.Where(c => c.Nombre.Contains(searchTerm));
```

### 19.10 Validacion de Inputs

```csharp
// Con FluentValidation
public class CrearClienteValidator : AbstractValidator<CrearClienteCommand>
{
    public CrearClienteValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("Nombre es requerido")
            .MaximumLength(200).WithMessage("Nombre no puede exceder 200 caracteres")
            .Matches(@"^[a-zA-Z0-9\s]+$").WithMessage("Nombre contiene caracteres invalidos");
        
        RuleFor(x => x.RUC)
            .NotEmpty().WithMessage("RUC es requerido")
            .Matches(@"^\d{13}$").WithMessage("RUC debe tener exactamente 13 digitos");
    }
}
```

### 19.11 Seguridad de Secrets

```csharp
// appsettings.json - NUNCA secrets en codigo
{
    "Jwt": {
        "Key": "DESDE_VARIABLE_DE_ENTORNO",
        "Issuer": "HorusEyeAPI",
        "Audience": "HorusEyeFrontend"
    },
    "ConnectionStrings": {
        "DefaultConnection": "DESDE_VARIABLE_DE_ENTORNO"
    }
}

// Docker Secrets
services.AddDbContext<HorusEyeDbContext>(options =>
    options.UseNpgsql(
        Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")));
```

### 19.12 Resumen de Seguridad

| Area | Estado Actual | Mejora |
|------|---------------|--------|
| **JWT** | ✅ Implementado | Mantener |
| **Roles** | ✅ Implementado | Agregar mas roles |
| **HTTPS** | ✅ En produccion | Verificar headers |
| **Password Policy** | ✅ Configurada | Mantener |
| **Rate Limiting** | ❌ No implementado | Agregar |
| **API Key dispositivos** | ❌ No implementado | Agregar para RFID |
| **SQL Injection** | ✅ Protegido (EF Core) | Mantener |
| **Input Validation** | ⚠️ Parcial | Agregar FluentValidation |
| **Secrets** | ⚠️ Parcial | Migrar todos a env vars |


---

## 20. LOGGING Y MONITOREO

### 20.1 Stack de Observabilidad

```
┌─────────────────────────────────────────────────────────────┐
│                    STACK DE OBSERVABILIDAD                    │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  LOGGING (Que paso)                                  │   │
│  │  └── Serilog → Archivos JSON + Console               │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                               │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  METRICAS (Cuanto)                                   │   │
│  │  └── Prometheus → Grafana                            │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                               │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  TRAZAS (Donde)                                     │   │
│  │  └── OpenTelemetry → Jaeger                          │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                               │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  HEALTH CHECKS (Estado)                              │   │
│  │  └── /health endpoint                                │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

### 20.2 Logging con Serilog

#### Configuracion

```csharp
// Program.cs
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogLevel.Warning)
    .MinimumLevel.Override("System", LogLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/log-.json",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        fileSizeLimitBytes: 50_000_000,  // 50MB max
        formatter: new JsonFormatter())
    .WriteTo.Seq("http://seq:5341")  // Opcional: Seq
    .CreateLogger();
```

#### Niveles de Log

| Nivel | Uso | Ejemplo |
|-------|-----|---------|
| **Verbose** | Debug detallado | "Tag EPC parseado: bytes 0-96" |
| **Debug** | Informacion desarrollo | "Conexion JWT exitosa" |
| **Information** | Eventos normales | "Cliente ABC creado" |
| **Warning** | Situaciones inusuales | "Tag no encontrado, pero sistema opera" |
| **Error** | Errores recuperables | "Timeout dispositivo, reintentando" |
| **Fatal** | Errores criticos | "Base de datos caida" |

#### Ejemplo de Uso

```csharp
public class MovimientoService : IMovimientoService
{
    private readonly ILogger<MovimientoService> _logger;
    
    public async Task<Movimiento> ProcesarEventoAsync(RfidEvent evento)
    {
        using (LogContext.PushProperty("DeviceId", evento.DeviceId))
        using (LogContext.PushProperty("EPC", evento.EPC))
        using (LogContext.PushProperty("ClienteId", evento.ClienteId))
        {
            _logger.LogInformation("Iniciando procesamiento de evento RFID");
            
            try
            {
                var movimiento = await CrearMovimientoAsync(evento);
                
                _logger.LogInformation(
                    "Movimiento creado: Id={MovimientoId}, Tipo={Tipo}, Autorizado={Autorizado}",
                    movimiento.Id, movimiento.TipoMovimiento, movimiento.Autorizado);
                
                return movimiento;
            }
            catch (TagNoRegistradoException ex)
            {
                _logger.LogWarning(ex, "Tag no registrado: {EPC}", evento.EPC);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando evento RFID");
                throw;
            }
        }
    }
}
```

### 20.3 Metricas con Prometheus

#### Configuracion

```csharp
// Program.cs
builder.Services.AddMetrics();
builder.Services.AddPrometheusScrapingEndpoint();

var app = builder.Build();

app.UseMetricServer("/metrics");
app.UseHttpMetrics();
app.MapPrometheusScrapingEndpoint();
```

#### Metricas Personalizadas

```csharp
// Metricas RFID
public class RfidMetrics
{
    private readonly Counter _tagReadCounter;
    private readonly Counter _movementCounter;
    private readonly Histogram _processingDuration;
    private readonly Gauge _connectedDevices;
    
    public RfidMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("HorusEye.RFID");
        
        _tagReadCounter = meter.CreateCounter<long>(
            "rfid_tags_read_total",
            "Total de tags leidos");
        
        _movementCounter = meter.CreateCounter<long>(
            "rfid_movements_created_total",
            "Total de movimientos creados",
            new[] { "type", "authorized" });
        
        _processingDuration = meter.CreateHistogram<double>(
            "rfid_processing_duration_seconds",
            "Duracion del procesamiento RFID");
        
        _connectedDevices = meter.CreateGauge<int>(
            "rfid_connected_devices",
            "Numero de dispositivos conectados");
    }
    
    public void RecordTagRead(string deviceId, string antenna)
    {
        _tagReadCounter.Add(1, 
            new KeyValuePair<string, object>("device", deviceId),
            new KeyValuePair<string, object>("antenna", antenna));
    }
    
    public void RecordMovement(string type, bool authorized)
    {
        _movementCounter.Add(1,
            new KeyValuePair<string, object>("type", type),
            new KeyValuePair<string, object>("authorized", authorized.ToString()));
    }
    
    public void RecordProcessingTime(double seconds)
    {
        _processingDuration.Record(seconds);
    }
    
    public void SetConnectedDevices(int count)
    {
        _connectedDevices.Set(count);
    }
}
```

### 20.4 Health Checks

#### Configuracion

```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, 
        name: "postgresql",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "db", "ready" })
    .AddCheck<DispositivosHealthCheck>(
        "dispositivos",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "rfid", "ready" })
    .AddCheck<ApiExternaHealthCheck>(
        "api-rfidlinked",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "external", "ready" })
    .AddDiskStorageHealthCheck(
        options => options.AddDrive("C:\\", 1024),  // 1GB min
        name: "disco",
        tags: new[] { "storage" })
    .AddProcessAllocatedMemoryHealthCheck(
        1024,  // 1GB max
        name: "memoria",
        tags: new[] { "resources" });

// Endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.ToString(),
                exception = e.Value.Exception?.Message,
                description = e.Value.Description
            }),
            totalDuration = report.TotalDuration.ToString()
        };
        await context.Response.WriteAsJsonAsync(result);
    }
});
```

#### Health Check Personalizado

```csharp
public class DispositivosHealthCheck : IHealthCheck
{
    private readonly HorusEyeDbContext _context;
    private readonly IHttpClientFactory _httpClient;
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken)
    {
        var dispositivos = await _context.Dispositivos
            .Where(d => d.Activo)
            .ToListAsync(cancellationToken);
        
        var conectados = 0;
        var desconectados = 0;
        
        foreach (var dispositivo in dispositivos)
        {
            try
            {
                var response = await _httpClient.CreateClient()
                    .GetAsync($"{dispositivo.DireccionIP}/ping", 
                        cancellationToken);
                
                if (response.IsSuccessStatusCode)
                    conectados++;
                else
                    desconectados++;
            }
            catch
            {
                desconectados++;
            }
        }
        
        var data = new Dictionary<string, object>
        {
            ["total"] = dispositivos.Count,
            ["conectados"] = conectados,
            ["desconectados"] = desconectados
        };
        
        if (desconectados > dispositivos.Count * 0.5)
            return HealthCheckResult.Degraded(
                "Mas del 50% de dispositivos desconectados", data: data);
        
        if (dispositivos.Count == 0)
            return HealthCheckResult.Degraded(
                "No hay dispositivos configurados", data: data);
        
        return HealthCheckResult.Healthy(
            $"{conectados} dispositivos conectados", data: data);
    }
}
```

### 20.5 Dashboard de Monitoreo (Grafana)

#### Panel RFID

| Metrica | Tipo | Descripcion |
|---------|------|-------------|
| Tags leidos/min | Rate | Volumen de lecturas |
| Movimientos/min | Rate | Movimientos procesados |
| Tiempo respuesta | Histogram | Latencia del API |
| Dispositivos activos | Gauge | Estado de hardware |
| Errores/min | Rate | Tasa de errores |
| Tags no registrados | Counter | Tags desconocidos |

#### Alertas

| Alerta | Condicion | Accion |
|--------|-----------|--------|
| **Dispositivo offline** | Sin heartbeat > 5 min | Notificar admin |
| **Alta tasa de errores** | > 5% errores en 5 min | Revisar logs |
| **Sin lecturas** | 0 tags en 10 min | Verificar hardware |
| **Memoria alta** | > 80% uso | Escalar recurso |

### 20.6 Endpoints de Monitoreo

| Endpoint | Metodo | Descripcion |
|----------|--------|-------------|
| `/health` | GET | Health check basico |
| `/health/ready` | GET | Ready (dependencias OK) |
| `/health/live` | GET | Live (proceso corriendo) |
| `/metrics` | GET | Metricas Prometheus |
| `/api/monitoreo/dispositivos` | GET | Estado de dispositivos |
| `/api/monitoreo/estadisticas` | GET | Estadisticas en tiempo real |

### 20.7 Resumen de Observabilidad

| Componente | Herramienta | Estado |
|------------|-------------|--------|
| **Logging** | Serilog | ✅ Implementado |
| **Metricas** | Prometheus | ❌ Pendiente |
| **Dashboards** | Grafana | ❌ Pendiente |
| **Health Checks** | ASP.NET Core | ⚠️ Basico |
| **Alertas** | Grafana/PagerDuty | ❌ Pendiente |
| **Traces** | OpenTelemetry | ❌ Pendiente (futuro) |


---

## 21. TESTING Y DESPLIEGUE

### 21.1 Estrategia de Testing

```
┌─────────────────────────────────────────────────────────────┐
│                    ESTRATEGIA DE TESTING                      │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  UNIT TESTS (70%)                                    │   │
│  │  └── xUnit + Moq + FluentAssertions                 │   │
│  │  └── Prueban logica de negocio aislada               │   │
│  └─────────────────────────────────────────────────────┘   │
│                          │                                   │
│                          ▼                                   │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  INTEGRATION TESTS (20%)                             │   │
│  │  └── WebApplicationFactory + TestContainers          │   │
│  │  └── Prueban interaccion entre componentes           │   │
│  └─────────────────────────────────────────────────────┘   │
│                          │                                   │
│                          ▼                                   │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  E2E TESTS (10%)                                     │   │
│  │  └── Playwright (Frontend) + Supertest (API)         │   │
│  │  └── Prueban flujo completo                          │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

### 21.2 Unit Tests (xUnit + Moq)

#### Estructura

```
tests/
├── HorusEye.UnitTests/
│   ├── Domain/
│   │   ├── Entities/
│   │   │   ├── ClienteTests.cs
│   │   │   ├── DispositivoTests.cs
│   │   │   └── MovimientoTests.cs
│   │   └── ValueObjects/
│   │       └── RfidEventTests.cs
│   ├── Application/
│   │   ├── Handlers/
│   │   │   ├── CrearClienteHandlerTests.cs
│   │   │   ├── RegistrarDispositivoHandlerTests.cs
│   │   │   └── ProcesarEventoRfidHandlerTests.cs
│   │   └── Validators/
│   │       ├── CrearClienteValidatorTests.cs
│   │       └── RegistrarDispositivoValidatorTests.cs
│   └── Services/
│       ├── TokenServiceTests.cs
│       └── AuditServiceTests.cs
```

#### Ejemplo de Unit Test

```csharp
// tests/HorusEye.UnitTests/Application/Handlers/ProcesarEventoRfidHandlerTests.cs
public class ProcesarEventoRfidHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<ProcesarEventoRfidHandler>> _loggerMock;
    private readonly ProcesarEventoRfidHandler _handler;
    
    public ProcesarEventoRfidHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ProcesarEventoRfidHandler>>();
        _handler = new ProcesarEventoRfidHandler(
            _unitOfWorkMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task Handle_TagNoRegistrado_LanzaExcepcion()
    {
        // Arrange
        var evento = new RfidEvent
        {
            EPC = "E280116020002081104504794",
            DeviceId = Guid.NewGuid()
        };
        
        _unitOfWorkMock.Setup(x => x.Tags.ObtenerPorEpcAsync(It.IsAny<string>()))
            .ReturnsAsync((Tag)null);
        
        // Act & Assert
        await Assert.ThrowsAsync<TagNoRegistradoException>(
            () => _handler.Handle(evento, CancellationToken.None));
    }
    
    [Fact]
    public async Task Handle_TagValido_CreaMovimiento()
    {
        // Arrange
        var tag = new Tag { Id = "EPC123", Estado = EstadoTag.ASIGNADO };
        var activo = new Activo { Id = Guid.NewGuid(), Placa = "NB-001" };
        tag.Activo = activo;
        
        _unitOfWorkMock.Setup(x => x.Tags.ObtenerPorEpcAsync(It.IsAny<string>()))
            .ReturnsAsync(tag);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);
        
        var evento = new RfidEvent
        {
            EPC = "EPC123",
            DeviceId = Guid.NewGuid(),
            TipoMovimiento = "INGRESO"
        };
        
        // Act
        var resultado = await _handler.Handle(evento, CancellationToken.None);
        
        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("INGRESO", resultado.TipoMovimiento);
        _unitOfWorkMock.Verify(x => x.Movimientos.AddAsync(It.IsAny<Movimiento>()), Times.Once);
    }
}
```

### 21.3 Integration Tests (WebApplicationFactory)

#### Configuracion

```csharp
// tests/HorusEye.IntegrationTests/IntegrationTestBase.cs
public class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    
    public IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        Factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Reemplazar BD real con TestContainers
                services.AddDbContext<HorusEyeDbContext>(options =>
                    options.UseNpgsql(TestContainerHelper.ConnectionString));
                
                // Mock de servicios externos
                services.AddScoped<IRfidDeviceService, MockRfidDeviceService>();
            });
        });
        
        Client = Factory.CreateClient();
    }
}
```

#### Ejemplo de Integration Test

```csharp
// tests/HorusEye.IntegrationTests/Controllers/ClientesControllerTests.cs
public class ClientesControllerTests : IntegrationTestBase
{
    public ClientesControllerTests(WebApplicationFactory<Program> factory) 
        : base(factory) { }
    
    [Fact]
    public async Task ObtenerCliente_ClienteExiste_RetornaOk()
    {
        // Arrange
        var cliente = await CrearClienteAsync("Empresa Test", "1234567890123");
        
        // Act
        var response = await Client.GetAsync($"/api/clientes/{cliente.Id}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<ClienteResponse>();
        Assert.Equal("Empresa Test", content.Nombre);
    }
    
    [Fact]
    public async Task CrearCliente_DatosValidos_RetornaCreated()
    {
        // Arrange
        var command = new CrearClienteCommand
        {
            Nombre = "Nueva Empresa",
            RUC = "1234567890123"
        };
        
        // Act
        var response = await Client.PostAsJsonAsync("/api/clientes", command);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
```

### 21.4 E2E Tests (Playwright)

#### Configuracion

```typescript
// playwright.config.ts
import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './tests/e2e',
  timeout: 30000,
  retries: 2,
  use: {
    baseURL: 'http://localhost:5173',
    screenshot: 'only-on-failure',
    trace: 'on-first-retry',
  },
  projects: [
    { name: 'chromium', use: { browserName: 'chromium' } },
    { name: 'firefox', use: { browserName: 'firefox' } },
    { name: 'webkit', use: { browserName: 'webkit' } },
  ],
  webServer: {
    command: 'npm run dev',
    port: 5173,
    reuseExistingServer: !process.env.CI,
  },
});
```

#### Ejemplo de E2E Test

```typescript
// tests/e2e/clientes.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Gestion de Clientes', () => {
  test.beforeEach(async ({ page }) => {
    // Login como administrador
    await page.goto('/login');
    await page.fill('[data-testid="email"]', 'admin@horuseye.com');
    await page.fill('[data-testid="password"]', 'Admin123!');
    await page.click('[data-testid="login-button"]');
    await page.waitForURL('/dashboard');
  });

  test('Crear nuevo cliente', async ({ page }) => {
    // Navegar a clientes
    await page.click('[data-testid="menu-clientes"]');
    await expect(page).toHaveURL('/clientes');
    
    // Click en crear
    await page.click('[data-testid="btn-crear-cliente"]');
    
    // Llenar formulario
    await page.fill('[data-testid="input-nombre"]', 'Empresa Test E2E');
    await page.fill('[data-testid="input-ruc"]', '1234567890123');
    
    // Guardar
    await page.click('[data-testid="btn-guardar"]');
    
    // Verificar exito
    await expect(page.locator('[data-testid="toast-exito"]')).toBeVisible();
    await expect(page.locator('text=Empresa Test E2E')).toBeVisible();
  });

  test('Eliminar cliente', async ({ page }) => {
    await page.click('[data-testid="menu-clientes"]');
    
    // Seleccionar primer cliente
    await page.click('[data-testid="btn-eliminar-0"]');
    
    // Confirmar eliminacion
    await page.click('[data-testid="btn-confirmar-eliminar"]');
    
    // Verificar
    await expect(page.locator('[data-testid="toast-exito"]')).toBeVisible();
  });
});
```

### 21.5 SonarQube (Calidad de Codigo)

#### Configuracion

```yaml
# sonar-project.properties
sonar.projectKey=horuseye-api
sonar.projectName=HorusEye API
sonar.projectVersion=1.0

sonar.sources=src
sonar.tests=tests
sonar.cs.opencover.reportsPaths=**/coverage.opencover.xml

sonar.host.url=http://sonarqube:9000
sonar.login=${SONAR_TOKEN}
```

#### Metricas de Calidad

| Metrica | Meta | Descripcion |
|---------|------|-------------|
| **Duplicacion de codigo** | < 3% | Codigo duplicado |
| **Cobertura de codigo** | > 70% | Lineas cubiertas por tests |
| **Bugs** | 0 | Errores criticos |
| **Vulnerabilidades** | 0 | Problemas de seguridad |
| **Code Smells** | < 50 | Malas practicas |
| **Technical Debt** | < 5 dias | Tiempo estimado de deuda |

#### Dashboard SonarQube

```
┌─────────────────────────────────────────────────────────────┐
│                    SONARQUBE DASHBOARD                        │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐   │
│  │  HEALTH  │  │  BUGS    │  │VULNS     │  │  DEBT    │   │
│  │  ● A     │  │  0       │  │  0       │  │  2d      │   │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘   │
│                                                               │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  COBERTURA: 78% ████████████████████░░░░░░           │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                               │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  DUPLICACION: 2.1% ██░░░░░░░░░░░░░░░░░░░░           │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

### 21.6 CI/CD Pipeline (GitHub Actions)

#### Pipeline Completo

```yaml
# .github/workflows/ci.yml
name: CI/CD Pipeline

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore --configuration Release
      
      - name: Unit Tests
        run: dotnet test --no-build --configuration Release --collect:"XPlat Code Coverage"
      
      - name: SonarQube Analysis
        uses: sonarqube/sonarqube-scan-action@master
        env:
          SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
      
      - name: Publish
        if: github.ref == 'refs/heads/main'
        run: dotnet publish -c Release -o ./publish

  deploy:
    needs: build
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    
    steps:
      - name: Deploy to OCI VM
        uses: appleboy/ssh-action@master
        with:
          host: ${{ secrets.OCI_HOST }}
          username: ubuntu
          key: ${{ secrets.OCI_SSH_KEY }}
          script: |
            cd /opt/horuseye
            git pull
            docker compose build api
            docker compose up -d

  deploy-frontend:
    needs: build
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    
    steps:
      - name: Deploy to Vercel
        uses: amondnet/vercel-action@v25
        with:
          vercel-token: ${{ secrets.VERCEL_TOKEN }}
          vercel-org-id: ${{ secrets.VERCEL_ORG_ID }}
          vercel-project-id: ${{ secrets.VERCEL_PROJECT_ID }}
          working-directory: Frontends/ReactTS
```

### 21.7 Despliegue

#### Entornos

| Entorno | URL | Proposito |
|---------|-----|-----------|
| **Desarrollo** | localhost | Desarrollo local |
| **Staging** | staging.horuseye.com | Pruebas pre-produccion |
| **Produccion** | horuseye.mauricioadachi.dev | Produccion |

#### Estrategia de Despliegue

```
┌─────────────────────────────────────────────────────────────┐
│                    ESTRATEGIA DE DESPLIEGUE                   │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  1. DESARROLLADOR hace push a develop                         │
│     │                                                        │
│     ▼                                                        │
│  2. GITHUB ACTIONS ejecuta CI                                │
│     ├── Build                                                │
│     ├── Unit Tests                                           │
│     ├── SonarQube Analysis                                   │
│     └── Code Coverage                                        │
│     │                                                        │
│     ▼                                                        │
│  3. DEPLOY A STAGING (automatico)                            │
│     ├── Backend → Docker en OCI VM                           │
│     └── Frontend → Vercel preview                            │
│     │                                                        │
│     ▼                                                        │
│  4. QA VERIFICA en staging                                   │
│     │                                                        │
│     ▼                                                        │
│  5. MERGE a main                                             │
│     │                                                        │
│     ▼                                                        │
│  6. DEPLOY A PRODUCCION (automatico)                         │
│     ├── Backend → Docker en OCI VM                           │
│     └── Frontend → Vercel produccion                         │
│     │                                                        │
│     ▼                                                        │
│  7. MONITOREO post-despliegue                                │
│     ├── Health checks                                        │
│     ├── Logs                                                 │
│     └── Metricas                                             │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

### 21.8 Checklist Pre-Despliegue

| Area | Checklist | Estado |
|------|-----------|--------|
| **Build** | Build exitoso sin warnings | Pendiente |
| **Tests** | Todos los tests pasan | Pendiente |
| **Coverage** | Coverage > 70% | Pendiente |
| **SonarQube** | No bugs criticos | Pendiente |
| **SonarQube** | No vulnerabilidades | Pendiente |
| **Security** | No secrets en codigo | Pendiente |
| **Database** | Migraciones aplicadas | Pendiente |
| **Config** | Variables de entorno configuradas | Pendiente |
| **Logs** | Logging configurado | Pendiente |
| **Health** | Health checks funcionando | Pendiente |

### 21.9 Rollback

```bash
# En caso de problemas, rollback rapido
cd /opt/horuseye

# Rollback a version anterior
git checkout <commit-anterior>
docker compose build api
docker compose up -d

# Verificar
docker ps
curl http://localhost:8081/health
```

### 21.10 Resumen de Testing y Despliegue

| Area | Herramienta | Estado |
|------|-------------|--------|
| **Unit Tests** | xUnit + Moq | ❌ Pendiente |
| **Integration Tests** | WebApplicationFactory | ❌ Pendiente |
| **E2E Tests** | Playwright | ❌ Pendiente |
| **Code Quality** | SonarQube | ❌ Pendiente |
| **CI/CD** | GitHub Actions | ⚠️ Basico |
| **Deploy Backend** | Docker + OCI VM | ✅ Implementado |
| **Deploy Frontend** | Vercel | ✅ Implementado |
| **Monitoring** | Health Checks | ⚠️ Basico |

