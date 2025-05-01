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
        Thread connection;
        Thread receptor;
        TcpClient client = new TcpClient();


        string userName;

        public AppForm1()
        {
            InitializeComponent();
        }

        //Btn to connect to the server
        private void btnConection_Click(object sender, EventArgs e)
        {
            if (btnConection.Text == "Disconnect")
            {
                client.Close();
                btnConection.Text = "Connect";
                MessageBox.Show("Disconnected");
                return;
            }
            else
            {
                if (tbxUserName.Text == "")
                {
                    MessageBox.Show("Please enter a username");
                    return;
                }
                else
                {
                    userName = tbxUserName.Text;
                    connection = new Thread(Conect);
                    connection.Start();
                }
            }
        }


        //Btn to send a message
        private void BtnSend_Click(object sender, EventArgs e)
        {
            if(tbxMessage.Text == "")
            {
                MessageBox.Show("Please enter a message");
                return;
            }
            else
            {
                sendMessage(tbxMessage.Text);
                tbxMessage.Text = "";
            }
        }



        //Try to connect to the server, if successful: launch thread to listen for messages
        private void Conect()
        {
            try
            {
                client.Connect("127.0.0.1", 1234);

                sendMessage(userName);

                this.Invoke((MethodInvoker)delegate {
                    btnConection.Text = "Disconnect";
                });

                MessageBox.Show("Connected");

                //Lanzo un hilo para ponerlo en escucha por los mensajes del servidor
                receptor = new Thread(reciveMessage);
                receptor.Start();
            }
            catch (Exception ex)
            {
                client.Dispose();
                MessageBox.Show("Connection error");
                MessageBox.Show(ex.Message);
            }
        }

        //Send message to the server
        private void sendMessage(string message)
        {
            try
            {
                Byte[] sendBytes = Encoding.ASCII.GetBytes(message);

                client.GetStream().Write(sendBytes, 0, sendBytes.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending message: " + ex.Message);
            }
        }

        private void reciveMessage()
        {
            try
            {
                int longitudRespuesta;
                string respuesta;
                Byte[] bytesRecibidos = new Byte[256];

                while (true)
                {

                    //Respuesta del servidor
                    longitudRespuesta = client.GetStream().Read(bytesRecibidos, 0, bytesRecibidos.Length);
                    respuesta = Encoding.ASCII.GetString(bytesRecibidos, 0, bytesRecibidos.Length);

                    if (longitudRespuesta > 0)
                    {
                        //Convertir los bytes a un string
                        string mensaje = Encoding.ASCII.GetString(bytesRecibidos, 0, longitudRespuesta);

                        //Actualizar el TextBox en el hilo principal
                        this.Invoke((MethodInvoker)delegate
                        {
                            lbxChat.Items.Add(mensaje);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Conexión perdida: " + ex.Message);
                client.Close();
            }
        }


    }
}
