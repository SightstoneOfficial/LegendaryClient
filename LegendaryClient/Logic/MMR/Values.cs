using System.Collections.Generic;

namespace LegendaryClient.Logic
{
    public sealed class Values
    {
        public static Dictionary<string, int> MMRValues = new Dictionary<string, int>
        {
            {"BRONZE V", 900},
            {"BRONZE IV", 950},
            {"BRONZE III", 1000},
            {"BRONZE II", 1050},
            {"BRONZE I", 1100},

            {"SILVER V", 1250},
            {"SILVER IV", 1300},
            {"SILVER III", 1350},
            {"SILVER II", 1400},
            {"SILVER I", 1450},

            {"GOLD V", 1600},
            {"GOLD IV", 1650},
            {"GOLD III", 1700},
            {"GOLD II", 1750},
            {"GOLD I", 1800},

            {"PLATINUM V", 1950},
            {"PLATINUM IV", 2000},
            {"PLATINUM III", 2050},
            {"PLATINUM II", 2100},
            {"PLATINUM I", 2150},

            {"DIAMOND V", 2300},
            {"DIAMOND IV", 2350},
            {"DIAMOND III", 2400},
            {"DIAMOND II", 2450},
            {"DIAMOND I", 2500},

            {"CHALLENGER I", 2650} //Challenger = Base + (sqrt(50) * sqrt(LP))
        }; 
    }
}
