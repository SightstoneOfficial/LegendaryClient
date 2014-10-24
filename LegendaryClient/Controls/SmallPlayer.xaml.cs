using jabber.connection;
using LegendaryClient.Logic;
using PVPNetConnect.RiotObjects.Platform;
using PVPNetConnect.RiotObjects.Platform.Summoner;
using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace LegendaryClient.Controls
{
    /// <summary>
    /// Interaction logic for SmallPlayer.xaml
    /// </summary>
    public partial class SmallPlayer : UserControl
    {
        private PublicSummoner summoner;
        public int status;

        public SmallPlayer(RoomParticipant participant)
        {
            InitializeComponent();
            Load(participant);
        }

        private async void Load(RoomParticipant participant)
        {
            summoner = await Client.PVPNet.GetSummonerByName(participant.Nick);
            PlayerName.Content = summoner.Name;
            LevelLabel.Content = summoner.SummonerLevel;
            var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", summoner.ProfileIconId + ".png"), UriKind.RelativeOrAbsolute);
            ProfileImage.Source = new BitmapImage(uriSource);
        }
    }
}