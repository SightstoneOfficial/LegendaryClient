using jabber.connection;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.Maps;
using LegendaryClient.Logic.PlayerSpell;
using LegendaryClient.Logic.Region;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect;
using PVPNetConnect.RiotObjects.Leagues.Pojo;
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
                    delegate(SummonerLeaguesDTO result)
                    {
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

                #endregion Get Current Tier
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
                    if (keyArray.Count > 0)
                    {
                        BlueBansLabel.Visibility = Visibility.Visible;
                        PurpleBansLabel.Visibility = Visibility.Visible;
                    }
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
                string url = region.SpectatorLink + "consumer/getGameMetaData/" + region.InternalName + "/" + GameId + "/token";
                using (WebClient client = new WebClient()) // WebClient class inherits IDisposable
                {
                    spectatorJSON = client.DownloadString(url);
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

        #endregion Featured Games

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

        private void fakeend_Click(object sender, RoutedEventArgs e)
        {
            EndOfGameStats fakeStats = new EndOfGameStats();
            fakeStats.TalentPointsGained = 0;
            fakeStats.Ranked = false;
            fakeStats.LeveledUp = false;
            fakeStats.SkinIndex = 4007;
            fakeStats.QueueBonusEarned = 14;
            fakeStats.GameType = "MATCHED_GAME";
            fakeStats.ExperienceEarned = 0;
            fakeStats.ImbalancedTeamsNoPoints = false;
            fakeStats.BasePoints = 32;
            fakeStats.ReportGameId = 22036662;
            fakeStats.Difficulty = null;
            fakeStats.GameLength = 1411;
            fakeStats.BoostXpEarned = 0;
            fakeStats.Invalid = false;
            fakeStats.OtherTeamInfo = null;
            fakeStats.RoomName = "endGame22036662";
            fakeStats.CustomMinutesLeftToday = 120;
            fakeStats.UserId = 200006292;
            fakeStats.CoOpVsAiMinutesLeftToday = 180;
            fakeStats.LoyaltyBoostIpEarned = 0;
            fakeStats.RpEarned = 0;
            fakeStats.CompletionBonusPoints = 0;
            fakeStats.CoOpVsAiMsecsUntilReset = 0;
            fakeStats.BoostIpEarned = 0;
            fakeStats.ExperienceTotal = 23;
            fakeStats.GameId = 125432223;
            fakeStats.TimeUntilNextFirstWinBonus = 0;
            fakeStats.LoyaltyBoostXpEarned = 0;
            fakeStats.RoomPassword = "CCebDkpkYhVjrSRB";
            fakeStats.Elo = 0;
            fakeStats.IpEarned = 0;
            fakeStats.FirstWinBonus = 0;
            fakeStats.SendStatsToTournamentProvider = false;
            fakeStats.EloChange = 0;
            fakeStats.GameMode = "ARAM";
            fakeStats.QueueType = "ARAM_UNRANKED_5x5";
            fakeStats.OdinBonusIp = 0;
            fakeStats.IpTotal = 295513;
            fakeStats.CustomMsecsUntilReset = -1;
            fakeStats.TeamPlayerParticipantStats = new List<PlayerParticipantStatsSummary>();
            fakeStats.OtherTeamPlayerParticipantStats = new List<PlayerParticipantStatsSummary>();
            
            for (int i = 0; i < 10; i++)
            {
                PlayerParticipantStatsSummary fakePlayer = new PlayerParticipantStatsSummary();
                fakePlayer.SkinName = "Lucian";
                fakePlayer.GameId = 22035552;
                fakePlayer.ProfileIconId = 550;
                fakePlayer.Elo = 0;
                fakePlayer.Leaver = false;
                fakePlayer.Leaves = 3;
                fakePlayer.TeamId = 200;
                fakePlayer.EloChange = 0;
                fakePlayer.Level = 30;
                fakePlayer.BotPlayer = false;
                fakePlayer.UserId = 331458;
                fakePlayer.Spell2Id = 4;
                fakePlayer.Spell1Id = 21;
                fakePlayer.Losses = 59;
                fakePlayer.SummonerName = "AO li AO";
                fakePlayer.Wins = 64;
                fakePlayer.Statistics = new List<RawStatDTO>();
                RawStatDTO Item0 = new RawStatDTO();
                Item0.StatTypeName = "ITEM0";
                Item0.Value = 3181;
                fakePlayer.Statistics.Add(Item0);
                RawStatDTO Item1 = new RawStatDTO();
                Item1.StatTypeName = "ITEM1";
                Item1.Value = 3046;
                fakePlayer.Statistics.Add(Item1);
                RawStatDTO Item2 = new RawStatDTO();
                Item2.StatTypeName = "ITEM2";
                Item2.Value = 3006;
                fakePlayer.Statistics.Add(Item2);
                RawStatDTO Item3 = new RawStatDTO();
                Item3.StatTypeName = "ITEM3";
                Item3.Value = 3031;
                fakePlayer.Statistics.Add(Item3);
                RawStatDTO Item4 = new RawStatDTO();
                Item4.StatTypeName = "ITEM4";
                Item4.Value = 1055;
                fakePlayer.Statistics.Add(Item4);
                RawStatDTO Item5 = new RawStatDTO();
                Item5.StatTypeName = "ITEM5";
                Item5.Value = 1036;
                fakePlayer.Statistics.Add(Item5);
                RawStatDTO Item6 = new RawStatDTO();
                Item6.StatTypeName = "ITEM6";
                Item6.Value = 0;
                fakePlayer.Statistics.Add(Item6);
                RawStatDTO GOLDEARNED = new RawStatDTO();
                GOLDEARNED.StatTypeName = "GOLD_EARNED";
                GOLDEARNED.Value = 11736;
                fakePlayer.Statistics.Add(GOLDEARNED);
                RawStatDTO Assists = new RawStatDTO();
                Assists.StatTypeName = "ASSISTS";
                Assists.Value = 23;
                RawStatDTO NUMDEATHS = new RawStatDTO();
                NUMDEATHS.StatTypeName = "NUM_DEATHS";
                NUMDEATHS.Value = 23;
                fakePlayer.Statistics.Add(NUMDEATHS);
                RawStatDTO LEVEL = new RawStatDTO();
                LEVEL.StatTypeName = "LEVEL";
                LEVEL.Value = 17;
                fakePlayer.Statistics.Add(LEVEL);
                RawStatDTO CHAMPIONSKILLED = new RawStatDTO();
                CHAMPIONSKILLED.StatTypeName = "CHAMPIONS_KILLED";
                CHAMPIONSKILLED.Value = 7;
                fakePlayer.Statistics.Add(CHAMPIONSKILLED);
                RawStatDTO MinionsKILLED = new RawStatDTO();
                MinionsKILLED.StatTypeName = "MINIONS_KILLED";
                MinionsKILLED.Value = 60;
                fakePlayer.Statistics.Add(MinionsKILLED);
                RawStatDTO WIN = new RawStatDTO();
                WIN.StatTypeName = "WIN";
                WIN.Value = 1;
                fakePlayer.Statistics.Add(WIN);
                if (i > 5)
                {
                    fakeStats.OtherTeamPlayerParticipantStats.Add(fakePlayer);
                }
                else
                {
                    fakeStats.TeamPlayerParticipantStats.Add(fakePlayer);
                }
            }

            EndOfGamePage test = new EndOfGamePage(fakeStats);
            //Client.OverlayContainer.Visibility = Visibility.Visible;
            //Client.OverlayContainer.Content = test.Content;

            string ObfuscatedName = Client.GetObfuscatedChatroomName(Textbox.Text, ChatPrefixes.Public);
            string JID = Client.GetChatroomJID(ObfuscatedName, "", true);
            Room newRoom = Client.ConfManager.GetRoom(new jabber.JID(JID));
            newRoom.Nickname = Client.LoginPacket.AllSummonerData.Summoner.Name;
            newRoom.Join();
        }
    }
}