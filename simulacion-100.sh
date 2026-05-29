#!/bin/bash
API="https://horuseye-api.onrender.com"
echo "LOGIN..."
LOGIN=$(curl -s -X POST "$API/api/auth/login" -H "Content-Type: application/json" -d '{"email":"admin@horuseye.com","password":"Admin123!"}')
TOKEN=$(echo "$LOGIN" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
[ -z "$TOKEN" ] && echo "ERROR" && exit 1

echo "CREANDO 100 TAGS..."
for i in $(seq -w 1 100); do
  curl -s -X POST "$API/api/tags" -H "Content-Type: application/json" -H "Authorization: Bearer $TOKEN" -d "{\"tagId\":\"TAG-BULK-$i\"}" > /dev/null 2>&1
  curl -s -X PUT "$API/api/tags/TAG-BULK-$i/estado" -H "Content-Type: application/json" -H "Authorization: Bearer $TOKEN" -d "{\"estado\":\"DISPONIBLE\"}" > /dev/null 2>&1
done
echo "100 tags creados y en DISPONIBLE"

echo "CREANDO 100 ACTIVOS..."
CATEGORIAS=("Computadores" "Monitores" "Perifericos" "Impresoras" "Sillas" "Redes" "Telefonia" "Tablets" "Audio" "Accesorios")
NOMBRES_COMP=(
  "Laptop Dell Latitude|Laptop HP EliteBook|Laptop Lenovo ThinkPad|MacBook Pro|MacBook Air"
  "Monitor Dell 24|Monitor LG 27|Monitor Samsung 32|Monitor Lenovo 22|Monitor ASUS ProArt"
  "Teclado Logitech|Mouse Logitech|Webcam Logitech|Hub USB-C|Audifonos Sony"
  "Impresora HP LaserJet|Impresora Epson EcoTank|Impresora Brother|Impresora Canon|Multifuncional HP"
  "Silla Ergonómica|Silla Ejecutiva|Silla Visitante|Silla Mesh|Silla Plegable"
  "Switch Cisco|Router Ubiquiti|Access Point TP-Link|Patch Panel|Firewall Fortinet"
  "Telefono IP Cisco|Telefono IP Yealink|Telefono Analogico|Auriculares Jabra|Base Carga"
  "iPad Pro|iPad Air|Samsung Galaxy Tab|Microsoft Surface|Kindle"
  "Parlante JBL|Microfono Shure|Camara Logitech|Proyector Epson|Pizarra Digital"
  "Cargador Laptop|Cable HDMI|Cable USB-C|Adaptador VGA|Estuche Laptop"
)
APELLIDOS=("Carlos Pérez" "María López" "Pedro Gómez" "Ana Martínez" "Luis Rodríguez" "Sofía Ramírez" "Diego Torres" "Valentina Ortiz" "Andrés Herrera" "Camila Ruiz" "IT" "Oficina Piso 1" "Oficina Piso 2" "Oficina Piso 3" "")

for i in $(seq 0 99); do
  NUM=$(printf "%03d" $((i+1)))
  CAT_IDX=$((i % 10))
  NOM_ARR=(${NOMBRES_COMP[$CAT_IDX]//|/ })
  NOM_IDX=$((i % ${#NOM_ARR[@]}))
  NOMBRE="${NOM_ARR[$NOM_IDX]}"
  APE_IDX=$((i % ${#APELLIDOS[@]}))
  TENE="${APELLIDOS[$APE_IDX]}"
  TAG="TAG-BULK-$(printf '%03d' $((i+1)))"

  CAT=""
  case $CAT_IDX in
    0) CAT="Computadores";;
    1) CAT="Monitores";;
    2) CAT="Perifericos";;
    3) CAT="Impresoras";;
    4) CAT="Sillas";;
    5) CAT="Redes";;
    6) CAT="Telefonia";;
    7) CAT="Tablets";;
    8) CAT="Audio";;
    9) CAT="Accesorios";;
  esac

  # Build placa from category prefix
  PREF="${CAT:0:3}"
  PLACA="${PREF^^}-$NUM"

  PAYLOAD="{\"placa\":\"$PLACA\",\"nombre\":\"$NOMBRE $NUM\",\"categoria\":\"$CAT\",\"tenedorResponsable\":\"$TENE\",\"tagId\":\"$TAG\"}"
  curl -s -X POST "$API/api/activos" -H "Content-Type: application/json" -H "Authorization: Bearer $TOKEN" -d "$PAYLOAD" > /dev/null 2>&1

  if [ $((i % 10)) -eq 0 ]; then echo "  $((i+1))/100 activos..."; fi
done
echo "100 activos creados"

echo "SIMULANDO 30 MOVIMIENTOS..."
PUNTOS=("PUERTA-PRINCIPAL" "PUERTA-BODEGA" "PUERTA-LABORATORIO")
TIPOS=("INGRESO" "SALIDA")
for i in $(seq 1 30); do
  TAG="TAG-BULK-$(printf '%03d' $i)"
  PTO="${PUNTOS[$((i % 3))]}"
  TIPO="${TIPOS[$((i % 2))]}"
  curl -s -X POST "$API/api/eventos-rfid" -H "Content-Type: application/json" \
    -d "{\"tagId\":\"$TAG\",\"puntoLecturaId\":\"$PTO\",\"tipoMovimiento\":\"$TIPO\"}" > /dev/null 2>&1
done
echo "30 movimientos registrados"

echo ""
echo "===== COMPLETADO ====="
echo "100 tags | 100 activos | 30 movimientos"
echo "https://horus-eye-kappa.vercel.app/dashboard"
