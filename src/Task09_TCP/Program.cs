using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Task09_TCP
{
    partial class Program
    {
        private static IPAddress host;
        private static int port = 5000;
        static void Main(string[] args)
        {
            string dagent = "service"; //"client";
            string arg = args.Length == 0 ? dagent  : args[0];
            Console.WriteLine($"Start TCP {arg}");
            host = IPAddress.Parse("127.0.0.1");

            int variant = 2;

            if (variant == 1)
            {
                if (arg == "service") Service();
                else if (arg == "client") Client();
                else if (arg == "test") TestClient();
            }
            else
            {
                if (arg == "service") Service2();
                else if (arg == "client") Client2();
            }


            var k = Console.ReadKey();
        }
        private static void Service()
        {
            var task = StartServerAsync();
            Task.Run(() => task);
        }
        private static async Task StartServerAsync()
        { 
            TcpListener listener = new TcpListener(host, port);
            listener.Start();
            while (true)
            {
                using (TcpClient client = await listener.AcceptTcpClientAsync())
                using (var stream = client.GetStream())
                {
                    //Console.WriteLine("Client accepted");

                    // принимаем запрос
                    byte[] buff = new byte[1000];
                    int nbytes = stream.Read(buff, 0, buff.Length);
                    string received = System.Text.Encoding.UTF8.GetString(buff, 0, nbytes);
                    //Console.WriteLine(received);

                    // посылаем ответ
                    string resp_message = @"HTTP/1.0 200 OK
Content-Type: text/plain
Content-Length: 6

Hello!";
                    resp_message = "OK.";
                    byte[] resp_arr = System.Text.Encoding.UTF8.GetBytes(resp_message);
                    stream.Write(resp_arr, 0, resp_arr.Length);
                }
            }
        }



        private static void Client()
        {
            var asker = Ask("from client");
            var qu = Task<string>.Run(() => asker);
            Console.WriteLine(qu.Result);
        }
        // Клиент только запрашивает, запрос может сколько-то длиться. 
        private static async Task<string> Ask(string request)
        {
            string response = "noresponse";
            using (TcpClient client = new TcpClient())
            {
                await client.ConnectAsync(host, port);
                using (NetworkStream stream = client.GetStream())
                {
                    // Посылаем
                    byte[] arr = System.Text.Encoding.ASCII.GetBytes(request);
                    stream.Write(arr, 0, arr.Length);
                    // Принимаем
                    System.IO.TextReader reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8);
                    response = reader.ReadToEnd();
                }
            }
            return response;
        }
        private static void TestClient()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Restart();
            for (int i = 0; i < 1000; i++)
            {
                var asker = Ask("from client");
                var qu = Task<string>.Run(() => asker);
                //Console.Write($"{qu.Result} ");
            }
            Console.WriteLine();
            sw.Stop();
            Console.WriteLine($"test ok. duration={sw.ElapsedMilliseconds}");

        }

    }
}
