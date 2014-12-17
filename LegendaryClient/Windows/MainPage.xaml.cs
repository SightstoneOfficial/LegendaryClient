#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.Maps;
using LegendaryClient.Logic.PlayerSpell;
using LegendaryClient.Logic.Region;
using LegendaryClient.Logic.Replays;
using LegendaryClient.Logic.SQLite;
using Newtonsoft.Json;
using PVPNetConnect;
using PVPNetConnect.RiotObjects.Leagues.Pojo;
using PVPNetConnect.RiotObjects.Platform.Broadcast;
using PVPNetConnect.RiotObjects.Platform.Clientfacade.Domain;
using PVPNetConnect.RiotObjects.Platform.Leagues.Client.Dto;
using PVPNetConnect.RiotObjects.Platform.Statistics;
using PVPNetConnect.RiotObjects.Platform.Summoner;
using Image = System.Windows.Controls.Image;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using Timer = System.Timers.Timer;

#endregion

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage
    {
        internal static Timer timer = new Timer();
        internal ArrayList GameList;
        internal ArrayList NewsList;
        internal int SelectedGame = 0;

        public MainPage()
        {
            InitializeComponent();
            GotPlayerData(Client.LoginPacket);
            SpectatorComboBox.SelectedValue = Client.Region.RegionName;
            BaseRegion region = BaseRegion.GetRegion(Client.Region.RegionName);
            uiLogic.CreateProfile(Client.LoginPacket.AllSummonerData.Summoner.Name);
            ChangeSpectatorRegion(region);
            GetNews(region);
            var update = new Timer
            {
                Interval = 5000
            };
            update.Elapsed +=
                (o, e) =>
                    Client.ChatClient.Presence(Client.CurrentPresence, Client.GetPresence(), Client.presenceStatus, 0);
            timer.Interval = (5000);
            //timer.Start();

            timer.Elapsed += (o, e) => Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                string jid =
                    Client.GetChatroomJID(Client.GetObfuscatedChatroomName("legendaryclient", ChatPrefixes.Public),
                        string.Empty, true);

                GroupChatItem item = Join(jid, "LegendaryClient");
                var chatGroup = new NotificationChatGroup
                {
                    Tag = item,
                    GroupTitle = item.GroupTitle,
                    Margin = new Thickness(1, 0, 1, 0),
                    GroupLabelName = {Content = item.GroupTitle}
                };
                if (Client.GroupChatItems.All(i => i.GroupTitle != "LegendaryClient"))
                {
                    Client.ChatListView.Items.Add(chatGroup);
                    Client.GroupChatItems.Add(item);
                }

                timer.Stop();
            }));

            if (Client.Dev)
            {
                fakeend.Visibility = Visibility.Visible;
                testChat.Visibility = Visibility.Visible;
                testInvite.Visibility = Visibility.Visible;
            }
        }

        [STAThread]
        private GroupChatItem Join(string jid, string chat)
        {
            return new GroupChatItem(jid, chat);
        }

        private void GotPlayerData(LoginDataPacket packet)
        {
            Client.PVPNet.OnMessageReceived += PVPNet_OnMessageReceived;
            UpdateSummonerInformation();
        }

        internal async void UpdateSummonerInformation()
        {
            AllSummonerData playerData =
                await Client.PVPNet.GetAllSummonerDataByAccount(Client.LoginPacket.AllSummonerData.Summoner.AcctId);
            SummonerNameLabel.Content = playerData.Summoner.Name;
            if (Client.LoginPacket.AllSummonerData.SummonerLevel.Level < 30)
            {
                PlayerProgressBar.Value = (playerData.SummonerLevelAndPoints.ExpPoints/
                                           playerData.SummonerLevel.ExpToNextLevel)*100;
                PlayerProgressLabel.Content = String.Format("Level {0}", playerData.SummonerLevel.Level);
                PlayerCurrentProgressLabel.Content = String.Format("{0}XP", playerData.SummonerLevelAndPoints.ExpPoints);
                PlayerAimProgressLabel.Content = String.Format("{0}XP", playerData.SummonerLevel.ExpToNextLevel);
            }
            else
                Client.PVPNet.GetAllLeaguesForPlayer(playerData.Summoner.SumId, GotLeaguesForPlayer);

            if (Client.LoginPacket.BroadcastNotification.BroadcastMessages != null)
            {
                var message =
                    Client.LoginPacket.BroadcastNotification.BroadcastMessages[0] as Dictionary<string, object>;
                if (message != null)
                    BroadcastMessage.Text = Convert.ToString(message["content"]);
            }

            foreach (PlayerStatSummary x in Client.LoginPacket.PlayerStatSummaries.PlayerStatSummarySet)
            {
                if (x.PlayerStatSummaryTypeString == "Unranked")
                {
                    Client.IsRanked = false;
                    Client.AmountOfWins = x.Wins;
                }
                if (x.PlayerStatSummaryTypeString != "RankedSolo5x5")
                    continue;

                Client.IsRanked = true;
                Client.AmountOfWins = x.Wins;
                break;
            }

            Client.InfoLabel.Content = "IP: " + Client.LoginPacket.IpBalance + " ∙ RP: " + Client.LoginPacket.RpBalance;
            int profileIconId = Client.LoginPacket.AllSummonerData.Summoner.ProfileIconId;
            var uriSource =
                new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", profileIconId + ".png"),
                    UriKind.RelativeOrAbsolute);
            ProfileImage.Source = new BitmapImage(uriSource);
            Client.MainPageProfileImage = ProfileImage;
        }

        private void GotLeaguesForPlayer(SummonerLeaguesDTO result)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                string currentLp = "";
                string currentTier = "";
                bool inPromo = false;
                if (result.SummonerLeagues != null && result.SummonerLeagues.Count > 0)
                {
                    foreach (
                        LeagueListDTO leagues in
                            result.SummonerLeagues.Where(leagues => leagues.Queue == "RANKED_SOLO_5x5"))
                    {
                        Client.Tier = leagues.RequestorsRank;
                        Client.TierName = leagues.Tier;
                        Client.LeagueName = leagues.Name;
                        currentTier = leagues.Tier + " " + leagues.RequestorsRank;
                        List<LeagueItemDTO> players =
                            leagues.Entries.OrderBy(o => o.LeaguePoints)
                                .Where(item => item.Rank == leagues.RequestorsRank)
                                .ToList();
                        foreach (LeagueItemDTO player in players)
                        {
                            if (player.PlayerOrTeamName != Client.LoginPacket.AllSummonerData.Summoner.Name)
                                continue;

                            var miniSeries = player.MiniSeries as TypedObject;
                            string series = "";
                            if (miniSeries != null)
                            {
                                series = ((string) miniSeries["progress"]).Replace('N', '-');
                                inPromo = true;
                            }
                            currentLp = (player.LeaguePoints == 100
                                ? series
                                : Convert.ToString(player.LeaguePoints));
                        }
                    }
                }
                else
                {
                    PlayerProgressBar.Value = 100;
                    PlayerProgressLabel.Content = "Level 30";
                    PlayerCurrentProgressLabel.Content = "";
                    PlayerAimProgressLabel.Content = "";
                }

                PlayerProgressLabel.Content = currentTier;
                if (inPromo)
                {
                    PlayerCurrentProgressLabel.Content = currentLp.Replace('N', '-');
                    PlayerProgressBar.Value = 100;
                }
                else
                {
                    if (string.IsNullOrEmpty(currentLp))
                        currentLp = "0";

                    PlayerCurrentProgressLabel.Content = currentLp + "LP";
                    PlayerProgressBar.Value = Convert.ToInt32(currentLp);
                }
            }));
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            object objectGame = GameList[SelectedGame];
            var spectatorGame = objectGame as Dictionary<string, object>;
            string key = "";
            int gameId = 0;
            string platformId = "";

            if (spectatorGame != null)
                foreach (var pair in spectatorGame)
                {
                    if (pair.Key == "gameId")
                        gameId = (int) pair.Value;

                    if (pair.Key == "observers")
                    {
                        var keyArray = pair.Value as Dictionary<string, object>;
                        if (keyArray != null)
                            foreach (
                                var keyArrayPair in keyArray.Where(keyArrayPair => keyArrayPair.Key == "encryptionKey"))
                                key = keyArrayPair.Value as string;
                    }
                    if (pair.Key == "platformId")
                        platformId = pair.Value as string;
                }

            BaseRegion region = BaseRegion.GetRegion((string) SpectatorComboBox.SelectedValue);
            new ReplayRecorder(region.SpectatorIpAddress, gameId, platformId, key);
            RecordButton.IsEnabled = false;
            RecordButton.Content = "Recording...";
        }

        private void HoverLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Client.OverlayContainer.Content = new ChooseProfilePicturePage().Content;
            Client.OverlayContainer.Visibility = Visibility.Visible;
        }

        private void HoverLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            HoverLabel.HoverLabel.Content = "Change";
            HoverLabel.Opacity = 100;
        }

        private void HoverLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            HoverLabel.Opacity = 0;
        }

        private void InviteTest_Click(object sender, RoutedEventArgs e)
        {
            //Client.OverlayContainer.Content = new GameInviteTest().Content;
            //Client.OverlayContainer.Visibility = Visibility.Visible;
        }

        private void PVPNet_OnMessageReceived(object sender, object message)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (!(message is BroadcastNotification))
                    return;

                var notif = message as BroadcastNotification;
                if (notif.BroadcastMessages == null)
                    return;

                var Message = notif.BroadcastMessages[0] as Dictionary<string, object>;
                if (Message != null && (bool) Message["active"])
                    BroadcastMessage.Text = Convert.ToString(Message["content"]);
                else
                    BroadcastMessage.Text = "";
            }));
        }

        private void fakeend_Click(object sender, RoutedEventArgs e)
        {
            Client.PVPNet.SimulateEndOfGame();
        }

        #region News

        private void GetNews(BaseRegion region)
        {
            var worker = new BackgroundWorker();
            worker.DoWork += delegate
            {
                string newsXml;
                using (var webClient = new WebClient())
                {
                    // To skip the 403 Error (Forbbiden)
                    try
                    {
                        webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)");
                        webClient.Headers.Add("Content-Type", "text/html; charset=UTF-8");
                        webClient.Headers.Add("Accept-Encoding", "gzip,deflate,sdch");
                        webClient.Headers.Add("Referer", "http://google.com/");
                        webClient.Headers.Add("Accept",
                            "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                    }
                    catch { }

                    newsXml = webClient.DownloadString(region.NewsAddress);
                }
                string newsJson;
                try
                {
                    var xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(newsXml);
                    newsJson = JsonConvert.SerializeXmlNode(xmlDocument);
                }
                catch
                {
                    newsJson = newsXml;
                }
                var deserializedJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(newsJson);
                var rss = deserializedJson["rss"] as Dictionary<string, object>;
                if (rss == null)
                    return;

                var channel = rss["channel"] as Dictionary<string, object>;
                if (channel != null)
                    NewsList = channel["item"] as ArrayList;
            };

            worker.RunWorkerCompleted += delegate { ParseNews(region); };

            worker.RunWorkerAsync();
        }

        private void ChatTest_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ParseNews(BaseRegion region)
        {
            if (NewsList == null)
                return;

            if (NewsList.Count <= 0)
                return;

            string imageUri = string.Empty;
            foreach (Dictionary<string, object> pair in NewsList)
            {
                var item = new NewsItem
                {
                    Margin = new Thickness(0, 5, 0, 5)
                };
                foreach (var kvPair in pair)
                {
                    if (kvPair.Key == "title")
                        item.NewsTitle.Content = kvPair.Value;

                    if (kvPair.Key == "description")
                    {
                        imageUri =
                            ((string) kvPair.Value).Substring(
                                ((string) kvPair.Value).IndexOf("src", StringComparison.Ordinal) + 6);
                        imageUri = imageUri.Remove(imageUri.IndexOf("?itok", StringComparison.Ordinal));

                        string noHtml = Regex.Replace(((string) kvPair.Value), @"<[^>]+>|&nbsp;", "").Trim();
                        string noHtmlNormalised = Regex.Replace(noHtml, @"\s{2,}", " ");

                        item.DescriptionLabel.Text = noHtmlNormalised;
                    }

                    if (kvPair.Key == "link")
                        item.Tag = kvPair.Value;

                    var promoImage = new BitmapImage();
                    promoImage.BeginInit();
                    promoImage.UriSource =
                        new Uri("http://" + region.RegionName + ".leagueoflegends.com/" + imageUri,
                            UriKind.RelativeOrAbsolute);
                    promoImage.CacheOption = BitmapCacheOption.OnLoad;
                    promoImage.EndInit();
                    item.PromoImage.Source = promoImage;
                }
                NewsItemListView.Items.Add(item);
            }
        }

        private void NewsItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NewsItemListView.SelectedIndex == -1)
                return;

            var item = (NewsItem) NewsItemListView.SelectedItem;
            Process.Start((string) item.Tag); //Launch the news article in browser
        }

        #endregion News

        #region Featured Games

        private void SpectatorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SpectatorComboBox.SelectedIndex == -1 || SpectatorComboBox.SelectedValue == null)
                return;

            BaseRegion region = BaseRegion.GetRegion((string) SpectatorComboBox.SelectedValue);
            ChangeSpectatorRegion(region);
        }

        public void ChangeSpectatorRegion(BaseRegion region)
        {
            try
            {
                // @TODO: Move to global worker to prevent mutiple requests on fast region switch
                var worker = new BackgroundWorker();
                worker.DoWork += delegate
                {
                    try
                    {
                        string spectatorJson;
                        using (var client = new WebClient())
                            spectatorJson = client.DownloadString(region.SpectatorLink + "featured");

                        var serializer = new JavaScriptSerializer();
                        var deserializedJson = serializer.Deserialize<Dictionary<string, object>>(spectatorJson);
                        GameList = deserializedJson["gameList"] as ArrayList;
                    }
                    catch (WebException)
                    {
                        Client.Log("Spectator JSON download timed out.");
                    }
                };

                worker.RunWorkerCompleted += (s, args) => ParseSpectatorGames();
                worker.RunWorkerAsync();
            }
            catch
            {
            }
        }

        private void ParseSpectatorGames()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                try
                {
                    if (GameList == null)
                        return;

                    if (GameList.Count <= 0)
                        return;

                    BlueBansLabel.Visibility = Visibility.Hidden;
                    PurpleBansLabel.Visibility = Visibility.Hidden;
                    BlueBanListView.Items.Clear();
                    PurpleBanListView.Items.Clear();
                    BlueListView.Items.Clear();
                    PurpleListView.Items.Clear();
                    int gameId = 0;
                    object objectGame = GameList[SelectedGame];
                    var spectatorGame = objectGame as Dictionary<string, object>;
                    ImageGrid.Children.Clear();
                    if (spectatorGame != null)
                        foreach (var pair in spectatorGame)
                        {
                            switch (pair.Key)
                            {
                                case "participants":
                                {
                                    var players = pair.Value as ArrayList;

                                    int i = 0;
                                    foreach (object objectPlayer in players)
                                    {
                                        var playerInfo = objectPlayer as Dictionary<string, object>;
                                        int teamId = 100;
                                        int championId = 0;
                                        int spell1Id = 0;
                                        int spell2Id = 0;
                                        string playerName = "";
                                        foreach (var playerPair in playerInfo)
                                        {
                                            switch (playerPair.Key)
                                            {
                                                case "teamId":
                                                    teamId = (int) playerPair.Value;
                                                    break;
                                                case "championId":
                                                    championId = (int) playerPair.Value;
                                                    break;
                                                case "summonerName":
                                                    playerName = playerPair.Value as string;
                                                    break;
                                                case "spell1Id":
                                                    spell1Id = (int) playerPair.Value;
                                                    break;
                                                case "spell2Id":
                                                    spell2Id = (int) playerPair.Value;
                                                    break;
                                            }
                                        }
                                        var control = new ChampSelectPlayer
                                        {
                                            ChampionImage = {Source = champions.GetChampion(championId).icon}
                                        };
                                        var uriSource =
                                            new Uri(
                                                Path.Combine(Client.ExecutingDirectory, "Assets", "spell",
                                                    SummonerSpell.GetSpellImageName(spell1Id)), UriKind.Absolute);
                                        control.SummonerSpell1.Source = new BitmapImage(uriSource);
                                        uriSource =
                                            new Uri(
                                                Path.Combine(Client.ExecutingDirectory, "Assets", "spell",
                                                    SummonerSpell.GetSpellImageName(spell2Id)), UriKind.Absolute);
                                        control.SummonerSpell2.Source = new BitmapImage(uriSource);
                                        control.PlayerName.Content = playerName;

                                        var m = new Image();
                                        Panel.SetZIndex(m, -2); //Put background behind everything
                                        m.Stretch = Stretch.None;
                                        m.Width = 100;
                                        m.Opacity = 0.30;
                                        m.HorizontalAlignment = HorizontalAlignment.Left;
                                        m.VerticalAlignment = VerticalAlignment.Stretch;
                                        m.Margin = new Thickness(i++*100, 0, 0, 0);
                                        var cropRect = new Rectangle(new Point(100, 0), new Size(100, 560));
                                        var src =
                                            System.Drawing.Image.FromFile(Path.Combine(Client.ExecutingDirectory,
                                                "Assets",
                                                "champions", champions.GetChampion(championId).portraitPath)) as Bitmap;
                                        var target = new Bitmap(cropRect.Width, cropRect.Height);

                                        using (Graphics g = Graphics.FromImage(target))
                                            g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                                                cropRect,
                                                GraphicsUnit.Pixel);

                                        m.Source = Client.ToWpfBitmap(target);
                                        ImageGrid.Children.Add(m);
                                        if (teamId == 100)
                                            BlueListView.Items.Add(control);
                                        else
                                            PurpleListView.Items.Add(control);
                                    }
                                }
                                    break;
                                case "gameId":
                                    gameId = (int) pair.Value;
                                    break;
                                case "mapId":
                                    try
                                    {
                                        MapLabel.Content = BaseMap.GetMap((int) pair.Value).DisplayName;
                                    }
                                    catch (Exception e)
                                    {
                                        Client.Log(e.Source, "Error");
                                        Client.Log(e.Message, "Error");
                                    }
                                    break;
                                case "gameLength":
                                {
                                    var seconds = (int) pair.Value;
                                    Client.spectatorTimer.Stop();
                                    Client.spectatorTimer = new Timer(1000);
                                    Client.spectatorTimer.Elapsed += (s, e) => // Sincerely Idk when to stop it
                                    {
                                        seconds ++;
                                        Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                                        {
                                            TimeSpan ts = TimeSpan.FromSeconds(seconds);
                                            GameTimeLabel.Content = string.Format("{0:D2}:{1:D2} min", ts.Minutes,
                                                ts.Seconds);
                                        }));
                                    };
                                    Client.spectatorTimer.Start();
                                }
                                    break;
                                case "bannedChampions":
                                {
                                    //ArrayList players = pair.Value as ArrayList;
                                    //Dictionary<string, object> playerInfo = objectPlayer as Dictionary<string, object>;
                                    //foreach (KeyValuePair<string, object> playerPair in playerInfo)
                                    var keyArray = pair.Value as ArrayList;
                                    if (keyArray.Count > 0)
                                    {
                                        BlueBansLabel.Visibility = Visibility.Visible;
                                        PurpleBansLabel.Visibility = Visibility.Visible;
                                    }
                                    foreach (Dictionary<string, object> keyArrayP in keyArray)
                                        //Dictionary<string, object> keyArrayP = keyArray as Dictionary<string, object>;
                                    {
                                        int cid = 0;
                                        int teamId = 100;
                                        foreach (var keyArrayPair in keyArrayP)
                                        {
                                            switch (keyArrayPair.Key)
                                            {
                                                case "championId":
                                                    cid = (int) keyArrayPair.Value;
                                                    break;
                                                case "teamId":
                                                    teamId = (int) keyArrayPair.Value;
                                                    break;
                                            }
                                        }
                                        var item = new ListViewItem();
                                        var champImage = new Image
                                        {
                                            Height = 58,
                                            Width = 58
                                        };
                                        //temp
                                        try
                                        {
                                            champImage.Source = champions.GetChampion(cid).icon;
                                        }
                                        catch
                                        {
                                        }

                                        item.Content = champImage;
                                        if (teamId == 100)
                                            BlueBanListView.Items.Add(item);
                                        else
                                            PurpleBanListView.Items.Add(item);
                                    }
                                }
                                    break;
                            }
                        }

                    try
                    {
                        BaseRegion region = BaseRegion.GetRegion((string) SpectatorComboBox.SelectedValue);
                        string spectatorJson;
                        string url = region.SpectatorLink + "consumer/getGameMetaData/" + region.InternalName + "/" +
                                     gameId + "/token";
                        using (var client = new WebClient())
                            spectatorJson = client.DownloadString(url);

                        var serializer = new JavaScriptSerializer();
                        var deserializedJson = serializer.Deserialize<Dictionary<string, object>>(spectatorJson);
                        MMRLabel.Content = "≈" + deserializedJson["interestScore"];
                    }
                    catch
                    {
                        MMRLabel.Content = "N/A";
                    }

                    if (Client.curentlyRecording.Contains(gameId))
                    {
                        RecordButton.IsEnabled = false;
                        RecordButton.Content = "Recording...";
                    }
                    else
                    {
                        RecordButton.IsEnabled = true;
                        RecordButton.Content = "Record";
                    }
                }
                catch (Exception e)
                {
                    Client.Log(e.Message);
                }
            }));
        }

        private void SpectateButton_Click(object sender, RoutedEventArgs e)
        {
            object objectGame = GameList[SelectedGame];
            var spectatorGame = objectGame as Dictionary<string, object>;
            string key = "";
            int gameId = 0;
            string platformId = "";

            if (spectatorGame != null)
                foreach (var pair in spectatorGame)
                {
                    switch (pair.Key)
                    {
                        case "gameId":
                            gameId = (int) pair.Value;
                            break;
                        case "observers":
                        {
                            var keyArray = pair.Value as Dictionary<string, object>;
                            if (keyArray != null)
                                foreach (
                                    var keyArrayPair in
                                        keyArray.Where(keyArrayPair => keyArrayPair.Key == "encryptionKey"))
                                    key = keyArrayPair.Value as string;
                        }
                            break;
                        case "platformId":
                            platformId = pair.Value as string;
                            break;
                    }
                }

            BaseRegion region = BaseRegion.GetRegion((string) SpectatorComboBox.SelectedValue);

            Client.LaunchSpectatorGame(region.SpectatorIpAddress, key, gameId, platformId);
        }

        private void NextGameButton_Click(object sender, RoutedEventArgs e)
        {
            NextGameButton.IsEnabled = true;
            PrevGameButton.IsEnabled = true;
            SelectedGame = SelectedGame + 1;
            if (SelectedGame >= GameList.Count - 1)
                NextGameButton.IsEnabled = false;

            ParseSpectatorGames();
        }

        private void PrevGameButton_Click(object sender, RoutedEventArgs e)
        {
            NextGameButton.IsEnabled = true;
            PrevGameButton.IsEnabled = true;
            SelectedGame = SelectedGame - 1;
            if (SelectedGame == 0)
                PrevGameButton.IsEnabled = false;

            ParseSpectatorGames();
        }

        #endregion Featured Games
    }
}