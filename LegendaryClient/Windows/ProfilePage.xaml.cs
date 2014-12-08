using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LegendaryClient.Logic;
using LegendaryClient.Windows.Profile;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Leagues.Client.Dto;
using PVPNetConnect.RiotObjects.Platform.Summoner;

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage
    {
        public ProfilePage()
        {
            InitializeComponent();
        }

        public void ProfileCreate(string Name)
        {
            InGameContainer.Content = new Ingame();
            OverviewContainer.Content = new Overview();
            LeaguesContainer.Content = new Leagues();
            MatchHistoryContainer.Content = new MatchHistory();
            ChampionsContainer.Content = new Champions();
            RunesContainer.Content = new Runes();
            MasteriesContainer.Content = new Masteries();
            SkinsContainer.Content = new Skins();
            LeagueMatchHistoryBetaContainer.Content = new MatchHistoryOnline(Name);

            GetSummonerProfile(String.IsNullOrEmpty(Name) ? Client.LoginPacket.AllSummonerData.Summoner.Name : Name);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            GetSummonerProfile(SearchTextBox.Text);
        }

        public async void GetSummonerProfile(string s)
        {
            PublicSummoner Summoner =
                await
                    Client.PVPNet.GetSummonerByName(String.IsNullOrWhiteSpace(s)
                        ? Client.LoginPacket.AllSummonerData.Summoner.Name
                        : s);
            if (String.IsNullOrWhiteSpace(Summoner.Name))
            {
                var overlay = new MessageOverlay
                {
                    MessageTitle = {Content = "No Summoner Found"},
                    MessageTextBox = {Text = "The summoner \"" + s + "\" does not exist."}
                };
                Client.OverlayContainer.Content = overlay.Content;
                Client.OverlayContainer.Visibility = Visibility.Visible;
                return;
            }
            SummonerNameLabel.Content = Summoner.Name;
            SummonerLevelLabel.Content = "Level " + Summoner.SummonerLevel;

            if (Summoner.SummonerLevel < 30)
            {
                LeagueHeader.Visibility = Visibility.Collapsed;
            }
            else
            {
                Client.PVPNet.GetAllLeaguesForPlayer(Summoner.SummonerId, GotLeaguesForPlayer);
            }

            int ProfileIconID = Summoner.ProfileIconId;
            string uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", ProfileIconID + ".png");
            ProfileImage.Source = Client.GetImage(uriSource);

            PlatformGameLifecycleDTO n = await Client.PVPNet.RetrieveInProgressSpectatorGameInfo(s);
            if (n.GameName != null)
            {
                InGameHeader.Visibility = Visibility.Visible;
                InGameHeader.IsSelected = true;
                var ingame = InGameContainer.Content as Ingame;
                ingame.Update(n);
            }
            else
            {
                InGameHeader.Visibility = Visibility.Collapsed;
                OverviewHeader.IsSelected = true;
            }

            if (Summoner.InternalName == Client.LoginPacket.AllSummonerData.Summoner.InternalName)
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

            var history = MatchHistoryContainer.Content as MatchHistory;
            history.Update(Summoner.AcctId);

            var overview = OverviewContainer.Content as Overview;
            overview.Update(Summoner.SummonerId, Summoner.AcctId);
        }

        private void GotLeaguesForPlayer(SummonerLeaguesDTO result)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (result.SummonerLeagues != null && result.SummonerLeagues.Count > 0)
                {
                    LeagueHeader.Visibility = Visibility.Visible;
                    var overview = LeaguesContainer.Content as Leagues;
                    overview.Update(result);
                }
                else
                {
                    LeagueHeader.Visibility = Visibility.Collapsed;
                }
            }));
        }

        private void TabContainer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((string) ((TabItem) TabContainer.SelectedItem).Header)
            {
                case "Champions":
                    var champions = ChampionsContainer.Content as Champions;
                    champions.Update();
                    break;

                case "Skins":
                    var skins = SkinsContainer.Content as Skins;
                    skins.Update();
                    break;
            }
        }

        public class KeyValueItem
        {
            public object Key { get; set; }

            public object Value { get; set; }
        }
    }
}
