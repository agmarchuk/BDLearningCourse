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
        public static void Main3(string[] args)
        {
            Random rnd = new Random();
            Console.WriteLine("Start Task04_Sequenses_Main3");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            PType tp_person = new PTypeRecord(
                new NamedType("id", new PType(PTypeEnumeration.integer)),
                new NamedType("name", new PType(PTypeEnumeration.sstring)),
                new NamedType("age", new PType(PTypeEnumeration.real)));

            TableView tab_person = new TableView(path + "tab_person.pac", tp_person);
            IndexViewImmutable<string> name_index = new IndexViewImmutable<string>(path + "name_index.pac")
            {
                Table = tab_person,
                KeyProducer = v => (string)((object[])((object[])v)[1])[1],
                tosort = true
            };
            IndexDynamic<string, IndexViewImmutable<string>> index_person_name = new IndexDynamic<string, IndexViewImmutable<string>>(false, name_index);
            tab_person.RegisterIndex(index_person_name);

            int nelements = 1_000_000;
            bool toload = true; // Загружать или нет новую базу данных
            if (toload)
            {
                sw.Restart();
                // Очистим ячейки последовательности и индекса 
                tab_person.Clear();

                IEnumerable<object> flow = Enumerable.Range(0, nelements)
                    .Select(i =>
                    {
                        int id = nelements - i;
                        string name = "=" + id.ToString() + "=";
                        double age = rnd.NextDouble() * 100.0;
                        return new object[] { id, name, age };
                    });
                tab_person.Fill(flow);

                // Теперь надо отсортировать индексный массив по ключу
                tab_person.BuildIndexes();
                sw.Stop();
                Console.WriteLine("Load ok. duration for {0} elements: {1} ms", nelements, sw.ElapsedMilliseconds);
            }
            else
            {
                tab_person.Warmup();
            }

            // Проверим работу
            int search_key = nelements * 2 / 3;
            var ob = index_person_name.GetAllByKey("=" + search_key +"=")
                .Select(ent => ((object[])ent.Get())[1])
                .FirstOrDefault();
            if (ob == null) throw new Exception("Didn't find person " + search_key);
            Console.WriteLine("Person {0} has name {1}", search_key, ((object[])ob)[1]);

            // Засечем скорость выборок
            int nprobe = 1000;
            sw.Restart();
            for (int i = 0; i < nprobe; i++)
            {
                search_key = rnd.Next(nelements) + 1;
                ob = index_person_name.GetAllByKey("=" + search_key + "=")
                    .Select(ent => ((object[])ent.Get())[1])
                    .FirstOrDefault();
                if (ob == null) throw new Exception("Didn't find person " + search_key);
                string nam = (string)((object[])ob)[1];
            }
            sw.Stop();
            Console.WriteLine($"Duration for {nprobe} search in {nelements} elements: {sw.ElapsedMilliseconds} ms");

            string search_string = "=66666";
            var query = index_person_name.GetAllByLevel((PaEntry entry) => 
            {
                var name = (string)((object[])((object[])entry.Get())[1])[1];
                if (name.StartsWith(search_string)) return 0;
                return name.CompareTo(search_string);
            });
            foreach (object v in query.Select(entry => ((object[])entry.Get())[1]))
            {
                Console.WriteLine(tp_person.Interpret(v));
            }
        }
    }
}
