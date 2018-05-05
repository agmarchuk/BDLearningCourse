using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Task09_TCP
{
    partial class Program1
    {
        private static IPAddress host = IPAddress.Parse("127.0.0.1");
        private static int port = 5000;
        static void Main(string[] args)
        {
            Console.WriteLine("Start TCP Server");
            Server();
        }
        static void Server()
        {
            TcpListener listener = new TcpListener(host, port);
            listener.Start();
            while(true)
            {
                var client = listener.AcceptTcpClient();
                Console.WriteLine("client accepted");
                var stream = client.GetStream();
                byte[] buff = new byte[1000];

                // Принимаем
                int nbytes = stream.Read(buff, 0, buff.Length);
                string received = System.Text.Encoding.UTF8.GetString(buff, 0, nbytes);
                Console.WriteLine(received);
                if (received == "exit") break;

                // Посылаем
                //string message = "OK " + DateTime.Now.ToString();
                string message = @"HTTP/1.0 200 OK
Content-Type: text/plain
Content-Length: 6

Hello!";
                byte[] arr = System.Text.Encoding.ASCII.GetBytes(message);
                stream.Write(arr, 0, arr.Length);

                stream.Close();
                client.Close();
            }
            listener.Stop();
        }
    }
}
