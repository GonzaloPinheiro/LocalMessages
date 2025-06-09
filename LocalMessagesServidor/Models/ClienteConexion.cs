using LocalMessagesCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalMessagesServidor.Models
{
    public class ClienteConexion
    {
        public string Nombre { get; set; }
        public ITransport Transporte { get; }

        public ClienteConexion(ITransport transporte)
        {
            Transporte = transporte;
        }
    }
}
