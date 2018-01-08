using System;
using System.IO;
using Polar.DB;
using Polar.Cells;


namespace Task03_PolarDB
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start Task03_PolarDB");
            string path = "../../Databases/";
            PType tp = new PType(PTypeEnumeration.sstring);
            PaCell cell = new PaCell(tp, path + "test.pac", false);

            cell.Clear();
            cell.Fill("Привет из ячейки базы данных!");
            Console.WriteLine("Содержимое ячейки: {0}", cell.Root.Get());

            PType tp_rec = new PTypeRecord(
                new NamedType("имя", new PType(PTypeEnumeration.sstring)),
                new NamedType("возраст", new PType(PTypeEnumeration.integer)),
                new NamedType("мужчина", new PType(PTypeEnumeration.boolean)));
            object rec_value = new object[] { "Пупкин", 22, true };
            PaCell cell_rec = new PaCell(tp_rec, path + "test_rec.pac", false);
            cell_rec.Clear();
            cell_rec.Fill(rec_value);
            object from_rec = cell_rec.Root.Get();
            Console.WriteLine(tp_rec.Interpret(from_rec));

            PType tp_seq = new PTypeSequence(tp_rec);
            object seq_value = new object[]
            {
                new object[] { "Иванов", 24, true },
                new object[] { "Петрова", 18, false },
                new object[] { "Пупкин", 22, true }
            };
            PaCell cell_seq = new PaCell(tp_seq, path + "test_seq.pac", false);
            cell_seq.Clear();
            cell_seq.Fill(seq_value);
            object from_seq = cell_seq.Root.Get();
            Console.WriteLine(tp_seq.Interpret(from_seq));

            cell_seq.Root.AppendElement(new object[] { "Сидоров", 23, true });
            Console.WriteLine(tp_seq.Interpret(cell_seq.Root.Get()));

            long v0 = cell_seq.Root.Count();
            var v1 = cell_seq.Root.Element(2).Field(0).Get();
            var v2 = cell_seq.Root.Element(3).Field(1).Get();
            Console.WriteLine($"{v0} {v1} {v2}");

            cell_seq.Root.Element(1).Field(1).Set(19);
            cell_seq.Root.Element(1).Field(2).Set(true);
            Console.WriteLine(tp_seq.Interpret(cell_seq.Root.Get()));



        }
    }
}
