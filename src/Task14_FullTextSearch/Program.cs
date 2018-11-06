using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Polar.DB;
using Polar.Cells;
using Polar.CellIndexes;


namespace Task14_FullTextSearch
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = "../../../../../";
            Console.WriteLine("Start Task14_FullTextSearch");
            var reader = new StreamReader(path + "Resources/zaliznyak_shortform.txt");
            Dictionary<string, string> normal_form = new Dictionary<string, string>();
            string line = null;
            string normal = null;
            int cnt = 0;
            while ((line = reader.ReadLine()) != null)
            {
                string[] prts = line.Split(' ');
                normal = prts[0];
                foreach (string w in prts)
                {
                    normal_form.TryAdd(w, normal);
                }
                cnt++;
            }
            Console.WriteLine($"n={cnt} words={normal_form.Count}"); 

            PType tp_elem = new PTypeRecord(
                new NamedType("nom", new PType(PTypeEnumeration.integer)),
                new NamedType("news", new PType(PTypeEnumeration.sstring)));
            int dnom = 0;
            Func<Stream> getStream = () => File.Open(path + "Databases/data" + (dnom++), FileMode.OpenOrCreate);
            TableSimple table = new TableSimple(tp_elem, new int[] { 0 }, getStream);

            table.Fill(Generate(path + "Resources/academpark-newsflow.txt"));

            // Построение полнотекстового индекса
            PType tp_index = new PTypeRecord(
                new NamedType("nom", new PType(PTypeEnumeration.integer)),
                new NamedType("word", new PType(PTypeEnumeration.sstring)));
            TableSimple ft_index = new TableSimple(tp_index, new int[] { 1 }, getStream);
            char[] delems = new char[] { ' ', ',', '.', '!', '?', '\n', '-', ':' };
            //Func<object[]> generateNomWordFlow = () =>
            var generateNomWordFlow = table.ElementValues()
                .SelectMany(nomtext =>
                {
                    int nom = (int)nomtext[0];
                    string text = (string)nomtext[1];
                    string[] words = text.Split(delems);
                    var setofwords = words.Where(w => !string.IsNullOrEmpty(w) && w.Length > 2 && char.IsLetter(w[0]))
                        .Select(w => { if (normal_form.TryGetValue(w.ToLower(), out string wrd)) return wrd; return null; })
                        .Where(w => !string.IsNullOrEmpty(w))
                        .Distinct();
                    return setofwords.Select(w => new object[] { nom, w });
                }); 
            // Заполнение таблицы
            ft_index.Fill(generateNomWordFlow);

            // Поиск по одному слову
            var qu = ft_index.GetAllByKey(1, "олимпиада");
            foreach (object[] vv in qu)
            {
                int nom = (int)vv[0];
                var qq = table.GetAllByKey(0, nom).FirstOrDefault();
                Console.WriteLine($"{qq[0]} {qq[1]}");
            }

            // Тестирование скорости
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            string aaa = "Завтра в Академпарке назовут новых резидентов бизнес инкубаторов 5 марта состоится торжественная церемония закрытия десятой юбилейной инновационной школы Академпарка Начало подведения итогов";
            string[] parts = aaa.ToLower().Split(' ');
            Console.WriteLine(parts.Count() + " samples");
            int cont = 0;
            sw.Start();
            foreach (string w in parts)
            {
                cont += ft_index.GetAllByKey(1, w).Select((object[] vv) => 
                {
                    int nom = (int)vv[0];
                    return table.GetAllByKey(0, nom).FirstOrDefault();
                }).Count();
            }
            sw.Stop();
            Console.WriteLine($"всего найдено документов: {cont}   за время: {sw.ElapsedMilliseconds}");
        }
        private static IEnumerable<object[]> Generate(string textspath)
        {
            var reader2 = new StreamReader(textspath);
            string line2 = null;
            int nline = 0;
            while ((line2 = reader2.ReadLine()) != null)
            {
                yield return new object[] { nline, line2 };
                nline++;
            }
        }
    }
}
