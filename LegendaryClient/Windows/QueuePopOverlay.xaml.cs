#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Properties;
using PVPNetConnect.RiotObjects.Leagues.Pojo;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Leagues.Client.Dto;
using Timer = System.Timers.Timer;

#endregion

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for QueuePopOverlay.xaml
    /// </summary>
    public partial class QueuePopOverlay
    {
        private static Timer QueueTimer;
        private readonly Page previousPage;
        public bool ReverseString = false;
        public int TimeLeft = 12;

        public QueuePopOverlay(GameDTO initialDto, Page previousPage)
        {
            if (initialDto == null)
                return;

            InitializeComponent();
            Change();

            Client.FocusClient();
            InitializePop(initialDto);
            this.previousPage = previousPage;
            TimeLeft = initialDto.JoinTimerDuration;
            Client.PVPNet.OnMessageReceived += PVPNet_OnMessageReceived;
            QueueTimer = new Timer(1000);
            QueueTimer.Elapsed += QueueElapsed;
            QueueTimer.Enabled = true;
        }

        public void Change()
        {
            var themeAccent = new ResourceDictionary
            {
                Source = new Uri(Settings.Default.Theme)
            };
            Resources.MergedDictionaries.Add(themeAccent);
        }

        internal void QueueElapsed(object sender, ElapsedEventArgs e)
        {
            if (TimeLeft <= 0)
                return;

            TimeLeft = TimeLeft - 1;
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() => { TimerLabel.Content = TimeLeft; }));
        }

        private void PVPNet_OnMessageReceived(object sender, object message)
        {
            if (message.GetType() == typeof (GameDTO))
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    var queueDto = message as GameDTO;
                    if (queueDto != null && queueDto.GameState == "TERMINATED")
                    {
                        Client.OverlayContainer.Visibility = Visibility.Hidden;
                        Client.PVPNet.OnMessageReceived -= PVPNet_OnMessageReceived;
                        return;
                    }
                    if (queueDto != null &&
                        (queueDto.GameState == "PRE_CHAMP_SELECT" || queueDto.GameState == "CHAMP_SELECT"))
                    {
                        Client.PVPNet.OnMessageReceived -= PVPNet_OnMessageReceived;
                        string s = queueDto.GameState;
                        Client.ChampSelectDTO = queueDto;
                        Client.GameID = queueDto.Id;
                        Client.LastPageContent = Client.Container.Content;
                        Client.OverlayContainer.Visibility = Visibility.Hidden;
                        Client.GameStatus = "championSelect";
                        Client.SetChatHover();
                        Client.SwitchPage(new ChampSelectPage(previousPage));
                    }

                    int i = 0;
                    if (queueDto == null)
                        return;

                    var playerParticipantStatus = (string) queueDto.StatusOfParticipants;
                    if (ReverseString)
                    {
                        string firstHalf = playerParticipantStatus.Substring(0, playerParticipantStatus.Length/2);
                        string secondHalf = playerParticipantStatus.Substring(playerParticipantStatus.Length/2,
                            playerParticipantStatus.Length/2);
                        playerParticipantStatus = secondHalf + firstHalf;
                    }
                    foreach (char c in playerParticipantStatus)
                    {
                        if (c == '1') //If checked
                        {
                            QueuePopPlayer player = null;
                            if (i < playerParticipantStatus.Length/2) //Team 1
                                if (i <= Team1ListBox.Items.Count - 1) player = (QueuePopPlayer) Team1ListBox.Items[i];
                                    //Team 2
                                else if (i - 5 <= Team2ListBox.Items.Count - 1)
                                    player = (QueuePopPlayer) Team2ListBox.Items[i - (playerParticipantStatus.Length/2)];

                            if (player != null)
                                player.ReadyCheckBox.IsChecked = true;
                        }
                        i++;
                    }
                }));
            }
        }

        public async void InitializePop(GameDTO initialDto)
        {
            List<Participant> allParticipants = initialDto.TeamOne;
            allParticipants.AddRange(initialDto.TeamTwo);
            if (initialDto.TeamOne[0] is ObfuscatedParticipant)
                ReverseString = true;

            allParticipants = allParticipants.Distinct().ToList();
            //Seems to have fixed the queuepopoverlay page crashing.
            //whichever team you're on sometimes duplicates and could not find a reason as it doesn't happen a lot.
            int i = 1;
            foreach (Participant p in allParticipants)
            {
                var player = new QueuePopPlayer
                {
                    Width = 264,
                    Height = 70
                };
                var part = p as PlayerParticipant;
                if (part != null)
                {
                    PlayerParticipant playerPart = part;
                    if (!String.IsNullOrEmpty(playerPart.SummonerName))
                    {
                        player.PlayerLabel.Content = playerPart.SummonerName;
                        player.RankLabel.Content = "";

                        Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async () =>
                        {
                            SummonerLeaguesDTO playerLeagues =
                                await Client.PVPNet.GetAllLeaguesForPlayer(playerPart.SummonerId);

                            foreach (
                                LeagueListDTO x in
                                    playerLeagues.SummonerLeagues.Where(x => x.Queue == "RANKED_SOLO_5x5"))
                                player.RankLabel.Content = x.Tier + " " + x.RequestorsRank;

                            if (String.IsNullOrEmpty(player.RankLabel.Content.ToString()))
                                player.RankLabel.Content = "Unranked";
                        }));

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
                    var oPlayer = p as ObfuscatedParticipant;
                    if (oPlayer != null)
                        player.PlayerLabel.Content = "Summoner " +
                                                     (oPlayer.GameUniqueId - (oPlayer.GameUniqueId > 5 ? 5 : 0));

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