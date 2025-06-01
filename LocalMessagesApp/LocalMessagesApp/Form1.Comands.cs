using LocalMessagesApp.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LocalMessagesApp
{

    //Partial class para manejar los comandos del chat del usuario
    public partial class AppForm1 : Form
    {
        private async Task ProcesarEntrada(string texto)
        {
            //Es comando
            if (texto.StartsWith("/"))
            {
                await ManejarComando(texto);
            }
            else //No es comando
            {
                await _chat.EnviarMensaje(texto);
            }
        }



        //Método para manejar los comandos
        private async Task ManejarComando(string texto)
        {
            var (comando, args) = ParsearComando(texto);

            switch (comando)
            {
                // Comando para cambiar el nombre de usuario
                case "nick":
                    await CmdNuevoNick(args);
                    break;

                case "list":
                    await CmdListaUsuarios();
                    break;

                case "connect":
                    await CmdConectar(args);
                    break;

                case "disconnect":
                    await CmdDesconectar();
                    break;

                default:
                    MessageBox.Show("Comando no reconocido");
                    break;

            }
        }

        //Comando para parsear la entrada del usuario
        private async Task CmdNuevoNick(string[] args)
        {
            if (args.Length != 1)
            {
                MessageBox.Show("Estructura correcta: /nick <nuevoNombre>");
                return;
            }
            var nuevoNombre = args[0];
            
            await _chat.EnviarMensaje($"CMD|nick|{nuevoNombre}");
        }

        //Comando para listar usuarios conectados
        private async Task CmdListaUsuarios()
        {
            await _chat.EnviarMensaje($"CMD|list");
        }

        //Comando para conectarse al servidor
        private async Task CmdConectar(string[] args)
        {
            if(args.Length != 1)
            {
                MessageBox.Show("Estructura  correcta: /connect <nombre>");
                return;
            }
            var nick = args[0];

            // Conectar de forma asíncrona
            await _chat.ConnectAsync("127.0.0.1", 1234, nick);
            tbxUserName.Text = nick;
        }

        //Comando para desconectarse del servidor
        private async Task CmdDesconectar()
        {
            _chat.DesconectarCliente();
        }

        // Método para parsear el comando y sus argumentos
        private (string comando, string[] args) ParsearComando(string texto)
        {
            // Quita la barra inicial y separa por espacios
            var trozos = texto.Substring(1).Split(' ', (char)StringSplitOptions.RemoveEmptyEntries);
            var comando = trozos[0].ToLower();           //Comando "/nick" o "/list" por ejemplo
            var args = trozos.Skip(1).ToArray();      // Argumentos del comando, si los hay (nuevo nick "Ana")
            return (comando, args);
        }
    }
}
