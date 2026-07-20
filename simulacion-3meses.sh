#!/bin/bash
API="https://horuseye-api.mauricioadachi.dev"

echo "=== LOGIN ==="
LOGIN=$(curl -s -X POST "$API/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@horuseye.com","password":"Admin123!"}')

TOKEN=$(echo "$LOGIN" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)

if [ -z "$TOKEN" ]; then
  echo "ERROR: $LOGIN"
  exit 1
fi
echo "OK"

# ============================================================
# FASE 1: Obtener activos existentes
# ============================================================
echo -e "\n=== FASE 1: Obteniendo activos ==="
ACTIVOS_JSON=$(curl -s -H "Authorization: Bearer $TOKEN" "$API/api/activos?pageSize=100")

# Extraer IDs y TAG IDs
ACTIVO_IDS=($(echo "$ACTIVOS_JSON" | grep -o '"id":"[^"]*"' | sed 's/"id":"//;s/"//'))
TAG_IDS=($(echo "$ACTIVOS_JSON" | grep -o '"tagId":"[^"]*"' | sed 's/"tagId":"//;s/"//'))

echo "Activos encontrados: ${#ACTIVO_IDS[@]}"

if [ ${#ACTIVO_IDS[@]} -eq 0 ]; then
  echo "ERROR: No se encontraron activos"
  exit 1
fi

# ============================================================
# FASE 2: Crear autorizaciones de salida (~30% de activos)
# ============================================================
echo -e "\n=== FASE 2: Creando autorizaciones ==="
AUTH_COUNT=0
AUTH_ACTIVOS=()

# Seleccionar ~15 activos para autorizar (salidas válidas)
for i in $(seq 0 14); do
  if [ $i -lt ${#ACTIVO_IDS[@]} ]; then
    ACTIVO_ID="${ACTIVO_IDS[$i]}"
    AUTH_ACTIVOS+=("$ACTIVO_ID")
    
    curl -s -X POST "$API/api/autorizaciones" -H "Content-Type: application/json" -H "Authorization: Bearer $TOKEN" \
      -d "{\"activoId\":\"$ACTIVO_ID\",\"motivo\":\"Autorización general\",\"autorizadoPor\":\"admin@horuseye.com\"}" > /dev/null 2>&1
    
    AUTH_COUNT=$((AUTH_COUNT + 1))
  fi
done
echo "$AUTH_COUNT autorizaciones creadas (activos con salidas válidas)"

# ============================================================
# FASE 3: Generar movimientos de los últimos 90 días
# ============================================================
echo -e "\n=== FASE 3: Generando movimientos de 90 días ==="

PUNTOS=("PUERTA-PRINCIPAL" "PUERTA-BODEGA" "PUERTA-LABORATORIO" "PUERTA-OFICINAS" "PUERTA-DESPACHO")
DIAS=90
TOTAL_MOV=0
ALARMAS=0

for offset in $(seq 0 $((DIAS - 1))); do
  FECHA=$(date -d "-$offset days" +%Y-%m-%d)
  DOW=$(date -d "-$offset days" +%u)  # 1=lunes, 7=domingo

  # Determinar volumen según día de la semana
  if [ "$DOW" -le 5 ]; then
    # Día laboral: 15-25 movimientos
    BASE=$((15 + RANDOM % 11))
  else
    # Fin de semana: 3-8 movimientos
    BASE=$((3 + RANDOM % 6))
  fi

  for j in $(seq 1 $BASE); do
    # Seleccionar activo aleatorio
    ACTIVO_IDX=$((RANDOM % ${#ACTIVO_IDS[@]}))
    ACTIVO_ID="${ACTIVO_IDS[$ACTIVO_IDX]}"
    TAG_ID="${TAG_IDS[$ACTIVO_IDX]}"

    # Seleccionar punto
    PTO_IDX=$((RANDOM % ${#PUNTOS[@]}))
    PTO="${PUNTOS[$PTO_IDX]}"

    # 70% ingresos, 30% salidas
    if [ $((RANDOM % 10)) -lt 7 ]; then
      TIPO="INGRESO"
    else
      TIPO="SALIDA"
    fi

    # Hora aleatoria entre 7am y 7pm
    HORA=$((7 + RANDOM % 13))
    MIN=$((RANDOM % 60))
    SEG=$((RANDOM % 60))
    TIMESTAMP="${FECHA}T$(printf '%02d' $HORA):$(printf '%02d' $MIN):$(printf '%02d' $SEG)"

    # Enviar evento RFID
    RESP=$(curl -s -X POST "$API/api/eventos-rfid" -H "Content-Type: application/json" \
      -d "{\"tagId\":\"$TAG_ID\",\"puntoLecturaId\":\"$PTO\",\"tipoMovimiento\":\"$TIPO\"}")
    
    TOTAL_MOV=$((TOTAL_MOV + 1))

    # Contar alarmas (salidas no autorizadas)
    if echo "$RESP" | grep -q '"activarAlarmaSonora":true'; then
      ALARMAS=$((ALARMAS + 1))
    fi
  done

  if [ $((offset % 10)) -eq 0 ]; then
    echo "  Día $((offset + 1))/90 ($FECHA) - $BASE movimientos"
  fi
done

echo -e "\n=== RESUMEN ==="
echo "Total movimientos: $TOTAL_MOV"
echo "Alarmas generadas: $ALARMAS"
echo "Período: últimos $DIAS días"
echo ""
echo "Recarga el dashboard: https://horuseye.mauricioadachi.dev"
