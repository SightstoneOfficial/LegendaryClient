using jabber.protocol.client;
using LegendaryClient.Logic;
using LegendaryClient.Logic.Maps;
using PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract;
using PVPNetConnect;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Xml;
using LegendaryClient.Windows;

namespace LegendaryClient.Controls
{
    /// <summary>
    /// Interaction logic for NotificationPopup.xaml
    /// </summary>
    public partial class NotificationPopup : UserControl
    {
        

        public NotificationPopup()
        {
            InitializeComponent();
            /*
            NotificationTextBox.Text = Player.SummonerName + " has invited you to a game" + Environment.NewLine
                                     + "Hosted on " + BaseMap.GetMap(MapId).DisplayName + Environment.NewLine
                                     + "Game Type: " + Client.TitleCaseString(GameType).Replace("_", " ") + Environment.NewLine;*/
        }
        
        private async void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            LobbyStatus Status = this.Tag as LobbyStatus;
            await Client.PVPNet.InvitationRequest(Status.InvitationID);
            Client.SwitchPage(new TeamQueuePage());
        }

        private void DeclineButton_Click(object sender, RoutedEventArgs e)
        {
            //await Client.PVPNet.InvitationRequest(Status.InvitationID);
            this.Visibility = System.Windows.Visibility.Hidden;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Client.MainGrid.Children.Remove(Client.ChatItem);
            //Client.ChatListView.Items.Remove(tempPlayer);
        }
        /*
        
        ChatSubjects Subject;
        Message MessageData;
        int InviteId = 0;
        int ProfileIconId = 0;
        int MapId = 0;
        int QueueId = 0;
        string GameType;

        public NotificationPopup(ChatSubjects subject, Message Message)
        {
            InitializeComponent();
            Subject = subject;
            MessageData = Message;
            NotificationTypeLabel.Content = Client.TitleCaseString(Enum.GetName(typeof(ChatSubjects), subject).Replace("_", " "));

            ChatPlayerItem Player = Client.AllPlayers[Message.From.User];
            using (XmlReader reader = XmlReader.Create(new StringReader(Message.Body)))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        #region Parse Popup

                        switch (reader.Name)
                        {
                            case "inviteId":
                                reader.Read();
                                InviteId = Convert.ToInt32(reader.Value);
                                break;
                            case "profileIconId":
                                reader.Read();
                                ProfileIconId = Convert.ToInt32(reader.Value);
                                break;
                            case "gameType":
                                reader.Read();
                                GameType = reader.Value;
                                break;
                            case "mapId":
                                reader.Read();
                                MapId = Convert.ToInt32(reader.Value);
                                break;
                            case "queueId":
                                reader.Read();
                                QueueId = Convert.ToInt32(reader.Value);
                                break;
                        }

                        #endregion Parse Popup
                    }
                }
            }

            var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", ProfileIconId + ".png"), UriKind.RelativeOrAbsolute);
            ProfileImage.Source = new BitmapImage(uriSource);

            NotificationTextBox.Text = Player.Username + " has invited you to a game" + Environment.NewLine
                                     + "Hosted on " + BaseMap.GetMap(MapId).DisplayName + Environment.NewLine
                                     + "Game Type: " + Client.TitleCaseString(GameType).Replace("_", " ") + Environment.NewLine;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            
            Client.MainGrid.Children.Remove(Client.ChatItem);
            //Client.ChatListView.Items.Remove(tempPlayer);
        }*/
    }
}
