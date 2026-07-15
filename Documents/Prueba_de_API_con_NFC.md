## Infraestructura de Pruebas de Bajo Costo (Costo $0)
Esta es la configuración de la infraestructura para probar y validar tu solución actual utilizando recursos disponibles, eliminando la inversión en hardware industrial.

[Tarjeta Bancaria (TAG)] 
       │  
       ▼ (Aproximación NFC a < 4 cm)
[Motorola G60 (Lector/Antena)] 
       │  
       ▼ (Petición HTTP POST vía Wi-Fi / Datos)
[Internet]
       │  
       ▼ 
[Tu API / Solución Existente]

------------------------------
## 1. El TAG del Producto (Simulado)

* Hardware: Tarjetas de crédito o débito físicas que tengan chip sin contacto (NFC).
* Función en la prueba: Cada tarjeta actúa como la etiqueta adherida a un producto. El chip de la tarjeta entrega un UID (Identificador Único) en formato hexadecimal (ej. 4B:E1:92:A5) que tu sistema registrará como el código de inventario del artículo.

## 2. El Lector / Antena (Simulado)

* Hardware: Teléfono móvil Motorola G60 con el chip NFC encendido [Soporte Oficial de Motorola].
* Función en la prueba: Reemplaza a las antenas fijas de los portales de inventario. Al acercar la tarjeta a la parte trasera superior del teléfono (junto a las cámaras), el móvil actúa como la antena lectora, captura el UID y activa el flujo [Soporte Oficial de Motorola].

## 3. El Transmisor de la Señal (Aplicación Puente)
Para conectar el hardware del teléfono con tu API existente sin modificar tu código actual, se utiliza la aplicación gratuita NFC Tools (disponible en Google Play Store). Ella se encarga de hacer el puente hacia internet de la siguiente manera:

* Configuración del envío: Dentro de la app, en la pestaña Tareas, se añade una acción de tipo Red / Petición HTTP.
* Configuración de tu API:
* Método: POST
   * URL: La dirección exacta de tu API actual (ej. https://tu-servidor.com).
   * Cuerpo (JSON): Configuras el paquete con la estructura exacta que tu solución ya espera recibir, inyectando el código de la tarjeta mediante la variable dinámica de la app: "tag_id": "[tag:uid]".

## 4. Ejecución del Script de Prueba Masiva (Opcional)
Como un teléfono solo puede leer una tarjeta a la vez, para probar cómo reacciona tu solución actual ante ráfagas masivas de inventario (por ejemplo, 50 productos entrando al mismo tiempo por una puerta), la infraestructura de pruebas se complementa con Postman o un Script en tu PC:

* Desde tu computadora ejecutas una secuencia que envíe una lista de 50 UIDs simulados en un solo segundo hacia tu URL. Así validas si tu motor de alertas actual soporta la carga de producción sin caídas.


