#!/bin/bash
API="https://horuseye-api.onrender.com"

echo "=============================="
echo "  SIMULACION COMPLETA HORUSEYE"
echo "=============================="

echo -e "\n=== 1. Login ==="
LOGIN=$(curl -s -X POST "$API/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@horuseye.com","password":"Admin123!"}')
TOKEN=$(echo "$LOGIN" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
[ -z "$TOKEN" ] && echo "ERROR: $LOGIN" && exit 1
echo "OK"

echo -e "\n=== 2. Crear 10 tags ==="
TAGS=(
  "TAG-EQ-001" "TAG-EQ-002" "TAG-EQ-003" "TAG-EQ-004" "TAG-EQ-005"
  "TAG-EQ-006" "TAG-EQ-007" "TAG-EQ-008" "TAG-EQ-009" "TAG-EQ-010"
)
for TAG in "${TAGS[@]}"; do
  curl -s -X POST "$API/api/tags" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $TOKEN" \
    -d "{\"tagId\":\"$TAG\"}" > /dev/null 2>&1
done
echo "10 tags creados"

echo -e "\n=== 3. Tags a DISPONIBLE ==="
for TAG in "${TAGS[@]}"; do
  curl -s -X PUT "$API/api/tags/$TAG/estado" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $TOKEN" \
    -d "{\"estado\":\"DISPONIBLE\"}" > /dev/null 2>&1
done
echo "10 tags en DISPONIBLE"

echo -e "\n=== 4. Crear 8 activos ==="
ASSETS=(
  '{"placa":"LAP-DELL-001","nombre":"Laptop Dell Latitude 5540","categoria":"Computadores","tenedorResponsable":"Carlos Pérez","tagId":"TAG-EQ-001"}'
  '{"placa":"LAP-HP-002","nombre":"Laptop HP EliteBook 840","categoria":"Computadores","tenedorResponsable":"María López","tagId":"TAG-EQ-002"}'
  '{"placa":"MON-LEN-003","nombre":"Monitor Lenovo ThinkVision 24","categoria":"Monitores","tenedorResponsable":"Pedro Gómez","tagId":"TAG-EQ-003"}'
  '{"placa":"TEC-LOG-004","nombre":"Teclado Logitech MX Keys","categoria":"Perifericos","tenedorResponsable":"Ana Martínez","tagId":"TAG-EQ-004"}'
  '{"placa":"MOU-LOG-005","nombre":"Mouse Logitech MX Master 3","categoria":"Perifericos","tenedorResponsable":"Ana Martínez","tagId":"TAG-EQ-005"}'
  '{"placa":"IMP-HP-006","nombre":"Impresora HP LaserJet Pro","categoria":"Impresoras","tenedorResponsable":"Oficina Piso 3","tagId":"TAG-EQ-006"}'
  '{"placa":"SIL-HER-007","nombre":"Silla Ergonómica Herman Miller","categoria":"Sillas","tenedorResponsable":"","tagId":"TAG-EQ-007"}'
  '{"placa":"SWI-NET-008","nombre":"Switch Cisco Catalyst 2960","categoria":"Redes","tenedorResponsable":"IT","tagId":"TAG-EQ-008"}'
)
for ASSET in "${ASSETS[@]}"; do
  curl -s -X POST "$API/api/activos" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $TOKEN" \
    -d "$ASSET" > /dev/null 2>&1
done
echo "8 activos creados"

echo -e "\n=== 5. Simular movimientos variados ==="
sleep 1

echo "[1/6] INGRESO Laptop Dell (LAP-DELL-001) - PUERTA-PRINCIPAL"
curl -s -X POST "$API/api/eventos-rfid" \
  -H "Content-Type: application/json" \
  -d '{"tagId":"TAG-EQ-001","puntoLecturaId":"PUERTA-PRINCIPAL","tipoMovimiento":"INGRESO"}' | grep -o '"mensaje":"[^"]*"'

sleep 1

echo "[2/6] INGRESO Monitor Lenovo (MON-LEN-003) - PUERTA-PRINCIPAL"
curl -s -X POST "$API/api/eventos-rfid" \
  -H "Content-Type: application/json" \
  -d '{"tagId":"TAG-EQ-003","puntoLecturaId":"PUERTA-PRINCIPAL","tipoMovimiento":"INGRESO"}' | grep -o '"mensaje":"[^"]*"'

sleep 1

echo "[3/6] SALIDA Teclado (TEC-LOG-004) SIN AUTORIZACION - PUERTA-PRINCIPAL"
curl -s -X POST "$API/api/eventos-rfid" \
  -H "Content-Type: application/json" \
  -d '{"tagId":"TAG-EQ-004","puntoLecturaId":"PUERTA-PRINCIPAL","tipoMovimiento":"SALIDA"}' | grep -o '"mensaje":"[^"]*"'

sleep 1

echo "[4/6] INGRESO Impresora HP (IMP-HP-006) - PUERTA-BODEGA"
curl -s -X POST "$API/api/eventos-rfid" \
  -H "Content-Type: application/json" \
  -d '{"tagId":"TAG-EQ-006","puntoLecturaId":"PUERTA-BODEGA","tipoMovimiento":"INGRESO"}' | grep -o '"mensaje":"[^"]*"'

sleep 1

echo "[5/6] SALIDA Switch Cisco (SWI-NET-008) - PUERTA-PRINCIPAL"
curl -s -X POST "$API/api/eventos-rfid" \
  -H "Content-Type: application/json" \
  -d '{"tagId":"TAG-EQ-008","puntoLecturaId":"PUERTA-PRINCIPAL","tipoMovimiento":"SALIDA"}' | grep -o '"mensaje":"[^"]*"'

sleep 1

echo "[6/6] INGRESO Laptop HP (LAP-HP-002) - PUERTA-PRINCIPAL"
curl -s -X POST "$API/api/eventos-rfid" \
  -H "Content-Type: application/json" \
  -d '{"tagId":"TAG-EQ-002","puntoLecturaId":"PUERTA-PRINCIPAL","tipoMovimiento":"INGRESO"}' | grep -o '"mensaje":"[^"]*"'

echo -e "\n=============================="
echo "  SIMULACION COMPLETADA"
echo "  10 tags | 8 activos | 6 movimientos"
echo "=============================="
echo -e "\nAbrí el dashboard: https://horus-eye-kappa.vercel.app/dashboard"
