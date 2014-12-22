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