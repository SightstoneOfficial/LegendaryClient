using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.Maps;
using LegendaryClient.Logic.PlayerSpell;
using LegendaryClient.Logic.Region;
using LegendaryClient.Logic.Replays;
using LegendaryClient.Logic.SQLite;
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

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage
    {
        internal static Timer timer = new Timer();
        internal int SelectedGame = 0;
        internal List<int> curentlyRecording;
        internal ArrayList gameList;
        internal ArrayList newsList;

        public MainPage()
        {
            InitializeComponent();
            curentlyRecording = new List<int>();
            AppDomain current = AppDomain.CurrentDomain;
            GotPlayerData(Client.LoginPacket);
            SpectatorComboBox.SelectedValue = Client.Region.RegionName;
            BaseRegion region = BaseRegion.GetRegion(Client.Region.RegionName);
            uiLogic.CreateProfile(Client.LoginPacket.AllSummonerData.Summoner.Name);
            ChangeSpectatorRegion(region);
            GetNews(region);
            var update = new Timer();
            update.Interval = 5000;
            update.Elapsed +=
                (o, e) =>
                {
                    Client.ChatClient.Presence(Client.CurrentPresence, Client.GetPresence(), Client.presenceStatus, 0);
                };
            timer.Interval = (5000);
            //timer.Start();

            timer.Elapsed += (o, e) =>
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    string JID =
                        Client.GetChatroomJID(Client.GetObfuscatedChatroomName("legendaryclient", ChatPrefixes.Public),
                            string.Empty, true);

                    GroupChatItem item = Join(JID, "LegendaryClient");
                    var ChatGroup = new NotificationChatGroup();
                    ChatGroup.Tag = item;
                    ChatGroup.GroupTitle = item.GroupTitle;
                    ChatGroup.Margin = new Thickness(1, 0, 1, 0);
                    ChatGroup.GroupLabelName.Content = item.GroupTitle;
                    if (!Client.GroupChatItems.Any(i => i.GroupTitle == "LegendaryClient"))
                    {
                        Client.ChatListView.Items.Add(ChatGroup);
                        Client.GroupChatItems.Add(item);
                    }

                    timer.Stop();
                }));
            };
        }

        [STAThread]
        private GroupChatItem Join(string JID, string Chat)
        {
            return new GroupChatItem(JID, "LegendaryClient");
        }

        private void GotPlayerData(LoginDataPacket packet)
        {
            Client.PVPNet.OnMessageReceived += PVPNet_OnMessageReceived;
            UpdateSummonerInformation();
        }

        internal async void UpdateSummonerInformation()
        {
            AllSummonerData PlayerData = await Client.PVPNet.GetAllSummonerDataByAccount(Client.LoginPacket.AllSummonerData.Summoner.AcctId);
            SummonerNameLabel.Content = PlayerData.Summoner.Name;
            if (Client.LoginPacket.AllSummonerData.SummonerLevel.Level < 30)
            {
                PlayerProgressBar.Value = (PlayerData.SummonerLevelAndPoints.ExpPoints / PlayerData.SummonerLevel.ExpToNextLevel) * 100;
                PlayerProgressLabel.Content = String.Format("Level {0}", PlayerData.SummonerLevel.Level);
                PlayerCurrentProgressLabel.Content = String.Format("{0}XP", PlayerData.SummonerLevelAndPoints.ExpPoints);
                PlayerAimProgressLabel.Content = String.Format("{0}XP", PlayerData.SummonerLevel.ExpToNextLevel);
            }
            else
            {
                Client.PVPNet.GetAllLeaguesForPlayer(PlayerData.Summoner.SumId, GotLeaguesForPlayer);
            }

            if (Client.LoginPacket.BroadcastNotification.BroadcastMessages != null)
            {
                var Message = Client.LoginPacket.BroadcastNotification.BroadcastMessages[0] as Dictionary<string, object>;
                BroadcastMessage.Text = Convert.ToString(Message["content"]);
            }

            foreach (PlayerStatSummary x in Client.LoginPacket.PlayerStatSummaries.PlayerStatSummarySet)
            {
                if (x.PlayerStatSummaryTypeString == "Unranked")
                {
                    Client.IsRanked = false;
                    Client.AmountOfWins = x.Wins;
                }
                if (x.PlayerStatSummaryTypeString == "RankedSolo5x5")
                {
                    Client.IsRanked = true;
                    Client.AmountOfWins = x.Wins;
                    break;
                }
            }

            Client.InfoLabel.Content = "IP: " + Client.LoginPacket.IpBalance + " ∙ RP: " + Client.LoginPacket.RpBalance;
            int ProfileIconID = Client.LoginPacket.AllSummonerData.Summoner.ProfileIconId;
            var uriSource =
                new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", ProfileIconID + ".png"),
                    UriKind.RelativeOrAbsolute);
            ProfileImage.Source = new BitmapImage(uriSource);
            Client.MainPageProfileImage = ProfileImage;
        }

        private void GotLeaguesForPlayer(SummonerLeaguesDTO result)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                string CurrentLP = "";
                string CurrentTier = "";
                bool InPromo = false;
                if (result.SummonerLeagues != null && result.SummonerLeagues.Count > 0)
                {
                    foreach (LeagueListDTO leagues in result.SummonerLeagues)
                    {
                        if (leagues.Queue == "RANKED_SOLO_5x5")
                        {
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
                                if (player.PlayerOrTeamName == Client.LoginPacket.AllSummonerData.Summoner.Name)
                                {
                                    var miniSeries = player.MiniSeries as TypedObject;
                                    string Series = "";
                                    if (miniSeries != null)
                                    {
                                        Series = ((string) miniSeries["progress"]).Replace('N', '-');
                                        InPromo = true;
                                    }
                                    CurrentLP = (player.LeaguePoints == 100
                                        ? Series
                                        : Convert.ToString(player.LeaguePoints));
                                }
                            }
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

                PlayerProgressLabel.Content = CurrentTier;
                if (InPromo)
                {
                    PlayerCurrentProgressLabel.Content = CurrentLP.Replace('N', '-');
                    PlayerProgressBar.Value = 100;
                }
                else
                {
                    if (string.IsNullOrEmpty(CurrentLP))
                    {
                        CurrentLP = "0";
                    }
                    PlayerCurrentProgressLabel.Content = CurrentLP + "LP";
                    PlayerProgressBar.Value = Convert.ToInt32(CurrentLP);
                }
            }));
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            object objectGame = gameList[SelectedGame];
            var SpectatorGame = objectGame as Dictionary<string, object>;
            string key = "";
            int gameId = 0;
            string platformId = "";

            foreach (var pair in SpectatorGame)
            {
                if (pair.Key == "gameId")
                {
                    gameId = (int) pair.Value;
                }
                if (pair.Key == "observers")
                {
                    var keyArray = pair.Value as Dictionary<string, object>;
                    foreach (var keyArrayPair in keyArray)
                    {
                        if (keyArrayPair.Key == "encryptionKey")
                        {
                            key = keyArrayPair.Value as string;
                        }
                    }
                }
                if (pair.Key == "platformId")
                {
                    platformId = pair.Value as string;
                }
            }

            BaseRegion region = BaseRegion.GetRegion((string) SpectatorComboBox.SelectedValue);
            //region.SpectatorIpAddress, key, gameId, platformId
            var recorder = new ReplayRecorder(region.SpectatorIpAddress, gameId, platformId, key);
            recorder.OnReplayRecorded += () => { curentlyRecording.Remove(gameId); };
            curentlyRecording.Add(gameId);
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
                if (message is BroadcastNotification)
                {
                    var notif = message as BroadcastNotification;
                    if (notif.BroadcastMessages != null)
                    {
                        var Message = notif.BroadcastMessages[0] as Dictionary<string, object>;
                        if ((bool) Message["active"])
                        {
                            BroadcastMessage.Text = Convert.ToString(Message["content"]);
                        }
                        else
                        {
                            BroadcastMessage.Text = "";
                        }
                    }
                }
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
                string newsJSON = "";
                using (var client = new WebClient())
                {
                    newsJSON = client.DownloadString(region.NewsAddress);
                }
                var serializer = new JavaScriptSerializer();
                var deserializedJSON = serializer.Deserialize<Dictionary<string, object>>(newsJSON);
                newsList = deserializedJSON["news"] as ArrayList;
                var promoList = deserializedJSON["promos"] as ArrayList;
                foreach (Dictionary<string, object> objectPromo in promoList)
                {
                    newsList.Add(objectPromo);
                }
            };

            worker.RunWorkerCompleted += delegate { ParseNews(); };

            worker.RunWorkerAsync();
        }

        private void ChatTest_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ParseNews()
        {
            if (newsList == null)
                return;
            if (newsList.Count <= 0)
                return;
            foreach (Dictionary<string, object> pair in newsList)
            {
                var item = new NewsItem();
                item.Margin = new Thickness(0, 5, 0, 5);
                foreach (var kvPair in pair)
                {
                    if (kvPair.Key == "title")
                    {
                        item.NewsTitle.Content = kvPair.Value;
                    }
                    if (kvPair.Key == "description" || kvPair.Key == "promoText")
                    {
                        item.DescriptionLabel.Text = (string) kvPair.Value;
                    }
                    if (kvPair.Key == "thumbUrl")
                    {
                        var promoImage = new BitmapImage();
                        promoImage.BeginInit(); //Download image
                        promoImage.UriSource = new Uri((string) kvPair.Value, UriKind.RelativeOrAbsolute);
                        promoImage.CacheOption = BitmapCacheOption.OnLoad;
                        promoImage.EndInit();
                        item.PromoImage.Source = promoImage;
                    }
                    if (kvPair.Key == "linkUrl")
                    {
                        item.Tag = kvPair.Value;
                    }
                }
                NewsItemListView.Items.Add(item);
            }
        }

        private void NewsItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NewsItemListView.SelectedIndex != -1)
            {
                var item = (NewsItem) NewsItemListView.SelectedItem;
                Process.Start((string) item.Tag); //Launch the news article in browser
            }
        }

        #endregion News

        #region Featured Games

        private void SpectatorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SpectatorComboBox.SelectedIndex != -1 && SpectatorComboBox.SelectedValue != null)
            {
                BaseRegion region = BaseRegion.GetRegion((string) SpectatorComboBox.SelectedValue);
                ChangeSpectatorRegion(region);
            }
        }

        private void ChangeSpectatorRegion(BaseRegion region)
        {
            try
            {
                // @TODO: Move to global worker to prevent mutiple requests on fast region switch
                var worker = new BackgroundWorker();
                worker.DoWork += delegate
                {
                    try
                    {
                        string spectatorJSON = "";
                        using (var client = new WebClient())
                        {
                            spectatorJSON = client.DownloadString(region.SpectatorLink + "featured");
                        }
                        var serializer = new JavaScriptSerializer();
                        var deserializedJSON = serializer.Deserialize<Dictionary<string, object>>(spectatorJSON);
                        gameList = deserializedJSON["gameList"] as ArrayList;
                    }
                    catch (WebException e)
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

        private async void ParseSpectatorGames()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                try
                {
                    if (gameList == null)
                        return;
                    if (gameList.Count <= 0)
                        return;
                    BlueBansLabel.Visibility = Visibility.Hidden;
                    PurpleBansLabel.Visibility = Visibility.Hidden;
                    BlueBanListView.Items.Clear();
                    PurpleBanListView.Items.Clear();
                    BlueListView.Items.Clear();
                    PurpleListView.Items.Clear();
                    int GameId = 0;
                    object objectGame = gameList[SelectedGame];
                    var SpectatorGame = objectGame as Dictionary<string, object>;
                    ImageGrid.Children.Clear();
                    foreach (var pair in SpectatorGame)
                    {
                        if (pair.Key == "participants")
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
                                string PlayerName = "";
                                foreach (var playerPair in playerInfo)
                                {
                                    if (playerPair.Key == "teamId")
                                    {
                                        teamId = (int) playerPair.Value;
                                    }
                                    if (playerPair.Key == "championId")
                                    {
                                        championId = (int) playerPair.Value;
                                    }
                                    if (playerPair.Key == "summonerName")
                                    {
                                        PlayerName = playerPair.Value as string;
                                    }
                                    if (playerPair.Key == "spell1Id")
                                    {
                                        spell1Id = (int) playerPair.Value;
                                    }
                                    if (playerPair.Key == "spell2Id")
                                    {
                                        spell2Id = (int) playerPair.Value;
                                    }
                                }
                                var control = new ChampSelectPlayer();
                                control.ChampionImage.Source = champions.GetChampion(championId).icon;
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
                                control.PlayerName.Content = PlayerName;

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
                                    System.Drawing.Image.FromFile(Path.Combine(Client.ExecutingDirectory, "Assets",
                                        "champions", champions.GetChampion(championId).portraitPath)) as Bitmap;
                                var target = new Bitmap(cropRect.Width, cropRect.Height);

                                using (Graphics g = Graphics.FromImage(target))
                                {
                                    g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                                        cropRect,
                                        GraphicsUnit.Pixel);
                                }

                                m.Source = Client.ToWpfBitmap(target);
                                ImageGrid.Children.Add(m);

                                if (teamId == 100)
                                {
                                    BlueListView.Items.Add(control);
                                }
                                else
                                {
                                    PurpleListView.Items.Add(control);
                                }
                            }
                        }
                        else if (pair.Key == "gameId")
                        {
                            GameId = (int) pair.Value;
                        }
                            //tried this, caused a crash, worked fine when deleted
                        else if (pair.Key == "mapId")
                        {
                            try
                            {
                                MapLabel.Content = BaseMap.GetMap((int) pair.Value).DisplayName;
                            }
                            catch (Exception e)
                            {
                                Client.Log(e.Source, "Error");
                                Client.Log(e.Message, "Error");
                            }
                        }
                        else if (pair.Key == "gameLength")
                        {
                            var seconds = (int) pair.Value;
                            GameTimeLabel.Content = string.Format("{0:D}:{1:00} min", seconds/60, seconds%60);
                        }
                        else if (pair.Key == "bannedChampions")
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
                                    if (keyArrayPair.Key == "championId")
                                    {
                                        cid = (int) keyArrayPair.Value;
                                    }
                                    if (keyArrayPair.Key == "teamId")
                                    {
                                        teamId = (int) keyArrayPair.Value;
                                    }
                                }
                                var item = new ListViewItem();
                                var champImage = new Image();
                                champImage.Height = 58;
                                champImage.Width = 58;
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
                                {
                                    BlueBanListView.Items.Add(item);
                                }
                                else
                                {
                                    PurpleBanListView.Items.Add(item);
                                }
                            }
                        }
                    }

                    try
                    {
                        BaseRegion region = BaseRegion.GetRegion((string) SpectatorComboBox.SelectedValue);
                        string spectatorJSON = "";
                        string url = region.SpectatorLink + "consumer/getGameMetaData/" + region.InternalName + "/" +
                                     GameId + "/token";
                        using (var client = new WebClient())
                        {
                            spectatorJSON = client.DownloadString(url);
                        }
                        var serializer = new JavaScriptSerializer();
                        var deserializedJSON = serializer.Deserialize<Dictionary<string, object>>(spectatorJSON);
                        MMRLabel.Content = "≈" + deserializedJSON["interestScore"];
                    }
                    catch
                    {
                        MMRLabel.Content = "N/A";
                    }

                    if (curentlyRecording.Contains(GameId))
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
            object objectGame = gameList[SelectedGame];
            var SpectatorGame = objectGame as Dictionary<string, object>;
            string key = "";
            int gameId = 0;
            string platformId = "";

            foreach (var pair in SpectatorGame)
            {
                if (pair.Key == "gameId")
                {
                    gameId = (int) pair.Value;
                }
                if (pair.Key == "observers")
                {
                    var keyArray = pair.Value as Dictionary<string, object>;
                    foreach (var keyArrayPair in keyArray)
                    {
                        if (keyArrayPair.Key == "encryptionKey")
                        {
                            key = keyArrayPair.Value as string;
                        }
                    }
                }
                if (pair.Key == "platformId")
                {
                    platformId = pair.Value as string;
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
            if (SelectedGame >= gameList.Count - 1)
            {
                NextGameButton.IsEnabled = false;
            }
            ParseSpectatorGames();
        }

        private void PrevGameButton_Click(object sender, RoutedEventArgs e)
        {
            NextGameButton.IsEnabled = true;
            PrevGameButton.IsEnabled = true;
            SelectedGame = SelectedGame - 1;
            if (SelectedGame == 0)
            {
                PrevGameButton.IsEnabled = false;
            }
            ParseSpectatorGames();
        }

        #endregion Featured Games
    }
}