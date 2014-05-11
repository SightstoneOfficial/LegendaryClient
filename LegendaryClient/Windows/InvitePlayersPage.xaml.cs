using LegendaryClient.Controls;
using LegendaryClient.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for InvitePlayersPage.xaml
    /// </summary>
    public partial class InvitePlayersPage : Page
    {
        private static System.Timers.Timer UpdateTimer;
        private List<string> invitedPlayers;
        private double GameId = 0;
        private int MapId = 0;
        private string Password = "";

        public InvitePlayersPage(double GameId, int MapId, string Password = "")
        {
            InitializeComponent();

            this.GameId = GameId;
            this.MapId = MapId;
            this.Password = Password;

            invitedPlayers = new List<string>();
            UpdateTimer = new System.Timers.Timer(1000);
            UpdateTimer.Elapsed += new System.Timers.ElapsedEventHandler(UpdateChat);
            UpdateTimer.Enabled = true;
            UpdateTimer.Start();
            UpdateChat(null, null);
        }

        private void UpdateChat(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                AvailablePlayerListView.Items.Clear();
                InvitedPlayerListView.Items.Clear();
                foreach (KeyValuePair<string, ChatPlayerItem> ChatPlayerPair in Client.AllPlayers.ToArray())
                {
                    if (ChatPlayerPair.Value.Level != 0)
                    {
                        ChatPlayer player = new ChatPlayer();
                        player.Width = 250;
                        player.Tag = ChatPlayerPair.Value;
                        player.DataContext = ChatPlayerPair.Value;
                        var uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", ChatPlayerPair.Value.ProfileIcon + ".png");
                        player.ProfileImage.Source = Client.GetImage(uriSource);

                        //Show available players
                        if (ChatPlayerPair.Value.GameStatus != "outOfGame")
                            continue;

                        bool ShouldBreak = false;
                        /*foreach (var x in Client.InviteListView.Items)
                        {
                            InvitePlayer invitePlayer = x as InvitePlayer;
                            if ((string)invitePlayer.PlayerLabel.Content == ChatPlayerPair.Value.Username)
                                if ((string)invitePlayer.StatusLabel.Content == "Accepted")
                                    ShouldBreak = true;
                        }*/
                        if (ShouldBreak)
                            continue;

                        if (invitedPlayers.Contains(ChatPlayerPair.Value.Id))
                        {
                            InvitedPlayerListView.Items.Add(player);
                        }
                        else
                        {
                            AvailablePlayerListView.Items.Add(player);
                        }
                    }
                }
            }));
        }

        private void AvailablePlayerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AvailablePlayerListView.SelectedIndex != -1)
            {
                ChatPlayer player = (ChatPlayer)AvailablePlayerListView.SelectedItem;
                AvailablePlayerListView.SelectedIndex = -1;
                ChatPlayerItem playerItem = (ChatPlayerItem)player.Tag;
                invitedPlayers.Add(playerItem.Id);
                UpdateChat(null, null);
            }
        }

        private void InvitedPlayerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InvitedPlayerListView.SelectedIndex != -1)
            {
                ChatPlayer player = (ChatPlayer)InvitedPlayerListView.SelectedItem;
                InvitedPlayerListView.SelectedIndex = -1;
                ChatPlayerItem playerItem = (ChatPlayerItem)player.Tag;
                invitedPlayers.Remove(playerItem.Id);
                UpdateChat(null, null);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Visibility = Visibility.Hidden;
        }

        private void SendInvitesButton_Click(object sender, RoutedEventArgs e)
        {
            string InviteMessage = "<body><inviteId>" + "190608647" + "</inviteId>" +
            "<userName>" + Client.LoginPacket.AllSummonerData.Summoner.Name + "</userName>" +
            "<profileIconId>" + Client.LoginPacket.AllSummonerData.Summoner.ProfileIconId + "</profileIconId>" +
            "<gameType>PRACTICE_GAME</gameType>" +
            "<gameId>" + GameId + "</gameId>" +
            "<mapId>" + MapId + "</mapId>" +
            "<gamePassword>" + Password + "</gamePassword></body>";
            /*
            foreach (string Player in invitedPlayers)
            {
                Client.Message(Player, InviteMessage, ChatSubjects.PRACTICE_GAME_INVITE);

                ChatPlayerItem PlayerInfo = Client.AllPlayers[Player];

                //If has already invited
                bool ShouldBreak = false;
                foreach (var x in Client.InviteListView.Items)
                {
                    InvitePlayer invitePlayer = x as InvitePlayer;
                    if ((string)invitePlayer.PlayerLabel.Content == PlayerInfo.Username)
                        ShouldBreak = true;
                }
                if (ShouldBreak)
                    continue;

                InvitePlayer InvitePlayerItem = new InvitePlayer();
                InvitePlayerItem.PlayerLabel.Content = PlayerInfo.Username;
                InvitePlayerItem.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                Client.InviteListView.Items.Add(InvitePlayerItem);
            }
            Client.OverlayContainer.Visibility = Visibility.Hidden;*/
        }
    }
}