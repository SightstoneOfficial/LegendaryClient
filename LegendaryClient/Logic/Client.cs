using LCLog;
using LegendaryClient.Controls;
using LegendaryClient.Logic.JSON;
using LegendaryClient.Logic.Region;
using LegendaryClient.Logic.Replays;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Windows;
using MahApps.Metro;
using MahApps.Metro.Controls;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using LegendaryClient.Logic.Riot.Platform;
using RtmpSharp.Net;
using Button = System.Windows.Controls.Button;
using Image = System.Windows.Controls.Image;
using Label = System.Windows.Controls.Label;
using ListView = System.Windows.Controls.ListView;
using Timer = System.Windows.Forms.Timer;
using LegendaryClient.Logic.Riot;
using LegendaryClient.Logic.Riot.Platform.Messaging.Persistence;
using RtmpSharp.Messaging;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.roster;
using agsXMPP;
using System.Security.Cryptography;

//using LegendaryClient.Logic.AutoReplayRecorder;

namespace LegendaryClient.Logic
{
    /// <summary>
    ///     Any logic that needs to be reused over multiple pages
    /// </summary>
    internal static class Client
    {
		/// <summary>
		/// Gets called when you receive a message
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="Message"></param>
		public delegate void OnMessageReceivedPy(string sender, string Message);
		/// <summary>
		/// Gets called when you receive a message
		/// </summary>
		public static event OnMessageReceivedPy onChatMessageReceived;
		public delegate void OnAccept(bool accept);

        public static event OnAccept PlayerAccepedQueue;

        public static StreamString SendPIPE;
        public static StreamString InPIPE;

        public static void SendAccept(bool accept)
        {
            if (PlayerAccepedQueue != null)
                PlayerAccepedQueue(accept);
        }

        public static string ToSHA1(this string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(hash);
            }
        }

        internal static bool patching = true;

        /// <summary>
        ///     This is all accounts that have been added to LegendaryClient
        ///     Use this for multiaccount in futuree
        /// </summary>
        internal static Dictionary<string, LoginDataPacket> accountslist = new Dictionary<string, LoginDataPacket>();

        internal static List<Group> Groups = new List<Group>();

        internal static string UID;

        /// <summary>
        ///     Gets the value of the league of Legends Settings
        /// </summary>
        /// <returns>All of the League Of Legends Settings</returns>
        public static Dictionary<string, string> LeagueSettingsReader(this string fileLocation)
        {
            var settings = new Dictionary<string, string>();
            if (File.Exists(fileLocation))
            {
                string[] file = File.ReadAllLines(fileLocation);
                foreach (string x in from x in file
                                     where !string.IsNullOrEmpty(x) && !string.IsNullOrWhiteSpace(x)
                                     where !x.Contains("[") && !x.Contains("]")
                                     where !x.StartsWith("#") && x.Contains("=")
                                     select x)
                    if (x.Contains("="))
                    {
                        string[] value = x.Split('=');
                        settings.Add(value[0], value[1]);
                    }
            }
            return settings;
        }

        public static bool InstaCall = false;
        public static string CallString = string.Empty;

        //internal static Dictionary<string, PVPNetConnection> pvpnetlist = new Dictionary<string, PVPNetConnection>();

        internal static LoginDataPacket AddAccount()
        {
            return new LoginDataPacket();
        }

        internal static LoginDataPacket AddAccount(LoginDataPacket packet)
        {
            if (packet == null)
                return new LoginDataPacket();

            accountslist.Add(packet.AllSummonerData.Summoner.Name, packet);

            return packet;
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

        internal static bool isOwnerOfGame = false;

        internal static double QueueId = 0;

        internal static agsXMPP.XmppClientConnection XmppConnection;

        /// <summary>
        ///     The database of all runes
        /// </summary>
        internal static List<runes> Runes;

        /// <summary>
        ///     Button For Lobby
        /// </summary>
        internal static Button ReturnButton;

        /// <summary>
        ///     spectatorTimer
        /// </summary>
        internal static System.Timers.Timer spectatorTimer = new System.Timers.Timer();

        /// <summary>
        ///     inQueueTimer
        /// </summary>
        internal static Label inQueueTimer;

        /// <summary>
        ///     Check if on Champion Select or Team Queue Lobby Page
        /// </summary>
        internal static Page CurrentPage;

        /// <summary>
        ///     If Player is creating an account
        /// </summary>
        internal static bool done = true;

        /// <summary>
        ///     Sets Sqlite Version
        ///     Like the language pack
        /// </summary>
        internal static string sqlite = "gameStats_en_US.sqlite";

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
        internal static string ExecutingDirectory = string.Empty;

        /// <summary>
        ///     Riot's database with all the client data
        /// </summary>
        internal static SQLiteConnection SQLiteDatabase;

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
        ///     All of players who have been invited
        /// </summary>
        internal static Dictionary<string, InviteInfo> InviteData = new Dictionary<string, InviteInfo>();

        internal static string Theme;

        internal static ChampionDTO[] PlayerChampions;

        internal static List<int> curentlyRecording = new List<int>();

        internal static List<string> Whitelist = new List<string>();

        #region Chat


        //Fix for invitations
        public delegate void OnMessageHandler(object sender, Message e);

        public static event OnMessageHandler OnMessage;

        public static Dictionary<string, string> PlayerNote = new Dictionary<string, string>();
        internal static RosterManager RostManager;
        internal static PresenceManager PresManager;
        //internal static ConferenceManager ConfManager;
        internal static bool UpdatePlayers = true;
        internal static Dictionary<string, ChatPlayerItem> AllPlayers = new Dictionary<string, ChatPlayerItem>();

        internal static PresenceType _CurrentPresence;

        internal static PresenceType CurrentPresence
        {
            get { return _CurrentPresence; }
            set
            {
                if (_CurrentPresence == value)
                    return;

                _CurrentPresence = value;
                if (XmppConnection == null)
                    return;

                if (XmppConnection.Authenticated)
                    SetChatHover();
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
                if (XmppConnection == null)
                    return;

                if (XmppConnection.Authenticated)
                    SetChatHover();
            }
        }

        internal static void XmppConnection_OnMessage(object sender, agsXMPP.protocol.client.Message msg)
        {
            Log(string.Format("Received chat msg \"{0}\" from the user \"{1}\"", msg.Body, msg.From.User));
            onChatMessageReceived(msg.From.User, msg.Body);
            //This means that it is not for the user
            Log(JsonConvert.SerializeObject(msg));

            //This blocks spammers from elo bosters
            if (Client.ChatAutoBlock == null)
            {
                using (var client = new WebClient())
                {
                    var banned = client.DownloadString("http://legendaryclient.net/Autoblock.txt");
                    Client.ChatAutoBlock = banned.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            if (Client.ChatAutoBlock.Any(x => (msg.From.User + Region.RegionName).ToSHA1() == x.Split('#')[0]) && autoBlock)
                return;
            if (msg.Body.ToLower().Contains("elo") && msg.Body.ToLower().Contains("boost"))
                return;

            if (msg.Subject != null)
            {
                MainWin.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    var subject = (ChatSubjects)Enum.Parse(typeof(ChatSubjects), msg.Subject, true);
                    NotificationPopup pop = new NotificationPopup(subject, msg);
                    pop.Height = 230;
                    pop.HorizontalAlignment = HorizontalAlignment.Right;
                    pop.VerticalAlignment = VerticalAlignment.Bottom;
                    NotificationGrid.Children.Add(pop);
                }));

                return;
            }

            if (!AllPlayers.ContainsKey(msg.From.User) || string.IsNullOrWhiteSpace(msg.Body))
                return;

            var chatItem = AllPlayers[msg.From.User];
            if (Filter)
                chatItem.Messages.Add(chatItem.Username + "|" + msg.Body.Filter());
            else
                chatItem.Messages.Add(chatItem.Username + "|" + msg.Body);
            MainWin.FlashWindow();
        }

        internal static bool autoBlock = true;

        internal static bool loadedGroups = false;

        internal static void ChatClientConnect(object sender)
        {
            loadedGroups = false;
            Groups.Add(new Group("Online"));

            //Get all groups
            var manager = sender as RosterManager;
            if (manager != null)
            {
                string ParseString = manager.ToString();
                var stringHackOne = new List<string>(ParseString.Split(new[] { "@pvp.net=" }, StringSplitOptions.None));
                stringHackOne.RemoveAt(0);
                foreach (
                    string Parse in
                        stringHackOne.Select(stringHack => Regex.Split(stringHack, @"</item>,"))
                            .Select(StringHackTwo => StringHackTwo[0]))
                {
                    string temp;
                    if (!Parse.Contains("</item>"))
                        temp = Parse + "</item>";
                    else
                        temp = Parse;
                    var xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(temp);
                    string PlayerJson = JsonConvert.SerializeXmlNode(xmlDocument).Replace("#", "").Replace("@", "");
                    try
                    {
                        if (PlayerJson.Contains(":{\"priority\":"))
                        {
                            RootObject root = JsonConvert.DeserializeObject<RootObject>(PlayerJson);

                            if (!string.IsNullOrEmpty(root.item.name) && !string.IsNullOrEmpty(root.item.note))
                                PlayerNote.Add(root.item.name, root.item.note);

                            if (root.item.group.text != "**Default" && Groups.Find(e => e.GroupName == root.item.group.text) == null && root.item.group.text != null)
                                Groups.Add(new Group(root.item.group.text));
                        }
                        else
                        {
                            RootObject2 root = JsonConvert.DeserializeObject<RootObject2>(PlayerJson);

                            if (!string.IsNullOrEmpty(root.item.name) && !string.IsNullOrEmpty(root.item.note))
                                PlayerNote.Add(root.item.name, root.item.note);

                            if (root.item.group != "**Default" && Groups.Find(e => e.GroupName == root.item.group) == null && root.item.group != null)
                                Groups.Add(new Group(root.item.group));
                        }
                    }
                    catch
                    {
                        Log("Can't load friends", "ERROR");
                    }
                }
            }

            Groups.Add(new Group("Offline"));
            SetChatHover();
            loadedGroups = true;
            Client.XmppConnection.OnRosterEnd -= Client.ChatClientConnect; //only update groups on login
        }

        internal static void RostManager_OnRosterItem(object sender, RosterItem ri)
        {
            UpdatePlayers = true;
            if (AllPlayers.ContainsKey(ri.Jid.User))
                return;

            var player = new ChatPlayerItem
            {
                Id = ri.Jid.User,
                Group = "Online"
            };
            //using (XmlReader reader = XmlReader.Create(new StringReader(ri.OuterXml)))
            using (XmlReader reader = XmlReader.Create(new StringReader(ri.ToString())))
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
            player.Username = ri.Name;
            AllPlayers.Add(ri.Jid.User, player);
        }

        internal static void SendMessage(string User, string Message)
        {
            XmppConnection.Send(new Message(User, Message));
        }

        internal static void SetChatHover()
        {
            if (XmppConnection.Authenticated)
            {
                if (presenceStatus != ShowType.NONE)
                    XmppConnection.Send(new Presence(presenceStatus, GetPresence(), 0) { Type = PresenceType.available });
                else
                    XmppConnection.Send(new Presence(presenceStatus, GetPresence(), 0) { Type = PresenceType.invisible });
            }
            
        }

        internal static bool hidelegendaryaddition = false;
        internal static int ChampId = -1;
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
            int level = Convert.ToInt32(LoginPacket.AllSummonerData.SummonerLevel.Level);
            if (GameStatus != "busy")
            {
                switch (GameStatus)
                {
                    case "inQueue":
                    case "championSelect":
                    case "inGame":
                        presenceStatus = ShowType.dnd;
                        break;
                    case "outOfGame":
                        presenceStatus = ShowType.chat;
                        break;
                }
            }
            var sb = new StringBuilder();
            sb.Append("<body><profileIcon>");
            sb.Append(LoginPacket.AllSummonerData.Summoner.ProfileIconId);
            sb.Append("</profileIcon><level>");
            sb.Append(level);
            sb.Append("</level>");
            if (!hidelegendaryaddition)
            {
                sb.Append("<dev>");
                sb.Append(Dev ? "true" : "false");
                sb.Append("</dev>");
            }
            sb.Append("<wins>");
            sb.Append(AmountOfWins);
            sb.Append("</wins><leaves>0</leaves><odinWins>0</odinWins><odinLeaves>0</odinLeaves>"); // TODO
            sb.Append("<queueType />");
            sb.Append("<rankedLosses>0</rankedLosses><rankedRating>0</rankedRating>"); // unused for now
            if (!IsRanked)
                sb.Append("<tier>UNRANKED</tier>");
            else
                sb.Append("<tier>" + TierName + "</tier>");
            sb.Append("<rankedSoloRestricted>");
            sb.Append(LoginPacket.RestrictedGamesRemainingForRanked != -1);
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
                    sb.Append("<gameQueueType>NONE</gameQueueType><isObservable>ALL</isObservable><skinname>");
                    sb.Append(ChampId != -1 ? champions.GetChampion(ChampId).displayName : "Unknown");
                    sb.Append("</skinname>");
                    break;
                case "championSelect":
                    sb.Append(
                        "<gameQueueType>NONE</gameQueueType><isObservable>ALL</isObservable><skinname>");
                    sb.Append(ChampId != -1 ? champions.GetChampion(ChampId).displayName : "Unknown");
                    sb.Append("</skinname>");
                    break;
            }
            if (CurrentStatus == "Online")
                sb.Append("<statusMsg />");
            else
                sb.Append("<statusMsg>" + CurrentStatus + "</statusMsg>");
            sb.Append("</body>");

            return sb.ToString();
        }

        /*
        internal static void RostManager_OnRosterItem(object sender, Item ri)
        {
            UpdatePlayers = true;
            if (AllPlayers.ContainsKey(ri.Jid.User))
                return;

            var player = new ChatPlayerItem
            {
                Id = ri.Jid.User,
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
            AllPlayers.Add(ri.Jid.User, player);
        }//*/


        internal static StackPanel chatlistview;
        //Re-add this later
        /*
        internal static void PresManager_OnPrimarySessionChange(object sender, Jid bare)
        {
            if (!AllPlayers.ContainsKey(bare.User))
                return;

            ChatPlayerItem Player = AllPlayers[bare.User];
            Player.IsOnline = false;
            UpdatePlayers = true;
            //Presence[] s = PresManager.GetAll(bare);
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
            if (string.IsNullOrWhiteSpace(Player.Status))
                Player.Status = "Online";
        }
        //*/

        internal static void Message(string To, string Message, ChatSubjects Subject)
        {
            var msg = new Message(new Jid(To + "@pvp.net"))
            {
                Type = MessageType.normal,
                Subject = Subject.ToString(),
                Body = Message
            };

            XmppConnection.Send(msg);
        }

        internal static string GetObfuscatedChatroomName(string Subject, string Type)
        {
            byte[] data = Encoding.UTF8.GetBytes(Subject);
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] result = sha.ComputeHash(data);
            string obfuscatedName = string.Empty;
            int incrementValue = 0;
            while (incrementValue < result.Length)
            {
                int bitHack = result[incrementValue];
                obfuscatedName = obfuscatedName + Convert.ToString(((uint)(bitHack & 240) >> 4), 16);
                obfuscatedName = obfuscatedName + Convert.ToString(bitHack & 15, 16);
                incrementValue = incrementValue + 1;
            }
            obfuscatedName = Regex.Replace(obfuscatedName, @"/\s+/gx", string.Empty);
            obfuscatedName = Regex.Replace(obfuscatedName, @"/[^a-zA-Z0-9_~]/gx", string.Empty);

            return Type + "~" + obfuscatedName;
        }

        internal static bool runonce = false;

        internal static string GetChatroomJid(string ObfuscatedChatroomName, string password, bool IsTypePublic)
        {
            if (!IsTypePublic)
                return ObfuscatedChatroomName + "@sec.pvp.net";

            if (string.IsNullOrEmpty(password))
                return ObfuscatedChatroomName + "@lvl.pvp.net";

            return ObfuscatedChatroomName + "@conference.pvp.net";
        }

        internal static int AmountOfWins; //Calculate wins for presence
        internal static bool IsRanked;
        internal static string TierName;
        internal static string Tier;
        internal static string LeagueName;
        internal static string GameStatus = "outOfGame";
        internal static ShowType presenceStatus = ShowType.chat;
        internal static double timeStampSince = 0;

        #endregion Chat

        internal static Grid MainGrid;
        internal static Grid NotificationGrid;
        internal static Grid StatusGrid = new Grid();
        internal static Image BackgroundImage;

        internal static Label StatusLabel;
        internal static Label InfoLabel;
        internal static ContentControl OverlayContainer;
        internal static ContentControl OverOverlayContainer;
        internal static Button PlayButton;
        internal static ContentControl ChatContainer;
        internal static ContentControl StatusContainer;
        internal static ContentControl NotificationOverlayContainer;
        internal static ContentControl NotificationContainer;
        internal static ContentControl FullNotificationOverlayContainer;
        internal static ListView ChatListView;
        internal static ChatItem ChatItem;
        internal static List<GroupChatItem> GroupChatItems = new List<GroupChatItem>();
        internal static GroupChatItem CurrentGroupChatItem;

        internal static ListView InviteListView;
        internal static Image MainPageProfileImage;

        internal static Label UserTitleBarLabel;
        internal static Image UserTitleBarImage;

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
            Log("Switching to the page: " + page.GetType());
            IsOnPlayPage = page.GetType() == typeof(PlayPage);
            BackgroundImage.Visibility = page.GetType() == typeof(ChampSelectPage)
                ? Visibility.Hidden
                : Visibility.Visible;
            if (page.GetType() == typeof(MainPage))
            {
                Page p = Pages.FirstOrDefault(x => x.GetType() == typeof(MainPage));

                var mainPage = p as MainPage;
                if (mainPage != null)
                    mainPage.UpdateSummonerInformation();
            }

            TrueCurrentPage = page;

            foreach (Page p in Pages.Where(p => p.GetType() == page.GetType()))
            {
                Container.Content = p.Content;

                return;
            }
            Container.Content = page.Content;
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

        public static GameScouter win;
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

        private static System.Timers.Timer HeartbeatTimer;

        internal static void StartHeartbeat()
        {
            HeartbeatTimer = new System.Timers.Timer();
            HeartbeatTimer.Elapsed += DoHeartbeat;
            HeartbeatTimer.Interval = 120000; // in milliseconds
            HeartbeatTimer.Start();
            DoHeartbeat(null, null);
        }

        private static int HeartbeatCount;
        public static Session PlayerSession;
        internal async static void DoHeartbeat(object sender, ElapsedEventArgs e)
        {
            if (IsLoggedIn)
            {
                string result = await RiotCalls.PerformLCDSHeartBeat(Convert.ToInt32(LoginPacket.AllSummonerData.Summoner.AcctId),
                    PlayerSession.Token,
                    HeartbeatCount,
                    DateTime.Now.ToString("ddd MMM d yyyy HH:mm:ss 'GMT-0700'"));
                if (result != "5")
                {
                    
                }
                HeartbeatCount++;
            }
        }

        internal static string Gas;

        /// <summary>
        ///     Main connection to the League of Legends server
        /// </summary>
        internal static RtmpClient RiotConnection;

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
        ///     Game Name of the current game that the client is connected to
        /// </summary>
        internal static string GameName = string.Empty;

        /// <summary>
        ///     The DTO of the game lobby when connected to a custom game
        /// </summary>
        internal static GameDTO GameLobbyDTO;

        /// <summary>
        /// 
        /// </summary>
        internal static bool donepatch = false;

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

        internal static bool AutoAcceptQueue = false;
        internal static object LobbyContent;
        internal static object LastPageContent;
        internal static bool IsInGame;
        internal static bool RunonePop = false;
#pragma warning disable 4014

        internal static void OnMessageReceived(object sender, MessageReceivedEventArgs message)
        {
            Log("Message received! The type is: " + message.Body.GetType());
            MainWin.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async () =>
            {
                var balance = message.Body as StoreAccountBalanceNotification;
                if (balance != null)
                {
                    StoreAccountBalanceNotification newBalance = balance;
                    InfoLabel.Content = "IP: " + newBalance.Ip + " ∙ RP: " + newBalance.Rp;
                    LoginPacket.IpBalance = newBalance.Ip;
                    LoginPacket.RpBalance = newBalance.Rp;
                }
                else
                {
                    var gameNotification = message.Body as GameNotification;
                    if (gameNotification != null)
                    {
                        GameNotification notification = gameNotification;
                        var messageOver = new MessageOverlay { MessageTitle = { Content = notification.Type } };
                        switch (notification.Type)
                        {
                            case "PLAYER_BANNED_FROM_GAME":
                                messageOver.MessageTitle.Content = "Banned from custom game";
                                messageOver.MessageTextBox.Text = "You have been banned from this custom game!";
                                break;
                            case "PLAYER_QUIT":
                                messageOver.MessageTitle.Content = "Player quit";
                                var name = await RiotCalls.GetSummonerNames(new[] { Convert.ToDouble(notification.MessageArgument) });
                                messageOver.MessageTextBox.Text = name[0] + " quit from queue!";
                                break;
                            default:
                                messageOver.MessageTextBox.Text = notification.MessageCode + Environment.NewLine;
                                messageOver.MessageTextBox.Text = System.Convert.ToString(notification.MessageArgument);
                                break;
                        }
                        OverlayContainer.Content = messageOver.Content;
                        OverlayContainer.Visibility = Visibility.Visible;
                        ClearPage(typeof(CustomGameLobbyPage));
                        if (notification.Type != "PLAYER_QUIT")
                            SwitchPage(Client.MainPage);
                    }
                    else if (message.Body is EndOfGameStats)
                    {
                        var stats = (EndOfGameStats)message.Body;
                        var EndOfGame = new EndOfGamePage(stats);
                        ClearPage(typeof(TeamQueuePage));
                        OverlayContainer.Visibility = Visibility.Visible;
                        OverlayContainer.Content = EndOfGame.Content;
                    }
                    else if (message.Body is StoreFulfillmentNotification)
                    {
                        PlayerChampions = await RiotCalls.GetAvailableChampions();
                    }
                    else if (message.Body is Inviter)
                    {
                        var stats = (Inviter)message.Body;
                        //CurrentInviter = stats;
                    }
                    else if (message.Body is InvitationRequest)
                    {
                        var stats = (InvitationRequest)message.Body;
                        if (stats.Inviter == null)
                            return;


                        if (InviteData.ContainsKey(stats.InvitationId))
                            return;
                        InviteInfo x = InviteData[stats.InvitationId];

                        if (x.Inviter != null)
                            return;
                        
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
                        MainWin.FlashWindow();
                    }
                    else if (message.Body is ClientLoginKickNotification)
                    {
                        var kick = (ClientLoginKickNotification)message.Body;
                        if (kick.sessionToken == null)
                            return;

                        Warning Warn = new Warning
                        {
                            Header = { Content = "Kicked from server" },
                            MessageText = { Text = "This account has been logged in from another location" }
                        };
                        Warn.backtochampselect.Click += (MainWin as MainWindow).LogoutButton_Click;
                        Warn.AcceptButton.Click += QuitClient;
                        Warn.hide.Visibility = Visibility.Hidden;
                        Warn.backtochampselect.Content = "Logout(Work in progress)";
                        Warn.AcceptButton.Content = "Quit";
                        FullNotificationOverlayContainer.Content = Warn.Content;
                        FullNotificationOverlayContainer.Visibility = Visibility.Visible;
                    }
                    else if (message.Body is SimpleDialogMessage)
                    {
                        var leagueInfo = message.Body as SimpleDialogMessage;
                        if (leagueInfo.Type == "leagues")
                        {
                            var promote = LeaguePromote.LeaguesPromote(leagueInfo.Params.ToString());
                            var messageOver = new MessageOverlay();
                            messageOver.MessageTitle.Content = "Leagues updated";
                            messageOver.MessageTextBox.Text = promote.leagueItem.PlayerOrTeamName + " have been promoted to " + promote.leagueItem.Rank;
                            var response = new SimpleDialogMessageResponse
                            {
                                Command = "ack",
                                AccountId = leagueInfo.AccountId,
                                MessageId = leagueInfo.MessageId
                            };
                            messageOver.AcceptButton.Click += (o, e) => { RiotCalls.CallPersistenceMessaging(response); };
                        }
                    }
                }
            }));
        }

        private static void QuitClient(object sender, RoutedEventArgs e)
        {
            if (IsLoggedIn)
            {
                RiotCalls.PurgeFromQueues();
                RiotCalls.Leave();
                Client.RiotConnection.Close();
            }
            Environment.Exit(0);

        }

        internal static Dictionary<string, string> queueNames;

        internal static string InternalQueueToPretty(string internalQueue)
        {
            if (queueNames == null)
            {
                using (WebClient client = new WebClient())
                {
                    string names = "";
                    try
                    {
                        names = client.DownloadString("http://legendaryclient.net/QueueName");
                    }
                    catch
                    {
                        //Try to download from Github
                        names = client.DownloadString("https://raw.githubusercontent.com/LegendaryClient/LegendaryClient/gh-pages/QueueName");
                    }
                    string[] queues = names.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    queueNames = queues.Select(x => x.Split('|')).ToDictionary(x => x[0], x => x[1]);
                }
            }
            if (queueNames.ContainsKey(internalQueue))
                return queueNames[internalQueue];

            Log(internalQueue);
            return internalQueue;
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
            var t = new Timer
            {
                Interval = 5000,
            };
            t.Tick += (o, m) =>
            {
                if (Region.Garena)
                    return;
                GameScouter scouter = new GameScouter();
                scouter.LoadScouter(LoginPacket.AllSummonerData.Summoner.Name);
                scouter.Show();
                scouter.Activate();
                t.Stop();
            };
            t.Start();
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
                    Arguments = "\"8394\" \"LoLLauncher.exe\" \"\" \"spectator "
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
                    OnMessage -= (OnMessageHandler)d;

            FixChampSelect();
            FixLobby();
            IsInGame = false;

            await RiotCalls.QuitGame();
            StatusGrid.Visibility = Visibility.Hidden;
            PlayButton.Visibility = Visibility.Visible;
            LobbyContent = null;
            LastPageContent = null;
            GameStatus = "outOfGame";
            SetChatHover();
            SwitchPage(Client.MainPage);
        }

        internal static void FixLobby()
        {

        }

        internal static void FixChampSelect()
        {

        }

        #endregion League Of Legends Logic

        internal static StatusPage statusPage;
        internal static FriendList FriendList;
        internal static NotificationPage notificationPage;
        internal static MetroWindow MainWin;
        internal static bool GroupIsShown;
        internal static bool PlayerChatIsShown;
        internal static Page TrueCurrentPage;
        internal static bool Garena = false;
        internal static string[] args;

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

        public static string TitleCaseString(string s)
        {
            if (s == null) return s;

            string[] words = s.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length == 0)
                    continue;

                char firstChar = char.ToUpper(words[i][0]);
                string rest = string.Empty;
                if (words[i].Length > 1)
                    rest = words[i].Substring(1).ToLower();

                words[i] = firstChar + rest;
            }

            return string.Join(" ", words);
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
            dtDateTime = dtDateTime.AddSeconds(Math.Round(javaTimeStamp / 1000)).ToLocalTime();
            return dtDateTime;
        }

        /// <summary>
        ///     Add data to the log file
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="type"></param>
        public static void Log(string lines, string type = "LOG")
        {
            WriteToLog.Log(lines, type);

            if (Pipe)
                return;
            try
            {
                if (SendPIPE == null)
                    return;
                SendPIPE.WriteString("[" + type + "] " + lines);
            }
            catch { }
        }

        public static void Log(Exception e)
        {
            Log(e.Message, "Exception");
        }

        //Get Image
        public static BitmapImage GetImage(string address)
        {
            var UriSource = new System.Uri(address, UriKind.RelativeOrAbsolute);
            if (File.Exists(address) || address.StartsWith("/LegendaryClient;component"))
                return new BitmapImage(UriSource);

            Log("Cannot find " + address, "WARN");
            UriSource = new System.Uri("/LegendaryClient;component/NONE.png", UriKind.RelativeOrAbsolute);

            return new BitmapImage(UriSource);
        }
        #endregion Public Helper Methods

        public static Accent CurrentAccent { get; set; }

        internal static void XmppConnection_OnPresence(object sender, Presence pres)
        {
            if (pres.GetAttribute("InnerText") == string.Empty)
            {
                if (presenceStatus != ShowType.NONE)
                    XmppConnection.Send(new Presence(presenceStatus, GetPresence(), 0) { Type = PresenceType.available });
                else
                    XmppConnection.Send(new Presence(presenceStatus, GetPresence(), 0) { Type = PresenceType.invisible });
            }
        }

        internal static string EncryptStringAES(this string input, string secret)
        {
            string output = string.Empty;
            var aesAlg = new RijndaelManaged();
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(secret))
                return output;

            try
            {
                // generate the key from the shared secret and the salt
                var key = new Rfc2898DeriveBytes(secret, Encoding.ASCII.GetBytes("o6806642kbM7c5"));
                // Create a RijndaelManaged object
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

                // Create a decryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (var msEncrypt = new MemoryStream())
                {
                    // prepend the IV
                    msEncrypt.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int));
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
            string output = string.Empty;
            var aesAlg = new RijndaelManaged();
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(Secret))
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
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
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
            var rawLength = new byte[sizeof(int)];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
                throw new SystemException("Stream did not contain properly formatted byte array");

            var buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
                throw new SystemException("Did not read byte array properly");

            return buffer;
        }
        public static bool Pipe = false;

        internal static int SelectChamp;
        internal static bool usingInstaPick = false;

        internal static KeyValuePair<string, string> userpass;
        internal static bool HasPopped = false;

        public static string UpdateRegion { get; set; }

        public static string GameType { get; set; }

        public static Dictionary<string, string> LocalRunePages = new Dictionary<string, string>();

        public static string GameClientVersion;

        public static ProfilePage Profile;

        public static MainPage MainPage;

        public static GameQueueConfig[] Queues;

        public static int MathRound(this double toRound)
        {
            return (int)Math.Round(toRound);
        }

        public static bool isConnectedToRTMP = true;

        public static string reconnectToken { get; set; }

        public static async void RiotConnection_Disconnected(object sender, EventArgs e)
        {
            isConnectedToRTMP = false;
            Log("Disconnected from RTMPS");
            if (connectionCheck == null)
            {
                connectionCheck = new Thread(CheckInternetConnection) { IsBackground = true };
                connectionCheck.Start();
            }
            else if (!connectionCheck.IsAlive)
            {
                connectionCheck = new Thread(CheckInternetConnection) { IsBackground = true };
                connectionCheck.Start();
            }

            while (!isInternetAvailable)
                Task.Delay(100);

            await RiotConnection.RecreateConnection(reconnectToken);
            //await RiotCalls.Login(reconnectToken);
            var str1 = string.Format("gn-{0}", PlayerSession.AccountSummary.AccountId);
            var str2 = string.Format("cn-{0}", PlayerSession.AccountSummary.AccountId);
            var str3 = string.Format("bc-{0}", PlayerSession.AccountSummary.AccountId);
            Task<bool>[] taskArray = { RiotConnection.SubscribeAsync("my-rtmps", "messagingDestination", str1, str1), 
                                       RiotConnection.SubscribeAsync("my-rtmps", "messagingDestination", str2, str2), 
                                       RiotConnection.SubscribeAsync("my-rtmps", "messagingDestination", "bc", str3) };

            await Task.WhenAll(taskArray);
            isConnectedToRTMP = true;
        }
        public static bool isInternetAvailable;

        public static void CheckInternetConnection()
        {
            isInternetAvailable = false;
            while (isInternetAvailable == false)
            {
                try
                {
                    Ping myPing = new Ping();
                    String host = "8.8.8.8";
                    byte[] buffer = new byte[32];
                    int timeout = 1000;
                    PingOptions pingOptions = new PingOptions();
                    PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                    isInternetAvailable = (reply.Status == IPStatus.Success);
                }
                catch (Exception)
                {
                    isInternetAvailable = false;
                }
            }
        }
        public static Thread connectionCheck;

        internal static void XmppConnection_OnError(object sender, Exception ex)
        {
            Client.Log("Error with chat connection");
            if (connectionCheck == null)
            {
                connectionCheck = new Thread(CheckInternetConnection) { IsBackground = true };
                connectionCheck.Start();
            }
            else if (!connectionCheck.IsAlive)
            {
                connectionCheck = new Thread(CheckInternetConnection) { IsBackground = true };
                connectionCheck.Start();
            }

            while (!isInternetAvailable)
                Task.Delay(100);

            XmppConnection.Open();
            SetChatHover();
        }

        public static string[] ChatAutoBlock { get; set; }
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

    public class Groups
    {
        public string priority { get; set; }
        public string text { get; set; }
    }

    public class JsonItems
    {
        public string subscription { get; set; }
        public string Jid { get; set; }
        public string name { get; set; }
        public string xmlns { get; set; }
        public string note { get; set; }
        public Groups group { get; set; }
    }

    public class RootObject
    {
        public JsonItems item { get; set; }
    }

    public class JsonItems2
    {
        public string subscription { get; set; }
        public string Jid { get; set; }
        public string name { get; set; }
        public string xmlns { get; set; }
        public string note { get; set; }
        public string group { get; set; }
    }

    public class RootObject2
    {
        public JsonItems2 item { get; set; }
    }
}
