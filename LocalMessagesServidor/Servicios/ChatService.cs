using ChatCSharp.Server.Services;
using LocalMessagesCore.Modelos;
using LocalMessagesServidor.Data;
using LocalMessagesServidor.Models;
using LocalMessagesServidor.ModelsDB;
using LocalMessagesServidor.Servicios;
using LocalMessagesServidor.Transports;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LocalMessagesServidor.Servicios
{
    public static class Server
    {
        // Lista de clientes conectados
        private static ObservableCollection<ClienteConexion> clientesConectados = new ObservableCollection<ClienteConexion>();

        private static int _puertoTcp = 1234; // Puerto para conexiones TCP
        private static int _puertoWebSocket = 1235; // Puerto para conexiones WebSocket
        private static string _host = "127.0.0.1"; // Host para conexiones 

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
            //Listener para TCP
            TcpListener listenerTCP;
            IPAddress ipAd = IPAddress.Parse(_host);


            //Listener para WebSocket
            HttpListener listenerWS = new HttpListener();
            listenerWS.Prefixes.Add($"http://{_host}:{_puertoWebSocket}/ws/");

            try
            {
                listenerTCP = new TcpListener(ipAd, _puertoTcp);
                listenerTCP.Start();

                listenerWS.Start(); // Iniciar el HttpListener para WebSocket

                // Lanzar tareas/hilos para cada tipo de conexión
                var tcpThread = new Thread(() => AceptarConexionesTcp(listenerTCP));
                tcpThread.Start();

                var wsThread = new Thread(() => AceptarConexionesWebSocket(listenerWS));
                wsThread.Start();
            }
            catch
            {
                Console.WriteLine("Error en el servidor");
            }
        }

        private static void AceptarConexionesTcp(TcpListener listener)
        {
            Console.WriteLine($"Server activo en puerto {_puertoTcp}...");
            Console.WriteLine("Esperando por conexiones tcp...\n");

            while (true)
            {
                TcpClient tcpClient = listener.AcceptTcpClient();
                var transporte = new TransporteTcp(tcpClient);
                var newClient = new ClienteConexion(transporte) { Nombre = "" };
                lock (clientesConectados)
                {
                    clientesConectados.Add(newClient);
                }
                    
                var clientThread = new Thread(() => ManejarClienteLoop(newClient));
                clientThread.Start();
            }
        }

        private static async void AceptarConexionesWebSocket(HttpListener httpListener)
        {
            Console.WriteLine($"Server activo en puerto {_puertoWebSocket}...");
            Console.WriteLine("Esperando por conexiones websocket...\n");


            while (true)
            {
                var context = await httpListener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    var wsContext = await context.AcceptWebSocketAsync(null);
                    var transporte = new TransporteWebSocket(wsContext.WebSocket);
                    var newClient = new ClienteConexion(transporte) { Nombre = "" };
                    lock (clientesConectados)
                    {
                        clientesConectados.Add(newClient);
                    }
                        
                    var clientThread = new Thread(() => ManejarClienteLoop(newClient));
                    clientThread.Start();
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }


        public static async Task ManejarClienteLoop(ClienteConexion cliente)
        {
            try
            {
                // Bucle de recepción de mensajes
                string mensajeRecibido;
                while ((mensajeRecibido = await cliente.Transporte.RecibirAsync()) != null)
                {
                    //string texto = await cliente.Transporte.RecibirAsync();
                    MensajeCliente mensajeSerializado = JsonSerializer.Deserialize<MensajeCliente>(mensajeRecibido);


                    switch(mensajeSerializado.PrefijoMensaje)
                    {

                        // Procesar mensaje de texto
                        case TipoMensaje.TXT:
                            string texto = mensajeSerializado.ContenidoMensaje;

                            // Mensaje normal: mostrar y reenviar
                            Console.WriteLine($"Mensaje de {cliente.Nombre}: {texto}");
                            await RepositorioMensajes.GuardarMensajeRecibido(texto, cliente.Nombre);

                            await MensajesService.EnviarMensajeBroadcastAsync(
                                $"{cliente.Nombre}: {texto}",
                                TipoMensaje.TXT,
                                clientesConectados);
                        break;


                        // Procesar comando
                        case TipoMensaje.CMD_List:
                        case TipoMensaje.CMD_Nick:
                        case TipoMensaje.CMD_Help:

                            await ComandosService.ProcesarComandoAsync(
                                mensajeSerializado.ContenidoMensaje,
                                cliente,
                                clientesConectados);
                        break;


                        // Procesar mensaje de conexión (recibir el nombre del usuario)
                        case TipoMensaje.CNX_TCP:
                        case TipoMensaje.CNX_WST:
                            cliente.Nombre = mensajeSerializado.ContenidoMensaje;
                            Console.WriteLine($"Nuevo cliente conectado: {cliente.Nombre}\n");

                            // 1)Notificar a todos menos al emisor
                            await MensajesService.EnviarMensajeMenosEmisorAsync(
                                $"El usuario {cliente.Nombre} se ha conectado.",
                                TipoMensaje.TXT,
                                cliente,
                                clientesConectados);

                            // 2)Enviar lista inicial de clientes
                            await MensajesService.EnviarMensajeBroadcastAsync(
                                MensajesService.GenerarListaClientes(clientesConectados),
                                TipoMensaje.CMD_List,
                                clientesConectados);

                            // 3) Enviar últimos mensajes
                            var mensajes = await RepositorioMensajes.ObtenerUltimosMensajes(20);

                            foreach (var mensaje in mensajes)
                            {
                                await MensajesService.EnviarMensajeAEmisorAsync(
                                    JsonSerializer.Serialize(mensaje.Remitente + ": " + mensaje.Contenido),
                                    cliente
                                );
                            }

                        break;

                        default:
                            Console.WriteLine($"Mensaje desconocido de {cliente.Nombre}: {mensajeRecibido}");
                            break;
                    }
                }


                // El cliente cerró la conexión
                Console.WriteLine($"Cliente desconectado: {cliente.Nombre}");
                await MensajesService.EnviarMensajeMenosEmisorAsync(
                    $"El usuario {cliente.Nombre} se ha desconectado.",
                    TipoMensaje.TXT,
                    cliente,
                    clientesConectados);

                lock (clientesConectados)
                {
                    clientesConectados.Remove(cliente);
                }


                // Enviar lista actualizada
                await MensajesService.EnviarMensajeBroadcastAsync(
                    MensajesService.GenerarListaClientes(clientesConectados),
                    TipoMensaje.CMD_List,
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
