using System.Collections.Generic;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Controls;
using System.Windows.Media;
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
        internal const string Version = "3.12.13_10_08_16_20";
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

        internal static RosterManager RostManager;
        internal static PresenceManager PresManager;

        internal static Dictionary<string, string> AllPlayers = new Dictionary<string, string>();
        internal static Dictionary<string, string> Messages = new Dictionary<string, string>();

        internal static bool ChatClient_OnInvalidCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        internal static void ChatClient_OnMessage(object sender, jabber.protocol.client.Message msg)
        {
            Messages.Add(msg.From.User, msg.Body);
        }

        internal static void ChatClientConnect(object sender)
        {
            SetChatHover(PresenceType.available, 30, 500, "im best in ozeeeeeeeeeeee", false);
        }

        internal static void SetChatHover(PresenceType Presence, int Level, int Wins, string statusMsg, bool inGame)
        {
            /*ChatClient.Presence(Presence,
                "<body>" +
                      "<level>" + Level + "</level>" +
                      "<wins>" + Wins + "</wins>" +
                      "<statusMsg>" + statusMsg + "</statusMsg>" +
                      "<gameStatus>" + ((inGame == true) ? "inGame" : "outOfGame") + "</gameStatus>" +
                "</body>", null, 0);*/
            ChatClient.Presence(Presence, "<body>" +
                "<profileIcon>552</profileIcon>" +
                "<level>" + Level + "</level>" +
                "<wins>" + Wins + "</wins>" +
                "<leaves>52</leaves>" +
                "<queueType /><rankedLosses>0</rankedLosses><rankedRating>0</rankedRating><tier>UNRANKED</tier>" + //Unused?
                //"<rankedLeagueName>Urgot&apos;s Patriots</rankedLeagueName>" +
                "<rankedLeagueName>legendaryclientDOTcom</rankedLeagueName>" +
                "<rankedLeagueDivision>I</rankedLeagueDivision>" +
                "<rankedLeagueTier>BRONZE</rankedLeagueTier>" + 
                "<rankedLeagueQueue>RANKED_SOLO_5x5</rankedLeagueQueue>"+
                "<rankedWins>287</rankedWins>" +
                "<gameStatus>" + ((inGame == true) ? "inGame" : "outOfGame") + "</gameStatus>" +
                "<statusMsg>best in oz m8</statusMsg>" + 
            "</body>", null, 0);
        }

        internal static void RostManager_OnRosterItem(object sender, jabber.protocol.iq.Item ri)
        {
            if (!AllPlayers.ContainsKey(ri.JID.User))
            {
                AllPlayers.Add(ri.JID.User, ri.Nickname);
            }
        }

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
}
