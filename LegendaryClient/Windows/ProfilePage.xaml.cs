using LegendaryClient.Logic;
using LegendaryClient.Properties;
using LegendaryClient.Windows.Profile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LegendaryClient.Logic.Riot;
using LegendaryClient.Logic.Riot.Platform;
using LegendaryClient.Logic.Riot.Team;
using LegendaryClient.Logic.MultiUser;

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage
    {
        static UserClient UserClient;
        public ProfilePage()
        {
            InitializeComponent();
            UserClient = UserList.Users[Client.Current];
            if (UserClient.Dev)
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    MatchHistoryBetaTab.Visibility = Visibility.Visible;
                    TeamsHeader.Visibility = Visibility.Visible;
                }));
            InGameContainer.Content = new Ingame();
            OverviewContainer.Content = new Overview();
            LeaguesContainer.Content = new Leagues();
            MatchHistoryContainer.Content = new MatchHistory();
            ChampionsContainer.Content = new Champions();
            RunesContainer.Content = new Runes();
            MasteriesContainer.Content = new Masteries();
            SkinsContainer.Content = new Skins();
            LeagueMatchHistoryBetaContainer.Content = new MatchHistoryOnline();
            TeamsContainer.Content = new Teams();

            getFirstWinOfTheDay();
            //Client.RiotConnection.MessageReceived += PVPNet_OnMessageReceived;
            //Auto get summoner profile when created instance.
            if (UserClient.IsLoggedIn)
                GetSummonerProfile(UserClient.LoginPacket.AllSummonerData.Summoner.Name);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            GetSummonerProfile(SearchTextBox.Text);
        }

        public async void GetSummonerProfile(string s)
        {
            PublicSummoner summoner =
                await
                    UserClient.calls.GetSummonerByName(string.IsNullOrWhiteSpace(s)
                        ? UserClient.LoginPacket.AllSummonerData.Summoner.Name
                        : s);
            if (string.IsNullOrWhiteSpace(summoner.Name))
            {
                var overlay = new MessageOverlay
                {
                    MessageTitle = { Content = "No Summoner Found" },
                    MessageTextBox = { Text = "The summoner \"" + s + "\" does not exist." }
                };
                Client.OverlayContainer.Content = overlay.Content;
                Client.OverlayContainer.Visibility = Visibility.Visible;

                return;
            }
            SummonerNameLabel.Content = summoner.Name;
            SummonerLevelLabel.Content = "Level " + summoner.SummonerLevel;

            if (summoner.SummonerLevel < 30)
            {
                LeagueHeader.Visibility = Visibility.Collapsed;
                TeamsHeader.Visibility = Visibility.Collapsed;
            }
            else
            {
                GotLeaguesForPlayer(await UserClient.calls.GetAllLeaguesForPlayer(summoner.SummonerId));
                PlayerDTO playerTeams = await UserClient.calls.FindPlayer(summoner.SummonerId);
                GotPlayerTeams(playerTeams);
            }


            int profileIconId = summoner.ProfileIconId;
            string UriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", profileIconId + ".png");
            ProfileImage.Source = Client.GetImage(UriSource);

            PlatformGameLifecycleDTO n = await UserClient.calls.RetrieveInProgressSpectatorGameInfo(s);
            if (n != null && n.GameName != null)
            {
                InGameHeader.Visibility = Visibility.Visible;
                InGameHeader.IsSelected = true;

                var ingame = InGameContainer.Content as Ingame;
                if (ingame != null)
                    ingame.Update(n, summoner.Name);
            }
            else
            {
                InGameHeader.Visibility = Visibility.Collapsed;
                OverviewHeader.IsSelected = true;
            }

            if (summoner.Name == UserClient.LoginPacket.AllSummonerData.Summoner.Name)
            {
                ChampionsTab.Visibility = Visibility.Visible;
                SkinsTab.Visibility = Visibility.Visible;

                MatchHistoryBetaTab.Margin = new Thickness(0, 0, 0, 0);
            }
            else
            {
                ChampionsTab.Visibility = Visibility.Hidden;
                SkinsTab.Visibility = Visibility.Hidden;
                MatchHistoryBetaTab.Margin = new Thickness(-211, 0, 211, 0);
            }

            var historyBeta = LeagueMatchHistoryBetaContainer.Content as MatchHistoryOnline;
            if (historyBeta != null)
                historyBeta.Update(summoner.AcctId);

            var history = MatchHistoryContainer.Content as MatchHistory;
            if (history != null)
                await history.Update(summoner.AcctId);

            var overview = OverviewContainer.Content as Overview;
            if (overview != null)
                overview.Update(summoner.SummonerId, summoner.AcctId);

            //RiotCalls.Call(Guid.NewGuid().ToString(), "championMastery", "getAllChampionMasteries", "[" + summoner.SummonerId + "]");
        }

        private void GotLeaguesForPlayer(SummonerLeaguesDTO result)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (result.SummonerLeagues != null && result.SummonerLeagues.Count > 0)
                {
                    LeagueHeader.Visibility = Visibility.Visible;

                    var overview = LeaguesContainer.Content as Leagues;
                    if (overview != null)
                        overview.Update(result);
                }
                else
                    LeagueHeader.Visibility = Visibility.Collapsed;
            }));
        }

        private void GotPlayerTeams(PlayerDTO teams)
        {

            if (teams.PlayerTeams.Count > 0)
            {
                var overview = TeamsContainer.Content as Teams;
                overview.Update(teams);
            }
            else
                TeamsHeader.Visibility = Visibility.Collapsed;
        }


        private void TabContainer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((string)((TabItem)TabContainer.SelectedItem).Header)
            {
                case "Champions":
                    var champions = ChampionsContainer.Content as Champions;
                    if (champions != null)
                        champions.Update();
                    break;

                case "Skins":
                    var skins = SkinsContainer.Content as Skins;
                    if (skins != null)
                        skins.Update();
                    break;
            }
        }

        private void getFirstWinOfTheDay()
        {
            if (UserClient.LoginPacket.TimeUntilFirstWinOfDay < 1)
            {
                FirstWinOfTheDayLabel.Content = "Ready";
            }
            else
            {
                TimeSpan time = TimeSpan.FromMilliseconds(UserClient.LoginPacket.TimeUntilFirstWinOfDay);
                FirstWinOfTheDayLabel.Content = "" + time.Hours + "h " + time.Minutes + "m " + time.Seconds + "s";
            }

        }

        public class KeyValueItem
        {
            public object Key { get; set; }

            public object Value { get; set; }
        }
    }
}