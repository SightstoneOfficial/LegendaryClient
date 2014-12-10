#region

using System.Linq;
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
        internal static Dictionary<String, LoginDataPacket> accountslist = new Dictionary<String, LoginDataPacket>();

        internal static List<Group> Groups = new List<Group>();

        /// <summary>
        ///     Gets the value of the league of Legends Settings
        /// </summary>
        /// <returns>All of the League Of Legends Settings</returns>
        public static Dictionary<String, String> LeagueSettingsReader(this string FileLocation)
        {
            var settings = new Dictionary<String, String>();
            string[] file = File.ReadAllLines(FileLocation);
            foreach (string x in from x in file
                where !String.IsNullOrEmpty(x) && !String.IsNullOrWhiteSpace(x)
                where !x.Contains("[") && !x.Contains("]")
                where !x.StartsWith("#") && x.Contains("=")
                select x)
            {
                try
                {
                    //Spit the one value into 2 values
                    string[] value = x.Split('=');
                    settings.Add(value[0], value[1]);
                }
                catch
                {
                    Log("Error reading a setting value: " + x, "ReaderError");
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

        internal static Dictionary<String, PVPNetConnection> pvpnetlist = new Dictionary<String, PVPNetConnection>();

        internal static async Task<LoginDataPacket> AddAccount()
        {
            return new LoginDataPacket();
        }

        internal static async Task<LoginDataPacket> AddAccount(LoginDataPacket packet)
        {
            if (packet == null)
                return new LoginDataPacket();

            accountslist.Add(packet.AllSummonerData.Summoner.Name, packet);
            return packet;
        }

        internal static async Task<LoginDataPacket> AddAccount(string Username, string Password)
        {
            var pvp = new PVPNetConnection();
            var credentials = new AuthenticationCredentials
            {
                ClientVersion = Version,
                AuthToken = "",
                Password = Password,
                IpAddress = ""
            };
            //pvp.Login();
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

        internal static bool isOwnerOfGame = false;

        internal static double QueueId = 0;

        /// <summary>
        ///     The database of all runes
        /// </summary>
        internal static List<runes> Runes;

        /// <summary>
        ///     Retreives UpdateDate For LegendaryClient
        /// </summary>
        /// <summary>
        ///     Stuff
        /// </summary>
        internal static string LegendaryClientVersion = "2.2.0.0";

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
        internal static bool done = true;

        /// <summary>
        ///     Sets Sqlite Version
        ///     Like the language pack
        /// </summary>
        internal static string sqlite = "gameStats_en_US.sqlite";

        //internal static string sqlite = "gameStats_ko_KR.sqlite";

        /// <summary>
        ///     Latest champion for League of Legends login screen
        /// </summary>
        internal const int LatestChamp = 103;

        /// <summary>
        ///     Latest version of League of Legends. Retrieved from ClientLibCommon.dat
        /// </summary>
        internal static string Version = "4.21.14";

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
        internal static SQLiteConnection SQLiteDatabase;

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
        internal static List<champions> Champions;

        /// <summary>
        ///     The database of all the champion abilities
        /// </summary>
        internal static List<championAbilities> ChampionAbilities;

        /// <summary>
        ///     The database of all the champion skins
        /// </summary>
        internal static List<championSkins> ChampionSkins;

        /// <summary>
        ///     The database of all the items
        /// </summary>
        internal static List<items> Items;

        /// <summary>
        ///     The database of all masteries
        /// </summary>
        internal static List<masteries> Masteries;

        /// <summary>
        ///     The Invite Data
        /// </summary>
        internal static List<invitationRequest> InviteJsonRequest = new List<invitationRequest>();

        /// <summary>
        ///     All of players who have been invited
        /// </summary>
        internal static Dictionary<String, InviteInfo> InviteData = new Dictionary<String, InviteInfo>();

        /// <summary>
        ///     The database of all the search tags
        /// </summary>
        internal static List<championSearchTags> SearchTags;

        /// <summary>
        ///     The database of all the keybinding defaults & proper names
        /// </summary>
        internal static List<keybindingEvents> Keybinds;

        internal static ChampionDTO[] PlayerChampions;

        internal static ReplayRecorder Autorecorder;

        internal static List<string> Whitelist = new List<string>();

        #region Chat

        internal static JabberClient ChatClient;
        //Fix for invitations
        public delegate void OnMessageHandler(object sender, Message e);

        public static event OnMessageHandler OnMessage;

        internal static PresenceType _CurrentPresence;

        internal static PresenceType CurrentPresence
        {
            get { return _CurrentPresence; }
            set
            {
                if (_CurrentPresence == value)
                    return;

                _CurrentPresence = value;
                if (ChatClient == null)
                    return;

                if (ChatClient.IsAuthenticated)
                    ChatClientConnect(null);
            }
        }

        internal static string _CurrentStatus;

        internal static string CurrentStatus
        {
            get { return _CurrentStatus; }
            set
            {
                if (_CurrentStatus == value)
                    return;

                _CurrentStatus = value;
                if (ChatClient == null)
                    return;

                if (ChatClient.IsAuthenticated)
                    ChatClientConnect(null);
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
                    var subject = (ChatSubjects) Enum.Parse(typeof (ChatSubjects), msg.Subject, true);
                    //NotificationPopup pop = new NotificationPopup(subject, msg);
                    //pop.Height = 230;
                    //pop.HorizontalAlignment = HorizontalAlignment.Right;
                    //pop.VerticalAlignment = VerticalAlignment.Bottom;
                    //Client.NotificationGrid.Children.Add(pop);
                }));

                return;
            }

            if (!AllPlayers.ContainsKey(msg.From.User) || String.IsNullOrWhiteSpace(msg.Body))
                return;

            ChatPlayerItem chatItem = AllPlayers[msg.From.User];
            chatItem.Messages.Add(chatItem.Username + "|" + msg.Body);
            MainWin.FlashWindow();
        }

        internal static async void GameInvite(object sender, PVPNetConnection PVPConnect, string GameID)
        {
            await PVPConnect.Accept(GameID);
        }

        internal static void ChatClientConnect(object sender)
        {
            Groups.Add(new Group("Online"));

            //Get all groups
            var manager = sender as RosterManager;
            if (manager != null)
            {
                string ParseString = manager.ToString();
                var StringHackOne = new List<string>(ParseString.Split(new[] {"@pvp.net="}, StringSplitOptions.None));
                StringHackOne.RemoveAt(0);
                foreach (
                    string Parse in
                        StringHackOne.Select(StringHack => StringHack.Split(','))
                            .Select(StringHackTwo => StringHackTwo[0]))
                {
                    using (XmlReader reader = XmlReader.Create(new StringReader(Parse)))
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsStartElement())
                                continue;

                            switch (reader.Name)
                            {
                                case "group":
                                    reader.Read();
                                    string Group = reader.Value;
                                    if (Group != "**Default" && Groups.Find(e => e.GroupName == Group) == null)
                                        Groups.Add(new Group(Group));
                                    break;
                            }
                        }
                    }
                }
            }

            Groups.Add(new Group("Offline"));
            SetChatHover();
        }

        internal static void SendMessage(string User, string Message)
        {
            ChatClient.Message(User, Message);
        }

        internal static void SetChatHover()
        {
            ChatClient.Presence(CurrentPresence, GetPresence(), presenceStatus, 0);
        }

        internal static bool hidelegendaryaddition;

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
            else if (hidelegendaryaddition)
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
                        presenceStatus = "dnd";
                        break;
                    case "outOfGame":
                        presenceStatus = "chat";
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
                    sb.Append("<timeStamp>" + timeStampSince + "</timeStamp>");
                    break;
                case "inGame":
                case "championSelect":
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
                Group = "Online"
            };
            using (XmlReader reader = XmlReader.Create(new StringReader(ri.OuterXml)))
            {
                while (reader.Read())
                {
                    if (!reader.IsStartElement())
                        continue;

                    switch (reader.Name)
                    {
                        case "group":
                            reader.Read();
                            string TempGroup = reader.Value;
                            if (TempGroup != "**Default")
                                player.Group = TempGroup;
                            break;
                    }
                }
            }
            player.Username = ri.Nickname;
            bool PlayerPresence = PresManager.IsAvailable(ri.JID);
            AllPlayers.Add(ri.JID.User, player);
        }


        internal static StackPanel chatlistview;

        internal static void PresManager_OnPrimarySessionChange(object sender, JID bare)
        {
            if (!AllPlayers.ContainsKey(bare.User))
                return;

            ChatPlayerItem Player = AllPlayers[bare.User];
            Player.IsOnline = false;
            UpdatePlayers = true;
            Presence[] s = PresManager.GetAll(bare);
            if (s.Length == 0)
                return;

            string Presence = s[0].Status;
            if (Presence == null)
                return;

            Player.RawPresence = Presence; //For debugging
            Player.IsOnline = true;
            using (XmlReader reader = XmlReader.Create(new StringReader(Presence)))
            {
                try
                {
                    while (reader.Read())
                    {
                        if (!reader.IsStartElement() || reader.IsEmptyElement)
                            continue;

                        #region Parse Presence

                        switch (reader.Name)
                        {
                            case "profileIcon":
                                reader.Read();
                                Player.ProfileIcon = Convert.ToInt32(reader.Value);
                                break;

                            case "level":
                                reader.Read();
                                Player.Level = Convert.ToInt32(reader.Value);
                                break;

                            case "wins":
                                reader.Read();
                                Player.Wins = Convert.ToInt32(reader.Value);
                                break;

                            case "leaves":
                                reader.Read();
                                Player.Leaves = Convert.ToInt32(reader.Value);
                                break;

                            case "rankedWins":
                                reader.Read();
                                Player.RankedWins = Convert.ToInt32(reader.Value);
                                break;

                            case "timeStamp":
                                reader.Read();
                                Player.Timestamp = Convert.ToInt64(reader.Value);
                                break;

                            case "statusMsg":
                                reader.Read();
                                Player.Status = reader.Value;
                                break;

                            case "dev":
                                reader.Read();
                                Player.UsingLegendary = true;
                                if (reader.Value == "true")
                                    Player.IsLegendaryDev = true;
                                break;

                            case "gameStatus":
                                reader.Read();
                                Player.GameStatus = reader.Value;
                                break;

                            case "skinname":
                                reader.Read();
                                Player.Champion = reader.Value;
                                break;

                            case "rankedLeagueName":
                                reader.Read();
                                Player.LeagueName = reader.Value;
                                break;

                            case "rankedLeagueTier":
                                reader.Read();
                                Player.LeagueTier = reader.Value;
                                break;

                            case "rankedLeagueDivision":
                                reader.Read();
                                Player.LeagueDivision = reader.Value;
                                break;
                        }

                        #endregion Parse Presence
                    }
                }
                catch (Exception e)
                {
                    Log(e.Message + " - remember to fix this later instead of avoiding the problem.");
                }
            }
            if (String.IsNullOrWhiteSpace(Player.Status))
                Player.Status = "Online";
        }

        internal static void Message(string To, string Message, ChatSubjects Subject)
        {
            var msg = new Message(ChatClient.Document)
            {
                Type = MessageType.normal,
                To = To + "@pvp.net",
                Subject = Subject.ToString(),
                Body = Message
            };

            ChatClient.Write(msg);
        }

        //Why do you even have to do this, riot?
        internal static string GetObfuscatedChatroomName(string Subject, string Type)
        {
            byte[] data = Encoding.UTF8.GetBytes(Subject);
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

            return Type + "~" + obfuscatedName;
        }

        internal static bool runonce = false;

        internal static string GetChatroomJID(string ObfuscatedChatroomName, string password, bool IsTypePublic)
        {
            if (!IsTypePublic)
                return ObfuscatedChatroomName + "@sec.pvp.net";

            if (String.IsNullOrEmpty(password))
                return ObfuscatedChatroomName + "@lvl.pvp.net";

            return ObfuscatedChatroomName + "@conference.pvp.net";
        }

        internal static int AmountOfWins; //Calculate wins for presence
        internal static bool IsRanked;
        internal static string TierName;
        internal static string Tier;
        internal static string LeagueName;
        internal static string GameStatus = "outOfGame";
        internal static string presenceStatus = "chat";
        internal static double timeStampSince = 0;

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
<<<<<<< HEAD
            if (page.GetType() == typeof(PlayPage)) IsOnPlayPage = true;
            else IsOnPlayPage = false;
            if (page.GetType() == typeof(ChampSelectPage)) BackgroundImage.Visibility = Visibility.Hidden;
            else BackgroundImage.Visibility = Visibility.Visible;
            if (page.GetType() == typeof(MainPage))
            {
                Page p = Pages.FirstOrDefault(x => x.GetType() == typeof(MainPage));
                if(p != null)
                {
                    (p as MainPage).UpdateSummonerInformation();
                }
            }
=======
            IsOnPlayPage = page.GetType() == typeof (PlayPage);
            BackgroundImage.Visibility = page.GetType() == typeof (ChampSelectPage)
                ? Visibility.Hidden
                : Visibility.Visible;
>>>>>>> origin/master

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
            foreach (
                UIElement element in
                    NotificationGrid.Children.Cast<UIElement>().Where(element => element.GetType() == containerType))
            {
                NotificationGrid.Children.Remove(element);
                return;
            }
        }

        internal static void ClearMainGrid(Type containerType)
        {
            foreach (
                UIElement element in
                    MainGrid.Children.Cast<UIElement>().Where(element => element.GetType() == containerType))
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
        internal static PVPNetConnection PVPNet;

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
        ///     GameID of the current game that the client is connected to
        /// </summary>
        internal static double GameID = 0;

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
        internal static GameDTO GameLobbyDTO;

        /// <summary>
        ///     A recorder
        /// </summary>
        internal static ReplayRecorder recorder = null;

        /// <summary>
        ///     When going into champion select reuse the last DTO to set up data
        /// </summary>
        internal static GameDTO ChampSelectDTO;

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
                    StoreAccountBalanceNotification newBalance = balance;
                    InfoLabel.Content = "IP: " + newBalance.Ip + " ∙ RP: " + newBalance.Rp;
                    LoginPacket.IpBalance = newBalance.Ip;
                    LoginPacket.RpBalance = newBalance.Rp;
                }
                else
                {
                    var gameNotification = message as GameNotification;
                    if (gameNotification != null)
                    {
                        GameNotification notification = gameNotification;
                        var messageOver = new MessageOverlay {MessageTitle = {Content = notification.Type}};
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
                        var EndOfGame = new EndOfGamePage(stats);
                        ClearPage(typeof (TeamQueuePage));
                        OverlayContainer.Visibility = Visibility.Visible;
                        OverlayContainer.Content = EndOfGame.Content;
                    }
                    else if (message is StoreFulfillmentNotification)
                    {
                        PlayerChampions = await PVPNet.GetAvailableChampions();
                    }
                    else if (message is Inviter)
                    {
                        var stats = message as Inviter;
                        //CurrentInviter = stats;
                    }
                    else if (message is InvitationRequest)
                    {
                        var stats = message as InvitationRequest;
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
                        catch
                        {
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

        internal static string InternalQueueToPretty(string InternalQueue)
        {
            switch (InternalQueue)
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
                    return InternalQueue;
            }
        }

        /// <summary>
        ///     Super complex method to get the queue name when it is unknown
        /// </summary>
        /// <param name="QueueName"></param>
        /// <returns></returns>
        private static string convert(string QueueName)
        {
            string result = string.Empty;
            string Queueinternal = "";
            string Bots = "";
            const string Players = "";
            const string Extra = "";
            string start = QueueName.Replace("matching-queue-", "").Replace("-game-queue", "");
            string[] x = start.Split('_');
            if (x[1].ToLower() == "bot")
            {
                Bots = " Bots";
                string[] m = x[3].Split('-');
            }
            else if (x[0].ToLower() == "bot" && x[1].ToLower() == "intro")
            {
                Queueinternal = "Intro";
                Bots = "Bots";
            }

            result = string.Format("{0}{1} {2} {3}", Queueinternal, Bots, Players, Extra);
            return result;
        }

        internal static string GetGameDirectory()
        {
            string Directory = Path.Combine(RootLocation, "RADS", "projects", "lol_game_client", "releases");

            var dInfo = new DirectoryInfo(Directory);
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
                latestVersion = info.Name;

            Directory = Path.Combine(Directory, latestVersion, "deploy");

            return Directory;
        }

        internal static string Location;
        internal static string LoLLauncherLocation;
        internal static string LOLCLIENTVERSION;
        internal static string RootLocation;

        internal static void LaunchGame()
        {
            string GameDirectory = Location;

            var p = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = GameDirectory,
                    FileName = Path.Combine(GameDirectory, "League of Legends.exe")
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
            string ObserverServerIp = replaydata.ObserverServerIp;
            double GameId = replaydata.GameId;
            string InternalName = Region.InternalName;
            string ObserverEncryptionKey = replaydata.ObserverEncryptionKey;
            var timer = new System.Timers.Timer {Interval = 5000};
            timer.Elapsed += (o, e) =>
            {
                var x = new Process
                {
                    StartInfo =
                    {
                        WorkingDirectory = ExecutingDirectory,
                        FileName = Path.Combine(ExecutingDirectory, "Replays", "ReplayRecorder.exe"),
                        Arguments = "\"" + ExecutingDirectory + "\" \"" + GameId + "\" \"" +
                                    ObserverEncryptionKey + "\" \"" +
                                    InternalName + "\" \"" + ObserverServerIp + "\""
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

        internal static void LaunchSpectatorGame(string SpectatorServer, string Key, int GameId, string Platform)
        {
            string GameDirectory = Location;

            var p = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = GameDirectory,
                    FileName = Path.Combine(GameDirectory, "League of Legends.exe"),
                    Arguments = "\"8393\" \"LoLLauncher.exe\" \"\" \"spectator "
                                + SpectatorServer + " "
                                + Key + " "
                                + GameId + " "
                                + Platform + "\""
                }
            };
            //p.StartInfo.FileName = Location;
            p.Start();
        }

        internal static async void QuitCurrentGame()
        {
            if (OnMessage != null)
                foreach (Delegate d in OnMessage.GetInvocationList())
                    OnMessage -= (OnMessageHandler) d;

            FixChampSelect();
            FixLobby();
            IsInGame = false;

            await PVPNet.QuitGame();
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
                PVPNet.OnMessageReceived -= (PVPNetConnection.OnMessageReceivedHandler) d;
                OnFixLobby -= (PVPNetConnection.OnMessageReceivedHandler) d;
            }
        }

        internal static void FixChampSelect()
        {
            if (OnFixChampSelect == null)
                return;

            foreach (Delegate d in OnFixChampSelect.GetInvocationList())
            {
                PVPNet.OnMessageReceived -= (PVPNetConnection.OnMessageReceivedHandler) d;
                OnFixChampSelect -= (PVPNetConnection.OnMessageReceivedHandler) d;
            }
        }

        #endregion League Of Legends Logic

        internal static StatusPage statusPage;
        internal static FriendList FriendList;
        internal static NotificationPage notificationPage;
        internal static MainWindow MainWin;
        internal static bool GroupIsShown;
        internal static bool PlayerChatIsShown;
        internal static Page TrueCurrentPage;

        #region Public Helper Methods

        internal static void FocusClient()
        {
            if (MainWin.WindowState == WindowState.Minimized)
                MainWin.WindowState = WindowState.Normal;

            MainWin.Activate();
            MainWin.Topmost = true; // important
            MainWin.Topmost = false; // important
            MainWin.Focus(); // important
        }

        public static String TitleCaseString(String s)
        {
            if (s == null) return s;

            String[] words = s.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length == 0)
                    continue;

                Char firstChar = Char.ToUpper(words[i][0]);
                String rest = "";
                if (words[i].Length > 1)
                    rest = words[i].Substring(1).ToLower();

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
        public static BitmapImage GetImage(string Address)
        {
            var UriSource = new Uri(Address, UriKind.RelativeOrAbsolute);
            if (File.Exists(Address) || Address.StartsWith("/LegendaryClient;component"))
                return new BitmapImage(UriSource);

            Log("Cannot find " + Address, "WARN");
            UriSource = new Uri("/LegendaryClient;component/NONE.png", UriKind.RelativeOrAbsolute);

            return new BitmapImage(UriSource);
        }

        #endregion Public Helper Methods

        public static Accent CurrentAccent { get; set; }

        internal static void ChatClient_OnPresence(object sender, Presence pres)
        {
            if (pres.InnerText == "")
                ChatClient.Presence(CurrentPresence, GetPresence(), presenceStatus, 0);
        }

        internal static string EncryptStringAES(this string input, string Secret)
        {
            string output = String.Empty;
            var aesAlg = new RijndaelManaged();
            if (String.IsNullOrEmpty(input) || String.IsNullOrEmpty(Secret))
                return output;

            try
            {
                // generate the key from the shared secret and the salt
                var key = new Rfc2898DeriveBytes(Secret, Encoding.ASCII.GetBytes("o6806642kbM7c5"));
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

        public static string DecryptStringAES(this string input, string Secret)
        {
            string output = String.Empty;
            var aesAlg = new RijndaelManaged();
            if (String.IsNullOrEmpty(input) || String.IsNullOrEmpty(Secret))
                return output;

            try
            {
                // generate the key from the shared secret and the salt
                var key = new Rfc2898DeriveBytes(Secret, Encoding.ASCII.GetBytes("o6806642kbM7c5"));

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
            catch
            {
                Log("Error decrypting password", "ERROR");
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
                throw new SystemException("Stream did not contain properly formatted byte array");

            var buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
                throw new SystemException("Did not read byte array properly");

            return buffer;
        }

        internal static int SelectChamp;
        internal static bool usingInstaPick = false;

        internal static KeyValuePair<String, String> userpass;

        internal static void ChatClient_OnDisconnect(object sender)
        {
            ChatClient.User = userpass.Key;
            ChatClient.Password = userpass.Value;
            ChatClient.Login();
        }
    }


    public class ChatPlayerItem
    {
        public List<string> Messages = new List<string>();
        public string Group { get; set; }

        public bool IsOnline { get; set; }

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

        public string RawPresence { get; set; }

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