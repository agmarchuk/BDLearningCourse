using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Polar.DB;

namespace Task12_Nametable
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start Task12_Nametable");
            int nelements = 1_000_000;

            // Генератор тестовых данных
            Func<int, IEnumerable<object[]>> Generate = n => Enumerable.Range(0, n).Select(i => new object[] { i.ToString(), i });

            // Проверим его работу
            foreach (object[] pair in Generate(10))
            {
                Console.WriteLine($"{pair[0].GetType().Name} {pair[0]} {pair[1].GetType().Name} {pair[1]} ");
            }

            // Файл и его заполнение
            FileStream fs = File.Open("data.bin", FileMode.OpenOrCreate);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write((long)nelements);
            Generate(nelements).Select(pair => { bw.Write((string)pair[0]); bw.Write((int)pair[1]); return true; }).Count();
            bw.Flush(); fs.Flush();

            // Сканирование файла с выдачей последних элементов
            BinaryReader br = new BinaryReader(fs);
            fs.Position = 0L;
            int ne = (int)br.ReadInt64();
            Console.WriteLine(ne);
            var query = Enumerable.Repeat(0, ne).Select(i =>
            {
                object[] pa = new object[2];
                pa[0] = br.ReadString();
                pa[1] = br.ReadInt32();
                return pa;
            });
            foreach (object[] pa in query.TakeLast(10))
            {
                Console.WriteLine($"element: {pa[0]} {pa[1]}");
            }

            fs.Position = 0L;

            Console.WriteLine("==== use Nametable class ====");
            Nametable nt = new Nametable("nametable.bin");
            foreach (object[] pa in Generate(nelements))
            {
                //Console.WriteLine($"element: {pa[0]} {pa[1]}");
                nt.Add((string)pa[0], (int)pa[1]);
            }
            nt.Flush();

            string k = (nelements * 2 / 3).ToString();
            Console.WriteLine($" key={k} value={nt.GetByKey(k)}");

            // значение состоит из имени и списка ключей друзей
            PType tp_rec = new PTypeRecord(
                new NamedType("key", new PType(PTypeEnumeration.sstring)),
                new NamedType("value",
                    new PTypeRecord(
                        new NamedType("name", new PType(PTypeEnumeration.sstring)),
                        new NamedType("friends", new PTypeSequence(new PType(PTypeEnumeration.sstring))))));

            // Сериализация и десериализация
            //ByteFlow.Serialize(bw, v, tp_rec);
            //ByteFlow.Deserialize(br, tp_rec);

        }
    }
}
