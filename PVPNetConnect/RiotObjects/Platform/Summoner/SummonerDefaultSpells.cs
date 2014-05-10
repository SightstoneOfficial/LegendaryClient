using System;

namespace PVPNetConnect.RiotObjects.Platform.Summoner
{
    public class SummonerDefaultSpells : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.summoner.SummonerDefaultSpells";

        public SummonerDefaultSpells()
        {
        }

        public SummonerDefaultSpells(Callback callback)
        {
            this.callback = callback;
        }

        public SummonerDefaultSpells(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(SummonerDefaultSpells result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("summonerDefaultSpellsJson")]
        public object SummonerDefaultSpellsJson { get; set; }

        [InternalName("summonerDefaultSpellMap")]
        public TypedObject SummonerDefaultSpellMap { get; set; }

        [InternalName("summonerId")]
        public Double SummonerId { get; set; }
    }
}