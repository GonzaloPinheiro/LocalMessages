using LocalMessagesCore.Interfaces;
using LocalMessagesCore.Modelos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LocalMessagesApp.Servicios
{
    internal class TransporteTcp : ITransport
    {
        private TcpClient _cliente;

        /// <summary>
        /// Conecta de forma asíncrona al servidor en host:puerto.
        /// </summary>
        public async Task ConectarAsync(string host, int port)
        {
            _cliente = new TcpClient();

            try
            {
                // Este await no bloquea la UI; espera hasta conectar o falle
                await _cliente.ConnectAsync(host, port);
            }
            catch (Exception ex)
            {
                //Si falla la conexión, desechamos el cliente y lanzamos excepción con contexto
                _cliente = null;
                throw new InvalidOperationException($"No se pudo conectar a {host}:{port}.", ex);
            }
        }


        /// <summary>
        /// Envía el texto (por ejemplo JSON) al servidor.
        /// </summary>
        public async Task EnviarAsync(TipoMensaje prefijo, string datos)
        {
            if (_cliente == null || !_cliente.Connected)
                throw new InvalidOperationException("Primero debes conectar antes de enviar.");

            var persona = new MensajeCliente { PrefijoMensaje = prefijo, ContenidoMensaje = datos };
            string json = JsonSerializer.Serialize(persona);

            try
            {
                var buffer = Encoding.UTF8.GetBytes(json);
                await _cliente.GetStream().WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                //Si algo sale mal al enviar, cerrar la conexión
                _cliente?.Close();
                _cliente = null;
                throw new InvalidOperationException($"No se pudo enviar el mensaje: «{json}».", ex);
            }
        }


        /// <summary>
        /// Recibe un texto (por ejemplo JSON) de forma asíncrona.
        /// </summary>
        public async Task<string> RecibirAsync()
        {
            if (_cliente == null || !_cliente.Connected)
                throw new InvalidOperationException("No conectado.");

            var buffer = new byte[4096];
            try
            {
                int cantidadLeida = await _cliente.GetStream().ReadAsync(buffer, 0, buffer.Length);
                if (cantidadLeida == 0)
                {
                    // El servidor cerró la conexión
                    _cliente.Close();
                    return null;
                }

                return Encoding.UTF8.GetString(buffer, 0, cantidadLeida);
            }
            catch (Exception ex)
            {
                //Falla en la lectura de datos
                throw new InvalidOperationException("Error al recibir datos(cliente desconectado?).", ex);
            }
            //try
            //{
            //    var stream = _cliente.GetStream();
            //    using (var reader = new StreamReader(stream, Encoding.UTF8, false, 4096, true))
            //    {
            //        string linea = await reader.ReadLineAsync();
            //        if (linea == null)
            //        {
            //            // El servidor cerró la conexión
            //            _cliente.Close();
            //            return null;
            //        }
            //        return linea;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    throw new InvalidOperationException("Error al recibir datos(cliente desconectado?).", ex);
            //}
        }


        /// <summary>
        /// Cierra la conexión TCP.
        /// </summary>
        public void Desconectar()
        {
            _cliente?.Close();
            _cliente = null;
        }

        public Task IniciarServerAsync()
        {
            throw new NotImplementedException();
        }
    }
}
