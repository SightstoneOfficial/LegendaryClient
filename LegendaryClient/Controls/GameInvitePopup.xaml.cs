using LegendaryClient.Logic;
using LegendaryClient.Windows;
using PVPNetConnect.RiotObjects.Gameinvite.Contract;
using LegendaryClient.Logic.SQLite;
using System.Windows;
using System.Windows.Controls;
using System;
using PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract;
using Newtonsoft.Json;
using System.Globalization;
using System.Threading;
using System.Windows.Threading;
using System.Collections.Generic;

namespace LegendaryClient.Controls
{
    /// <summary>
    /// Interaction logic for AcceptGameInvite.xaml
    /// </summary>
    public partial class GameInvitePopup : UserControl
    {
        string GameMetaData, InvitationStateAsString, InvitationState, InvitationId, Inviter, rankedTeamName, gameMode, gameType;
        int queueId, mapId, gameTypeConfigId;
        bool isRanked;

        public GameInvitePopup(InvitationRequest stats)
        {
            InitializeComponent();
            try
            {
                InviteInfo info = Client.InviteData[stats.InvitationId];
                if (stats.InvitationState == "ON_HOLD")
                {
                    info.popup.NotificationTextBox.Text = string.Format("The invite from {0} is now on hold", info.Inviter);
                    Lockup();
                }
                else if (stats.InvitationState == "TERMINATED")
                {
                    info.popup.NotificationTextBox.Text = string.Format("The invite from {0} has been terminated", info.Inviter);
                    Lockup();
                }
                else if (stats.InvitationState == "REVOKED")
                {
                    info.popup.NotificationTextBox.Text = string.Format("The invite from {0} has timed out");
                    Lockup();
                }
                else if (stats.InvitationState == "ACTIVE")
                {
                    info.popup.NotificationTextBox.Text = "";
                    if (stats.Inviter == null)
                        LoadGamePopupData(info.stats);
                    else
                        LoadGamePopupData(stats);
                    Unlock();
                }
                else
                {
                    info.popup.NotificationTextBox.Text = string.Format("The invite from {0} is now {1}", info.Inviter, Client.TitleCaseString(stats.InvitationState));
                    Lockup();
                }
            }
            catch
            {
                LoadGamePopupData(stats);
                Unlock();
            }
            //IDK WHY I'M Receiving this stuff -.-
            

        }
        private void Lockup()
        {
            AcceptButton.Visibility = Visibility.Hidden;
            DeclineButton.Visibility = Visibility.Hidden;
            OkayButton.Visibility = Visibility.Visible;
        }
        private void Unlock()
        {
            AcceptButton.Visibility = Visibility.Visible;
            DeclineButton.Visibility = Visibility.Visible;
            OkayButton.Visibility = Visibility.Hidden;
        }

        private void RenderNotificationTextBox(string s)
        {
            NotificationTextBox.Text += s + Environment.NewLine;
        }

        private void LoadGamePopupData(InvitationRequest stats)
        {
            InvitationStateAsString = stats.InvitationStateAsString;
            GameMetaData = stats.GameMetaData;
            InvitationState = stats.InvitationState;
            Inviter = stats.Inviter.SummonerName;
            InvitationId = stats.InvitationId;

            if (InvitationId != null)
            {
                NoGame.Visibility = Visibility.Hidden;
            }

            //Get who the Inviter's Name


            //Simple way to get lobby data with Json.Net
            invitationRequest m = JsonConvert.DeserializeObject<invitationRequest>(stats.GameMetaData);
            queueId = m.queueId;
            isRanked = m.isRanked;
            rankedTeamName = m.rankedTeamName;
            mapId = m.mapId;
            gameTypeConfigId = m.gameTypeConfigId;
            gameMode = m.gameMode;
            gameType = m.gameType;

            Client.PVPNet.getLobbyStatusInviteId = InvitationId;

            //So if there is a new map, it won't get a null error
            string MapName = "Unknown Map";


            if (mapId == 1)
            {
                MapName = "Summoners Rift";
            }
            else if (mapId == 10)
            {
                MapName = "The Twisted Treeline";
            }
            else if (mapId == 12)
            {
                MapName = "Howling Abyss";
            }
            else if (mapId == 8)
            {
                MapName = "The Crystal Scar";
            }


            //This is used so we can call the ToTitleCase [first letter is capital rest are not]
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            TextInfo textInfo = cultureInfo.TextInfo;
            var gameModeLower = textInfo.ToTitleCase(string.Format(gameMode.ToLower()));
            var gameTypeLower = textInfo.ToTitleCase(string.Format(gameType.ToLower()));
            //Why do I have to do this Riot?
            var gameTypeRemove = gameTypeLower.Replace("_game", "");
            var removeAllUnder = gameTypeRemove.Replace("_", " ");

            if (Inviter == null)
            {
                RenderNotificationTextBox("An unknown player has invited you to a game");
                RenderNotificationTextBox("");
                RenderNotificationTextBox("Mode: " + gameModeLower);
                RenderNotificationTextBox("Map: " + MapName);
                RenderNotificationTextBox("Type: " + removeAllUnder);
            }
            else if (Inviter == "")
            {
                RenderNotificationTextBox("An unknown player has invited you to a game");
                RenderNotificationTextBox("");
                RenderNotificationTextBox("Mode: " + gameModeLower);
                RenderNotificationTextBox("Map: " + MapName);
                RenderNotificationTextBox("Type: " + removeAllUnder);
            }
            else if (Inviter != null && Inviter != "")
            {
                RenderNotificationTextBox(Inviter + " has invited you to a game");
                RenderNotificationTextBox("");
                RenderNotificationTextBox("Mode: " + gameModeLower);
                RenderNotificationTextBox("Map: " + MapName);
                RenderNotificationTextBox("Type: " + removeAllUnder);
            }

            InviteInfo y = new InviteInfo();
            y.stats = stats;
            y.popup = this;
            y.Inviter = Inviter;

            Client.InviteData.Add(stats.InvitationId, y);
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            if (gameType == "PRACTICE_GAME")
            {
                Client.SwitchPage(new CustomGameLobbyPage());
            }
            //goddammit teambuilder
            else if (gameType == "NORMAL_GAME" && queueId != 61)
            {
                LobbyStatus NewLobby = Client.PVPNet.InviteLobby;
                Client.SwitchPage(new TeamQueuePage(InvitationId));
            }
            else if (gameType == "NORMAL_GAME" && queueId == 61)
            {
                LobbyStatus NewLobby = Client.PVPNet.InviteLobby;
                Client.SwitchPage(new TeamBuilderPage(false));
            }
            else if (gameType == "RANKED_GAME")
            {
                LobbyStatus NewLobby = Client.PVPNet.InviteLobby;
                //NewLobby = await Client.PVPNet.getLobbyStatus(InvitationId);
                Client.SwitchPage(new TeamQueuePage(InvitationId));
            }
            this.Visibility = Visibility.Hidden;
            
        }
        private void Decline_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                this.Visibility = Visibility.Hidden;
            }));
            Client.PVPNet.Decline(InvitationId);
        }
        private void Hide_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            InviteInfo x = Client.InviteData[InvitationId];
            x.PopupVisible = false;
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            Client.PVPNet.Decline(InvitationId);
            Client.InviteData.Remove(InvitationId);
        }
    }
}