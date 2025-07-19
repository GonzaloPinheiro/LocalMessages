using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using LocalMessagesServidor.ModelsDB;

namespace LocalMessagesServidor.Data
{
    public static class RepositorioMensajes
    {
       
        public static async Task GuardarMensajeRecibido(String mensaje, string nombreRemitente)
        {
            using (var conexion = FabricaConexion.CrearConexion())
            {
                await conexion.OpenAsync();

                var mensajeRecibido = new MensajeChat
                {
                    Contenido = mensaje,
                    Remitente = nombreRemitente, // Asignar un remitente genérico
                    Receptor = null, // Asignar null para mensajes públicos (sin implementar mensajes privados)
                    FechaEnvio = DateTime.Now,
                };

                await conexion.InsertAsync(mensajeRecibido);
            }
        }

        public static async Task<IEnumerable<MensajeChat>> ObtenerUltimosMensajes(int cantidadMensajes)
        {
            using (var conexion = FabricaConexion.CrearConexion())
            {
                await conexion.OpenAsync();

                //return conexion.Query<MensajeChat>(
                //    "SELECT TOP (@cantidadMensajes) * FROM MensajesChat ORDER BY FechaEnvio DESC",
                //    new { cantidadMensajes });

                string sql = $"SELECT TOP {cantidadMensajes} * FROM MensajesChat ORDER BY FechaEnvio ASC";

                return await conexion.QueryAsync<MensajeChat>(sql);
            }
        }
    }
}
