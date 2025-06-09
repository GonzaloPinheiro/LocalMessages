using LocalMessagesCore.Interfaces;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LocalMessagesServidor.Transports
{

    //Esto usa ITransport junto a tcp para establecer la conexión con el cliente
    public class TransporteTcp : ITransport
    {
        private readonly TcpClient _client;

        // El servidor ya tiene la conexión activa, así que la recibe por constructor
        /// <summary>
        /// Inicializa una nueva instancia de <see cref="TransporteTcp"/> con un cliente TCP ya conectado.
        /// </summary>
        /// <param name="connectedClient">Instancia de <see cref="TcpClient"/> que representa la conexión activa.</param>
        public TransporteTcp(TcpClient connectedClient)
        {
            _client = connectedClient;
        }


        /// <summary>
        /// En el contexto del servidor no se admite reconexión; siempre lanza <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="host">Host al que se intentaría conectar (no usado).</param>
        /// <param name="port">Puerto al que se intentaría conectar (no usado).</param>
        /// <exception cref="NotSupportedException">Se lanza al invocar este método.</exception>
        public Task ConectarAsync(string host, int port)
        {
            //El servidor no se re-conecta; si se llama, lanza excepción
            throw new NotSupportedException("El servidor ya recibió la conexión.");
        }

        /// <summary>
        /// Envía de forma asíncrona un texto al cliente usando UTF-8.
        /// </summary>
        /// <param name="datos">Cadena de texto a enviar.</param>
        public async Task EnviarAsync(string datos)
        {
            var bytes = Encoding.UTF8.GetBytes(datos);
            await _client.GetStream().WriteAsync(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Recibe de forma asíncrona un mensaje del cliente.
        /// </summary>
        /// <returns>El mensaje recibido como <see cref="string"/>, o <c>null</c> si la conexión se ha cerrado.</returns>

        public async Task<string> RecibirAsync()
        {
            var buffer = new byte[4096];
            int n = await _client.GetStream().ReadAsync(buffer, 0, buffer.Length);
            if (n == 0) return null;  //conexión cerrada
            return Encoding.UTF8.GetString(buffer, 0, n);
        }

        /// <summary>
        /// Cierra la conexión TCP y libera los recursos asociados.
        /// </summary>
        public void Desconectar()
        {
            _client.Close();
        }
    }
}
