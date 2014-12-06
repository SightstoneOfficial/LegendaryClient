using System.Collections;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
namespace LegendaryClient.Logic.SQLite
{
    public class runes
    {
        public int id { get; set; }

        public string name { get; set; }

        public string description { get; set; }

        public Dictionary<string, object> stats { get; set; }

        public ArrayList tags { get; set; }

        public BitmapImage icon { get; set; }
    }
}