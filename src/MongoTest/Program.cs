using System;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Core;
using MongoDB.Bson;

namespace MongoTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start MongoTest");
            // коннектимся к серверному приложению через порт
            var client = new MongoClient("mongodb://localhost:27017");
            // объявляем рабочую базу данных
            var database = client.GetDatabase("foo");
            // используем или создаем новую коллекцию персон
            var collection = database.GetCollection<BsonDocument>("persons");
            database.DropCollection("persons");

            int nelements = 1000;
            var documents = Enumerable.Range(0, nelements).Select(i => new BsonDocument()
                {
                    { "id", nelements - i },
                    { "name", "Pupkin " + (nelements - i) },
                    { "age", 33 }
                });
            collection.InsertMany(documents);
            
            collection.Indexes.CreateOne(new BsonDocument("id", 1));

            Console.WriteLine(collection.Count(new BsonDocument()));

            int id = nelements * 2 / 3;
            var doc = collection.Find(Builders<BsonDocument>.Filter.Eq("id", id)).First();
            Console.WriteLine(doc);

        }
    }
}
