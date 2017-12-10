using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using System.Data.Common;

namespace Task02_SQLite
{
    partial class Program
    {
        public static void Main_1(string[] args)
        {
            Console.WriteLine("Start SQLite 1");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            Random rnd = new Random();

            long npersons = 40000;

            string path = "../../../../";

            string filename = path + "Databases/test.db3";

            if (!System.IO.File.Exists(filename))
            {
                SQLiteConnection.CreateFile(filename);
            }

            DbProviderFactory factory = new SQLiteFactory();
            DbConnection connection = factory.CreateConnection();
            connection.ConnectionString = "Data Source=" + filename;

            bool toload = args.Any(s => s == "noload") ? false : true;
            if (toload)
            {
                // Создание таблиц
                connection.Open();
                DbCommand comm = connection.CreateCommand();
                comm.CommandText = @"DROP TABLE persons;";
                try { comm.ExecuteNonQuery(); }
                catch (Exception ex) { Console.WriteLine($"Warning in DROP section {ex.Message}"); }
                connection.Close();

                connection.Open();
                comm.CommandText =
                @"CREATE TABLE persons (id INTEGER PRIMARY KEY ASC, name TEXT, age INTEGER);";
                try { comm.ExecuteNonQuery(); }
                catch (Exception ex) { Console.WriteLine($"Warning in CREATE TABLE section {ex.Message}"); }
                connection.Close();

                // Загрузка таблиц
                sw.Restart();
                int portion = 1000;
                for (long i = 0; i < npersons; i += portion)
                {
                    connection.Open();
                    comm = connection.CreateCommand();
                    StringBuilder sb = new StringBuilder("(" + i + ",\"" + i + "\"," + rnd.Next(150) + ")");
                    for (int j = 1; j < portion; j++) sb.Append(",(" + (i+j) + ",\"" + (i+j) + "\"," + rnd.Next(150) + ")");
                    comm.CommandText = "INSERT INTO persons VALUES " + sb.ToString() + ";";
                    comm.ExecuteNonQuery();
                    connection.Close();

                    if (i % 10 == 0) Console.Write($"{i} ");
                }
                sw.Stop();
                Console.WriteLine($"\nduration {sw.ElapsedMilliseconds}");


            }

            // Получение записи по ключу
            sw.Restart();
            for (long i = 0; i < 1000; i += 1)
            {
                connection.Open();
                var com = connection.CreateCommand();
                //int key = (int)(npersons * 2 / 3);
                int key = rnd.Next((int)npersons);
                com.CommandText = "SELECT * FROM persons WHERE id=" + key + ";";
                object[] res = null;
                var reader = com.ExecuteReader();
                int cnt = 0;
                while (reader.Read())
                {
                    int ncols = reader.FieldCount;
                    res = new object[ncols];
                    for (int j = 0; j < ncols; j++) res[j] = reader.GetValue(j);
                    cnt += 1;
                }
                if (cnt == 0) { Console.WriteLine("no solutions. key = {key}"); }
                else if (cnt > 1) { Console.WriteLine("multiple solutions. key = {key} cnt = {cnt}"); }
                //Console.WriteLine($"{key} => {res[0]} {res[1]} {res[2]}");

                reader.Close();
                connection.Close();
            }
            sw.Stop();
            Console.WriteLine($"duration {sw.ElapsedMilliseconds}");

        }
    }
}
