using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LocalMessagesApp.Servicios
{
    /// <summary>
    /// Clase que encapsula toda la lógica de conexión,
    /// envío y recepción de mensajes TCP de forma asíncrona.
    /// </summary>
    public class ChatClient
    {
        private TcpClient _tcp;
        private CancellationTokenSource _cts;

        public event Action<string> OnMessageReceived;
        public event Action<bool> OnConnectionStatusChanged;



        /// <summary>
        /// Conecta al servidor y envía el nombre de usuario.
        /// También lanza en segundo plano el bucle de recepción.
        /// </summary>
        public async Task ConnectAsync(string host, int port, string username)
        {
            _tcp = new TcpClient();
            _cts = new CancellationTokenSource();

            await _tcp.ConnectAsync(host, port);
            OnConnectionStatusChanged?.Invoke(true);

            //Enviar el nombre de usuario
            await SendAsync(username);

            //Variable discard, no se va a usar. Solo la uso para lanzar el task.
            _ = Task.Run(ReciveLoop, _cts.Token);
        }


        /// <summary>
        /// Envía un texto al servidor de forma asíncrona.
        /// </summary>
        public async Task SendAsync(string mensaje)
        {
            var data = Encoding.UTF8.GetBytes(mensaje);
            await _tcp.GetStream().WriteAsync(data, 0, data.Length);
        }


        /// <summary>
        /// Bucle que permanece leyendo datos del servidor
        /// hasta que se cancele o se cierre la conexión.
        /// </summary>
        private async Task ReciveLoop()
        {
            var buffer = new byte[4096];
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    var n = await _tcp.GetStream().ReadAsync(buffer, 0, buffer.Length, _cts.Token);
                    if (n == 0)
                    {
                        break;
                    }
                    var mensaje = Encoding.UTF8.GetString(buffer, 0, n);
                    OnMessageReceived?.Invoke(mensaje);
                }

            }catch(Exception ex)
            {  
            }
            finally
            {
                Disconnect();
            }
        }

        public void Disconnect()
        {
            _cts?.Cancel();
            _tcp?.Close();
            OnConnectionStatusChanged?.Invoke(false);
        }

    }
}
