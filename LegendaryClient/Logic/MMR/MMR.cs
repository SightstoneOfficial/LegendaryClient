using System;
using System.Collections.Generic;
using System.Linq;

namespace LegendaryClient.Logic
{
    public class MMR
    {
        //Basic class for calculating MMR

        public static double GetCalulatedMMR(List<double> ELOVals)
        {
            return (1200 + ELOVals.Sum())/(ELOVals.Count() + 1);
        }

        public static double GetInaccurateMMR(string League, int LeaguePoints)
        {
            if (!League.Contains("CHALLENGER"))
                return Values.MMRValues[League] + (LeaguePoints/2);
            else
                return Values.MMRValues[League] + ((double) 7.07017 * Math.Sqrt(LeaguePoints));
        }
    }
}
