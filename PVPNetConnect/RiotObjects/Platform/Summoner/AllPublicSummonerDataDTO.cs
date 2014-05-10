using PVPNetConnect.RiotObjects.Platform.Summoner.Spellbook;

namespace PVPNetConnect.RiotObjects.Platform.Summoner
{
    public class AllPublicSummonerDataDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.summoner.AllPublicSummonerDataDTO";

        public AllPublicSummonerDataDTO()
        {
        }

        public AllPublicSummonerDataDTO(Callback callback)
        {
            this.callback = callback;
        }

        public AllPublicSummonerDataDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(AllPublicSummonerDataDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("spellBook")]
        public SpellBookDTO SpellBook { get; set; }

        [InternalName("summonerDefaultSpells")]
        public SummonerDefaultSpells SummonerDefaultSpells { get; set; }

        [InternalName("summonerTalentsAndPoints")]
        public SummonerTalentsAndPoints SummonerTalentsAndPoints { get; set; }

        [InternalName("summoner")]
        public BasePublicSummonerDTO Summoner { get; set; }

        [InternalName("summonerLevelAndPoints")]
        public SummonerLevelAndPoints SummonerLevelAndPoints { get; set; }

        [InternalName("summonerLevel")]
        public SummonerLevel SummonerLevel { get; set; }
    }
}