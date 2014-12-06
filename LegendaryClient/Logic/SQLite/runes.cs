#region

using System.Collections;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

#endregion

namespace LegendaryClient.Logic.SQLite
{
    public class Runes
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public Dictionary<string, object> Stats { get; set; }

        public ArrayList Tags { get; set; }

        public BitmapImage Icon { get; set; }
    }
}