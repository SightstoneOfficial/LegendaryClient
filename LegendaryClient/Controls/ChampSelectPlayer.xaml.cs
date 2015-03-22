using LegendaryClient.Logic;
using LegendaryClient.Logic.Player;
using LegendaryClient.Logic.SQLite;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LegendaryClient.Logic.Riot.Leagues;
using LegendaryClient.Logic.Riot.Platform;

namespace LegendaryClient.Controls
{
    /// <summary>
    ///     Interaction logic for ChampSelectPlayer.xaml
    /// </summary>
    public partial class ChampSelectPlayer
    {
        private PlayerStatisticsChampSelect stats;
        public string sumName;
        public int champID;
        public bool KnownPar;

        public ChampSelectPlayer(string sumName = "", int champID = 0, bool known = true)
        {
            InitializeComponent();
            this.sumName = sumName;
            this.champID = champID;
        }

        private async void ChampPlayer_MouseOver(object sender, MouseEventArgs e)
        {
            if (!KnownPar)
                return;


            if (stats == null)
            {
                stats = new PlayerStatisticsChampSelect();
                var summoner = await Client.PVPNet.GetSummonerByName(sumName);
                if (summoner.SummonerLevel < 30)
                    stats.Rank.Content = "Level " + summoner.SummonerLevel;
                else
                {
                   SummonerLeaguesDTO playerLeagues = await Client.PVPNet.GetAllLeaguesForPlayer(summoner.SummonerId);
                   stats.Rank.Content = playerLeagues.SummonerLeagues;

                    foreach (LeagueListDTO x in playerLeagues.SummonerLeagues.Where(x => x.Queue == "RANKED_SOLO_5x5"))
                        stats.Rank.Content = x.Tier + " " + x.RequestorsRank;

                    if (string.IsNullOrEmpty(stats.Rank.Content.ToString()))
                        stats.Rank.Content = "Unranked";
                }
                if (summoner == null || stats == null || summoner.InternalName.Contains("bot"))
                {
                    stats = null;
                    return;
                }
                var topChampions = await Client.PVPNet.RetrieveTopPlayedChampions(summoner.AcctId, "CLASSIC");
                foreach (var x in topChampions)
                {
                    Debugger.Log(0, "CHAMPS", x.ChampionId + Environment.NewLine);
                }
                //_stats.PlayerName.Content = PlayerName.Content;
                GetKDA kda;
                if (topChampions.Length > 0 && KnownPar)
                {
                    if (topChampions[0].ChampionId != 0)
                    {
                        stats.MostPlayed.Source = champions.GetChampion((int)topChampions[0].ChampionId).icon;
                        stats.Champion1.Content = champions.GetChampion((int)topChampions[0].ChampionId).displayName +
                                                   " - Games: " + topChampions[0].TotalGamesPlayed;
                        var wins = 0.0;
                        var total = 0.0;
                        kda = new GetKDA((int)summoner.AcctId, (int)topChampions[0].ChampionId);
                        kda.stats.ChampID = (int)topChampions[0].ChampionId;

                        string[] xz = await kda.stats.Load((int)summoner.AcctId);
                        
                        stats.MPChamp.Content = xz[0];
                        foreach (var stat in topChampions[0].Stats)
                        {
                            switch (stat.StatType)
                            {
                                case "TOTAL_SESSIONS_WON":
                                    wins = stat.Value;
                                    break;
                                case "TOTAL_SESSIONS_PLAYED":
                                    total = stat.Value;
                                    break;
                            }
                        }

                        if ((Math.Abs(wins / total * 100.0) > 0) && Math.Abs(total) > 0)
                            stats.Champ1ProgressBar.Value = wins / total * 100.0;
                    }
                    else if (topChampions[1].ChampionId != 0 && topChampions.Length > 1)
                    {
                        if (topChampions[1].ChampionId != 0)
                        {
                            try
                            {

                                stats.MostPlayed.Source = champions.GetChampion((int)topChampions[1].ChampionId).icon;
                                stats.Champion1.Content = champions.GetChampion((int)topChampions[1].ChampionId).displayName +
                                                        " - Games: " + topChampions[1].TotalGamesPlayed;
                                var wins = 0.0;
                                var total = 0.0;
                                kda = new GetKDA((int)summoner.AcctId, (int)topChampions[1].ChampionId);
                                kda.stats.ChampID = (int)topChampions[1].ChampionId;
                                string[] x = await kda.stats.Load((int)summoner.AcctId);
                                stats.MPChamp.Content = x[0];
                                foreach (var stat in topChampions[1].Stats)
                                {
                                    switch (stat.StatType)
                                    {
                                        case "TOTAL_SESSIONS_WON":
                                            wins = stat.Value;
                                            break;
                                        case "TOTAL_SESSIONS_PLAYED":
                                            total = stat.Value;
                                            break;
                                    }
                                }

                                if ((Math.Abs(wins / total * 100.0) > 0) && Math.Abs(total) > 0)
                                    stats.Champ1ProgressBar.Value = wins / total * 100.0;
                                else
                                    stats.Champ1ProgressBar.Visibility = Visibility.Hidden;
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        stats.Champ1ProgressBar.Visibility = Visibility.Hidden;
                        stats.Champion1.Visibility = Visibility.Hidden;
                    }

                    kda = new GetKDA((int)summoner.AcctId, 103);
                    kda.stats.ChampID = 103;
                    string[] xm = await kda.stats.Load((int)summoner.AcctId);
                    try
                    {
                        stats.Overall.Content = xm[1];
                        stats.Champ3ProgressBar.Value = kda.stats.WinLossRatio;
                    }
                    catch { }

                    //
                    if (champID != 0)
                    {
                        kda = new GetKDA((int)summoner.AcctId, champID);
                        kda.stats.ChampID = champID;
                        string[] xmt = await kda.stats.Load((int)summoner.AcctId);
                        if (!string.IsNullOrEmpty(xmt[0]))
                        {
                            try
                            {

                                stats.Champ2ProgressBar.Value = kda.stats.WinLossChampRatio;
                                stats.CurrentChamp.Content = xmt[0];
                            }
                            catch
                            {
                                try
                                {
                                    stats.Champ2ProgressBar.Visibility = Visibility.Hidden;
                                    stats.CurrentChamp.Content = "NO GAMES!";
                                }
                                catch { }
                            }
                        }
                        else
                        {
                            try
                            {
                                stats.Champ2ProgressBar.Value = 0;
                                stats.CurrentChamp.Content = "NO GAMES!";
                            }
                            catch { }
                        }
                    }


                    Client.MainGrid.Children.Add(stats);
                }
                try
                {

                    var mouseLocation = e.GetPosition(Client.MainGrid);
                    var yMargin = mouseLocation.Y - 25;
                    if (Mouse.GetPosition(Client.MainGrid).X < 200)
                    {
                        stats.HorizontalAlignment = HorizontalAlignment.Left;
                        stats.VerticalAlignment = VerticalAlignment.Top;
                        stats.Margin = new Thickness(142, yMargin, 0, 0);
                    }
                    else
                    {
                        stats.HorizontalAlignment = HorizontalAlignment.Right;
                        stats.VerticalAlignment = VerticalAlignment.Top;
                        stats.Margin = new Thickness(0, yMargin, 155, 0);
                    }
                }
                catch { }
            }
        }

        private void ChampPlayer_MouseLeave(object sender, MouseEventArgs e)
        {
            Client.MainGrid.Children.Remove(stats);
        }
    }
}