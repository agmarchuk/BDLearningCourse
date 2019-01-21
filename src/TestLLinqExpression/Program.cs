using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TestLinqExpression
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start TestLinqExpression");
            Func<int> f1 = () => 2 + 2;
            int res = f1();
            Console.WriteLine(res);
            
            // Теперь повторим это через Expression (заменим слагаемые)
            Expression exp = BinaryExpression.Add(ConstantExpression.Constant(4), ConstantExpression.Constant(4));
            var f2 = Expression.Lambda<Func<int>>(exp).Compile();
            res = f2();
            Console.WriteLine(res);

            // Теперь придумываем "язык" для формул exp. Язык будет объектным. Константа c целого типа будет 
            // соответствовать выражению ConstantExpression.Constant(c), массив object[] {"Add", object1, object2}
            // будет BinaryExpression.Add(object1,  object2)
            // Делаем транслятор object -> Expression

            Func<object, Expression> objectTranslator = null;
            objectTranslator = (object ob) =>
            {
                if (ob is int) return ConstantExpression.Constant(ob);
                return null;
            };

            // испытываем
            Expression expr2 = objectTranslator(444);
            Console.WriteLine(Expression.Lambda<Func<int>>(expr2).Compile()());

            // Делаем другую ветвь в трансляторе
            objectTranslator = (object ob) =>
            {
                if (ob is int) return ConstantExpression.Constant(ob);
                if (ob is object[])
                {
                    object[] arr = (object[])ob;
                    if ((string)arr[0] == "Add")
                        return BinaryExpression.Add(objectTranslator(arr[1]), objectTranslator(arr[1]));
                }
                return null;
            };

            // испытываем
            object[] myexpression = new object[] { "Add", 2222, 4444 }; // Вот он язык описания вычислений!!!

            Expression expr3 = objectTranslator(myexpression);
            Console.WriteLine(Expression.Lambda<Func<int>>(expr3).Compile()());

            myexpression = new object[] { "Add", new object[] {"Add", 2, 2 }, new object[] { "Add", 2, 2 } };
            Console.WriteLine(Expression.Lambda<Func<int>>(objectTranslator(myexpression)).Compile()());
        }
    }
}
