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
using LegendaryClient.Scripting_Environment;
using Newtonsoft.Json;
using Image = System.Windows.Controls.Image;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using Timer = System.Timers.Timer;
using LegendaryClient.Logic.Crypto;
using LegendaryClient.Logic.Riot;
using LegendaryClient.Logic.Riot.Leagues;
using LegendaryClient.Logic.Riot.Platform;
using RtmpSharp.Messaging;
using agsXMPP.protocol.client;

#endregion

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for MainPage.xaml
    /// </summary>
    /// 

    public partial class MainPage
    {
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollviewer = sender as ScrollViewer;
            if (e.Delta > 0)
                scrollviewer.LineLeft();
            else
                scrollviewer.LineRight();
            e.Handled = true;
        }

        internal static Timer timer = new Timer();
        internal ArrayList GameList;
        internal ArrayList NewsList;
        internal int SelectedGame = 0;
        internal bool CheckedDev = false;
        public MainPage()
        {
            InitializeComponent();
            GotPlayerData(Client.LoginPacket);
            SpectatorComboBox.SelectedValue = Client.Region.RegionName.ToUpper();
            BaseRegion region = BaseRegion.GetRegion(Client.Region.RegionName);
            Client.Profile = new ProfilePage();
            GetNews(region);
            ChangeSpectatorRegion(Client.Region);
            var update = new Timer
            {
                Interval = 5000
            };
            update.Elapsed +=
                (o, e) =>
                    Client.XmppConnection.Send(new Presence(Client.presenceStatus, Client.GetPresence(), 0));
            timer.Interval = (5000);
            //timer.Start();

            timer.Elapsed += (o, e) => Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                string Jid =
                    Client.GetChatroomJid(Client.GetObfuscatedChatroomName("legendaryclient", ChatPrefixes.Public),
                        string.Empty, true);

                GroupChatItem item = Join(Jid, "LegendaryClient");
                var chatGroup = new NotificationChatGroup
                {
                    Tag = item,
                    GroupTitle = item.GroupTitle,
                    Margin = new Thickness(1, 0, 1, 0),
                    GroupLabelName = { Content = item.GroupTitle }
                };
                if (Client.GroupChatItems.All(i => i.GroupTitle != "LegendaryClient"))
                {
                    Client.ChatListView.Items.Add(chatGroup);
                    Client.GroupChatItems.Add(item);
                }

                timer.Stop();
            }));

            //Update featured games every minute.
            var featuredUpdateTimer = new Timer
            {
                Interval = 60000
            };
            featuredUpdateTimer.Elapsed += (o, e) =>
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    if (SpectatorComboBox.SelectedIndex == -1 || SpectatorComboBox.SelectedValue == null)
                    {
                        ChangeSpectatorRegion(region);
                    }
                    else
                    {
                        ChangeSpectatorRegion(BaseRegion.GetRegion((string)SpectatorComboBox.SelectedValue));
                    }
                }));
            };
            featuredUpdateTimer.Start();
        }

        [STAThread]
        private GroupChatItem Join(string Jid, string chat)
        {
            return new GroupChatItem(Jid, chat);
        }

        private void GotPlayerData(LoginDataPacket packet)
        {
            Client.RiotConnection.MessageReceived += PVPNet_OnMessageReceived;
            UpdateSummonerInformation();
        }

        internal async void UpdateSummonerInformation()
        {
            if (Client.IsLoggedIn)
            {
                AllSummonerData playerData =
                    await RiotCalls.GetAllSummonerDataByAccount(Client.LoginPacket.AllSummonerData.Summoner.AcctId);
                SummonerNameLabel.Content = playerData.Summoner.Name;
                

                SummonerActiveBoostsDTO activeBoost = await RiotCalls.GetSummonerActiveBoosts();

                string xpBoostTime = ConvertBoostTime(activeBoost.XpBoostEndDate);
                if (xpBoostTime != string.Empty && activeBoost.XpBoostEndDate > 0)
                {
                    XPBoost.Content = "XP Boost " + ConvertBoostTime(activeBoost.XpBoostEndDate) + ". " + activeBoost.XpBoostPerWinCount + " Win.";
                }
                else if (xpBoostTime != string.Empty)
                {
                    XPBoost.Content = "XP Boost " + ConvertBoostTime(activeBoost.XpBoostEndDate) + ".";
                }
                else if (activeBoost.XpBoostPerWinCount > 0)
                {
                    XPBoost.Content = "XP Boost " + activeBoost.XpBoostPerWinCount + ".";
                }
                else
                {
                    XPBoost.Visibility = Visibility.Hidden;
                }

                string ipBoostTime = ConvertBoostTime(activeBoost.IpBoostEndDate);
                if (ipBoostTime != string.Empty && activeBoost.IpBoostEndDate > 0)
                {
                    IPBoost.Content = "IP Boost " + ipBoostTime + ". " + activeBoost.IpBoostPerWinCount + " Win.";
                }
                else if (ipBoostTime != string.Empty)
                {
                    IPBoost.Content = "IP Boost " + ipBoostTime + ".";
                }
                else if (activeBoost.IpBoostPerWinCount > 0)
                {
                    IPBoost.Content = "IP Boost " + activeBoost.IpBoostPerWinCount + ".";
                }
                else
                {
                    IPBoost.Visibility = Visibility.Hidden;
                }

                Sha1 sha1 = new Sha1();
                if (!CheckedDev)
                {
                    if (DevUsers.getDevelopers().Contains(sha1.HashString(playerData.Summoner.Name + " " + Client.Region.RegionName)))
                    {
                        Client.Dev = true;
                    }
                    CheckedDev = true;
                }

                if (Client.Dev)
                {
                    Client.UserTitleBarLabel.Content = "Dev ∙ " + playerData.Summoner.Name;
                }
                else
                {
                    Client.UserTitleBarLabel.Content = playerData.Summoner.Name;
                }

                if (Client.LoginPacket.AllSummonerData.SummonerLevel.Level < 30)
                {
                    PlayerProgressBar.Value = (playerData.SummonerLevelAndPoints.ExpPoints /
                                               playerData.SummonerLevel.ExpToNextLevel) * 100;
                    PlayerProgressLabel.Content = string.Format("Level {0}", playerData.SummonerLevel.Level);
                    PlayerCurrentProgressLabel.Content = string.Format("{0}XP", playerData.SummonerLevelAndPoints.ExpPoints);
                    PlayerAimProgressLabel.Content = string.Format("{0}XP", playerData.SummonerLevel.ExpToNextLevel);
                    Client.UserTitleBarLabel.Content = Client.UserTitleBarLabel.Content + string.Format(" ∙ Level: {0}", playerData.SummonerLevel.Level);
                }
                else
                    GotLeaguesForPlayer(await RiotCalls.GetAllLeaguesForPlayer(playerData.Summoner.SumId));

                if (Client.LoginPacket.BroadcastNotification.broadcastMessages != null)
                {
                    var message = Client.LoginPacket.BroadcastNotification.broadcastMessages[0];
                    if (message != null)
                    {
                        //BroadcastMessage.Text = message.Content;
                    }
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

                    if (x.Rating != 0)
                    {
                        Client.IsRanked = true;
                        Client.AmountOfWins = x.Wins;
                    }
                    break;
                }

                Client.InfoLabel.Content = "IP: " + Client.LoginPacket.IpBalance + " ∙ RP: " + Client.LoginPacket.RpBalance;
                int profileIconId = Client.LoginPacket.AllSummonerData.Summoner.ProfileIconId;
                var UriSource =
                    new System.Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", profileIconId + ".png"),
                    UriKind.RelativeOrAbsolute);
                try
                {
                    ProfileImage.Source = new BitmapImage(UriSource);
                    Client.UserTitleBarImage.Source = new BitmapImage(UriSource);
                }
                catch
                {
                    Client.Log("Can't load profile image.", "ERROR");
                }
                Client.MainPageProfileImage = ProfileImage;
            }
        }

        private string ConvertBoostTime(double millisTimeStamp)
        {
            string unixTimeStamp = millisTimeStamp.ToString();
            if (unixTimeStamp != "0")
            {
                unixTimeStamp = unixTimeStamp.Substring(0, unixTimeStamp.Length - 3);
                DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
                dtDateTime = dtDateTime.AddSeconds(double.Parse(unixTimeStamp));
                TimeSpan diff2 = dtDateTime.Subtract(DateTime.Now);
                if (diff2.Days >= 0 && diff2.Hours > 0)
                {
                    string time = diff2.Days.ToString() + " Days ";
                    time += diff2.Hours.ToString() + " Hours";
                    return time;
                }
                return string.Empty;
            }
            return string.Empty;
        }


        private void GotLeaguesForPlayer(SummonerLeaguesDTO result)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                string CurrentLP = "";
                string CurrentTier = "";
                bool InPromo = false;
                if (result.SummonerLeagues.Exists(x => x.Queue == "RANKED_SOLO_5x5") &&
                    result.SummonerLeagues.Count > 0)
                {
                    var leagues = result.SummonerLeagues.Single(x => x.Queue == "RANKED_SOLO_5x5");

                    Client.Tier = leagues.RequestorsRank;
                    Client.TierName = leagues.Tier;
                    Client.LeagueName = leagues.Name;
                    CurrentTier = leagues.Tier + " " + leagues.RequestorsRank;
                    List<LeagueItemDTO> players =
                        leagues.Entries.OrderBy(o => o.LeaguePoints)
                            .Where(item => item.Rank == leagues.RequestorsRank)
                            .ToList();
                    foreach (LeagueItemDTO player in players)
                    {
                        if (player.PlayerOrTeamName != Client.LoginPacket.AllSummonerData.Summoner.Name)
                            continue;

                        string Series = "";
                        if (player.MiniSeries != null)
                        {
                            Series = player.MiniSeries.Progress.Replace('N', '-');
                            InPromo = true;
                        }
                        CurrentLP = (player.LeaguePoints == 100 ? Series : Convert.ToString(player.LeaguePoints));
                    }
                }

                else
                {
                    PlayerProgressBar.Value = 100;
                    PlayerProgressLabel.Content = "Level 30";
                    PlayerCurrentProgressLabel.Content = "";
                    PlayerAimProgressLabel.Content = "";
                    return;
                }

                PlayerProgressLabel.Content = CurrentTier;
                if (InPromo)
                {
                    PlayerCurrentProgressLabel.Content = CurrentLP.Replace('N', '-');
                    PlayerProgressBar.Value = 100;
                }
                else
                {
                    PlayerCurrentProgressLabel.Content = CurrentLP + "LP";
                    PlayerProgressBar.Value = Convert.ToInt32(CurrentLP);
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
                        gameId = (int)pair.Value;

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

            BaseRegion region = BaseRegion.GetRegion((string)SpectatorComboBox.SelectedValue);
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

        private void PVPNet_OnMessageReceived(object sender, MessageReceivedEventArgs message)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (!(message.Body is BroadcastNotification))
                    return;

                //var notif = message as BroadcastNotification;
                //if (notif.BroadcastMessages == null)
                //    return;
                //
                //var Message = notif.BroadcastMessages[0];
                //if (Message != null && Message.Active)
                //    BroadcastMessage.Text = Message.Content;
                //else
                //    BroadcastMessage.Text = "";
            }));
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
                    }
                    catch
                    {
                    }

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
                var serializer = new JavaScriptSerializer();
                Dictionary<string, object> deserializedJson;
                try
                {
                    deserializedJson = serializer.Deserialize<Dictionary<string, object>>(newsJson);
                }
                catch
                {
                    deserializedJson = null;
                    return;
                }

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
                        if ((string)kvPair.Value == string.Empty)
                            continue;

                        imageUri =
                            ((string)kvPair.Value).Substring(
                                ((string)kvPair.Value).IndexOf("src", StringComparison.Ordinal) + 6);
                        if (imageUri.IndexOf("?itok", StringComparison.Ordinal) > 0)
                            imageUri = imageUri.Remove(imageUri.IndexOf("?itok", StringComparison.Ordinal));

                        string noHtml = Regex.Replace(((string)kvPair.Value), @"<[^>]+>|&nbsp;", "").Trim();
                        string noHtmlNormalised = Regex.Replace(noHtml, @"\s{2,}", " ");
                        string noXmlAmpersands = Regex.Replace(noHtmlNormalised, @"&amp;", "&");

                        item.DescriptionLabel.Text = noXmlAmpersands;
                    }

                    if (kvPair.Key == "link")
                        item.Tag = kvPair.Value;

                    // Image
                    if (!string.IsNullOrEmpty(imageUri))
                    {
                        var promoImage = new BitmapImage(new System.Uri("http://" + region.RegionName + ".leagueoflegends.com/" + imageUri, UriKind.Absolute));
                        item.PromoImage.Stretch = Stretch.Fill;
                        item.PromoImage.Source = promoImage;
                    }
                }
                NewsItemListView.Items.Add(item);
            }
        }

        private void NewsItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NewsItemListView.SelectedIndex == -1)
                return;

            var item = (NewsItem)NewsItemListView.SelectedItem;
            Process.Start((string)item.Tag); //Launch the news article in browser
        }

        #endregion News

        #region Featured Games

        private void SpectatorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SpectatorComboBox.SelectedIndex == -1 || SpectatorComboBox.SelectedValue == null)
                return;

            BaseRegion region = BaseRegion.GetRegion((string)SpectatorComboBox.SelectedValue);
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
                        {
                            client.Encoding = System.Text.Encoding.UTF8;
                            spectatorJson = client.DownloadString(region.SpectatorLink + "featured");
                        }

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
                    BlueListView.Items.Clear();
                    PurpleListView.Items.Clear();
                    int gameId = 0;
                    object objectGame = GameList[SelectedGame];
                    var spectatorGame = objectGame as Dictionary<string, object>;;
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
                                                        teamId = (int)playerPair.Value;
                                                        break;
                                                    case "championId":
                                                        championId = (int)playerPair.Value;
                                                        break;
                                                    case "summonerName":
                                                        playerName = playerPair.Value as string;
                                                        break;
                                                    case "spell1Id":
                                                        spell1Id = (int)playerPair.Value;
                                                        break;
                                                    case "spell2Id":
                                                        spell2Id = (int)playerPair.Value;
                                                        break;
                                                }
                                            }
                                            var control = new MainPagePlayer
                                            {
                                                ChampionImage = { Source = champions.GetChampion(championId).icon },
                                                champID = championId,
                                                sumName = playerName,
                                                KnownPar = true
                                            };


                                            control.Tag = new List<object> { playerName, championId };

                                            var m = new Image();
                                            Panel.SetZIndex(m, -2); //Put background behind everything
                                            m.Stretch = Stretch.None;
                                            m.Width = 100;
                                            m.Opacity = 0.30;
                                            m.HorizontalAlignment = HorizontalAlignment.Left;
                                            m.VerticalAlignment = VerticalAlignment.Stretch;
                                            m.Margin = new Thickness(i++ * 100, 0, 0, 0);
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
                                            if (teamId == 100)
                                                BlueListView.Items.Add(control);
                                            else
                                                PurpleListView.Items.Add(control);
                                        }
                                    }
                                    break;
                                case "gameId":
                                    gameId = (int)pair.Value;
                                    break;
                                case "mapId":
                                    try
                                    {
                                        MapLabel.Content = BaseMap.GetMap((int)pair.Value).DisplayName;
                                    }
                                    catch (Exception e)
                                    {
                                        Client.Log(e.Source, "Error");
                                        Client.Log(e.Message, "Error");
                                    }
                                    break;
                                case "gameLength":
                                    {
                                        var seconds = (int)pair.Value;
                                        Client.spectatorTimer.Stop();
                                        Client.spectatorTimer = new Timer(1000);
                                        Client.spectatorTimer.Elapsed += (s, e) => // Sincerely Idk when to stop it
                                        {
                                            seconds++;
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
                                    }
                                    break;
                            }
                        }

                    try
                    {
                        BaseRegion region = BaseRegion.GetRegion((string)SpectatorComboBox.SelectedValue);
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
                            gameId = (int)pair.Value;
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

            BaseRegion region = BaseRegion.GetRegion((string)SpectatorComboBox.SelectedValue);

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
