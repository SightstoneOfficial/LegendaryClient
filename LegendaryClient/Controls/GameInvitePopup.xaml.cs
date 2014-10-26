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
        string GameMetaData, InvitationStateAsString, InvitationState, InvitationId, Inviter, rankedTeamName, gameMode, gameType, MapName, mode, type;
        int queueId, mapId, gameTypeConfigId;
        bool isRanked;

        public GameInvitePopup(InvitationRequest stats)
        {
            InitializeComponent();
            Client.PVPNet.OnMessageReceived += PVPNet_OnMessageReceived;
            try
            {
                InviteInfo info = Client.InviteData[stats.InvitationId];
                Client.Log("Tried to find a popup that existed but should have been blocked. ", "Error");
                if (info == null)
                    throw new NullReferenceException("Tried to find a nonexistant popup");
                else
                    PVPNet_OnMessageReceived(this, stats);

                //This should be hidden
                this.Visibility = Visibility.Hidden;
            }
            catch
            {
                LoadGamePopupData(stats);
                Unlock();
            }
        }

        void PVPNet_OnMessageReceived(object sender, object message)
        {
            if (message is InvitationRequest)
            {
                InvitationRequest stats = (InvitationRequest)message;
                try
                {
                    InviteInfo info = Client.InviteData[stats.InvitationId];
                    //Data about this popup has changed. We want to set this
                    if (info.popup == this)
                    {
                        switch (stats.InvitationState)
                        {
                            case "ON_HOLD":
                                this.NotificationTextBox.Text = string.Format("The invite from {0} is now on hold", info.Inviter);
                                this.Lockup();
                                this.Visibility = Visibility.Hidden;
                                break;
                            case "TERMINATED":
                                this.NotificationTextBox.Text = string.Format("The invite from {0} has been terminated", info.Inviter);
                                this.Lockup();
                                this.Visibility = Visibility.Hidden;
                                break;
                            case "REVOKED":
                                this.NotificationTextBox.Text = string.Format("The invite from {0} has timed out");
                                this.Lockup();
                                this.Visibility = Visibility.Hidden;
                                break;
                            case "ACTIVE":
                                this.NotificationTextBox.Text = "";
                                if (stats.Inviter == null)
                                    LoadGamePopupData(info.stats);
                                else
                                    LoadGamePopupData(stats);
                                this.Visibility = Visibility.Hidden;

                                RenderNotificationTextBox(this.Inviter + " has invited you to a game");
                                RenderNotificationTextBox("");
                                RenderNotificationTextBox("Mode: " + this.mode);
                                RenderNotificationTextBox("Map: " + this.MapName);
                                RenderNotificationTextBox("Type: " + this.type);
                                this.Unlock();
                                break;
                            default:
                                this.NotificationTextBox.Text = string.Format("The invite from {0} is now {1}", info.Inviter, Client.TitleCaseString(stats.InvitationState));
                                Lockup();
                                break;
                        }
                    }
                }
                catch
                {
                    //We do not need this popup. it is a new one. Let it launch
                    return;
                }
            }
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
            invitationRequest m = JsonConvert.DeserializeObject<invitationRequest>(stats.GameMetaData);
            queueId = m.queueId;
            isRanked = m.isRanked;
            rankedTeamName = m.rankedTeamName;
            mapId = m.mapId;
            gameTypeConfigId = m.gameTypeConfigId;
            gameMode = m.gameMode;
            gameType = m.gameType;

            Client.PVPNet.getLobbyStatusInviteId = InvitationId;
            switch(mapId)
            {
                case 1:
                    MapName = "Summoners Rift";
                    break;
                case 8:
                    MapName = "The Crystal Scar";
                    break;
                case 10:
                    MapName = "The Twisted Treeline";
                    break;
                case 12:
                    MapName = "Howling Abyss";
                    break;
                default:
                    MapName = "Unknown Map";
                    break;
            }
            var gameModeLower = Client.TitleCaseString(string.Format(gameMode.ToLower()));
            var gameTypeLower = Client.TitleCaseString(string.Format(gameType.ToLower()));
            var gameTypeRemove = gameTypeLower.Replace("_game", "");
            var removeAllUnder = gameTypeRemove.Replace("_", " ");

            if (String.IsNullOrEmpty(Inviter))
                Inviter = "An unknown player";

            mode = gameModeLower;
            type = removeAllUnder;
            RenderNotificationTextBox(Inviter + " has invited you to a game");
            RenderNotificationTextBox("");
            RenderNotificationTextBox("Mode: " + gameModeLower);
            RenderNotificationTextBox("Map: " + MapName);
            RenderNotificationTextBox("Type: " + removeAllUnder);

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
                Client.SwitchPage(new TeamBuilderPage(false, NewLobby));
            }
            else if (gameType == "RANKED_GAME")
            {
                LobbyStatus NewLobby = Client.PVPNet.InviteLobby;
                Client.SwitchPage(new TeamQueuePage(InvitationId));
            }
            this.Visibility = Visibility.Hidden;
            Client.InviteData.Remove(InvitationId);
            
        }
        private void Decline_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                this.Visibility = Visibility.Hidden;
            }));
            Client.PVPNet.Decline(InvitationId);
            Client.InviteData.Remove(InvitationId);
        }
        private void Hide_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            InviteInfo x = Client.InviteData[InvitationId];
            x.PopupVisible = false;
        }
    }
}