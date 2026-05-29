#!/usr/bin/env bash
set -euo pipefail

# ============================================================
# deploy-backend.sh — Build & push Docker image for Render
# Usage:
#   ./scripts/deploy-backend.sh            # build + push
#   ./scripts/deploy-backend.sh --no-push  # build only (validate Dockerfile)
# ============================================================

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
cd "$PROJECT_ROOT"

IMAGE_NAME="ttl.sh/horuseye-api:latest"

echo "==> Building backend .NET solution..."
dotnet restore Backends/WebApi/HorusEye.Api/HorusEye.Api.csproj
dotnet build Backends/WebApi/HorusEye.Api/HorusEye.Api.csproj --configuration Release --no-restore

echo "==> Running backend tests..."
dotnet test Backends/WebApi/HorusEye.Tests/HorusEye.Tests.csproj --configuration Release --no-restore

echo "==> Building Docker image: $IMAGE_NAME"
docker build -f Dockerfile -t "$IMAGE_NAME" .

if [ "${1:-}" = "--no-push" ]; then
    echo "==> Build OK. Skipping push (--no-push)."
    exit 0
fi

echo "==> Pushing image to $IMAGE_NAME"
docker push "$IMAGE_NAME"

echo "==> Triggering Render deploy hook..."
RENDER_HOOK="${RENDER_DEPLOY_HOOK:-}"
if [ -n "$RENDER_HOOK" ]; then
    curl -s -X POST \
        -H "Authorization: Bearer $RENDER_HOOK" \
        -H "Accept: application/json" \
        "https://api.render.com/v1/services/$RENDER_HOOK/deploys"
    echo ""
    echo "==> Deploy triggered successfully."
else
    echo "==> WARNING: RENDER_DEPLOY_HOOK not set. Skipping deploy trigger."
    echo "    Set it in your shell or CI env:"
    echo "    export RENDER_DEPLOY_HOOK=<your-deploy-hook-url>"
fi
