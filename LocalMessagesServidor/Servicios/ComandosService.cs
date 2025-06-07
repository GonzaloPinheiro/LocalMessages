using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using LocalMessagesCore.Modelos;
using LocalMessagesServidor.Servicios;

namespace LocalMessagesServidor.Servicios
{
    internal static class ComandosService
    {
        public static void ProcesarComando(string mensaje, Cliente emisor, IEnumerable<Cliente> clienetesConectados, TcpClient tcpClient)
        {
            // Estructura: "CMD|<comando>|<arg1>|<arg2>|..."
            var parts = mensaje.Split('|', (char)StringSplitOptions.RemoveEmptyEntries);
            var command = parts[1].ToLower();
            var args = parts.Skip(2).ToArray();

            switch (command)
            {
                case "nick":
                    if (args.Length == 1)
                    {
                        string oldNick = emisor.Nombre;
                        emisor.Nombre = args[0];
                        Console.WriteLine($"El usuario {oldNick} ha cambiado su nombre a {emisor.Nombre}");
                        MensajesService.EnviarMensajeMenosEmisor($"El usuario {oldNick} ha cambiado su nombre a {emisor.Nombre}", tcpClient, clienetesConectados);
                        MensajesService.EnviarMensajeAEmisor($"Tu nombre ha sido cambiado a {emisor.Nombre}", tcpClient, clienetesConectados);
                        MensajesService.EnviarMensajeBroadcast(GenerarListaClientes(clienetesConectados), clienetesConectados);
                    }
                    else
                    {
                        Console.WriteLine("Comando /nick malformado");
                    }
                    break;

                case "list":
                    lock (clienetesConectados)
                    {
                        string userList = "Usuarios conectados: " + string.Join(", ", clienetesConectados.Select(c => c.Nombre));
                        tcpClient.Client.Send(Encoding.ASCII.GetBytes(userList));
                    }
                    break;

                default:
                    Console.WriteLine("Comando no reconocido: " + command);
                    break;
            }
        }

        private static string GenerarListaClientes(IEnumerable<Cliente> clienetesConectados)
        {
            lock (clienetesConectados)
            {
                return "CMD|list|" + string.Join("|", clienetesConectados.Select(c => c.Nombre));
            }
        }
    }
}
