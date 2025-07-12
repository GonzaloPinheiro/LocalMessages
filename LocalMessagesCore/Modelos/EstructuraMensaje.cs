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
        // Tipo de mensaje: "texto/TXT", "comando/CMD", etc.
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

        // Comandos específicos (siempre combinados con CMD)
        CMD_Nick = 4 | CMD,   // 00000100 | 00000010 = 00000110 → 6
        CMD_List = 8 | CMD,   // 00001000 | 00000010 = 00001010 → 10
        CMD_Help = 16 | CMD,  // 00010000 | 00000010 = 00010010 → 18

        // Protocolos o medios de transporte
        CNX_TCP = 32 | CNX,       // 00100000 | 00000011 = 00100011 → 35
        CNX_WST = 64 | CNX        // 01000000 | 00000011 = 01000011 → 67
    }
}
