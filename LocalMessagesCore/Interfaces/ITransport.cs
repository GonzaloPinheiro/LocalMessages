using LocalMessagesCore.Modelos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalMessagesCore.Interfaces
{
    /// <summary>
    /// Define interfaz para enviar y recibir mensajes para cualquier mecanismo de transporte (TCP, WebSocket, etc.)
    /// </summary>
    public interface ITransport
    {
        Task IniciarServerAsync();

        /// <summary>
        /// Conecta de forma asíncrona al servidor en host y puerto
        /// </summary>
        Task ConectarAsync(string host, int port);

        /// <summary>
        /// // Envía texto (por ejemplo JSON) de forma asíncrona
        /// </summary>
        Task EnviarAsync(TipoMensaje TipoMensaje, string datos);

        /// <summary>
        /// Recibe un mensaje completo (por ejemplo JSON) de forma asíncrona.
        /// </summary>
        Task<string> RecibirAsync();

        /// <summary>
        /// Cierra la conexión y libera recursos.
        /// </summary>
        void Desconectar();
    }
}
