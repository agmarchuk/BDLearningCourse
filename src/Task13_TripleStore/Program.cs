using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Polar.DB;
using Polar.Cells;
using Polar.CellIndexes;

namespace Task13_TripleStore
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start Task13_TripleStore");
            // Создаем генератор триплетов теста фототека. Генерируем поток массивов трех строк. Входным параметром является количество персон
            Random rnd = new Random();
            Func<int, IEnumerable<string[]>> GenerateTriples = n =>
            {
                // n персон
                var persons = Enumerable.Range(0, n).SelectMany(i => new string[][]
                  {
                    new string[] { "<p" + (n - i - 1) + ">", "<name>", "Pupkin_" + (n - i - 1) }, // имя
                    new string[] { "<p" + (n - i - 1) + ">", "<age>", "33" } // возраст
                  });
                // 2*n фоток
                var photos = Enumerable.Range(0, 2 * n).SelectMany(i => new string[][]
                  {
                    new string[] { "<ph" + (2 * n - i - 1) + ">", "<name>", "DSP_" + (2 * n - i - 1) }, // имя
                    new string[] { "<ph" + (2 * n - i - 1) + ">", "<age>", "33" } // возраст
                  });
                // 6*n отражений
                var reflections = Enumerable.Range(0, 6 * n).SelectMany(i => new string[][]
                  {
                    new string[] { "<re" + (6*n - i - 1) + ">", "<reflected>", "<p" + rnd.Next(n) + ">" }, // отражаемое
                    new string[] { "<re" + (6*n - i - 1) + ">", "<inphoto>", "<ph" + rnd.Next(2*n) + ">" } // в документе
                  });
                return persons.Concat(photos).Concat(reflections);
            };

            PType tp_triple = new PTypeRecord(
                new NamedType("subject", new PType(PTypeEnumeration.sstring)),
                new NamedType("predicate", new PType(PTypeEnumeration.sstring)),
                new NamedType("object", new PType(PTypeEnumeration.sstring)));

            string path = "data";
            int nom = 0;
            TableSimple triples = new TableSimple(tp_triple, new int[] { 0, 2 }, () => File.Open(path + (nom++), FileMode.OpenOrCreate));


            int npersons = 400_000;

            bool toload = true;
            if (toload) // Загрузка данных
            {
                triples.Fill(GenerateTriples(npersons));
            }

            string sample = "<p" + (npersons * 2 / 3) + ">";

            var rfls = triples.GetAllByKey(2, sample).Where(r => (string)r[1] == "<reflected>");
            foreach (object[] r in rfls) Console.WriteLine($"{r[0]} {r[1]} {r[2]}");
            Console.WriteLine();

            var indocs = rfls.SelectMany(r => triples.GetAllByKey(0, r[0])).Where(t => (string)t[1] == "<inphoto>");
            foreach (object[] r in indocs) Console.WriteLine($"{r[0]} {r[1]} {r[2]}");
            Console.WriteLine();

            var phs = indocs.SelectMany(r => triples.GetAllByKey(0, r[2])).Where(t => (string)t[1] == "<name>");
            foreach (object[] r in phs) Console.WriteLine($"{r[0]} {r[1]} {r[2]}");
            Console.WriteLine();

            // Теперь цикл по образцам
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            int nsamples = 1000;
            sw.Start();
            int count = 0;
            for (int i=0; i<nsamples; i++)
            {
                sample = "<p" + rnd.Next(npersons) + ">";
                var que = triples.GetAllByKey(2, sample).Where(r => (string)r[1] == "<reflected>")
                    .SelectMany(r => triples.GetAllByKey(0, r[0])).Where(t => (string)t[1] == "<inphoto>")
                    .SelectMany(r => triples.GetAllByKey(0, r[2])).Where(t => (string)t[1] == "<name>");
                //foreach (object[] r in que) Console.WriteLine($"{r[0]} {r[1]} {r[2]}");
                //Console.WriteLine();
                count += que.Count();
            }
            sw.Stop();
            Console.WriteLine($"count={count} duration={sw.ElapsedMilliseconds}");

        }
    }
}
