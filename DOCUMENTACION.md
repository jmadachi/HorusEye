# HorusEye - Control de Inventarios RFID

> Documentación de estado actual. Última actualización: 2026-07-05

## Resumen

HorusEye es un sistema de control de inventarios y activos en tiempo real mediante lectores RFID. Rastrea entrada/salida de productos en múltiples almacenes usando lectores RFID (ecosistema Keonn/AdvanNet), proyecta datos en un dashboard dinámico y proporciona auditorías.

## URLs de Producción

| Servicio | URL |
|----------|-----|
| Frontend | https://horuseye-app.mauricioadachi.dev |
| API | https://horuseye-api.mauricioadachi.dev |

## Stack Tecnológico

- **Backend**: .NET 10 + Entity Framework Core + SignalR
- **Frontend**: React 19 + TypeScript + Vite 8 + Tailwind CSS 4
- **Base de datos**: PostgreSQL 18
- **Despliegue Backend**: Oracle Cloud VM (Docker)
- **Despliegue Frontend**: Vercel

## Infraestructura

### Oracle Cloud (OCI) - Compartida con EBM
- **VM**: instance-20260703-1405 (ARM, 4 OCPUs / 24 GB RAM)
- **IP Pública**: 129.146.22.246
- **SSH**: `ssh -i ~/.ssh/id_rsa_oci ubuntu@129.146.22.246`

### Contenedores Docker
| Contenedor | Servicio | Puerto |
|------------|----------|--------|
| horuseye-api | API .NET | 8081 → 8080 (interno) |
| horuseye-db | PostgreSQL | 5433 → 5432 (interno) |
| horuseye-tunnel | Cloudflare Tunnel | (compartido) |

### Redes Docker
- HorusEye usa `horuseye_default`
- `horuseye-api` conectado también a `dotnet10_default` (para tunnel)

### Cloudflare
- **Tunnel**: ebm-tunnel (compartido con EBM)
- **Ruta API**: `horuseye-api.mauricioadachi.dev` → `http://horuseye-api:8080`
- **Ruta EBM**: `api.mauricioadachi.dev` → `http://ebm-api:8080`

## Arquitectura de Red

```
Usuario → HTTPS → Vercel (Frontend)
                     ↓ API calls
                  HTTPS → Cloudflare → HTTP → horuseye-api:8080 (Docker)
                                                ↓
                                             PostgreSQL
```

## Repositorio

- **GitHub**: `jmadachi/HorusEye` (privado)
- **Dockerfile**: En raíz del proyecto

## Estructura del Proyecto

```
HorusEye/
├── Backends/
│   └── WebApi/
│       ├── HorusEye.slnx
│       ├── HorusEye.Api/          # API REST + SignalR
│       ├── HorusEye.Core/         # Entidades y enums
│       ├── HorusEye.Infrastructure/ # EF Core, migraciones
│       └── HorusEye.Tests/
├── Frontends/
│   └── ReactTS/
│       ├── vercel.json
│       ├── .env.production
│       └── src/
│           ├── pages/             # Dashboard, Activos, Tags, etc.
│           ├── components/        # Layout, ProtectedRoute
│           ├── context/           # AuthContext, ThemeContext
│           ├── services/          # api.ts, signalR.ts
│           └── hooks/             # useSignalR
├── Databases/
│   └── PostgreSQL/
│       ├── docker-compose.yml
│       └── init.sql
├── docker-compose.yml             # Despliegue en OCI VM
├── Dockerfile                     # Build multi-stage
├── render.yaml                    # Config Render (legacy)
└── scripts/                       # Scripts de simulación
```

## Módulos Backend

| Módulo | Descripción |
|--------|-------------|
| Auth | Login, registro, JWT, refresh tokens, 7 roles RBAC |
| Usuarios | CRUD de usuarios con roles jerarquicos |
| Activos | Gestión de activos/inventario |
| Tags | Tags RFID y estado |
| Autorizaciones | Autorizaciones de salida |
| Movimientos | Registro de entradas/salidas |
| Dashboard | KPIs y tendencias |
| Reportes | Generación de reportes |
| Proveedores | CRUD de proveedores RFID |
| Clientes | CRUD de clientes con aislamiento por proveedor |
| Dispositivos | CRUD de dispositivos RFID configurables |
| Ubicaciones | Arbol jerarquico configurable por cliente |
| Fabricantes | Gestion de fabricantes y mapeo JSON de payloads |
| RFID Providers | Provider Pattern (Chainway + Generico configurable) |

## Páginas Frontend

| Página | Descripción |
|--------|-------------|
| Dashboard | Métricas en tiempo real |
| Activos | Gestión de activos |
| Tags | Tags RFID |
| Autorizaciones | Autorizaciones de salida |
| Usuarios | Gestión de usuarios (7 roles) |
| Proveedores | CRUD de proveedores |
| Clientes | CRUD de clientes |
| Dispositivos | CRUD de dispositivos RFID |
| Ubicaciones | Arbol jerarquico de ubicaciones |
| Fabricantes | Gestion de fabricantes y campos JSON |
| Reportes | Generación de reportes |
| Login | Autenticación |

## Roles del Sistema (RBAC)

| Rol | Descripción | Permisos |
|-----|-------------|----------|
| Administrador del Sistema | Acceso total | CRUD completo, gestionar todo |
| Asistente del Administrador del Sistema | Asistente admin | Crear usuarios de proveedor/cliente, gestionar dispositivos |
| Soporte del Sistema | Soporte tecnico | Solo lectura global |
| Administrador del Proveedor | Admin de proveedor | Gestionar sus clientes y dispositivos |
| Asistente del Proveedor | Asistente de proveedor | Solo lectura dentro de su proveedor |
| Administrador del Cliente | Admin de cliente | Gestionar sus dispositivos y ubicaciones |
| Asistente del Cliente | Asistente de cliente | Solo lectura dentro de su cliente |

## Base de Datos

### Tablas (21)
- Tags, TagsDaniosHistorial
- Activos, Movimientos, AutorizacionesSalida
- Proveedores, Clientes
- DispositivosRfid, NodosUbicacion
- FabricantesDispositivo, CamposPayloadFabricante
- UsuariosExtendidos
- RefreshTokens
- AspNetUsers, AspNetRoles, AspNetUserRoles, etc.
- __EFMigrationsHistory

### Credenciales DB
| Campo | Valor |
|-------|-------|
| Host | horuseye-db (Docker) |
| Port | 5432 (interno) / 5433 (host) |
| Database | horuseyedb |
| User | horuseyeuser |
| Password | H0rvEy3 |

## Usuarios de Prueba

| Email | Contraseña | Rol |
|-------|------------|-----|
| admin@horuseye.com | Admin123! | Administrador del Sistema (acceso completo) |
| consulta@horuseye.com | Consulta123! | Asistente del Cliente (solo lectura) |

## Configuración JWT

| Campo | Valor |
|-------|-------|
| Issuer | HorusEyeAPI |
| Audience | HorusEyeFrontend |
| Key | HorusEyeSuperSecretKey2026!@#$%^&*()VeryLongSecure |

## Funcionalidades Clave

### RFID Event Flow
1. `POST /api/eventos-rfid` recibe lecturas del hardware (formato generico)
2. `POST /api/eventos-rfid/{provider}` recibe por proveedor (ej: `chainway`)
3. Verifica debounce (5s TTL en memoria)
4. Valida contra AutorizacionesSalida
5. Crea Movimiento (asocia DispositivoRfid si se identifica)
6. Broadcast via SignalR
7. Retorna estado de alarma

### Alarmas
Se disparan cuando:
- Tipo es SALIDA
- No hay AutorizacionSalida activa para el activo

### SignalR Hub
- Endpoint: `/hubs/movimientos`
- Auth: JWT via query string `access_token`
- Eventos: movimientos en tiempo real

## Despliegue

### Backend (OCI VM)
```bash
# Conectar
ssh -i ~/.ssh/id_rsa_oci ubuntu@129.146.22.246

# Directorio
cd /opt/horuseye

# Actualizar
git pull

# Reconstruir imagen (SIEMPRE usar --no-cache despues de cambios)
docker compose build --no-cache api

# Reiniciar
docker compose up -d

# Verificar
docker ps
curl http://localhost:8081/health
```

### Frontend (Vercel)
```bash
# Automatico: push a main -> Vercel auto-deploy
# Manual:
cd Frontends/ReactTS
vercel --prod
```

### Variables de Entorno Vercel
- `VITE_API_URL`: `https://horuseye-api.mauricioadachi.dev`

### Tunel Cloudflare
- El tunel es compartido con EBM (`ebm-tunnel`)
- Token en `/opt/ebm/Backends/Apis/dotNET10/.env`
- Copiar a `/opt/horuseye/.env` si se pierde: `CLOUDFLARE_TUNNEL_TOKEN=eyJh...`
- Reiniciar: `docker compose up -d cloudflared`

## Migraciones de BD

El contenedor API ejecuta migraciones automáticamente al iniciar (`MigrateAsync()` con fallback a `EnsureCreated()`).

Para migración manual:
```bash
# Generar script SQL
docker run --rm --network horuseye_default \
  -v /opt/horuseye/Backends/WebApi:/src \
  -w /src/HorusEye.Infrastructure \
  mcr.microsoft.com/dotnet/sdk:10.0 sh -c \
  'dotnet restore && \
   dotnet tool install --global dotnet-ef --version 10.0.9 2>/dev/null 1>/dev/null && \
   PATH="/root/.dotnet/tools:$PATH" dotnet ef migrations script -s ../HorusEye.Api --idempotent 2>/dev/null' \
  > /tmp/horuseye_migration.sql

# Aplicar
docker exec -i horuseye-db psql -U horuseyeuser -d horuseyedb < /tmp/horuseye_migration.sql
```

## Scripts de Simulación

- `simulacion-100.sh`: Simula 100 eventos RFID
- `simulacion-prod.sh`: Simulación con datos de producción
- `simulacion-prod-full.sh`: Simulación completa

## Notas Importantes

1. **Puerto 8081**: HorusEye usa 8081 en el host para evitar conflicto con EBM (8080)

2. **CORS**: La API permite `https://horuseye.vercel.app` y `https://horuseye-app.mauricioadachi.dev`

3. **SignalR**: El token JWT se pasa como query parameter `access_token` en WebSocket

4. **Enum serialization**: Controllers usan `JsonStringEnumConverter` para serializar enums como strings

5. **Npgsql timestamps**: `Npgsql.EnableLegacyTimestampBehavior = true` en Program.cs

6. **Render (legacy)**: El `render.yaml` existe pero ya no se usa. El backend está en OCI VM.

7. **Docker build**: SIEMPRE usar `--no-cache` despues de cambios en el codigo C# para evitar capas viejas.

8. **Tunel Cloudflare**: Compartido con EBM. Token en `/opt/ebm/Backends/Apis/dotNET10/.env`.

## Endpoints

### Autenticacion
| Metodo | Ruta | Auth | Descripcion |
|--------|------|------|-------------|
| POST | `/api/auth/login` | Publico | Login |
| POST | `/api/auth/register` | Admin/AsistAdmin | Registrar usuario |
| POST | `/api/auth/refresh-token` | Publico | Renovar JWT |
| POST | `/api/auth/change-password` | Authenticated | Cambiar contrasena |
| GET | `/api/auth/users` | Segun rol | Listar usuarios (paginado) |
| PUT | `/api/auth/users/{id}` | Segun rol | Actualizar usuario |
| DELETE | `/api/auth/users/{id}` | Segun rol | Eliminar usuario |

### Proveedores
| Metodo | Ruta | Auth | Descripcion |
|--------|------|------|-------------|
| GET | `/api/proveedores` | Authenticated | Listar (paginado) |
| POST | `/api/proveedores` | Admin/AsistAdmin | Crear |
| PUT | `/api/proveedores/{id}` | Admin/AsistAdmin | Actualizar |
| DELETE | `/api/proveedores/{id}` | AdminSistema | Eliminar |

### Clientes
| Metodo | Ruta | Auth | Descripcion |
|--------|------|------|-------------|
| GET | `/api/clientes` | Segun aislamiento | Listar (paginado) |
| POST | `/api/clientes` | Admin/AsistAdmin/AdminProv | Crear |
| PUT | `/api/clientes/{id}` | Segun aislamiento | Actualizar |
| DELETE | `/api/clientes/{id}` | AdminSistema | Eliminar |

### Dispositivos RFID
| Metodo | Ruta | Auth | Descripcion |
|--------|------|------|-------------|
| GET | `/api/dispositivos` | Segun aislamiento | Listar (paginado) |
| POST | `/api/dispositivos` | Admin/AsistAdmin/AdminProv/AdminCli | Crear |
| PUT | `/api/dispositivos/{id}` | Segun aislamiento | Actualizar |
| DELETE | `/api/dispositivos/{id}` | AdminSistema | Eliminar |

### Ubicaciones (Arbol Jerarquico)
| Metodo | Ruta | Auth | Descripcion |
|--------|------|------|-------------|
| GET | `/api/ubicaciones?clienteId=` | Segun aislamiento | Listar nodos |
| GET | `/api/ubicaciones/arbol?clienteId=` | Segun aislamiento | Arbol JSON |
| POST | `/api/ubicaciones` | Admin/AsistAdmin/AdminProv/AdminCli | Crear nodo |
| PUT | `/api/ubicaciones/{id}` | Segun aislamiento | Actualizar |
| DELETE | `/api/ubicaciones/{id}` | AdminSistema | Eliminar |

### Fabricantes
| Metodo | Ruta | Auth | Descripcion |
|--------|------|------|-------------|
| GET | `/api/fabricantes` | Admin/AsistAdmin/Soporte | Listar |
| POST | `/api/fabricantes` | Admin/AsistAdmin | Crear |
| PUT | `/api/fabricantes/{id}` | Admin/AsistAdmin | Actualizar |
| DELETE | `/api/fabricantes/{id}` | AdminSistema | Eliminar |
| POST | `/api/fabricantes/{id}/campos` | Admin/AsistAdmin | Agregar campo |
| DELETE | `/api/fabricantes/campo/{id}` | Admin/AsistAdmin | Eliminar campo |

### RFID
| Metodo | Ruta | Auth | Descripcion |
|--------|------|------|-------------|
| POST | `/api/eventos-rfid` | AllowAnonymous | Evento generico |
| POST | `/api/eventos-rfid/{provider}` | AllowAnonymous | Evento por proveedor |

### Otros
| Metodo | Ruta | Auth | Descripcion |
|--------|------|------|-------------|
| GET | `/api/activos` | Authenticated | Listar activos (paginado) |
| GET | `/api/tags` | Authenticated | Listar tags (paginado) |
| GET | `/api/autorizaciones` | Authenticated | Listar autorizaciones |
| GET | `/api/movimientos` | Authenticated | Listar movimientos |
| GET | `/api/dashboard/kpis` | Authenticated | KPIs |
| GET | `/api/health` | Publico | Health check |

## Pendientes

- [ ] Configurar backups de PostgreSQL
- [ ] Agregar health checks en docker-compose
- [ ] Monitoreo y alertas
- [ ] Migrar completamente de Render ( eliminar `render.yaml` si ya no se usa)
- [ ] Documentar API con Swagger/OpenAPI
