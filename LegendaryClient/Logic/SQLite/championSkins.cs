#region

using System.Linq;

#endregion

namespace LegendaryClient.Logic.SQLite
{
    public class ChampionSkins
    {
        public int Id { get; set; }

        public int IsBase { get; set; }

        public int Rank { get; set; }

        public int ChampionId { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string PortraitPath { get; set; }

        public string SplashPath { get; set; }

        public static ChampionSkins GetSkin(int id)
        {
            return Client.ChampionSkins.FirstOrDefault(c => c.Id == id);
        }
    }
}