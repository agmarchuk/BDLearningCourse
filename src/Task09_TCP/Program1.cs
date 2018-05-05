using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Task09_TCP
{
    partial class Program1
    {
        private static IPAddress host = IPAddress.Parse("127.0.0.1");
        private static int port = 5000;
        private static System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        static void Main(string[] args)
        {
            if (args.Any(s => s == "client"))
            {
                Console.WriteLine("Start TCP Client");
                Client();
            }
            else if (args.Any(s => s == "clients"))
            {
                Console.WriteLine("Start TCP Clients");
                sw.Restart();
                for (int i=0; i<1000; i++)
                {
                    Client();
                }
                sw.Stop();
                Console.WriteLine($"duration={sw.ElapsedMilliseconds}");
            }
            else
            {
                Console.WriteLine("Start TCP Server");
                Server();
            }
        }
        static void Server()
        {
            TcpListener listener = new TcpListener(host, port);
            listener.Start();
            while(true)
            {
                var client = listener.AcceptTcpClient();
                //Console.WriteLine("client accepted");
                var stream = client.GetStream();
                byte[] buff = new byte[1000];

                // Принимаем
                int nbytes = stream.Read(buff, 0, buff.Length);
                string received = System.Text.Encoding.UTF8.GetString(buff, 0, nbytes);
                //Console.WriteLine(received);

                // Посылаем
                string message = "OK";
                byte[] arr = System.Text.Encoding.ASCII.GetBytes(message);
                stream.Write(arr, 0, arr.Length);

                stream.Close();
                client.Close();
            }
            listener.Stop();
        }
        static void Client()
        {
            TcpClient client = new TcpClient();
            client.Connect(host, port);
            //Console.WriteLine("client connected");

            var stream = client.GetStream();
            byte[] buff = new byte[1000];

            // Посылаем
            byte[] arr = System.Text.Encoding.ASCII.GetBytes("request");
            stream.Write(arr, 0, arr.Length);

            // Принимаем
            int nbytes = stream.Read(buff, 0, 3);// buff.Length);
            string received = System.Text.Encoding.UTF8.GetString(buff, 0, nbytes);
            //Console.WriteLine(received);

            client.Close();
        }
    }
}
