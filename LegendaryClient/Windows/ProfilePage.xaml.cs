using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Security.Cryptography.X509Certificates;
using PVPNetConnect;
using PVPNetConnect.RiotObjects.Leagues.Pojo;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Leagues.Client.Dto;
using PVPNetConnect.RiotObjects.Platform.Statistics;
using PVPNetConnect.RiotObjects.Platform.Summoner;
using PVPNetConnect.RiotObjects.Platform.Summoner.Masterybook;
using PVPNetConnect.RiotObjects.Platform.Catalog.Champion;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using System.Threading.Tasks;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        double AccountId = 0;
        List<PlayerGameStats> GameStats = new List<PlayerGameStats>();
        List<LeagueItemDTO> players = new List<LeagueItemDTO>();
        List<PlayerStatSummary> PlayerStats = new List<PlayerStatSummary>();
        List<MasteryBookPageDTO> MasteryPages = new List<MasteryBookPageDTO>();
        string SelectedTier = "N/A";
        SummonerLeaguesDTO Leagues;

        public ProfilePage()
        {
            InitializeComponent();
            GetSummonerProfile(Client.LoginPacket.AllSummonerData.Summoner.Name);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            GetSummonerProfile(SearchTextBox.Text);
        }

        public async void GetSummonerProfile(string s)
        {
            PublicSummoner Summoner = await Client.PVPNet.GetSummonerByName(s);
            if (String.IsNullOrWhiteSpace(Summoner.Name))
            {
                SummonerNameLabel.Content = "No Summoner Found";
                SummonerLevelLabel.Content = "Level -1";
                return;
            }
            SummonerNameLabel.Content = Summoner.Name;
            SummonerLevelLabel.Content = "Level " + Summoner.SummonerLevel;

            BrushConverter bc = new BrushConverter();
            Brush brush = (Brush)bc.ConvertFrom("#FFFFFF");
            LeaguesButton.IsEnabled = true;
            LeaguesButton.Foreground = brush;

            AccountId = Summoner.AcctId;
            int ProfileIconID = Summoner.ProfileIconId;
            //TODO: Convert ProfileIconID to the decompiled images
            var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "profileImages", 137 + ".jpg"), UriKind.Absolute);
            ProfileImage.Source = new BitmapImage(uriSource);

            //Clear pages
            OverviewComboBox.Items.Clear();
            OverviewListView.Items.Clear();
            GamesListView.Items.Clear();
            GameStatsListView.Items.Clear();
            BlueListView.Items.Clear();
            PurpleListView.Items.Clear();
            LeagueListView.Items.Clear();
            ChampionListView.Items.Clear();
            SkinListView.Items.Clear();
            ProfileTabControl.SelectedItem = OverviewItem;

            //If not own summoner
            if (Summoner.Name.ToLower() != Client.LoginPacket.AllSummonerData.Summoner.Name.ToLower())
            {
                ChampionButton.IsEnabled = false;
                SkinButton.IsEnabled = false;
                brush = (Brush)bc.ConvertFrom("#FFAAAAAA");
            }
            else
            {
                ChampionButton.IsEnabled = true;
                SkinButton.IsEnabled = true;
                //Put on own thread
                GotChamps(await Client.PVPNet.GetAvailableChampions());
            }
            ChampionButton.Foreground = brush;
            SkinButton.Foreground = brush;

            //Get ingame stats
            IngameButton.IsEnabled = false;
            IngameButton.Foreground = (Brush)bc.ConvertFrom("#FFAAAAAA");

            PlatformGameLifecycleDTO n = await Client.PVPNet.RetrieveInProgressSpectatorGameInfo(s);
            if (n.GameName != null)
            {
                brush = (Brush)bc.ConvertFrom("#FFFFFF");
                IngameButton.IsEnabled = true;
                IngameButton.Foreground = brush;
                ProfileTabControl.SelectedItem = IngameItem;
                IsIngame(n);
            }

            //Get other stats
            Client.PVPNet.GetRecentGames(AccountId, new RecentGames.Callback(GotRecentGames));
            Client.PVPNet.GetAllLeaguesForPlayer(Summoner.SummonerId, new SummonerLeaguesDTO.Callback(GotLeaguesForPlayer));
            Client.PVPNet.RetrievePlayerStatsByAccountId(AccountId, "CURRENT", new PlayerLifetimeStats.Callback(GotPlayerStats));
            //Global.PVPNet.GetChallengerLeague("RANKED_SOLO_5x5", new LeagueListDTO.Callback(GotChallengerLeague));
        }

        private void IngameButton_Click(object sender, RoutedEventArgs e)
        {
            ProfileTabControl.SelectedItem = IngameItem;
        }

        private async void IsIngame(PlatformGameLifecycleDTO IngameDTO)
        {
            GameDTO gameDTO = IngameDTO.Game;
            BlueMMR.Content = "~" + Math.Round(await CalculateMMRPerPlayer(gameDTO.TeamOne), 2);
            RedMMR.Content = "~" + Math.Round(await CalculateMMRPerPlayer(gameDTO.TeamTwo), 2);
            
       }

        public async Task<double> CalculateMMRPerPlayer(List<Participant> Participants)
        {
            List<double> PlayerMMR = new List<double>();
            foreach (PlayerParticipant Player in Participants)
            {
                SummonerLeaguesDTO result = await Client.PVPNet.GetAllLeaguesForPlayer(Player.SummonerId);
                int LeaguePoints = 0;
                foreach (LeagueListDTO leagues in result.SummonerLeagues)
                {
                    foreach (LeagueItemDTO leagueEntry in leagues.Entries)
                    {
                        if (leagueEntry.PlayerOrTeamName == Player.SummonerName)
                        {
                            TypedObject miniSeries = leagueEntry.MiniSeries as TypedObject;
                            int LeaguePointSeries = 0;
                            if (miniSeries != null)
                            {
                                foreach (char c in (string)miniSeries["progress"])
                                {
                                    if (c == 'L')
                                    {
                                        LeaguePointSeries -= 50;
                                    }
                                    if (c == 'W')
                                    {
                                        LeaguePointSeries += 50;
                                    }
                                }
                            }
                            LeaguePoints = (leagueEntry.LeaguePoints == 100 ? LeaguePointSeries : leagueEntry.LeaguePoints);
                        }
                    }
                    PlayerMMR.Add(MMR.GetInaccurateMMR(leagues.Tier + " " + leagues.RequestorsRank, LeaguePoints));
                }
            }
            return MMR.GetCalulatedMMR(PlayerMMR);
        }

        #region Overview
        private void OverviewButton_Click(object sender, RoutedEventArgs e)
        {
            ProfileTabControl.SelectedItem = OverviewItem;
        }

        public void GotPlayerStats(PlayerLifetimeStats result)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                OverviewComboBox.Items.Clear();
                PlayerStats = result.PlayerStatSummaries.PlayerStatSummarySet.ToList();
                foreach (PlayerStatSummary GameMode in result.PlayerStatSummaries.PlayerStatSummarySet)
                {
                    if (GameMode.AggregatedStats.Stats.Count > 0)
                    {
                        OverviewComboBox.Items.Add(GameMode.PlayerStatSummaryTypeString.Replace("Odin", "Dominion") + " (" + GameMode.Wins + " wins)");
                    }
                    else
                    {
                        PlayerStats.Remove(GameMode);
                    }
                }
                if (OverviewComboBox.Items.Count > 0)
                {
                    OverviewComboBox.SelectedIndex = 0;
                }
            }));
        }

        private void OverviewComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OverviewComboBox.SelectedIndex != -1)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    OverviewListView.Items.Clear();
                    PlayerStatSummary GameMode = PlayerStats[OverviewComboBox.SelectedIndex];
                    foreach (SummaryAggStat stat in GameMode.AggregatedStats.Stats)
                    {
                        KeyValueItem item = new KeyValueItem
                        {
                            Key = TitleCaseString(stat.StatType.Replace('_', ' ')),
                            Value = stat.Value.ToString()
                        };
                        OverviewListView.Items.Add(item);
                    }

                    //Resize columns
                    if (double.IsNaN(OverviewKeyHeader.Width))
                    {
                        OverviewKeyHeader.Width = OverviewKeyHeader.ActualWidth;
                    }
                    if (double.IsNaN(OverviewValueHeader.Width))
                    {
                        OverviewValueHeader.Width = OverviewValueHeader.ActualWidth;
                    }
                    OverviewKeyHeader.Width = double.NaN;
                    OverviewValueHeader.Width = double.NaN;
                }));
            }
        }
        #endregion

        #region RecentGames
        public void GotRecentGames(RecentGames result)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                GameStats = result.GameStatistics;
                GameStats.Sort((s1, s2) => s2.CreateDate.CompareTo(s1.CreateDate));
                foreach (PlayerGameStats Game in GameStats)
                {
                    RecentGameOverview item = new RecentGameOverview();
                    var uriSource =
                        new Uri(
                            Path.Combine(Client.ExecutingDirectory, "Assets", "champions",
                                champions.GetChampion((int) Math.Round(Game.ChampionId)).iconPath),
                            UriKind.Absolute);
                    item.ChampionImage.Source = new BitmapImage(uriSource);
                    bool Lose = false;
                    double Deaths = 0;
                    double Kills = 0;
                    double Assists = 0;
                    double CS = 0;
                    foreach (RawStat Stat in Game.Statistics)
                    {
                        if (Stat.StatType == "LOSE")
                            if (Stat.Value == 1)
                                Lose = true;

                        if (Stat.StatType == "NUM_DEATHS")
                            Deaths = Stat.Value;

                        if (Stat.StatType == "ASSISTS")
                            Assists = Stat.Value;

                        if (Stat.StatType == "CHAMPIONS_KILLED")
                            Kills = Stat.Value;

                        if (Stat.StatType == "MINIONS_KILLED")
                            CS += Stat.Value;

                        if (Stat.StatType == "NEUTRAL_MINIONS_KILLED")
                            CS += Stat.Value;
                    }
                    item.ChampionNameLabel.Content = champions.GetChampion(Convert.ToInt32(Game.ChampionId)).displayName;
                    item.ScoreLabel.Content = Kills + "/" + Deaths + "/" + Assists + "(" + TitleCaseString(Game.GameType.Replace("_GAME", "")
                        .Replace("MATCHED", "NORMAL")
                        .Replace('_', ' ')) + ")";
                    item.CreepScoreLabel.Content = CS + " minions";
                    item.DateLabel.Content = Game.CreateDate;
                    item.IPEarnedLabel.Content = "+" + Game.IpEarned + " IP";
                    item.PingLabel.Content = Game.UserServerPing + "ms";
                    if (Lose)
                    {
                        BrushConverter bc = new BrushConverter();
                        Brush brush = (Brush)bc.ConvertFrom("#FF9E6060");
                        item.GridView.Background = brush;
                    }
                    else
                    {
                        BrushConverter bc = new BrushConverter();
                        Brush brush = (Brush)bc.ConvertFrom("#FF609E74");
                        item.GridView.Background = brush;
                    }
                    item.GridView.Width = 250;
                    GamesListView.Items.Add(item);
                }
            }));
        }

        private void RecentGamesButton_Click(object sender, RoutedEventArgs e)
        {
            ProfileTabControl.SelectedItem = RecentGamesItem;
        }

        private async void GamesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GamesListView.SelectedIndex != -1)
            {
                PlayerGameStats stats = GameStats[GamesListView.SelectedIndex];
                Dictionary<double, string> IDtoName = new Dictionary<double, string>();
                List<double> SummonerIDs = new List<double>();
                foreach (FellowPlayerInfo info in stats.FellowPlayers)
                {
                    SummonerIDs.Add(info.SummonerId);
                }
                string[] SummonerNames = await Client.PVPNet.GetSummonerNames(SummonerIDs.ToArray());
                for (int i = 0; i < SummonerNames.Length; i++)
                {
                    IDtoName.Add(SummonerIDs[i], SummonerNames[i]);
                }
                GameStatsListView.Items.Clear();
                PurpleListView.Items.Clear();
                BlueListView.Items.Clear();

                //Add self to game players
                var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champions.GetChampion((int)Math.Round(stats.ChampionId)).iconPath), UriKind.Absolute);
                Image img = new Image();
                img.Width = 58;
                img.Height = 58;
                img.Source = new BitmapImage(uriSource);
                BlueListView.Items.Add(img);

                foreach (FellowPlayerInfo info in stats.FellowPlayers)
                {
                    if (info.TeamId == stats.TeamId)
                    {
                        uriSource = new Uri(
                                Path.Combine(Client.ExecutingDirectory, "Assets", "champions",
                                    champions.GetChampion((int)Math.Round(info.ChampionId)).iconPath),
                                UriKind.Absolute);
                        Image i = new Image();
                        i.Width = 58;
                        i.Height = 58;
                        i.Source = new BitmapImage(uriSource);
                        BlueListView.Items.Add(i);
                    }
                    else
                    {
                        uriSource = new Uri(
                                Path.Combine(Client.ExecutingDirectory, "Assets", "champions",
                                    champions.GetChampion((int)Math.Round(info.ChampionId)).iconPath),
                                UriKind.Absolute);
                        Image i = new Image();
                        i.Width = 58;
                        i.Height = 58;
                        i.Source = new BitmapImage(uriSource);
                        PurpleListView.Items.Add(i);
                    }
                }
                foreach (RawStat Stat in stats.Statistics)
                {
                    if (!Stat.StatType.StartsWith("ITEM"))
                    {
                        KeyValueItem item = new KeyValueItem
                        {
                            Key = TitleCaseString(Stat.StatType.Replace('_', ' ')),
                            Value = Stat.Value.ToString()
                        };
                        GameStatsListView.Items.Add(item);
                    }
                }

                //Resize Columns
                if (double.IsNaN(GameKeyHeader.Width))
                    GameKeyHeader.Width = GameKeyHeader.ActualWidth;
                if (double.IsNaN(GameValueHeader.Width))
                    GameValueHeader.Width = GameValueHeader.ActualWidth;
                GameKeyHeader.Width = double.NaN;
                GameValueHeader.Width = double.NaN;
            }
        }
        #endregion

        #region Leagues
        public static String TitleCaseString(String s)
        {
            if (s == null) return s;

            String[] words = s.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length == 0) continue;

                Char firstChar = Char.ToUpper(words[i][0]);
                String rest = "";
                if (words[i].Length > 1)
                {
                    rest = words[i].Substring(1).ToLower();
                }
                words[i] = firstChar + rest;
            }
            return String.Join(" ", words);
        }

        private void LeaguesButton_Click(object sender, RoutedEventArgs e)
        {
            ProfileTabControl.SelectedItem = LeaguesItem;
        }

        public void GotLeaguesForPlayer(SummonerLeaguesDTO result)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (result.SummonerLeagues.Count <= 0)
                {
                    LeaguesButton.IsEnabled = false;
                    BrushConverter bc = new BrushConverter();
                    Brush brush = (Brush)bc.ConvertFrom("#FFAAAAAA");
                    LeaguesButton.Foreground = brush;
                    return;
                }
                Leagues = result;
                foreach (LeagueListDTO leagues in result.SummonerLeagues)
                {
                    if (SelectedTier == "N/A")
                    {
                        SelectedTier = leagues.RequestorsRank;
                    }
                    if (SelectedTier == "V")
                    {
                        UpTierButton.IsEnabled = true;
                        DownTierButton.IsEnabled = false;
                    }
                    else if (SelectedTier == "I")
                    {
                        UpTierButton.IsEnabled = false;
                        DownTierButton.IsEnabled = true;
                    }
                    else
                    {
                        UpTierButton.IsEnabled = true;
                        DownTierButton.IsEnabled = true;
                    }
                    LeagueLabel.Content = leagues.Name + " (" + leagues.Tier + " " + SelectedTier + ")";
                    LeagueListView.Items.Clear();
                    int x = 1;
                    players = leagues.Entries.OrderBy(o => o.LeaguePoints).Where(item => item.Rank == SelectedTier).ToList();
                    players.Reverse();
                    foreach (LeagueItemDTO player in players)
                    {
                        TypedObject miniSeries = player.MiniSeries as TypedObject;
                        string Series = "";
                        if (miniSeries != null)
                        {
                            Series = (string)miniSeries["progress"];
                        }
                        LeagueItem item = new LeagueItem
                        {
                            Rank = x,
                            LeaguePoints = (player.LeaguePoints == 100 ? Series : Convert.ToString(player.LeaguePoints)),
                            Name = player.PlayerOrTeamName,
                            Wins = player.Wins,
                            HotStreak = (player.HotStreak ? "Y" : ""),
                            Recruit = (player.FreshBlood ? "Y" : ""),
                            Veteran = (player.Veteran ? "Y" : "")
                        };
                        LeagueListView.Items.Add(item);
                        x += 1;
                    }

                    if (double.IsNaN(RankHeader.Width))
                        RankHeader.Width = RankHeader.ActualWidth;
                    if (double.IsNaN(NameHeader.Width))
                        NameHeader.Width = NameHeader.ActualWidth;
                    if (double.IsNaN(LeaguePointsHeader.Width))
                        LeaguePointsHeader.Width = LeaguePointsHeader.ActualWidth;
                    if (double.IsNaN(HotStreakHeader.Width))
                        HotStreakHeader.Width = HotStreakHeader.ActualWidth;
                    if (double.IsNaN(RecruitHeader.Width))
                        RecruitHeader.Width = RecruitHeader.ActualWidth;
                    if (double.IsNaN(VeteranHeader.Width))
                        VeteranHeader.Width = VeteranHeader.ActualWidth;
                    VeteranHeader.Width = double.NaN;
                    RecruitHeader.Width = double.NaN;
                    HotStreakHeader.Width = double.NaN;
                    LeaguePointsHeader.Width = double.NaN;
                    RankHeader.Width = double.NaN;
                    NameHeader.Width = double.NaN;
                }
            }));
        }

        private void UpTierButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTier == "V")
            {
                SelectedTier = "IV";
            }
            else if (SelectedTier == "IV")
            {
                SelectedTier = "III";
            }
            else if (SelectedTier == "III")
            {
                SelectedTier = "II";
            }
            else if (SelectedTier == "II")
            {
                SelectedTier = "I";
            }
            GotLeaguesForPlayer(Leagues);
        }

        private void DownTierButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTier == "I")
            {
                SelectedTier = "II";
            }
            else if (SelectedTier == "II")
            {
                SelectedTier = "III";
            }
            else if (SelectedTier == "III")
            {
                SelectedTier = "IV";
            }
            else if (SelectedTier == "IV")
            {
                SelectedTier = "V";
            }
            GotLeaguesForPlayer(Leagues);
        }

        private void LeagueListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LeagueListView.SelectedIndex != -1)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async () =>
                {
                    LeagueItem playerItem = LeagueListView.SelectedItem as LeagueItem;
                    PublicSummoner t = await Client.PVPNet.GetSummonerByName(playerItem.Name);

                    TopChampListView.Items.Clear();

                    AggregatedStats Stats = await Client.PVPNet.GetAggregatedStats(t.AcctId, "CLASSIC", "CURRENT");

                    /*
                     * Stats:
                     * TOTAL_DEATHS_PER_SESSION
                     * TOTAL_PENTA_KILLS
                     * TOTAL_QUADRA_KILLS
                     * TOTAL_TRIPLE_KILLS
                     * TOTAL_DOUBLE_KILLS
                     * TOTAL_MINION_KILLS
                     * MOST_CHAMPION_KILLS_PER_SESSION
                     * TOTAL_DAMAGE_TAKEN
                     * TOTAL_DAMAGE_DEALT
                     * TOTAL_CHAMPION_KILLS
                     * TOTAL_SESSIONS_WON
                     * TOTAL_SESSIONS_LOST
                     * TOTAL_SESSIONS_PLAYED
                     * TOTAL_GOLD_EARNED
                     * MOST_SPELLS_CAST
                     * TOTAL_TURRETS_KILLED
                     * TOTAL_MAGIC_DAMAGE_DEALT
                     * TOTAL_PHYSICAL_DAMAGE_DEALT
                     * TOTAL_ASSISTS
                     * TOTAL_TIME_SPENT_DEAD
                     * TOTAL_FIRST_BLOOD
                     * TOTAL_UNREAL_KILLS
                     * MAX_NUM_DEATHS
                     * MAX_CHAMPIONS_KILLED
                     * BOT_GAMES_PLAYED
                     * RANKED_SOLO_GAMES_PLAYED
                     * TOTAL_HEAL
                     * MAX_TIME_SPENT_LIVING
                     * MAX_LARGEST_CRITICAL_STRIKE
                     * TOTAL_NEUTRAL_MINIONS_KILLED
                     * TOTAL_LEAVES
                     * RANKED_PREMADE_GAMES_PLAYED
                     * MAX_TIME_PLAYED
                     * KILLING_SPREE
                     * MAX_LARGEST_KILLING_SPREE
                     * NORMAL_GAMES_PLAYED
                     * */

                    List<KeyValueItem> TopChamps = new List<KeyValueItem>();
                    foreach (AggregatedStat stat in Stats.LifetimeStatistics)
                    {
                        if (stat.StatType == "TOTAL_SESSIONS_PLAYED")
                        {
                            TopChamps.Add(new KeyValueItem
                            {
                                Key = stat.ChampionId,
                                Value = stat.Value
                            });
                        }
                    }

                    TopChamps = TopChamps.OrderByDescending(x => x.Value).ToList();

                    foreach (KeyValueItem stat in TopChamps.Take(8))
                    {
                        if ((double)stat.Key != 0)
                        {
                            KeyValueItem item = new KeyValueItem
                            {
                                Key = champions.GetChampion(Convert.ToInt32(stat.Key)).displayName,
                                Value = stat.Value + " games"
                            };
                            TopChampListView.Items.Add(item);
                        }
                    }

                    //Resize columns
                    if (double.IsNaN(TopChampKeyHeader.Width))
                    {
                        TopChampKeyHeader.Width = TopChampKeyHeader.ActualWidth;
                    }
                    if (double.IsNaN(TopChampValueHeader.Width))
                    {
                        TopChampValueHeader.Width = TopChampValueHeader.ActualWidth;
                    }
                    TopChampKeyHeader.Width = double.NaN;
                    TopChampValueHeader.Width = double.NaN;
                }));
            }
        }
        #endregion

        private void ChampionButton_Click(object sender, RoutedEventArgs e)
        {
            ProfileTabControl.SelectedItem = ChampionsItem;
        }

        private void GotChamps(ChampionDTO[] dto)
        {
            List<ChampionDTO> champList = new List<ChampionDTO>(dto);

            foreach (ChampionDTO champ in champList)
            {
                if (champ.Owned || champ.FreeToPlay)
                {
                    ListViewItem item = new ListViewItem();
                    Image champImage = new Image();
                    champImage.Height = 58;
                    champImage.Width = 58;
                    var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champions.GetChampion(champ.ChampionId).iconPath), UriKind.Absolute);
                    champImage.Source = new BitmapImage(uriSource);
                    item.Content = champImage;
                    ChampionListView.Items.Add(item);
                    foreach (ChampionSkinDTO champSkinDTO in champ.ChampionSkins)
                    {
                        if (champSkinDTO.Owned)
                        {
                            ListViewItem iteme = new ListViewItem();
                            Image champSkinImage = new Image();
                            champSkinImage.Height = 560;
                            champSkinImage.Width = 308;
                            uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", championSkins.GetSkin(champSkinDTO.SkinId).portraitPath), UriKind.Absolute);
                            champSkinImage.Source = new BitmapImage(uriSource);
                            iteme.Content = champSkinImage;
                            SkinListView.Items.Add(iteme);
                        }
                    }
                }
            }
        }

        private void SkinButton_Click(object sender, RoutedEventArgs e)
        {
            ProfileTabControl.SelectedItem = SkinsItem;
        }
    }

    public class KeyValueItem
    {
        public object Key { get; set; }
        public object Value { get; set; }
    }

    public class LeagueItem
    {
        public string LeaguePoints { get; set; }
        public int Wins { get; set; }
        public int Rank { get; set; }
        public string HotStreak { get; set; }
        public string Recruit { get; set; }
        public string Veteran { get; set; }
        public string Name { get; set; }
    }
}
