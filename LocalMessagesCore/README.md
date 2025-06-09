# 📦 ChatCSharp.Core (Plantilla v7.0)

Biblioteca de clases compartida para la aplicación de chat en C#  
Contiene los tipos, contratos y DTOs que usan tanto el cliente como el servidor.

Este documento es una **plantilla** de cómo deberá funcionar el core en su versión (v7.0). Creada por ChatGPT para usar como guía con las diferentes versiones del proyecto.
Sirve como referencia independiente de cualquier paso intermedio y no tiene por que ser fiel al proyecto.

---

## 🔧 Tecnologías y patrones clave

- **.NET Standard 2.0** (compatible con .NET Framework y .NET Core/5+).  
- **Interfaces de contrato**:  
  - `ITransport` abstrae el envío y recepción de texto sin atarse a sockets o protocolos concretos.  
- **Modelos y DTOs**:  
  - Tipos de dominio (`User`, `Message`).  
  - DTOs ligeros para serializar peticiones y respuestas (`ChatMessageDto`, `LoginRequest`, `RegisterRequest`).  
- **Separación de capas**:  
  - El Core no contiene lógica de negocio ni E/S de red, solo definiciones compartidas.  
- **Inyección de dependencias**:  
  - El cliente y el servidor consumen los contratos y modelos de este proyecto para permanecer desacoplados.

---

## 📁 Estructura de carpetas

LocalMessagesCore/
├── Interfaces/
│ └── ITransport.cs ← Contrato para conectar, enviar, recibir y desconectar
│
├── Models/
│ ├── User.cs ← Representa un usuario (ID, Nombre, Roles…)
│ ├── Message.cs ← Representa un mensaje de chat (Emisor, Texto, Fecha)
│ └── ChatMessageDto.cs ← DTO para serializar el transporte de mensajes
│
└── DTOs/
├── LoginRequest.cs ← Datos para login (Usuario, Contraseña)
└── RegisterRequest.cs ← Datos para registro de nuevos usuarios

---

## ⚙️ Uso y flujo de trabajo

1. **Referenciar**  
   - Agrega `ChatCSharp.Core` como dependencia en tus proyectos de cliente y servidor.

2. **Contratos**  
   - Implementa `ITransport` para tu mecanismo de transporte (TCP, WebSocket, TLS…).  
   - Inyecta esa implementación en `ChatClient` (cliente) o en tu servicio de servidor.

3. **Modelos**  
   - Usa `User` y `Message` en la capa de presentación y lógica de negocio.  
   - Serializa/deserializa con los DTOs (`ChatMessageDto`, `LoginRequest`, `RegisterRequest`) al comunicarse con APIs o al enviar datos por transporte.

4. **Evolución**  
   - Añade nuevos DTOs en `DTOs/` para cualquier nueva operación de red.  
   - Extiende los modelos en `Models/` para reflejar cambios en el dominio.

---

## 📄 Licencia

MIT — Proyecto personal de aprendizaje. ¡Compártelo y contribuye si quieres! 😉  