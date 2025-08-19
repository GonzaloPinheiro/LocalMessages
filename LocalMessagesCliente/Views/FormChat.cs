using LocalMessagesApp.Servicios;
using LocalMessagesCore.Interfaces;
using LocalMessagesCore.Modelos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

//V2.0
namespace LocalMessagesApp.Views
{
    public partial class FormChat : Form
    {
        private  ITransport _transporte;
        private  ChatClient _chat;

        private int puerto = 1234;

        public FormChat()
        {
            InitializeComponent();
            cbxTipoConexion.DataSource = Properties.Settings.Default.CanalesChat;
            cbxTipoConexion.SelectedIndex = 0;        
        }


        // Botón Conectar/Desconectar
        private async void btnConection_Click(object sender, EventArgs e)
        {
            if (btnConection.Text == "Desconectar")
            {
                _chat.DesconectarCliente();
                lbxChat.Items.Clear();
                lbxUssers.Items.Clear();
            }
            else
            {
                // Elige el transporte según lo seleccionado en el ComboBox
                var tipo = cbxTipoConexion.SelectedItem?.ToString();

                switch (tipo)
                {
                    case "TCP":
                        puerto = 1234;
                        _transporte = new TransporteTcp();
                        _chat = new ChatClient(_transporte);
                        break;
                    case "WebSocket":
                        puerto = 1235;
                        _transporte = new TransporteWebSocket();
                        _chat = new ChatClient(_transporte);
                        break;
                    default:
                        _transporte = new TransporteTcp();
                        _chat = new ChatClient(_transporte);
                        break;
                }

                SuscribirEventosChat();


                var nick = tbxUserName.Text.Trim();


                if (string.IsNullOrEmpty(nick))
                {
                    MessageBox.Show("Porfavor introduzca un nombre de usuario");
                    return;
                }


                //Conectar de forma asíncrona
                await _chat.ConnectarAsync("127.0.0.1", puerto, nick);

                //Enviar el nombre de usuario al servidor
                if(cbxTipoConexion.SelectedItem?.ToString() == "WebSocket") 
                {
                    await _chat.EnviarMensajeAsync(TipoMensaje.CNX_WST, nick);
                }
                else if(cbxTipoConexion.SelectedItem?.ToString() == "TCP")
                {
                    await _chat.EnviarMensajeAsync(TipoMensaje.CNX_TCP, nick);
                }
                else
                {
                    MessageBox.Show("Error al enviar el nombre de usuario");
                    _chat.DesconectarCliente();
                }

                //Solicitar ultimos mensajes?
            }
        }



        //Botón Enviar
        private async void BtnSend_Click(object sender, EventArgs e)
        {
            var text = tbxMessage.Text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Por favor introduzca un mensaje");
                return;
            }

            // Procesar la entrada del usuario y comprueba si es mensaje o comando
            //await ProcesarEntrada(text);
            if(text.StartsWith("/"))
            {
                // Enviar comando
                await _chat.EnviarMensajeAsync(TipoMensaje.CMD_Nick, text);
            }
            else
            {
                // Enviar mensaje normal
                await _chat.EnviarMensajeAsync(TipoMensaje.TXT, text);
            }
            

            
            tbxMessage.Clear();
            tbxMessage.Focus();
        }


        /// <summary>
        /// Configura los handlers de los eventos de ChatClient.
        /// </summary>
        private void SuscribirEventosChat()
        {
            _chat.OnMessageReceived += ProcesarMensajeRecibido;
            _chat.OnConnectionStatusChanged += Chat_OnConnectionStatusChanged;
        }

        /// <summary>
        /// Actualiza la UI cuando cambia el estado de la conexión.
        /// </summary>
        private void Chat_OnConnectionStatusChanged(bool conectado)
        {
            Invoke((Action)(() =>
            {
                btnConection.Text = conectado ? "Desconectar" : "Conectar";
                MessageBox.Show(conectado ? "Conectado" : "Desconectado");
            }));
        }

        /// <summary>
        /// Maneja los mensajes entrantes.
        /// </summary>
        private void ProcesarMensajeRecibido(string msg)
        {
            MensajeCliente mensajeDeserializado = JsonSerializer.Deserialize<MensajeCliente>(msg);

            switch(mensajeDeserializado.PrefijoMensaje)
            {
                case TipoMensaje.TXT:
                    // Mensaje de texto normal
                    Invoke((Action)(() => lbxChat.Items.Add(mensajeDeserializado.ContenidoMensaje)));
                    break;

                case TipoMensaje.CMD_List:
                    // Mensaje de lista de usuarios

                    // Split usando la sobrecarga correcta
                    var partes = mensajeDeserializado.ContenidoMensaje.Split(
                        new[] { '|' },
                        StringSplitOptions.RemoveEmptyEntries
                    );

                    if (partes.Length < 2) return;
                    var comando = partes[1].ToLower();
                    var nombres = partes.Skip(2);

                    Invoke((Action)(() =>
                    {
                        lbxUssers.Items.Clear();
                        foreach (var nombre in nombres)
                            lbxUssers.Items.Add(nombre);
                    })); 
                    
                    //ProcesarListaUsuarios(mensajeDeserializado.ContenidoMensaje);
                    break;

                //case TipoMensaje.CNX_TCP:
                //case TipoMensaje.CNX_WST:
                //    // Mensaje de conexión
                //    Invoke((Action)(() => lbxChat.Items.Add($"[Conexión] {mensajeDeserializado.ContenidoMensaje}")));
                //    break;
                default:
                    // Otros tipos de mensajes
                    Invoke((Action)(() => lbxChat.Items.Add($"[Desconocido] {mensajeDeserializado.ContenidoMensaje}")));
                    break;
            }
        }
    }
}