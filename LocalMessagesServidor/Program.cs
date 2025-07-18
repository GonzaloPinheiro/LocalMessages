using LocalMessagesServidor.Servicios;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LocalMessagesServidor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Server.Start();

                Console.WriteLine("Servidor iniciado. Presiona ENTER para salir (no implementado el enter)...");
                Console.ReadLine();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error al iniciar el servidor");

                Console.WriteLine(ex.Message);
            }
        }
    }
}