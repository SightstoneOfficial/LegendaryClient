#region

using System;
using System.Windows;
using LegendaryClient.Logic;
using LegendaryClient.Properties;

#endregion

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    ///     Interaction logic for MatchHistoryOnline.xaml
    /// </summary>
    public partial class MatchHistoryOnline
    {
        private string SumName;

        public MatchHistoryOnline(String name = "")
        {
            InitializeComponent();
            Change();

            //Started work on
            if (String.IsNullOrEmpty(name))
                name = Client.LoginPacket.AllSummonerData.Summoner.Name;

            SumName = name;
            GetData();
        }

        public void Change()
        {
            var themeAccent = new ResourceDictionary
            {
                Source = new Uri(Settings.Default.Theme)
            };
            Resources.MergedDictionaries.Add(themeAccent);
        }

        public void GetData()
        {
            //PlayerMatchHistory history = new PlayerMatchHistory();
            //HistoryAccount acc = await history.GetPlayerAsync(Client.Region.InternalName, SumName);
            //PlayerListGames games = await history.GetGamesAsync(acc);

            //This is the first game
            //Game game = await history.GetFullGameDataAsync(games.games.games[0]);

            //This is the IndepthTimeline
            //IndepthTimeline timeline = await history.GetGameTimelineAsync(games.games.games[0]);
        }
    }
}