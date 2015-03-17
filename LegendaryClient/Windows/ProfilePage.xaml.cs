#region

using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LegendaryClient.Logic;
using LegendaryClient.Properties;
using LegendaryClient.Windows.Profile;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Leagues.Client.Dto;
using PVPNetConnect.RiotObjects.Platform.Summoner;
using PVPNetConnect.RiotObjects.Team.Dto;
using PVPNetConnect.RiotObjects.Platform.ServiceProxy.Dispatch;
using System.Web.Script.Serialization;
using PVPNetConnect;
using System.Collections.Generic;

#endregion

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
            Change();
            if (Client.Dev)
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

            //Client.PVPNet.OnMessageReceived += PVPNet_OnMessageReceived;
            //Auto get summoner profile when created instance.
            if (Client.IsLoggedIn)
                GetSummonerProfile(Client.LoginPacket.AllSummonerData.Summoner.Name);
        }

        public void Change()
        {
            var themeAccent = new ResourceDictionary
            {
                Source = new Uri(Settings.Default.Theme)
            };
            Resources.MergedDictionaries.Add(themeAccent);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            GetSummonerProfile(SearchTextBox.Text);
        }

        public async void GetSummonerProfile(string s)
        {
            PublicSummoner summoner =
                await
                    Client.PVPNet.GetSummonerByName(String.IsNullOrWhiteSpace(s)
                        ? Client.LoginPacket.AllSummonerData.Summoner.Name
                        : s);
            if (String.IsNullOrWhiteSpace(summoner.Name))
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
            SummonerNameLabel.Content = summoner.Name;
            SummonerLevelLabel.Content = "Level " + summoner.SummonerLevel;

            if (summoner.SummonerLevel < 30)
            {
                LeagueHeader.Visibility = Visibility.Collapsed;
                TeamsHeader.Visibility = Visibility.Collapsed;
            }      
            else
            {
                Client.PVPNet.GetAllLeaguesForPlayer(summoner.SummonerId, GotLeaguesForPlayer);
                PlayerDTO playerTeams = await Client.PVPNet.FindPlayer(summoner.SummonerId);
                GotPlayerTeams(playerTeams);
            }
                

            int profileIconId = summoner.ProfileIconId;
            string uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", profileIconId + ".png");
            ProfileImage.Source = Client.GetImage(uriSource);

            PlatformGameLifecycleDTO n = await Client.PVPNet.RetrieveInProgressSpectatorGameInfo(s);
            if (n.GameName != null)
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

            if (summoner.InternalName == Client.LoginPacket.AllSummonerData.Summoner.InternalName)
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
                history.Update(summoner.AcctId);

            var overview = OverviewContainer.Content as Overview;
            if (overview != null)
                overview.Update(summoner.SummonerId, summoner.AcctId);

            //Client.PVPNet.Call(Guid.NewGuid().ToString(), "championMastery", "getAllChampionMasteries", "[" + summoner.SummonerId + "]");
        }
        private void PVPNet_OnMessageReceived(object sender, object message)
        {

            if (message.GetType() == typeof(LcdsServiceProxyResponse))
            {
                var serializer = new JavaScriptSerializer();
                var deserializedJson = serializer.Deserialize<List<ChampionMastery>>((message as LcdsServiceProxyResponse).Payload);
            }
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

            if(teams.PlayerTeams.Count > 0)
            {
                var overview = TeamsContainer.Content as Teams;
                overview.Update(teams);
            }
        }


        private void TabContainer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((string) ((TabItem) TabContainer.SelectedItem).Header)
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

        public class KeyValueItem
        {
            public object Key { get; set; }

            public object Value { get; set; }
        }
    }
}