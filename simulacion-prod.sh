#!/bin/bash
API="https://horuseye-api.onrender.com"

echo "=== 1. Login ==="
LOGIN=$(curl -s -X POST "$API/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@horuseye.com","password":"Admin123!"}')

TOKEN=$(echo "$LOGIN" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)

if [ -z "$TOKEN" ]; then
  echo "ERROR: $LOGIN"
  exit 1
fi
echo "OK"

echo -e "\n=== 2. Crear tags ==="
for TAG in TAG-DEMO-001 TAG-DEMO-002 TAG-DEMO-003; do
  curl -s -X POST "$API/api/tags" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $TOKEN" \
    -d "{\"tagId\":\"$TAG\"}" > /dev/null
  echo "  Tag $TAG"
done

echo -e "\n=== 3. Tags a DISPONIBLE ==="
for TAG in TAG-DEMO-001 TAG-DEMO-002 TAG-DEMO-003; do
  curl -s -X PUT "$API/api/tags/$TAG/estado" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $TOKEN" \
    -d "{\"estado\":\"DISPONIBLE\"}" > /dev/null
  echo "  $TAG → DISPONIBLE"
done

echo -e "\n=== 4. Crear activos ==="
curl -s -X POST "$API/api/activos" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"placa":"LAP-DELL-DEMO","nombre":"Laptop Dell Demo","categoria":"Computadores","tenedorResponsable":"Carlos","tagId":"TAG-DEMO-001"}'

curl -s -X POST "$API/api/activos" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"placa":"IMP-HP-DEMO","nombre":"Impresora HP Demo","categoria":"Impresoras","tenedorResponsable":"Ana","tagId":"TAG-DEMO-002"}'

curl -s -X POST "$API/api/activos" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"placa":"SILLA-DEMO","nombre":"Silla Demo","categoria":"Sillas","tenedorResponsable":"","tagId":"TAG-DEMO-003"}'

echo -e "\n=== 5. Simular lecturas RFID ==="
echo "-- INGRESO TAG-DEMO-001 --"
curl -s -X POST "$API/api/eventos-rfid" \
  -H "Content-Type: application/json" \
  -d '{"tagId":"TAG-DEMO-001","puntoLecturaId":"PUERTA-PRINCIPAL","tipoMovimiento":"INGRESO"}'

echo ""
echo "-- SALIDA SIN AUTORIZACION (activa alarma) --"
curl -s -X POST "$API/api/eventos-rfid" \
  -H "Content-Type: application/json" \
  -d '{"tagId":"TAG-DEMO-002","puntoLecturaId":"PUERTA-PRINCIPAL","tipoMovimiento":"SALIDA"}'

echo ""
echo "-- INGRESO TAG-DEMO-003 --"
curl -s -X POST "$API/api/eventos-rfid" \
  -H "Content-Type: application/json" \
  -d '{"tagId":"TAG-DEMO-003","puntoLecturaId":"PUERTA-PRINCIPAL","tipoMovimiento":"INGRESO"}'

echo -e "\n=== Hecho. Abrí el dashboard ==="
