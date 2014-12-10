#region

using System.Linq;

#endregion

namespace LegendaryClient.Logic.SQLite
{
    public class championSkins
    {
        public int id { get; set; }

        public int isBase { get; set; }

        public int rank { get; set; }

        public int championId { get; set; }

        public string name { get; set; }

        public string displayName { get; set; }

        public string portraitPath { get; set; }

        public string splashPath { get; set; }

        public static championSkins GetSkin(int id)
        {
            return Client.ChampionSkins.FirstOrDefault(c => c.id == id);
        }
    }
}