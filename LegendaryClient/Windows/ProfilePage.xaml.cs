using LegendaryClient.Logic;
using LegendaryClient.Windows.Profile;
using PVPNetConnect.RiotObjects.Platform.Summoner;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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