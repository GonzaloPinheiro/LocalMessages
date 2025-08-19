using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LocalMessagesCore.Modelos
{
    public class MensajeCliente
    {
        // Tipo de mensaje: "texto/TXT", "comando/CMD", "mensaje/MSG", etc.
        public TipoMensaje PrefijoMensaje { get; set; }

        // Contenido del mensaje
        public string ContenidoMensaje { get; set; }
    }

    [Flags]
    public enum TipoMensaje
    {
        // General
        Ninguno = 0,

        // Tipos de contenido
        TXT = 1,          // 00000001
        CMD = 2,          // 00000010
        CNX = 3,          // 00000011 → Combinación de TXT y CMD
        MSG = 4,          // 00000100 → Prefijo de mensajes genéricos

        // Comandos específicos (siempre combinados con CMD)
        CMD_Nick = 8 | CMD,   // 00001000 | 00000010 = 00001010 → 10
        CMD_List = 16 | CMD,   // 00010000 | 00000010 = 00010010 → 18
        CMD_Help = 32 | CMD,   // 00100000 | 00000010 = 00100010 → 34

        // Protocolos o medios de transporte
        CNX_TCP = 64 | CNX,   // 01000000 | 00000011 = 01000011 → 67
        CNX_WST = 128 | CNX,   // 10000000 | 00000011 = 10000011 → 131

        // Operaciones relacionadas con mensajes (ejemplos dummy)
        MSG_1 = 256 | MSG,     // 000100000000 | 00000100 = 000100000100 → 260
        MSG_2 = 512 | MSG,    // 001000000000 | 00000100 = 001000000100 → 516
        MSG_3 = 1024 | MSG  // 010000000000 | 00000100 = 010000000100 → 1028
    }
}