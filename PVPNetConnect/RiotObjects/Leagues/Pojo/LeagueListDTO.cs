using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Leagues.Pojo
{
    public class LeagueListDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.leagues.pojo.LeagueListDTO";

        public LeagueListDTO()
        {
        }

        public LeagueListDTO(Callback callback)
        {
            this.callback = callback;
        }

        public LeagueListDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(LeagueListDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("queue")]
        public string Queue { get; set; }

        [InternalName("name")]
        public string Name { get; set; }

        [InternalName("tier")]
        public string Tier { get; set; }

        [InternalName("requestorsRank")]
        public string RequestorsRank { get; set; }

        [InternalName("entries")]
        public List<LeagueItemDTO> Entries { get; set; }

        [InternalName("requestorsName")]
        public string RequestorsName { get; set; }
    }
}