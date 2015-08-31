using Sightstone.Controls;
using Sightstone.Logic;
using Sightstone.Logic.SQLite;
using Sightstone.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Sightstone.Logic.Riot.Platform;
using Timer = System.Timers.Timer;
using Sightstone.Logic.Riot;
using System.Threading.Tasks;
using agsXMPP;
using agsXMPP.protocol.client;
using Sightstone.Logic.MultiUser;

namespace Sightstone.Windows
{
    /// <summary>
    ///     Interaction logic for ChatPage.xaml
    /// </summary>
    public partial class FriendList
    {
        private static Timer UpdateTimer;
        private LargeChatPlayer PlayerItem;
        private ChatPlayerItem LastPlayerItem;
        private SelectionChangedEventArgs selection;
        bool loaded = false;
        UserClient client = new UserClient();

        /// <summary>
        /// Work on this later
        /// </summary>
        public FriendList()
        {
            InitializeComponent();
            //lazy method
            if (Client.CurrentUser != string.Empty)
                client = (UserList.Users[Client.CurrentServer])[Client.CurrentUser];

            if (Settings.Default.StatusMsg != "Set your status message")
                StatusBox.Text = Settings.Default.StatusMsg;
            UpdateTimer = new Timer(5000);
            UpdateTimer.Elapsed += UpdateChat;
            UpdateTimer.Enabled = true;
            UpdateTimer.Start();
            Client.chatlistview = ChatListView;
        }

        private void PresenceChanger_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PresenceChanger.SelectedIndex == -1)
                return;
            
            switch ((string)PresenceChanger.SelectedValue)
            {
                case "Online":
                    client.CurrentPresence = PresenceType.available;
                    client.presenceStatus = ShowType.chat;
                    break;
                case "Busy":
                    //TODO: fix away status, for some reason its not doing anything but there is a function depending on presenceStatus being "away" or not so...
                    client.CurrentPresence = PresenceType.available;
                    client.presenceStatus = ShowType.away;
                    break;
                case "Invisible":
                    client.presenceStatus = ShowType.NONE;
                    client.CurrentPresence = PresenceType.invisible;
                    break;
            }
        }

        private void UpdateChat(object sender, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async () =>
            {
                if (client.CurrentStatus != StatusBox.Text && StatusBox.Text != "Set your status message")
                    client.CurrentStatus = StatusBox.Text;
                else if (StatusBox.Text == "Set your status message")
                    client.CurrentStatus = "Online";

                Settings.Default.StatusMsg = StatusBox.Text;
                Settings.Default.Save();
                if (!Client.UpdatePlayers)
                    return;

                Client.UpdatePlayers = false;

                ChatListView.Children.Clear();

                #region "Groups"
                while (!Client.loadedGroups)
                {
                    await Task.Delay(1000);
                }
                foreach (Group g in Client.Groups)
                {
                    var playersListView = new ListView
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalContentAlignment = VerticalAlignment.Stretch
                    };
                    playersListView.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty,
                        ScrollBarVisibility.Disabled);
                    playersListView.Foreground = Brushes.White;
                    playersListView.Background = null;
                    playersListView.BorderBrush = null;
                    playersListView.MouseDoubleClick += PlayersListView_MouseDoubleClick;
                    playersListView.SelectionChanged += ChatListView_SelectionChanged;
                    playersListView.PreviewMouseWheel += PlayersListView_PreviewMouseWheel;

                    int players = 0;

                    foreach (var chatPlayerPair in Client.AllPlayers.ToArray().OrderBy(u => u.Value.Username))
                    {
                        var player = new ChatPlayer
                        {
                            Tag = chatPlayerPair.Value,
                            DataContext = chatPlayerPair.Value,
                            ContextMenu = (ContextMenu)Resources["PlayerChatMenu"],
                            PlayerName = { Content = chatPlayerPair.Value.Username },
                            LevelLabel = { Content = chatPlayerPair.Value.Level }
                        };

                        var bc = new BrushConverter();
                        var brush = (Brush)bc.ConvertFrom("#FFFFFFFF");
                        player.PlayerStatus.Content = chatPlayerPair.Value.Status;
                        player.PlayerStatus.Foreground = brush;

                        if (chatPlayerPair.Value.IsOnline && g.GroupName == chatPlayerPair.Value.Group)
                        {
                            player.Width = 250;
                            bc = new BrushConverter();
                            brush = (Brush)bc.ConvertFrom("#FFFFFFFF");
                            player.PlayerStatus.Foreground = brush;
                            string UriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon",
                                chatPlayerPair.Value.ProfileIcon + ".png");
                            player.ProfileImage.Source = Client.GetImage(UriSource);

                            if (chatPlayerPair.Value.GameStatus != "outOfGame")
                            {
                                switch (chatPlayerPair.Value.GameStatus)
                                {
                                    case "inGame":
                                        champions inGameChamp = champions.GetChampion(chatPlayerPair.Value.Champion);
                                        if (inGameChamp != null)
                                            player.PlayerStatus.Content = "In Game as " + inGameChamp.displayName;
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
                                    case "hostingRankedGame":
                                        player.PlayerStatus.Content = "Creating Ranked Game";
                                        break;
                                    case "teamSelect":
                                        player.PlayerStatus.Content = "In Team Select";
                                        break;
                                    case "hostingNormalGame":
                                        player.PlayerStatus.Content = "Creating Normal Game";
                                        break;
                                    case "hostingCoopVsAIGame":
                                        player.PlayerStatus.Content = "Creating Co-op vs. AI Game";
                                        break;
                                    case "inTeamBuilder":
                                        player.PlayerStatus.Content = "In Team Builder";
                                        break;
                                    case "tutorial":
                                        player.PlayerStatus.Content = "In Tutorial";
                                        break;
                                }
                                brush = (Brush)bc.ConvertFrom("#FFFFFF99");
                                player.PlayerStatus.Foreground = brush;
                            }

                            player.MouseRightButtonDown += player_MouseRightButtonDown;
                            player.MouseMove += ChatPlayerMouseOver;
                            player.MouseLeave += player_MouseLeave;
                            playersListView.Items.Add(player);
                            players++;
                        }
                        else if (!chatPlayerPair.Value.IsOnline && g.GroupName == "Offline")
                        {
                            player.Width = 250;
                            player.Height = 30;
                            player.PlayerName.Margin = new Thickness(5, 2.5, 0, 0);
                            player.PlayerName.SetValue(Grid.ColumnProperty, 0);
                            player.ProfileImage.Visibility = Visibility.Hidden;
                            player.ProfileImageContainer.Visibility = Visibility.Hidden;
                            player.LevelLabel.Visibility = Visibility.Hidden;
                            player.LevelLabelContainer.Visibility = Visibility.Hidden;
                            player.PlayerInfoContainer.Margin = new Thickness(5, 5, 5, 5);
                            player.PlayerInfoContainer.HorizontalAlignment = HorizontalAlignment.Left;
                            playersListView.Items.Add(player);
                            players++;
                        }
                    }
                    var groupControl = new ChatGroup
                    {
                        Width = 230,
                        PlayersLabel = { Content = players },
                        NameLabel = { Content = g.GroupName }
                    };
                    groupControl.GroupListView.Children.Add(playersListView);
                    if (g.IsOpen)
                    {
                        groupControl.ExpandLabel.Content = "-";
                        groupControl.GroupListView.Visibility = Visibility.Visible;
                    }
                    if (!string.IsNullOrEmpty(g.GroupName))
                        ChatListView.Children.Add(groupControl);
                    else Client.Log("Removed a group");
                }
                #endregion

                if (ChatListView.Children.Count > 0 && ChatListView.Children[0] is ChatGroup && loaded)
                {
                    //Stop droping 100 times
                    ((ChatGroup) ChatListView.Children[0]).GroupGrid_MouseDown(null, null);
                    loaded = true;
                }
            }));
        }

        private void player_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = (ChatPlayer)sender;
            var playerItem = (ChatPlayerItem)item.Tag;
            LastPlayerItem = playerItem;
        }

        private void PlayersListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (selection.AddedItems.Count <= 0)
                return;

            var player = (ChatPlayer)selection.AddedItems[0];
            ((ListView)e.Source).SelectedIndex = -1;
            var playerItem = (ChatPlayerItem)player.Tag;
            List<NotificationChatPlayer> items = (from object x in Client.ChatListView.Items where x.GetType() == typeof (NotificationChatPlayer) select x).Cast<NotificationChatPlayer>().ToList();
            if (items.Any(x => (string)x.PlayerLabelName.Content == playerItem.Username))
                return;

            var chatPlayer = new NotificationChatPlayer
            {
                Tag = playerItem,
                PlayerName = playerItem.Username,
                Margin = new Thickness(1, 0, 1, 0),
                PlayerLabelName = { Content = playerItem.Username }
            };
            Client.ChatListView.Items.Add(chatPlayer);
        }

        private void PlayersListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Handled)
                return;

            e.Handled = true;
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = MouseWheelEvent,
                Source = sender
            };
            var parent = ((Control)sender).Parent as UIElement;
            if (parent != null)
                parent.RaiseEvent(eventArg);
        }

        private void player_MouseLeave(object sender, MouseEventArgs e)
        {
            if (PlayerItem == null)
                return;

            Client.MainGrid.Children.Remove(PlayerItem);
            PlayerItem = null;
        }

        private void ChatPlayerMouseOver(object sender, MouseEventArgs e)
        {
            var item = (ChatPlayer)sender;
            var playerItem = (ChatPlayerItem)item.Tag;
            if (PlayerItem == null)
            {
                PlayerItem = new LargeChatPlayer();
                Client.MainGrid.Children.Add(PlayerItem);
                Panel.SetZIndex(PlayerItem, 5);
                PlayerItem.Tag = playerItem;
                if (client.PlayerNote.Any(x => x.Key == playerItem.Username))
                {
                    PlayerItem.Note.Text = client.PlayerNote[playerItem.Username];
                    PlayerItem.Note.Foreground = Brushes.Green;
                    PlayerItem.Note.Visibility = Visibility.Visible;
                }
                PlayerItem.PlayerName.Content = playerItem.Username;
                if (string.IsNullOrEmpty(playerItem.LeagueTier))
                    PlayerItem.PlayerLeague.Content = "Unranked";
                else
                    PlayerItem.PlayerLeague.Content = playerItem.LeagueTier + " " + playerItem.LeagueDivision;
                PlayerItem.PlayerStatus.Content = playerItem.Status;
                if (playerItem.RankedWins == 0)
                    PlayerItem.PlayerWins.Content = playerItem.Wins + " Normal Wins";
                else
                    PlayerItem.PlayerWins.Content = playerItem.RankedWins + " Ranked Wins";

                PlayerItem.LevelLabel.Content = playerItem.Level;
                PlayerItem.UsingLegendary.Visibility = playerItem.UsingLegendary
                    ? Visibility.Visible
                    : Visibility.Hidden;

                //PlayerItem.Dev.Visibility = playerItem.IsLegendaryDev ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                string UriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon",
                    playerItem.ProfileIcon + ".png");
                PlayerItem.ProfileImage.Source = Client.GetImage(UriSource);
                PlayerItem.BlurBackground.Source = Client.GetImage(UriSource);
                if (playerItem.Status != null)
                {
                }
                else if (playerItem.Status == null)
                {
                    client.hidelegendaryaddition = true;
                }
                else
                {
                    PlayerItem.PlayerStatus.Content = "";
                }

                if (playerItem.GameStatus != "outOfGame")
                {
                    var elapsed = new TimeSpan();
                    if (playerItem.Timestamp != 0)
                        elapsed = DateTime.Now.Subtract(Client.JavaTimeStampToDateTime(playerItem.Timestamp));

                    switch (playerItem.GameStatus)
                    {
                        case "inGame":
                            champions inGameChamp = champions.GetChampion(playerItem.Champion);
                            if (inGameChamp != null)
                                PlayerItem.InGameStatus.Text = "In Game as " + inGameChamp.displayName + Environment.NewLine +
                                                               "For " +
                                                               string.Format("{0} Minutes and {1} Seconds",
                                                                   elapsed.Minutes, elapsed.Seconds);
                            else
                                PlayerItem.InGameStatus.Text = "In Game";
                            break;
                        case "hostingPracticeGame":
                            PlayerItem.InGameStatus.Text = "Creating Custom Game";
                            break;
                        case "inQueue":
                            PlayerItem.InGameStatus.Text = "In Queue" + Environment.NewLine +
                                                           "For " +
                                                           string.Format("{0} Minutes and {1} Seconds", elapsed.Minutes,
                                                               elapsed.Seconds);
                            break;
                        case "spectating":
                            PlayerItem.InGameStatus.Text = "Spectating";
                            break;
                        case "championSelect":
                            PlayerItem.InGameStatus.Text = "In Champion Select" + Environment.NewLine +
                                                           "For " +
                                                           string.Format("{0} Minutes and {1} Seconds", elapsed.Minutes,
                                                               elapsed.Seconds);
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
                    PlayerItem.InGameStatus.Visibility = Visibility.Visible;
                }

                PlayerItem.Width = 360;
                PlayerItem.HorizontalAlignment = HorizontalAlignment.Right;
                PlayerItem.VerticalAlignment = VerticalAlignment.Top;
            }

            Point mouseLocation = e.GetPosition(Client.MainGrid);
            double yMargin = mouseLocation.Y;
            if (yMargin + 195 > Client.MainGrid.ActualHeight)
                yMargin = Client.MainGrid.ActualHeight - 95;
            PlayerItem.Margin = new Thickness(0, yMargin, 250, 0);
        }

        private void ChatListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selection = e;
        }

        private void ProfileItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (ChatPlayer)selection.AddedItems[0];
            Client.Profile.GetSummonerProfile(item.PlayerName.Content.ToString());
            Client.SwitchPage(Client.Profile);
        }

        private async void Invite_Click(object sender, RoutedEventArgs e)
        {
            if (client.isOwnerOfGame)
                await client.calls.Invite(LastPlayerItem.Id.Replace("sum", ""));
        }

        private async void AddFriendButton_Click(object sender, RoutedEventArgs e)
        {
            PublicSummoner user = await client.calls.GetSummonerByName(FriendAddBox.Text);
            var Jid = new Jid("sum" + user.SummonerId, client.XmppConnection.Server, "");
            client.PresManager.Subscribe(Jid);
            FriendAddBox.Text = "";
        }

        private async void SpectateGame_Click(object sender, RoutedEventArgs e)
        {
            switch (LastPlayerItem.GameStatus)
            {
                case "inGame":
                    {
                        PlatformGameLifecycleDTO n =
                            await client.calls.RetrieveInProgressSpectatorGameInfo(LastPlayerItem.Username);
                        if (n.GameName != null)
                            Client.LaunchSpectatorGame(client.Region.SpectatorIpAddress,
                                n.PlayerCredentials.ObserverEncryptionKey, (int)n.PlayerCredentials.GameId,
                                client.Region.InternalName);
                    }
                    break;
                case "spectating":
                    break;
            }
        }

        private void RankedStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!client.Dev) return;
            client.TierName = RankedStatus.SelectedItem.ToString().ToUpper();
            client.SetChatHover();
        }

        private void BlockFriend_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void RemoveFriend_Click(object sender, RoutedEventArgs e)
        {
            if (LastPlayerItem == null) return;
            PublicSummoner sum = await client.calls.GetSummonerByName(LastPlayerItem.Username);
            var Jid = new Jid("sum" + sum.SummonerId, client.XmppConnection.Server, "");
            client.PresManager.Unsubscribe(Jid);
            //Client.PresManager.Remove(Jid);
            Client.AllPlayers.Remove(Jid.User);
            Client.UpdatePlayers = true;
            UpdateChat(null, null);
        }
    }
}