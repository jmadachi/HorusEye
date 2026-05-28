# 👁️ HorusEye

**HorusEye** es una solución integral de software diseñada para el monitoreo, control y registro en tiempo real de las entradas y salidas de productos en múltiples bodegas, utilizando tecnología de lectura **RFID (Radio Frequency Identification)**. 

El sistema centraliza las lecturas automatizadas del hardware y las proyecta de manera instantánea en un dashboard dinámico, garantizando visibilidad total y trazabilidad del inventario.

---

## 🚀 Características Principales

*   **Dashboard en Tiempo Real:** Visualización instantánea de movimientos (Entradas/Salidas) analizados por bodega.
*   **Integración RFID Nativa:** Arquitectura desacoplada lista para conectar con hardware (como el ecosistema Keonn / AdvanNet) mediante Webhooks HTTP o MQTT.
*   **Filtro Anti-Duplicados (*De-bounce*):** Mecanismo inteligente en memoria para mitigar lecturas redundantes de tags estáticos.
*   **Métricas e Indicadores Clave:** Resúmenes operativos por almacén, alertas de stock y registro histórico auditable.

---

## 🛠️ Stack Tecnológico

La aplicación está construida sobre una arquitectura moderna, robusta y de alta concurrencia:

*   **Base de Datos:** [PostgreSQL](https://postgresql.org) (Persistencia relacional optimizada con índices cronológicos y transacciones ACID).
*   **Backend:** [.NET 10](https://microsoft.com) (Web API de alto rendimiento con Entity Framework Core y SignalR para comunicación bidireccional).
*   **Frontend:** [React](https://react.dev) + [TypeScript](https://typescriptlang.org) (Interfaz de usuario tipada, segura y reactiva).
*   **Herramienta de Construcción:** [Vite](https://vitejs.dev) (Entorno de desarrollo ultra rápido para el frontend).
*   **Gestor de Paquetes:** [pnpm](https://pnpm.io) (Instalación eficiente, veloz y orientada a monorepositorios o proyectos modulares).

---

## 📁 Estructura del Proyecto

```text
horuseye/
├── horuseye-backend/        # API REST y Servicios en .NET 10
│   ├── HorusEye.Api/        # Controladores, Endpoints y SignalR Hubs
│   ├── HorusEye.Core/       # Entidades de dominio y Lógica de Negocio
│   └── HorusEye.Infrastructure/ # Contexto de PostgreSQL (EF Core) y Repositorios
│
└── horuseye-dashboard/      # Aplicación Frontend (React TS + Vite)
    ├── src/
    │   ├── components/      # Componentes reutilizables del Dashboard
    │   ├── hooks/           # Hooks personalizados (ej. useSignalR)
    │   └── services/        # Conexiones a la API de .NET
    ├── package.json
    └── pnpm-lock.yaml
```

---

## ⚡ Requisitos Previos

Antes de comenzar, asegúrate de tener instalado:
*   [.NET 10 SDK](https://microsoft.comdownload)
*   [PostgreSQL 16+](https://postgresql.orgdownload/)
*   [Node.js 20+](https://nodejs.org)
*   [pnpm](https://pnpm.ioinstallation) (`npm install -g pnpm`)

---

## 🔧 Configuración Inicial

### 1. Base de Datos (PostgreSQL)
1. Crea una base de datos llamada `horuseye_db`.
2. Configura tu cadena de conexión en el archivo `appsettings.json` del backend.

### 2. Levantar el Backend (.NET 10)
```bash
cd horuseye-backend/HorusEye.Api
dotnet restore
dotnet ef database update
dotnet run
```
*El servidor backend correrá por defecto en `http://localhost:5000` o `https://localhost:5001`.*

### 3. Levantar el Frontend (React TS)
```bash
cd horuseye-dashboard
pnpm install
pnpm dev
```
*El dashboard se abrirá automáticamente en tu navegador en `http://localhost:5173`.*

---

## 📡 Flujo de Datos del Sistema

1. **Captura:** El lector RFID detecta el tag del producto en la bodega.
2. **Recepción:** El hardware envía el evento mediante un `POST HTTP` o mensaje `MQTT` al backend de **.NET 10**.
3. **Procesamiento:** El backend valida el movimiento, aplica el filtro de rebote y lo registra en **PostgreSQL**.
4. **Difusión:** **SignalR** emite el nuevo registro instantáneamente hacia el frontend.
5. **Visualización:** El componente de **React TS** actualiza el dashboard en tiempo real sin recargar la página.

---
✒️ **HorusEye** - *Vigilancia y control absoluto de inventarios.*

