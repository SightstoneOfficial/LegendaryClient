using LegendaryClient.Controls;
using LegendaryClient.Logic;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Summoner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for QueuePopOverlay.xaml
    /// </summary>
    public partial class QueuePopOverlay : Page
    {
        public bool ReverseString = false;
        public QueuePopOverlay(GameDTO InitialDTO)
        {
            InitializeComponent();
            InitializePop(InitialDTO);
            Client.PVPNet.OnMessageReceived += PVPNet_OnMessageReceived;
        }

        void PVPNet_OnMessageReceived(object sender, object message)
        {
            if (message.GetType() == typeof(GameDTO))
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    GameDTO QueueDTO = message as GameDTO;
                    if (QueueDTO.TerminatedCondition == "TERMINATED")
                    {
                        Client.OverlayContainer.Visibility = Visibility.Hidden;
                        Client.PVPNet.OnMessageReceived -= PVPNet_OnMessageReceived;
                        return;
                    }
                    int i = 0;
                    string PlayerParticipantStatus = (string)QueueDTO.StatusOfParticipants;
                    if (ReverseString)
                    {
                        string FirstHalf = PlayerParticipantStatus.Substring(0, PlayerParticipantStatus.Length / 2);
                        string SecondHalf = PlayerParticipantStatus.Substring(PlayerParticipantStatus.Length / 2, PlayerParticipantStatus.Length / 2);
                        PlayerParticipantStatus = SecondHalf + FirstHalf;
                    }
                    foreach (char c in PlayerParticipantStatus)
                    {
                        if (c == '1') //If checked
                        {
                            QueuePopPlayer player = null;
                            if (i < (PlayerParticipantStatus.Length / 2)) //Team 1
                            {
                                player = (QueuePopPlayer)Team1ListBox.Items[i];
                            }
                            else //Team 2
                            {
                                player = (QueuePopPlayer)Team2ListBox.Items[i - (PlayerParticipantStatus.Length / 2)];
                            }
                            player.ReadyCheckBox.IsChecked = true;
                        }
                        i++;
                    }
                }));
            }
        }

        public async void InitializePop(GameDTO InitialDTO)
        {
            List<Participant> AllParticipants = InitialDTO.TeamOne;
            AllParticipants.AddRange(InitialDTO.TeamTwo);
            if (InitialDTO.TeamOne[0] is ObfuscatedParticipant)
            {
                ReverseString = true;
            }

            foreach (Participant p in AllParticipants)
            {
                QueuePopPlayer player = new QueuePopPlayer();
                player.Width = 300;
                player.Height = 100;
                if (p is PlayerParticipant)
                {
                    PlayerParticipant playerPart = (PlayerParticipant)p;
                    player.PlayerLabel.Content = playerPart.SummonerName;
                    player.RankLabel.Content = "";
                    Team1ListBox.Items.Add(player);
                }
                else
                {
                    player.PlayerLabel.Content = "Enemy";
                    player.RankLabel.Content = "";
                    Team2ListBox.Items.Add(player);
                }
            }

            if (Client.AutoAcceptQueue)
            {
                await Client.PVPNet.AcceptPoppedGame(true);
            }
        }

        private async void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            await Client.PVPNet.AcceptPoppedGame(true);
        }
    }
}
