using System;
using Polar.DB;

namespace Task12_Nametable
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start Task12_Nametable");
            PType tp_rec = new PTypeRecord(
                new NamedType("key", new PType(PTypeEnumeration.sstring)),
                new NamedType("value", new PType(PTypeEnumeration.integer)));
            int nelements = 1_000_000;


        }
    }
}
