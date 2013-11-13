using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using jabber.client;
using jabber.protocol.client;
using LegendaryClient.Logic.Region;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect;
using PVPNetConnect.RiotObjects.Platform.Clientfacade.Domain;
using PVPNetConnect.RiotObjects.Platform.Game;
using SQLite;

namespace LegendaryClient.Logic
{
    /// <summary>
    /// Any logic that needs to be reused over multiple pages
    /// </summary>
    internal static class Client
    {
        /// <summary>
        /// Latest champion for League of Legends login screen
        /// </summary>
        internal const int LatestChamp = 222;
        /// <summary>
        /// Latest version of League of Legends
        /// </summary>
        internal const string Version = "3.13.13_10_28_21_11";
        /// <summary>
        /// The current directory the client is running from
        /// </summary>
        internal static string ExecutingDirectory = "";
        /// <summary>
        /// Riot's database with all the client data
        /// </summary>
        internal static SQLiteConnection SQLiteDatabase;
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
        /// The database of all the search tags
        /// </summary>
        internal static List<championSearchTags> SearchTags;
        /// <summary>
        /// The database of all the keybinding defaults & proper names
        /// </summary>
        internal static List<keybindingEvents> Keybinds;

        internal static JabberClient ChatClient;

        internal static PresenceType _CurrentPresence;
        internal static PresenceType CurrentPresence
        {
            get { return _CurrentPresence; }
            set
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

        internal static string _CurrentStatus;
        internal static string CurrentStatus
        {
            get { return _CurrentStatus; }
            set
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

        internal static RosterManager RostManager;
        internal static PresenceManager PresManager;
        internal static bool UpdatePlayers = true;

        internal static Dictionary<string, ChatPlayerItem> AllPlayers = new Dictionary<string, ChatPlayerItem>();

        internal static bool ChatClient_OnInvalidCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        internal static void ChatClient_OnMessage(object sender, jabber.protocol.client.Message msg)
        {
            if (AllPlayers.ContainsKey(msg.From.User))
                AllPlayers[msg.From.User].Messages.Add(AllPlayers[msg.From.User].Username + ": " + msg.Body);
        }

        internal static void ChatClientConnect(object sender)
        {
            SetChatHover(CurrentPresence, LoginPacket.AllSummonerData.SummonerLevel.Level, 500, CurrentStatus, false);
        }

        internal static void SendMessage(string User, string Message)
        {
            ChatClient.Message(User, Message);
        }

        internal static void SetChatHover(PresenceType Presence, double Level, int Wins, string statusMsg, bool inGame)
        {
            ChatClient.Presence(Presence, "<body>" +
                "<profileIcon>0</profileIcon>" +
                "<level>" + Level + "</level>" +
                "<wins>" + Wins + "</wins>" +
                "<leaves>52</leaves>" +
                (Level >= 30 ?
                "<queueType /><rankedLosses>0</rankedLosses><rankedRating>0</rankedRating><tier>UNRANKED</tier>" + //Unused?
                "<rankedLeagueName>Urgot&apos;s Patriots</rankedLeagueName>" +
                "<rankedLeagueDivision>I</rankedLeagueDivision>" +
                "<rankedLeagueTier>BRONZE</rankedLeagueTier>" + 
                "<rankedLeagueQueue>RANKED_SOLO_5x5</rankedLeagueQueue>"+
                "<rankedWins>287</rankedWins>" : "") +
                "<gameStatus>" + ((inGame == true) ? "inGame" : "outOfGame") + "</gameStatus>" +
                "<statusMsg>" + statusMsg + "∟</statusMsg>" + //Look for "∟" to recognize that LegendaryClient - not shown on normal client
            "</body>", null, 0);
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
                                    break;
                                case "gameStatus":
                                    reader.Read();
                                    string gameStatus = reader.Value;
                                    Player.InGame = (gameStatus != "outOfGame");
                                    break;

                                case "skinName": //No idea what this is
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
                            #endregion
                        }
                    }
                }
                if (String.IsNullOrWhiteSpace(Player.Status))
                {
                    Player.Status = "Online";
                }
            }
        }

        internal static Grid MainGrid;
        internal static Label InfoLabel;
        internal static ContentControl ChatContainer;

        #region WPF Tab Change
        /// <summary>
        /// The container that contains the page to display
        /// </summary>
        internal static ContentControl Container;
        /// <summary>
        /// Page cache to stop having to recreate all information if pages are overwritted
        /// </summary>
        internal static List<Page> Pages;

        /// <summary>
        /// Switches the contents of the frame to the requested page. Also sets background on
        /// the button on the top to show what section you are currently on.
        /// </summary>
        internal static void SwitchPage(Page page)
        {
            foreach (Page p in Pages) //Cache pages
            {
                if (p.GetType() == page.GetType())
                {
                    Container.Content = p.Content;
                    return;
                }
            }
            Container.Content = page.Content;
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

        #endregion

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
        /// Game Name of the current game that the client is connected to
        /// </summary>
        internal static string GameName = "";
        /// <summary>
        /// The DTO of the game lobby when connected to a custom game
        /// </summary>
        internal static GameDTO GameLobbyDTO;
        /// <summary>
        /// When going into champion select reuse the last DTO to set up data
        /// </summary>
        internal static GameDTO ChampSelectDTO;
        /// <summary>
        /// When connected to a game retrieve details to connect to
        /// </summary>
        internal static PlayerCredentialsDto CurrentGame;
        /// <summary>
        /// When an error occurs while connected. Currently un-used
        /// </summary>
        internal static void PVPNet_OnError(object sender, PVPNetConnect.Error error) { }
        #endregion
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
        public bool InGame { get; set; }
        public long Timestamp { get; set; }
        public bool Busy { get; set; }
        public string Champion { get; set; }
        public string Status { get; set; }
        public bool UsingLegendary { get; set; }
        public List<string> Messages = new List<string>();
    }
}
