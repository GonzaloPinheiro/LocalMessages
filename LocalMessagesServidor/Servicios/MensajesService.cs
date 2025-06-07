using LocalMessagesCore.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LocalMessagesServidor.Servicios
{
    public static class MensajesService
    {
        public static void EnviarMensajeBroadcast(string text, IEnumerable<Cliente> clientesConectados)
        {
            lock (clientesConectados)
            {
                byte[] data = Encoding.ASCII.GetBytes(text);
                foreach (var c in clientesConectados)
                {
                    try
                    {
                        c.Tcp.Client.Send(data);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error al enviar mensaje a un cliente: " + e.Message);
                    }
                }
            }
        }

        public static void EnviarMensajeMenosEmisor(string text, TcpClient excepto, IEnumerable<Cliente> clientesConectados)
        {
            lock (clientesConectados)
            {
                byte[] data = Encoding.ASCII.GetBytes(text);
                foreach (var c in clientesConectados)
                {
                    if (c.Tcp.Client != excepto.Client)
                    {
                        try
                        {
                            c.Tcp.Client.Send(data);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error al enviar mensaje a un cliente: " + e.Message);
                        }
                    }
                }
            }
        }

        public static void EnviarMensajeAEmisor(string text, TcpClient emisor, IEnumerable<Cliente> clientesConectados)
        {
            lock (clientesConectados)
            {
                byte[] data = Encoding.ASCII.GetBytes(text);
                foreach (var c in clientesConectados)
                {
                    if (c.Tcp.Client == emisor.Client)
                    {
                        try
                        {
                            c.Tcp.Client.Send(data);
                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error al enviar mensaje al cliente: " + e.Message);
                        }
                    }
                }
            }
        }
    }
}
