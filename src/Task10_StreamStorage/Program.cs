using System;
using System.IO;
using System.Linq;
using Polar.PagedStreams;

namespace Task10_StreamStorage
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = "../../Databases/";
            Console.WriteLine("Start Task10_StreamStorage");
            string s_path = path + "storage.bin";

            bool toload = true;

            if (toload && File.Exists(s_path)) File.Delete(s_path);
            Polar.PagedStreams.StreamStorage storage = new StreamStorage(s_path);

            string[] files = null;
            int[] numbers = null;

            if (toload)
            {
                var s1 = storage.CreateStream(out var n1);

                files = Directory.GetFiles(@"C:\Users\Public\Pictures\Sample Pictures");
                numbers = new int[files.Length];
                for (int i = 0; i < files.Length; i++)
                {
                    Console.WriteLine($"geting {files[i]}");
                    Stream destination = storage.CreateStream(out var n);
                    numbers[i] = n;
                    File.Open(files[i], FileMode.Open, FileAccess.Read).CopyTo(destination);
                }

            }

            string dir_new = path + "copies/";
            for (int i = 0; i < files.Length; i++)
            {
                Console.WriteLine($"puting {files[i]}");
                Stream source = storage[numbers[i]];
                source.Position = 0L;
                string filename = files[i].Split('\\').Last();
                Stream dest = File.Create(dir_new + filename);
                source.CopyTo(dest);
                dest.Close();
            }

            storage.Close();

        }
    }
}
