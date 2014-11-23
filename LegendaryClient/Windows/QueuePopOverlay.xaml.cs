using LegendaryClient.Controls;
using LegendaryClient.Logic;
using PVPNetConnect.RiotObjects.Leagues.Pojo;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Leagues.Client.Dto;
using PVPNetConnect.RiotObjects.Platform.Summoner;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using System.Linq;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for QueuePopOverlay.xaml
    /// </summary>
    public partial class QueuePopOverlay : Page
    {
        public bool ReverseString = false;
        private static System.Timers.Timer QueueTimer;
        public int TimeLeft = 12;
        private Page previousPage;

        public QueuePopOverlay(GameDTO InitialDTO, Page previousPage)
        {
            if (InitialDTO != null)
            {
                InitializeComponent();
                Client.FocusClient();
                InitializePop(InitialDTO);
                this.previousPage = previousPage;
                TimeLeft = InitialDTO.JoinTimerDuration;
                Client.PVPNet.OnMessageReceived += PVPNet_OnMessageReceived;
                QueueTimer = new System.Timers.Timer(1000);
                QueueTimer.Elapsed += new ElapsedEventHandler(QueueElapsed);
                QueueTimer.Enabled = true;
            }
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

        private void PVPNet_OnMessageReceived(object sender, object message)
        {
            if (message.GetType() == typeof(GameDTO))
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    GameDTO QueueDTO = message as GameDTO;
                    if (QueueDTO.GameState == "TERMINATED")
                    {
                        Client.OverlayContainer.Visibility = Visibility.Hidden;
                        Client.PVPNet.OnMessageReceived -= PVPNet_OnMessageReceived;
                        return;
                    }
                    else if (QueueDTO.GameState == "PRE_CHAMP_SELECT" || QueueDTO.GameState == "CHAMP_SELECT")
                    {
                        Client.PVPNet.OnMessageReceived -= PVPNet_OnMessageReceived;
                        string s = QueueDTO.GameState;
                        Client.ChampSelectDTO = QueueDTO;
                        Client.GameID = QueueDTO.Id;
                        Client.ChampSelectDTO = QueueDTO;
                        Client.LastPageContent = Client.Container.Content;
                        Client.OverlayContainer.Visibility = Visibility.Hidden;
                        Client.SwitchPage(new ChampSelectPage(previousPage));
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
                                try
                                {
                                    player = (QueuePopPlayer)Team1ListBox.Items[i];
                                }
                                catch
                                {
                                    Client.Log("Error with queue pop");
                                }
                            }
                            else //Team 2
                            {
                                try
                                {
                                    player = (QueuePopPlayer)Team2ListBox.Items[i - (PlayerParticipantStatus.Length / 2)];
                                }
                                catch
                                {
                                    Client.Log("Error with queue pop");
                                }
                            }
                            try
                            {
                                player.ReadyCheckBox.IsChecked = true;
                            }
                            catch
                            {
                                Client.Log("Error with queue pop");
                            }
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
            AllParticipants = AllParticipants.Distinct().ToList(); //Seems to have fixed the queuepopoverlay page crashing.
                                                                   //whichever team you're on sometimes duplicates and could not find a reason as it doesn't happen a lot.
            int i = 1;
            foreach (Participant p in AllParticipants)
            {
                QueuePopPlayer player = new QueuePopPlayer();
                player.Width = 264;
                player.Height = 70;
                if (p is PlayerParticipant)
                {
                    PlayerParticipant playerPart = (PlayerParticipant)p;
                    if (!String.IsNullOrEmpty(playerPart.SummonerName))
                    {
                        player.PlayerLabel.Content = playerPart.SummonerName;
                        player.RankLabel.Content = "";
                        SummonerLeaguesDTO playerLeagues = await Client.PVPNet.GetAllLeaguesForPlayer(playerPart.SummonerId);
                        foreach (LeagueListDTO x in playerLeagues.SummonerLeagues)
                        {
                            if (x.Queue == "RANKED_SOLO_5x5")
                            {
                                player.RankLabel.Content = x.Tier + " " + x.RequestorsRank;
                            }
                        }
                        if (String.IsNullOrEmpty(player.RankLabel.Content.ToString()))
                            player.RankLabel.Content = "Unranked";
                        Team1ListBox.Items.Add(player);
                    }
                    else
                    {
                        Client.Log(playerPart.SummonerId.ToString());
                        player.PlayerLabel.Content = "Summoner " + i;
                        i++;
                        player.RankLabel.Content = "";
                        Team2ListBox.Items.Add(player);
                    }
                }
                else
                {
                    ObfuscatedParticipant oPlayer = p as ObfuscatedParticipant;
                    player.PlayerLabel.Content = "Summoner " + (oPlayer.GameUniqueId - (oPlayer.GameUniqueId > 5 ? 5 : 0));
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