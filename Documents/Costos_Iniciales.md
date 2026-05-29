# Costos Iniciales — HorusEye

**Fecha:** 29-Mayo-2026

---

## Infraestructura Actual

### GitHub (Plan Free)

| Concepto | Costo |
|----------|-------|
| Repositorios privados ilimitados | $0/mes |
| Colaboradores ilimitados | $0/mes |
| GitHub Actions (2,000 min/mes) | $0/mes |
| **Total GitHub** | **$0/mes** |

### Vercel (Plan Hobby — Free)

| Concepto | Costo |
|----------|-------|
| Sitios estáticos ilimitados | $0/mes |
| Ancho de banda (100 GB/mes) | $0/mes |
| Builds (6,000 min/mes) | $0/mes |
| **Total Vercel** | **$0/mes** |

### Render

| Servicio | Plan | Costo |
|----------|------|-------|
| Web Service `horuseye-api` | Starter ($7) — 512 MB RAM, 0.5 CPU, sin spin-down | **$7/mes** |
| PostgreSQL `horuseye-db` | Basic-256mb ($6) — desde el mes 2 (30 días gratis iniciales) | **$6/mes** |
| Workspace | Hobby ($0) | **$0/mes** |
| **Total Render** | | **$13/mes** |

---

## Alternativa: Railway

Railway usa un modelo **híbrido**: suscripción base + consumo variable por segundo.

### Planes

| Plan | Mínimo/mes | Incluye |
|------|-----------|---------|
| Free | $0 | $5 de crédito único (trial, 30 días) |
| Hobby | **$5** | $5 de uso incluido |
| Pro | **$20** | $20 de uso incluido |

### Precios de recursos (estimado 24/7)

| Recurso | Precio |
|---------|--------|
| CPU | $20/vCPU/mes |
| RAM | $10/GB/mes |
| PostgreSQL | Se cobra como servicio normal (CPU + RAM) |
| Egress | $0.05/GB |
| Volumen | $0.15/GB/mes |

### Costo estimado para HorusEye en Railway (Hobby)

| Componente | Estimado |
|------------|----------|
| API (.NET, ~0.5 vCPU + 512 MB RAM) | ~$15/mes |
| PostgreSQL (~0.5 vCPU + 512 MB RAM) | ~$15/mes |
| Frontend (Vercel Free) | $0/mes |
| **Total estimado** (incluye $5 de crédito) | **~$25/mes** |

> Railway no tiene BD gratis permanente. Su plan Free es solo trial con $5 de crédito único.

---

## Comparativa Render vs Railway

| Concepto | Render | Railway |
|----------|--------|---------|
| API (0.5 CPU + 512 MB) | **$7 fijo** | ~$15 variable |
| BD PostgreSQL | **$6 fijo** | ~$15 variable |
| Frontend | Vercel $0 | Vercel $0 |
| Predictibilidad | **Alta** (precios fijos) | Baja (usage-based) |
| **Total/mes** | **$13** | **~$30** |

Railway puede ser más barato si la app se usa pocas horas al día, porque cobran por segundo. Pero HorusEye necesita estar siempre activa (SignalR + lectores RFID), lo que hace que Render sea más económico y predecible.

---

## Resumen

| Servicio | Costo |
|----------|-------|
| GitHub | $0/mes |
| Vercel | $0/mes |
| Render | $13/mes |
| **Total** | **$13/mes** |

---

## Proyección por Clientes

| Clientes | Frontend | Backend | BD | Total/mes |
|----------|----------|---------|----|-----------|
| 0–50 | Vercel $0 | $7 | $6 | **$13** |
| 50–200 | Vercel $20 | $7 | $6 | **$33** |
| 200+ | Según demanda (Azure / AWS / DigitalOcean) | | | |

---

## Notas

- **Vercel Free** no expira, es de por vida para proyectos como este (frontend estático, bajo tráfico).
- **Render Free PostgreSQL** expira a los 30 días. El costo real de arranque es $0 el primer mes y $13/mes del segundo en adelante.
- **Railway** es más caro para 24/7 por su modelo variable; Render gana en predictibilidad para este proyecto.
- HorusEye no necesita servicios costosos adicionales (CDN, WAF, balanceadores) a esta escala.
- **Dominio propio** (.com): ~$10–15/año si se desea uno personalizado (independiente de la infraestructura).
