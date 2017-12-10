using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Data.Sql;
using System.Data;
using System.Data.SQLite;
using System.Data.Common;

namespace Task02_SQLite
{
    partial class Program
    {
        static void Main_2(string[] args)
        {
            Console.WriteLine("Start SQLite 2");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            Random rnd = new Random();

            string number = args.Length == 0? null : args.FirstOrDefault(s => char.IsDigit(s[0]));
            int npersons = number==null? 40000: Int32.Parse(number);

            string path = "../../../";

            string filename = path + "databases/test.db3";

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
                comm.CommandText = @"DROP TABLE persons;
DROP TABLE photos;DROP TABLE reflections;";
                try { comm.ExecuteNonQuery(); }
                catch (Exception ex) { Console.WriteLine($"Warning in DROP section {ex.Message}"); }
                connection.Close();

                connection.Open();
                comm.CommandText =
                @"CREATE TABLE persons (id INTEGER PRIMARY KEY ASC, name TEXT, age INTEGER);
CREATE TABLE photos (id INTEGER PRIMARY KEY, name TEXT);
CREATE TABLE reflections (id INTEGER PRIMARY KEY, reflected INTEGER, indoc INTEGER);";
                try { comm.ExecuteNonQuery(); }
                catch (Exception ex) { Console.WriteLine($"Warning in CREATE TABLE section {ex.Message}"); }
                connection.Close();

                // Загрузка таблиц
                sw.Restart();
                LoadRecord(connection, npersons, "persons", i => (npersons - i) + ", \"" + (npersons - i) + "\", " + rnd.Next(150));
                int nphotos = npersons * 2;
                LoadRecord(connection, nphotos, "photos", i => (nphotos - i) + ", \"DSP" + (nphotos - i) + "\"");
                int nreflections = npersons * 6;
                LoadRecord(connection, nreflections, "reflections", i => (nreflections - i) + ", " + rnd.Next(npersons) + ", " + rnd.Next(nphotos));
                sw.Stop();
                Console.WriteLine($"load ok. duration {sw.ElapsedMilliseconds}");

                sw.Restart();
                connection.Open();
                comm.CommandText = @"CREATE INDEX reflection_reflected ON reflections(reflected);
CREATE INDEX reflection_indoc ON reflections(indoc); 
CREATE INDEX pname ON persons(name);
";
                try { comm.ExecuteNonQuery(); }
                catch (Exception ex) { Console.WriteLine($"Error in making index {ex.Message}"); }
                connection.Close();
                sw.Stop();
                Console.WriteLine($"create index ok. duration {sw.ElapsedMilliseconds}");

            }

            DbCommand runcomm = connection.CreateCommand();
            runcomm.CommandType = CommandType.Text;

            // Получение записи по ключу
            sw.Restart();
            connection.Open();
            DbTransaction transaction = connection.BeginTransaction();
            runcomm.Transaction = transaction;
            int ntests = 10000;
            for (long i = 0; i < ntests; i += 1)
            {
                //int key = (int)(npersons * 2 / 3);
                int key = rnd.Next((int)npersons);
                runcomm.CommandText = "SELECT * FROM persons WHERE id=" + key + ";";
                object[] res = null;
                var reade = runcomm.ExecuteReader();
                int cnt = 0;
                while (reade.Read())
                {
                    int ncols = reade.FieldCount;
                    res = new object[ncols];
                    for (int j = 0; j < ncols; j++) res[j] = reade.GetValue(j);
                    cnt += 1;
                }
                if (cnt == 0) { Console.WriteLine($"no solutions. key = {key}"); }
                else if (cnt > 1) { Console.WriteLine("multiple solutions. key = {key} cnt = {cnt}"); }
                //Console.WriteLine($"{key} => {res[0]} {res[1]} {res[2]}");

                reade.Close();
            }
            transaction.Commit();
            connection.Close();
            sw.Stop();
            Console.WriteLine($"bykey duration {sw.ElapsedMilliseconds} for {ntests}");


            // Получение записи по имени
            sw.Restart();
            connection.Open();
            transaction = connection.BeginTransaction();
            runcomm.Transaction = transaction;
            ntests = 1000;
            for (long i = 0; i < ntests; i += 1)
            {
                int key = rnd.Next((int)npersons);
                string skey = "" + key;
                runcomm.CommandText = "SELECT * FROM persons WHERE name = '" + skey + "';";
                object[] res = null;
                var reade = runcomm.ExecuteReader();
                int cnt = 0;
                while (reade.Read())
                {
                    int ncols = reade.FieldCount;
                    res = new object[ncols];
                    for (int j = 0; j < ncols; j++) res[j] = reade.GetValue(j);
                    cnt += 1;
                }
                if (cnt == 0) { Console.WriteLine($"no solutions. key = {key}"); }
                else if (cnt > 1) { Console.WriteLine("multiple solutions. key = {key} cnt = {cnt}"); }

                reade.Close();
            }
            transaction.Commit();
            connection.Close();
            sw.Stop();
            Console.WriteLine($"byname duration {sw.ElapsedMilliseconds} for {ntests}");

            // Построение портрета по ключу: берется случайный ключ персоны, по таблице reflections определяется множество записей, в которых
            // этот ключ на второй позиции, для каждой записи берется значение ключа фотки, по ключу фотки выдается запись фотографии
            sw.Restart();
            int coun = 0;
            ntests = 1000;
            connection.Open();
            transaction = connection.BeginTransaction();
            runcomm.Transaction = transaction;
            for (int i = 0; i < ntests; i++)
            {
                int ke = rnd.Next(npersons); //npersons * 2 / 3;
                runcomm.CommandText = "SELECT photos.id,photos.name FROM reflections INNER JOIN photos ON reflections.indoc=photos.id WHERE reflections.reflected=" + ke + ";";
                var reader = runcomm.ExecuteReader();
                object[] row = new object[reader.FieldCount];
                while (reader.Read())
                {
                    var c = reader.GetValues(row);
                    coun++;
                }
                reader.Close();
            }
            transaction.Commit();
            connection.Close();
            Console.WriteLine($"counter={coun}");
            sw.Stop();
            Console.WriteLine($"portrait duration {sw.ElapsedMilliseconds} for {ntests}");
        }

        private static void LoadRecord(DbConnection connection, int nelements, string table, Func<int, string> record)
        {
            connection.Open();
            DbCommand runcommand = connection.CreateCommand();
            runcommand.CommandType = CommandType.Text;
            DbTransaction loadtransaction = connection.BeginTransaction();
            runcommand.Transaction = loadtransaction;
            for (int i = 0; i < nelements; i += 1)
            {
                runcommand.CommandText = "INSERT INTO " + table + " VALUES (" + record(i) + ");";
                runcommand.ExecuteNonQuery();

                if (i % 10000 == 0) Console.Write($"{i / 10000} ");
            }
            Console.WriteLine();
            loadtransaction.Commit();
            connection.Close();
        }
    }
}
