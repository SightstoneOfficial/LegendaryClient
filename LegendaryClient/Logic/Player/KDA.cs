using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryClient.Logic.Player
{
    public class KDA
    {
        public int Kills;
        public int Deaths;
        public int Assists;
        public int Games;

        public string KDAToString(KDA kda)
        {
            return (kda.Kills / kda.Games) + "/" + (kda.Deaths / kda.Games) + "/" + (kda.Assists / kda.Games);
        }
    }
}
