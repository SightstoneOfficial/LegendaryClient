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
        public String Queue { get; set; }

        [InternalName("name")]
        public String Name { get; set; }

        [InternalName("tier")]
        public String Tier { get; set; }

        [InternalName("requestorsRank")]
        public String RequestorsRank { get; set; }

        [InternalName("entries")]
        public List<LeagueItemDTO> Entries { get; set; }

        [InternalName("requestorsName")]
        public String RequestorsName { get; set; }
    }
}