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
        public GameInvitePopup(InvitationRequest stats)
        {
            InitializeComponent();
            //IDK WHY I'M Receiving this stuff -.-
            Client.PVPNet.OnMessageReceived += Update_OnMessageReceived;
            GameMetaData = stats.GameMetaData;
            InvitationStateAsString = stats.InvitationStateAsString;
            InvitationState = stats.InvitationState;
            this.Inviter = stats.Inviter;
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
                RenderNotificationTextBox(SummonerName + " has invited you to a game");
                RenderNotificationTextBox("");
                RenderNotificationTextBox("Mode: " + gameModeLower);
                RenderNotificationTextBox("Map: " + MapName);
                RenderNotificationTextBox("Type: " + removeAllUnder);
            }
        }

        private void RenderNotificationTextBox(string s)
        {
            NotificationTextBox.Text += s + Environment.NewLine;

        }
        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            Client.PVPNet.getLobbyStatus(InvitationId);
            LobbyStatus NewLobby = Client.PVPNet.InviteLobby;
            //NewLobby = await Client.PVPNet.getLobbyStatus(InvitationId);
            Client.SwitchPage(new TeamQueuePage(InvitationId));
            this.Visibility = Visibility.Hidden;
            
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
                //why did I do this... I don't actually know
                LobbyStatus Lobbystatus = message as LobbyStatus;
            }
        }
    }
}
