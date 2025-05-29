using LocalMessagesApp.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LocalMessagesApp
{

    //Partial class para manejar los comandos del chat
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
                MessageBox.Show("Uso: /nick <nuevoNombre>");
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
