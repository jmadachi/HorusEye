#!/usr/bin/env bash
set -euo pipefail

# ============================================================
# health-check.sh — Verify Render + Vercel deployments
# ============================================================

API_URL="https://horuseye-api.onrender.com"
FRONTEND_URL="https://horus-eye-kappa.vercel.app"

echo "=========================================="
echo "  HorusEye — Health Check"
echo "=========================================="
echo ""

check() {
    local name="$1" url="$2" method="${3:-GET}" data="${4:-}" auth="${5:-}"
    local status
    if [ -n "$data" ] && [ -n "$auth" ]; then
        status=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 -X "$method" -H "Content-Type: application/json" -H "$auth" -d "$data" "$url")
    elif [ -n "$data" ]; then
        status=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 -X "$method" -H "Content-Type: application/json" -d "$data" "$url")
    elif [ -n "$auth" ]; then
        status=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 -X "$method" -H "$auth" "$url")
    else
        status=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 "$url")
    fi
    if [ "$status" = "000" ]; then
        echo "  ❌ $name — UNREACHABLE ($url)"
        return 1
    elif [ "$status" -ge 200 ] && [ "$status" -lt 500 ]; then
        echo "  ✅ $name — HTTP $status"
    else
        echo "  ⚠️  $name — HTTP $status"
    fi
}

check "Frontend"    "$FRONTEND_URL"
check "API Root"    "$API_URL/"
check "API Login"   "$API_URL/api/auth/login" POST '{"email":"admin@horuseye.com","password":"Admin123!"}'

# Get token and check authenticated endpoints
echo ""
echo "--- Authenticated Endpoints ---"
TOKEN=$(curl -s -X POST "$API_URL/api/auth/login" \
    -H "Content-Type: application/json" \
    -d '{"email":"admin@horuseye.com","password":"Admin123!"}' 2>/dev/null \
    | python3 -c "import sys,json; print(json.load(sys.stdin)['data']['accessToken'])" 2>/dev/null || true)

if [ -n "$TOKEN" ]; then
    echo "  Token obtained: ${TOKEN:0:20}..."
    check "Dashboard KPIs" "$API_URL/api/dashboard/kpis" GET "" "Authorization: Bearer $TOKEN"
    check "Activos"        "$API_URL/api/activos"        GET "" "Authorization: Bearer $TOKEN"
    check "Tags"           "$API_URL/api/tags"           GET "" "Authorization: Bearer $TOKEN"
    check "Autorizaciones" "$API_URL/api/autorizaciones" GET "" "Authorization: Bearer $TOKEN"
    echo ""
    echo "  ✅ JWT Auth — Working"
else
    echo "  ❌ JWT Auth — FAILED (could not obtain token)"
fi

echo ""
echo "=========================================="
echo "  Health check completed!"
echo "=========================================="
