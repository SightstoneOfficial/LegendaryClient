using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using LegendaryClient.Logic;
using LegendaryClient.Controls;
using System.Windows.Threading;
using System.Threading;
using jabber.protocol.client;
using System.IO;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for ChatPage.xaml
    /// </summary>
    public partial class ChatPage : Page
    {
        static System.Timers.Timer UpdateTimer;
        LargeChatPlayer PlayerItem;
        PlayerChatBox ChatBox;

        public ChatPage()
        {
            InitializeComponent();
            UpdateTimer = new System.Timers.Timer(1000);
            UpdateTimer.Elapsed += new System.Timers.ElapsedEventHandler(UpdateChat);
            UpdateTimer.Enabled = true;
            UpdateTimer.Start();
        }

        private void PresenceChanger_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PresenceChanger.SelectedIndex != -1)
            {
                switch ((string)PresenceChanger.SelectedValue)
                {
                    case "Online":
                        Client.CurrentPresence = PresenceType.available;
                        break;
                    case "Invisible":
                        Client.CurrentPresence = PresenceType.invisible;
                        break;
                }
            }
        }

        void UpdateChat(object sender, System.Timers.ElapsedEventArgs e)
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
                            player.PlayerStatus.Content = ChatPlayerPair.Value.Status;
                            var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", ChatPlayerPair.Value.ProfileIcon + ".png"), UriKind.RelativeOrAbsolute);
                            player.ProfileImage.Source = new BitmapImage(uriSource);
                            player.ContextMenu = (ContextMenu)Resources["PlayerChatMenu"];
                            player.MouseMove += ChatPlayerMouseOver;
                            player.MouseLeave += player_MouseLeave;
                            ChatListView.Items.Add(player);
                        }
                    }
                }
            }));
        }

        void player_MouseLeave(object sender, MouseEventArgs e)
        {
            if (PlayerItem != null)
            {
                Client.MainGrid.Children.Remove(PlayerItem);
                PlayerItem = null;
            }
        }

        void ChatPlayerMouseOver(object sender, MouseEventArgs e)
        {
            ChatPlayer item = (ChatPlayer)sender;
            ChatPlayerItem playerItem = (ChatPlayerItem)item.Tag;
            if (PlayerItem == null)
            {
                PlayerItem = new LargeChatPlayer();
                Client.MainGrid.Children.Add(PlayerItem);
            }
            PlayerItem.Tag = playerItem;
            PlayerItem.PlayerName.Content = playerItem.Username;
            PlayerItem.PlayerLeague.Content = playerItem.LeagueTier + " " + playerItem.LeagueDivision;
            if (playerItem.RankedWins == 0)
                PlayerItem.PlayerWins.Content = playerItem.Wins + " Normal Wins";
            else
                PlayerItem.PlayerWins.Content = playerItem.RankedWins + " Ranked Wins";
            PlayerItem.LevelLabel.Content = playerItem.Level;
            PlayerItem.UsingLegendary.Visibility = playerItem.UsingLegendary ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", playerItem.ProfileIcon + ".png"), UriKind.RelativeOrAbsolute);
            PlayerItem.ProfileImage.Source = new BitmapImage(uriSource);
            if (playerItem.Status != null)
            {
                PlayerItem.PlayerStatus.Text = playerItem.Status.Replace("∟", "");
            }
            else
            {
                PlayerItem.PlayerStatus.Text = "";
            }
            PlayerItem.Width = 250;
            PlayerItem.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            PlayerItem.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            Point MouseLocation = e.GetPosition(Client.MainGrid);
            double YMargin = MouseLocation.Y;
            if (YMargin + 155 > Client.MainGrid.ActualHeight)
                YMargin = Client.MainGrid.ActualHeight - 155;
            PlayerItem.Margin = new Thickness(0, YMargin, 250, 0);
        }

        private void ChatListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChatListView.SelectedIndex != -1)
            {
                if (ChatListView.SelectedItem is PlayerChatBox)
                    return;
                if (ChatBox != null)
                {
                    ChatListView.Items.Remove(ChatBox);
                }
                ChatBox = new PlayerChatBox();
                ChatBox.Width = 225;
                ChatBox.Player = (ChatPlayerItem)((ChatPlayer)ChatListView.SelectedItem).Tag;
                ChatListView.Items.Insert(ChatListView.SelectedIndex + 1, ChatBox);
            }
        }
    }
}
