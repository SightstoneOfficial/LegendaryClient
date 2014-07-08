using jabber.protocol.client;
using LegendaryClient.Logic;
using LegendaryClient.Logic.Maps;
using LegendaryClient.Windows;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml;

namespace LegendaryClient.Controls
{
    /// <summary>
    /// Interaction logic for NotificationPopup.xaml
    /// </summary>
    public partial class NotificationPopup : UserControl
    {
        ChatSubjects Subject;
        Message MessageData;
        int InviteId = 0;
        int ProfileIconId = 0;
        int MapId = 0;
        int QueueId = 0;
        int GameId = 0;
        string GameType;

        public NotificationPopup(ChatSubjects subject, Message Message)
        {
            InitializeComponent();
            Subject = subject;
            MessageData = Message;
            NotificationTypeLabel.Content = Client.TitleCaseString(Enum.GetName(typeof(ChatSubjects), subject).Replace("_", " "));

            //TODO: Get name from id
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
                            case "gameId":
                                reader.Read();
                                GameId = Convert.ToInt32(reader.Value);
                                break;
                        }

                        #endregion Parse Popup
                    }
                }
            }

            var uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", ProfileIconId + ".png");
            ProfileImage.Source = Client.GetImage(uriSource);

            NotificationTextBox.Text = Player.Username + " has invited you to a game" + Environment.NewLine
                                     + "Hosted on " + BaseMap.GetMap(MapId).DisplayName + Environment.NewLine
                                     + "Game Type: " + Client.TitleCaseString(GameType).Replace("_", " ") + Environment.NewLine;
        }

        public NotificationPopup(ChatSubjects subject, string Message)
        {
            InitializeComponent();
            Subject = subject;
            NotificationTypeLabel.Content = Client.TitleCaseString(Enum.GetName(typeof(ChatSubjects), subject).Replace("_", " "));
            NotificationTextBox.Text = Message;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Hidden;
        }

        private void DeclineButton_Click(object sender, RoutedEventArgs e)
        {
            Client.Message(MessageData.From.User, MessageData.Body, ChatSubjects.GAME_INVITE_REJECT);
            this.Visibility = System.Windows.Visibility.Hidden;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (Subject == ChatSubjects.PRACTICE_GAME_INVITE)
            {
                Client.Message(MessageData.From.User, MessageData.Body, ChatSubjects.PRACTICE_GAME_INVITE_ACCEPT);
                Client.PVPNet.JoinGame(GameId);

                Client.InGame = true;
                Client.GameID = GameId;
                Client.GameName = "Joined game";

                Client.SwitchPage(new CustomGameLobbyPage());
                this.Visibility = System.Windows.Visibility.Hidden;
            }
            else if (Subject == ChatSubjects.GAME_INVITE)
            {
                Client.Message(MessageData.From.User, MessageData.Body, ChatSubjects.GAME_INVITE_ACCEPT);

                //Client.SwitchPage(new TeamQueuePage(false));
                this.Visibility = System.Windows.Visibility.Hidden;
            }
        }
    }
}