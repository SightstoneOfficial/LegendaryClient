using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Summoner
{
    public class SummonerCatalog : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.summoner.SummonerCatalog";

        public SummonerCatalog()
        {
        }

        public SummonerCatalog(Callback callback)
        {
            this.callback = callback;
        }

        public SummonerCatalog(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(SummonerCatalog result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("items")]
        public object Items { get; set; }

        [InternalName("talentTree")]
        public List<TalentGroup> TalentTree { get; set; }

        [InternalName("spellBookConfig")]
        public List<RuneSlot> SpellBookConfig { get; set; }
    }
}