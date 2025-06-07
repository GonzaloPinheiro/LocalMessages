using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LocalMessagesCore.Interfaces; 

namespace LocalMessagesApp.Servicios
{
    /// <summary>
    /// Clase que encapsula toda la lógica de conexión,
    /// envío y recepción de mensajes TCP de forma asíncrona.
    /// </summary>
    public class ChatClient
    {
        private readonly ITransport _transporte;
        private CancellationTokenSource _cts;

        public event Action<string> OnMessageReceived;
        public event Action<bool> OnConnectionStatusChanged;


        /// <summary>
        /// En el constructor, recibe el transporte (por ejemplo, TransporteTcp).
        /// </summary>
        public ChatClient(ITransport transporte)
        {
            _transporte = transporte;
        }


        /// <summary>
        /// Conecta al servidor y envía el nombre de usuario.
        /// También lanza en segundo plano el bucle de recepción.
        /// </summary>
        public async Task ConnectarAsync(string host, int port, string username)
        {
            //_tcp = new TcpClient();
            _cts = new CancellationTokenSource();

            // 1) Conectar usando ITransport
            await _transporte.ConectarAsync(host, port);
            OnConnectionStatusChanged?.Invoke(true);

            // 2) Enviar el nombre de usuario
            await EnviarMensajeAsync(username);

            // 3) Lanzar el bucle de recepción en segundo plano
            _ = Task.Run(RecibirLoop, _cts.Token);
        }


        /// <summary>
        /// Envía un texto al servidor de forma asíncrona.
        /// </summary>
        public async Task EnviarMensajeAsync(string mensaje)
        {
            await _transporte.EnviarAsync(mensaje);
        }


        /// <summary>
        /// Bucle que permanece leyendo datos del servidor
        /// hasta que se cancele o se cierre la conexión.
        /// </summary>
        private async Task RecibirLoop()
        {
            var buffer = new byte[4096];
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    //var n = await _tcp.GetStream().ReadAsync(buffer, 0, buffer.Length, _cts.Token);
                    string mensaje = await _transporte.RecibirAsync();

                    if (mensaje == null)
                    {
                        break;
                    }
                    //var mensaje = Encoding.UTF8.GetString(buffer, 0, n);
                    OnMessageReceived?.Invoke(mensaje);
                }

            }catch(Exception ex)
            {  
            }
            finally
            {
                DesconectarCliente();
            }
        }


        //Desconecta el cliente y cancela el token de cancelación.
        public void DesconectarCliente()
        {
            _cts?.Cancel();
            _transporte.Desconectar();
            OnConnectionStatusChanged?.Invoke(false);
        }

    }
}
