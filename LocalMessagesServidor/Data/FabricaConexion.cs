using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalMessagesServidor.Data
{
    public static class FabricaConexion
    {
        private readonly static string cadenaConexion = "Server=localhost;Database=ChatMensajesDb;Trusted_Connection=True;";

        public static SqlConnection CrearConexion()
        {
            return new SqlConnection(cadenaConexion);
        }
    }
}
