using PVPNetConnect.RiotObjects.Leagues.Pojo;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Leagues.Client.Dto
{
    public class SummonerLeagueItemsDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.leagues.client.dto.SummonerLeagueItemsDTO";

        public SummonerLeagueItemsDTO()
        {
        }

        public SummonerLeagueItemsDTO(Callback callback)
        {
            this.callback = callback;
        }

        public SummonerLeagueItemsDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(SummonerLeagueItemsDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("summonerLeagues")]
        public List<LeagueItemDTO> SummonerLeagues { get; set; }
    }
}