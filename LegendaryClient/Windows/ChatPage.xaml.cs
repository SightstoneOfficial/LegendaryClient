using jabber.protocol.client;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.Region;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect.RiotObjects.Platform.Game;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for ChatPage.xaml
    /// </summary>
    public partial class ChatPage : Page
    {
        private static System.Timers.Timer UpdateTimer;
        private LargeChatPlayer PlayerItem;
        private ChatPlayerItem LastPlayerItem;

        public ChatPage()
        {
            InitializeComponent();
            if (Properties.Settings.Default.StatusMsg != "Set your status message")
                StatusBox.Text = Properties.Settings.Default.StatusMsg;
            UpdateTimer = new System.Timers.Timer(1000);
            UpdateTimer.Elapsed += new System.Timers.ElapsedEventHandler(UpdateChat);
            UpdateTimer.Enabled = true;
            UpdateTimer.Start();
            Client.chatlistview = this.ChatListView;
            Change();
        }

        private void PresenceChanger_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PresenceChanger.SelectedIndex != -1)
            {
                switch ((string)PresenceChanger.SelectedValue)
                {
                    case "Online":
                        Client.CurrentPresence = PresenceType.available;
                        Client.presenceStatus = "chat";
                        break;
                    case "Busy": //TODO: fix away status, for some reason its not doing anything but there is a function depending on presenceStatus being "away" or not so...
                        Client.CurrentPresence = PresenceType.available;
                        Client.presenceStatus = "away";
                        break;
                    case "Invisible":
                        Client.presenceStatus = "";
                        Client.CurrentPresence = PresenceType.invisible;
                        break;
                }
            }
        }

        private void UpdateChat(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (Client.CurrentStatus != StatusBox.Text && StatusBox.Text != "Set your status message")
                {
                    Client.CurrentStatus = StatusBox.Text;
                }
                else if (StatusBox.Text == "Set your status message")
                {
                    Client.CurrentStatus = "Online";
                }

                Properties.Settings.Default.StatusMsg = StatusBox.Text;
                Properties.Settings.Default.Save();

                if (Client.UpdatePlayers)
                {
                    Client.UpdatePlayers = false;

                    ChatListView.Items.Clear();
                    foreach (KeyValuePair<string, ChatPlayerItem> ChatPlayerPair in Client.AllPlayers.ToArray())
                    {
                        if (ChatPlayerPair.Value.Level != 0)
                        {
                            ChatPlayer player = new ChatPlayer();
                            player.Width = 250;
                            player.Tag = ChatPlayerPair.Value;
                            player.PlayerName.Content = ChatPlayerPair.Value.Username;
                            player.LevelLabel.Content = ChatPlayerPair.Value.Level;
                            BrushConverter bc = new BrushConverter();
                            Brush brush = (Brush)bc.ConvertFrom("#FFFFFFFF");
                            player.PlayerStatus.Content = ChatPlayerPair.Value.Status;
                            player.PlayerStatus.Foreground = brush;
                            var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", ChatPlayerPair.Value.ProfileIcon + ".png"), UriKind.RelativeOrAbsolute);
                            player.ProfileImage.Source = new BitmapImage(uriSource);

                            if (ChatPlayerPair.Value.GameStatus != "outOfGame")
                            {
                                switch (ChatPlayerPair.Value.GameStatus)
                                {
                                    case "inGame":
                                        champions InGameChamp = champions.GetChampion(ChatPlayerPair.Value.Champion);
                                        if (InGameChamp != null)
                                            player.PlayerStatus.Content = "In Game as " + InGameChamp.displayName;
                                        else
                                            player.PlayerStatus.Content = "In Game";
                                        break;
                                    case "hostingPracticeGame":
                                        player.PlayerStatus.Content = "Creating Custom Game";
                                        break;
                                    case "inQueue":
                                        player.PlayerStatus.Content = "In Queue";
                                        break;
                                    case "spectating":
                                        player.PlayerStatus.Content = "Spectating";
                                        break;
                                    case "championSelect":
                                        player.PlayerStatus.Content = "In Champion Select";
                                        break;
                                }
                                brush = (Brush)bc.ConvertFrom("#FFFFFF99");
                                player.PlayerStatus.Foreground = brush;
                            }

                            player.ContextMenu = (ContextMenu)Resources["PlayerChatMenu"];
                            player.MouseMove += ChatPlayerMouseOver;
                            player.MouseLeave += player_MouseLeave;
                            ChatListView.Items.Add(player);
                        }
                    }
                }
            }));
        }

        private void player_MouseLeave(object sender, MouseEventArgs e)
        {
            if (PlayerItem != null)
            {
                Client.MainGrid.Children.Remove(PlayerItem);
                PlayerItem = null;
            }
        }

        private void ChatPlayerMouseOver(object sender, MouseEventArgs e)
        {
            ChatPlayer item = (ChatPlayer)sender;
            ChatPlayerItem playerItem = (ChatPlayerItem)item.Tag;
            if (PlayerItem == null)
            {
                PlayerItem = new LargeChatPlayer();
                Client.MainGrid.Children.Add(PlayerItem);
                Panel.SetZIndex(PlayerItem, 5);
                PlayerItem.Tag = playerItem;
                PlayerItem.PlayerName.Content = playerItem.Username;
                PlayerItem.PlayerLeague.Content = playerItem.LeagueTier + " " + playerItem.LeagueDivision;
                PlayerItem.PlayerStatus.Text = playerItem.Status;
                if (playerItem.RankedWins == 0)
                    PlayerItem.PlayerWins.Content = playerItem.Wins + " Normal Wins";
                else
                    PlayerItem.PlayerWins.Content = playerItem.RankedWins + " Ranked Wins";
                PlayerItem.LevelLabel.Content = playerItem.Level;
                PlayerItem.UsingLegendary.Visibility = playerItem.UsingLegendary ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;

                //PlayerItem.Dev.Visibility = playerItem.IsLegendaryDev ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", playerItem.ProfileIcon + ".png"), UriKind.RelativeOrAbsolute);
                PlayerItem.ProfileImage.Source = new BitmapImage(uriSource);
                if (playerItem.Status != null)
                {

                }
                else if (playerItem.Status == null)
                {
                    Client.hidelegendaryaddition = true;
                }
                else
                {
                    PlayerItem.PlayerStatus.Text = "";
                }

                if (playerItem.GameStatus != "outOfGame")
                {
                    TimeSpan elapsed = new TimeSpan();
                    if (playerItem.Timestamp != 0)
                    {
                        elapsed = DateTime.Now.Subtract(Client.JavaTimeStampToDateTime(playerItem.Timestamp));
                    }
                    switch (playerItem.GameStatus)
                    {
                        case "inGame":
                            champions InGameChamp = champions.GetChampion(playerItem.Champion);
                            if (InGameChamp != null)
                                PlayerItem.InGameStatus.Text = "In Game" + Environment.NewLine +
                                                               "Playing as " + InGameChamp.displayName + Environment.NewLine +
                                                               "For " + string.Format("{0} Minutes and {1} Seconds", elapsed.Minutes, elapsed.Seconds) ;
                            else
                                PlayerItem.InGameStatus.Text = "In Game";
                            break;
                        case "hostingPracticeGame":
                            PlayerItem.InGameStatus.Text = "Creating Custom Game";
                            break;
                        case "inQueue":
                            PlayerItem.InGameStatus.Text = "In Queue" + Environment.NewLine +
                                                           "For " + string.Format("{0} Minutes and {1} Seconds", elapsed.Minutes, elapsed.Seconds) ;
                            break;
                        case "spectating":
                            PlayerItem.InGameStatus.Text = "Spectating";
                            break;
                        case "championSelect":
                            PlayerItem.InGameStatus.Text = "In Champion Select" + Environment.NewLine +
                                                           "For " + string.Format("{0} Minutes and {1} Seconds", elapsed.Minutes, elapsed.Seconds);
                            break;
                        case "hostingRankedGame":
                            PlayerItem.InGameStatus.Text = "Creating Ranked Game";
                            break;
                        case "teamSelect":
                            PlayerItem.InGameStatus.Text = "In Team Select";
                            break;
                        case "hostingNormalGame":
                            PlayerItem.InGameStatus.Text = "Creating Normal Game";
                            break;
                        case "hostingCoopVsAIGame":
                            PlayerItem.InGameStatus.Text = "Creating Co-op vs. AI Game";
                            break;
                        case "inTeamBuilder":
                            PlayerItem.InGameStatus.Text = "In Team Builder";
                            break;
                        case "tutorial":
                            PlayerItem.InGameStatus.Text = "In Tutorial";
                            break;                           
                    }
                    PlayerItem.InGameStatus.Visibility = System.Windows.Visibility.Visible;
                }

                PlayerItem.Width = 250;
                PlayerItem.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                PlayerItem.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            }

            Point MouseLocation = e.GetPosition(Client.MainGrid);
            double YMargin = MouseLocation.Y;
            if (YMargin + 195 > Client.MainGrid.ActualHeight)
                YMargin = Client.MainGrid.ActualHeight - 195;
            PlayerItem.Margin = new Thickness(0, YMargin, 250, 0);
        }

        private void ChatListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChatListView.SelectedIndex != -1)
            {
                ChatPlayer player = (ChatPlayer)ChatListView.SelectedItem;
                ChatListView.SelectedIndex = -1;
                ChatPlayerItem playerItem = (ChatPlayerItem)player.Tag;
                LastPlayerItem = playerItem;
                foreach (object x in Client.ChatListView.Items)
                {
                    if(x.GetType() == typeof(NotificationChatPlayer))
                        if ((string)(x as NotificationChatPlayer).PlayerLabelName.Content == playerItem.Username)
                            return;
                }
                NotificationChatPlayer ChatPlayer = new NotificationChatPlayer();
                ChatPlayer.Tag = playerItem;
                ChatPlayer.PlayerName = playerItem.Username;
                ChatPlayer.Margin = new Thickness(1, 0, 1, 0);
                ChatPlayer.PlayerLabelName.Content = playerItem.Username;
                Client.ChatListView.Items.Add(ChatPlayer);
            }
        }

        private void ProfileItem_Click(object sender, RoutedEventArgs e)
        {
            Client.SwitchPage(new ProfilePage(LastPlayerItem.Username));
        }

#pragma warning disable 4014

        public void Change()
        {
            var bc = new BrushConverter();
            bool x = Properties.Settings.Default.DarkTheme;
            if (x)
                TheGrid.Background = (Brush)bc.ConvertFrom("#E5000000");
            else
                TheGrid.Background = (Brush)bc.ConvertFrom("#E5B4B4B4");
        }

        private void Invite_Click(object sender, RoutedEventArgs e)
        {
            if(Client.isOwnerOfGame == true)
            {
                Client.PVPNet.Invite(LastPlayerItem.Id.Replace("sum", ""));
            }
            else
            {

            }
        }

        private void AddFriendButton_Click(object sender, RoutedEventArgs e)
        {
            //Fix Add Friend Button
        }

        private async void SpectateGame_Click(object sender, RoutedEventArgs e)
        {
            if (LastPlayerItem.GameStatus == "inGame")
            {
                PlatformGameLifecycleDTO n = await Client.PVPNet.RetrieveInProgressSpectatorGameInfo(LastPlayerItem.Username);
                if (n.GameName != null)
                {
                    Client.LaunchSpectatorGame(Client.Region.SpectatorIpAddress, n.PlayerCredentials.ObserverEncryptionKey, (int)n.PlayerCredentials.GameId, Client.Region.InternalName);
                }
            }
            else if (LastPlayerItem.GameStatus == "spectating")
            {

            }
        }
    }
}