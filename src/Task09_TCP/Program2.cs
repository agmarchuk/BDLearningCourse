using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Task09_TCP
{
    partial class Program
    {
        private static void Service2()
        {
            TcpListener listener = new TcpListener(host, port);
            listener.Start();
            var client = listener.AcceptTcpClient();
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
        private static void Client2()
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
