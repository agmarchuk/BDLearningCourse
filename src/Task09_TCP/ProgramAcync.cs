using System;
using System.Threading.Tasks;

namespace Task09_TCP
{
    class ProgramAcync
    {
        public static void Main()
        {
            Console.WriteLine("Start ProgramAsync");
            //MethodA();
            //Task.Run(() => MethodA());
            System.Collections.Generic.LinkedList<Task> tlist = new System.Collections.Generic.LinkedList<Task>();
            int ntasks = 10;
            Task[] tarr = new Task[ntasks];
            for (int i = 0; i< 10; i++)
            {
               tarr[i] = Task.Run(() => MethodA());
            }
            Console.WriteLine("Finish");
            Task.WaitAll(tarr);
            Console.WriteLine("All");
            Console.ReadKey();
        }
        private static int cnt = 0;
        //private static async Task MethodA()
        private static void MethodA()
        {
            int n = ++cnt;
            Console.Write($"MethodA starts {n} ");
            for (long i = 0; i < 1_000_000_000; i++) { var r = i * 2; }
            Console.WriteLine($"finish {n}. ");
        }
    }
}
