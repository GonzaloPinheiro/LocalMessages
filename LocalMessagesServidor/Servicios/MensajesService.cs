using LocalMessagesServidor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using LocalMessagesCore.Interfaces;
using LocalMessagesCore.Modelos;


namespace ChatCSharp.Server.Services
{
    public static class MensajesService
    {
        // 1)Enviar mensaje a todos
        public static async Task EnviarMensajeBroadcastAsync(
            string texto,
            TipoMensaje tipoMensaje,
            IEnumerable<ClienteConexion> clientesConectados)
        {
            // Creamos una copia para evitar excep. por colección modificada
            var destino = clientesConectados.ToList();
            foreach (var cliente in destino)
            {
                try
                {
                    await cliente.Transporte.EnviarAsync(tipoMensaje, texto);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error al enviar a {cliente.Nombre}: {e.Message}");
                }
            }
        }

        // 2)Enviar mensaje a todos menos al indicado
        public static async Task EnviarMensajeMenosEmisorAsync(
            string texto,
            TipoMensaje tipoMensaje,
            ClienteConexion emisor,
            IEnumerable<ClienteConexion> clientesConectados)
        {
            var destino = clientesConectados.ToList();
            foreach (var cliente in destino)
            {
                if (cliente != emisor)
                {
                    try
                    {
                        await cliente.Transporte.EnviarAsync(tipoMensaje, texto);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error al enviar a {cliente.Nombre}: {e.Message}");
                    }
                }
            }
        }

        // 3)Enviar mensaje solo al indicado
        public static async Task EnviarMensajeAEmisorAsync(
            string texto,
            ClienteConexion emisor)
        {
            try
            {
                await emisor.Transporte.EnviarAsync(TipoMensaje.TXT, texto);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error al enviar a {emisor.Nombre}: {e.Message}");
            }
        }

        // 4)Generar string con lista de nombres
        public static string GenerarListaClientes(
            IEnumerable<ClienteConexion> clientesConectados)
        {
            lock (clientesConectados)
            {
                return "CMD|list|" +
                    string.Join("|", clientesConectados.Select(c => c.Nombre));
            }
        }
    }
}
