using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryClient.Logic.Player
{
    public class KDA
    {
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
        public int Games { get; set; }

        public string KDAToString()
        {
            return (Kills/Games) + "/" + (Deaths/Games) + "/" + (Assists/Games);
        }
    }
}
