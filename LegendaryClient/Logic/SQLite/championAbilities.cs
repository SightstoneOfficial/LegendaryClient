#region

using System.Collections.Generic;
using System.Linq;

#endregion

namespace LegendaryClient.Logic.SQLite
{
    public class ChampionAbilities
    {
        public int Id { get; set; }

        public int Rank { get; set; }

        public int ChampionId { get; set; }

        public string Name { get; set; }

        public string Cost { get; set; }

        public string Cooldown { get; set; }

        public string IconPath { get; set; }

        public string VideoPath { get; set; }

        public double Range { get; set; }

        public string Effect { get; set; }

        public string Description { get; set; }

        public string Hotkey { get; set; }

        public static List<ChampionAbilities> GetAbilities(int champId)
        {
            return Client.ChampionAbilities.Where(c => c.ChampionId == champId).ToList();
        }
    }
}