using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Task09_TCP
{
    partial class Program
    {
        private static IPAddress host=IPAddress.Parse("127.0.0.1");
        private static int port = 5000;
        static void Main0(string[] args)
        {
            string dagent = "service"; //"client";
            string arg = args.Length == 0 ? dagent  : args[0];
            Console.WriteLine($"Start TCP {arg}");
            host = IPAddress.Parse("127.0.0.1");

            int variant = 1;

            if (variant == 1)
            {
                if (arg == "service") Service1();
                else if (arg == "client") Client1();
            }
            else
            {
                if (arg == "service") Service2();
                else if (arg == "client") Client2();
                else if (arg == "test") TestClient2();
            }


            var k = Console.ReadKey();
        }

        private static void Service1()
        {
            TcpListener listener = new TcpListener(host, port);
            listener.Start();
            var client = listener.AcceptTcpClient();
            Console.WriteLine("client accepted");
            var stream = client.GetStream();
            byte[] buff = new byte[1000];

            while (true)
            {
                // Принимаем
                int nbytes = stream.Read(buff, 0, buff.Length);
                string received = System.Text.Encoding.UTF8.GetString(buff, 0, nbytes);
                //Console.WriteLine(received);
                if (received == "exit") break;

                // Посылаем
                byte[] arr = System.Text.Encoding.ASCII.GetBytes("OK.");
                stream.Write(arr, 0, arr.Length);
            }

            stream.Close();
            client.Close();
            listener.Stop();
        }
        private static void Client1()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            TcpClient client = new TcpClient();
            client.Connect(host, port);
            Console.WriteLine("client connected");

            var stream = client.GetStream();
            byte[] buff = new byte[1000];

            sw.Restart();
            int nrequ = 10_000;
            for (int i=0; i<nrequ; i++)
            {
                // Посылаем
                byte[] arr = System.Text.Encoding.ASCII.GetBytes("Hello");
                stream.Write(arr, 0, arr.Length);

                // Принимаем
                int nbytes = stream.Read(buff, 0, 3);// buff.Length);
                string received = System.Text.Encoding.UTF8.GetString(buff, 0, nbytes);
                //Console.WriteLine(received);
            }
            sw.Stop();
            Console.WriteLine($"{nrequ} requ/resp ok. duration={sw.ElapsedMilliseconds}");
            //client.Close();
        }

    }
}
