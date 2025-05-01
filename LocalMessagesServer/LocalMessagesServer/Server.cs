using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

class Server
{
    static List<Client> conectedClients = new List<Client>();
    static Thread serverThread;
    static bool activeServer = false;

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


    // Method to launch the server
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


            //Listening for connections
            while (true)
            {
                TcpClient clientTcp = escuchaTcp.AcceptTcpClient();

                Client clienteNuevo = new Client
                {
                    clientName = "",
                    tcpClient = clientTcp
                };



                lock (conectedClients)
                {
                    conectedClients.Add(clienteNuevo);
                }

                Console.WriteLine("Conexión aceptada desde " + clientTcp.Client.RemoteEndPoint);


                //Launching a thread for the new client
                Thread clienteThread = new Thread(() => HiloCliente(clientTcp, clienteNuevo));
                clienteThread.Start();
            }
        }
        catch
        {
            Console.WriteLine("Error en el servidor");
        }
    }

    // Manager for the client thread
    static void HiloCliente(TcpClient tcpClient, Client client)
    {
        try
        {
            byte[] b = new byte[100];
            int recivedBytes;

            recivedBytes = tcpClient.Client.Receive(b);
            client.clientName = Encoding.ASCII.GetString(b, 0, recivedBytes);
            Console.WriteLine("New connected client: " + client.clientName + "\n\n");

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


            while (true)
            {
                recivedBytes = tcpClient.Client.Receive(b);

                if (recivedBytes == 0)
                    break;

                string mensaje = Encoding.ASCII.GetString(b, 0, recivedBytes);
                Console.WriteLine($"Mensaje del cliente ({client.clientName}): {mensaje}");


                string clientText = $"{client.clientName}: {mensaje}";
                sendMessage(clientText, tcpClient);
                
            }

            tcpClient.Close();
            Console.WriteLine($"Cliente desconectado: {client.clientName}");

            lock (conectedClients)
            {
                conectedClients.Remove(client);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Conexión con un cliente perdida: " + e.Message);
            tcpClient.Close();
        }
    }

    // Send message to all clients
    static void sendMessage(string mensaje, TcpClient emisor)
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
}

class Client
{
    public string clientName;
    public TcpClient tcpClient;
}
