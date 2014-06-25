using PVPNetConnect.RiotObjects.Platform.Broadcast;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Game.Message;
using PVPNetConnect.RiotObjects.Platform.Messaging;
using PVPNetConnect.RiotObjects.Platform.Reroll.Pojo;
using PVPNetConnect.RiotObjects.Platform.Statistics;
using System.Collections.Generic;

namespace PVPNetConnect
{
    public partial class PVPNetConnection
    {
        public void SimulateEogPointChange()
        {
            EogPointChangeBreakdown pointBreakdown = new EogPointChangeBreakdown()
            {
                PointChangeFromChampionsOwned = 115.0,
                PointsUsed = 500.0,
                EndPoints = 145.0,
                PreviousPoints = 500.0,
                PointChangeFromGamePlay = 30.0
            };
            MessageReceived(pointBreakdown);
        }

        public void SimulateBannedFromGame()
        {
            GameNotification notification = new GameNotification()
            {
                MessageCode = "PG-0021",
                Type = "PLAYER_BANNED_FROM_GAME"
            };
            MessageReceived(notification);
        }

        public void SimulatePlayerLeftQueue()
        {
            GameNotification notification = new GameNotification()
            {
                MessageCode = "CG-0001",
                MessageArgument = "318908",
                Type = "PLAYER_QUIT"
            };
            MessageReceived(notification);
        }

        public void SimulateStartedGame()
        {
            PlayerCredentialsDto startedGame = new PlayerCredentialsDto()
            {
                /*
                EncryptionKey = "fake",
                GameId = 125432223.0,
                LastSelectedSkinIndex = 0,
                ServerIp = "59.100.95.227",
                Observer = false,
                SummonerId = 222908.0,
                ObserverServerIp = "192.64.169.29",
                DataVersion = 0,
                HandshakeToken = "fake",
                PlayerId = 20006292.0,
                ServerPort = 5129,
                ObserverServerPort = 8088,
                SummonerName = "Snowl",
                ObserverEncryptionKey = "fake",
                ChampionId = 133
                */
            };
            MessageReceived(startedGame);
        }

        public void SimulateBroadcastNotification(string Message, string Severity)
        {
            BroadcastNotification broadcast = new BroadcastNotification()
            {
                BroadcastMessages = new object[1] {
                    new Dictionary<string, object> () {
                        {"id", 0},
                        {"active", true},
                        {"content", Message},
                        {"messageKey", "generic"},
                        {"severity", Severity}
                    }
                }
            };
            MessageReceived(broadcast);
        }

        public void SimulateStoreAccountBalance(int ip, int rp)
        {
            StoreAccountBalanceNotification newBalance = new StoreAccountBalanceNotification()
            {
                Ip = ip,
                Rp = rp
            };
            MessageReceived(newBalance);
        }

        public void SimulateSimpleDialogMessage()
        {
            SimpleDialogMessage message = new SimpleDialogMessage()
            {
                TitleCode = "leagues",
                AccountId = 200006292.0,
                Type = "leagues",
                Params = new Dictionary<string, object>()
                {
                    {"leagueItem", new Dictionary<string, object>()
                        {
                            {"playerOrTeamId", 222908},
                            {"playerOrTeamName", "Snowl"},
                            {"leagueName", "Urgot's Patriots"},
                            {"queueType", "RANKED_SOLO_5x5"},
                            {"tier", "SILVER"},
                            {"rank", "III"},
                            {"leaguePoints", 95},
                            {"previousDayLeaguePosition", 5},
                            {"wins", 291},
                            {"losses", 278},
                            {"lastPlayed", 1385704806989},
                            {"timeUntilDecay", 8639913599826},
                            {"timeLastDecayMessageShown", 0},
                            {"miniSeries", null},
                            {"displayDecayWarning", false},
                            {"demotionWarning", 0},
                            {"totalPlayed", 569},
                            {"hotStreak", false},
                            {"veteran", true},
                            {"freshBlood", false},
                            {"inactive", false}
                        }
                    },
                    {"notifyReason", "LEAGUE_POINTS_UPDATE"},
                    {"leaguePointsDelta", 18},
                    {"gameId", 125707704}
                }
            };
            MessageReceived(message);
        }

        //TODO: Clean this up
        public void SimulateEndOfGame()
        {
            EndOfGameStats fakeStats = new EndOfGameStats();
            fakeStats.TalentPointsGained = 0;
            fakeStats.Ranked = false;
            fakeStats.LeveledUp = false;
            fakeStats.SkinIndex = 6001;
            fakeStats.QueueBonusEarned = 14;
            fakeStats.GameType = "MATCHED_GAME";
            fakeStats.ExperienceEarned = 0;
            fakeStats.ImbalancedTeamsNoPoints = false;
            fakeStats.BasePoints = 32;
            fakeStats.ReportGameId = 22036662;
            fakeStats.Difficulty = null;
            fakeStats.GameLength = 1411;
            fakeStats.BoostXpEarned = 0;
            fakeStats.Invalid = false;
            fakeStats.OtherTeamInfo = null;
            fakeStats.RoomName = "endGame22036662";
            fakeStats.CustomMinutesLeftToday = 120;
            fakeStats.UserId = 200006292;
            fakeStats.CoOpVsAiMinutesLeftToday = 180;
            fakeStats.LoyaltyBoostIpEarned = 0;
            fakeStats.RpEarned = 0;
            fakeStats.CompletionBonusPoints = 0;
            fakeStats.CoOpVsAiMsecsUntilReset = 0;
            fakeStats.BoostIpEarned = 0;
            fakeStats.ExperienceTotal = 23;
            fakeStats.GameId = 125432223;
            fakeStats.TimeUntilNextFirstWinBonus = 0;
            fakeStats.LoyaltyBoostXpEarned = 0;
            fakeStats.RoomPassword = "CCebDkpkYhVjrSRB";
            fakeStats.Elo = 0;
            fakeStats.IpEarned = 0;
            fakeStats.FirstWinBonus = 0;
            fakeStats.SendStatsToTournamentProvider = false;
            fakeStats.EloChange = 0;
            fakeStats.GameMode = "ARAM";
            fakeStats.QueueType = "ARAM_UNRANKED_5x5";
            fakeStats.OdinBonusIp = 0;
            fakeStats.IpTotal = 295513;
            fakeStats.CustomMsecsUntilReset = -1;
            fakeStats.TeamPlayerParticipantStats = new List<PlayerParticipantStatsSummary>();
            fakeStats.OtherTeamPlayerParticipantStats = new List<PlayerParticipantStatsSummary>();

            for (int i = 0; i < 10; i++)
            {
                PlayerParticipantStatsSummary fakePlayer = new PlayerParticipantStatsSummary();
                fakePlayer.SkinName = "Urgot";
                fakePlayer.GameId = 22035552;
                fakePlayer.ProfileIconId = 550;
                fakePlayer.Elo = 0;
                fakePlayer.Leaver = false;
                fakePlayer.Leaves = 3;
                fakePlayer.TeamId = 200;
                fakePlayer.EloChange = 0;
                fakePlayer.Level = 30;
                fakePlayer.BotPlayer = false;
                fakePlayer.UserId = 331458;
                fakePlayer.Spell2Id = 4;
                fakePlayer.Spell1Id = 21;
                fakePlayer.Losses = 59;
                fakePlayer.SummonerName = "Snowl";
                fakePlayer.Wins = 64;
                fakePlayer.Statistics = new List<RawStatDTO>();
                RawStatDTO Item0 = new RawStatDTO();
                Item0.StatTypeName = "ITEM0";
                Item0.Value = 3181;
                fakePlayer.Statistics.Add(Item0);
                RawStatDTO Item1 = new RawStatDTO();
                Item1.StatTypeName = "ITEM1";
                Item1.Value = 3046;
                fakePlayer.Statistics.Add(Item1);
                RawStatDTO Item2 = new RawStatDTO();
                Item2.StatTypeName = "ITEM2";
                Item2.Value = 3006;
                fakePlayer.Statistics.Add(Item2);
                RawStatDTO Item3 = new RawStatDTO();
                Item3.StatTypeName = "ITEM3";
                Item3.Value = 3031;
                fakePlayer.Statistics.Add(Item3);
                RawStatDTO Item4 = new RawStatDTO();
                Item4.StatTypeName = "ITEM4";
                Item4.Value = 1055;
                fakePlayer.Statistics.Add(Item4);
                RawStatDTO Item5 = new RawStatDTO();
                Item5.StatTypeName = "ITEM5";
                Item5.Value = 1036;
                fakePlayer.Statistics.Add(Item5);
                RawStatDTO Item6 = new RawStatDTO();
                Item6.StatTypeName = "ITEM6";
                Item6.Value = 0;
                fakePlayer.Statistics.Add(Item6);
                RawStatDTO GOLDEARNED = new RawStatDTO();
                GOLDEARNED.StatTypeName = "GOLD_EARNED";
                GOLDEARNED.Value = 11736;
                fakePlayer.Statistics.Add(GOLDEARNED);
                RawStatDTO Assists = new RawStatDTO();
                Assists.StatTypeName = "ASSISTS";
                Assists.Value = 23;
                RawStatDTO NUMDEATHS = new RawStatDTO();
                NUMDEATHS.StatTypeName = "NUM_DEATHS";
                NUMDEATHS.Value = 0;
                fakePlayer.Statistics.Add(NUMDEATHS);
                RawStatDTO LEVEL = new RawStatDTO();
                LEVEL.StatTypeName = "LEVEL";
                LEVEL.Value = 17;
                fakePlayer.Statistics.Add(LEVEL);
                RawStatDTO CHAMPIONSKILLED = new RawStatDTO();
                CHAMPIONSKILLED.StatTypeName = "CHAMPIONS_KILLED";
                CHAMPIONSKILLED.Value = 99;
                fakePlayer.Statistics.Add(CHAMPIONSKILLED);
                RawStatDTO MinionsKILLED = new RawStatDTO();
                MinionsKILLED.StatTypeName = "MINIONS_KILLED";
                MinionsKILLED.Value = 60;
                fakePlayer.Statistics.Add(MinionsKILLED);
                RawStatDTO WIN = new RawStatDTO();
                WIN.StatTypeName = "WIN";
                WIN.Value = 1;
                fakePlayer.Statistics.Add(WIN);
                if (i < 5)
                {
                    fakeStats.OtherTeamPlayerParticipantStats.Add(fakePlayer);
                }
                else
                {
                    fakeStats.TeamPlayerParticipantStats.Add(fakePlayer);
                }
            }

            MessageReceived(fakeStats);
        }
    }
}