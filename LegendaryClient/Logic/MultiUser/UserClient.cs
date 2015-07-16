using LCLog;
using LegendaryClient.Controls;
using LegendaryClient.Logic.JSON;
using LegendaryClient.Logic.Region;
using LegendaryClient.Logic.Replays;
using LegendaryClient.Windows;
using MahApps.Metro;
using MahApps.Metro.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using System.Xml;
using LegendaryClient.Logic.Riot.Platform;
using RtmpSharp.Net;
using LegendaryClient.Logic.Riot;
using LegendaryClient.Logic.Riot.Platform.Messaging.Persistence;
using RtmpSharp.Messaging;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.roster;
using agsXMPP;
using LegendaryClient.Logic.SQLite;


namespace LegendaryClient.Logic.MultiUser
{

    public class UserClient
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
        public event OnMessageReceivedPy onChatMessageReceived;

		public delegate void OnAccept(bool accept);
        public event OnAccept PlayerAccepedQueue;

        public RiotCalls calls;

        public void SendAccept(bool accept)
        {
            if (PlayerAccepedQueue != null)
                PlayerAccepedQueue(accept);
        }

        internal string UID;

        public bool InstaCall = false;
        public string CallString = string.Empty;

        internal bool Filter = true;
        
        internal bool Garena = false;

        internal bool isOwnerOfGame = false;

        internal double QueueId = 0;

        internal UserAccount userAccount;

        internal agsXMPP.XmppClientConnection XmppConnection;


        /// <summary>
        ///     If Player is creating an account
        /// </summary>
        internal bool done = true;
                
        /// <summary>
        ///     To see if the user is a dev
        /// </summary>
        internal bool Dev = false;
        
                /// <summary>
        ///     All of players who have been invited
        /// </summary>
        internal Dictionary<string, InviteInfo> InviteData = new Dictionary<string, InviteInfo>();


        internal ChampionDTO[] PlayerChampions;

        //internal List<int> curentlyRecording = new List<int>();
        internal List<int> curentlyRecording
        {
            get { return Client.curentlyRecording; }
            set { Client.curentlyRecording = value; }
        }

        internal List<string> Whitelist = new List<string>();

        #region Chat


        //Fix for invitations
        public delegate void OnMessageHandler(object sender, Message e);
        public event OnMessageHandler OnMessage;

        public Dictionary<string, string> PlayerNote = new Dictionary<string, string>();
        internal RosterManager RostManager;
        internal PresenceManager PresManager;

        internal PresenceType _CurrentPresence;

        internal PresenceType CurrentPresence
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

        internal string _CurrentStatus;

        internal string CurrentStatus
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

        internal void XmppConnection_OnMessage(object sender, agsXMPP.protocol.client.Message msg)
        {
            //This means that it is not for the user
            Client.Log(msg.InnerXml);

            //This blocks spammers from elo bosters
            if (ChatAutoBlock == null)
            {
                using (var client = new WebClient())
                {
                    try
                    {
                        var banned = client.DownloadString("http://legendaryclient.net/Autoblock.txt");
                        ChatAutoBlock = banned.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            if (ChatAutoBlock.Any(x => (msg.From.User + Region.RegionName).ToSHA1() == x.Split('#')[0]) && autoBlock)
                return;
            if (msg.Body.ToLower().Contains("elo") && msg.Body.ToLower().Contains("boost"))
                return;

            if (msg.Subject != null)
            {
                Client.MainWin.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    var subject = (ChatSubjects)Enum.Parse(typeof(ChatSubjects), msg.Subject, true);
                    NotificationPopup pop = new NotificationPopup(subject, msg, this)
                    {
                        Height = 230,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Bottom
                    };
                    Client.NotificationGrid.Children.Add(pop);
                }));

                return;
            }

            if (!Client.AllPlayers.ContainsKey(msg.From.User) || string.IsNullOrWhiteSpace(msg.Body))
                return;

            var chatItem = Client.AllPlayers[msg.From.User];
            if (Filter)
            {
                var item = new AllMessageInfo()
                {
                    message = msg.Body.Filter(),
                    time = DateTime.Now,
                    name = chatItem.Username
                };
                chatItem.Messages.Add(item);
            }
            else
            {
                var item = new AllMessageInfo()
                {
                    message = msg.Body,
                    time = DateTime.Now,
                    name = chatItem.Username
                };
                chatItem.Messages.Add(item);
            }

            Client.MainWin.FlashWindow();
            try
            {
                if (onChatMessageReceived != null)
                    onChatMessageReceived(msg.From.User, msg.Body);
            }
            catch { }
        }

        internal bool autoBlock = true;


        internal void ChatClientConnect(object sender)
        {
            Client.loadedGroups = false;
            Client.Groups.Add(new Group(LoginPacket.AllSummonerData.Summoner.InternalName));

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

                            if (root.item.group.text != "**Default" && Client.Groups.Find(e => e.GroupName == root.item.group.text) == null && root.item.group.text != null)
                                Client.Groups.Add(new Group(root.item.group.text));
                        }
                        else
                        {
                            RootObject2 root = JsonConvert.DeserializeObject<RootObject2>(PlayerJson);

                            if (!string.IsNullOrEmpty(root.item.name) && !string.IsNullOrEmpty(root.item.note))
                                PlayerNote.Add(root.item.name, root.item.note);

                            if (root.item.group != "**Default" && Client.Groups.Find(e => e.GroupName == root.item.group) == null && root.item.group != null)
                                Client.Groups.Add(new Group(root.item.group));
                        }
                    }
                    catch
                    {
                        Client.Log("Can't load friends", "ERROR");
                    }
                }
            }

            Client.Groups.Add(new Group("Offline"));
            SetChatHover();
            Client.loadedGroups = true;
            XmppConnection.OnRosterEnd -= ChatClientConnect; //only update groups on Client.Login
        }


        internal async Task<string> GetUserFromJid(string Jid)
        {
            if (Jid.Contains("@"))
                Jid = Jid.Split('@')[0];
            var names = await calls.GetSummonerNames(new double[] {Jid.Replace("sum", "").ToInt() });
            return names[0];
        }

        internal async void XmppConnection_OnPresence(object sender, Presence pres)
        {
            /*
            Client.Log("Received pres (Bare): " + pres.From.Bare);
            Client.Log("From user: " + pres.From.User);
            Client.Log("Pres Type: " + pres.Type);
            Client.Log("Other stuff: " + pres.InnerXml);
            //*/
            if (pres.From.User.Contains(LoginPacket.AllSummonerData.Summoner.AcctId.ToString()))
                return;
            if (pres.From.Bare.Contains("@sec.pvp.net") || pres.From.Bare.Contains("@lvl.pvp.net") || pres.From.Bare.Contains("@conference.pvp.net"))
                return;
            SetChatHover();
            switch (pres.Type)
            {
                case PresenceType.subscribe:
                case PresenceType.subscribed:
                    Client.MainWin.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        FriendInvite pop = new FriendInvite(ChatSubjects.XMPP_SUBSCRIBE, pres, this)
                        {
                            Height = 230,
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Bottom
                        };
                        Client.NotificationGrid.Children.Add(pop);
                        try
                        {
                            Client.AllPlayers.Add(pres.From.User, new ChatPlayerItem());
                        }
                        catch { }
                    }));
                    break;
                case PresenceType.unsubscribe:
                case PresenceType.unsubscribed:
                    await Client.MainWin.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        NotifyPlayerPopup notify = new NotifyPlayerPopup("Friends", string.Format("{0} is no longer your friend", pres.From.User));
                        Client.NotificationGrid.Children.Add(notify);
                    }));
                    Client.AllPlayers.Remove(pres.From.User);
                    break;
                case PresenceType.available:
                    if (!Client.AllPlayers.ContainsKey(pres.From.User))
                    {
                        if (pres.InnerXml.Contains("profileIcon"))
                        {
                            Client.AllPlayers.Add(pres.From.User, new ChatPlayerItem());
                        }
                    }

                    ChatPlayerItem Player = Client.AllPlayers[pres.From.User];
                    Player.IsOnline = false;
                    Client.UpdatePlayers = true;

                    string Presence = pres.Status;
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
                            Client.Log(e.Message + " - remember to fix this later instead of avoiding the problem.");
                        }
                    }
                    if (string.IsNullOrWhiteSpace(Player.Status))
                        Player.Status = "Online";
                    break;
                case PresenceType.unavailable:
                case PresenceType.invisible:
                    if (pres.From.User.Contains(LoginPacket.AllSummonerData.Summoner.AcctId.ToString()))
                        return;
                    try
                    {
                        ChatPlayerItem x = Client.AllPlayers[pres.From.User];
                        x.IsOnline = false;
                        Client.UpdatePlayers = true;
                    }
                    catch { }
                    break;
            }
        }
        
        internal void RostManager_OnRosterItem(object sender, RosterItem ri)
        {
            Client.UpdatePlayers = true;
            if (Client.AllPlayers.ContainsKey(ri.Jid.User))
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
                            else
                                player.Group = LoginPacket.AllSummonerData.Summoner.InternalName;
                            break;
                    }
                }
            }
            player.Username = ri.Name;
            Client.AllPlayers.Add(ri.Jid.User, player);
        }

        internal void SendMessage(string User, string Message)
        {
            XmppConnection.Send(new Message(User, Message));
        }

        internal void SetChatHover()
        {
            if (XmppConnection == null) return;
            if (XmppConnection.Authenticated)
            {
                if (presenceStatus != ShowType.NONE)
                    XmppConnection.Send(new Presence(presenceStatus, GetPresence(), 0) { Type = PresenceType.available });
                else
                    XmppConnection.Send(new Presence(presenceStatus, GetPresence(), 0) { Type = PresenceType.invisible });
            }            
        }

        internal bool hidelegendaryaddition = false;
        internal int ChampId = -1;
        internal string GetPresence()
        {
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


        

        internal void Message(string To, string Message, ChatSubjects Subject)
        {
            var msg = new Message(new Jid(To + "@pvp.net"))
            {
                Type = MessageType.normal,
                Subject = Subject.ToString(),
                Body = Message
            };

            XmppConnection.Send(msg);
        }


        
        
        internal int AmountOfWins; //Calculate wins for presence
        internal bool IsRanked;
        internal string TierName;
        internal string Tier;
        internal string LeagueName;
        internal string GameStatus = "outOfGame";
        internal ShowType presenceStatus = ShowType.chat;
        internal double timeStampSince = 0;

        #endregion Chat
        
        #region League Of Legends Client.Logic

        private System.Timers.Timer HeartbeatTimer;

        internal void StartHeartbeat()
        {
            HeartbeatTimer = new System.Timers.Timer();
            HeartbeatTimer.Elapsed += DoHeartbeat;
            HeartbeatTimer.Interval = 120000; // in milliseconds
            HeartbeatTimer.Start();
            DoHeartbeat(null, null);
        }

        private int HeartbeatCount;
        public Session PlayerSession;
        internal async void DoHeartbeat(object sender, ElapsedEventArgs e)
        {
            if (IsLoggedIn)
            {
                string result = await calls.PerformLCDSHeartBeat(Convert.ToInt32(LoginPacket.AllSummonerData.Summoner.AcctId),
                    PlayerSession.Token,
                    HeartbeatCount,
                    DateTime.Now.ToString("ddd MMM d yyyy HH:mm:ss 'GMT-0700'"));
                if (result != "5")
                {
                    
                }
                HeartbeatCount++;
            }
        }

        internal string Gas;

        /// <summary>
        ///     Main connection to the League of Legends server
        /// </summary>
        internal RtmpClient RiotConnection;

        /// <summary>
        ///     Packet recieved when initially Client.Logged on. Cached so the packet doesn't
        ///     need to requested multiple times, causing slowdowns
        /// </summary>
        internal LoginDataPacket LoginPacket;

        /// <summary>
        ///     All enabled game configurations for the user
        /// </summary>
        internal List<GameTypeConfigDTO> GameConfigs;

        /// <summary>
        ///     The region the user is connecting to
        /// </summary>
        internal BaseRegion Region;

        /// <summary>
        ///     Is the client Client.Logged in to the League of Legends server
        /// </summary>
        internal bool IsLoggedIn = false;

        /// <summary>
        ///     GameID of the current game that the client is connected to
        /// </summary>
        internal double GameID = 0;

        /// <summary>
        ///     Game Name of the current game that the client is connected to
        /// </summary>
        internal string GameName = string.Empty;

        /// <summary>
        ///     The DTO of the game lobby when connected to a custom game
        /// </summary>
        internal GameDTO GameLobbyDTO;
        
        /// <summary>
        ///     A recorder
        /// </summary>
        internal ReplayRecorder recorder = null;

        /// <summary>
        ///     When going into champion select reuse the last DTO to set up data
        /// </summary>
        internal GameDTO ChampSelectDTO;

        /// <summary>
        ///     When connected to a game retrieve details to connect to
        /// </summary>
        internal PlayerCredentialsDto CurrentGame;

        internal bool AutoAcceptQueue = false;
        internal object LobbyContent;
        
        internal bool IsInGame;
        internal bool RunonePop = false;
#pragma warning disable 4014

        internal void OnMessageReceived(object sender, MessageReceivedEventArgs message)
        {
            Client.Log("Message received! The type is: " + message.Body.GetType());
            Client.MainWin.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async () =>
            {
                var balance = message.Body as StoreAccountBalanceNotification;
                if (balance != null)
                {
                    StoreAccountBalanceNotification newBalance = balance;
                    //InfoLabel.Content = "IP: " + newBalance.Ip + " ∙ RP: " + newBalance.Rp;
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
                                var name = await calls.GetSummonerNames(new[] { Convert.ToDouble(notification.MessageArgument) });
                                messageOver.MessageTextBox.Text = name[0] + " quit from queue!";
                                break;
                            default:
                                messageOver.MessageTextBox.Text = notification.MessageCode + Environment.NewLine;
                                messageOver.MessageTextBox.Text = System.Convert.ToString(notification.MessageArgument);
                                break;
                        }
                        Client.OverlayContainer.Content = messageOver.Content;
                        Client.OverlayContainer.Visibility = Visibility.Visible;
                        Client.ClearPage(typeof(CustomGameLobbyPage));
                        if (notification.Type != "PLAYER_QUIT")
                            Client.SwitchPage(Client.MainPage);
                    }
                    else if (message.Body is EndOfGameStats)
                    {
                        var stats = (EndOfGameStats)message.Body;
                        var EndOfGame = new EndOfGamePage(stats);
                        Client.ClearPage(typeof(TeamQueuePage));
                        Client.OverlayContainer.Visibility = Visibility.Visible;
                        Client.OverlayContainer.Content = EndOfGame.Content;
                    }
                    else if (message.Body is StoreFulfillmentNotification)
                    {
                        PlayerChampions = await calls.GetAvailableChampions();
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
                        {
                            InviteInfo x = InviteData[stats.InvitationId];
                            if (x.Inviter != null)
                                return;
                        }

                        Client.MainWin.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                        {
                            var pop = new GameInvitePopup(stats, this)
                            {
                                HorizontalAlignment = HorizontalAlignment.Right,
                                VerticalAlignment = VerticalAlignment.Bottom,
                                Height = 230
                            };

                            Client.NotificationGrid.Children.Add(pop);
                        }));
                        Client.MainWin.FlashWindow();
                    }
                    else if (message.Body is ClientLoginKickNotification)
                    {
                        var kick = (ClientLoginKickNotification)message.Body;
                        if (kick.sessionToken == null)
                            return;

                        Warning Warn = new Warning
                        {
                            Header = { Content = "Kicked from server" },
                            MessageText = { Text = "This account has been Client.Logged in from another location" }
                        };
                        Warn.backtochampselect.Click += (Client.MainWin as MainWindow).LogoutButton_Click;
                        Warn.AcceptButton.Click += QuitClient;
                        Warn.hide.Visibility = Visibility.Hidden;
                        Warn.backtochampselect.Content = "Client.Logout(Work in progress)";
                        Warn.AcceptButton.Content = "Quit";
                        Client.FullNotificationOverlayContainer.Content = Warn.Content;
                        Client.FullNotificationOverlayContainer.Visibility = Visibility.Visible;
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
                            messageOver.AcceptButton.Click += (o, e) => { calls.CallPersistenceMessaging(response); };
                        }
                    }
                }
            }));
        }

        private void QuitClient(object sender, RoutedEventArgs e)
        {
            if (IsLoggedIn)
            {
                calls.PurgeFromQueues();
                calls.Leave();
                RiotConnection.Close();
            }
            Environment.Exit(0);
        }

        internal async void QuitCurrentGame()
        {
            if (OnMessage != null)
                foreach (Delegate d in OnMessage.GetInvocationList())
                    OnMessage -= (OnMessageHandler)d;

            FixChampSelect();
            FixLobby();
            IsInGame = false;

            await calls.QuitGame();
            Client.StatusGrid.Visibility = Visibility.Hidden;
            Client.PlayButton.Visibility = Visibility.Visible;
            LobbyContent = null;
            Client.LastPageContent = null;
            GameStatus = "outOfGame";
            SetChatHover();
            Client.SwitchPage(Client.MainPage);
        }

        internal void FixLobby()
        {

        }

        internal void FixChampSelect()
        {

        }

        #endregion League Of Legends Client.Logic
                

        

        internal int SelectChamp;
        internal bool usingInstaPick = false;

        internal KeyValuePair<string, string> userpass;
        internal bool HasPopped = false;

        //public string UpdateRegion { get; set; }

        public string GameType { get; set; }

        public Dictionary<string, string> LocalRunePages = new Dictionary<string, string>();

        public GameQueueConfig[] Queues;

        public bool isConnectedToRTMP = true;

        public string reconnectToken { get; set; }

        public async void RiotConnection_Disconnected(object sender, EventArgs e)
        {
            isConnectedToRTMP = false;
            Client.Log(LoginPacket.AllSummonerData.Summoner.InternalName + " is disconnected from RTMPS");
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
            //await RiotCalls.Client.Login(reconnectToken);
            var str1 = string.Format("gn-{0}", PlayerSession.AccountSummary.AccountId);
            var str2 = string.Format("cn-{0}", PlayerSession.AccountSummary.AccountId);
            var str3 = string.Format("bc-{0}", PlayerSession.AccountSummary.AccountId);
            Task<bool>[] taskArray = { RiotConnection.SubscribeAsync("my-rtmps", "messagingDestination", str1, str1), 
                                       RiotConnection.SubscribeAsync("my-rtmps", "messagingDestination", str2, str2), 
                                       RiotConnection.SubscribeAsync("my-rtmps", "messagingDestination", "bc", str3) };

            await Task.WhenAll(taskArray);
            isConnectedToRTMP = true;
        }
        public bool isInternetAvailable;

        public void CheckInternetConnection()
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
        public Thread connectionCheck;

        internal void XmppConnection_OnError(object sender, Exception ex)
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

        public string[] ChatAutoBlock { get; set; }
    }


    public class ChatPlayerItem
    {
        public List<AllMessageInfo> Messages = new List<AllMessageInfo>();

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

    public class AllMessageInfo
    {
        public DateTime time { get; set; }

        public string name { get; set; }

        public string message { get; set; }
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
