using LocalMessagesCore.Modelos;
using LocalMessagesServidor.Servicios;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LocalMessagesServidor.Servicios
{
    public static class Server
    {
        // Lista de clientes conectados
        private static ObservableCollection<Cliente> clientesConectados = new ObservableCollection<Cliente>();


        // Hilo que ejecuta el servidor
        private static Thread serverThread;
        private static bool activeServer = false;

        public static void Start() //lógica de LanzarServidor
        {
            if (!activeServer)
            {
                serverThread = new Thread(LanzarServidor);
                serverThread.Start();
                activeServer = true;
            }
            else
            {
                Console.WriteLine("Server ya está funcionando");
            }
        }

        private static void LanzarServidor()
        {
            TcpListener listener;
            IPAddress ipAd = IPAddress.Parse("127.0.0.1");

            try
            {
                listener = new TcpListener(ipAd, 1234);
                listener.Start();

                Console.WriteLine("Server activo en puerto 1234...");
                Console.WriteLine("Esperando por conexiones...\n");

                // Bucle principal: aceptar conexiones entrantes
                while (true)
                {
                    TcpClient tcpClient = listener.AcceptTcpClient();

                    var newClient = new Cliente
                    {
                        Nombre = "",
                        Tcp = tcpClient
                    };

                    lock (clientesConectados)
                    {
                        clientesConectados.Add(newClient);
                    }

                    Console.WriteLine("Conexión aceptada desde " + tcpClient.Client.RemoteEndPoint);

                    // Arrancamos un hilo para atender a este cliente
                    var clientThread = new Thread(() => ManejarClienteLoop(tcpClient, newClient));
                    clientThread.Start();
                }
            }
            catch
            {
                Console.WriteLine("Error en el servidor");
            }
        }


        private static void ManejarClienteLoop(TcpClient tcpClient, Cliente cliente) //invocado en cada hilo
        {
            try
            {
                byte[] buffer = new byte[100];
                int receivedBytes;

                // Primera lectura: recibimos el nombre de usuario
                receivedBytes = tcpClient.Client.Receive(buffer);
                cliente.Nombre = Encoding.ASCII.GetString(buffer, 0, receivedBytes);
                Console.WriteLine("Nuevo cliente conectado: " + cliente.Nombre + "\n");

                // Notificamos a todos menos al emisor que el usuario se conectó
                MensajesService.EnviarMensajeMenosEmisor($"El usuario {cliente.Nombre} se ha conectado.", tcpClient, clientesConectados);

                // Enviamos la lista inicial de clientes al recién conectado
                MensajesService.EnviarMensajeBroadcast(GenerarListaClientes(), clientesConectados);

                // Actualizamos el nombre en la colección (por si llega vacío al crearse)
                lock (clientesConectados)
                {
                    for (int i = 0; i < clientesConectados.Count; i++)
                    {
                        if (clientesConectados[i].Tcp.Client == tcpClient.Client)
                        {
                            clientesConectados[i].Nombre = cliente.Nombre;
                            break;
                        }
                    }
                }

                // Bucle de recepción de mensajes
                while (true)
                {
                    receivedBytes = tcpClient.Client.Receive(buffer);

                    // Si recibe 0 bytes, el cliente se desconectó
                    if (receivedBytes == 0)
                        break;

                    string message = Encoding.ASCII.GetString(buffer, 0, receivedBytes);

                    if (message.StartsWith("CMD|"))
                    {
                        // Procesar comando (/nick, /list, etc.)
                        ComandosService.ProcesarComando(message, cliente, clientesConectados ,tcpClient);
                    }
                    else
                    {
                        // Mensaje normal: lo mostramos y lo reenviamos a todos
                        Console.WriteLine($"Mensaje del cliente ({cliente.Nombre}): {message}");
                        string clientText = $"{cliente.Nombre}: {message}";
                        MensajesService.EnviarMensajeBroadcast(clientText, clientesConectados);
                    }
                }

                // Si salimos del bucle, este cliente se ha desconectado
                tcpClient.Close();
                Console.WriteLine($"Cliente desconectado: {cliente.Nombre}");
                MensajesService.EnviarMensajeMenosEmisor($"El usuario {cliente.Nombre} se ha desconectado.", tcpClient, clientesConectados);

                // Lo eliminamos de la lista de clientes
                lock (clientesConectados)
                {
                    clientesConectados.Remove(cliente);
                }

                // Enviamos la lista actualizada de clientes a todos
                MensajesService.EnviarMensajeBroadcast(GenerarListaClientes(), clientesConectados);
            }
            catch (Exception e)
            {
                Console.WriteLine("Conexión con un cliente perdida: " + e.Message);
                tcpClient.Close();
            }

        }

        private static string GenerarListaClientes()
        {
            lock (clientesConectados)
            {
                return "CMD|list|" + string.Join("|", clientesConectados.Select(c => c.Nombre));
            }
        }
    }
}
