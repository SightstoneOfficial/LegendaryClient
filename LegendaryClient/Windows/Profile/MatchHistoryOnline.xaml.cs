using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using LegendaryClient.Logic.Riot.Platform;

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    ///     Interaction logic for MatchHistoryOnline.xaml
    /// </summary>
    public partial class MatchHistoryOnline
    {
        private string sumName;
        private readonly List<MatchStats> gameStats = new List<MatchStats>();

        public MatchHistoryOnline(string name = "")
        {
            InitializeComponent();

            //Started work on
            if (string.IsNullOrEmpty(name))
            {
                name = Client.LoginPacket.AllSummonerData.Summoner.Name;
            }

            sumName = name;
        }

        public void Update(double accountId)
        {
            Client.PVPNet.GetRecentGames(accountId, GotRecentGames);
        }

        public void GotRecentGames(RecentGames result)
        {
            gameStats.Clear();
            result.GameStatistics.Sort((s1, s2) => s2.CreateDate.CompareTo(s1.CreateDate));
            foreach (var game in result.GameStatistics)
            {
                game.GameType =
                    Client.TitleCaseString(game.GameType.Replace("_GAME", string.Empty).Replace("MATCHED", "NORMAL"));
                var match = new MatchStats();
                foreach (var stat in game.Statistics)
                {
                    var type = typeof (MatchStats);
                    var fieldName = Client.TitleCaseString(stat.StatType.Replace('_', ' ')).Replace(" ", string.Empty);
                    var f = type.GetField(fieldName);
                    f.SetValue(match, stat.Value);
                }
                match.Game = game;
                gameStats.Add(match);
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                GamesListView.Items.Clear();
                foreach (var stats in gameStats)
                {
                    var item = new RecentGameOverview();
                    var gameChamp = champions.GetChampion((int) Math.Round(stats.Game.ChampionId));
                    item.ChampionImage.Source = gameChamp.icon;
                    item.ChampionNameLabel.Content = gameChamp.displayName;
                    item.ScoreLabel.Content =
                        string.Format("{0}/{1}/{2} ",
                            stats.ChampionsKilled,
                            stats.NumDeaths,
                            stats.Assists);

                    switch (stats.Game.QueueType)
                    {
                        case "NORMAL":
                            item.ScoreLabel.Content += "(Normal)";
                            break;
                        case "NORMAL_3x3":
                            item.ScoreLabel.Content += "(Normal 3v3)";
                            break;
                        case "ARAM_UNRANKED_5x5":
                            item.ScoreLabel.Content += "(ARAM)";
                            break;
                        case "NONE":
                            item.ScoreLabel.Content += "(Custom)";
                            break;
                        case "RANKED_SOLO_5x5":
                            item.ScoreLabel.Content += "(Ranked 5v5)";
                            break;
                        case "RANKED_TEAM_5x5":
                            item.ScoreLabel.Content += "(Ranked Team 5v5)";
                            break;
                        case "RANKED_TEAM_3x3":
                            item.ScoreLabel.Content += "(Ranked Team 3v3)";
                            break;
                        case "CAP_5x5":
                            item.ScoreLabel.Content += "(Team Builder)";
                            break;
                        case "BOT":
                            item.ScoreLabel.Content += "(Bots)";
                            break;
                        case "KING_PORO":
                            item.ScoreLabel.Content += "(King Poro)";
                            break;
                        default:
                            Client.Log(stats.Game.QueueType);
                            item.ScoreLabel.Content += "Please upload this log to github.";
                            break;
                    }

                    item.CreepScoreLabel.Content = stats.MinionsKilled + " minions";
                    item.DateLabel.Content = stats.Game.CreateDate;
                    item.IPEarnedLabel.Content = "+" + stats.Game.IpEarned + " IP";
                    item.PingLabel.Content = stats.Game.UserServerPing + "ms";

                    var bc = new BrushConverter();
                    var brush = (Brush) bc.ConvertFrom("#FF609E74");

                    if (Math.Abs(stats.Lose - 1) < .00001)
                    {
                        brush = (Brush) bc.ConvertFrom("#FF9E6060");
                    }
                    else if (Math.Abs(stats.Game.IpEarned) < .00001)
                    {
                        brush = (Brush) bc.ConvertFrom("#FFE27100");
                    }

                    item.GridView.Background = brush;
                    item.GridView.Width = 280;
                    GamesListView.Items.Add(item);
                }
                if (GamesListView.Items.Count > 0)
                {
                    GamesListView.SelectedIndex = 0;
                }
            }));
        }

        private void GamesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GamesListView.SelectedIndex == -1)
            {
                return;
            }

            var stats = gameStats[GamesListView.SelectedIndex];
            Browser.Focusable = false;
            Browser.Source =
                new Uri(string.Format("http://matchhistory.na.leagueoflegends.com/en/#match-details/{0}/{1}/{2}?tab=overview", 
                    Client.Region.InternalName,
                    (int)Math.Round(stats.Game.GameId),
                    stats.Game.UserId));
            Client.Log(string.Format("http://matchhistory.na.leagueoflegends.com/en/#match-details/{0}/{1}/{2}?tab=overview",
                    Client.Region.InternalName,
                    (int)Math.Round(stats.Game.GameId),
                    stats.Game.UserId));
        }
    }
}