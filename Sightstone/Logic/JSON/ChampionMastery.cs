using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sightstone.Logic.JSON
{
    class ChampionMastery
    {
        public string shardId { get; set; }
        public int gameId { get; set; }
        public int playerId { get; set; }
        public int championId { get; set; }
        public int championLevel { get; set; }
        public int championPointsBeforeGame { get; set; }
        public int championPointsGained { get; set; }
        public int championPointsGainedIndividualContribution { get; set; }
        public int bonusChampionPointsGained { get; set; }
        public string playerGrade { get; set; }
        public int championPointsSinceLastLevelBeforeGame { get; set; }
        public int championPointsUntilNextLevelBeforeGame { get; set; }
        public int championPointsUntilNextLevelAfterGame { get; set; }
        public bool championLevelUp { get; set; }
        public int score { get; set; }
        public object[] levelUpList { get; set; }
    }
}
