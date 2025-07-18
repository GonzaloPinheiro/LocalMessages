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
                    Receptor = null, // Asignar null para mensajes públicos (sin implementear mensajes privados)
                    FechaEnvio = DateTime.Now,
                };

                await conexion.InsertAsync(mensajeRecibido);
            }
        }
    }
}
