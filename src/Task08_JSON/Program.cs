using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Task08_JSON
{
    class Program
    {
        static void Main(string[] args)
        { 
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

            // serialize JSON to a string and then write string to a file
            File.WriteAllText("test_account.json", JsonConvert.SerializeObject(account));

            // byte serialization BSON
            FileStream fs = File.Open("test1.bson", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            JsonSerializer serializer = new JsonSerializer();

            BsonDataWriter writer = new BsonDataWriter(fs);
            serializer.Serialize(writer, account);

            // BSON deserialization
            fs.Position = 0L;
            BsonDataReader reader = new BsonDataReader(fs);
            Account e = serializer.Deserialize<Account>(reader);
            Console.WriteLine(e.CreatedDate);

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
    public class Person
    {
        public int id { get; set; }
        public string name { get; set; }
        public int age { get; set; }
    }


}
