using System;
using System.Linq;
using System.Xml.Linq;

namespace Task05_XML
{
    class Program
    {
        static string dbpath = "../../Databases/";
        static void Main(string[] args)
        {
            Console.WriteLine("Start XML tests");
            XElement db = XElement.Load("XMLdata.xml");
            Console.WriteLine(db.ToString());

            string id = "p002";
            var q = db.Elements()
                .Where(el => el.Attribute("id").Value == id)
                .First()
                .Elements()
                .Select(e => e.Value)
                .Aggregate((s, t) => s + " " + t);
            Console.WriteLine(q);

            var db2 = new XElement("db",
                new XElement("person",
                    new XAttribute("id", "p001"),
                    new XElement("name", "Иванов"),
                    new XElement("age", "22")),
                new XElement("person",
                    new XAttribute("id", "p002"),
                    new XElement("name", "Петров"),
                    new XElement("age", "23"))
                // ...
                );

            int npersons = 1_000_000;
            Random rnd = new Random();
            XElement db3 = new XElement("db",
                Enumerable.Range(1, npersons)
                .Select(i => new XElement("person",
                    new XAttribute("id", "p" + i),
                    new XElement("name", "=p" + i + "="),
                    new XElement("age", "" + rnd.NextDouble() * 150))));
            foreach (var el in db3.Elements().Skip(1000).Take(5))
            {
                Console.WriteLine(el.ToString());
            }

            db3.Save(dbpath + "db3.xml");
        }
    }
}
