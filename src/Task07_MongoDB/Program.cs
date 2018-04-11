using System;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Core;
using MongoDB.Bson;

namespace Task07_MongoDB
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            Console.WriteLine("Hello World!");
            // коннектимся к серверному приложению через порт
            var client = new MongoClient("mongodb://localhost:27017");
            // объявляем рабочую базу данных
            var database = client.GetDatabase("foo");

            bool toload = true;

            // чистим предыдущее состояние коллекции персон
            if (toload) database.DropCollection("persons");
            
            // используем или создаем новую коллекцию персон
            var collection = database.GetCollection<BsonDocument>("persons");

            // generate nelements documents
            int nelements = 1_000_000;

            if (toload)
            {
                sw.Restart();
                var documents = Enumerable.Range(0, nelements).Select(i => new BsonDocument()
                    {
                        { "id", nelements - i },
                        { "name", "Pupkin " + (nelements - i) },
                        { "age", 33 }
                    });
                collection.InsertMany(documents);
                sw.Stop();
                Console.WriteLine($"load ok. duration={sw.ElapsedMilliseconds}");

                collection.Indexes.CreateOne(new BsonDocument("id", 1));
                collection.Indexes.CreateOne(new BsonDocument("name", 1));
            }

            Console.WriteLine(collection.Count(new BsonDocument()));

            var doc1 = collection.Find(new BsonDocument()).Skip(nelements * 2 / 3).FirstOrDefault();
            Console.WriteLine(doc1);

            var oi = (ObjectId)collection.Find(new BsonDocument()).Skip(666).FirstOrDefault().Values.First();
            Console.WriteLine(oi);
            var d1 = collection.Find(Builders<BsonDocument>.Filter.Eq("_id", oi)).FirstOrDefault();
            Console.WriteLine(d1);


            int ntests = 1000;
            Random rnd = new Random();

            sw.Restart();
            for (int i=0; i<ntests; i++)
            {
                int id = rnd.Next(nelements);
                var doc = collection.Find(Builders<BsonDocument>.Filter.Eq("id", id)).First();
                //Console.WriteLine(doc);
            }
            sw.Stop();
            Console.WriteLine($"{ntests} finds ok. duration={sw.ElapsedMilliseconds}");

            bool by_id = false;
            if (by_id)
            {
                var _id_arr = collection.Find(new BsonDocument()).ToCursor().ToList()
                    .Select(d => d.Values.First())
                    //.Cast<ObjectId>()
                    .ToArray();
                sw.Restart();
                for (int i = 0; i < ntests; i++)
                {
                    int nom = rnd.Next(nelements);
                    var doc = collection.FindSync(Builders<BsonDocument>.Filter.Eq("_id", _id_arr[nom])).First();
                    //if (i<10) Console.WriteLine(doc);
                }
                sw.Stop();
                Console.WriteLine($"By _id: {ntests} finds ok. duration={sw.ElapsedMilliseconds}");
            }

            //ntests = 10;
            sw.Restart();
            for (int i = 0; i < ntests; i++)
            {
                int id = rnd.Next(nelements);
                var doc = collection.Find(Builders<BsonDocument>.Filter.Eq("name", "Pupkin " + id)).First();
                //Console.WriteLine(doc);
            }
            sw.Stop();
            Console.WriteLine($"By name: {ntests} finds ok. duration={sw.ElapsedMilliseconds}");

        }
    }
}
