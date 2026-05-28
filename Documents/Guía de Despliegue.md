# Guía de Despliegue — HorusEye

> **Frontend:** Vercel (100% gratuito)
> **Backend + Base de Datos:** Render (plan free)
> **Repositorio:** https://github.com/jmadachi/HorusEye

---

## Índice

1. [Preparar el repositorio en GitHub](#1-preparar-el-repositorio-en-github)
2. [Desplegar Frontend en Vercel](#2-desplegar-frontend-en-vercel)
3. [Desplegar Backend + BD en Render](#3-desplegar-backend--bd-en-render)
4. [Configurar Variables de Entorno](#4-configurar-variables-de-entorno)
5. [Verificar el Despliegue](#5-verificar-el-despliegue)

---

## 1. Preparar el repositorio en GitHub

```bash
# Desde la raíz del proyecto
cd /home/mauricio/WorkSpace/Proyectos/Cloud/HorusEye

# Inicializar git
git init

# Agregar todo y hacer commit
git add .
git commit -m "Initial commit: HorusEye RFID Asset Control System"

# Conectar con el repositorio remoto
git remote add origin https://github.com/jmadachi/HorusEye.git

# Subir a GitHub
git push -u origin main
```

> Si tu rama por defecto es `master`, usa `git push -u origin master`.

---

## 2. Desplegar Frontend en Vercel

### 2.1 Conectar el repositorio

1. Ve a [vercel.com](https://vercel.com) e inicia sesión con tu GitHub.
2. Haz clic en **Add New → Project**.
3. Busca y selecciona el repositorio `jmadachi/HorusEye`.

### 2.2 Configurar el proyecto

| Campo | Valor |
|-------|-------|
| **Framework Preset** | Vite (se detecta automáticamente) |
| **Root Directory** | `Frontends/ReactTS` |
| **Build Command** | `pnpm build` |
| **Output Directory** | `dist` |

### 2.3 Variables de entorno

| Variable | Valor |
|----------|-------|
| `VITE_API_URL` | `https://horuseye-api.onrender.com` (la URL de Render, se define después) |

### 2.4 Desplegar

Haz clic en **Deploy**. Vercel usará el archivo `Frontends/ReactTS/vercel.json` para configurar los SPA rewrites automáticamente.

> **Nota:** Si no ves la opción de `pnpm` en Vercel, ve a tu proyecto en Vercel → **Settings** → **General** → **Node.js Version** y selecciona la más reciente (22.x). Vercel detecta `pnpm` automáticamente si existe `pnpm-lock.yaml` en el root directory seleccionado.

---

## 3. Desplegar Backend + BD en Render

Hay dos formas: **Blueprint (automática)** o **Manual**.

### 3.1 Opción A — Blueprint (recomendada)

1. Ve a [render.com](https://render.com) e inicia sesión con tu GitHub.
2. Haz clic en **New → Blueprint**.
3. Conecta el repositorio `jmadachi/HorusEye`.
4. Render leerá automáticamente el archivo `render.yaml` en la raíz y creará:
   - **Web Service:** `horuseye-api` (backend .NET 10 con Docker)
   - **PostgreSQL:** `horuseye-db` (base de datos)
5. Durante la creación, te pedirá configurar las variables marcadas como `sync: false`. La única es `Jwt__Key`.

### 3.2 Opción B — Manual

#### 3.2.1 Crear la base de datos PostgreSQL

1. Render Dashboard → **New → PostgreSQL**
2. Configurar:

| Campo | Valor |
|-------|-------|
| **Name** | `horuseye-db` |
| **Database** | `horuseyedb` |
| **User** | `horuseyeuser` |
| **Plan** | Free |

3. Render crea la BD y te entrega la **Internal Connection String** (la usaremos en el Web Service).

#### 3.2.2 Crear el Web Service

1. Render Dashboard → **New → Web Service**
2. Conectar el repositorio `jmadachi/HorusEye`
3. Configurar:

| Campo | Valor |
|-------|-------|
| **Name** | `horuseye-api` |
| **Runtime** | `Docker` |
| **Root Directory** | `Backends/WebApi` |
| **Dockerfile Path** | `./Dockerfile` |
| **Plan** | Free |

4. En **Environment Variables**, agregar:

| Variable | Valor |
|----------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | (pegar la Internal Connection String de PostgreSQL) |
| `Jwt__Key` | `HorusEyeSuperSecretKey2026!@#$%^&*()VeryLongSecure` |
| `Jwt__Issuer` | `HorusEyeAPI` |
| `Jwt__Audience` | `HorusEyeFrontend` |
| `CORS__AllowedOrigin` | `https://horuseye.vercel.app` (la URL de Vercel) |

5. Haz clic en **Deploy Web Service**.

---

## 4. Configurar Variables de Entorno

### 4.1 Resumen de variables necesarias

#### Backend (Render)

| Variable | Dónde se define | Propósito |
|----------|-----------------|-----------|
| `ASPNETCORE_ENVIRONMENT` | Dashboard Render | Modo producción |
| `ConnectionStrings__DefaultConnection` | Render (inyectada automática en Blueprint) | Conexión a PostgreSQL |
| `Jwt__Key` | Dashboard Render (secreta) | Firma de tokens JWT |
| `Jwt__Issuer` | Dashboard Render | Emisor del JWT |
| `Jwt__Audience` | Dashboard Render | Audiencia del JWT |
| `CORS__AllowedOrigin` | Dashboard Render | Origen permitido (Vercel) |

#### Frontend (Vercel)

| Variable | Dónde se define | Propósito |
|----------|-----------------|-----------|
| `VITE_API_URL` | Dashboard Vercel | URL base del backend |

### 4.2 Obtener la URL del backend

Render asigna una URL como `https://horuseye-api.onrender.com` al Web Service. Una vez desplegado:

1. Render Dashboard → Web Service → `horuseye-api`
2. Copiar la URL pública (ej: `https://horuseye-api.onrender.com`)
3. Ir a Vercel → Project → **Settings** → **Environment Variables**
4. Agregar `VITE_API_URL` con ese valor
5. **Re-deployar** el frontend en Vercel (nuevo commit o redeploy manual)

---

## 5. Verificar el Despliegue

### 5.1 Backend (API)

```bash
curl https://horuseye-api.onrender.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@horuseye.com","password":"Admin123!"}'
```

Respuesta esperada: `{ "success": true, "data": { "accessToken": "...", ... } }`

### 5.2 Frontend

Abrir `https://horuseye.vercel.app` en el navegador e iniciar sesión con:
- **Email:** `admin@horuseye.com`
- **Contraseña:** `Admin123!`

### 5.3 SignalR (tiempo real)

Abrir el Dashboard y simular una lectura RFID:

```bash
curl -X POST https://horuseye-api.onrender.com/api/eventos-rfid \
  -H "Content-Type: application/json" \
  -d '{"tagId":"TAG-DEPLOY-001","puntoLecturaId":"PUERTA-PRINCIPAL","tipoMovimiento":"INGRESO"}'
```

El movimiento debería aparecer en tiempo real en el Dashboard.

---

## Notas importantes

1. **Jwt__Key es secreta:** No la subas al repositorio. Configúrala directamente en el dashboard de Render.
2. **Render Free Tier:** El servicio web se apaga después de 15 minutos sin uso. La primera solicitud después de inactividad tarda ~30-60 segundos en responder (cold start). Ideal para demos, no para producción real.
3. **CORS:** Si cambia la URL de Vercel (por dominio personalizado), actualiza `CORS__AllowedOrigin` en Render.
4. **Puerto:** Render espera que la aplicación escuche en el puerto `8080`. El Dockerfile ya lo configura con `ASPNETCORE_URLS=http://+:8080`.
