using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using System.Collections;
using LegendaryClient.Logic.PlayerSpell;
using LegendaryClient.Logic.Region;
using System.Web.Script.Serialization;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Logic.Maps;
using System.Windows.Media;
using PVPNetConnect.RiotObjects.Platform.Summoner;
using PVPNetConnect.RiotObjects.Platform.Clientfacade.Domain;
using PVPNetConnect.RiotObjects.Platform.Leagues.Client.Dto;
using PVPNetConnect.RiotObjects.Leagues.Pojo;
using PVPNetConnect;
using System.Linq;
using System.Windows.Threading;
using System.Threading;

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
                #region Get Current Tier
                Client.PVPNet.GetAllLeaguesForPlayer(PlayerData.Summoner.SumId, new SummonerLeaguesDTO.Callback(
                    delegate(SummonerLeaguesDTO result) {
                        Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                        {
                            string CurrentLP = "0";
                            string CurrentTier = "Gold V";
                            if (result.SummonerLeagues.Count <= 0)
                            {
                                CurrentLP = "";
                                CurrentTier = "Level 30";
                            }
                            else
                            {
                                foreach (LeagueListDTO leagues in result.SummonerLeagues)
                                {
                                    CurrentTier = leagues.Tier + " " + leagues.RequestorsRank;
                                    List<LeagueItemDTO> players = leagues.Entries.OrderBy(o => o.LeaguePoints).Where(item => item.Rank == leagues.RequestorsRank).ToList();
                                    foreach (LeagueItemDTO player in players)
                                    {
                                        if (player.PlayerOrTeamName == PlayerData.Summoner.Name)
                                        {
                                            TypedObject miniSeries = player.MiniSeries as TypedObject;
                                            string Series = "";
                                            if (miniSeries != null)
                                            {
                                                Series = (string)miniSeries["progress"];
                                            }
                                            CurrentLP = (player.LeaguePoints == 100 ? Series : Convert.ToString(player.LeaguePoints));
                                        }
                                    }
                                }
                            }
                            PlayerProgressLabel.Content = CurrentTier;
                            PlayerCurrentProgressLabel.Content = CurrentLP + "LP";
                            PlayerProgressBar.Value = Convert.ToInt32(CurrentLP);
                        }));
                    })
                );
                #endregion
            }

            Client.InfoLabel.Content = "IP: " + Client.LoginPacket.IpBalance + " ∙ RP: " + Client.LoginPacket.RpBalance;
            //LocationLabel.Content = "Region: " + Client.LoginPacket.CompetitiveRegion;
            int ProfileIconID = Client.LoginPacket.AllSummonerData.Summoner.ProfileIconId;
            var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", ProfileIconID + ".png"), UriKind.RelativeOrAbsolute);
            ProfileImage.Source = new BitmapImage(uriSource);
            Client.MainPageProfileImage = ProfileImage;
        }

        #region News
        private void GetNews(BaseRegion region)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                string newsJSON = "";
                using (WebClient client = new WebClient()) // WebClient class inherits IDisposable
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
        #endregion

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
                using (WebClient client = new WebClient()) // WebClient class inherits IDisposable
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
                        var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champions.GetChampion(championId).iconPath), UriKind.Absolute);
                        control.ChampionImage.Source = new BitmapImage(uriSource);
                        uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName(spell1Id)), UriKind.Absolute);
                        control.SummonerSpell1.Source = new BitmapImage(uriSource);
                        uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName(spell2Id)), UriKind.Absolute);
                        control.SummonerSpell2.Source = new BitmapImage(uriSource);
                        control.PlayerName.Content = PlayerName;

                        Image m = new Image();
                        Canvas.SetZIndex(m, -2); //Put background behind everything
                        m.Stretch = Stretch.None;
                        m.Width = 100;
                        m.Opacity = 0.50;
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

                        m.Source = ToWpfBitmap(target);
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
                    ArrayList keyArray = pair.Value as ArrayList;
                    foreach (Dictionary<string, object> keyArrayP in keyArray)
                    {
                        int cid = 0;
                        int teamId = 100;
                        foreach (KeyValuePair<string, object> keyArrayPair in keyArrayP)
                        {
                            if (keyArrayPair.Key == "championId")
                            {
                                cid = (int)keyArrayPair.Value;
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
                        var uriSource =
                            new Uri(
                                Path.Combine(Client.ExecutingDirectory, "Assets", "champions",
                                    champions.GetChampion(cid).iconPath), UriKind.Absolute);
                        champImage.Source = new BitmapImage(uriSource);
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
                string url = region.SpectatorLink + "consumer/getGameMetaData/" + region.InternalName.ToLower() + "/" + GameId + "/token";
                using (WebClient client = new WebClient()) // WebClient class inherits IDisposable
                {
                    spectatorJSON = client.DownloadString(region.SpectatorLink + "consumer/getGameMetaData/" + region.InternalName + "/" + GameId + "/token");
                }
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> deserializedJSON = serializer.Deserialize<Dictionary<string, object>>(spectatorJSON);
                MMRLabel.Content = "≈" + deserializedJSON["interestScore"];
            }
            catch { MMRLabel.Content = "N/A"; }
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

        #endregion

        public BitmapSource ToWpfBitmap(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);

                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
                // Force the bitmap to load right now so we can dispose the stream.
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }

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

    }
}
