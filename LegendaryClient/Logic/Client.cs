using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
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
        internal static IEnumerable<champions> Champions;
        /// <summary>
        /// The database of all the champion abilities
        /// </summary>
        internal static IEnumerable<championAbilities> ChampionAbilities;
        /// <summary>
        /// The database of all the champion skins
        /// </summary>
        internal static IEnumerable<championSkins> ChampionSkins;
        /// <summary>
        /// The database of all the items
        /// </summary>
        internal static IEnumerable<items> Items;

        #region WPF Tab Change
        /// <summary>
        /// The container that contains the page to display
        /// </summary>
        internal static Frame Container;
        /// <summary>
        /// Page cache to stop having to recreate all information if pages are overwritted
        /// </summary>
        internal static List<Page> Pages;
        /// <summary>
        /// All buttons to enable/disable when logging in-out/playing a game
        /// </summary>
        internal static List<Button> EnableButtons;

        /// <summary>
        /// Switches the contents of the frame to the requested page. Also sets background on
        /// the button on the top to show what section you are currently on.
        /// </summary>
        internal static void SwitchPage(Page page, string ButtonContent)
        {
            foreach (Button b in Client.EnableButtons)
            {
                if ((string)b.Content == ButtonContent)
                {
                    BrushConverter bc = new BrushConverter();
                    Brush brush = (Brush)bc.ConvertFrom("#FF5A5A5A");
                    b.Background = brush;
                }
                else
                {
                    BrushConverter bc = new BrushConverter();
                    Brush brush = (Brush)bc.ConvertFrom("#FF505050");
                    b.Background = brush;
                }
            }

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
