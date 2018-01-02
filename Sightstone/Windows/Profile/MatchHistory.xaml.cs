﻿using Sightstone.Controls;
using Sightstone.Logic;
using Sightstone.Logic.SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Sightstone.Logic.Riot;
using Sightstone.Logic.Riot.Platform;
using System.Threading.Tasks;
using Sightstone.Logic.MultiUser;

namespace Sightstone.Windows.Profile
{
    /// <summary>
    ///     Interaction logic for MatchHistory.xaml
    /// </summary>
    public partial class MatchHistory
    {
        //private string _matchLinkOnline;
        private LargeRuneMastery playerItem;
        //private static readonly ILog Log = LogManager.GetLogger(typeof (MatchHistory));
        private readonly List<MatchStats> gameStats = new List<MatchStats>();
        static UserClient UserClient = (UserList.Users[Client.CurrentServer])[Client.CurrentUser];

        public MatchHistory()
        {
            InitializeComponent();
            gameRecordedEllipse.Fill = (Brush)new BrushConverter().ConvertFrom("#CC444444");
            ReplayLabel.Content = "game not recorded";
        }

        public async Task<object> Update(double accountId)
        {
            GotRecentGames(await UserClient.calls.GetRecentGames(accountId));
            return null;
        }

        public void GotRecentGames(RecentGames result)
        {
            if (result.GameStatistics == null)
            {
                return;
            }

            gameStats.Clear();
            result.GameStatistics.Sort((s1, s2) => s2.CreateDate.CompareTo(s1.CreateDate));
            foreach (var game in result.GameStatistics)
            {
                game.GameType =
                    Client.TitleCaseString(game.GameType.Replace("_GAME", string.Empty).Replace("MATCHED", "NORMAL"));
                var match = new MatchStats();

                foreach (var stat in game.Statistics)
                {
                    var type = typeof (MatchStats);
                    var fieldName = Client.TitleCaseString(stat.StatType.Replace('_', ' ')).Replace(" ", string.Empty);
                    var f = type.GetField(fieldName);
                    f.SetValue(match, stat.Value);
                }

                match.Game = game;

                gameStats.Add(match);
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                GamesListView.Items.Clear();
                BlueListView.Items.Clear();
                ItemsListView.Items.Clear();
                PurpleListView.Items.Clear();
                GameStatsListView.Items.Clear();
                foreach (var stats in gameStats)
                {
                    var item = new RecentGameOverview();
                    var gameChamp = champions.GetChampion((int) Math.Round(stats.Game.ChampionId));
                    item.ChampionImage.Source = gameChamp.icon;
                    item.ChampionNameLabel.Content = gameChamp.displayName;
                    item.ScoreLabel.Content =
                        string.Format("{0}/{1}/{2} ",
                            stats.ChampionsKilled,
                            stats.NumDeaths,
                            stats.Assists);

                    switch (stats.Game.QueueType)
                    {
                        case "NORMAL":
                            item.ScoreLabel.Content += "(Normal)";
                            break;
                        case "NORMAL_3x3":
                            item.ScoreLabel.Content += "(Normal 3v3)";
                            break;
                        case "ODIN_UNRANKED":
                            item.ScoreLabel.Content += "(Dominion)";
                            break;
                        case "ARAM_UNRANKED_5x5":
                            item.ScoreLabel.Content += "(ARAM)";
                            break;
                        case "NONE":
                            item.ScoreLabel.Content += "(Custom)";
                            break;
                        case "RANKED_SOLO_5x5":
                            item.ScoreLabel.Content += "(Ranked 5v5)";
                            break;
                        case "RANKED_TEAM_5x5":
                            item.ScoreLabel.Content += "(Ranked Team 5v5)";
                            break;
                        case "RANKED_TEAM_3x3":
                            item.ScoreLabel.Content += "(Ranked Team 3v3)";
                            break;
                        case "CAP_5x5":
                            item.ScoreLabel.Content += "(Team Builder)";
                            break;
                        case "BOT":
                            item.ScoreLabel.Content += "(Bots)";
                            break;
                        case "KING_PORO":
                            item.ScoreLabel.Content += "(King Poro)";
                            break;
                        case "COUNTER_PICK":
                            item.ScoreLabel.Content += "(Nemesis Draft)";
                            break;
                        case "URF":
                            item.ScoreLabel.Content += "(URF)";
                            break;
                        default:
                            Client.Log(stats.Game.QueueType);
                            item.ScoreLabel.Content += "Please upload this log to github.";
                            break;
                    }

                    item.CreepScoreLabel.Content = stats.MinionsKilled + " minions";
                    item.DateLabel.Content = stats.Game.CreateDate;
                    item.IPEarnedLabel.Content = "+" + stats.Game.IpEarned + " IP";
                    item.PingLabel.Content = stats.Game.UserServerPing + "ms";

                    var bc = new BrushConverter();
                    var brush = (Brush) bc.ConvertFrom("#FF609E74");

                    if (Math.Abs(stats.Lose - 1) < .00001)
                    {
                        brush = (Brush) bc.ConvertFrom("#FF9E6060");
                    }
                    else if (Math.Abs(stats.Game.IpEarned) < .00001)
                    {
                        brush = (Brush) bc.ConvertFrom("#FFE27100");
                    }

                    item.GridView.Background = brush;
                    item.GridView.Width = 280;
                    GamesListView.Items.Add(item);
                }
                if (GamesListView.Items.Count > 0)
                {
                    GamesListView.SelectedIndex = 0;
                }
            }));
        }

        private void GamesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GamesListView.SelectedIndex != -1)
            {
                var stats = gameStats[GamesListView.SelectedIndex];
                GameStatsListView.Items.Clear();
                PurpleListView.Items.Clear();
                BlueListView.Items.Clear();
                ItemsListView.Items.Clear();
                /*_matchLinkOnline = "http://matchhistory.na.leagueoflegends.com/en/#match-details/" +
                                  Client.Region.InternalName + "/" + (int) Math.Round(stats.Game.GameId) + "/" +
                                  stats.Game.UserId;*/

                //Add self to game players
                var img = new Image
                {
                    Width = 58,
                    Height = 58,
                    Source = champions.GetChampion((int) Math.Round(stats.Game.ChampionId)).icon
                };
                BlueListView.Items.Add(img);
                foreach (var info in stats.Game.FellowPlayers)
                {
                    img = new Image
                    {
                        Width = 58,
                        Height = 58,
                        Source = champions.GetChampion((int) Math.Round(info.ChampionId)).icon
                    };
                    if (Math.Abs(info.TeamId - stats.Game.TeamId) < .00001)
                    {
                        BlueListView.Items.Add(img);
                    }
                    else
                    {
                        PurpleListView.Items.Add(img);
                    }

                    BlueListView.Visibility = BlueListView.Items.Count > 0 ? Visibility.Visible : Visibility.Hidden;
                    PurpleListView.Visibility = PurpleListView.Items.Count > 0 ? Visibility.Visible : Visibility.Hidden;
                }

                var classType = typeof (MatchStats);

                foreach (var field in classType.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (field.GetValue(stats) is double)
                    {
                        if (Math.Abs((double) field.GetValue(stats)) < .00001)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }

                    var item = new ProfilePage.KeyValueItem
                    {
                        Key =
                            Client.TitleCaseString(
                                string.Concat(field.Name.Select(fe => Char.IsUpper(fe) ? " " + fe : fe.ToString()))
                                    .TrimStart(' ')),
                        Value = field.GetValue(stats)
                    };

                    if (((string) item.Key).StartsWith("Item"))
                    {
                        img = new Image
                        {
                            Width = 58,
                            Height = 58,
                            Source = Client.GetItemIcon(item.Value.ToInt()),
                            Tag = item
                        };
                        img.MouseMove += img_MouseMove;
                        img.MouseLeave += img_MouseLeave;
                        ItemsListView.Items.Add(img);
                    }
                    else
                    {
                        GameStatsListView.Items.Add(item);
                    }
                }
            }

            if (double.IsNaN(GameKeyHeader.Width))
            {
                GameKeyHeader.Width = GameKeyHeader.ActualWidth;
            }

            if (double.IsNaN(GameValueHeader.Width))
            {
                GameValueHeader.Width = GameValueHeader.ActualWidth;
            }

            GameKeyHeader.Width = double.NaN;
            GameValueHeader.Width = double.NaN;
        }

        private void img_MouseLeave(object sender, MouseEventArgs e)
        {
            if (playerItem == null)
            {
                return;
            }

            Client.MainGrid.Children.Remove(playerItem);
            playerItem = null;
        }

        private void img_MouseMove(object sender, MouseEventArgs e)
        {
            //TODO: add item info
            //return;

            var item = (Image) sender;
            var playerItem = (ProfilePage.KeyValueItem) item.Tag;
            if (this.playerItem == null)
            {
                var Item = items.GetItem(Convert.ToInt32(playerItem.Value));
                this.playerItem = new LargeRuneMastery();
                Client.MainGrid.Children.Add(this.playerItem);


                this.playerItem.PlayerName.Content = Item.name;

                this.playerItem.PlayerName.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                this.playerItem.Width = this.playerItem.PlayerName.DesiredSize.Width > 250
                    ? this.playerItem.PlayerName.DesiredSize.Width
                    : 250;

                this.playerItem.PlayerWins.Content = Item.price + " gold (" + Item.sellprice + " sell)";
                this.playerItem.PlayerLeague.Content = "Item ID " + Item.id;
                this.playerItem.LevelLabel.Content = string.Empty;
                this.playerItem.UsingLegendary.Visibility = Visibility.Hidden;

                var parsedDescription = Item.description;
                parsedDescription = parsedDescription.Replace("<br>", Environment.NewLine);
                parsedDescription = Regex.Replace(parsedDescription, "<.*?>", string.Empty);
                this.playerItem.PlayerStatus.Content = parsedDescription;

                this.playerItem.ProfileImage.Source = Client.GetItemIcon(Item.id);

                this.playerItem.HorizontalAlignment = HorizontalAlignment.Left;
                this.playerItem.VerticalAlignment = VerticalAlignment.Top;
            }

            var mouseLocation = e.GetPosition(Client.MainGrid);

            var yMargin = mouseLocation.Y;

            var xMargin = mouseLocation.X;
            if (xMargin + this.playerItem.Width + 10 > Client.MainGrid.ActualWidth)
            {
                xMargin = Client.MainGrid.ActualWidth - this.playerItem.Width - 10;
            }

            this.playerItem.Margin = new Thickness(xMargin + 5, yMargin + 5, 0, 0);
        }

        private void ClickGrid_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }
    }

    public class MatchStats
    {
        public double Assists = 0;
        public double BarracksKilled = 0;
        public double ChampionsKilled = 0;
        public double CombatPlayerScore = 0;
        public PlayerGameStats Game;
        public double GoldEarned = 0;
        public double Item0 = 0;
        public double Item1 = 0;
        public double Item2 = 0;
        public double Item3 = 0;
        public double Item4 = 0;
        public double Item5 = 0;
        public double Item6 = 0;
        public double LargestCriticalStrike = 0;
        public double LargestKillingSpree = 0;
        public double LargestMultiKill = 0;
        public double Level = 0;
        public double Lose = 0;
        public double MagicDamageDealtPlayer = 0;
        public double MagicDamageDealtToChampions = 0;
        public double MagicDamageTaken = 0;
        public double MinionsKilled = 0;
        public double NeutralMinionsKilled = 0;
        public double NeutralMinionsKilledEnemyJungle = 0;
        public double NeutralMinionsKilledYourJungle = 0;
        public double NodeCapture = 0;
        public double NodeCaptureAssist = 0;
        public double NodeNeutralize = 0;
        public double NodeNeutralizeAssist = 0;
        public double NumDeaths = 0;
        public double objectivePlayerScore = 0;
        public double PhysicalDamageDealtPlayer = 0;
        public double PhysicalDamageDealtToChampions = 0;
        public double PhysicalDamageTaken = 0;
        public double SightWardsBoughtInGame = 0;
        public double TeamObjective = 0;
        public double TotalDamageDealt = 0;
        public double TotalDamageDealtToChampions = 0;
        public double TotalDamageTaken = 0;
        public double TotalHeal = 0;
        public double TotalPlayerScore = 0;
        public double TotalScoreRank = 0;
        public double TotalTimeCrowdControlDealt = 0;
        public double TotalTimeSpentDead = 0;
        public double TrueDamageDealtPlayer = 0;
        public double TrueDamageDealtToChampions = 0;
        public double TrueDamageTaken = 0;
        public double TurretsKilled = 0;
        public double VictoryPointTotal = 0;
        public double VisionWardsBoughtInGame = 0;
        public double WardKilled = 0;
        public double WardPlaced = 0;
        public double Win = 0;
    }
}