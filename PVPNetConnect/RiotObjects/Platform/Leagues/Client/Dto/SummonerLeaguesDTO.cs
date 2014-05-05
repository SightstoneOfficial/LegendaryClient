using PVPNetConnect.RiotObjects.Leagues.Pojo;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Leagues.Client.Dto
{
    public class SummonerLeaguesDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.leagues.client.dto.SummonerLeaguesDTO";

        public SummonerLeaguesDTO()
        {
        }

        public SummonerLeaguesDTO(Callback callback)
        {
            this.callback = callback;
        }

        public SummonerLeaguesDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(SummonerLeaguesDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("summonerLeagues")]
        public List<LeagueListDTO> SummonerLeagues { get; set; }
    }
}