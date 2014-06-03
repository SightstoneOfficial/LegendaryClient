using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.Maps;
using LegendaryClient.Logic.PlayerSpell;
using LegendaryClient.Logic.Region;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect;
using PVPNetConnect.RiotObjects.Leagues.Pojo;
using PVPNetConnect.RiotObjects.Platform.Broadcast;
using PVPNetConnect.RiotObjects.Platform.Clientfacade.Domain;
using PVPNetConnect.RiotObjects.Platform.Leagues.Client.Dto;
using PVPNetConnect.RiotObjects.Platform.Statistics;
using PVPNetConnect.RiotObjects.Platform.Summoner;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        internal int SelectedGame = 0;
        internal ArrayList gameList;
        internal ArrayList newsList;

        public MainPage()
        {
            InitializeComponent();
            GotPlayerData(Client.LoginPacket);
            SpectatorComboBox.SelectedValue = Client.LoginPacket.CompetitiveRegion;
            BaseRegion region = BaseRegion.GetRegion(Client.LoginPacket.CompetitiveRegion);
            ChangeSpectatorRegion(region);

            GetNews(region);
        }

        private void GotPlayerData(LoginDataPacket packet)
        {
            Client.PVPNet.OnMessageReceived += PVPNet_OnMessageReceived;
            AllSummonerData PlayerData = packet.AllSummonerData;
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
                Client.PVPNet.GetAllLeaguesForPlayer(PlayerData.Summoner.SumId, new SummonerLeaguesDTO.Callback(GotLeaguesForPlayer));
            }

            if (packet.BroadcastNotification.BroadcastMessages != null)
            {
                Dictionary<string, object> Message = packet.BroadcastNotification.BroadcastMessages[0] as Dictionary<string, object>;
                BroadcastMessage.Text = Convert.ToString(Message["content"]);
            }

            foreach (PlayerStatSummary x in packet.PlayerStatSummaries.PlayerStatSummarySet)
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

            if (packet.ReconnectInfo != null)
            {
                ;
            }

            Client.InfoLabel.Content = "IP: " + Client.LoginPacket.IpBalance + " ∙ RP: " + Client.LoginPacket.RpBalance;
            int ProfileIconID = Client.LoginPacket.AllSummonerData.Summoner.ProfileIconId;
            var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", ProfileIconID + ".png"), UriKind.RelativeOrAbsolute);
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
                            List<LeagueItemDTO> players = leagues.Entries.OrderBy(o => o.LeaguePoints).Where(item => item.Rank == leagues.RequestorsRank).ToList();
                            foreach (LeagueItemDTO player in players)
                            {
                                if (player.PlayerOrTeamName == Client.LoginPacket.AllSummonerData.Summoner.Name)
                                {
                                    TypedObject miniSeries = player.MiniSeries as TypedObject;
                                    string Series = "";
                                    if (miniSeries != null)
                                    {
                                        Series = (string)miniSeries["progress"];
                                        InPromo = true;
                                    }
                                    CurrentLP = (player.LeaguePoints == 100 ? Series : Convert.ToString(player.LeaguePoints));
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
                    PlayerCurrentProgressLabel.Content = CurrentLP;
                    PlayerProgressBar.Value = 100;
                }
                else
                {
                    PlayerCurrentProgressLabel.Content = CurrentLP + "LP";
                    PlayerProgressBar.Value = Convert.ToInt32(CurrentLP);
                }
            }));
        }

        #region News

        private void GetNews(BaseRegion region)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                string newsJSON = "";
                using (WebClient client = new WebClient())
                {
                    newsJSON = client.DownloadString(region.NewsAddress);
                }
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> deserializedJSON = serializer.Deserialize<Dictionary<string, object>>(newsJSON);
                newsList = deserializedJSON["news"] as ArrayList;
                ArrayList promoList = deserializedJSON["promos"] as ArrayList;
                foreach (Dictionary<string, object> objectPromo in promoList)
                {
                    newsList.Add(objectPromo);
                }
            };

            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                ParseNews();
            };

            worker.RunWorkerAsync();
        }

        private void ParseNews()
        {
            if (newsList == null)
                return;
            if (newsList.Count <= 0)
                return;
            foreach (Dictionary<string, object> pair in newsList)
            {
                NewsItem item = new NewsItem();
                item.Margin = new System.Windows.Thickness(0, 5, 0, 5);
                foreach (KeyValuePair<string, object> kvPair in pair)
                {
                    if (kvPair.Key == "title")
                    {
                        item.NewsTitle.Content = kvPair.Value;
                    }
                    if (kvPair.Key == "description" || kvPair.Key == "promoText")
                    {
                        item.DescriptionLabel.Text = (string)kvPair.Value;
                    }
                    if (kvPair.Key == "thumbUrl")
                    {
                        BitmapImage promoImage = new BitmapImage();
                        promoImage.BeginInit(); //Download image
                        promoImage.UriSource = new Uri((string)kvPair.Value, UriKind.RelativeOrAbsolute);
                        promoImage.CacheOption = BitmapCacheOption.OnLoad;
                        promoImage.EndInit();
                        item.PromoImage.Source = promoImage;
                    }
                    if (kvPair.Key == "linkUrl")
                    {
                        item.Tag = (string)kvPair.Value;
                    }
                }
                NewsItemListView.Items.Add(item);
            }
        }

        private void NewsItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NewsItemListView.SelectedIndex != -1)
            {
                NewsItem item = (NewsItem)NewsItemListView.SelectedItem;
                System.Diagnostics.Process.Start((string)item.Tag); //Launch the news article in browser
            }
        }

        #endregion News

        #region Featured Games

        private void SpectatorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SpectatorComboBox.SelectedIndex != -1 && SpectatorComboBox.SelectedValue != null)
            {
                BaseRegion region = BaseRegion.GetRegion((string)SpectatorComboBox.SelectedValue);
                ChangeSpectatorRegion(region);
            }
        }

        private void ChangeSpectatorRegion(BaseRegion region)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                string spectatorJSON = "";
                using (WebClient client = new WebClient())
                {
                    spectatorJSON = client.DownloadString(region.SpectatorLink + "featured");
                }
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> deserializedJSON = serializer.Deserialize<Dictionary<string, object>>(spectatorJSON);
                gameList = deserializedJSON["gameList"] as ArrayList;
            };

            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                ParseSpectatorGames();
            };

            worker.RunWorkerAsync();
        }

        private void ParseSpectatorGames()
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
            var objectGame = gameList[SelectedGame];
            Dictionary<string, object> SpectatorGame = objectGame as Dictionary<string, object>;
            ImageGrid.Children.Clear();
            foreach (KeyValuePair<string, object> pair in SpectatorGame)
            {
                if (pair.Key == "participants")
                {
                    ArrayList players = pair.Value as ArrayList;

                    int i = 0;
                    foreach (var objectPlayer in players)
                    {
                        Dictionary<string, object> playerInfo = objectPlayer as Dictionary<string, object>;
                        int teamId = 100;
                        int championId = 0;
                        int spell1Id = 0;
                        int spell2Id = 0;
                        string PlayerName = "";
                        foreach (KeyValuePair<string, object> playerPair in playerInfo)
                        {
                            if (playerPair.Key == "teamId")
                            {
                                teamId = (int)playerPair.Value;
                            }
                            if (playerPair.Key == "championId")
                            {
                                championId = (int)playerPair.Value;
                            }
                            if (playerPair.Key == "summonerName")
                            {
                                PlayerName = playerPair.Value as string;
                            }
                            if (playerPair.Key == "spell1Id")
                            {
                                spell1Id = (int)playerPair.Value;
                            }
                            if (playerPair.Key == "spell2Id")
                            {
                                spell2Id = (int)playerPair.Value;
                            }
                        }
                        ChampSelectPlayer control = new ChampSelectPlayer();
                        control.ChampionImage.Source = champions.GetChampion(championId).icon;
                        var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName(spell1Id)), UriKind.Absolute);
                        control.SummonerSpell1.Source = new BitmapImage(uriSource);
                        uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName(spell2Id)), UriKind.Absolute);
                        control.SummonerSpell2.Source = new BitmapImage(uriSource);
                        control.PlayerName.Content = PlayerName;

                        Image m = new Image();
                        Canvas.SetZIndex(m, -2); //Put background behind everything
                        m.Stretch = Stretch.None;
                        m.Width = 100;
                        m.Opacity = 0.30;
                        m.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        m.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                        m.Margin = new System.Windows.Thickness(i++ * 100, 0, 0, 0);
                        System.Drawing.Rectangle cropRect = new System.Drawing.Rectangle(new System.Drawing.Point(100, 0), new System.Drawing.Size(100, 560));
                        System.Drawing.Bitmap src = System.Drawing.Image.FromFile(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champions.GetChampion(championId).portraitPath)) as System.Drawing.Bitmap;
                        System.Drawing.Bitmap target = new System.Drawing.Bitmap(cropRect.Width, cropRect.Height);

                        using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(target))
                        {
                            g.DrawImage(src, new System.Drawing.Rectangle(0, 0, target.Width, target.Height),
                                            cropRect,
                                            System.Drawing.GraphicsUnit.Pixel);
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
                if (pair.Key == "gameId")
                {
                    GameId = (int)pair.Value;
                }
                if (pair.Key == "mapId")
                {
                    MapLabel.Content = BaseMap.GetMap((int)pair.Value).DisplayName;
                }
                if (pair.Key == "bannedChampions")
                {
                    //ArrayList players = pair.Value as ArrayList;
                    //Dictionary<string, object> playerInfo = objectPlayer as Dictionary<string, object>;
                    //foreach (KeyValuePair<string, object> playerPair in playerInfo)
                    ArrayList keyArray = pair.Value as ArrayList;
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
                        foreach (KeyValuePair<string, object> keyArrayPair in keyArrayP)
                        {
                            if (keyArrayPair.Key == "championId")
                            {
                                cid = (int)keyArrayPair.Value;
                                //cid = (int)playerPair.Value;
                            }
                            if (keyArrayPair.Key == "teamId")
                            {
                                teamId = (int)keyArrayPair.Value;
                            }
                        }
                        ListViewItem item = new ListViewItem();
                        Image champImage = new Image();
                        champImage.Height = 58;
                        champImage.Width = 58;
                        //temp
                        champImage.Source = champions.GetChampion(cid).icon;
                        

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
                BaseRegion region = BaseRegion.GetRegion((string)SpectatorComboBox.SelectedValue);
                string spectatorJSON = "";
                string url = region.SpectatorLink + "consumer/getGameMetaData/" + region.InternalName + "/" + GameId + "/token";
                using (WebClient client = new WebClient())
                {
                    spectatorJSON = client.DownloadString(url);
                }
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> deserializedJSON = serializer.Deserialize<Dictionary<string, object>>(spectatorJSON);
                MMRLabel.Content = "≈" + deserializedJSON["interestScore"];
            }
            catch { MMRLabel.Content = "N/A"; }
        }

        private void SpectateButton_Click(object sender, RoutedEventArgs e)
        {
            var objectGame = gameList[SelectedGame];
            Dictionary<string, object> SpectatorGame = objectGame as Dictionary<string, object>;
            string key = "";
            int gameId = 0;
            string platformId = "";

            foreach (KeyValuePair<string, object> pair in SpectatorGame)
            {
                if (pair.Key == "gameId")
                {
                    gameId = (int)pair.Value;
                }
                if (pair.Key == "observers")
                {
                    Dictionary<string, object> keyArray = pair.Value as Dictionary<string, object>;
                    foreach (KeyValuePair<string, object> keyArrayPair in keyArray)
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

            BaseRegion region = BaseRegion.GetRegion((string)SpectatorComboBox.SelectedValue);

            Client.LaunchSpectatorGame(region.SpectatorIpAddress, key, gameId, platformId);
        }

        private void NextGameButton_Click(object sender, System.Windows.RoutedEventArgs e)
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

        private void PrevGameButton_Click(object sender, System.Windows.RoutedEventArgs e)
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

        private void HoverLabel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Client.OverlayContainer.Content = new ChooseProfilePicturePage().Content;
            Client.OverlayContainer.Visibility = Visibility.Visible;
        }

        private void HoverLabel_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            HoverLabel.HoverLabel.Content = "Change";
            HoverLabel.Opacity = 100;
        }

        private void HoverLabel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            HoverLabel.Opacity = 0;
        }

        private void InviteTest_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Content = new GameInviteTest().Content;
            Client.OverlayContainer.Visibility = Visibility.Visible;
        }

        private void PVPNet_OnMessageReceived(object sender, object message)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (message is BroadcastNotification)
                {
                    BroadcastNotification notif = message as BroadcastNotification;
                    if (notif.BroadcastMessages != null)
                    {
                        Dictionary<string, object> Message = notif.BroadcastMessages[0] as Dictionary<string, object>;
                        if ((bool)Message["active"] == true)
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
    }
}