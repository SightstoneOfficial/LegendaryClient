using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Security.Cryptography.X509Certificates;
using PVPNetConnect;
using PVPNetConnect.RiotObjects.Leagues.Pojo;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Leagues.Client.Dto;
using PVPNetConnect.RiotObjects.Platform.Statistics;
using PVPNetConnect.RiotObjects.Platform.Summoner;
using PVPNetConnect.RiotObjects.Platform.Summoner.Masterybook;
using PVPNetConnect.RiotObjects.Platform.Catalog.Champion;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using System.Threading.Tasks;
using LegendaryClient.Windows.Profile;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {

        public ProfilePage()
        {
            InitializeComponent();
            InGameContainer.Content = new Ingame();
            OverviewContainer.Content = new Overview();
            LeaguesContainer.Content = new Leagues();
            MatchHistoryContainer.Content = new MatchHistory();
            ChampionsContainer.Content = new Champions();
            RunesContainer.Content = new Runes();
            MasteriesContainer.Content = new Masteries();
            SkinsContainer.Content = new Skins();
            GetSummonerProfile(Client.LoginPacket.AllSummonerData.Summoner.Name);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            GetSummonerProfile(SearchTextBox.Text);
        }

        public async void GetSummonerProfile(string s)
        {
            PublicSummoner Summoner = await Client.PVPNet.GetSummonerByName(s);
            if (String.IsNullOrWhiteSpace(Summoner.Name))
            {
                SummonerNameLabel.Content = "No Summoner Found";
                SummonerLevelLabel.Content = "Level -1";
                return;
            }
            SummonerNameLabel.Content = Summoner.Name;
            SummonerLevelLabel.Content = "Level " + Summoner.SummonerLevel;

            int ProfileIconID = Summoner.ProfileIconId;
            //TODO: Convert ProfileIconID to the decompiled images
            var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", ProfileIconID + ".png"), UriKind.RelativeOrAbsolute);
            ProfileImage.Source = new BitmapImage(uriSource);

            MatchHistory history = MatchHistoryContainer.Content as MatchHistory;
            history.Update(Summoner.AcctId);
        }

        public class KeyValueItem
        {
            public object Key { get; set; }
            public object Value { get; set; }
        }
    }
}
