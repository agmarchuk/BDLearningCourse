using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Task12_Nametable
{
    public class Nametable
    {
        private FileStream fs;
        private BinaryWriter bw;
        private BinaryReader br;
        private int nelements;
        private long freespace;
        private Dictionary<string, int> dic = new Dictionary<string, int>();
        public Nametable(string file)
        {
            fs = File.Open(file, FileMode.OpenOrCreate);
            bw = new BinaryWriter(fs);
            br = new BinaryReader(fs);
            // файл может быть пустой или уже заполненный
            if (fs.Length == 0L)
            {
                nelements = 0;
                freespace = 8L;
                bw.Write(0L);
            }
            else
            {
                fs.Position = 0L;
                nelements = (int)br.ReadInt64();
                for (int i=0; i<nelements; i++)
                {
                    string key = br.ReadString();
                    int value = br.ReadInt32();
                    if (!dic.ContainsKey(key))
                    {
                        dic.Add(key, value);
                    }
                }
                freespace = fs.Position;
            }
            Flush();
        }
        public void Add(string key, int value)
        {
            if (!dic.ContainsKey(key))
            {
                dic.Add(key, value);
                fs.Position = freespace;
                bw.Write(key);
                bw.Write(value);
                freespace = fs.Position;
                nelements += 1;
            }
        }
        public void Flush()
        {
            fs.Position = 0L;
            bw.Write((long)nelements);
            bw.Flush(); fs.Flush();
        }
        public int GetByKey(string key)
        {
            if (dic.TryGetValue(key, out int value)) return value;
            return Int32.MinValue;
        }
    }
}
