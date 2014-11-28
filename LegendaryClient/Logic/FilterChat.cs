using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryClient.Logic
{
    public static class FilterChat
    {
        private static List<String> FilteredWords = new List<String>()
        {
            "fuck",
            "shit",
            "bitch",
            ""
        };

        /// <summary>
        /// Used to change swear words into the car(*)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Filter(this string input)
        {
            var filter = input.ToLower();
            foreach(String wordlist in FilteredWords)
            {
                if (filter.Contains(wordlist) && wordlist.Length != 0)
                {
                    filter.Replace(wordlist, String.Concat(Enumerable.Repeat("*", wordlist.Length)));
                }
            }
            return filter;
        }
    }
}
