# 🚀 ChatCSharp.Client.WinForms (Plantilla v7.0)

Este documento es una **plantilla** de cómo deberá funcionar el cliente WinForms en su versió(v7.0). Creada por ChatGPT para usar como guía con las diferentes versiones del proyecto.
Sirve como referencia independiente de cualquier paso intermedio y no tiene por que ser fiel al proyecto.

---

## 🔧 Tecnologías y patrones clave

- **.NET Framework 4.7.2 / .NET 8.0**  
- **ITransport**: interfaz que abstrae cómo se envían y reciben los mensajes (TCP, WebSockets, TLS, etc.) sin acoplar la lógica de la UI a sockets concretos.  
- **Implementaciones de transporte**:  
  - `TransporteTcp` para comunicación sobre TCP.  
  - (Futuras variantes) `WebSocketTransport`, `TlsTransport`, …  
- **Programación asíncrona** con `async/await` en todas las operaciones de red y UI para evitar bloqueos.  
- **Patrón MVVM ligero**: separación entre la lógica de la interfaz (Forms + ViewModels) y la lógica de negocio (`ChatClient`).  
- **Protocolos de chat**: mensajes de texto UTF-8 y comandos prefijados con `CMD|` (p.ej. `/nick`, `/list`).  
- **Eventos**: `OnMessageReceived` y `OnConnectionStatusChanged` para notificar la UI sin depender del transporte.  
- **Inyección de dependencias**: el `ChatClient` recibe una instancia de `ITransport`, lo que facilita cambiar el canal de transporte sin tocar la UI.

---

## 📁 Estructura de carpetas

LocalMessagesCliente/
├── Program.cs ← Punto de arranque de la aplicación WinForms
├── App.config ← Ajustes de servidor, puerto y transporte
│
├── Views/ ← Formularios y su código parcial
│ ├── AppForm1.cs ← Lógica de eventos y bindings de UI
│ ├── AppForm1.Designer.cs ← Definición visual de controles WinForms
│ ├── AppForm1.Commands.cs ← Gestión de comandos (/nick, /list,…)
│ ├── SettingsForm.cs ← Form para configurar IP/puerto/transportes
│ └── SettingsForm.Designer.cs ← Diseño visual de SettingsForm
│
├── ViewModels/ ← Clases de presentación (MVVM ligero)
│ ├── LoginViewModel.cs ← Validación y datos del formulario de login
│ └── ChatViewModel.cs ← Propiedades y comandos para el chat
│
├── Services/ ← Implementaciones de transporte y clientes REST
│ ├── ChatClient.cs ← Gestión de sesión: conectar, enviar, recibir
│ ├── TransporteTcp.cs ← ITransport basado en TcpClient
│ └── AuthClient.cs ← Llamadas HTTP a endpoints de autenticación
│
├── Models/ ← DTOs y tipos usados por la UI
│ ├── UserUi.cs ← Representación de usuario en la interfaz
│ └── MessageUi.cs ← Representación de mensaje en la interfaz
│
└── Properties/ ← Metadatos y recursos del ensamblado
├── AssemblyInfo.cs ← Información del ensamblado (.NET)
├── Resources.resx ← Recursos (imágenes, cadenas localizadas)
└── Settings.settings ← Valores de configuración de usuario

---

## ⚙️ Flujo de funcionamiento

1. **Inicio**  
   - La aplicación lee `App.config` y carga una implementación de `ITransport` (por defecto TCP).  
   - Se crea un `ChatClient` inyectando ese transporte y se suscriben eventos para actualizar la UI.

2. **Conexión**  
   - Al pulsar “Conectar”, el cliente llama `ITransport.ConectarAsync(host, port)`.  
   - Tras conectar, envía el nombre de usuario y comienza un bucle de recepción en segundo plano.

3. **Recepción de mensajes**  
   - Cada vez que `ChatClient` recibe texto o comandos, dispara `OnMessageReceived`.  
   - La UI recibe el evento y muestra mensajes en un ListBox o procesa comandos de lista de usuarios.

4. **Envío de mensajes**  
   - Al pulsar “Enviar” o escribir una barra `/`, la UI delega a `ChatClient` que llama `ITransport.EnviarAsync(texto)`.  
   - Los comandos (`/nick`, `/list`) se formatean como `CMD|…` antes de enviar.

5. **Cambio de transporte**  
   - Gracias a `ITransport`, se puede cambiar a WebSockets o añadir cifrado TLS simplemente substituyendo la instancia en `Program.cs` o `SettingsForm`.

6. **Desconexión**  
   - Al pulsar “Desconectar” o cerrar la app, `ChatClient` cancela el bucle de recepción y llama `ITransport.Desconectar()`, notificando a la UI.

---

## 🛠️ Compilar y ejecutar

1. Abre **ChatCSharp.sln** en Visual Studio 2022+.  
2. Selecciona el proyecto **ChatCSharp.Client.WinForms** como inicio.  
3. Ajusta en **SettingsForm** la dirección del servidor (IP, puerto) y el transporte.  
4. Ejecuta la app; introduce un nombre de usuario y pulsa “Conectar”.

---

## 📄 Licencia

MIT — Este cliente forma parte de un proyecto de aprendizaje; ¡siéntete libre de usarlo y mejorarlo! 😉

---

![Imagen chat](images/Chat.png)
