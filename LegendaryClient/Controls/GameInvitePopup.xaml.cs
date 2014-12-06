#region

using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Windows;
using Newtonsoft.Json;
using PVPNetConnect.RiotObjects.Gameinvite.Contract;
using PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract;

#endregion

namespace LegendaryClient.Controls
{
    /// <summary>
    ///     Interaction logic for AcceptGameInvite.xaml
    /// </summary>
    public partial class GameInvitePopup
    {
        private string _gameMetaData;

        private string _gameMode,
            _gameType;

        private int _gameTypeConfigId;

        private string _invitationId;

        private string _invitationState;
        private string _invitationStateAsString;

        private string _inviter;

        private bool _isRanked;
        private int _mapId;
        private string _mapName;
        private string _mode;
        private int _queueId;
        private string _rankedTeamName;
        private string _type;

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
                PVPNet_OnMessageReceived(this, stats);

                //This should be hidden
                Visibility = Visibility.Hidden;
            }
            catch
            {
                LoadGamePopupData(stats);
                Unlock();
            }
        }

        private void PVPNet_OnMessageReceived(object sender, object message)
        {
            if (!(message is InvitationRequest))
                return;

            var stats = (InvitationRequest) message;
            try
            {
                InviteInfo info = Client.InviteData[stats.InvitationId];
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

                        RenderNotificationTextBox(_inviter + " has invited you to a game");
                        RenderNotificationTextBox("");
                        RenderNotificationTextBox("Mode: " + _mode);
                        RenderNotificationTextBox("Map: " + _mapName);
                        RenderNotificationTextBox("Type: " + _type);
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
            _invitationStateAsString = stats.InvitationStateAsString;
            _gameMetaData = stats.GameMetaData;
            _invitationState = stats.InvitationState;
            _inviter = stats.Inviter.SummonerName;
            _invitationId = stats.InvitationId;

            if (_invitationId != null)
            {
                NoGame.Visibility = Visibility.Hidden;
            }
            var m = JsonConvert.DeserializeObject<invitationRequest>(stats.GameMetaData);
            _queueId = m.queueId;
            _isRanked = m.isRanked;
            _rankedTeamName = m.rankedTeamName;
            _mapId = m.mapId;
            _gameTypeConfigId = m.gameTypeConfigId;
            _gameMode = m.gameMode;
            _gameType = m.gameType;

            Client.PVPNet.getLobbyStatusInviteId = _invitationId;
            switch (_mapId)
            {
                case 1:
                    _mapName = "Summoners Rift";
                    break;
                case 8:
                    _mapName = "The Crystal Scar";
                    break;
                case 10:
                    _mapName = "The Twisted Treeline";
                    break;
                case 11:
                    _mapName = "New Summoners Rift";
                    break;
                case 12:
                    _mapName = "Howling Abyss";
                    break;
                default:
                    _mapName = "Unknown Map";
                    break;
            }
            string gameModeLower = Client.TitleCaseString(string.Format(_gameMode.ToLower()));
            string gameTypeLower = Client.TitleCaseString(string.Format(_gameType.ToLower()));
            string gameTypeRemove = gameTypeLower.Replace("_game", "");
            string removeAllUnder = gameTypeRemove.Replace("_", " ");

            if (String.IsNullOrEmpty(_inviter))
                _inviter = "An unknown player";

            _mode = gameModeLower;
            _type = removeAllUnder;
            RenderNotificationTextBox(_inviter + " has invited you to a game");
            RenderNotificationTextBox("");
            RenderNotificationTextBox("Mode: " + gameModeLower);
            RenderNotificationTextBox("Map: " + _mapName);
            RenderNotificationTextBox("Type: " + removeAllUnder);

            var y = new InviteInfo
            {
                stats = stats,
                popup = this,
                Inviter = _inviter
            };

            Client.InviteData.Add(stats.InvitationId, y);
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            if (_gameType == "PRACTICE_GAME")
            {
                Client.SwitchPage(new CustomGameLobbyPage());
            }
                //goddammit teambuilder
            else if (_gameType == "NORMAL_GAME" && _queueId != 61)
            {
                Client.SwitchPage(new TeamQueuePage(_invitationId));
            }
            else if (_gameType == "NORMAL_GAME" && _queueId == 61)
            {
                LobbyStatus newLobby = Client.PVPNet.InviteLobby;
                Client.SwitchPage(new TeamBuilderPage(false, newLobby));
            }
            else if (_gameType == "RANKED_GAME")
            {
                Client.SwitchPage(new TeamQueuePage(_invitationId));
            }
            Visibility = Visibility.Hidden;
            Client.InviteData.Remove(_invitationId);
        }

        private void Decline_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() => { Visibility = Visibility.Hidden; }));
            Client.PVPNet.Decline(_invitationId);
            Client.InviteData.Remove(_invitationId);
        }

        private void Hide_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
            InviteInfo x = Client.InviteData[_invitationId];
            x.PopupVisible = false;
        }
    }
}