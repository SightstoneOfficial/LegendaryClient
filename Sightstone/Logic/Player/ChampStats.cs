﻿using Sightstone.Logic.SQLite;
using Sightstone.Windows.Profile;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Sightstone.Logic.Riot.Platform;
using Sightstone.Logic.Riot;
using Sightstone.Logic.MultiUser;

namespace Sightstone.Logic.Player
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

        public UserClient UserClient;

        public ChampStats(int champId, string playerName)
        {
            UserClient = (UserList.Users[Client.CurrentServer])[Client.CurrentUser];
            Champ = champions.GetChampion(champId);
            ChampID = Champ.id;
            LoadName(playerName);
        }

        public ChampStats(string champName, string playerName)
        {
            UserClient = (UserList.Users[Client.CurrentServer])[Client.CurrentUser];
            Champ = champions.GetChampion(champName);
            ChampID = Champ.id;
            LoadName(playerName);
        }


        public ChampStats(string champName, int playerName)
        {
            UserClient = (UserList.Users[Client.CurrentServer])[Client.CurrentUser];
            Champ = champions.GetChampion(champName);
            ChampID = Champ.id;
            Load(playerName);
        }

        public ChampStats(int champName, int playerName)
        {
            UserClient = (UserList.Users[Client.CurrentServer])[Client.CurrentUser];
            Champ = champions.GetChampion(champName);
            ChampID = Champ.id;
            Load(playerName);
        }

        async void LoadName(string Name)
        {
            PublicSummoner summoner = await UserClient.calls.GetSummonerByName(Name);
            await Load(summoner.AcctId);
        }

        public int ChampID { get; set; }
        private List<MatchStats> GameStats = new List<MatchStats>();

        public async Task<string[]> Load(double ID)
        {
            RecentGames result = await UserClient.calls.GetRecentGames(ID);
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
                AKills = AKills + (int)stats.ChampionsKilled;
                ADeaths = ADeaths + (int)stats.NumDeaths;
                AAssists = AAssists + (int)stats.Assists;
                AGamesPlayed++;

                if (ChampID == (int)Math.Round(stats.Game.ChampionId))
                {
                    if (stats.Win == 1)
                        ChampWins++;
                    ChampKills = ChampKills + (int)stats.ChampionsKilled;
                    ChampDeaths = ChampDeaths + (int)stats.NumDeaths;
                    ChampAssists = ChampAssists + (int)stats.Assists;
                    ChampGamesPlayed++;
                    GamesWithChamp++;
                }
            }
            WinLossRatio = (Wins / AGamesPlayed) * 100;
            try
            {
                WinLossChampRatio = (ChampGamesPlayed != 0 && ChampGamesPlayed != 0 ? ChampWins / ChampGamesPlayed : 1) * 100;
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
                    (ChampKills != 0 && ChampGamesPlayed != 0 ? ChampKills / ChampGamesPlayed : 0),
                    (ChampDeaths != 0 && ChampGamesPlayed != 0 ? ChampDeaths / ChampGamesPlayed : 0),
                    (ChampAssists != 0 && ChampGamesPlayed != 0 ? ChampAssists / ChampGamesPlayed : 0));

            }
            catch
            { ChampKDAString = "NO RECENT GAMES!!!"; }
            //GetKDA string
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
