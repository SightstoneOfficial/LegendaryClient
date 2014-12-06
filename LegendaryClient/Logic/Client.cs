using System.Linq;

#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using jabber;
using jabber.client;
using jabber.connection;
using jabber.protocol.client;
using jabber.protocol.iq;
using LCLog;
using LegendaryClient.Controls;
using LegendaryClient.Logic.Region;
using LegendaryClient.Logic.Replays;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Properties;
using LegendaryClient.Windows;
using MahApps.Metro;
using PVPNetConnect;
using PVPNetConnect.RiotObjects.Gameinvite.Contract;
using PVPNetConnect.RiotObjects.Platform.Catalog.Champion;
using PVPNetConnect.RiotObjects.Platform.Clientfacade.Domain;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Game.Message;
using PVPNetConnect.RiotObjects.Platform.Login;
using PVPNetConnect.RiotObjects.Platform.Messaging;
using SQLite;
using Brush = System.Windows.Media.Brush;
using Button = System.Windows.Controls.Button;
using EndOfGameStats = PVPNetConnect.RiotObjects.Platform.Statistics.EndOfGameStats;
using Error = PVPNetConnect.Error;
using Image = System.Windows.Controls.Image;
using InvitationRequest = LegendaryClient.Logic.SQLite.InvitationRequest;
using Items = LegendaryClient.Logic.SQLite.Items;
using Label = System.Windows.Controls.Label;
using ListView = System.Windows.Controls.ListView;
using Message = jabber.protocol.client.Message;
using Timer = System.Windows.Forms.Timer;

#endregion

//using LegendaryClient.Logic.AutoReplayRecorder;

namespace LegendaryClient.Logic
{
    /// <summary>
    ///     Any logic that needs to be reused over multiple pages
    /// </summary>
    internal static class Client
    {
        /// <summary>
        ///     This is all accounts that have been added to LegendaryClient
        ///     Use this for multiaccount in futuree
        /// </summary>
        internal static Dictionary<String, LoginDataPacket> Accountslist = new Dictionary<String, LoginDataPacket>();

        internal static List<Group> Groups = new List<Group>();

        /// <summary>
        ///     Gets the value of the league of Legends Settings
        /// </summary>
        /// <returns>All of the League Of Legends Settings</returns>
        public static Dictionary<String, String> LeagueSettingsReader(this string fileLocation)
        {
            var settings = new Dictionary<String, String>();
            string[] file = File.ReadAllLines(fileLocation);
            foreach (string x in file.Where(x => !String.IsNullOrEmpty(x) && !String.IsNullOrWhiteSpace(x)).Where(x => !x.Contains("[") && !x.Contains("]")))
            {
                try
                {
                    //Spit the one value into 2 values
                    string[] value = x.Split('=');
                    settings.Add(value[0], value[1]);
                }
                catch (Exception e)
                {
                    Log(e.Message);
                }
            }
            return settings;
        }

        public static Brush Change()
        {
            string y = Settings.Default.Theme;
            var bc = new BrushConverter();
            if (y.Contains("Blue"))
                return (Brush) bc.ConvertFrom("#FF1585B5");
            if (y.Contains("Red"))
                return (Brush) bc.ConvertFrom("#FFA01414");
            if (y.Contains("Green"))
                return (Brush) bc.ConvertFrom("#FF2DA014");
            if (y.Contains("Purple"))
                return (Brush) bc.ConvertFrom("#FF5A14A0");
            return (Brush) bc.ConvertFrom("#FF141414"); //Steel
        }

        internal static Dictionary<String, PVPNetConnection> PvpNetList = new Dictionary<String, PVPNetConnection>();

        internal static async Task<LoginDataPacket> AddAccount()
        {
            return new LoginDataPacket();
        }

        internal static async Task<LoginDataPacket> AddAccount(LoginDataPacket packet)
        {
            if (packet == null)
                return new LoginDataPacket();

            Accountslist.Add(packet.AllSummonerData.Summoner.Name, packet);
            return packet;
        }

        internal static async Task<LoginDataPacket> AddAccount(string username, string password)
        {
            var pvp = new PVPNetConnection();
            var credentials = new AuthenticationCredentials
            {
                ClientVersion = Version,
                AuthToken = "",
                Password = password,
                IpAddress = ""
            };

            await pvp.Login(credentials);

            return new LoginDataPacket();
        }

        internal static bool Filter = true;

        /// <summary>
        ///     Use this to play sounds
        /// </summary>
        internal static MediaElement SoundPlayer;

        /// <summary>
        ///     Use this to play sounds in the background only
        /// </summary>
        internal static MediaElement AmbientSoundPlayer;


        /// <summary>
        ///     Timer used so replays won't start right away
        /// </summary>
        internal static Timer ReplayTimer;

        internal static bool IsOwnerOfGame = false;

        internal static double QueueId = 0;

        /// <summary>
        ///     The database of all runes
        /// </summary>
        internal static List<Runes> Runes;

        /// <summary>
        ///     Retreives UpdateDate For LegendaryClient
        /// </summary>
        /// <summary>
        ///     Stuff
        /// </summary>
        internal static string LegendaryClientVersion = "2.0.0.0";

        /// <summary>
        ///     Button For Lobby
        /// </summary>
        internal static Button ReturnButton;

        /// <summary>
        ///     Check if on Champion Select or Team Queue Lobby Page
        /// </summary>
        internal static Page CurrentPage;

        /// <summary>
        ///     Update Data
        /// </summary>
        internal static int LegendaryClientReleaseNumber = 3;

        /// <summary>
        ///     If Player is creating an account
        /// </summary>
        internal static bool Done = true;

        /// <summary>
        ///     Sets Sqlite Version
        ///     Like the language pack
        /// </summary>
        internal static string Sqlite = "gameStats_en_US.sqlite";

        //internal static string sqlite = "gameStats_ko_KR.sqlite";

        /// <summary>
        ///     Latest champion for League of Legends login screen
        /// </summary>
        internal const int LatestChamp = 103;

        /// <summary>
        ///     Latest version of League of Legends. Retrieved from ClientLibCommon.dat
        /// </summary>
        internal static string Version = "4.12.2";

        /// <summary>
        ///     To see if the user is a dev
        /// </summary>
        internal static bool Dev = false;

        /// <summary>
        ///     The current directory the client is running from
        /// </summary>
        internal static string ExecutingDirectory = "";

        /// <summary>
        ///     Riot's database with all the client data
        /// </summary>
        internal static SQLiteConnection SqliteDatabase;

        /// <summary>
        ///     Fix for champ select. Do not use this!
        /// </summary>
        internal static event PVPNetConnection.OnMessageReceivedHandler OnFixChampSelect;

        /// <summary>
        ///     Allow lobby to still have a connection. Do not use this!
        /// </summary>
        internal static event PVPNetConnection.OnMessageReceivedHandler OnFixLobby;

        /// <summary>
        ///     The database of all the champions
        /// </summary>
        internal static List<Champions> Champions;

        /// <summary>
        ///     The database of all the champion abilities
        /// </summary>
        internal static List<ChampionAbilities> ChampionAbilities;

        /// <summary>
        ///     The database of all the champion skins
        /// </summary>
        internal static List<ChampionSkins> ChampionSkins;

        /// <summary>
        ///     The database of all the items
        /// </summary>
        internal static List<Items> Items;

        /// <summary>
        ///     The database of all masteries
        /// </summary>
        internal static List<Masteries> Masteries;

        /// <summary>
        ///     The Invite Data
        /// </summary>
        internal static List<InvitationRequest> InviteJsonRequest = new List<InvitationRequest>();

        /// <summary>
        ///     All of players who have been invited
        /// </summary>
        internal static Dictionary<String, InviteInfo> InviteData = new Dictionary<String, InviteInfo>();

        /// <summary>
        ///     The database of all the search tags
        /// </summary>
        internal static List<ChampionSearchTags> SearchTags;

        /// <summary>
        ///     The database of all the keybinding defaults & proper names
        /// </summary>
        internal static List<KeybindingEvents> Keybinds;

        internal static ChampionDTO[] PlayerChampions;

        internal static ReplayRecorder Autorecorder;

        internal static List<string> Whitelist = new List<string>();

        #region Chat

        internal static JabberClient ChatClient;
        //Fix for invitations
        public delegate void OnMessageHandler(object sender, Message e);

        public static event OnMessageHandler OnMessage;

        internal static PresenceType _currentPresence;

        internal static PresenceType CurrentPresence
        {
            get { return _currentPresence; }
            set
            {
                if (_currentPresence == value)
                    return;

                _currentPresence = value;
                if (ChatClient == null)
                    return;

                if (ChatClient.IsAuthenticated)
                {
                    ChatClientConnect(null);
                }
            }
        }

        internal static string _currentStatus;

        internal static string CurrentStatus
        {
            get { return _currentStatus; }
            set
            {
                if (_currentStatus == value)
                    return;

                _currentStatus = value;
                if (ChatClient == null)
                    return;

                if (ChatClient.IsAuthenticated)
                {
                    ChatClientConnect(null);
                }
            }
        }

        internal static RosterManager RostManager;
        internal static PresenceManager PresManager;
        internal static ConferenceManager ConfManager;
        internal static bool UpdatePlayers = true;

        internal static Dictionary<string, ChatPlayerItem> AllPlayers = new Dictionary<string, ChatPlayerItem>();

        internal static bool ChatClient_OnInvalidCertificate(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        internal static void ChatClient_OnMessage(object sender, Message msg)
        {
            if (msg.Subject != null)
            {
                MainWin.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    //var subject = (ChatSubjects) Enum.Parse(typeof (ChatSubjects), msg.Subject, true);
                    //NotificationPopup pop = new NotificationPopup(subject, msg);
                    //pop.Height = 230;
                    //pop.HorizontalAlignment = HorizontalAlignment.Right;
                    //pop.VerticalAlignment = VerticalAlignment.Bottom;
                    //Client.NotificationGrid.Children.Add(pop);
                }));

                return;
            }

            if (AllPlayers.ContainsKey(msg.From.User) && !String.IsNullOrWhiteSpace(msg.Body))
            {
                ChatPlayerItem chatItem = AllPlayers[msg.From.User];
                chatItem.Messages.Add(chatItem.Username + "|" + msg.Body);
                MainWin.FlashWindow();
            }
        }

        internal static async void GameInvite(object sender, PVPNetConnection pvpConnect, string gameId)
        {
            await pvpConnect.Accept(gameId);
        }

        internal static void ChatClientConnect(object sender)
        {
            SetChatHover();
        }

        internal static void SendMessage(string user, string message)
        {
            ChatClient.Message(user, message);
        }

        internal static void SetChatHover()
        {
            ChatClient.Presence(CurrentPresence, GetPresence(), PresenceStatus, 0);
        }

        internal static bool HideLegendaryAddition;

        internal static string LegendaryClientAddition = "∟";

        internal static void NewStatus()
        {
            if (Dev == false)
            {
                LegendaryClientAddition = CurrentStatus;
            }
            else if (Dev)
            {
                LegendaryClientAddition = CurrentStatus;
            }
            else if (HideLegendaryAddition)
            {
                LegendaryClientAddition = CurrentStatus;
            }
        }

        internal static string GetPresence()
        {
            //<dev>true</dev> == lc dev
            //<dev>false</dev> == lc user

            //Queue types
            //NONE,NORMAL,NORMAL_3x3,ODIN_UNRANKED,ARAM_UNRANKED_5x5,BOT,BOT_3x3,RANKED_SOLO_5x5,RANKED_TEAM_3x3,RANKED_TEAM_5x5,
            //ONEFORALL_5x5,FIRSTBLOOD_1x1,FIRSTBLOOD_2x2,SR_6x6,CAP_5x5,URF,URF_BOT,NIGHTMARE_BOT

            //TODO: GameStatus values:
            //"teamSelect","hostingNormalGame","hostingPracticeGame","hostingRankedGame","hostingCoopVsAIGame","inQueue"
            //"spectating","outOfGame","championSelect","inGame","inTeamBuilder","tutorial"

            if (GameStatus != "busy")
            {
                switch (GameStatus)
                {
                    case "inQueue":
                    case "championSelect":
                    case "inGame":
                        PresenceStatus = "dnd";
                        break;
                    case "outOfGame":
                        PresenceStatus = "chat";
                        break;
                }
            }
            var sb = new StringBuilder();
            sb.Append("<body><profileIcon>");
            sb.Append(LoginPacket.AllSummonerData.Summoner.ProfileIconId);
            sb.Append("</profileIcon><level>");
            sb.Append(LoginPacket.AllSummonerData.SummonerLevel.Level);
            sb.Append("</level><dev>");
            sb.Append(Dev ? "true" : "false");
            sb.Append("</dev><wins>");
            sb.Append(AmountOfWins);
            sb.Append("</wins><leaves>0</leaves><odinWins>0</odinWins><odinLeaves>0</odinLeaves>"); // TODO
            sb.Append("<queueType />");
            sb.Append("<rankedLosses>0</rankedLosses><rankedRating>0</rankedRating>"); // unused for now
            if (IsRanked)
                sb.Append("<tier>UNRANKED</tier>");
            else
                sb.Append("<tier>" + TierName + "</tier>");
            sb.Append("<rankedSoloRestricted>");
            sb.Append(LoginPacket.restrictedGamesRemainingForRanked != -1);
            sb.Append("</rankedSoloRestricted>");
            if (IsRanked)
                sb.Append("<rankedLeagueName>" + LeagueName + "</rankedLeagueName><rankedLeagueDivision>" + Tier +
                          "</rankedLeagueDivision><rankedLeagueTier>" + TierName +
                          "</rankedLeagueTier><rankedLeagueQueue>RANKED_SOLO_5x5</rankedLeagueQueue><rankedWins>" +
                          AmountOfWins + "</rankedWins>");
            else
                sb.Append("<rankedLeagueName /><rankedLeagueDivision /><rankedLeagueTier /><rankedLeagueQueue />");
            sb.Append("<gameStatus>");
            sb.Append(GameStatus);
            sb.Append("</gameStatus>");
            switch (GameStatus)
            {
                case "inQueue":
                    sb.Append("<timeStamp>" + TimeStampSince + "</timeStamp>");
                    break;
                case "inGame":
                case "championSelect":
                    //TODO: Spectate-able, queue type & champ name
                    sb.Append(
                        "<gameQueueType>NONE</gameQueueType><isObservable>ALL</isObservable><skinname>Random</skinname>");
                    break;
            }
            if (CurrentStatus == "Online")
                sb.Append("<statusMsg />");
            else
                sb.Append("<statusMsg>" + CurrentStatus + "</statusMsg>");
            sb.Append("</body>");

            return sb.ToString();
        }

        internal static void RostManager_OnRosterItem(object sender, Item ri)
        {
            UpdatePlayers = true;

            if (AllPlayers.ContainsKey(ri.JID.User))
                return;

            var player = new ChatPlayerItem
            {
                Id = ri.JID.User,
                Username = ri.Nickname
            };

            PresManager.IsAvailable(ri.JID);
            AllPlayers.Add(ri.JID.User, player);
        }

        internal static ListView Chatlistview;

        internal static void PresManager_OnPrimarySessionChange(object sender, JID bare)
        {
            Presence[] s = PresManager.GetAll(bare);
            if (s.Length == 0)
                return;
            string presence = s[0].Status;
            if (presence == null)
                return;
            Debug.WriteLine(presence);
            if (!AllPlayers.ContainsKey(bare.User))
                return;

            UpdatePlayers = true;
            ChatPlayerItem player = AllPlayers[bare.User];
            using (XmlReader reader = XmlReader.Create(new StringReader(presence)))
            {
                try
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement() && !reader.IsEmptyElement)
                        {
                            #region Parse Presence

                            switch (reader.Name)
                            {
                                case "profileIcon":
                                    reader.Read();
                                    player.ProfileIcon = Convert.ToInt32(reader.Value);
                                    break;

                                case "level":
                                    reader.Read();
                                    player.Level = Convert.ToInt32(reader.Value);
                                    break;

                                case "wins":
                                    reader.Read();
                                    player.Wins = Convert.ToInt32(reader.Value);
                                    break;

                                case "leaves":
                                    reader.Read();
                                    player.Leaves = Convert.ToInt32(reader.Value);
                                    break;

                                case "rankedWins":
                                    reader.Read();
                                    player.RankedWins = Convert.ToInt32(reader.Value);
                                    break;

                                case "timeStamp":
                                    reader.Read();
                                    player.Timestamp = Convert.ToInt64(reader.Value);
                                    break;

                                case "statusMsg":
                                    reader.Read();
                                    player.Status = reader.Value;
                                    break;

                                case "dev":
                                    reader.Read();
                                    player.UsingLegendary = true;
                                    if (reader.Value == "true")
                                        player.IsLegendaryDev = true;
                                    break;

                                case "gameStatus":
                                    reader.Read();
                                    player.GameStatus = reader.Value;
                                    break;

                                case "skinname":
                                    reader.Read();
                                    player.Champion = reader.Value;
                                    break;

                                case "rankedLeagueName":
                                    reader.Read();
                                    player.LeagueName = reader.Value;
                                    break;

                                case "rankedLeagueTier":
                                    reader.Read();
                                    player.LeagueTier = reader.Value;
                                    break;

                                case "rankedLeagueDivision":
                                    reader.Read();
                                    player.LeagueDivision = reader.Value;
                                    break;
                            }

                            #endregion Parse Presence
                        }
                    }
                }
                catch (Exception e)
                {
                    Log(e.Message + " - remember to fix this later instead of avoiding the problem.");
                }
            }
            if (String.IsNullOrWhiteSpace(player.Status))
            {
                player.Status = "Online";
            }
        }

        internal static void Message(string to, string message, ChatSubjects subject)
        {
            var msg = new Message(ChatClient.Document)
            {
                Type = MessageType.normal,
                To = to + "@pvp.net",
                Subject = subject.ToString(),
                Body = message
            };

            ChatClient.Write(msg);
        }

        //Why do you even have to do this, riot?
        internal static string GetObfuscatedChatroomName(string subject, string type)
        {
            byte[] data = Encoding.UTF8.GetBytes(subject);
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] result = sha.ComputeHash(data);
            string obfuscatedName = "";
            int incrementValue = 0;
            while (incrementValue < result.Length)
            {
                int bitHack = result[incrementValue];
                obfuscatedName = obfuscatedName + Convert.ToString(((uint) (bitHack & 240) >> 4), 16);
                obfuscatedName = obfuscatedName + Convert.ToString(bitHack & 15, 16);
                incrementValue = incrementValue + 1;
            }
            obfuscatedName = Regex.Replace(obfuscatedName, @"/\s+/gx", "");
            obfuscatedName = Regex.Replace(obfuscatedName, @"/[^a-zA-Z0-9_~]/gx", "");
            return type + "~" + obfuscatedName;
        }

        internal static bool RunOnce = false;

        internal static string GetChatroomJid(string obfuscatedChatroomName, string password, bool isTypePublic)
        {
            if (!isTypePublic)
                return obfuscatedChatroomName + "@sec.pvp.net";

            if (String.IsNullOrEmpty(password))
                return obfuscatedChatroomName + "@lvl.pvp.net";

            return obfuscatedChatroomName + "@conference.pvp.net";
        }

        internal static int AmountOfWins; //Calculate wins for presence
        internal static bool IsRanked;
        internal static string TierName;
        internal static string Tier;
        internal static string LeagueName;
        internal static string GameStatus = "outOfGame";
        internal static string PresenceStatus = "chat";
        internal static double TimeStampSince = 0;

        #endregion Chat

        internal static Grid MainGrid;
        internal static Grid NotificationGrid;
        internal static Grid StatusGrid = new Grid();
        internal static Image BackgroundImage;

        internal static Label StatusLabel;
        internal static Label InfoLabel;
        internal static ContentControl OverlayContainer;
        internal static Button PlayButton;
        internal static ContentControl ChatContainer;
        internal static ContentControl StatusContainer;
        internal static ContentControl NotificationOverlayContainer;
        internal static ContentControl NotificationContainer;
        internal static ListView ChatListView;
        internal static ChatItem ChatItem;
        internal static List<GroupChatItem> GroupChatItems = new List<GroupChatItem>();
        internal static GroupChatItem CurrentGroupChatItem;

        internal static ListView InviteListView;
        internal static Image MainPageProfileImage;

        #region WPF Tab Change

        /// <summary>
        ///     The container that contains the page to display
        /// </summary>
        internal static ContentControl Container;

        /// <summary>
        ///     Page cache to stop having to recreate all information if pages are overwritted
        /// </summary>
        internal static List<Page> Pages;

        internal static bool IsOnPlayPage = false;

        /// <summary>
        ///     Switches the contents of the frame to the requested page. Also sets background on
        ///     the button on the top to show what section you are currently on.
        /// </summary>
        internal static void SwitchPage(Page page)
        {
            IsOnPlayPage = page.GetType() == typeof (PlayPage);
            BackgroundImage.Visibility = page.GetType() == typeof (ChampSelectPage) ? Visibility.Hidden : Visibility.Visible;

            TrueCurrentPage = page;

            foreach (Page p in Pages.Where(p => p.GetType() == page.GetType()))
            {
                Container.Content = p.Content;
                return;
            }
            Container.Content = page.Content;
            if (!(page is FakePage))
                Pages.Add(page);
        }

        /// <summary>
        ///     Clears the cache of a certain page if not used anymore
        /// </summary>
        internal static void ClearPage(Type pageType)
        {
            foreach (Page p in Pages.Where(p => p.GetType() == pageType))
            {
                Pages.Remove(p);
                return;
            }
        }

        internal static void ClearNotification(Type containerType)
        {
            foreach (UIElement element in NotificationGrid.Children.Cast<UIElement>().Where(element => element.GetType() == containerType))
            {
                NotificationGrid.Children.Remove(element);
                return;
            }
        }

        internal static void ClearMainGrid(Type containerType)
        {
            foreach (UIElement element in MainGrid.Children.Cast<UIElement>().Where(element => element.GetType() == containerType))
            {
                MainGrid.Children.Remove(element);
                return;
            }
        }

        #endregion WPF Tab Change

        #region League Of Legends Logic

        /// <summary>
        ///     Main connection to the League of Legends server
        /// </summary>
        internal static PVPNetConnection PvpNet;

        /// <summary>
        ///     Packet recieved when initially logged on. Cached so the packet doesn't
        ///     need to requested multiple times, causing slowdowns
        /// </summary>
        internal static LoginDataPacket LoginPacket;

        /// <summary>
        ///     All enabled game configurations for the user
        /// </summary>
        internal static List<GameTypeConfigDTO> GameConfigs;

        /// <summary>
        ///     The region the user is connecting to
        /// </summary>
        internal static BaseRegion Region;

        /// <summary>
        ///     Is the client logged in to the League of Legends server
        /// </summary>
        internal static bool IsLoggedIn = false;

        /// <summary>
        ///     gameId of the current game that the client is connected to
        /// </summary>
        internal static double GameId = 0;

        /// <summary>
        /// </summary>
        internal static int GameQueue;

        /// <summary>
        ///     Game Name of the current game that the client is connected to
        /// </summary>
        internal static string GameName = "";

        /// <summary>
        ///     The DTO of the game lobby when connected to a custom game
        /// </summary>
        internal static GameDTO GameLobbyDto;

        /// <summary>
        ///     A recorder
        /// </summary>
        internal static ReplayRecorder Recorder = null;

        /// <summary>
        ///     When going into champion select reuse the last DTO to set up data
        /// </summary>
        internal static GameDTO ChampSelectDto;

        /// <summary>
        ///     When connected to a game retrieve details to connect to
        /// </summary>
        internal static PlayerCredentialsDto CurrentGame;

        internal static Session PlayerSession;

        internal static bool AutoAcceptQueue = false;
        internal static object LobbyContent;
        internal static object LastPageContent;
        internal static bool IsInGame = false;
        internal static bool RunonePop = false;

        /// <summary>
        ///     When an error occurs while connected. Currently just logged
        /// </summary>
        internal static void PVPNet_OnError(object sender, Error error)
        {
            Log(error.Type.ToString(), "PVPNetError");
            Log(error.Message, "PVPNetError");
        }


        internal static System.Timers.Timer HeartbeatTimer;
        internal static int HeartbeatCount;


        //internal static Inviter CurrentInviter;

#pragma warning disable 4014

        internal static void OnMessageReceived(object sender, object message)
        {
            MainWin.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async () =>
            {
                var balance = message as StoreAccountBalanceNotification;
                if (balance != null)
                {
                    var newBalance = balance;
                    InfoLabel.Content = "IP: " + newBalance.Ip + " ∙ RP: " + newBalance.Rp;
                    LoginPacket.IpBalance = newBalance.Ip;
                    LoginPacket.RpBalance = newBalance.Rp;
                }
                else
                {
                    var gameNotification = message as GameNotification;
                    if (gameNotification != null)
                    {
                        var notification = gameNotification;
                        var messageOver = new MessageOverlay
                        {
                            MessageTitle = {Content = notification.Type}
                        };
                        switch (notification.Type)
                        {
                            case "PLAYER_BANNED_FROM_GAME":
                                messageOver.MessageTitle.Content = "Banned from custom game";
                                messageOver.MessageTextBox.Text = "You have been banned from this custom game!";
                                break;

                            default:
                                messageOver.MessageTextBox.Text = notification.MessageCode + Environment.NewLine;
                                messageOver.MessageTextBox.Text = Convert.ToString(notification.MessageArgument);
                                break;
                        }
                        OverlayContainer.Content = messageOver.Content;
                        OverlayContainer.Visibility = Visibility.Visible;
                        ClearPage(typeof (CustomGameLobbyPage));
                        if (messageOver.MessageTitle.Content.ToString() != "PLAYER_QUIT")
                            SwitchPage(new MainPage());
                    }
                    else if (message is EndOfGameStats)
                    {
                        var stats = message as EndOfGameStats;
                        var endOfGame = new EndOfGamePage(stats);
                        ClearPage(typeof (TeamQueuePage));
                        OverlayContainer.Visibility = Visibility.Visible;
                        OverlayContainer.Content = endOfGame.Content;
                    }
                    else if (message is StoreFulfillmentNotification)
                    {
                        PlayerChampions = await PvpNet.GetAvailableChampions();
                    }
                    else if (message is Inviter)
                    {
                        //var stats = message as Inviter;
                        //CurrentInviter = stats;
                    }
                    else if (message is PVPNetConnect.RiotObjects.Gameinvite.Contract.InvitationRequest)
                    {
                        var stats = message as PVPNetConnect.RiotObjects.Gameinvite.Contract.InvitationRequest;
                        //TypedObject body = (TypedObject)to["body"];
                        if (stats.Inviter == null)
                            return;

                        try
                        {
                            //Already existant popup. Do not create a new one
                            InviteInfo x = InviteData[stats.InvitationId];
                            if (x.Inviter != null)
                                return;
                        }
                        catch (Exception e)
                        {
                            Log(e.Message);
                        }
                        MainWin.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                        {
                            var pop = new GameInvitePopup(stats)
                            {
                                HorizontalAlignment = HorizontalAlignment.Right,
                                VerticalAlignment = VerticalAlignment.Bottom,
                                Height = 230
                            };

                            NotificationGrid.Children.Add(pop);
                        }));
                    }
                }
            }));
        }

        internal static string InternalQueueToPretty(string internalQueue)
        {
            switch (internalQueue)
            {
                case "matching-queue-NORMAL-5x5-game-queue":
                    return "Normal 5v5";

                case "matching-queue-NORMAL-3x3-game-queue":
                    return "Normal 3v3";

                case "matching-queue-NORMAL-5x5-draft-game-queue":
                    return "Draft 5v5";

                case "matching-queue-RANKED_SOLO-5x5-game-queue":
                    return "Ranked 5v5";

                case "matching-queue-RANKED_TEAM-3x3-game-queue":
                    return "Ranked Team 5v5";

                case "matching-queue-RANKED_TEAM-5x5-game-queue":
                    return "Ranked Team 3v3";

                case "matching-queue-ODIN-5x5-game-queue":
                    return "Dominion 5v5";

                case "matching-queue-ARAM-5x5-game-queue":
                    return "ARAM 5v5";

                case "matching-queue-BOT-5x5-game-queue":
                    return "Bot 5v5 Beginner";

                case "matching-queue-ODIN-5x5-draft-game-queue":
                    return "Dominion Draft 5v5";

                case "matching-queue-BOT_TT-3x3-game-queue":
                    return "Bot 3v3 Beginner";

                case "matching-queue-BOT_EASY-5x5-game-queue":
                    return "Bot 5v5 Easy";

                case "matching-queue-BOT_MEDIUM-5x5-game-queue":
                    return "Bot 5v5 Medium";

                case "matching-queue-ODINBOT-5x5-game-queue":
                    return "Dominion Bot 5v5 Beginner";

                case "matching-queue-ONEFORALL-5x5-game-queue":
                    return "One For All 5v5";

                case "matching-queue-GROUPFINDER-5x5-game-queue":
                    return "Team Builder 5v5";

                case "matching-queue-BOT_INTRO-5x5-game-queue":
                    return "Bot 5v5 Intro";

                case "matching-queue-GROUP_FINDER-5x5-game-queue":
                    return "Teambuilder 5v5 Beta (In Dev. Do Not Play)";

                case "matching-queue-NIGHTMARE_BOT_1-5x5-game-queue":
                    return "Nightmare Bots 5v5 (Easy)";

                case "matching-queue-NIGHTMARE_BOT_2-5x5-game-queue":
                    return "Nightmare Bots 5v5 (Med)";

                case "matching-queue-NIGHTMARE_BOT_3-5x5-game-queue":
                    return "Nightmare Bots 5v5 (Hard)";

                case "matching-queue-TT-6x6-game-queue":
                    return "Hexakill Twisted Treeline";

                default:
                    return internalQueue;
            }
        }

        /// <summary>
        ///     Super complex method to get the queue name when it is unknown
        /// </summary>
        /*private static string Convert(string queueName)
        {
            string queueinternal = "";
            string bots = "";
            const string players = "";
            const string extra = "";
            string start = queueName.Replace("matching-queue-", "").Replace("-game-queue", "");
            string[] x = start.Split('_');
            if (x[1].ToLower() == "bot")
            {
                bots = " Bots";
                //string[] m = x[3].Split('-');
            }
            else if (x[0].ToLower() == "bot" && x[1].ToLower() == "intro")
            {
                queueinternal = "Intro";
                bots = "Bots";
            }

            string result = string.Format("{0}{1} {2} {3}", queueinternal, bots, players, extra);

            return result;
        }*/

        internal static string GetGameDirectory()
        {
            string directory = Path.Combine(RootLocation, "RADS", "projects", "lol_game_client", "releases");

            var dInfo = new DirectoryInfo(directory);

            DirectoryInfo[] subdirs;
            try
            {
                subdirs = dInfo.GetDirectories();
            }
            catch
            {
                return "0.0.0";
            }

            string latestVersion = "0.0.1";
            foreach (DirectoryInfo info in subdirs)
            {
                latestVersion = info.Name;
            }

            directory = Path.Combine(directory, latestVersion, "deploy");

            return directory;
        }

        internal static string Location;
        internal static string LoLLauncherLocation;
        internal static string LoLClientVersion;
        internal static string RootLocation;

        internal static void LaunchGame()
        {
            string gameDirectory = Location;

            var p = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = gameDirectory,
                    FileName = Path.Combine(gameDirectory, "League of Legends.exe")
                }
            };
            p.Exited += p_Exited;
            p.StartInfo.Arguments = "\"8394\" \"" + RootLocation + "LoLLauncher.exe" + "\" \"" + "\" \"" +
                                    CurrentGame.ServerIp + " " +
                                    CurrentGame.ServerPort + " " +
                                    CurrentGame.EncryptionKey + " " +
                                    CurrentGame.SummonerId + "\"";
            p.Start();

            if (!File.Exists(Path.Combine(ExecutingDirectory, "Replays", "ReplayRecorder.exe")))
                return;

            PlayerCredentialsDto replaydata = CurrentGame;
            string observerServerIp = replaydata.ObserverServerIp;
            double gameId = replaydata.GameId;
            string internalName = Region.InternalName;
            string observerEncryptionKey = replaydata.ObserverEncryptionKey;
            var timer = new System.Timers.Timer
            {
                Interval = 5000
            };

            timer.Elapsed += (o, e) =>
            {
                var x = new Process
                {
                    StartInfo =
                    {
                        WorkingDirectory = ExecutingDirectory,
                        FileName = Path.Combine(ExecutingDirectory, "Replays", "ReplayRecorder.exe"),
                        Arguments = "\"" + ExecutingDirectory + "\" \"" + gameId + "\" \"" +
                                    observerEncryptionKey + "\" \"" +
                                    internalName + "\" \"" + observerServerIp + "\""
                    }
                };
                x.Start();
                timer.Stop();
            };
            timer.Start();
        }

        private static void p_Exited(object sender, EventArgs e)
        {
            MainWin.Show();
            MainWin.Visibility = Visibility.Visible;
        }

        internal static void LaunchSpectatorGame(string spectatorServer, string key, int gameId, string platform)
        {
            string gameDirectory = Location;

            var p = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = gameDirectory,
                    FileName = Path.Combine(gameDirectory, "League of Legends.exe"),
                    Arguments = "\"8393\" \"LoLLauncher.exe\" \"\" \"spectator "
                                + spectatorServer + " "
                                + key + " "
                                + gameId + " "
                                + platform + "\""
                }
            };
            //p.StartInfo.FileName = Location;
            p.Start();
        }

        internal static async void QuitCurrentGame()
        {
            if (OnMessage != null)
            {
                foreach (Delegate d in OnMessage.GetInvocationList())
                {
                    OnMessage -= (OnMessageHandler) d;
                }
            }

            FixChampSelect();
            FixLobby();
            IsInGame = false;

            await PvpNet.QuitGame();
            StatusGrid.Visibility = Visibility.Hidden;
            PlayButton.Visibility = Visibility.Visible;
            LobbyContent = null;
            LastPageContent = null;
            GameStatus = "outOfGame";
            SetChatHover();
            SwitchPage(new MainPage());
        }

        internal static void FixLobby()
        {
            if (OnFixLobby == null)
                return;

            foreach (Delegate d in OnFixLobby.GetInvocationList())
            {
                PvpNet.OnMessageReceived -= (PVPNetConnection.OnMessageReceivedHandler) d;
                OnFixLobby -= (PVPNetConnection.OnMessageReceivedHandler) d;
            }
        }

        internal static void FixChampSelect()
        {
            if (OnFixChampSelect == null)
                return;

            foreach (Delegate d in OnFixChampSelect.GetInvocationList())
            {
                PvpNet.OnMessageReceived -= (PVPNetConnection.OnMessageReceivedHandler) d;
                OnFixChampSelect -= (PVPNetConnection.OnMessageReceivedHandler) d;
            }
        }

        #endregion League Of Legends Logic

        internal static StatusPage StatusPage;
        internal static FriendList FriendList;
        internal static NotificationPage NotificationPage;
        internal static MainWindow MainWin;
        internal static bool GroupIsShown;
        internal static bool PlayerChatIsShown;
        internal static Page TrueCurrentPage;

        #region Public Helper Methods

        internal static void FocusClient()
        {
            if (MainWin.WindowState == WindowState.Minimized)
            {
                MainWin.WindowState = WindowState.Normal;
            }

            MainWin.Activate();
            MainWin.Topmost = true; // important
            MainWin.Topmost = false; // important
            MainWin.Focus(); // important
        }

        public static String TitleCaseString(String s)
        {
            if (s == null)
                return null;

            String[] words = s.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length == 0) continue;

                Char firstChar = Char.ToUpper(words[i][0]);
                String rest = "";
                if (words[i].Length > 1)
                {
                    rest = words[i].Substring(1).ToLower();
                }
                words[i] = firstChar + rest;
            }
            return String.Join(" ", words);
        }

        public static BitmapSource ToWpfBitmap(Bitmap bitmap)
        {
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Bmp);

                stream.Position = 0;
                var result = new BitmapImage();
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();

                return result;
            }
        }

        public static DateTime JavaTimeStampToDateTime(double javaTimeStamp)
        {
            // Java timestamp is millisecods past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(Math.Round(javaTimeStamp/1000)).ToLocalTime();
            return dtDateTime;
        }

        /// <summary>
        ///     Add data to the log file
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="type"></param>
        public static void Log(String lines, String type = "LOG")
        {
            WriteToLog.Log(lines, type);
        }

        //Get Image
        public static BitmapImage GetImage(string address)
        {
            var uriSource = new Uri(address, UriKind.RelativeOrAbsolute);
            if (File.Exists(address) || address.StartsWith("/LegendaryClient;component"))
                return new BitmapImage(uriSource);

            Log("Cannot find " + address, "WARN");
            uriSource = new Uri("/LegendaryClient;component/NONE.png", UriKind.RelativeOrAbsolute);
            
            return new BitmapImage(uriSource);
        }

        #endregion Public Helper Methods

        public static Accent CurrentAccent { get; set; }

        internal static void ChatClient_OnPresence(object sender, Presence pres)
        {
            if (pres.InnerText == "")
                ChatClient.Presence(CurrentPresence, GetPresence(), PresenceStatus, 0);
        }

        internal static string EncryptStringAES(this string input, string secret)
        {
            string output = String.Empty;
            var aesAlg = new RijndaelManaged();
            if (String.IsNullOrEmpty(input) || String.IsNullOrEmpty(secret))
                return output;
            try
            {
                // generate the key from the shared secret and the salt
                var key = new Rfc2898DeriveBytes(secret, Encoding.ASCII.GetBytes("o6806642kbM7c5"));
                // Create a RijndaelManaged object
                aesAlg.Key = key.GetBytes(aesAlg.KeySize/8);

                // Create a decryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (var msEncrypt = new MemoryStream())
                {
                    // prepend the IV
                    msEncrypt.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof (int));
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(input);
                        }
                    }
                    output = Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                aesAlg.Clear();
            }
            return output;
        }

        public static string DecryptStringAES(this string input, string secret)
        {
            string output = String.Empty;
            var aesAlg = new RijndaelManaged();
            if (String.IsNullOrEmpty(input) || String.IsNullOrEmpty(secret))
                return output;

            try
            {
                // generate the key from the shared secret and the salt
                var key = new Rfc2898DeriveBytes(secret, Encoding.ASCII.GetBytes("o6806642kbM7c5"));

                // Create the streams used for decryption.                
                byte[] bytes = Convert.FromBase64String(input);
                using (var msDecrypt = new MemoryStream(bytes))
                {
                    // Create a RijndaelManaged object
                    // with the specified key and IV.
                    aesAlg = new RijndaelManaged();
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize/8);
                    // Get the initialization vector from the encrypted stream
                    aesAlg.IV = ReadByteArray(msDecrypt);
                    // Create a decrytor to perform the stream transform.
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            output = srDecrypt.ReadToEnd();
                    }
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                aesAlg.Clear();
            }

            return output;
        }

        private static byte[] ReadByteArray(Stream s)
        {
            var rawLength = new byte[sizeof (int)];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
            {
                throw new SystemException("Stream did not contain properly formatted byte array");
            }

            var buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new SystemException("Did not read byte array properly");
            }

            return buffer;
        }

        internal static int SelectChamp;
        internal static bool UsingInstaPick = false;
    }


    public class ChatPlayerItem
    {
        public List<string> Messages = new List<string>();
        public string Id { get; set; }

        public string Username { get; set; }

        public int ProfileIcon { get; set; }

        public int Level { get; set; }

        public int Wins { get; set; }

        public int RankedWins { get; set; }

        public int Leaves { get; set; }

        public string LeagueTier { get; set; }

        public string LeagueDivision { get; set; }

        public string LeagueName { get; set; }

        public string GameStatus { get; set; }

        public long Timestamp { get; set; }

        public bool Busy { get; set; }

        public string Champion { get; set; }

        public string Status { get; set; }

        public bool UsingLegendary { get; set; }

        public bool IsLegendaryDev { get; set; }
    }

    public class Group
    {
        public Group(string s)
        {
            GroupName = s;
        }

        public string GroupName { get; set; }

        public bool IsOpen { get; set; }
    }
}