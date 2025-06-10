using ChatCSharp.Server.Services;
using LocalMessagesCore.Modelos;
using LocalMessagesServidor.Models;
using LocalMessagesServidor.Servicios;
using LocalMessagesServidor.Transports;
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
        private static ObservableCollection<ClienteConexion> clientesConectados = new ObservableCollection<ClienteConexion>();


        // Hilo que ejecuta el servidor
        private static Thread hiloServidor;
        private static bool servidorActivo = false;

        public static void Start() //lógica de LanzarServidor
        {
            if (!servidorActivo)
            {
                hiloServidor = new Thread(LanzarServidor);
                hiloServidor.Start();
                servidorActivo = true;
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

                //Bucle principal: aceptar conexiones entrantes
                while (true)
                {
                    TcpClient tcpClient = listener.AcceptTcpClient();

                    //Después creo el ITransport de tcp
                    var transporte = new TransporteTcp(tcpClient);
                    var newClient = new ClienteConexion(transporte) { Nombre = ""};

                    lock (clientesConectados)
                    {
                        clientesConectados.Add(newClient);
                    }

                    Console.WriteLine("Conexión aceptada desde " + tcpClient.Client.RemoteEndPoint);

                    // Arrancamos un hilo para atender a este cliente
                    var clientThread = new Thread(() => ManejarClienteLoop(newClient));
                    clientThread.Start();
                }
            }
            catch
            {
                Console.WriteLine("Error en el servidor");
            }
        }


        private static async Task ManejarClienteLoop(ClienteConexion cliente)
        {
            try
            {
                // 1)Primera lectura: recibir el nombre de usuario
                cliente.Nombre = await cliente.Transporte.RecibirAsync();
                Console.WriteLine($"Nuevo cliente conectado: {cliente.Nombre}\n");

                // 2)Notificar a todos menos al emisor
                await MensajesService.EnviarMensajeMenosEmisorAsync(
                    $"El usuario {cliente.Nombre} se ha conectado.",
                    cliente,
                    clientesConectados);

                // 3)Enviar lista inicial de clientes
                await MensajesService.EnviarMensajeBroadcastAsync(
                    MensajesService.GenerarListaClientes(clientesConectados),
                    clientesConectados);

                // 4)Bucle de recepción de mensajes
                string mensaje;
                while ((mensaje = await cliente.Transporte.RecibirAsync()) != null)
                {
                    if (mensaje.StartsWith("CMD|"))
                    {
                        // Procesar comando
                        await ComandosService.ProcesarComandoAsync(
                            mensaje,
                            cliente,
                            clientesConectados);
                    }
                    else
                    {
                        // Mensaje normal: mostrar y reenviar
                        Console.WriteLine($"Mensaje de {cliente.Nombre}: {mensaje}");
                        await MensajesService.EnviarMensajeBroadcastAsync(
                            $"{cliente.Nombre}: {mensaje}",
                            clientesConectados);
                    }
                }

                // 5)El cliente cerró la conexión
                Console.WriteLine($"Cliente desconectado: {cliente.Nombre}");
                await MensajesService.EnviarMensajeMenosEmisorAsync(
                    $"El usuario {cliente.Nombre} se ha desconectado.",
                    cliente,
                    clientesConectados);

                lock (clientesConectados)
                {
                    clientesConectados.Remove(cliente);
                }

                // 6)Enviar lista actualizada
                await MensajesService.EnviarMensajeBroadcastAsync(
                    MensajesService.GenerarListaClientes(clientesConectados),
                    clientesConectados);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error con cliente {cliente.Nombre}: {ex.Message}");
            }
            finally
            {
                //Asegurarse de desconexión limpia
                cliente.Transporte.Desconectar();
            }
        }
    }
}
