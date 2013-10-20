using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using System.Collections;
using LegendaryClient.Logic.Region;
using System.Web.Script.Serialization;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Logic.Maps;
using System.Windows.Media;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        internal int SelectedGame = 0;
        internal ArrayList gameList;

        public MainPage()
        {
            InitializeComponent();
            SummonerNameLabel.Content = Client.LoginPacket.AllSummonerData.Summoner.Name;
            SummonerLevelLabel.Content = "Level " + Client.LoginPacket.AllSummonerData.SummonerLevel.Level;
            BalanceLabel.Content = "IP: " + Client.LoginPacket.IpBalance + " | RP: " + Client.LoginPacket.RpBalance;
            LocationLabel.Content = "Region: " + Client.LoginPacket.CompetitiveRegion;
            int ProfileIconID = Client.LoginPacket.AllSummonerData.Summoner.ProfileIconId;
            //TODO: Convert ProfileIconID to the decompiled images
            var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "profileImages", 137 + ".jpg"), UriKind.Absolute);
            ProfileImage.Source = new BitmapImage(uriSource);

            SpectatorComboBox.SelectedValue = Client.LoginPacket.CompetitiveRegion;
            BaseRegion region = BaseRegion.GetRegion(Client.LoginPacket.CompetitiveRegion);
            ChangeSpectatorRegion(region);
        }

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
                    spectatorJSON = client.DownloadString(region.SpectatorLink);
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
            var objectGame = gameList[SelectedGame];
            Dictionary<string, object> SpectatorGame = objectGame as Dictionary<string, object>;
            foreach (KeyValuePair<string, object> pair in SpectatorGame)
            {
                if (pair.Key == "participants")
                {
                    ArrayList players = pair.Value as ArrayList;
                    foreach (var objectPlayer in players)
                    {
                        Dictionary<string, object> playerInfo = objectPlayer as Dictionary<string, object>;
                        int teamId = 100;
                        int championId = 0;
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
                        }
                        ChampSelectPlayer control = new ChampSelectPlayer();
                        Image champImage = new Image();
                        champImage.Height = 58;
                        champImage.Width = 58;
                        var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champions.GetChampion(championId).iconPath), UriKind.Absolute);
                        champImage.Source = new BitmapImage(uriSource);
                        control.ChampionImage = champImage;
                        control.ChampPlayer.Content = PlayerName;
                        control.Width = 125;
                        control.Height = 80;
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
                    GameIdLabel.Content = "Game ID " + (int)pair.Value;
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

    }
}
