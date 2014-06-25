using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Summoner.Runes
{
    public class SummonerRuneInventory : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.summoner.runes.SummonerRuneInventory";

        public SummonerRuneInventory()
        {
        }

        public SummonerRuneInventory(Callback callback)
        {
            this.callback = callback;
        }

        public SummonerRuneInventory(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(SummonerRuneInventory result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("summonerRunesJson")]
        public object SummonerRunesJson { get; set; }

        [InternalName("dateString")]
        public String DateString { get; set; }

        [InternalName("summonerRunes")]
        public List<SummonerRune> SummonerRunes { get; set; }

        [InternalName("summonerId")]
        public Double SummonerId { get; set; }
    }
}