using System;
using Polar.DB;
using Polar.Cells;

namespace Task04_Sequenses
{
    partial class Program
    {
        static string path = "../../Databases/";
        static void Main(string[] args)
        {
            Main3(args);
        }
        public static void Main1(string[] args)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            PType tp_rec = new PTypeRecord(
                new NamedType("id", new PType(PTypeEnumeration.integer)),
                new NamedType("name", new PType(PTypeEnumeration.sstring)),
                new NamedType("age", new PType(PTypeEnumeration.integer)));
            PaCell cell = new PaCell(new PTypeSequence(tp_rec), path + "people.pac", false);
            PType tp_pair = new PTypeRecord(
                new NamedType("id", new PType(PTypeEnumeration.integer)),
                new NamedType("offset", new PType(PTypeEnumeration.longinteger)));
            PaCell cell_index = new PaCell(new PTypeSequence(tp_pair), path + "people_index.pac", false);

            int npersons = 1_000_000;
            Random rnd = new Random();
            Console.WriteLine("Start Task04: Main1");
         
            // Загрузка данных
            sw.Restart();
            cell.Clear();
            cell.Fill(new object[0]);
            cell_index.Clear();
            cell_index.Fill(new object[0]);
            for (int i = 0; i < npersons; i++)
            {
                int code = npersons - i;
                long offset = cell.Root.AppendElement(new object[] { code, "=" + code + "=", rnd.Next(120) });
                cell_index.Root.AppendElement(new object[] { code, offset });
            }
            cell.Flush();
            cell_index.Flush();
            cell_index.Root.SortByKey<int>(pair => (int)((object[])pair)[0]);
            sw.Stop();
            Console.WriteLine($"load {npersons} records. duration {sw.ElapsedMilliseconds}");

            // Поиск
            int key = npersons * 2 / 3;
            int ntests = 10000;
            sw.Restart();
            PaEntry entry = cell.Root.Element(0);
            for (int j = 0; j < ntests; j++)
            {
                key = rnd.Next(npersons);
                PaEntry en = cell_index.Root.BinarySearchFirst(ent => ((int)((object[])ent.Get())[0]).CompareTo(key));
                object operson = entry.SetOffset((long)((object[])en.Get())[1]).Get();
                //Console.WriteLine($"val={tp_rec.Interpret(operson)}");
            }
            sw.Stop();
            Console.WriteLine($"getByKey {ntests} times. duration {sw.ElapsedMilliseconds}");

        }
    }
}
