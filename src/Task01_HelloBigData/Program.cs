using System;
using System.IO;

namespace Task01_HelloBigData
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            Console.WriteLine("Hello World!");
            FileStream fs = File.Open("data.bin", FileMode.OpenOrCreate);
            BinaryWriter bw = new BinaryWriter(fs);
            BinaryReader br = new BinaryReader(fs);

            long nelements = 100000000;
            bool toload = false;

            if (toload)
            {
                sw.Restart();
                for (long ii = 0L; ii < nelements; ii++) bw.Write(ii);
                fs.Flush();
                sw.Stop();
                Console.WriteLine($"load time {sw.ElapsedMilliseconds}");
            }
            else
            {
                fs.Position = 0L;
                sw.Restart();
                byte[] buffer = new byte[100000];
                int nblocks = (int)(nelements * 8 / buffer.Length);
                for (int i = 0; i < nblocks; i++) fs.Read(buffer, 0, buffer.Length);
                //for (long ii = 0L; ii < nelements; ii++) br.ReadInt64();
                sw.Stop();
                Console.WriteLine($"warm up time {sw.ElapsedMilliseconds}");
            }



            fs.Position = (nelements * 2 / 3) * 8;
            long v = br.ReadInt64();
            Console.WriteLine($"v = {v}");

            sw.Restart();
            Random rnd = new Random();
            long nreads = 1000000;
            for (long ii = 0; ii < nreads; ii++)
            {
                long ind = rnd.Next((int)nelements);
                fs.Position = ind * 8;
                long val = br.ReadInt64();
                if (val != ind) throw new Exception($"Err: ind={ind} val={val}");
            }
            sw.Stop();
            Console.WriteLine($"elapsed {sw.ElapsedMilliseconds} ms.");
        }
    }
}
