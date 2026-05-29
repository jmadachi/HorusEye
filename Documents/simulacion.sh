#!/bin/bash
# Script de simulación HorusEye
# Uso: ./simulacion.sh
# Requiere: curl, jq (opcional para parsear JSON)

API="http://localhost:5000"

echo "=== 1. Login como admin ==="
LOGIN=$(curl -s -X POST "$API/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@horuseye.com","password":"Admin123!"}')

TOKEN=$(echo "$LOGIN" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)

if [ -z "$TOKEN" ]; then
  echo "ERROR: No se pudo autenticar"
  echo "$LOGIN"
  exit 1
fi
echo "Token obtenido: ${TOKEN:0:30}..."

echo -e "\n=== 2. Registrar tags ==="
for TAG in TAG-A001 TAG-A002 TAG-A003; do
  curl -s -X POST "$API/api/tags" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $TOKEN" \
    -d "{\"tagId\":\"$TAG\"}" | grep -o '"success":true' && echo "  Tag $TAG registrado" || echo "  Tag $TAG ya existe"
done

echo -e "\n=== 3. Poner tags como DISPONIBLES ==="
for TAG in TAG-A001 TAG-A002 TAG-A003; do
  curl -s -X PUT "$API/api/tags/$TAG/estado" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $TOKEN" \
    -d "{\"estado\":\"DISPONIBLE\"}" | grep -o '"success":true' && echo "  $TAG → DISPONIBLE"
done

echo -e "\n=== 4. Crear activos con tags asignados ==="
curl -s -X POST "$API/api/activos" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"placa":"LAP-DELL-001","nombre":"Laptop Dell Latitude 5540","categoria":"Computadores","tenedorResponsable":"Carlos Pérez","tagId":"TAG-A001"}'

curl -s -X POST "$API/api/activos" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"placa":"IMP-HP-002","nombre":"Impresora HP LaserJet Pro","categoria":"Impresoras","tenedorResponsable":"Ana Martínez","tagId":"TAG-A002"}'

curl -s -X POST "$API/api/activos" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"placa":"SILLA-003","nombre":"Silla Ergonómica","categoria":"Sillas","tenedorResponsable":"","tagId":"TAG-A003"}'

echo -e "\n=== 5. Simular lecturas RFID ==="
echo "-- INGRESO de LAP-DELL-001 (TAG-A001) --"
curl -s -X POST "$API/api/eventos-rfid" \
  -H "Content-Type: application/json" \
  -d '{"tagId":"TAG-A001","puntoLecturaId":"PUERTA-PRINCIPAL","tipoMovimiento":"INGRESO"}' | grep -o '"mensaje":"[^"]*"'

echo ""
echo "-- SALIDA de IMP-HP-002 (TAG-A002) SIN autorización (debería activar alarma) --"
curl -s -X POST "$API/api/eventos-rfid" \
  -H "Content-Type: application/json" \
  -d '{"tagId":"TAG-A002","puntoLecturaId":"PUERTA-PRINCIPAL","tipoMovimiento":"SALIDA"}' | grep -o '"activarAlarmaSonora":[^,}]*'

echo ""
echo "-- INGRESO de SILLA-003 (TAG-A003) --"
curl -s -X POST "$API/api/eventos-rfid" \
  -H "Content-Type: application/json" \
  -d '{"tagId":"TAG-A003","puntoLecturaId":"PUERTA-PRINCIPAL","tipoMovimiento":"INGRESO"}' | grep -o '"mensaje":"[^"]*"'

echo ""
echo "-- Misma lectura otra vez (debería ignorarse por de-bounce de 5s) --"
curl -s -X POST "$API/api/eventos-rfid" \
  -H "Content-Type: application/json" \
  -d '{"tagId":"TAG-A003","puntoLecturaId":"PUERTA-PRINCIPAL","tipoMovimiento":"INGRESO"}' | grep -o '"mensaje":"[^"]*"'

echo -e "\n=== 6. Ver dashboard ==="
curl -s "$API/api/dashboard/kpis" -H "Authorization: Bearer $TOKEN" | grep -o '"totalActivos":[0-9]*'

echo -e "\n✅ Simulación completada. Abre http://localhost:5173 en el navegador."
