using LocalMessagesApp.Servicios;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace LocalMessagesApp
{
    public partial class AppForm1 : Form
    {
        private readonly ChatClient _chat = new ChatClient();

        public AppForm1()
        {
            InitializeComponent();

            //Mostrar el mensaje recibido en el ListBox
            _chat.OnMessageReceived += msg =>
            {
                //Invocar en el hilo de la UI para evitar cross-thread errors
                Invoke((Action)(() => lbxChat.Items.Add(msg)));
            };


            // Mostrar el estado de la conexión
            _chat.OnConnectionStatusChanged += connected =>
            {
                Invoke((Action)(() =>
                {
                    // Cambiar el texto del botón y mostrar notificación
                    btnConection.Text = connected ? "Disconnect" : "Connect";
                    MessageBox.Show(connected ? "Connected" : "Disconnected");
                }));
            };
        }



        // Botón Conectar/Desconectar
        private async void btnConection_Click(object sender, EventArgs e)
        {
            if (btnConection.Text == "Disconnect")
            {
                _chat.Disconnect();
            }
            else
            {
                var nick = tbxUserName.Text.Trim();
                if (string.IsNullOrEmpty(nick))
                {
                    MessageBox.Show("Please enter a username");
                    return;
                }
                // Conectar de forma asíncrona
                await _chat.ConnectAsync("127.0.0.1", 1234, nick);
            }
        }



        // Botón Enviar
        private async void BtnSend_Click(object sender, EventArgs e)
        {
            var text = tbxMessage.Text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Please enter a message");
                return;
            }
            // Enviar mensaje
            await _chat.SendAsync(text);
            tbxMessage.Clear();
        }
    }
}