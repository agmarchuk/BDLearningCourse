using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Task08_JSON
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            // Serialize object
            Console.WriteLine("Start Task08_JSON");
            Account account = new Account
            {
                Email = "james@example.com",
                Active = true,
                CreatedDate = new DateTime(2013, 1, 20, 0, 0, 0, DateTimeKind.Utc),
                Roles = new List<string>
                {
                    "User",
                    "Admin"
                }
            };
            string json = JsonConvert.SerializeObject(account, Formatting.Indented);
            Console.WriteLine(json);

            var ob = JsonConvert.DeserializeObject<Account>(json);

            Console.WriteLine($"{ob.Email} {ob.Active} {ob.CreatedDate}");
            foreach (var r in ob.Roles) Console.Write($"{r} ");
            Console.WriteLine("\n");

            // byte serialization BSON for database DB

            Database db = new Database
            {
                persons = new List<Person>
                {
                    new Person { id = 997, name="Pupkin_997", age=23 },
                    new Person { id = 998, name="Pupkin_997", age=22 },
                    new Person { id = 999, name="Pupkin_997", age=21 },
                }
            };
            // JSON check
            string jdb = JsonConvert.SerializeObject(db, Formatting.Indented);
            Console.WriteLine(jdb);

            
            int npersons = 1_000_000;

            bool toload = true;
            if (toload)
            {
                sw.Restart();
                db = new Database
                {
                    persons = Enumerable.Range(0, npersons)
                        .Select(i => new Person { id = i, name = "Pupkin_" + i, age = 23 })
                        .ToList()
                };
                jdb = JsonConvert.SerializeObject(db, Formatting.None);
                //Console.WriteLine(jdb);
                File.WriteAllText("db.json", jdb);
                sw.Stop();
                Console.WriteLine($"json load ok. duration={sw.ElapsedMilliseconds}");
            }


            // Обратный процесс: чтение строки из файла и десериализация

            sw.Restart();
            string jdb1 = File.ReadAllText("db.json");
            var db1 = JsonConvert.DeserializeObject<Database>(jdb1);
            sw.Stop();
            Console.WriteLine($"load ok. duration={sw.ElapsedMilliseconds}");

            int nprobes = 100000;
            sw.Restart();
            Random rnd = new Random();
            for (int j = 0; j<nprobes; j++)
            {
                int cod = rnd.Next(npersons);
                var pers = db1.persons[cod];
                //Console.WriteLine($"{cod} => {pers.name}");
            }
            sw.Stop();
            Console.WriteLine($"{nprobes} GetByIndex. duration={sw.ElapsedMilliseconds}");

            
            
            // Поработаем с BSON =========== Это ничего не дает!!!! ============
            // создаем объект
            FileStream fs = File.Open("test1.bson", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            JsonSerializer serializer = new JsonSerializer();
            if (toload)
            {
                db = new Database
                {
                    persons = Enumerable.Range(0, npersons)
                        .Select(i => new Person { id = i, name = "Pupkin_" + i, age = 23 })
                        .ToList()
                };
                // создаем файл, сериализатор, writer, сериализуем в поток байтов 
                sw.Restart();
                BsonDataWriter writer = new BsonDataWriter(fs);
                serializer.Serialize(writer, db);
                sw.Stop();
                Console.WriteLine($"BSON serialization. duration={sw.ElapsedMilliseconds}");
            }

            // BSON deserialization
            sw.Restart();
            fs.Position = 0L;
            BsonDataReader reader = new BsonDataReader(fs);
            Database e = serializer.Deserialize<Database>(reader);
            Console.WriteLine(e.persons.Count());
            sw.Stop();
            Console.WriteLine($"deserialization. duration={sw.ElapsedMilliseconds}");

            fs.Close();
        }
    }

    public class Account
    {
        public string Email { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedDate { get; set; }
        public IList<string> Roles { get; set; }
    }
    public class Database { public IList<Person> persons; }
    public class Person
    {
        public int id { get; set; }
        public string name { get; set; }
        public int age { get; set; }
    }


}
