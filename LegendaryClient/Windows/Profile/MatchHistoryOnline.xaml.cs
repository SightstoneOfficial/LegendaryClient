#region

using System;
using System.Windows;
using LegendaryClient.Logic;
using LegendaryClient.Properties;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Threading;
using PVPNetConnect.RiotObjects.Platform.Statistics;
using System.Collections.Generic;
using LegendaryClient.Controls;
using LegendaryClient.Logic.SQLite;
using System.Windows.Media;

#endregion

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    ///     Interaction logic for MatchHistoryOnline.xaml
    /// </summary>
    public partial class MatchHistoryOnline
    {
        private string SumName;
        private List<MatchStats> GameStats = new List<MatchStats>();
        public MatchHistoryOnline(String name = "")
        {
            InitializeComponent();
            Change();

            //Started work on
            if (String.IsNullOrEmpty(name))
                name = Client.LoginPacket.AllSummonerData.Summoner.Name;

            SumName = name;
        }

        public void Change()
        {
            var themeAccent = new ResourceDictionary
            {
                Source = new Uri(Settings.Default.Theme)
            };
            Resources.MergedDictionaries.Add(themeAccent);
        }

        public void Update(double AccountId)
        {
            Client.PVPNet.GetRecentGames(AccountId, new RecentGames.Callback(GotRecentGames));
        }

        public void GotRecentGames(RecentGames result)
        {
            GameStats.Clear();
            result.GameStatistics.Sort((s1, s2) => s2.CreateDate.CompareTo(s1.CreateDate));
            foreach (PlayerGameStats Game in result.GameStatistics)
            {
                Game.GameType = Client.TitleCaseString(Game.GameType.Replace("_GAME", "").Replace("MATCHED", "NORMAL"));
                MatchStats Match = new MatchStats();

                foreach (RawStat Stat in Game.Statistics)
                {
                    var type = typeof(MatchStats);
                    string fieldName = Client.TitleCaseString(Stat.StatType.Replace('_', ' ')).Replace(" ", "");
                    var f = type.GetField(fieldName);
                    f.SetValue(Match, Stat.Value);
                }

                Match.Game = Game;

                GameStats.Add(Match);
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                GamesListView.Items.Clear();
                foreach (MatchStats stats in GameStats)
                {
                    RecentGameOverview item = new RecentGameOverview();
                    champions GameChamp = champions.GetChampion((int)Math.Round(stats.Game.ChampionId));
                    item.ChampionImage.Source = GameChamp.icon;
                    item.ChampionNameLabel.Content = GameChamp.displayName;
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

                    BrushConverter bc = new BrushConverter();
                    Brush brush = (Brush)bc.ConvertFrom("#FF609E74");

                    if (stats.Lose == 1)
                        brush = (Brush)bc.ConvertFrom("#FF9E6060");

                    else if (stats.Game.IpEarned == 0)
                        brush = (Brush)bc.ConvertFrom("#FFE27100");

                    item.GridView.Background = brush;
                    item.GridView.Width = 250;
                    GamesListView.Items.Add(item);
                }
                if (GamesListView.Items.Count > 0) GamesListView.SelectedIndex = 0;
            }));
        }

        private void GamesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GamesListView.SelectedIndex != -1)
            {
                MatchStats stats = GameStats[GamesListView.SelectedIndex];
                Browser.Source = new Uri("http://matchhistory.na.leagueoflegends.com/en/#match-details/" + Client.Region.InternalName + "/" + (int)Math.Round(stats.Game.GameId) + "/" + stats.Game.UserId);
                
            }
        }
    }
}