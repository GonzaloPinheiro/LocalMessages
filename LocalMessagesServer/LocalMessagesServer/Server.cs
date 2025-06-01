using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

class Server
{
    //static List<Client> conectedClients = new List<Client>();
    static ObservableCollection<Client> conectedClients = new ObservableCollection<Client>();
    static Thread serverThread;
    static bool activeServer = false;

    static TcpClient tcpCiente;

    static Server()
    {
        // 2) Nos subscribimos al evento CollectionChanged
        conectedClients.CollectionChanged += ConectedClients_CollectionChanged;
    }

    static void Main(string[] args)
    {
        try
        {
            if (!activeServer)
            {
                serverThread = new Thread(launchServer);
                serverThread.Start();
                activeServer = true;
            }
            else
            {
                Console.WriteLine("Server is already running");
            }
        }
        catch
        {
            Console.WriteLine("Error");
        }

        Console.WriteLine("Servidor iniciado. Presiona ENTER para salir...");
        Console.ReadLine(); // <- esto evita que el programa termine
    }


    // Método para lanzar el servidor
    static void launchServer()
    {
        TcpListener escuchaTcp;
        IPAddress ipAd = IPAddress.Parse("127.0.0.1");

        try
        {
            escuchaTcp = new TcpListener(ipAd, 1234);
            escuchaTcp.Start();

            Console.WriteLine("Server active in port 1234...");
            Console.WriteLine("Waiting for connections...\n");


            //Escuchando conexiones
            while (true)
            {
                //TcpClient clientTcp = escuchaTcp.AcceptTcpClient();
                tcpCiente = escuchaTcp.AcceptTcpClient();

                Client clienteNuevo = new Client
                {
                    clientName = "",
                    tcpClient = tcpCiente
                };



                lock (conectedClients)
                {
                    conectedClients.Add(clienteNuevo);
                }

                Console.WriteLine("Conexión aceptada desde " + tcpCiente.Client.RemoteEndPoint);


                //Lanzo un hilo para atender al cliente
                Thread clienteThread = new Thread(() => HiloCliente(tcpCiente, clienteNuevo));
                clienteThread.Start();
            }
        }
        catch
        {
            Console.WriteLine("Error en el servidor");
        }
    }

    // Manager para cada cliente
    static void HiloCliente(TcpClient tcpClient, Client client)
    {
        try
        {
            byte[] b = new byte[100];
            int recivedBytes;

            recivedBytes = tcpClient.Client.Receive(b);
            client.clientName = Encoding.ASCII.GetString(b, 0, recivedBytes);
            Console.WriteLine("New connected client: " + client.clientName + "\n\n");

            //Notificación de cliente conectado a todos los demás clientes
            sendMessageButEmitter($"El usuario {client.clientName} se ha conectado.", tcpClient);

            //Envio la lista con todos los nombres de los clientes
            sendMessage(generadorListaClientes());

            lock (conectedClients)
            {
                for (int i = 0; i < conectedClients.Count; i++)
                { 
                    if (conectedClients[i].tcpClient.Client == tcpClient.Client)
                    {
                        conectedClients[i].clientName = client.clientName;
                        break;
                    }
                }
            }



            //Escucha los mensajes del cliente y opera acorde a ellos
            while (true)
            {
                recivedBytes = tcpClient.Client.Receive(b);


                //El cliente se ha desconectado
                if (recivedBytes == 0)
                {
                    break;
                }
                    

                string mensaje = Encoding.ASCII.GetString(b, 0, recivedBytes);


                if (mensaje.StartsWith("CMD|")) //Es un comando
                {
                    ProcesarComando(mensaje, tcpClient, client);
                }
                else { //Es un menasje normal
                    Console.WriteLine($"Mensaje del cliente ({client.clientName}): {mensaje}");


                    string clientText = $"{client.clientName}: {mensaje}";
                    sendMessage(clientText);
                }
                
            }

            //Salgo del while = este cliente se ha desconectado

            tcpClient.Close();
            Console.WriteLine($"Cliente desconectado: {client.clientName}");
            sendMessageButEmitter($"El usuario {client.clientName} se ha desconectado.", tcpClient);
            
            //Lo elimino de la lista de clietes
            lock (conectedClients)
            {
                conectedClients.Remove(client);
            }

            //Envio la lista actualizada de los clientes conectados
            sendMessage(generadorListaClientes());
        }
        catch (Exception e)
        {
            Console.WriteLine("Conexión con un cliente perdida: " + e.Message);
            tcpClient.Close();
        }
    }


    //Se encarga de procesar los mensajes recibidos que empiezan por "CMD|"
    private static void ProcesarComando(string mensaje, TcpClient tcpClient, Client client)
    {
        // Estructura: "CMD|<comando>|<arg1>|<arg2>|..."
        var partes = mensaje.Split('|', StringSplitOptions.RemoveEmptyEntries);
        var comando = partes[1].ToLower();
        var args = partes.Skip(2).ToArray();

        switch (comando)
        {

            //Cambiar el nick del usuario
            case "nick":
                if (args.Length == 1)
                {
                    string nuevoNick = args[0];
                    string nickAnterior = client.clientName;
                    client.clientName = nuevoNick;

                    Console.WriteLine($"El usuario {client.clientName} ha cambiado su nombre a {nuevoNick}");
                    sendMessageButEmitter($"El usuario {nickAnterior} ha cambiado su nombre a {nuevoNick}", tcpClient);
                    sendMessageToEmitter($"Tu nombre ha sido cambiado a {nuevoNick}", tcpClient);
                }
                else
                {
                    Console.WriteLine("Comando /nick malformado");
                }
                break;

            //Enviar lista de clientes conectados
            case "list":
                lock (conectedClients)
                {
                    string listaUsuarios = "Usuarios conectados: " + string.Join(", ", conectedClients.Select(c => c.clientName));
                    tcpClient.Client.Send(Encoding.ASCII.GetBytes(listaUsuarios));
                }
                break;

            //Comando no reconocido
            default:
                Console.WriteLine("Comando no reconocido: " + comando);
                break;
        }
    }


    //NOTA: Revisar si el argumento TcpClient sobra, 
    // Mandar mensaje a todos los clientes
    static void sendMessage(string mensaje)
    {
        lock (conectedClients)
        {
            byte[] datos = Encoding.ASCII.GetBytes(mensaje);
            foreach (var cliente in conectedClients)
            {
                try
                {
                    //if (cliente.tcpClient.Client != emisor.Client)
                    //{
                        cliente.tcpClient.Client.Send(datos);
                    //}
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error al enviar mensaje a un cliente: " + e.Message);
                }
            }
        }
    }
    // Mandar mensaje a todos los clientes menos al emisor
    static void sendMessageButEmitter(string mensaje, TcpClient emisor)
    {
        lock (conectedClients)
        {
            byte[] datos = Encoding.ASCII.GetBytes(mensaje);
            foreach (var cliente in conectedClients)
            {
                try
                {
                    if (cliente.tcpClient.Client != emisor.Client)
                    {
                        cliente.tcpClient.Client.Send(datos);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error al enviar mensaje a un cliente: " + e.Message);
                }
            }
        }
    }

    static void sendMessageToEmitter(string mensaje, TcpClient emisor)
    {
        lock (conectedClients)
        {
            byte[] datos = Encoding.ASCII.GetBytes(mensaje);
            foreach (var cliente in conectedClients)
            {
                try
                {
                    if (cliente.tcpClient.Client == emisor.Client)
                    {
                        cliente.tcpClient.Client.Send(datos);
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error al enviar mensaje a un cliente: " + e.Message);
                }
            }
        }
    }





    //Manejador del evento CollectionChanged de lista conectedClients
    private static void ConectedClients_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Detecta si se modifico la lista
        //if (e.Action == NotifyCollectionChangedAction.Add)
        //{
        //    foreach (Client nuevo in e.NewItems!)
        //    {
        //        Console.WriteLine($"[Evento] Cliente agregado: {nuevo.clientName ?? "(aún sin nombre)"}");
        //        sendMessage(generadorListaClientes(), tcpCiente);
        //        // Aquí podría notificar a los clienetes del cliente conectado
        //    }
        //}
        //else if (e.Action == NotifyCollectionChangedAction.Remove)
        //{
        //    foreach (Client viejo in e.OldItems!)
        //    {
        //        Console.WriteLine($"[Evento] Cliente eliminado: {viejo.clientName}");
        //        sendMessage(generadorListaClientes(), tcpCiente);
        //        // Aquí podría notificar a los clienetes del cliente desconectado
        //    }
        //}
        //NOTA: También se puede manejar Replace, Move, Reset, etc.
    }

    //Genera el comando a enviar con la lita de clientes
    private static string generadorListaClientes()
    {
        string listaUsuarios;
        lock (conectedClients)
        {
            listaUsuarios = "CMD|list|" + string.Join("|", conectedClients.Select(c => c.clientName));
        }

        return listaUsuarios;
    }

}

class Client
{
    public string clientName;
    public TcpClient tcpClient;
}
