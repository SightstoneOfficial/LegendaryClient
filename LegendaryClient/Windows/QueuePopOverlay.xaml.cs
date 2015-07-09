using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SoundLogic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LegendaryClient.Logic.Riot;
using LegendaryClient.Logic.Riot.Leagues;
using LegendaryClient.Logic.Riot.Platform;
using RtmpSharp.Messaging;
using Timer = System.Timers.Timer;
using LegendaryClient.Logic.MultiUser;

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for QueuePopOverlay.xaml
    /// </summary>
    public partial class QueuePopOverlay
    {
        private static Timer QueueTimer;
        private readonly Page previousPage;
        private ChampSelectPage page;
        public bool ReverseString;
        public int TimeLeft = 12;
        private bool accepted;
        static UserClient UserClient = UserList.users[Client.Current];

        public QueuePopOverlay(GameDTO InitialDTO, Page previousPage)
        {
            if (InitialDTO == null)
                return;

            InitializeComponent();
            Client.FocusClient();
            InitializePop(InitialDTO);
            this.previousPage = previousPage;
            page = new ChampSelectPage(InitialDTO.RoomName, InitialDTO.RoomPassword);
            TimeLeft = InitialDTO.JoinTimerDuration;
            UserClient.RiotConnection.MessageReceived += PVPNet_OnMessageReceived;
            QueueTimer = new Timer(1000);
            QueueTimer.Elapsed += QueueElapsed;
            QueueTimer.Enabled = true;
            Client.MainWin.FlashWindow();
            if (!Properties.Settings.Default.DisableClientSound)
                QueuePopSound.PlayQueuePopSound();
        }

        internal void QueueElapsed(object sender, ElapsedEventArgs e)
        {
            if (TimeLeft <= 0)
                return;

            TimeLeft = TimeLeft - 1;
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() => { TimerLabel.Content = TimeLeft; }));
        }

        private void PVPNet_OnMessageReceived(object sender, MessageReceivedEventArgs message)
        {
            if (message.Body.GetType() == typeof(GameDTO))
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    var QueueDTO = message.Body as GameDTO;
                    if (QueueDTO != null && QueueDTO.GameState == "TERMINATED")
                    {
                        Client.OverlayContainer.Visibility = Visibility.Hidden;
                        UserClient.RiotConnection.MessageReceived -= PVPNet_OnMessageReceived;
                        UserClient.SendAccept(accepted);
                        Client.HasPopped = false;
                        return;
                    }
                    if (QueueDTO != null &&
                        (QueueDTO.GameState == "PRE_CHAMP_SELECT" || QueueDTO.GameState == "CHAMP_SELECT"))
                    {
                        UserClient.RiotConnection.MessageReceived -= PVPNet_OnMessageReceived;
                        string s = QueueDTO.GameState;
                        UserClient.ChampSelectDTO = QueueDTO;
                        UserClient.GameID = QueueDTO.Id;
                        Client.LastPageContent = Client.Container.Content;
                        Client.OverlayContainer.Visibility = Visibility.Hidden;
                        UserClient.GameStatus = "championSelect";
                        UserClient.SetChatHover();
                        Client.SwitchPage(page.Load(this));
                        UserClient.SendAccept(accepted);
                        Client.HasPopped = false;
                    }

                    int i = 0;
                    if (QueueDTO == null)
                        return;

                    var playerParticipantStatus = (string)QueueDTO.StatusOfParticipants;
                    if (ReverseString)
                    {
                        string firstHalf = playerParticipantStatus.Substring(0, playerParticipantStatus.Length / 2);
                        string secondHalf = playerParticipantStatus.Substring(playerParticipantStatus.Length / 2,
                            playerParticipantStatus.Length / 2);
                        playerParticipantStatus = secondHalf + firstHalf;
                    }
                    foreach (char c in playerParticipantStatus)
                    {
                        QueuePopPlayer player = null;
                        if (i < playerParticipantStatus.Length / 2) //Team 1
                        {
                            if (i <= Team1ListBox.Items.Count - 1)
                                player = (QueuePopPlayer)Team1ListBox.Items[i];
                        }
                        else if (i - 5 <= Team2ListBox.Items.Count - 1) //Team 2
                            player = (QueuePopPlayer)Team2ListBox.Items[i - (playerParticipantStatus.Length / 2)];

                        if (player != null)
                        {
                            switch (c)
                            {
                                case '1':
                                    player.ReadyImage.Source = Client.GetImage("/LegendaryClient;component/accepted.png");
                                    break;
                                case '2':
                                    player.ReadyImage.Source = Client.GetImage("/LegendaryClient;component/declined.png");
                                    break;
                            }
                        }

                        i++;
                    }
                }));
            }
        }

        public async void InitializePop(GameDTO InitialDTO)
        {
            List<Participant> allParticipants = InitialDTO.TeamOne;
            allParticipants.AddRange(InitialDTO.TeamTwo);
            if (InitialDTO.TeamOne[0] is ObfuscatedParticipant)
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
                    if (!string.IsNullOrEmpty(playerPart.SummonerName))
                    {
                        player.PlayerLabel.Content = playerPart.SummonerName;
                        player.RankLabel.Content = "";
                        player.Tag = part;
                        Team1ListBox.Items.Add(player);
                    }
                    else
                    {
                        Client.Log(playerPart.SummonerId.ToString(CultureInfo.InvariantCulture));
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
            GetPlayerLeagues();
            if (!UserClient.AutoAcceptQueue)
                return;

            await UserClient.calls.AcceptPoppedGame(true);
            accepted = true;
            
        }

        private async void GetPlayerLeagues()
        {
            foreach (QueuePopPlayer item in Team1ListBox.Items)
            {
                var playerInfo = item.Tag as PlayerParticipant;
                await Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async () =>
                {
                    SummonerLeaguesDTO playerLeagues =
                        await UserClient.calls.GetAllLeaguesForPlayer(playerInfo.SummonerId);

                    foreach (
                        LeagueListDTO x in
                            playerLeagues.SummonerLeagues.Where(x => x.Queue == "RANKED_SOLO_5x5"))
                        item.RankLabel.Content = x.Tier + " " + x.RequestorsRank;

                    if (string.IsNullOrEmpty(item.RankLabel.Content.ToString()))
                        item.RankLabel.Content = "Unranked";
                }));
            }
        }

        private async void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            await UserClient.calls.AcceptPoppedGame(true);
            accepted = true;
        }

        private async void DeclineButton_Click(object sender, RoutedEventArgs e)
        {
            await UserClient.calls.AcceptPoppedGame(false);
            accepted = false;

            //TODO: temp, use the Client.PlayerAccepedQueue event instead
            if (previousPage is TeamQueuePage)
                (previousPage as TeamQueuePage).VisualQueueLeave();
        }
    }
}