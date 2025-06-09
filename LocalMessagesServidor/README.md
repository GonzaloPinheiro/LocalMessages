# 📡 ChatCSharp.Server (Plantilla v7.0)

Este documento es una **plantilla** de cómo deberá funcionar el servidor en su versión (v7.0). Creada por ChatGPT para usar como guía con las diferentes versiones del proyecto.
Sirve como referencia independiente de cualquier paso intermedio y no tiene por que ser fiel al proyecto.

---

## 🔧 Tecnologías y patrones clave

- **.NET Framework 4.7.2 / .NET 8.0**  
- **ITransport**: interfaz que abstrae el envío y recepción de texto, permitiendo cambiar entre TCP, WebSockets, TLS, etc., sin tocar la lógica de chat.  
- **Implementaciones de transporte**:  
  - `TransporteTcp`: basada en sockets TCP.  
  - (Posibles variantes) `WebSocketTransport`, `TlsTransport`, …  
- **Programación asíncrona** con `async/await` para toda E/S de red y procesamiento de comandos.  
- **Colecciones controladas** (bloqueos o estructuras concurrentes) para manejar clientes en paralelo sin condiciones de carrera.  
- **Inyección de dependencias ligera**: el transporte y servicios se configuran al iniciar, manteniendo el código desacoplado.  
- **Protocolo de chat**: texto plano UTF-8 y comandos con prefijo `CMD|<comando>|...`.  
- **Difusión de mensajes**:  
  - **Broadcast** general a todos los clientes.  
  - **Envío selectivo** (excluir al emisor o solo al emisor).  
- **Autenticación con JWT**: emisión y validación de tokens en endpoints REST para login/registro.  
- **Persistencia con Dapper**: almacenamiento de usuarios y mensajes en SQL Server o SQLite.

---

## 🗂️ Estructura de carpetas esperada (no tiene por que ajustarse a la realidad)

LocalMessagesServidor/
├── Program.cs ← Punto de entrada y arranque del servidor
├── appsettings.json ← Configuración de puerto, JWT y cadena de base de datos
│
├── Transports/ ← Implementaciones de ITransport
│ ├── TransporteTcp.cs ← Lógica de transporte TCP (Connect, Send, Receive)
│ └── WebSocketTransport.cs ← Lógica de transporte WebSocket (futuro)
│
├── Models/ ← Modelos de dominio del servidor
│ └── ClienteConexion.cs ← Representa cliente conectado (Nombre + ITransport)
│
├── Services/ ← Lógica principal de chat
│ ├── ChatService.cs ← Gestión de conexiones y ciclo de atención de clientes
│ ├── MensajesService.cs ← Difusión de mensajes (broadcast y selectivos)
│ └── ComandosService.cs ← Interpretación y ejecución de comandos (CMD|…)
│
├── Controllers/ ← API REST para autenticación e historial
│ └── AuthController.cs ← Endpoints de login/registro con JWT
│
├── Repositories/ ← Acceso a datos con Dapper
│ ├── UserRepository.cs ← Operaciones CRUD de usuarios
│ └── MessageRepository.cs ← Operaciones CRUD de mensajes e historial
│
└── Authentication/ ← Configuración de JWT
└── JwtSettings.cs ← Claves, issuer y audience para tokens

---

## ⚙️ Flujo de funcionamiento

1. **Arranque**  
   Carga configuración y registra el transporte y servicios necesarios.

2. **Escucha de conexiones**  
   Se inicializa el listener de `ITransport` y se acepta cada cliente en paralelo.

3. **Registro de usuario**  
   El cliente envía su nombre; el servidor lo incluye en la colección y notifica al resto.

4. **Comunicación**  
   - **Texto plano**: envío directo a través de `ITransport`.  
   - **Comandos**: identificados por `CMD|`, permiten operaciones como cambio de nickname o listado de usuarios.

5. **Difusión**  
   - **Broadcast** a todos.  
   - **Envío selectivo**: a todos menos al emisor, o solo al emisor.

6. **Desconexión**  
   Al cerrar la conexión, se limpia la colección y se notifica la salida.

---

## 🛠️ Compilar y ejecutar

1. Abre **ChatCSharp.sln** en Visual Studio 2022+ (o VS 2025).  
2. Selecciona el proyecto **ChatCSharp.Server** y presiona **Iniciar**.  
3. El servidor escuchará en el puerto definido en `appsettings.json` (por defecto 1234).

> 💡 Para pruebas locales, utiliza `127.0.0.1:1234`. Luego podrás cambiar a dirección de red o Raspberry Pi.

---

## 📄 Licencia

MIT — Proyecto personal de aprendizaje. ¡Siéntete libre de usar y modificar! 😉  

---

![Imagen server](images/Server.png)