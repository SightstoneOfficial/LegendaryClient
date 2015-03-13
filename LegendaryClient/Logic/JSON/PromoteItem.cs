using PVPNetConnect.RiotObjects.Leagues.Pojo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegendaryClient.Logic.JSON
{
    class PromoteItem
    {
        public LeagueItemDTO leagueItem { get; set; }
        public string notifyReason { get; set; }
        public int? LeaguePointsDelta { get; set; }
        public int? GameId { get; set; }
    }
}
