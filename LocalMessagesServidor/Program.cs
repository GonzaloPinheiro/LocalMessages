using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using LocalMessagesCore.Modelos;
using LocalMessagesServidor.Servicios;

namespace LocalMessagesServidor
{
    internal class Program
    {
        static void Main(string[] args)
        {

            try
            {
                Server.Start();

                Console.WriteLine("Servidor iniciado. Presiona ENTER para salir...");
                Console.ReadLine();
            }
            catch
            {
                Console.WriteLine("Error al iniciar el servidor");
            }
        }
    }
}