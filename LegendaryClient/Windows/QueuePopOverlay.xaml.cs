using LegendaryClient.Controls;
using LegendaryClient.Logic;
using PVPNetConnect.RiotObjects.Leagues.Pojo;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Leagues.Client.Dto;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for QueuePopOverlay.xaml
    /// </summary>
    public partial class QueuePopOverlay : Page
    {
        public bool ReverseString = false;
        public bool HasStartedChampSelect = false;
        private static System.Timers.Timer QueueTimer;
        public int TimeLeft = 12;

        public QueuePopOverlay(GameDTO InitialDTO)
        {
            InitializeComponent();
            Client.IsInGame = true;
            Client.FocusClient();
            InitializePop(InitialDTO);
            TimeLeft = InitialDTO.JoinTimerDuration;
            //Client.RtmpConnection.MessageReceived += OnMessageReceived;
            Client.PVPNet.OnMessageReceived += PVPNet_OnMessageReceived;
            QueueTimer = new System.Timers.Timer(1000);
            QueueTimer.Elapsed += new ElapsedEventHandler(QueueElapsed);
            QueueTimer.Enabled = true;
        }

        internal void QueueElapsed(object sender, ElapsedEventArgs e)
        {
            if (TimeLeft <= 0)
                return;
            TimeLeft = TimeLeft - 1;
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                TimerLabel.Content = TimeLeft;
            }));
        }

        //private void OnMessageReceived(object sender, MessageReceivedEventArgs message)
        private void PVPNet_OnMessageReceived(object sender, object message)
        {
            //if (message.Body.GetType() == typeof(GameDTO))
            if (message.GetType() == typeof(GameDTO))
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    //GameDTO QueueDTO = message.Body as GameDTO;
                    GameDTO QueueDTO = message as GameDTO;
                    if (QueueDTO.GameState == "TERMINATED" || QueueDTO.GameState == "TERMINATED_IN_ERROR")
                    {
                        Client.OverlayContainer.Visibility = Visibility.Hidden;
                        //Client.RtmpConnection.MessageReceived -= OnMessageReceived;
                        Client.PVPNet.OnMessageReceived -= PVPNet_OnMessageReceived;
                        Client.IsInGame = false;
                        return;
                    }
                    else if (QueueDTO.GameState == "CHAMP_SELECT")
                    {
                        HasStartedChampSelect = true;
                        //Client.RtmpConnection.MessageReceived -= OnMessageReceived;
                        Client.PVPNet.OnMessageReceived -= PVPNet_OnMessageReceived;
                        string s = QueueDTO.GameState;
                        Client.ChampSelectDTO = QueueDTO;
                        Client.GameID = QueueDTO.Id;
                        Client.ChampSelectDTO = QueueDTO;
                        Client.LastPageContent = Client.Container.Content;
                        Client.OverlayContainer.Visibility = Visibility.Hidden;
                        Client.SwitchPage(new ChampSelectPage());
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
                            player.ReadyCheckBox.Foreground = System.Windows.Media.Brushes.Green;
                        }
                        else if (c == '2')
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
                            player.ReadyCheckBox.IsChecked = null;
                            player.ReadyCheckBox.Foreground = System.Windows.Media.Brushes.Red;
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

            int i = 0;
            foreach (Participant p in AllParticipants)
            {
                if (p is PlayerParticipant)
                {
                    QueuePopPlayer player = (QueuePopPlayer)Team1ListBox.Items[i];
                    PlayerParticipant playerPart = (PlayerParticipant)p;
                    SummonerLeaguesDTO playerLeagues = await Client.PVPNet.GetAllLeaguesForPlayer(playerPart.SummonerId);
                    foreach (LeagueListDTO x in playerLeagues.SummonerLeagues)
                    {
                        if (x.Queue == "RANKED_SOLO_5x5")
                        {
                            player.RankLabel.Content = x.Tier + " " + x.RequestorsRank;
                        }
                    }
                    //People can be ranked without having solo queue so don't put if statement checking List.Length
                    if (String.IsNullOrEmpty((string)player.RankLabel.Content))
                    {
                        player.RankLabel.Content = "Unranked";
                    }
                    i++;
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