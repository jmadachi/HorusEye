# Plan de Infraestructura de implementación.

Para optimizar al máximo los recursos y no pagar nada (o pagar lo mínimo), la estructura ideal para montar tu demo con el stack actual es:                  
```
                  ┌──────────────────────────────────────────┐
                  │                 GITHUB                   │
                  └────┬────────────────────────────────┬────┘
                       │                                │
                       ▼ (Despliegue Automático)        ▼ (Docker Build)
          ┌─────────────────────────┐     ┌────────────────────────────┐
          │         VERCEL          │     │    RENDER o RAILWAY        │
          │  (Plan 100% Gratuito)   │     │ (Plan Base / Desarrollador)│ 
          ├─────────────────────────┤     ├────────────────────────────┤
          │  • Frontend (React TS)  │     │  • Backend (.NET 10)       │
          │  • Archivos estáticos   │     │  • Base de Datos (Postgres)│
          └────────────┬────────────┘     └────────────┬───────────────┘
                       │                               │
                       │     (Peticiones API/SignalR)  │
                       └───────────────────────────────┘
```

El Frontend va a Vercel (Sin Docker): Subes la carpeta de React. Vercel compila el proyecto usando pnpm build de forma nativa en su infraestructura. El consumo para ti es $0 USD y la velocidad de carga para el cliente será inmediata.El Backend y la BD van a Render/Railway (Con Docker): Subes tu Dockerfile y tu docker-compose.yml para empaquetar la API de .NET 10 y PostgreSQL. Todo tu presupuesto o recursos gratuitos se concentran exclusivamente en el procesamiento del backend y del tiempo real.
