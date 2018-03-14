using System;
using System.Collections.Generic;
using System.Text;
using Polar.DB;
using Polar.Cells;

namespace Task04_Sequenses
{
    partial class Program
    {
        public static void Main2(string[] args)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            PType tp_rec = new PTypeRecord(
                new NamedType("id", new PType(PTypeEnumeration.integer)),
                new NamedType("name", new PType(PTypeEnumeration.sstring)),
                new NamedType("age", new PType(PTypeEnumeration.integer)));
            PaCell cell = new PaCell(new PTypeSequence(tp_rec), path + "people.pac", false);

            int npersons = 1_000_000;
            Random rnd = new Random();
            Console.WriteLine("Start Task04: Main2");

            // Загрузка данных
            bool toload = true;
            if (toload)
            {
                sw.Restart();
                cell.Clear();
                cell.Fill(new object[0]);
                for (int i = 0; i < npersons; i++)
                {
                    int code = npersons - i;
                    long offset = cell.Root.AppendElement(new object[] { code, "=" + code + "=", rnd.Next(120) });
                }
                cell.Flush();
                sw.Stop();
                Console.WriteLine($"load {npersons} records ok. duration={sw.ElapsedMilliseconds}");
            }

            int cod = npersons * 2 / 3;
            Func<int, long> OffsetByKey = key => cell.Root.Element(cod).offset;

            Check(tp_rec, cell, OffsetByKey, 111111);

            int[] keys = new int[npersons];
            long[] offsets = new long[npersons];
            int j = 0;
            cell.Root.Scan((o, v) =>
            {
                object[] re = (object[])v;
                keys[j] = (int)re[0];
                offsets[j] = o;
                j++;
                return true;
            });
            OffsetByKey = key =>
            {
                int ind = Array.IndexOf(keys, key);
                return offsets[ind];
            };

            Check(tp_rec, cell, OffsetByKey, cod);

            Get1000(sw, cell, OffsetByKey);

            Array.Sort(keys, offsets);
            OffsetByKey = key =>
            {
                int ind = Array.BinarySearch(keys, key);
                return offsets[ind];
            };
            Get1000(sw, cell, OffsetByKey);

            Dictionary<int, long> keyoffdic = new Dictionary<int, long>();
            cell.Root.Scan((o, v) =>
            {
                object[] re = (object[])v;
                keyoffdic.Add((int)re[0], o);
                return true;
            });
            OffsetByKey = key =>
            {
                return keyoffdic[key];
            };
            Get1000(sw, cell, OffsetByKey);


        }

        private static void Get1000(System.Diagnostics.Stopwatch sw, PaCell cell, Func<int, long> OffsetByKey)
        {
            Random rnd = new Random();
            sw.Restart();
            int nprobe = 1000;
            var entry = cell.Root.Element(0);
            int n = (int)cell.Root.Count();
            for (int l = 0; l < nprobe; l++)
            {
                int cod = rnd.Next(n); 
                entry.SetOffset(OffsetByKey(cod));
                var rec = entry.Get();
            }
            sw.Stop();
            Console.WriteLine($"get {nprobe} records by id ok. duration={sw.ElapsedMilliseconds}");
        }

        private static void Check(PType tp_rec, PaCell cell, Func<int, long> OffsetByKey, int cod)
        {
            var entry = cell.Root.Element(0);
            entry.SetOffset(OffsetByKey(cod));
            var rec = entry.Get();
            Console.WriteLine(tp_rec.Interpret(rec));
        }
    }
}
