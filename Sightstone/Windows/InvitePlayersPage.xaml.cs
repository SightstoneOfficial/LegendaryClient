﻿using Sightstone.Controls;
using Sightstone.Logic;
using Sightstone.Logic.MultiUser;
using Sightstone.Logic.Riot;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Timer = System.Timers.Timer;

namespace Sightstone.Windows
{
    /// <summary>
    ///     Interaction logic for InvitePlayersPage.xaml
    /// </summary>
    public partial class InvitePlayersPage
    {
        private static Timer UpdateTimer;
        private readonly List<string> invitedPlayers;

        public InvitePlayersPage()
        {
            InitializeComponent();
            invitedPlayers = new List<string>();
            UpdateTimer = new Timer(1000);
            UpdateTimer.Elapsed += UpdateChat;
            UpdateTimer.Enabled = true;
            UpdateTimer.Start();
            UpdateChat(null, null);
        }

        private void UpdateChat(object sender, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                AvailablePlayerListView.Items.Clear();
                InvitedPlayerListView.Items.Clear();
                foreach (var chatPlayerPair in Client.AllPlayers.ToArray())
                {
                    if (chatPlayerPair.Value.Level == 0)
                        continue;

                    var player = new ChatPlayer
                    {
                        Width = 250,
                        Tag = chatPlayerPair.Value,
                        DataContext = chatPlayerPair.Value
                    };
                    var playerItem = (ChatPlayerItem) player.Tag;
                    player.PlayerName.Content = playerItem.Username;
                    player.PlayerStatus.Content = playerItem.Status;
                    player.PlayerId.Content = playerItem.Id;
                    player.LevelLabel.Content = chatPlayerPair.Value.Level;
                    player.ProfileImage.Source = new BitmapImage(Client.GetIconUri(chatPlayerPair.Value.ProfileIcon));

                    //Show available players
                    if (chatPlayerPair.Value.GameStatus != "outOfGame")
                        continue;

                    bool shouldBreak = false;
                    KeyValuePair<string, ChatPlayerItem> pair = chatPlayerPair;
                    foreach (InvitePlayer invitePlayer in from object x in Client.InviteListView.Items
                        select x as InvitePlayer
                        into invitePlayer
                        where (string) invitePlayer.PlayerLabel.Content == pair.Value.Username
                        where (string) invitePlayer.StatusLabel.Content == "Accepted"
                        select invitePlayer)
                        shouldBreak = true;

                    if (shouldBreak)
                        continue;

                    if (invitedPlayers.Contains(chatPlayerPair.Value.Id))
                        InvitedPlayerListView.Items.Add(player);
                    else
                        AvailablePlayerListView.Items.Add(player);
                }
            }));
        }

        private void AvailablePlayerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AvailablePlayerListView.SelectedIndex == -1)
                return;

            var player = (ChatPlayer) AvailablePlayerListView.SelectedItem;
            AvailablePlayerListView.SelectedIndex = -1;
            var playerItem = (ChatPlayerItem) player.Tag;
            player.PlayerName.Content = playerItem.Username;
            invitedPlayers.Add(playerItem.Id);
            UpdateChat(null, null);
        }

        private void InvitedPlayerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InvitedPlayerListView.SelectedIndex == -1)
                return;

            var player = (ChatPlayer) InvitedPlayerListView.SelectedItem;
            InvitedPlayerListView.SelectedIndex = -1;
            var playerItem = (ChatPlayerItem) player.Tag;
            player.PlayerName.Content = playerItem.Username;
            invitedPlayers.Remove(playerItem.Id);
            UpdateChat(null, null);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Visibility = Visibility.Hidden;
        }

        private async void SendInvitesButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (string player in invitedPlayers)
            {
                //This invites the player
                await (UserList.Users[Client.CurrentServer])[Client.CurrentUser].calls.Invite(player.Replace("sum", ""));

                ChatPlayerItem playerInfo = Client.AllPlayers[player];

                //If has already invited
                bool shouldBreak = false;
                foreach (
                    InvitePlayer invitePlayer in
                        Client.InviteListView.Items.Cast<object>()
                            .Select(x => x as InvitePlayer)
                            .Where(invitePlayer => (string) invitePlayer.PlayerLabel.Content == playerInfo.Username))
                    shouldBreak = true;

                if (shouldBreak)
                    continue;
            }
            Client.OverlayContainer.Visibility = Visibility.Hidden;
        }
    }
}
