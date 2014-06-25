using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Summoner.Icon
{
    public class SummonerIconInventoryDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.summoner.icon.SummonerIconInventoryDTO";

        public SummonerIconInventoryDTO()
        {
        }

        public SummonerIconInventoryDTO(Callback callback)
        {
            this.callback = callback;
        }

        public SummonerIconInventoryDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(SummonerIconInventoryDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("summonerId")]
        public Double SummonerId { get; set; }

        [InternalName("summonerIcons")]
        public List<Platform.Catalog.Icon.Icon> SummonerIcons { get; set; }
    }
}