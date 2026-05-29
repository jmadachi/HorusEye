#!/bin/bash
# Script de simulación de Autorizaciones de Salida
# Uso: bash Documents/simulacion-autorizaciones.sh
# Requiere: curl

API="https://horuseye-api.onrender.com"

echo "=========================================="
echo "  Simulación de Autorizaciones de Salida"
echo "=========================================="

echo -e "\n=== 1. Login ==="
LOGIN=$(curl -s -X POST "$API/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@horuseye.com","password":"Admin123!"}')

TOKEN=$(echo "$LOGIN" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
if [ -z "$TOKEN" ]; then
  echo "ERROR: No se pudo autenticar"
  exit 1
fi
echo "OK"

echo -e "\n=== 2. Obtener activos con tag RFID ==="
ACTIVOS_JSON=$(curl -s "$API/api/activos?pageSize=5" -H "Authorization: Bearer $TOKEN")
echo "$ACTIVOS_JSON" | python3 -c "
import sys,json
d=json.load(sys.stdin)
items=d['data'].get('items',d['data'])
for i,a in enumerate(items[:5]):
    print(f'  [{i+1}] {a[\"placa\"]} - {a[\"nombre\"]} (tag: {a[\"tagId\"]})')
"

echo -e "\n=== 3. Crear autorizaciones de salida ==="
echo "$ACTIVOS_JSON" | python3 -c "
import sys,json,subprocess

d=json.load(sys.stdin)
items=d['data'].get('items',d['data'])
token = '$TOKEN'
api = '$API'

for a in items[:3]:
    payload = {
        'activoId': a['id'],
        'autorizadoPor': 'Cesar Moreno',
        'fechaVencimiento': '2026-06-15T23:59:59Z'
    }
    result = subprocess.run([
        'curl', '-s', '-X', 'POST', f'{api}/api/autorizaciones',
        '-H', 'Content-Type: application/json',
        '-H', f'Authorization: Bearer {token}',
        '-d', json.dumps(payload)
    ], capture_output=True, text=True)
    resp = json.loads(result.stdout)
    if resp.get('success'):
        print(f'  ✅ {a[\"placa\"]} - {a[\"nombre\"]} → AUTORIZADO (vence: 2026-06-15)')
    else:
        print(f'  ⚠️  {a[\"placa\"]} → {resp.get(\"message\",\"error\")}')
"

echo -e "\n=== 4. Consultar autorizaciones creadas ==="
curl -s "$API/api/autorizaciones?pageSize=10" -H "Authorization: Bearer $TOKEN" | python3 -c "
import sys,json
d=json.load(sys.stdin)
items=d['data'].get('items',d['data'])
print(f'  Total autorizaciones: {d[\"data\"].get(\"total\",len(items))}')
for a in items[:5]:
    vence = a['fechaVencimiento'] or 'sin vencimiento'
    estado = '✅ Activa' if a['activa'] else '❌ Revocada'
    print(f'  {estado} | {a[\"activoPlaca\"]} - {a[\"activoNombre\"]} | Autorizado por: {a[\"autorizadoPor\"]} | Vence: {vence}')
"

echo -e "\n=== 5. Probar SALIDA AUTORIZADA (no debería activar alarma) ==="
echo "  TAG del primer activo autorizado:"
curl -s "$API/api/eventos-rfid" \
  -H "Content-Type: application/json" \
  -d '{"tagId":"TAG-BULK-100","puntoLecturaId":"PUERTA-PRINCIPAL","tipoMovimiento":"SALIDA"}' | python3 -c "
import sys,json
r=json.load(sys.stdin)
print(f'  {'🔴 ALARMA!' if r.get('activarAlarmaSonora') else '✅ Salida autorizada, sin alarma'}')
print(f'  Mensaje: {r.get('mensaje','')}')
"

echo -e "\n=== 6. Probar SALIDA SIN AUTORIZACIÓN (debería activar alarma) ==="
echo "  Usando TEL-097 / TAG-BULK-097 (NO autorizado):"
curl -s "$API/api/eventos-rfid" \
  -H "Content-Type: application/json" \
  -d '{"tagId":"TAG-BULK-097","puntoLecturaId":"PUERTA-PRINCIPAL","tipoMovimiento":"SALIDA"}' | python3 -c "
import sys,json
r=json.load(sys.stdin)
print(f'  {'🔴 ALARMA ACTIVADA!' if r.get('activarAlarmaSonora') else '✅ Sin alarma'}')
print(f'  Mensaje: {r.get('mensaje','')}')
"

echo -e "\n=== 7. Resumen en dashboard ==="
curl -s "$API/api/dashboard/kpis" -H "Authorization: Bearer $TOKEN" | python3 -c "
import sys,json
r=json.load(sys.stdin)
d=r['data']
print(f'  Total activos: {d[\"totalActivos\"]}  |  Ingresos hoy: {d[\"ingresosHoy\"]}  |  Salidas hoy: {d[\"salidasHoy\"]}')
"

echo ""
echo "✅ Simulación completada."
echo "   Abre https://horus-eye-kappa.vercel.app/autorizaciones para ver las autorizaciones."
echo "   Ve al Dashboard para ver los movimientos en tiempo real."
