using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Polar.DB;
using Polar.Cells;
using Polar.CellIndexes;

namespace Task04_Sequenses
{
    partial class Program
    {
        public static void Main6(string[] args)
        {
            Random rnd = new Random();
            Console.WriteLine("Start Task04_Sequenses_Main6");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            PType tp_person = new PTypeRecord(
                new NamedType("id", new PType(PTypeEnumeration.integer)),
                new NamedType("name", new PType(PTypeEnumeration.sstring)),
                new NamedType("age", new PType(PTypeEnumeration.real)));
            string tablegroup = "../../../" + path + "data";
            int fnom = 0;
            TableRelational table = new TableRelational(tp_person, () => System.IO.File.Open(tablegroup + fnom++, System.IO.FileMode.OpenOrCreate));
            //table.IndexInt(0);
            //table.IndexString(1);
            table.Indexes(new int[] { 0 });

            int nelements = 1_000_000;
            bool toload = true; // Загружать или нет новую базу данных
            if (toload)
            {
                sw.Restart();
                // Очистим ячейки последовательности и индекса 
                table.Clear();

                IEnumerable<object> flow = Enumerable.Range(0, nelements)
                    .Select(i =>
                    {
                        int id = nelements - i;
                        string name = "=" + id.ToString() + "=";
                        double age = rnd.NextDouble() * 100.0;
                        return new object[] { id, name, age };
                    });
                table.Fill(flow);

                // Теперь надо отсортировать индексный массив по ключу
                table.BuildIndexes();
                sw.Stop();
                Console.WriteLine("Load ok. duration for {0} elements: {1} ms", nelements, sw.ElapsedMilliseconds);
            }
            else
            {
                //sw.Restart();
                //table.Warmup();
                //sw.Stop();
                //Console.WriteLine("Warmup ok. duration for {0} elements: {1} ms", nelements, sw.ElapsedMilliseconds);
            }

            // Проверим работу
            int search_key = nelements * 2 / 3;
            //var ob = index_person_name.GetAllByKey("=" + search_key + "=")
            //    .Select(ent => ((object[])ent.Get())[1])
            //    .FirstOrDefault();
            var ob = table.GetAllByKey(0, search_key).FirstOrDefault();
            //if (ob == null) throw new Exception("Didn't find person " + search_key);
            Console.WriteLine("Person {0} has name {1}", search_key, ((object[])ob)[1]);


            // Засечем скорость выборок
            int nprobe = 1000;
            sw.Restart();
            for (int i = 0; i < nprobe; i++)
            {
                search_key = rnd.Next(nelements) + 1;
                ob = table.GetAllByKey(0, search_key)
                    .FirstOrDefault();
                if (ob == null) throw new Exception("Didn't find person " + search_key);
                string nam = (string)((object[])ob)[1];
            }
            sw.Stop();
            Console.WriteLine($"Duration for {nprobe} search in {nelements} elements: {sw.ElapsedMilliseconds} ms");

        }
    }
}
