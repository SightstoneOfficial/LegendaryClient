using LegendaryClient.Controls;
using LegendaryClient.Logic;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for StatusPage.xaml
    /// </summary>
    public partial class StatusPage : Page
    {
        public StatusPage()
        {
            InitializeComponent();
            Client.StatusLabel = StatusLabel;
            //Client.StatusGrid = StatusGrid;
            Client.ChatListView = ChatListView;
            Client.ChatClient.OnMessage += ChatClient_OnMessage;
        }

        //Blink and add to notification list if messaged
        void ChatClient_OnMessage(object sender, jabber.protocol.client.Message msg)
        {
            //If is special message, don't show popup
            if (msg.Subject != null)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    ChatSubjects subject = (ChatSubjects)Enum.Parse(typeof(ChatSubjects), msg.Subject, true);

                    if ((subject == ChatSubjects.PRACTICE_GAME_INVITE ||
                        subject == ChatSubjects.GAME_INVITE) &&
                        Client.NotificationContainer.Visibility != System.Windows.Visibility.Visible)
                    {
                        NotificationButton.Content = ".";
                    }
                }));
                return;
            }

            if (Client.AllPlayers.ContainsKey(msg.From.User) && !String.IsNullOrWhiteSpace(msg.Body))
            {
                ChatPlayerItem chatItem = Client.AllPlayers[msg.From.User];
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    NotificationChatPlayer player = null;
                    foreach (NotificationChatPlayer x in ChatListView.Items)
                    {
                        if (x.PlayerName == chatItem.Username)
                        {
                            player = x;
                            break;
                        }
                    }

                    if (player == null)
                    {
                        player = new NotificationChatPlayer();
                        player.Tag = chatItem;
                        player.PlayerName = chatItem.Username;
                        player.Margin = new Thickness(1, 0, 1, 0);
                        player.PlayerLabelName.Content = chatItem.Username;
                        Client.ChatListView.Items.Add(player);
                    }

                    if (Client.ChatItem != null)
                    {
                        if ((string)Client.ChatItem.PlayerLabelName.Content != chatItem.Username)
                        {
                            player.BlinkRectangle.Visibility = System.Windows.Visibility.Visible;
                        }
                    }
                    else
                    {
                        player.BlinkRectangle.Visibility = System.Windows.Visibility.Visible;
                    }
                }));
            }
        }

        private void ChatButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Client.ChatContainer.Visibility == System.Windows.Visibility.Hidden)
            {
                Client.ChatContainer.Visibility = System.Windows.Visibility.Visible;
                Client.NotificationContainer.Visibility = System.Windows.Visibility.Hidden;
                Client.NotificationOverlayContainer.Margin = new Thickness(0, 0, 260, 50);
            }
            else
            {
                Client.ChatContainer.Visibility = System.Windows.Visibility.Hidden;
                Client.NotificationContainer.Visibility = System.Windows.Visibility.Hidden;
                Client.NotificationOverlayContainer.Margin = new Thickness(0, 0, 10, 50);
            }
        }

        private void NotificationButton_Click(object sender, RoutedEventArgs e)
        {
            if (Client.NotificationContainer.Visibility == System.Windows.Visibility.Hidden)
            {
                Client.NotificationContainer.Visibility = System.Windows.Visibility.Visible;
                Client.ChatContainer.Visibility = System.Windows.Visibility.Hidden;
                Client.NotificationOverlayContainer.Margin = new Thickness(0, 0, 260, 50);
            }
            else
            {
                Client.NotificationContainer.Visibility = System.Windows.Visibility.Hidden;
                Client.ChatContainer.Visibility = System.Windows.Visibility.Hidden;
                Client.NotificationOverlayContainer.Margin = new Thickness(0, 0, 10, 50);
            }
        }

        private void ChatListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChatListView.SelectedIndex != -1)
            {
                NotificationChatPlayer item = (NotificationChatPlayer)ChatListView.SelectedItem;
                ChatListView.SelectedIndex = -1;
                if (Client.ChatItem == null)
                {
                    Client.ChatItem = new ChatItem();
                    Client.MainGrid.Children.Add(Client.ChatItem);
                }
                else
                {
                    string CurrentName = (string)Client.ChatItem.PlayerLabelName.Content;
                    Client.MainGrid.Children.Remove(Client.ChatItem);
                    Client.ChatClient.OnMessage -= Client.ChatItem.ChatClient_OnMessage;
                    Client.ChatItem = null;
                    if (CurrentName != (string)item.PlayerLabelName.Content)
                    {
                        Client.ChatItem = new ChatItem();
                        Client.MainGrid.Children.Add(Client.ChatItem);
                    }
                    else
                    {
                        return;
                    }
                }

                item.BlinkRectangle.Visibility = System.Windows.Visibility.Hidden;

                Panel.SetZIndex(Client.ChatItem, 3);

                Client.ChatItem.PlayerLabelName.Content = item.PlayerLabelName.Content;

                Client.ChatItem.Update();

                Client.ChatItem.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                Client.ChatItem.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

                Point relativePoint = item.TransformToAncestor(Client.MainWin).Transform(new Point(0, 0));

                Client.ChatItem.Margin = new System.Windows.Thickness(relativePoint.X, 0, 0, 40);
            }
        }

        private async void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            await Client.PVPNet.QuitGame();
            //Client.PVPNet.OnMessageReceived -= ChampSelect_OnMessageReceived;
            Client.ClearPage(typeof(CustomGameLobbyPage));
            Client.ClearPage(typeof(CreateCustomGamePage));
            Client.ClearPage(typeof(StatusPage));

            Client.SwitchPage(new MainPage());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Client.LastPageContent != null)
            {
                Grid testGrid = (Grid)Client.LastPageContent;
                //Already on this page
                if (testGrid.Parent != null)
                    return;
                FakePage fakePage = new FakePage();
                fakePage.Content = Client.LastPageContent;
                Client.SwitchPage(fakePage);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}