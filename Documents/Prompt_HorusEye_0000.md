# Prompt 0000 - Horus Eye

Actúa como un Arquitecto de Software Senior y Experto en .NET 10, PostgreSQL y React con TypeScript. Necesito que generes la estructura de código base, el modelo de datos y los componentes principales para el proyecto "HorusEye", un sistema de control de inventarios y activos en tiempo real mediante lectores RFID, adaptado a los requerimientos de auditoría del cliente.

Debes construir la aplicación basándote estrictamente en las siguientes especificaciones técnicas y de negocio:

### 1. MODELO DE BASE DE DATOS (PostgreSQL)
Diseña el script SQL de las tablas considerando:
- Hay una estructura inicial de tablas que quiero que respetes.
- Tabla 'tags': Campos para ID (EPC/UID), y un Estado obligatorio de tipo ENUM ('REGISTRADO', 'ASIGNADO', 'DISPONIBLES', 'DAÑADO'). Incluir tabla de historial de daños de TAGs.
- Tabla 'activos': (Cambiar terminología de "computadores" a "activos"). Debe incluir campos para: ID (UUID), Placa del Activo (String, Único/Indexado para evitar duplicados), Nombre del Activo, Categoría (Sillas, Impresoras, Electrodomésticos, etc.), Nombre del Tenedor/Responsable, Estado_Ubicacion ('DENTRO_INSTALACIONES', 'FUERA_INSTALACIONES') y Tag_Id (FK a tags, Nullable).
- Tabla 'movimientos': ID (BigSerial), Activo_Id, Punto_Lectura_Id, Tipo_Movimiento ('INGRESO', 'SALIDA'), Autorizado (Boolean), Fecha_Registro (Timestamp con zona horaria).
- Tablas de Usuarios e Identity: Integrar el esquema estándar de ASP.NET Core Identity. Se especifica de forma obligatoria que las contraseñas de los usuarios DEBEN guardarse protegidas en la base de datos mediante hashing seguro de una sola vía (PBKDF2 con Salt), quedando estrictamente prohibido el almacenamiento en texto plano.

### 2. BACKEND (.NET 10 Web API)
Genera el código para los siguientes componentes clave:
- Controlador de Eventos RFID: Endpoint 'POST /api/eventos-rfid' que reciba las lecturas del hardware (Keonn/AdvanNet). Debe incluir:
  1. Filtro Anti-Duplicados (De-bounce) usando 'IMemoryCache' por 5 segundos para ignorar ráfagas del mismo TAG.
  2. Lógica de Alarma: Si el 'Tipo_Movimiento' es 'SALIDA' y el activo NO tiene una autorización de salida vigente en la base de datos, debe retornar en el JSON la instrucción explícita "Activar_Alarma_Sonora: true". Si está autorizado, "Activar_Alarma_Sonora: false".
- SignalR Hub: Un 'MovimientosHub' que exponga el método para transmitir en tiempo real cada evento procesado a todos los clientes de React conectados.
- Consultas y Filtros Horarios: Los contadores de "Ingresos del Día" y "Salidas del Día" deben calcularse estrictamente en el rango horario de 00:00 a 24:00 del día en curso del servidor.

### 3. AUTENTICACIÓN Y AUTORIZACIÓN (JWT + OAuth 2.0)
- Roles y Políticas: Define mediante Claims de Identity dos roles estrictos:
  - Rol "Usuario de Consulta": Permiso exclusivo de lectura. Solo puede visualizar el Dashboard, ver los movimientos en tiempo real y generar reportes. Tiene bloqueados los endpoints de mutación (POST, PUT, DELETE).
  - Rol "Usuario de Gestión": Permiso de administración total. Puede registrar activos, asignar/reasignar TAGs, inhabilitar TAGs dañados y autorizar la salida de equipos.
- AuthController: Implementa endpoints protegidos y públicos para:
  - 'POST /api/auth/register': Exclusivo para administradores, permite dar de alta nuevos usuarios y asignarles su rol.
  - 'POST /api/auth/login': Valida credenciales contra el hash seguro, genera un Token JWT firmado (ID, Email, Rol) y retorna un Access Token junto con un Refresh Token (OAuth 2.0) gestionado en la BD.
  - 'POST /api/auth/refresh-token': Para renovar el JWT de forma transparente sin interrumpir la conexión de SignalR.
  - 'POST /api/auth/change-password' y '/api/auth/recover-password': Para que el usuario pueda cambiar su contraseña o solicitar una recuperación por olvido.
- Configuración SignalR: Configura JwtBearerEvents en Program.cs para permitir que el token de autenticación se reciba a través de la Query String ('access_token') cuando se inicialice la conexión por WebSockets.

### 4. ESTANDARIZACIÓN, MANEJO DE ERRORES Y LOGS
- Respuesta API Estandarizada: Diseña un objeto genérico único 'ApiResponse<T>' para que todos los controladores devuelvan una estructura idéntica:
  { "success": boolean, "message": string, "data": T, "errors": string[], "timestamp": datetime }
- Manejo Global de Errores: Implementa un 'Custom Exception Middleware' o 'IExceptionHandler' de .NET 10 para capturar de forma centralizada cualquier excepción no controlada. Debe mapear excepciones a códigos HTTP semánticos (404, 401, 400, 500) y ocultar el StackTrace detallado en entornos de producción por motivos de seguridad.
- Sistema de Logs Estructurados en JSON: Configura el logging (o Serilog) para escribir registros estructurados en formato JSON dentro de una carpeta raíz llamada 'logs/'. Aplica una estrategia 'Rolling File' diaria (ej. 'log-YYYYMMDD.json') con retención de 30 días. Cada entrada debe registrar automáticamente: Timestamp, LogLevel, RequestPath, HttpMethod, UserId/Role y el mensaje detallado de eventos críticos (lecturas RFID, de-bounce, activaciones de alarmas y logins fallidos).

### 5. FRONTEND (React TS + Vite + Tailwind)
- AuthContext & Axios Interceptor: Implementa un proveedor de estado global para manejar el JWT, verificar su expiración y un interceptor de Axios que adjunte el encabezado 'Authorization: Bearer <token>'. Si expira (Error 401), debe ejecutar el refresh-token automáticamente.
- Rutas Protegidas y UI Basada en Roles: Protege las rutas con un componente 'ProtectedRoute'. Oculta o deshabilita visualmente los elementos de edición, registro o eliminación si el usuario logueado tiene el rol de "Usuario de Consulta".
- Dashboard Principal en Tiempo Real: Tarjetas de Indicadores KPIs (Total Activos, Activos Dentro, Activos Fuera, TAGs Registrados, TAGs Asignados, TAGs Disponibles) y una Tabla de Movimientos Recientes alimentada por SignalR con alertas visuales para salidas no autorizadas.
- Formulario de Gestión de Activos: Permite registrar/editar activos con campos para Placa, Nombre, Categoría y Responsable. El campo de TAG RFID debe ser un Dropdown controlado que consuma únicamente los TAGs en estado 'DISPONIBLES' (sin escritura manual libre). Incluye un modal de confirmación obligatorio antes de eliminar cualquier activo.
- Módulo de Reportes: Vista con filtros por rango de fecha y Tipo de Reporte ('Movimientos', 'Tags Registrados', 'Activos Dentro', 'Activos Fuera'), excluyendo por completo la opción "Usuarios". Añade un botón funcional para exportar los resultados a formato Excel.

Entrega el código de forma limpia, modular, aplicando las mejores prácticas de desarrollo y arquitectura limpia.

## Layout de la APP de React

Necesito que implementar un Layout Responsive para la Aplicación de React.  

Web
```
         1         2         3         4         5         6         7         8
12345678901234567890123456789012345678901234567890123456789012345678901234567890

┌──────────┬───────────────────────────────────────────────────────┬───────────┐
│   Logo   │                           Título                      │  Usuario  │
├──────────┴───────────────────────────────────────────────────────┴───────────┤
│                                Menú del Sistema                              │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│                                                                              │
│                                                                              │
│                                                                              │
│                              Contenido de la App                             │
│                                                                              │
│                                                                              │
│                                                                              │
│                                                                              │
│                                                                              │
│                                                                              │
├──────────────────────────────────────────────────────────────────────────────┤
│                   Todos los Derechos Reservados Edson 2026                   │
└──────────────────────────────────────────────────────────────────────────────┘
```

Móvil
```
         1         2         3         4         5         6         7         8
12345678901234567890123456789012345678901234567890123456789012345678901234567890

┌──────┬──────────────┬─────────┐
│ Logo │    Título    │ Usuario │
├──────┴──────────────┴─────────┤
│                               │
│                               │
│                               │
│                               │
│                               │
│                               │
│                               │
│                               │
│   Contenido de la App         │
│           /                   │
│   Menú Sistema Desplazable    │
│                               │
│                               │
│                               │
│                               │
│                               │
│                               │
│                               │
│                               │
│                               │
├───────────────────────────────┤
│       ® Edson 2026            │
└───────────────────────────────┘
```


## Autenticación y Autorización.

Añade al diseño de la arquitectura de "HorusEye" el módulo de Autenticación, Autorización y Registro de Usuarios utilizando .NET 10 Identity, JWT (JSON Web Tokens) y el flujo de OAuth 2.0. Configura los siguientes requerimientos específicos:

1. ROLES Y AUTORIZACIÓN (Sección 5.2 del cliente)
Define mediante Claims de ASP.NET Core Identity dos roles estrictos basados en políticas:
- Rol "Usuario de Consulta": Permiso exclusivo de lectura. Solo puede visualizar el Dashboard, ver los movimientos en tiempo real y generar reportes. Tiene bloqueados los endpoints de mutación (POST, PUT, DELETE).
- Rol "Usuario de Gestión": Permiso de administración total. Puede registrar activos, asignar/reasignar TAGs, inhabilitar TAGs dañados y autorizar la salida de equipos (Módulo de Alarma).

2. ENDPOINTS DEL BACKEND (.NET 10 + JWT)
Implementa un 'AuthController' con los siguientes endpoints protegidos y públicos:
- 'POST /api/auth/register': Endpoint de registro exclusivo para el administrador del sistema, que permite dar de alta nuevos usuarios asignándoles un rol específico ('Consulta' o 'Gestión').
- 'POST /api/auth/login': Valida credenciales y genera un Token JWT firmado con una clave secreta que incluye el ID de usuario, Email y su Rol (ClaimTypes.Role). Debe retornar el Access Token y un Refresh Token (OAuth 2.0) gestionado en PostgreSQL.
- 'POST /api/auth/refresh-token': Para renovar el JWT de forma transparente sin interrumpir la conexión de SignalR en el Dashboard.
- 'POST /api/auth/change-password' y '/api/auth/recover-password': Endpoints para que el usuario pueda cambiar su contraseña actual o solicitar una recuperación por olvido (Sección 5.1 del cliente).

3. SEGURIDAD EN EL FRONTEND (React TS + Context API)
- AuthContext: Implementa un proveedor de estado global en React que maneje el token JWT, verifique la expiración y almacene los datos del usuario logueado.
- Interceptor Axios: Configura un interceptor de salida que adjunte automáticamente el encabezado 'Authorization: Bearer <token>' en cada petición a la API. Si el token expira (Error 401), debe intentar renovarlo automáticamente llamando al endpoint de 'refresh-token'.
- Rutas Protegidas y Componentes Basados en Roles:
  - Crea un componente 'ProtectedRoute' para evitar que usuarios no autenticados entren al Dashboard.
  - Oculta o deshabilita visualmente los botones de "Eliminar Activo", "Registrar Activo" o "Autorizar Salida" si el usuario logueado tiene el rol de "Usuario de Consulta".

Entrega el código de configuración de JWT en el 'Program.cs' de .NET 10 y el Hook 'useAuth' correspondiente en React TypeScript.

4. ESTANDARIZACIÓN, MANEJO DE ERRORES Y LOGS EN EL BACKEND (.NET 10)
Implementa las siguientes políticas de desarrollo a nivel de arquitectura en toda la API:

- Respuesta API Estandarizada: Diseña un objeto genérico de respuesta único 'ApiResponse<T>' para que todos los controladores devuelvan una estructura idéntica. El formato JSON de salida debe ser:
  {
    "success": true/false,
    "message": "Mensaje informativo de la operación",
    "data": { ... }, // Objeto genérico T (o null si hay error)
    "errors": [ "Detalle del error 1", "Detalle del error 2" ], // Null si success es true
    "timestamp": "2026-05-27T23:39:00Z"
  }

- Manejo Global de Errores (Best Practices): 
  1. No utilices bloques try-catch repetitivos en los controladores. Implementa un 'Custom Exception Middleware' (o el nuevo 'IExceptionHandler' de .NET 10) para capturar de forma centralizada cualquier excepción no controlada.
  2. El middleware debe mapear las excepciones a códigos de estado HTTP semánticos (ej. 'KeyNotFoundException' a 404, 'UnauthorizedAccessException' a 401, 'ArgumentException' a 400, y cualquier otra no controlada a 500 Internal Server Error).
  3. En entornos de producción, el middleware debe ocultar el StackTrace detallado al cliente final en la respuesta JSON para evitar fugas de información sensible, mostrando un mensaje genérico amigable.

- Sistema de Logs Estructurados en JSON:
  1. Configura el logging nativo de .NET 10 (o mediante Serilog) para escribir registros estructurados estrictamente en formato JSON.
  2. Los archivos de log deben guardarse físicamente en una carpeta raíz llamada 'logs/' dentro del proyecto backend.
  3. Implementa una estrategia de rotación de archivos diaria ('Rolling File') con nombres de archivo basados en la fecha (ej. 'log-20260527.json') y un límite de retención (ej. 30 días) para no saturar el almacenamiento de pruebas.
  4. Cada entrada del log JSON debe capturar de manera obligatoria y automática: Timestamp, LogLevel (Information, Warning, Error), RequestPath, HttpMethod, UserId/Role (si está autenticado) y el Message detallado. 
  5. Asegura que cada evento crítico del sistema (ingreso de un TAG RFID, omisión por de-bounce, activación de alarma sonora o intentos de login fallidos) escriba una traza de nivel 'Information' o 'Warning' en estos archivos para auditoría técnica.


