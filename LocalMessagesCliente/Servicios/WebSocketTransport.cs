using LocalMessagesCore.Interfaces;
using LocalMessagesCore.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LocalMessagesApp.Servicios
{
    //Esto usa ITransport junto a WebSocket para establecer la conexión con el cliente
    internal class TransporteWebSocket : ITransport
    {
        private ClientWebSocket _client; // Change WebSocket to ClientWebSocket

        public async Task ConectarAsync(string host, int port)
        {
            _client = new ClientWebSocket(); // Initialize ClientWebSocket

            try
            {
                await _client.ConnectAsync(
                    //new Uri($"ws://{host}:{port}"),
                    new Uri($"ws://{host}:{port}/ws/"),
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                _client = null;
                throw new InvalidOperationException($"No se pudo conectar a {host}:{port}.", ex);
            }
        }

        public async Task EnviarAsync(string datos)
        {
            if (_client != null && _client.State == WebSocketState.Open)
            {
                await _client.SendAsync(
                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(datos)),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
        }

        public Task IniciarServerAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<string> RecibirAsync()
        {
            if (_client != null && _client.State == WebSocketState.Open)
            {
                var receiveBuffer = new byte[1024];
                var result = await _client.ReceiveAsync(
                    new ArraySegment<byte>(receiveBuffer),
                    CancellationToken.None);

                return Encoding.UTF8.GetString(receiveBuffer, 0, result.Count);
            }

            return string.Empty;
        }

        void ITransport.Desconectar()
        {
            if (_client != null && _client.State == WebSocketState.Open)
            {
                _client.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Cierre normal",
                    CancellationToken.None);
            }
        }
    }
}
