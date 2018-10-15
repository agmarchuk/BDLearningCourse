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

            var fs = File.Open("data.bin", FileMode.OpenOrCreate);
            var subj_ind = File.Open("subj_ind.bin", FileMode.OpenOrCreate);
            var objj_ind = File.Open("obj_ind.bin", FileMode.OpenOrCreate);

            PType tp_triple = new PTypeRecord(
                new NamedType("subject", new PType(PTypeEnumeration.sstring)),
                new NamedType("predicate", new PType(PTypeEnumeration.sstring)),
                new NamedType("object", new PType(PTypeEnumeration.sstring)));
            PaCell triples = new PaCell(new PTypeSequence(tp_triple), fs, false);
            Polar.CellIndexes.IndexHalfkeyImmutable<string> subj_index_arr = new IndexHalfkeyImmutable<string>(subj_ind)
            {
                //Table = triples,
            };

            int npersons = 400_000;

            bool toload = true;
            if (toload)
            {
                // Загрузка данных
                triples.Clear();
                triples.Fill(new object[0]);
                foreach (string[] triple in GenerateTriples(npersons))
                {
                    //Console.WriteLine($"{triple[0]} {triple[1]} {triple[2]} .");
                    triples.Root.AppendElement(new object[] { triple[0], triple[1], triple[2] });
                }
                triples.Flush();
            }


        }
    }
}
