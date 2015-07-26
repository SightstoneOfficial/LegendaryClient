using Sightstone.Logic.Riot.Leagues;

namespace Sightstone.Logic.JSON
{
    class PromoteItem
    {
        public LeagueItemDTO leagueItem { get; set; }
        public string notifyReason { get; set; }
        public int? LeaguePointsDelta { get; set; }
        public int? GameId { get; set; }
    }
}
