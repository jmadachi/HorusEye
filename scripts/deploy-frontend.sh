#!/usr/bin/env bash
set -euo pipefail

# ============================================================
# deploy-frontend.sh — Build & deploy React frontend to Vercel
# Usage:
#   ./scripts/deploy-frontend.sh              # build + deploy
#   ./scripts/deploy-frontend.sh --no-deploy  # build only
# ============================================================

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
cd "$PROJECT_ROOT"

FRONTEND_DIR="Frontends/ReactTS"

if [ ! -f "$FRONTEND_DIR/package.json" ]; then
    echo "ERROR: Frontend directory not found at $FRONTEND_DIR"
    exit 1
fi

echo "==> Installing frontend dependencies..."
cd "$FRONTEND_DIR"
pnpm install --frozen-lockfile

echo "==> Linting..."
pnpm lint

echo "==> Building..."
pnpm build

if [ "${1:-}" = "--no-deploy" ]; then
    echo "==> Build OK. Skipping deploy (--no-deploy)."
    exit 0
fi

echo "==> Deploying to Vercel..."
VERCEL_TOKEN="${VERCEL_TOKEN:-}"
if [ -z "$VERCEL_TOKEN" ]; then
    echo "ERROR: VERCEL_TOKEN not set."
    echo "    export VERCEL_TOKEN=<your-vercel-token>"
    exit 1
fi

npx vercel pull --yes --environment=production --token="$VERCEL_TOKEN"
npx vercel build --prod --token="$VERCEL_TOKEN"
npx vercel deploy --prebuilt --prod --token="$VERCEL_TOKEN"

echo "==> Deploy completed successfully."
