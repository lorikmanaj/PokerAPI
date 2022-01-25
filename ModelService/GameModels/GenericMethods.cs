using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerLogic.Models
{
    public static class GenericMethods
    {
        private static Random rng = new Random();
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

    }



    public static class StringExtensions
    {
        public static bool EqualsIgnoreCase(this string str1, string str2)
        {
            return String.Equals(str1, str2, StringComparison.InvariantCultureIgnoreCase);
        }
    }

}
