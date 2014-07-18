using LegendaryClient.Logic;
using LegendaryClient.Windows;
using PVPNetConnect.RiotObjects.Gameinvite.Contract;
using LegendaryClient.Logic.SQLite;
using System.Windows;
using System.Windows.Controls;
using System;
using PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract;
using Newtonsoft.Json;

namespace LegendaryClient.Controls
{
    /// <summary>
    /// Interaction logic for AcceptGameInvite.xaml
    /// </summary>
    public partial class GameInvitePopup : UserControl
    {
        string GameMetaData;
        string InvitationStateAsString;
        string InvitationState;
        string InvitationId;
        string SummonerName;
        string Inviter;

        int queueId;
        bool isRanked;
        string rankedTeamName;
        int mapId;
        int gameTypeConfigId;
        string gameMode;
        string gameType;
        public GameInvitePopup(InvitationRequest stats, Inviter Inviter)
        {
            InitializeComponent();
            Client.PVPNet.OnMessageReceived += Update_OnMessageReceived;
            GameMetaData = stats.GameMetaData;
            InvitationStateAsString = stats.InvitationStateAsString;
            InvitationState = stats.InvitationState;
            InvitationId = stats.InvitationId;

            if (InvitationId != null)
            {
                NoGame.Visibility = Visibility.Hidden;
            }
            //foreach (Inviter Invite in stats.Inviter)
            //{
            //    SummonerName = Invite.SummonerName;
            //}
            invitationRequest m = JsonConvert.DeserializeObject<invitationRequest>(stats.GameMetaData);
            queueId = m.queueId;
            isRanked = m.isRanked;
            rankedTeamName = m.rankedTeamName;
            mapId = m.mapId;
            gameTypeConfigId = m.gameTypeConfigId;
            gameMode = m.gameMode;
            gameType = m.gameType;

            //gameMode = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(gameMode.toLower());

            string MapName;

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
            else
            {
                MapName = "Unknown Map";
            }

            var gameModeLower = string.Format(gameMode.ToLower());
            var gameTypeLower = string.Format(gameType.ToLower());
            var gameTypeRemove = gameTypeLower.Replace("_game", "");

            if (Inviter == null)
            {
                RenderNotificationTextBox("An unknown player has invited you to a game");
                RenderNotificationTextBox("");
                RenderNotificationTextBox("Mode: " + gameModeLower);
                RenderNotificationTextBox("Map: " + MapName);
                RenderNotificationTextBox("Type: " + MapName);
            }
            else if (Inviter != null)
            {
                RenderNotificationTextBox(SummonerName + " has invited you to a game");
                RenderNotificationTextBox("");
                RenderNotificationTextBox("Mode: " + gameModeLower);
                RenderNotificationTextBox("Map: " + MapName);
                RenderNotificationTextBox("Type: " + MapName);
            }
        }

        private void RenderNotificationTextBox(string s)
        {
            NotificationTextBox.Text += s + Environment.NewLine;
        }
        private async void Accept_Click(object sender, RoutedEventArgs e)
        {
            if(InvitationId != null)
            {
                this.Visibility = Visibility.Hidden;
                //Client.SwitchPage(new TeamQueuePage(false));
                
                Client.SwitchPage(new TeamQueuePage(InvitationId));
            }
            if(InvitationId == null)
            {
                MessageOverlay overlay = new MessageOverlay();
                overlay.MessageTextBox.Text = "No Invitation Id was received. Report to Eddy5641 [https://github.com/Eddy5641/LegendaryClient/issues/new]";
                overlay.MessageTitle.Content = "No Invite Id";
                Client.OverlayContainer.Content = overlay.Content;
                Client.OverlayContainer.Visibility = Visibility.Visible;
                return;
            }
            
        }
        private void Decline_Click(object sender, RoutedEventArgs e)
        {
            InvitationRequest Request = new InvitationRequest();
            this.Visibility = Visibility.Hidden;
            Client.PVPNet.Decline(InvitationId);
        }
        private void Hide_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }
        private void Update_OnMessageReceived(object sender, object message)
        {
            if (message.GetType() == typeof(LobbyStatus))
            {
                LobbyStatus Lobbystatus = message as LobbyStatus;
            }
        }
    }
}
