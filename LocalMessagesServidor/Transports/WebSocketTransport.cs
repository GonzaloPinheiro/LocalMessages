using LocalMessagesCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalMessagesServidor.Transports
{
    public class WebSocketTransport : ITransport
    {
        private readonly WebSocketTransport _transporte;

        public WebSocketTransport(WebSocketTransport connectedClient)
        {
            _transporte = connectedClient;
        }

         
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
