using LegendaryClient.Logic.SQLite;
using LegendaryClient.Windows.Profile;
using PVPNetConnect.RiotObjects.Platform.Statistics;
using PVPNetConnect.RiotObjects.Platform.Summoner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryClient.Logic.Player
{
    public class ChampStats
    {
        /// <summary>
        /// The champion who they are playing
        /// </summary>
        public champions Champ;

        /// <summary>
        /// The KDA they have with this champion ~20 aprox (last games)
        /// </summary>
        public KDA Champkda;

        /// <summary>
        /// Player's overall kda ~20 aprox
        /// </summary>
        public KDA OverallKDA;

        /// <summary>
        /// out of 100
        /// </summary>
        public int WinLossRatio;

        /// <summary>
        /// out of 100
        /// </summary>
        public int WinLossChampRatio;

        /// <summary>
        /// Games with the champ
        /// </summary>
        public int GamesWithChamp;

        public ChampStats(int champId, string playerName)
        {
            Champ = champions.GetChampion(champId);
            ChampID = Champ.id;
            LoadName(playerName);
        }

        public ChampStats(string champName, string playerName)
        {
            Champ = champions.GetChampion(champName);
            ChampID = Champ.id;
            LoadName(playerName);
        }
        
        public ChampStats(string champName, int playerName)
        {
            Champ = champions.GetChampion(champName);
            ChampID = Champ.id;
            Load(playerName);
        }

        public ChampStats(int champName, int playerName)
        {
            Champ = champions.GetChampion(champName);
            ChampID = Champ.id;
            Load(playerName);
        }

        async void LoadName(string Name)
        {
            PublicSummoner summoner = await Client.PVPNet.GetSummonerByName(Name);
            await Load(summoner.AcctId);
        }

        public int ChampID { get; set; }
        private List<MatchStats> GameStats = new List<MatchStats>();

        public async Task<string[]> Load(double ID)
        {
            RecentGames result = await Client.PVPNet.GetRecentGames(ID);
            result.GameStatistics.Sort((s1, s2) => s2.CreateDate.CompareTo(s1.CreateDate));
            GamesWithChamp = 0;
            foreach (PlayerGameStats game in result.GameStatistics)
            {
                game.GameType = Client.TitleCaseString(game.GameType.Replace("_GAME", "").Replace("MATCHED", "NORMAL"));
                var match = new MatchStats();

                foreach (RawStat stat in game.Statistics)
                {
                    Type type = typeof(MatchStats);
                    string fieldName = Client.TitleCaseString(stat.StatType.Replace('_', ' ')).Replace(" ", "");
                    FieldInfo f = type.GetField(fieldName);
                    f.SetValue(match, stat.Value);
                }
                match.Game = game;
                GameStats.Add(match);
            }
            int AKills, ChampKills;
            int ADeaths, ChampDeaths;
            int AAssists, ChampAssists;
            int AGamesPlayed, ChampGamesPlayed;
            int Wins, ChampWins;
            AKills = 0; ADeaths = 0; AAssists = 0; AGamesPlayed = 0; ChampKills = 0;
            ChampDeaths = 0; ChampAssists = 0; ChampGamesPlayed = 0; Wins = 0; ChampWins = 0;
            //Load average KDA for past 20 games if possible
            foreach (MatchStats stats in GameStats)
            {
                if (stats.Win == 1)
                    Wins++;
                champions gameChamp = champions.GetChampion((int)Math.Round(stats.Game.ChampionId));
                AKills = AKills + (Int32)stats.ChampionsKilled;
                ADeaths = ADeaths + (Int32)stats.NumDeaths;
                AAssists = AAssists + (Int32)stats.Assists;
                AGamesPlayed++;

                if (ChampID == (int)Math.Round(stats.Game.ChampionId))
                {
                    if (stats.Win == 1)
                        ChampWins++;
                    ChampKills = ChampKills + (Int32)stats.ChampionsKilled;
                    ChampDeaths = ChampDeaths + (Int32)stats.NumDeaths;
                    ChampAssists = ChampAssists + (Int32)stats.Assists;
                    ChampGamesPlayed++;
                    GamesWithChamp++;
                }
            }
            WinLossRatio = (Wins / AGamesPlayed) * 100;
            try
            {
                WinLossChampRatio = (ChampWins / ChampGamesPlayed) * 100;
            }
            catch { }

            string KDAString = string.Format("{0}/{1}/{2}",
                                (AKills / AGamesPlayed),
                                (ADeaths / AGamesPlayed),
                                (AAssists / AGamesPlayed));
            string ChampKDAString = "";
            try
            {
                ChampKDAString = string.Format("{0}/{1}/{2}",
                (ChampKills / ChampGamesPlayed),
                (ChampDeaths / ChampGamesPlayed),
                (ChampAssists / ChampGamesPlayed));

            }
            catch
            { ChampKDAString = "NO RECENT GAMES!!!"; }
            //GetKDA String
            OverallKDA = new KDA()
            {
                Kills = AKills,
                Deaths = ADeaths,
                Assists = AAssists,
                Games = AGamesPlayed
            };
            //Get champ KDA
            Champkda = new KDA()
            {
                Kills = ChampKills,
                Deaths = ChampDeaths,
                Assists = ChampAssists,
                Games = ChampGamesPlayed
            };
            return new List<string>() { ChampKDAString, KDAString }.ToArray();
        }
    }
}
