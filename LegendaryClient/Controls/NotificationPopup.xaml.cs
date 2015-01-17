#region

using System;
using System.IO;
using System.Windows;
using System.Xml;
using jabber.protocol.client;
using LegendaryClient.Logic;
using LegendaryClient.Logic.Maps;
using LegendaryClient.Windows;
using PVPNetConnect.RiotObjects.Team;

#endregion

namespace LegendaryClient.Controls
{
    /// <summary>
    ///     Interaction logic for NotificationPopup.xaml
    /// </summary>
    public partial class NotificationPopup
    {
        private readonly int _gameId;
        private readonly string _gameType;
        private readonly int _mapId;
        private readonly Message _messageData;
        private readonly int _profileIconId;
        private readonly ChatSubjects _subject;
        private readonly string _teamName;
        private readonly string _teamId;
        private readonly string _msgType;
        private int _inviteId;
        private int _queueId;

        public NotificationPopup(ChatSubjects subject, Message message)
        {
            InitializeComponent();
            _subject = subject;
            _messageData = message;
            var name = Enum.GetName(typeof(ChatSubjects), subject);
            if (name != null)
                NotificationTypeLabel.Content =
                    Client.TitleCaseString(name.Replace("_", " "));

            //TODO: Get name from id
            if (!Client.AllPlayers.ContainsKey(message.From.User))
                return;

            var player = Client.AllPlayers[message.From.User];
            using (var reader = XmlReader.Create(new StringReader(message.Body)))
            {
                while (reader.Read())
                {
                    if (!reader.IsStartElement())
                        continue;

                    #region Parse Popup

                    switch (reader.Name)
                    {
                        case "inviteId":
                            reader.Read();
                            _inviteId = Convert.ToInt32(reader.Value);
                            break;
                        case "profileIconId":
                            reader.Read();
                            _profileIconId = Convert.ToInt32(reader.Value);
                            break;
                        case "gameType":
                            reader.Read();
                            _gameType = reader.Value;
                            break;
                        case "mapId":
                            reader.Read();
                            _mapId = Convert.ToInt32(reader.Value);
                            break;
                        case "queueId":
                            reader.Read();
                            _queueId = Convert.ToInt32(reader.Value);
                            break;
                        case "gameId":
                            reader.Read();
                            _gameId = Convert.ToInt32(reader.Value);
                            break;
                        case "teamName":
                            reader.Read();
                            _teamName = reader.Value;
                            break;
                        case "teamId":
                            reader.Read();
                            _teamId = reader.Value;
                            break;
                        case "msgType":
                            reader.Read();
                            _msgType = reader.Value;
                            break;
                    }

                    #endregion Parse Popup
                }
            }

            var uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", _profileIconId + ".png");
            ProfileImage.Source = Client.GetImage(uriSource);

            if (name == "RANKED_TEAM_UPDATE")
            {
                switch (_msgType)
                {
                    case "invited":
                        NotificationTextBox.Text = player.Username + " has invited you to a Ranked Team" + Environment.NewLine + "Team Name: " + _teamName + Environment.NewLine;
                        break;
                    case "kicked":
                        NotificationTextBox.Text = "You have been kicked from a ranked team " + _teamName + ".";
                        break;
                    default:
                        NotificationTextBox.Text = "Unhandled msgType in NotificationPopup: " + _msgType + Environment.NewLine + "Please create an issue on github.";
                        break;
                }
            }
            else
            {
                NotificationTextBox.Text = player.Username + " has invited you to a game" + Environment.NewLine
                                           + "Hosted on " + BaseMap.GetMap(_mapId).DisplayName + Environment.NewLine
                                           + "Game Type: " + Client.TitleCaseString(_gameType).Replace("_", " ") +
                                           Environment.NewLine;
            }

            if (!string.IsNullOrWhiteSpace(_msgType) && _msgType != "invited")
            {
                AcceptButton.Visibility = Visibility.Hidden;
                DeclineButton.Visibility = Visibility.Hidden;
            }
        }

        public NotificationPopup(ChatSubjects subject, string message)
        {
            InitializeComponent();
            _subject = subject;
            var name = Enum.GetName(typeof(ChatSubjects), subject);
            if (name != null)
                NotificationTypeLabel.Content =
                    Client.TitleCaseString(name.Replace("_", " "));

            NotificationTextBox.Text = message;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
        }

        private void DeclineButton_Click(object sender, RoutedEventArgs e)
        {
            switch (_subject)
            {
                case ChatSubjects.GAME_INVITE:
                case ChatSubjects.PRACTICE_GAME_INVITE:
                    Client.Message(_messageData.From.User, _messageData.Body, ChatSubjects.GAME_INVITE_REJECT);
                    break;
                case ChatSubjects.RANKED_TEAM_UPDATE:
                    Client.PVPNet.DeclineTeamInvite(new TeamId { FullId = _teamId });
                    break;
            }
            Visibility = Visibility.Hidden;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            switch (_subject)
            {
                case ChatSubjects.PRACTICE_GAME_INVITE:
                    Client.Message(_messageData.From.User, _messageData.Body, ChatSubjects.PRACTICE_GAME_INVITE_ACCEPT);
#pragma warning disable 4014
                    Client.PVPNet.JoinGame(_gameId);
#pragma warning restore 4014
                    Client.GameID = _gameId;
                    Client.GameName = "Joined game";
                    Client.SwitchPage(new CustomGameLobbyPage());
                    Visibility = Visibility.Hidden;
                    break;
                case ChatSubjects.GAME_INVITE:
                    Client.Message(_messageData.From.User, _messageData.Body, ChatSubjects.GAME_INVITE_ACCEPT);
                    Visibility = Visibility.Hidden;
                    break;
                case ChatSubjects.RANKED_TEAM_UPDATE:
                    Client.PVPNet.AcceptTeamInvite(new TeamId { FullId = _teamId });
                    Visibility = Visibility.Hidden;
                    break;
            }
        }
    }
}