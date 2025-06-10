using LocalMessagesCore.Interfaces;
using LocalMessagesCore.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace LocalMessagesApp.Servicios
{
    //Esto usa ITransport junto a WebSocket para establecer la conexión con el cliente
    internal class TransporteWebSocket : ITransport
    {
        private readonly WebSocket _client;

        public Task ConectarAsync(string host, int port)
        {
            throw new NotImplementedException();
        }

        public void Desconectar()
        {
            throw new NotImplementedException();
        }

        public Task EnviarAsync(string datos)
        {
            throw new NotImplementedException();
        }

        public Task<string> RecibirAsync()
        {
            throw new NotImplementedException();
        }
    }
}
