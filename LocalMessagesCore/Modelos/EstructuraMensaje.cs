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
        Ninguno = 0,
        TXT = 1,
        CMD = 2,

        CMD_Nick = 4 | CMD,   // 00100 | 00010 = 00110 → 6 (comando /nick)
        CMD_List = 8 | CMD,   // 01000 | 00010 = 01010 → 10 (comando /list)
        CMD_Help = 16 | CMD   // 10000 | 00010 = 10010 → 18 (comando /help)
    }
}
