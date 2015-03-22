using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.game.PlayerChampionSelectionDTO")]
    public class PlayerChampionSelectionDTO
    {
        [SerializedName("summonerInternalName")]
        public String SummonerInternalName { get; set; }

        [SerializedName("spell2Id")]
        public Double Spell2Id { get; set; }

        [SerializedName("selectedSkinIndex")]
        public Int32 SelectedSkinIndex { get; set; }

        [SerializedName("championId")]
        public Int32 ChampionId { get; set; }

        [SerializedName("spell1Id")]
        public Double Spell1Id { get; set; }
    }
}
