using LCLog;
using Sightstone.Controls;
using Sightstone.Logic.Region;
using Sightstone.Logic.SQLite;
using Sightstone.Windows;
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
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading;
using Sightstone.Logic.PlayerSpell;

namespace Sightstone.Logic.MultiUser
{
    /// <summary>
    ///     Any logic that needs to be reused over multiple pages
    /// </summary>
    public static class Client
    {
        #region version
        internal static string Version = "4.21.14";
        #endregion version

        #region Language
        /// <summary>
        /// The language ResourceDictionary
        /// </summary>
        public static ResourceDictionary Dict = new ResourceDictionary();

        /// <summary>
        /// Check paths for spell icon
        /// </summary>
        /// <param name="spell"></param>
        /// <returns></returns>
        internal static Uri GetSpellIconUri(NameToImage spell)
        {
            if(File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", "Summoner" + spell + ".png")))
            {
                return new Uri(Path.Combine(ExecutingDirectory, "Assets", "spell", "Summoner" + spell + ".png"),
                        UriKind.Absolute);
            }
            else if (File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "swf", "ImagePack_spells",
                "ImagePack_spells_Embeds__e_Spell_Summoner" + spell + ".bmp")))
            {
                return new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "swf", "ImagePack_spells",
                    "ImagePack_spells_Embeds__e_Spell_Summoner" + spell + ".bmp"), 
                    UriKind.Absolute);
            }
            else
            {
                Client.Log("Cannot find spell: " + spell.ToString(), "WARN");
                return
                    new Uri("/Sightstone;component/NONE.png", UriKind.RelativeOrAbsolute);
            }
        }

        /// <summary>
        /// Get the text from the ResourceDictionary
        /// </summary>
        /// <param name="key">The key in the <see cref="Dict"/></param>
        /// <returns>The <see cref="Dict"/> value for the key or the <see cref="key"/> if not found</returns>
        public static string GetDictText(string key)
        {
            foreach (var keys in Dict.Keys.Cast<string>().Where(keys => keys == key))
            {
                return ((string)Dict[keys]).Replace("\n", Environment.NewLine);
            }
            return key;
        }

        internal static Uri GetIconUri(int iconId)
        {
            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", iconId + ".png")))
            {
                return
                    new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", iconId + ".png"),
                        UriKind.Absolute);
            }
            else if (File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", iconId + ".jpg")))
            {
                return
                    new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", iconId + ".jpg"),
                        UriKind.Absolute);
            }
            else if (File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "swf", "ImagePack_buddyIcons",
                        "ImagePack_buddyIcons_Embeds__e_profileIcon" + iconId + ".jpg")))
            {
                return
                    new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "swf", "ImagePack_buddyIcons",
                        "ImagePack_buddyIcons_Embeds__e_profileIcon" + iconId + ".jpg"),
                        UriKind.Absolute);
            }
            else if (File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "swf", "ImagePack_buddyIcons",
            "ImagePack_buddyIcons_Embeds__e_profileIconTencent" + iconId + ".jpg")))
            {
                return
                    new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "swf", "ImagePack_buddyIcons",
                        "ImagePack_buddyIcons_Embeds__e_profileIconTencent" + iconId + ".jpg"),
                        UriKind.Absolute);
            }
            else
            {
                Client.Log("Cannot find icon number" + iconId, "WARN");
                return 
                    new System.Uri("/Sightstone;component/NONE.png", UriKind.RelativeOrAbsolute);
            }
        }


        #endregion Language

        #region Current
        internal static string CurrentUser = "";

        internal static string CurrentServer = "";
        #endregion Current

        #region pipes
        public static StreamString SendPIPE;
        public static StreamString InPIPE;
        public static bool Pipe = false;

        #region logs
        public static void Log(string lines, string type = "LOG")
        {
            WriteToLog.Log(lines, type);
#if DEBUG
            Debugger.Log(0, type, lines + Environment.NewLine);
#endif
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
        public static string EncrytKey;
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

        internal static BitmapImage GetItemIcon(int id)
        {
            var iconFileName = items.GetItem(id).iconPath;
            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "swf", "ImagePack_items",
                "ImagePack_items_Embeds__e_" + iconFileName)))
            {
                var uri = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "swf", "ImagePack_items",
                "ImagePack_items_Embeds__e_" + iconFileName));
                return new BitmapImage(uri);
            }
            else
            {
                var files = Directory.GetFiles(Path.Combine(Client.ExecutingDirectory, "Assets", "swf", "ImagePack_items"), "*" + id + "_*");

                if (files.Count() > 0)
                {
                    return new BitmapImage(new Uri(files.First()));
                }
                else
                {
                    return new BitmapImage(new Uri("/Sightstone;component/NONE.png", UriKind.RelativeOrAbsolute));
                }
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
        internal static string GLocation;
        internal static string RootLocation;
        internal static string GRootLocation;

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
            var isGarena = (UserList.Users[CurrentServer])[CurrentUser].Garena;
            string GameDirectory = isGarena ? GLocation : Location;
            string StartFileName = Path.Combine(GameDirectory, "League of Legends.exe");
            Log("LoL Exe Path : " + StartFileName);

            var p = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = GameDirectory,
                    FileName = StartFileName
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
        }
        private static void p_Exited(object sender, EventArgs e)
        {
            MainWin.Show();
            MainWin.Visibility = Visibility.Visible;
        }

        internal static void LaunchSpectatorGame(string SpectatorServer, string Key, int GameId, string Platform)
        {
            string GameDirectory = (UserList.Users[CurrentServer])[CurrentUser].Garena ? GLocation : Location;

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

        internal static void ErrorOverlay(string Message)
        {
            var overlay = new MessageOverlay { MessageTitle = { Content = "Error" }, MessageTextBox = { Text = Message } };
            Client.OverlayContainer.Content = overlay.Content;
            Client.OverlayContainer.Visibility = Visibility.Visible;
        }

        internal static ImageSource ImageSourceFromUri(Uri source)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = source;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                return bitmap;
            }
            catch(FileNotFoundException e)
            {
                Uri oUri = new Uri("pack://application:,,,/Sightstone;component/NONE.png", UriKind.RelativeOrAbsolute);
                return BitmapFrame.Create(oUri);
            }
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
                        try
                        {
                            names = client.DownloadString("https://raw.githubusercontent.com/LegendaryClient/LegendaryClient/gh-pages/QueueName");
                        }
                        catch { }
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
            var UriSource = new Uri(address, UriKind.RelativeOrAbsolute);
            if (File.Exists(address) || address.StartsWith("/Sightstone;component"))
                return new BitmapImage(UriSource);

            Client.Log("Cannot find " + address, "WARN");
            UriSource = new Uri("/Sightstone;component/NONE.png", UriKind.RelativeOrAbsolute);

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

        internal static bool ready = false;
        
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
