#region

using System.Collections;
using System.Windows.Media.Imaging;

#endregion

namespace LegendaryClient.Logic.SQLite
{
    public class masteries
    {
        public int id { get; set; }

        public int prereq { get; set; }

        public int ranks { get; set; }

        public int selectedRank { get; set; }

        public string name { get; set; }

        public string tree { get; set; }

        public int treeRow { get; set; }

        public ArrayList description { get; set; }

        public BitmapImage icon { get; set; }
    }
}