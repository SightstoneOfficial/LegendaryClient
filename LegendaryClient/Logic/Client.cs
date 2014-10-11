using jabber.client;
using jabber.connection;
using jabber.protocol.client;
using LegendaryClient.Controls;
using LegendaryClient.Logic.Region;
using LegendaryClient.Logic.Replays;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Windows;
using PVPNetConnect;
using PVPNetConnect.RiotObjects.Gameinvite.Contract;
using PVPNetConnect.RiotObjects.Platform.Catalog.Champion;
using PVPNetConnect.RiotObjects.Platform.Clientfacade.Domain;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Game.Message;
using PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract;
using PVPNetConnect.RiotObjects.Platform.Login;
using PVPNetConnect.RiotObjects.Platform.Messaging;
using PVPNetConnect.RiotObjects.Platform.Statistics;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using log4net;
//using LegendaryClient.Logic.AutoReplayRecorder;

namespace LegendaryClient.Logic
{
    /// <summary>
    /// Any logic that needs to be reused over multiple pages
    /// </summary>
    internal static class Client
    {

        /// <summary>
        /// Use this to play sounds
        /// </summary>
        internal static MediaElement SoundPlayer;

        /// <summary>
        /// Use this to play sounds in the background only
        /// </summary>
        internal static MediaElement AmbientSoundPlayer;

        private static readonly ILog log = LogManager.GetLogger(typeof(Client));

        /// <summary>
        /// Timer used so replays won't start right away
        /// </summary>
        internal static System.Windows.Forms.Timer ReplayTimer;

        internal static bool isOwnerOfGame = false;

        internal static double QueueId = 0;

        /// <summary>
        /// The database of all runes
        /// </summary>
        internal static List<runes> Runes;

        /// <summary>
        /// Retreives UpdateDate For LegendaryClient
        /// </summary>
        internal static List<UpdateData> updateData;

        /// <summary>
        /// Stuff
        /// </summary>
        internal static string LegendaryClientVersion = "2.0.0.0";

        /// <summary>
        /// Button For Lobby
        /// </summary>
        internal static Button LobbyButton;

        /// <summary>
        /// Update Data
        /// </summary>
        internal static int LegendaryClientReleaseNumber = 3;

        /// <summary>
        /// Sets Sqlite Version
        /// Like the language pack
        /// </summary>
        internal static string sqlite = "gameStats_en_US.sqlite";
        //internal static string sqlite = "gameStats_ko_KR.sqlite";
        
        /// <summary>
        /// Latest champion for League of Legends login screen
        /// </summary>
        internal const int LatestChamp = 103;

        /// <summary>
        /// Latest version of League of Legends. Retrieved from ClientLibCommon.dat
        /// </summary>
        internal static string Version = "4.12.2";

        ///<summary>
        /// To see if the user is a dev
        /// </summary>
        internal static bool Dev = false;

        /// <summary>
        /// The current directory the client is running from
        /// </summary>
        internal static string ExecutingDirectory = "";

        /// <summary>
        /// Riot's database with all the client data
        /// </summary>
        internal static SQLiteConnection SQLiteDatabase;

        /// <summary>
        /// Fix for champ select. Do not use this!
        /// </summary>
        internal static event PVPNetConnection.OnMessageReceivedHandler OnFixChampSelect;

        /// <summary>
        /// Allow lobby to still have a connection. Do not use this!
        /// </summary>
        internal static event PVPNetConnection.OnMessageReceivedHandler OnFixLobby;

        /// <summary>
        /// The database of all the champions
        /// </summary>
        internal static List<champions> Champions;

        /// <summary>
        /// The database of all the champion abilities
        /// </summary>
        internal static List<championAbilities> ChampionAbilities;

        /// <summary>
        /// The database of all the champion skins
        /// </summary>
        internal static List<championSkins> ChampionSkins;

        /// <summary>
        /// The database of all the items
        /// </summary>
        internal static List<items> Items;

        /// <summary>
        /// The database of all masteries
        /// </summary>
        internal static List<masteries> Masteries;

        /// <summary>
        /// The Invite Data
        /// </summary>
        internal static List<invitationRequest> InviteJsonRequest = new List<invitationRequest>();

        /// <summary>
        /// The database of all the search tags
        /// </summary>
        internal static List<championSearchTags> SearchTags;

        /// <summary>
        /// The database of all the keybinding defaults & proper names
        /// </summary>
        internal static List<keybindingEvents> Keybinds;

        internal static ChampionDTO[] PlayerChampions;

        internal static AutoReplayRecorder Autorecorder;

        internal static List<string> Whitelist = new List<string>();

        #region Chat

        internal static JabberClient ChatClient;
        //Fix for invitations
        public delegate void OnMessageHandler(object sender, jabber.protocol.client.Message e);
        public static event OnMessageHandler OnMessage;

        internal static PresenceType _CurrentPresence;

        internal static PresenceType CurrentPresence
        {
            get { return _CurrentPresence; }
            set
            {
                if (_CurrentPresence != value)
                {
                    _CurrentPresence = value;
                    if (ChatClient != null)
                    {
                        if (ChatClient.IsAuthenticated)
                        {
                            ChatClientConnect(null);
                        }
                    }
                }
            }
        }

        internal static string _CurrentStatus;

        internal static string CurrentStatus
        {
            get { return _CurrentStatus; }
            set
            {
                if (_CurrentStatus != value)
                {
                    _CurrentStatus = value;
                    if (ChatClient != null)
                    {
                        if (ChatClient.IsAuthenticated)
                        {
                            ChatClientConnect(null);
                        }
                    }
                }
            }
        }

        internal static RosterManager RostManager;
        internal static PresenceManager PresManager;
        internal static ConferenceManager ConfManager;
        internal static bool UpdatePlayers = true;

        internal static Dictionary<string, ChatPlayerItem> AllPlayers = new Dictionary<string, ChatPlayerItem>();

        internal static bool ChatClient_OnInvalidCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        internal static void ChatClient_OnMessage(object sender, jabber.protocol.client.Message msg)
        {
            if (msg.Subject != null)
            {
                MainWin.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    ChatSubjects subject = (ChatSubjects) Enum.Parse(typeof(ChatSubjects), msg.Subject, true);
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

        internal async static void GameInvite(object sender, PVPNetConnection PVPConnect, string GameID)
        {
            await PVPConnect.Accept(GameID);
        }

        internal static void ChatClientConnect(object sender)
        {
            SetChatHover();
        }

        internal static void SendMessage(string User, string Message)
        {
            ChatClient.Message(User, Message);
        }

        internal static void SetChatHover()
        {
            ChatClient.Presence(CurrentPresence, GetPresence(), null, 0);
        }

        internal static bool hidelegendaryaddition;

        internal static string LegendaryClientAddition = "∟";
        internal static void NewStatus()
        {
            if (Dev == false)
            {
                Client.LegendaryClientAddition = CurrentStatus + "∟";
            }
            else if (Dev == true)
            {
                Client.LegendaryClientAddition = CurrentStatus + "♒";
            }
            else if (hidelegendaryaddition == true)
            {
                Client.LegendaryClientAddition = CurrentStatus;
            }
        }

        internal static string GetPresence()
        {
            NewStatus();
            return "<body>" +
                  "<profileIcon>" + LoginPacket.AllSummonerData.Summoner.ProfileIconId + "</profileIcon>" +
                  "<level>" + LoginPacket.AllSummonerData.SummonerLevel.Level + "</level>" +
                  "<wins>" + AmountOfWins + "</wins>" +
                  (IsRanked ?
                  "<queueType /><rankedLosses>0</rankedLosses><rankedRating>0</rankedRating><tier>UNRANKED</tier>" + //Unused?
                  "<rankedLeagueName>" + LeagueName + "</rankedLeagueName>" +
                  "<rankedLeagueDivision>" + Tier + "</rankedLeagueDivision>" +
                  "<rankedLeagueTier>" + TierName + "</rankedLeagueTier>" +
                  "<rankedLeagueQueue>RANKED_SOLO_5x5</rankedLeagueQueue>" +
                  "<rankedWins>" + AmountOfWins + "</rankedWins>" : "") +
                  "<gameStatus>" + GameStatus + "</gameStatus>" +
                  "<statusMsg>" + LegendaryClientAddition + "</statusMsg>" + 
                  //Look for "∟" to recognize LegendaryClient Users
                  //Look for "♒" to recongnize Devs
                    "</body>";
        }

        

        internal static void RostManager_OnRosterItem(object sender, jabber.protocol.iq.Item ri)
        {
            UpdatePlayers = true;
            if (!AllPlayers.ContainsKey(ri.JID.User))
            {
                ChatPlayerItem player = new ChatPlayerItem();
                player.Id = ri.JID.User;
                player.Username = ri.Nickname;
                bool PlayerPresence = PresManager.IsAvailable(ri.JID);
                AllPlayers.Add(ri.JID.User, player);
            }
        }

        internal static void PresManager_OnPrimarySessionChange(object sender, jabber.JID bare)
        {
            jabber.protocol.client.Presence[] s = Client.PresManager.GetAll(bare);
            if (s.Length == 0)
                return;
            string Presence = s[0].Status;
            if (Presence == null)
                return;
            Debug.WriteLine(Presence);
            if (Client.AllPlayers.ContainsKey(bare.User))
            {
                UpdatePlayers = true;
                ChatPlayerItem Player = Client.AllPlayers[bare.User];
                using (XmlReader reader = XmlReader.Create(new StringReader(Presence)))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
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
                                    if (Player.Status.EndsWith("∟"))
                                    {
                                        Player.UsingLegendary = true;
                                    }
                                    else if (Player.Status.EndsWith("♒"))
                                    {
                                        Player.IsLegendaryDev = true;
                                        Player.UsingLegendary = true;
                                    }
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
                }
                if (String.IsNullOrWhiteSpace(Player.Status))
                {
                    Player.Status = "Online";
                }
            }
        }

        internal static void Message(string To, string Message, ChatSubjects Subject)
        {
            Message msg = new Message(Client.ChatClient.Document);
            msg.Type = MessageType.normal;
            msg.To = To + "@pvp.net";
            msg.Subject = ((ChatSubjects)Subject).ToString();
            msg.Body = Message;
            Client.ChatClient.Write(msg);
        }

        //Why do you even have to do this, riot?
        internal static string GetObfuscatedChatroomName(string Subject, string Type)
        {
            int bitHack = 0;
            byte[] data = System.Text.Encoding.UTF8.GetBytes(Subject);
            byte[] result;
            SHA1 sha = new SHA1CryptoServiceProvider();
            result = sha.ComputeHash(data);
            string obfuscatedName = "";
            int incrementValue = 0;
            while (incrementValue < result.Length)
            {
                bitHack = result[incrementValue];
                obfuscatedName = obfuscatedName + Convert.ToString(((uint)(bitHack & 240) >> 4), 16);
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

        #endregion Chat

        internal static Grid MainGrid;
        internal static Grid NotificationGrid;
        internal static Grid StatusGrid;

        internal static Label StatusLabel;
        internal static Label InfoLabel;
        internal static ContentControl OverlayContainer;
        internal static Button PlayButton;
        internal static Button HideTeamQueuePage;
        internal static ContentControl ChatContainer;
        internal static ContentControl StatusContainer;
        internal static ContentControl NotificationOverlayContainer;
        internal static ContentControl NotificationContainer;
        internal static ListView ChatListView;
        internal static ChatItem ChatItem;

        internal static ListView InviteListView;
        internal static Image MainPageProfileImage;

        #region WPF Tab Change

        /// <summary>
        /// The container that contains the page to display
        /// </summary>
        internal static ContentControl Container;

        /// <summary>
        /// Page cache to stop having to recreate all information if pages are overwritted
        /// </summary>
        internal static List<Page> Pages;

        internal static bool IsOnPlayPage = false;

        /// <summary>
        /// Switches the contents of the frame to the requested page. Also sets background on
        /// the button on the top to show what section you are currently on.
        /// </summary>
        internal static void SwitchPage(Page page)
        {
            IsOnPlayPage = page is PlayPage;
            foreach (Page p in Pages) //Cache pages
            {
                if (p.GetType() == page.GetType())
                {
                    Container.Content = p.Content;
                    return;
                }
            }
            Container.Content = page.Content;
            if (!(page is FakePage))
                Pages.Add(page);
        }

        /// <summary>
        /// Clears the cache of a certain page if not used anymore
        /// </summary>
        internal static void ClearPage(Page page)
        {
            foreach (Page p in Pages.ToArray())
            {
                if (p.GetType() == page.GetType())
                {
                    Pages.Remove(p);
                    return;
                }
            }
        }

        #endregion WPF Tab Change

        

        #region League Of Legends Logic

        /// <summary>
        /// Main connection to the League of Legends server
        /// </summary>
        internal static PVPNetConnection PVPNet;

        /// <summary>
        /// Packet recieved when initially logged on. Cached so the packet doesn't
        /// need to requested multiple times, causing slowdowns
        /// </summary>
        internal static LoginDataPacket LoginPacket;

        /// <summary>
        /// All enabled game configurations for the user
        /// </summary>
        internal static List<GameTypeConfigDTO> GameConfigs;

        /// <summary>
        /// The region the user is connecting to
        /// </summary>
        internal static BaseRegion Region;

        /// <summary>
        /// The region the user is connecting to
        /// </summary>
        internal static bool Chunk;

        /// <summary>
        /// Is the client logged in to the League of Legends server
        /// </summary>
        internal static bool IsLoggedIn = false;

        /// <summary>
        /// Is the player in game at the moment
        /// </summary>
        internal static bool InGame = false;

        /// <summary>
        /// GameID of the current game that the client is connected to
        /// </summary>
        internal static double GameID = 0;

        /// <summary>
        /// 
        /// </summary>
        internal static int GameQueue;

        /// <summary>
        /// Game Name of the current game that the client is connected to
        /// </summary>
        internal static string GameName = "";

        /// <summary>
        /// The DTO of the game lobby when connected to a custom game
        /// </summary>
        internal static GameDTO GameLobbyDTO;

        /// <summary>
        /// A recorder
        /// </summary>
        internal static ReplayRecorder recorder;

        /// <summary>
        /// When going into champion select reuse the last DTO to set up data
        /// </summary>
        internal static GameDTO ChampSelectDTO;

        /// <summary>
        /// When connected to a game retrieve details to connect to
        /// </summary>
        internal static PlayerCredentialsDto CurrentGame;

        internal static Session PlayerSession;

        internal static bool AutoAcceptQueue = false;
        internal static object LobbyContent;
        internal static object LastPageContent;
        internal static bool IsInGame = false;
        internal static bool RunonePop = false;

        /// <summary>
        /// When an error occurs while connected. Currently un-used
        /// </summary>
        /// 


        internal static void PVPNet_OnError(object sender, PVPNetConnect.Error error)
        {
            ;
        }

        internal static System.Timers.Timer HeartbeatTimer;
        internal static int HeartbeatCount;

        internal static void StartHeartbeat()
        {
            HeartbeatTimer = new System.Timers.Timer();
            HeartbeatTimer.Elapsed += new ElapsedEventHandler(DoHeartbeat);
            HeartbeatTimer.Interval = 120000; // in milliseconds
            HeartbeatTimer.Start();
        }

        internal async static void DoHeartbeat(object sender, ElapsedEventArgs e)
        {
            if (IsLoggedIn)
            {
                //string LCDSHeartBeatString = Convert.ToInt32(LoginPacket.AllSummonerData.Summoner.AcctId) + "|" + PlayerSession.Token, HeartbeatCount + "|" + DateTime.Now.ToString("ddd MMM d yyyy HH:mm:ss 'GMT'KKKK");
                Client.Log("Preforming LCDSHeartBeat");
                await PVPNet.PerformLCDSHeartBeat(Convert.ToInt32(LoginPacket.AllSummonerData.Summoner.AcctId), PlayerSession.Token, HeartbeatCount, DateTime.Now.ToString("ddd MMM d yyyy HH:mm:ss 'GMT'KKKK"));

                HeartbeatCount++;
            }
        }

        //internal static Inviter CurrentInviter;

        internal static void OnMessageReceived(object sender, object message)
        {
            MainWin.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async () =>
            {
                if (message is StoreAccountBalanceNotification)
                {
                    StoreAccountBalanceNotification newBalance = (StoreAccountBalanceNotification)message;
                    InfoLabel.Content = "IP: " + newBalance.Ip + " ∙ RP: " + newBalance.Rp;
                    Client.LoginPacket.IpBalance = newBalance.Ip;
                    Client.LoginPacket.RpBalance = newBalance.Rp;
                }
                else if (message is GameNotification)
                {
                    GameNotification notification = (GameNotification)message;
                    MessageOverlay messageOver = new MessageOverlay();
                    messageOver.MessageTitle.Content = notification.Type;
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
                    Client.OverlayContainer.Content = messageOver.Content;
                    Client.OverlayContainer.Visibility = Visibility.Visible;
                    Client.ClearPage(new CustomGameLobbyPage());
                    Client.SwitchPage(new MainPage());
                }
                else if (message is EndOfGameStats)
                {
                    EndOfGameStats stats = message as EndOfGameStats;
                    EndOfGamePage EndOfGame = new EndOfGamePage(stats);
                    Client.OverlayContainer.Visibility = Visibility.Visible;
                    Client.OverlayContainer.Content = EndOfGame.Content;
                }
                else if (message is StoreFulfillmentNotification)
                {
                    PlayerChampions = await PVPNet.GetAvailableChampions();
                }
                else if (message is Inviter)
                {
                    Inviter stats = message as Inviter;
                    //CurrentInviter = stats;
                }
                else if (message is InvitationRequest)
                {
                    InvitationRequest stats = message as InvitationRequest;
                    Inviter Inviterstats = message as Inviter;
                    //TypedObject body = (TypedObject)to["body"];
                    MainWin.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        //Gameinvite stuff
                        GameInvitePopup pop = new GameInvitePopup(stats);
                        //await Invite.Callback;
                        //Invite.InvitationRequest(body);
                        pop.HorizontalAlignment = HorizontalAlignment.Right;
                        pop.VerticalAlignment = VerticalAlignment.Bottom;
                        pop.Height = 230;
                        Client.NotificationGrid.Children.Add(pop);
                        //Client.InviteJsonRequest = LegendaryClient.Logic.JSON.InvitationRequest.PopulateGameInviteJson();
                        //message.GetType() == typeof(GameInvitePopup)
                    }));
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

                default:
                    return InternalQueue;
            }
        }

        private static string convert(string QueueName)
        {
            string result = string.Empty;
            string Queueinternal = "";
            string Bots = "";
            string Players = "";
            string Extra = "";
            var start = QueueName.Replace("matching-queue-", "").Replace("-game-queue","");
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
            else
            {

            }
            if(!string.IsNullOrEmpty(Extra))
                Extra= "(" + Extra + ")";
            result = string.Format("{0}{1} {2} {3}", Queueinternal, Bots, Players, Extra);
            return result;
        }

        internal static string GetGameDirectory()
        {
            string Directory = Path.Combine(RootLocation, "RADS", "projects", "lol_game_client", "releases");

            DirectoryInfo dInfo = new DirectoryInfo(Directory);
            DirectoryInfo[] subdirs = null;
            try
            {
                subdirs = dInfo.GetDirectories();
            }
            catch { return "0.0.0"; }
            string latestVersion = "0.0.1";
            foreach (DirectoryInfo info in subdirs)
            {
                latestVersion = info.Name;
            }

            Directory = Path.Combine(Directory, latestVersion, "deploy");

            return Directory;
        }


        private static int counter;

        internal static string Location;
        internal static string LoLLauncherLocation;
        internal static string LOLCLIENTVERSION;
        internal static string RootLocation;
        internal static void LaunchGame()
        {
            string GameDirectory = Location;

            var p = new System.Diagnostics.Process();
            p.StartInfo.WorkingDirectory = GameDirectory;
            p.StartInfo.FileName = Path.Combine(GameDirectory, "League of Legends.exe");
            //"8394" "LoLLauncher.exe" "" "127.0.0.1 5119 17BLOhi6KZsTtldTsizvHg== 47917791"
            p.StartInfo.Arguments = "\"8394\" \"" + RootLocation + "LoLLauncher.exe" + "\" \"" + "\" \"" +
                CurrentGame.ServerIp + " " +
                CurrentGame.ServerPort + " " +
                CurrentGame.EncryptionKey + " " +
                CurrentGame.SummonerId + "\"";
            p.Start();

            ReplayTimer = new System.Windows.Forms.Timer();
            ReplayTimer.Tick += new EventHandler(CountdownTimer_Tick);
            ReplayTimer.Interval = 10000; // 10 seconds
            ReplayTimer.Start();
        }
        

        private static void CountdownTimer_Tick(object sender, EventArgs e)
        {
            string ObserverServerIp;
            double GameId;
            string InternalName;
            string ObserverEncryptionKey;

            PlayerCredentialsDto replaydata = new PlayerCredentialsDto();
            ObserverServerIp = replaydata.ObserverServerIp;
            GameId = replaydata.GameId;
            InternalName =Region.InternalName;
            ObserverEncryptionKey = replaydata.ObserverEncryptionKey;
            if (ReplayTimer.Interval == 0)
            {
                ReplayTimer.Stop();
                Autorecorder = new LegendaryClient.Logic.Replays.AutoReplayRecorder(ObserverServerIp, GameId, InternalName, ObserverEncryptionKey);
            }
        }

        internal static void LaunchSpectatorGame(string SpectatorServer, string Key, int GameId, string Platform)
        {
            string GameDirectory = GetGameDirectory();

            var p = new System.Diagnostics.Process();
            p.StartInfo.WorkingDirectory = GameDirectory;
            p.StartInfo.FileName = Path.Combine(GameDirectory, "League of Legends.exe");
            //p.StartInfo.FileName = Location;
            p.StartInfo.Arguments = "\"8393\" \"LoLLauncher.exe\" \"\" \"spectator "
                + SpectatorServer + " "
                + Key + " "
                + GameId + " "
                + Platform + "\"";
            p.Start();
            
        }

        internal async static void QuitCurrentGame()
        {
            if (OnMessage != null)
            {
                foreach (Delegate d in OnMessage.GetInvocationList())
                {
                    OnMessage -= (OnMessageHandler)d;
                }
            }

            FixChampSelect();
            FixLobby();
            IsInGame = false;

            await PVPNet.QuitGame();
            StatusGrid.Visibility = System.Windows.Visibility.Hidden;
            PlayButton.Visibility = System.Windows.Visibility.Visible;
            LobbyContent = null;
            LastPageContent = null;
            GameStatus = "outOfGame";
            SetChatHover();
            SwitchPage(new MainPage());
        }

        internal static void FixLobby()
        {
            if (OnFixLobby != null)
            {
                foreach (Delegate d in OnFixLobby.GetInvocationList())
                {
                    PVPNet.OnMessageReceived -= (PVPNetConnection.OnMessageReceivedHandler)d;
                    OnFixLobby -= (PVPNetConnection.OnMessageReceivedHandler)d;
                }
            }
        }

        internal static void FixChampSelect()
        {
            if (OnFixChampSelect != null)
            {
                foreach (Delegate d in OnFixChampSelect.GetInvocationList())
                {
                    PVPNet.OnMessageReceived -= (PVPNetConnection.OnMessageReceivedHandler)d;
                    OnFixChampSelect -= (PVPNetConnection.OnMessageReceivedHandler)d;
                }
            }
        }

        #endregion League Of Legends Logic

        internal static MainWindow MainWin;

        #region Public Helper Methods
        internal static void FocusClient()
        {
            if (MainWin.WindowState == WindowState.Minimized)
            {
                MainWin.WindowState = WindowState.Normal;
            }

            MainWin.Activate();
            MainWin.Topmost = true;  // important
            MainWin.Topmost = false; // important
            MainWin.Focus();         // important
        }

        public static String TitleCaseString(String s)
        {
            if (s == null) return s;

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

        public static BitmapSource ToWpfBitmap(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);

                stream.Position = 0;
                BitmapImage result = new BitmapImage();
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
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(Math.Round(javaTimeStamp / 1000)).ToLocalTime();
            return dtDateTime;
        }
        /// <summary>
        /// Add data to the log file
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="type"></param>
        public static void Log(String lines, String type = "LOG")
        {
            LCLog.WriteToLog.Log(lines, type);
        }

        //Get Image
        public static BitmapImage GetImage(string Address)
        {
            Uri UriSource = new Uri(Address, UriKind.RelativeOrAbsolute);
            if (!File.Exists(Address) && !Address.StartsWith("/LegendaryClient;component"))
            {
                Log("Cannot find " + Address, "WARN");
                UriSource = new Uri("/LegendaryClient;component/NONE.png", UriKind.RelativeOrAbsolute);
            }
            return new BitmapImage(UriSource);
        }
        #endregion Public Helper Methods

        
    }



    public class ChatPlayerItem
    {
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

        public List<string> Messages = new List<string>();
    }

}