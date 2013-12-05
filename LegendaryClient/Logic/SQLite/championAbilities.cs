using System.Collections.Generic;

namespace LegendaryClient.Logic.SQLite
{
    public class championAbilities
    {
        public int id { get; set; }

        public int rank { get; set; }

        public int championId { get; set; }

        public string name { get; set; }

        public string cost { get; set; }

        public string cooldown { get; set; }

        public string iconPath { get; set; }

        public string videoPath { get; set; }

        public double range { get; set; }

        public string effect { get; set; }

        public string description { get; set; }

        public string hotkey { get; set; }

        public static List<championAbilities> GetAbilities(int champId)
        {
            List<championAbilities> abilitylist = new List<championAbilities>();
            foreach (championAbilities c in Client.ChampionAbilities)
            {
                if (c.championId == champId)
                    abilitylist.Add(c);
            }
            return abilitylist;
        }
    }
}