#region

using System.Collections;
using System.Windows.Media.Imaging;

#endregion

namespace LegendaryClient.Logic.SQLite
{
    public class Masteries
    {
        public int Id { get; set; }

        public int Prereq { get; set; }

        public int Ranks { get; set; }

        public int SelectedRank { get; set; }

        public string Name { get; set; }

        public string Tree { get; set; }

        public int TreeRow { get; set; }

        public ArrayList Description { get; set; }

        public BitmapImage Icon { get; set; }
    }
}