using Sightstone.Logic;
using Sightstone.Logic.SQLite;
using Sightstone.Windows;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Sightstone.Logic.Riot;
using Sightstone.Logic.Riot.Platform;
using RtmpSharp.Messaging;
using Sightstone.Logic.JSON;
using Sightstone.Logic.MultiUser;

namespace Sightstone.Controls
{
    /// <summary>
    ///     Interaction logic for AcceptGameInvite.xaml
    /// </summary>
    public partial class GameInvitePopup
    {
        private string gameMetaData;

        private string gameMode,
            gameType;

        private int gameTypeConfigId;
        private string invitationID;
        private string invitationState;
        private string invitationStateAsString;
        private string inviter;
        private bool isRanked;
        private int mapId;
        private string mapName;
        private string mode;
        private int queueId;
        private string rankedTeamName;
        private string type;
        private GameDTO tempDTO;
        private static UserClient UserClient;

        public GameInvitePopup(InvitationRequest stats, UserClient userClient)
        {
            InitializeComponent();
            UserClient = userClient;
            UserClient.RiotConnection.MessageReceived += PVPNet_OnMessageReceived;

            if (!UserClient.InviteData.ContainsKey(stats.InvitationId))
            {
                LoadGamePopupData(stats);
                Unlock();
            }
            else
            {
                var info = UserClient.InviteData[stats.InvitationId];
                Client.Log("Tried to find a popup that existed but should have been blocked. ", "Error");
                if (info == null)
                    throw new NullReferenceException("Tried to find a nonexistant popup");

                PVPNet_OnMessageReceived(this, stats);

                //This should be hidden
                //Visibility = Visibility.Hidden;
            }
        }

        private void PVPNet_OnMessageReceived(object sender, object msg)
        {
            var args = msg as MessageReceivedEventArgs;
            var message = args != null ? args.Body : msg;
            if (!(message is InvitationRequest))
                return;

            if (message.GetType() == typeof(InvitationRequest))
            {
                var stats = (InvitationRequest)message;
                try
                {
                    var info = UserClient.InviteData[stats.InvitationId];
                    //Data about this popup has changed. We want to set this
                    if (!Equals(info.popup, this))
                        return;

                    switch (stats.InvitationState)
                    {
                        case "ON_HOLD":
                            NotificationTextBox.Text = string.Format("The invite from {0} is now on hold",
                                info.Inviter);
                            Lockup();
                            Visibility = Visibility.Hidden;
                            break;
                        case "TERMINATED":
                            NotificationTextBox.Text = string.Format("The invite from {0} has been terminated",
                                info.Inviter);
                            Lockup();
                            Visibility = Visibility.Hidden;
                            break;
                        case "REVOKED":
                            NotificationTextBox.Text = string.Format("The invite from {0} has timed out", info.Inviter);
                            Lockup();
                            Visibility = Visibility.Hidden;
                            break;
                        case "ACTIVE":
                            NotificationTextBox.Text = "";
                            LoadGamePopupData(stats.Inviter == null ? info.stats : stats);
                            Visibility = Visibility.Hidden;

                            RenderNotificationTextBox(inviter + " has invited you to a game");
                            RenderNotificationTextBox("");
                            RenderNotificationTextBox("Mode: " + mode);
                            RenderNotificationTextBox("Map: " + mapName);
                            RenderNotificationTextBox("Type: " + type);
                            Unlock();
                            break;
                        default:
                            NotificationTextBox.Text = string.Format("The invite from {0} is now {1}", info.Inviter,
                                Client.TitleCaseString(stats.InvitationState));
                            Lockup();
                            break;
                    }
                }
                catch (Exception)
                {
                    //We do not need this popup. it is a new one. Let it launch
                }
            }
            else if (message.GetType() == typeof (GameDTO))
            {
                tempDTO = (GameDTO)message;
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
            invitationStateAsString = stats.InvitationStateAsString;
            gameMetaData = stats.GameMetaData;
            invitationState = stats.InvitationState;
            if (stats.Inviter != null)
                inviter = stats.Inviter.SummonerName;
            else
                inviter = stats.Owner.SummonerName;
            invitationID = stats.InvitationId;
            if (invitationID != null)
                NoGame.Visibility = Visibility.Hidden;

            var m = JsonConvert.DeserializeObject<invitationRequest>(stats.GameMetaData);
            queueId = m.queueId;
            isRanked = m.isRanked;
            rankedTeamName = m.rankedTeamName;
            mapId = m.mapId;
            gameTypeConfigId = m.gameTypeConfigId;
            gameMode = m.gameMode;
            gameType = m.gameType;
            switch (mapId)
            {
                case 1:
                    mapName = "Summoners Rift";
                    break;
                case 8:
                    mapName = "The Crystal Scar";
                    break;
                case 10:
                    mapName = "The Twisted Treeline";
                    break;
                case 11:
                    mapName = "New Summoners Rift";
                    break;
                case 12:
                    mapName = "Howling Abyss";
                    break;
                default:
                    mapName = "Unknown Map";
                    break;
            }
            var gameModeLower = Client.TitleCaseString(string.Format(gameMode.ToLower()));
            var gameTypeLower = Client.TitleCaseString(string.Format(gameType.ToLower()));
            var gameTypeRemove = gameTypeLower.Replace("_game", string.Empty);
            var removeAllUnder = gameTypeRemove.Replace("_", " ");

            if (string.IsNullOrEmpty(inviter))
                inviter = "An unknown player";

            mode = gameModeLower;
            type = removeAllUnder;
            RenderNotificationTextBox(inviter + " has invited you to a game");
            RenderNotificationTextBox(string.Empty);
            RenderNotificationTextBox("Mode: " + gameModeLower);
            RenderNotificationTextBox("Map: " + mapName);
            RenderNotificationTextBox("Type: " + removeAllUnder);

            var y = new InviteInfo
            {
                stats = stats,
                popup = this,
                Inviter = inviter
            };

            UserClient.InviteData.Add(stats.InvitationId, y);
        }

        private async void Accept_Click(object sender, RoutedEventArgs e)
        {
            if (gameType == "PRACTICE_GAME")
            {
#pragma warning disable 4014
                UserClient.calls.AcceptInvite(invitationID);
                Client.SwitchPage(new CustomGameLobbyPage(tempDTO));
            }
            //goddammit teambuilder
            else if (gameType == "NORMAL_GAME" && queueId != 61)
            {
                Client.SwitchPage(new TeamQueuePage(invitationID));
            }
            else if (gameType == "NORMAL_GAME" && queueId == 61)
            {
                Client.SwitchPage(new TeamBuilderPage(false, await UserClient.calls.AcceptInvite(invitationID)));
            }
            else if (gameType == "RANKED_GAME")
            {
                Client.SwitchPage(new TeamQueuePage(invitationID));
            }
            else if (gameType == "RANKED_TEAM_GAME")
            {
                Client.SwitchPage(new TeamQueuePage(invitationID));
            }
            Visibility = Visibility.Hidden;
            UserClient.InviteData.Remove(invitationID);
        }

        private void Decline_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() => { Visibility = Visibility.Hidden; }));
#pragma warning disable 4014
            UserClient.calls.DeclineInvite(invitationID);
            UserClient.InviteData.Remove(invitationID);
        }

        private void Hide_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
            var x = UserClient.InviteData[invitationID];
            x.PopupVisible = false;
        }
    }
}