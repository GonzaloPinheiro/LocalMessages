using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

class Program
{
    static List<Cliente> conectedClients = new List<Cliente>();
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

                Cliente clienteNuevo = new Cliente
                {
                    clientName = "",
                    tcpClient = clientTcp
                };



                lock (conectedClients)
                {
                    conectedClients.Add(clienteNuevo);
                }

                Console.WriteLine("Conexión aceptada desde " + clientTcp.Client.RemoteEndPoint);

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
    static void HiloCliente(TcpClient tcpClient, Cliente cliente)
    {
        try
        {
            byte[] b = new byte[100];
            int recivedBytes;

            recivedBytes = tcpClient.Client.Receive(b);
            cliente.clientName = Encoding.ASCII.GetString(b, 0, recivedBytes);
            Console.WriteLine("New connected client: " + cliente.clientName + "\n\n");

            lock (conectedClients)
            {
                for (int i = 0; i < conectedClients.Count; i++)
                { 
                    if (conectedClients[i].tcpClient.Client == tcpClient.Client)
                    {
                        conectedClients[i].clientName = cliente.clientName;
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
                Console.WriteLine($"Mensaje del cliente ({cliente.clientName}): {mensaje}");


                string clientText = $"{cliente.clientName}: {mensaje}";
                sendMessage(clientText, tcpClient);
                
            }

            tcpClient.Close();
            Console.WriteLine($"Cliente desconectado: {cliente.clientName}");

            lock (conectedClients)
            {
                conectedClients.Remove(cliente);
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
}

class Cliente
{
    public string clientName;
    public TcpClient tcpClient;
}
