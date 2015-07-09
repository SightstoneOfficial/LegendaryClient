using LCLog;
using LegendaryClient.Controls;
using LegendaryClient.Logic.Region;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Windows;
using MahApps.Metro;
using MahApps.Metro.Controls;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

/// <summary>
///     Any logic that needs to be reused over multiple pages
/// </summary>
namespace LegendaryClient.Logic.MultiUser
{
    public static class Client
    {
        #region version
        internal static string Version = "4.21.14";
        #endregion version

        #region Current
        internal static string Current = "";
        #endregion Current

        #region pipes
        public static StreamString SendPIPE;
        public static StreamString InPIPE;
        public static bool Pipe = false;

        #region logs
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
        #endregion logs
        #endregion pipes

        #region accounts
        public static string DecryptStringAES(this string input, string Secret)
        {
            string output = string.Empty;
            var aesAlg = new RijndaelManaged();
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(Secret))
                return output;

            try
            {
                var key = new Rfc2898DeriveBytes(Secret, Encoding.ASCII.GetBytes("o6806642kbM7c5"));
            
                byte[] bytes = Convert.FromBase64String(input);
                using (var msDecrypt = new MemoryStream(bytes))
                {
                    aesAlg = new RijndaelManaged();
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                    aesAlg.IV = ReadByteArray(msDecrypt);
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
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
                aesAlg.Clear();
            }

            return output;
        }

        internal static string EncryptStringAES(this string input, string secret)
        {
            string output = string.Empty;
            var aesAlg = new RijndaelManaged();
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(secret))
                return output;

            try
            {
                var key = new Rfc2898DeriveBytes(secret, Encoding.ASCII.GetBytes("o6806642kbM7c5"));
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (var msEncrypt = new MemoryStream())
                {
                    msEncrypt.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int));
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(input);
                        }
                    }
                    output = Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
            finally
            {
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

        public static string ToSHA1(this string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(hash);
            }
        }
        #endregion account

        #region lolClient
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

        internal static string Location;
        internal static string RootLocation;

        /// <summary>
        /// Gets where the lolclient is located
        /// </summary>
        /// <returns>The location of the lolclient</returns>
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
        /// <summary>
        /// Launches lolclient to enter a game
        /// </summary>
        internal static void LaunchGame(string ip, string port, string key, string sumId, string sname, BaseRegion region)
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
                                    ip + " " +
                                    port + " " +
                                    key + " " +
                                    sumId + "\"";
            /*
            p.StartInfo.Arguments = "\"8394\" \"" + RootLocation + "LoLLauncher.exe" + "\" \"" + "\" \"" +
                                    CurrentGame.ServerIp + " " +
                                    CurrentGame.ServerPort + " " +
                                    CurrentGame.EncryptionKey + " " +
                                    CurrentGame.SummonerId + "\"";
            //*/
            p.Start();
            var t = new Timer
            {
                Interval = 5000,
            };
            t.Elapsed += (o, m) =>
            {
                if (region.Garena)
                    return;
                GameScouter scouter = new GameScouter();
                scouter.LoadScouter(sname);
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
        #endregion lolClient

        //todo: Remove this
        #region lcPatcher
        internal static bool patching = true;

        internal static bool donepatch = false;
        #endregion lcPatcher

        #region lcLogic
        public static Accent CurrentAccent { get; set; }

        internal static List<int> curentlyRecording = new List<int>();

        internal static string Theme;

        public static string GameClientVersion;

        internal static Grid MainGrid;

        internal static Grid NotificationGrid;

        internal static Grid StatusGrid = new Grid();

        internal static System.Windows.Controls.Image BackgroundImage;
        
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

        internal static System.Windows.Controls.Image MainPageProfileImage;

        internal static ComboBoxItem UserTitleBarLabel;

        public static GameScouter win;

        internal static System.Windows.Controls.Image UserTitleBarImage;

        public static ProfilePage Profile;

        public static MainPage MainPage;

        internal static StatusPage statusPage;

        internal static FriendList FriendList;

        internal static NotificationPage notificationPage;

        internal static MetroWindow MainWin;

        internal static bool GroupIsShown;

        internal static bool PlayerChatIsShown;

        internal static Page TrueCurrentPage;
        
        internal static MediaElement SoundPlayer;

        internal static MediaElement AmbientSoundPlayer;

        internal static StackPanel chatlistview;

        internal static string[] args;
        
        internal static string ExecutingDirectory = string.Empty;

        internal static bool HasPopped = false;

        internal static bool runonce = false;

        internal static object LastPageContent;

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

        internal static void FocusClient()
        {
            if (MainWin.WindowState == WindowState.Minimized)
                MainWin.WindowState = WindowState.Normal;

            MainWin.Activate();
            MainWin.Topmost = true; // important
            MainWin.Topmost = false; // important
            MainWin.Focus(); // important
        }
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
            Client.Log("Switching to the page: " + page.GetType());
            IsOnPlayPage = page is PlayPage;
            BackgroundImage.Visibility = page is ChampSelectPage
                ? Visibility.Hidden
                : Visibility.Visible;

            if (page is MainPage)
            {
                Page p = Pages.FirstOrDefault(x => x is MainPage);
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
        #endregion lcLogic

        #region queues
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
                        client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)");
                        client.Headers.Add("Content-Type", "text/html; charset=UTF-8");
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
        #endregion queues

        #region converters
        public static int MathRound(this double toRound)
        {
            return (int)Math.Round(toRound);
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
        public static BitmapImage GetImage(string address)
        {
            var UriSource = new System.Uri(address, UriKind.RelativeOrAbsolute);
            if (File.Exists(address) || address.StartsWith("/LegendaryClient;component"))
                return new BitmapImage(UriSource);

            Client.Log("Cannot find " + address, "WARN");
            UriSource = new System.Uri("/LegendaryClient;component/NONE.png", UriKind.RelativeOrAbsolute);

            return new BitmapImage(UriSource);
        }

        internal static int ToInt(this object convert)
        {
            return Convert.ToInt32(convert);
        }
        #endregion converters

        #region chat
        internal static List<Group> Groups = new List<Group>();

        internal static bool UpdatePlayers = true;
        
        internal static bool loadedGroups = false;

        internal static string GetChatroomJid(string ObfuscatedChatroomName, string password, bool IsTypePublic)
        {
            if (!IsTypePublic)
                return ObfuscatedChatroomName + "@sec.pvp.net";

            if (string.IsNullOrEmpty(password))
                return ObfuscatedChatroomName + "@lvl.pvp.net";

            return ObfuscatedChatroomName + "@conference.pvp.net";
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

        internal static Dictionary<string, ChatPlayerItem> AllPlayers = new Dictionary<string, ChatPlayerItem>();
        #endregion chat
    }
}
