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
        public static void Main2(string[] args)
        {
            Random rnd = new Random();
            Console.WriteLine("Start Task04_Sequenses_Main2");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            PType tp_person = new PTypeRecord(
                new NamedType("id", new PType(PTypeEnumeration.integer)),
                new NamedType("name", new PType(PTypeEnumeration.sstring)),
                new NamedType("rage", new PType(PTypeEnumeration.real)));

            TableView tab_person = new TableView(path + "tab_person.pac", tp_person);
            Func<object, int> person_code_keyproducer = v => (int)((object[])((object[])v)[1])[0];
            IndexKeyImmutable<int> ind_arr_person = new IndexKeyImmutable<int>(path + "person_ind")
            {
                Table = tab_person,
                KeyProducer = person_code_keyproducer,
                Scale = null
            };
            ind_arr_person.Scale = new ScaleCell(path + "person_ind") { IndexCell = ind_arr_person.IndexCell };
            IndexDynamic<int, IndexKeyImmutable<int>> index_person = new IndexDynamic<int, IndexKeyImmutable<int>>(true)
            {
                Table = tab_person,
                IndexArray = ind_arr_person,
                KeyProducer = person_code_keyproducer
            };
            tab_person.RegisterIndex(index_person);

            int nelements = 1000000;
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

            // Проверим работу
            int search_key = nelements * 2 / 3;
            var ob = index_person.GetAllByKey(search_key)
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
                ob = index_person.GetAllByKey(search_key)
                    .Select(ent => ((object[])ent.Get())[1])
                    .FirstOrDefault();
                if (ob == null) throw new Exception("Didn't find person " + search_key);
                string nam = (string)((object[])ob)[1];
            }
            sw.Stop();
            Console.WriteLine($"Duration for {nprobe} search in {nelements} elements: {sw.ElapsedMilliseconds} ms");


        }
    }
}
