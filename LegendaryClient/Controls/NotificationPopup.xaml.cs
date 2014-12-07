﻿#region

using System;
using System.IO;
using System.Windows;
using System.Xml;
using jabber.protocol.client;
using LegendaryClient.Logic;
using LegendaryClient.Logic.Maps;
using LegendaryClient.Windows;

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
        private int _inviteId;
        private int _queueId;

        public NotificationPopup(ChatSubjects subject, Message message)
        {
            InitializeComponent();
            _subject = subject;
            _messageData = message;
            string name = Enum.GetName(typeof (ChatSubjects), subject);
            if (name != null)
                NotificationTypeLabel.Content =
                    Client.TitleCaseString(name.Replace("_", " "));

            //TODO: Get name from id
            ChatPlayerItem player = Client.AllPlayers[message.From.User];
            using (XmlReader reader = XmlReader.Create(new StringReader(message.Body)))
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
                    }

                    #endregion Parse Popup
                }
            }

            string uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", _profileIconId + ".png");
            ProfileImage.Source = Client.GetImage(uriSource);

            NotificationTextBox.Text = player.Username + " has invited you to a game" + Environment.NewLine
                                       + "Hosted on " + BaseMap.GetMap(_mapId).DisplayName + Environment.NewLine
                                       + "Game Type: " + Client.TitleCaseString(_gameType).Replace("_", " ") +
                                       Environment.NewLine;
        }

        public NotificationPopup(ChatSubjects subject, string message)
        {
            InitializeComponent();
            _subject = subject;
            string name = Enum.GetName(typeof (ChatSubjects), subject);
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
            Client.Message(_messageData.From.User, _messageData.Body, ChatSubjects.GAME_INVITE_REJECT);
            Visibility = Visibility.Hidden;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            switch (_subject)
            {
                case ChatSubjects.PRACTICE_GAME_INVITE:
                    Client.Message(_messageData.From.User, _messageData.Body, ChatSubjects.PRACTICE_GAME_INVITE_ACCEPT);
                    Client.PVPNet.JoinGame(_gameId);
                    Client.GameID = _gameId;
                    Client.GameName = "Joined game";
                    Client.SwitchPage(new CustomGameLobbyPage());
                    Visibility = Visibility.Hidden;
                    break;
                case ChatSubjects.GAME_INVITE:
                    Client.Message(_messageData.From.User, _messageData.Body, ChatSubjects.GAME_INVITE_ACCEPT);
                    Visibility = Visibility.Hidden;
                    break;
            }
        }
    }
}