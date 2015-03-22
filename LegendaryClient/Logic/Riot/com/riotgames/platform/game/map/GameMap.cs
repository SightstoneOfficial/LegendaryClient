using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.game.map.GameMap")]
    public class GameMap
    {
        [SerializedName("displayName")]
        public String DisplayName { get; set; }

        [SerializedName("name")]
        public String Name { get; set; }

        [SerializedName("mapId")]
        public Int32 MapId { get; set; }

        [SerializedName("minCustomPlayers")]
        public Int32 MinCustomPlayers { get; set; }

        [SerializedName("totalPlayers")]
        public Int32 TotalPlayers { get; set; }

        [SerializedName("description")]
        public String Description { get; set; }

        public static GameMap SummonersRift = new GameMap()
        {
            Description =
                "The oldest and most venerated Field of Justice is known as Summoner's Rift.  This battleground is known for the constant conflicts fought between two opposing groups of Summoners.  Traverse down one of three different paths in order to attack your enemy at their weakest point.  Work with your allies to siege the enemy base and destroy their Headquarters!",
            MapId = 1,
            DisplayName = "Summoner's Rift",
            MinCustomPlayers = 1,
            Name = "SummonersRift",
            TotalPlayers = 10
        };

        public static GameMap TheCrystalScar = new GameMap()
        {
            Description =
                "The Crystal Scar was once known as the mining village of Kalamanda, until open war between Demacia and Noxus broke out over control of its vast underground riches. Settle your disputes on this Field of Justice by working with your allies to seize capture points and declare dominion over your enemies!",
            MapId = 8,
            DisplayName = "The Crystal Scar",
            MinCustomPlayers = 1,
            Name = "CrystalScar",
            TotalPlayers = 10
        };

        public static GameMap TheTwistedTreeline = new GameMap()
        {
            Description =
                "Deep in the Shadow Isles lies a ruined city shattered by magical disaster. Those who venture inside the ruins and wander through the Twisted Treeline seldom return, but those who do tell tales of horrific creatures and the vengeful dead.",
            MapId = 10,
            DisplayName = "The Twisted Treeline",
            MinCustomPlayers = 1,
            Name = "TwistedTreeline",
            TotalPlayers = 6
        };

        public static GameMap HowlingAbyss = new GameMap()
        {
            Description =
                "The Howling Abyss is a bottomless crevasse located in the coldest, cruelest, part of the Freljord. Legends say that, long ago, a great battle took place here on the narrow bridge spanning this chasm. No one remembers who fought here, or why, but it is said that if you listen carefully to the wind you can still hear the cries of the vanquished tossed howling into the Abyss.",
            MapId = 12,
            DisplayName = "Howling Abyss",
            MinCustomPlayers = 1,
            Name = "HowlingAbyss",
            TotalPlayers = 10
        };
    }
}
