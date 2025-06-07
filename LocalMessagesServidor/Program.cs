using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LocalMessagesServidor
{
    internal class Program
    {
        // Lista de clientes conectados
        private static ObservableCollection<Client> clientesConectados = new ObservableCollection<Client>();

        // Hilo que ejecuta el servidor
        private static Thread serverThread;
        private static bool activeServer = false;

        static void Main(string[] args)
        {
            // Suscribimos al evento CollectionChanged (opcional)
            clientesConectados.CollectionChanged += ConnectedClients_CollectionChanged;

            try
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
            catch
            {
                Console.WriteLine("Error al iniciar el servidor");
            }

            Console.WriteLine("Servidor iniciado. Presiona ENTER para salir...");
            Console.ReadLine(); // Evita que el programa termine inmediatamente
        }

        // Método que arranca el listener y acepta clientes
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

                    var newClient = new Client
                    {
                        clientName = "",
                        tcpClient = tcpClient
                    };

                    lock (clientesConectados)
                    {
                        clientesConectados.Add(newClient);
                    }

                    Console.WriteLine("Conexión aceptada desde " + tcpClient.Client.RemoteEndPoint);

                    // Arrancamos un hilo para atender a este cliente
                    var clientThread = new Thread(() => ManejarCliente(tcpClient, newClient));
                    clientThread.Start();
                }
            }
            catch
            {
                Console.WriteLine("Error en el servidor");
            }
        }

        // Método que atiende a cada cliente en un hilo separado
        private static void ManejarCliente(TcpClient tcpClient, Client client)
        {
            try
            {
                byte[] buffer = new byte[100];
                int receivedBytes;

                // Primera lectura: recibimos el nombre de usuario
                receivedBytes = tcpClient.Client.Receive(buffer);
                client.clientName = Encoding.ASCII.GetString(buffer, 0, receivedBytes);
                Console.WriteLine("Nuevo cliente conectado: " + client.clientName + "\n");

                // Notificamos a todos menos al emisor que el usuario se conectó
                EnviarMensajeMenosEmisor($"El usuario {client.clientName} se ha conectado.", tcpClient);

                // Enviamos la lista inicial de clientes al recién conectado
                EnviarMensajeATodos(GenerarListaClientes());

                // Actualizamos el nombre en la colección (por si llega vacío al crearse)
                lock (clientesConectados)
                {
                    for (int i = 0; i < clientesConectados.Count; i++)
                    {
                        if (clientesConectados[i].tcpClient.Client == tcpClient.Client)
                        {
                            clientesConectados[i].clientName = client.clientName;
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
                        ProcessCommand(message, tcpClient, client);
                    }
                    else
                    {
                        // Mensaje normal: lo mostramos y lo reenviamos a todos
                        Console.WriteLine($"Mensaje del cliente ({client.clientName}): {message}");
                        string clientText = $"{client.clientName}: {message}";
                        EnviarMensajeATodos(clientText);
                    }
                }

                // Si salimos del bucle, este cliente se ha desconectado
                tcpClient.Close();
                Console.WriteLine($"Cliente desconectado: {client.clientName}");
                EnviarMensajeMenosEmisor($"El usuario {client.clientName} se ha desconectado.", tcpClient);

                // Lo eliminamos de la lista de clientes
                lock (clientesConectados)
                {
                    clientesConectados.Remove(client);
                }

                // Enviamos la lista actualizada de clientes a todos
                EnviarMensajeATodos(GenerarListaClientes());
            }
            catch (Exception e)
            {
                Console.WriteLine("Conexión con un cliente perdida: " + e.Message);
                tcpClient.Close();
            }
        }

        // Procesa los mensajes que empiezan con "CMD|"
        private static void ProcessCommand(string message, TcpClient tcpClient, Client client)
        {
            // Estructura: "CMD|<comando>|<arg1>|<arg2>|..."
            var parts = message.Split('|', (char)StringSplitOptions.RemoveEmptyEntries);
            var command = parts[1].ToLower();
            var args = parts.Skip(2).ToArray();

            switch (command)
            {
                case "nick":
                    if (args.Length == 1)
                    {
                        string oldNick = client.clientName;
                        client.clientName = args[0];
                        Console.WriteLine($"El usuario {oldNick} ha cambiado su nombre a {client.clientName}");
                        EnviarMensajeMenosEmisor($"El usuario {oldNick} ha cambiado su nombre a {client.clientName}", tcpClient);
                        EnviarMensajeAEmisor($"Tu nombre ha sido cambiado a {client.clientName}", tcpClient);
                        EnviarMensajeATodos(GenerarListaClientes());
                    }
                    else
                    {
                        Console.WriteLine("Comando /nick malformado");
                    }
                    break;

                case "list":
                    lock (clientesConectados)
                    {
                        string userList = "Usuarios conectados: " + string.Join(", ", clientesConectados.Select(c => c.clientName));
                        tcpClient.Client.Send(Encoding.ASCII.GetBytes(userList));
                    }
                    break;

                default:
                    Console.WriteLine("Comando no reconocido: " + command);
                    break;
            }
        }

        // Envía un mensaje a todos los clientes
        private static void EnviarMensajeATodos(string text)
        {
            lock (clientesConectados)
            {
                byte[] data = Encoding.ASCII.GetBytes(text);
                foreach (var c in clientesConectados)
                {
                    try
                    {
                        c.tcpClient.Client.Send(data);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error al enviar mensaje a un cliente: " + e.Message);
                    }
                }
            }
        }

        // Envía un mensaje a todos excepto al emisor
        private static void EnviarMensajeMenosEmisor(string text, TcpClient except)
        {
            lock (clientesConectados)
            {
                byte[] data = Encoding.ASCII.GetBytes(text);
                foreach (var c in clientesConectados)
                {
                    if (c.tcpClient.Client != except.Client)
                    {
                        try
                        {
                            c.tcpClient.Client.Send(data);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error al enviar mensaje a un cliente: " + e.Message);
                        }
                    }
                }
            }
        }

        // Envía un mensaje solo al cliente que lo envió
        private static void EnviarMensajeAEmisor(string text, TcpClient sender)
        {
            lock (clientesConectados)
            {
                byte[] data = Encoding.ASCII.GetBytes(text);
                foreach (var c in clientesConectados)
                {
                    if (c.tcpClient.Client == sender.Client)
                    {
                        try
                        {
                            c.tcpClient.Client.Send(data);
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

        // Maneja cambios en la colección de clientes (opcional)
        private static void ConnectedClients_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Aquí podrías reaccionar a clientes añadidos/quedados
        }

        // Genera el texto en formato CMD|list|cliente1|cliente2|...
        private static string GenerarListaClientes()
        {
            lock (clientesConectados)
            {
                return "CMD|list|" + string.Join("|", clientesConectados.Select(c => c.clientName));
            }
        }
    }

    internal class Client
    {
        public string clientName;
        public TcpClient tcpClient;
    }
}