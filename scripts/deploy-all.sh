#!/usr/bin/env bash
set -euo pipefail

# ============================================================
# deploy-all.sh — Full CI/CD pipeline (local execution)
# Builds, tests, and deploys both backend and frontend.
# ============================================================

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
cd "$PROJECT_ROOT"

echo "=========================================="
echo "  HorusEye — Full Deploy Pipeline"
echo "=========================================="
echo ""

echo "╔══════════════════════════════════════════════╗"
echo "║  1/4 — Backend: Build & Test                ║"
echo "╚══════════════════════════════════════════════╝"
dotnet restore Backends/WebApi/HorusEye.Api/HorusEye.Api.csproj
dotnet build Backends/WebApi/HorusEye.Api/HorusEye.Api.csproj --configuration Release --no-restore
dotnet test Backends/WebApi/HorusEye.Tests/HorusEye.Tests.csproj --configuration Release --no-restore
echo ""

echo "╔══════════════════════════════════════════════╗"
echo "║  2/4 — Frontend: Install, Lint & Build      ║"
echo "╚══════════════════════════════════════════════╝"
cd Frontends/ReactTS
pnpm install --frozen-lockfile
pnpm lint
pnpm build
cd "$PROJECT_ROOT"
echo ""

echo "╔══════════════════════════════════════════════╗"
echo "║  3/4 — Docker: Build & Push Backend         ║"
echo "╚══════════════════════════════════════════════╝"
IMAGE_NAME="ttl.sh/horuseye-api:latest"
docker build -f Dockerfile -t "$IMAGE_NAME" .
docker push "$IMAGE_NAME"
echo ""

echo "╔══════════════════════════════════════════════╗"
echo "║  4/4 — Deploy to Render & Vercel            ║"
echo "╚══════════════════════════════════════════════╝"

# Render deploy hook
RENDER_HOOK="${RENDER_DEPLOY_HOOK:-}"
if [ -n "$RENDER_HOOK" ]; then
    echo "==> Triggering Render deploy..."
    curl -s -X POST \
        -H "Authorization: Bearer $RENDER_HOOK" \
        -H "Accept: application/json" \
        "https://api.render.com/v1/services/$RENDER_HOOK/deploys"
    echo ""
else
    echo "==> WARNING: RENDER_DEPLOY_HOOK not set. Skipping Render deploy."
fi

# Vercel deploy
VERCEL_TOKEN="${VERCEL_TOKEN:-}"
if [ -n "$VERCEL_TOKEN" ]; then
    echo "==> Deploying frontend to Vercel..."
    cd Frontends/ReactTS
    npx vercel pull --yes --environment=production --token="$VERCEL_TOKEN"
    npx vercel build --prod --token="$VERCEL_TOKEN"
    npx vercel deploy --prebuilt --prod --token="$VERCEL_TOKEN"
    cd "$PROJECT_ROOT"
else
    echo "==> WARNING: VERCEL_TOKEN not set. Skipping Vercel deploy."
fi

echo ""
echo "=========================================="
echo "  Deploy pipeline completed!"
echo "=========================================="
