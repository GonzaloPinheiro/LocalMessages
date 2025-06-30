using LocalMessagesCore.Interfaces;
using LocalMessagesCore.Modelos;
using LocalMessagesServidor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LocalMessagesServidor.Transports
{
    public class TransporteWebSocket : ITransport
    {
        private WebSocket _webSocket;
        private readonly HttpListener _listener = new HttpListener();

        public TransporteWebSocket(WebSocket connectedClient)
        {
            _webSocket = connectedClient;
        }

        public async Task IniciarServerAsync()
        {
            //_listener.Start();
            //Console.WriteLine("Servidor WebSocket escuchando...");

            //while (true)
            //{
            //    var context = await _listener.GetContextAsync();

            //    if (context.Request.IsWebSocketRequest)
            //    {
            //        // Aceptar conexión WebSocket
            //        var wsContext = await context.AcceptWebSocketAsync(null);
            //        var webSocket = wsContext.WebSocket;

            //        // Crear instancia de TransporteWebSocket (como haces con TcpClient)
            //        var transporte = new TransporteWebSocket(webSocket);
            //        var newClient = new ClienteConexion(transporte) { Nombre = "" };

            //        Console.WriteLine("Nuevo cliente conectado por WebSocket");

            //        // Aqui habría que usar la función de ManejarClienteLoop em chatService.cs
            //        _ = Task.Run(() => ManejarClienteLoop(newClient));
            //    }
            //    else
            //    {
            //        context.Response.StatusCode = 400;
            //        context.Response.Close();
            //    }
            //}
        } 

        public async Task ConectarAsync(string host, int port)
        {
            var clientWebSocket = new ClientWebSocket();
            await clientWebSocket.ConnectAsync(new Uri($"ws://{host}:{port}"), CancellationToken.None);
            _webSocket = clientWebSocket;

        }

        public void Desconectar()
        {
            _webSocket?.Abort();
            _webSocket?.Dispose();
        }

        public async Task EnviarAsync(TipoMensaje prefijo, string datos)
        {
            var persona = new MensajeCliente { PrefijoMensaje = prefijo, ContenidoMensaje = datos };
            string json = JsonSerializer.Serialize(persona);

            if (_webSocket != null && _webSocket.State == WebSocketState.Open)
            {
                var buffer = Encoding.UTF8.GetBytes(json);


                await _webSocket.SendAsync(
                    new ArraySegment<byte>(buffer),
                    WebSocketMessageType.Text,
                    endOfMessage: true,
                    CancellationToken.None
                );
            }
        }

        public async Task<string> RecibirAsync()
        {
            if (_webSocket == null || _webSocket.State != WebSocketState.Open)
            {
                return null;
            }
                

            var buffer = new byte[1024];
            var mensaje = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (mensaje.MessageType == WebSocketMessageType.Close)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Cerrado", CancellationToken.None);
                return string.Empty;
            }

            return Encoding.UTF8.GetString(buffer, 0, mensaje.Count);
        }
    }



}
