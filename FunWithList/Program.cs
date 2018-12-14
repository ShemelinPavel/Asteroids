using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunWithList
{
    class Program
    {

        struct MyStru
        {
            public int key;
            public int count;
        }

        static void Main( string[] args )
        {


            List<int> list = new List<int> { 1, 2, 3, 4, 3, 5, 4, 0, 1, 1, 4, 1 };

            #region LINQ
            Console.WriteLine( "LINQ" );
            var selection = from q in list group q by q into c select new { key = c.Key, count = c.Count() };

            foreach (var item in selection)
            {
                Console.WriteLine( $"Значение: <{item.key}> встречается: {item.count} раз" );
            }
            #endregion LINQ

            #region Расширение + анонимные делегаты
            Console.WriteLine( "\nРасширение + анонимные делегаты" );
            Func<int, int> qq = delegate ( int s ) { return s; };
            Func<IGrouping<int, int>, MyStru> cc = delegate ( IGrouping<int, int> s ) { return new MyStru { key = s.Key, count = s.Count() }; };
            var selection2 = list.GroupBy( qq ).Select( cc );
            foreach (var item in selection2)
            {
                Console.WriteLine( $"Значение: <{item.key}> встречается: {item.count} раз" );
            }
            #endregion Расширение + анонимные делегаты


            #region Расширение + Лямбда
            Console.WriteLine( "\nРасширение + Лямбда" );
            var selection3 = list.GroupBy( q => q ).Select( c => new { key = c.Key, count = c.Count() } );

            foreach (var item in selection2)
            {
                Console.WriteLine( $"Значение: <{item.key}> встречается: {item.count} раз" );
            }
            #endregion Расширение + Лямбда

            Console.ReadKey();
        }
    }
}
