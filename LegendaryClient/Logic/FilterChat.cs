#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace LegendaryClient.Logic
{
    public static class FilterChat
    {
        private static readonly List<String> FilteredWords = new List<String>
        {
            "fuck",
            "shit",
            "bitch",
            ""
        };

        /// <summary>
        ///     Used to change swear words into the car(*)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Filter(this string input)
        {
            string filter = input.ToLower();
            foreach (
                string wordlist in FilteredWords.Where(wordlist => filter.Contains(wordlist) && wordlist.Length != 0))
            {
                return filter.Replace(wordlist, String.Concat(Enumerable.Repeat("*", wordlist.Length)));
            }
            return filter;
        }
    }
}