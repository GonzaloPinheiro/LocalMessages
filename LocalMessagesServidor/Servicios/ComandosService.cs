using ChatCSharp.Server.Services;
using LocalMessagesCore.Modelos;
using LocalMessagesServidor.Models;
using LocalMessagesServidor.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LocalMessagesServidor.Servicios
{
    internal static class ComandosService
    {
        /// <summary>
        /// Procesa los comandos recibidos ("/nick", "/list", etc.) de forma asíncrona.
        /// </summary>
        public static async Task ProcesarComandoAsync(
            string mensaje,
            ClienteConexion emisor,
            IEnumerable<ClienteConexion> clientesConectados)
        {
            // Estructura: "CMD|<comando>|<arg1>|<arg2>|..."
            var partes = mensaje.Split('|', (char)StringSplitOptions.RemoveEmptyEntries);
            var comando = partes[1].ToLower();
            var args = partes.Skip(2).ToArray();

            switch (comando)
            {
                case "nick":
                    if (args.Length == 1)
                    {
                        string nickAnterior = emisor.Nombre;
                        emisor.Nombre = args[0];
                        Console.WriteLine($"El usuario {nickAnterior} ha cambiado su nombre a {emisor.Nombre}");

                        //Notificar al resto del cambio de nick
                        await MensajesService.EnviarMensajeMenosEmisorAsync(
                            $"El usuario {nickAnterior} ha cambiado su nombre a {emisor.Nombre}",
                            emisor,
                            clientesConectados);

                        //Notificar al propio emisor del cambio de nick
                        await MensajesService.EnviarMensajeAEmisorAsync(
                            $"Tu nombre ha sido cambiado a {emisor.Nombre}",
                            emisor);

                        //Enviar lista actualizada a todos los clientes
                        var lista = MensajesService.GenerarListaClientes(clientesConectados);
                        await MensajesService.EnviarMensajeBroadcastAsync(lista, clientesConectados);
                    }
                    else
                    {
                        Console.WriteLine("Comando /nick malformado");
                    }
                    break;

                case "list":
                    //Construir el string con la lista y enviarlo solo al emisor
                    string listaUsuarios = "Usuarios conectados: " +
                        string.Join(", ", clientesConectados.Select(c => c.Nombre));
                    await MensajesService.EnviarMensajeAEmisorAsync(listaUsuarios, emisor);
                    break;

                default:
                    Console.WriteLine("Comando no reconocido: " + comando);
                    break;
            }
        }

    }
}
