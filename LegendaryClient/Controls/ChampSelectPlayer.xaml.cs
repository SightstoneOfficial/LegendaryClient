#region

using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Windows;
using LegendaryClient.Logic.Player;
using System.Diagnostics;
using PVPNetConnect.RiotObjects.Leagues.Pojo;
using PVPNetConnect.RiotObjects.Platform.Leagues.Client.Dto;

#endregion

namespace LegendaryClient.Controls
{
    /// <summary>
    ///     Interaction logic for ChampSelectPlayer.xaml
    /// </summary>
    public partial class ChampSelectPlayer
    {
        private PlayerStatisticsChampSelect _stats;
        public string _sumName;
        public int _champID;
        public bool KnownPar;

        public ChampSelectPlayer(string sumName = "", int champID = 0, bool known = true)
        {
            InitializeComponent();
            _sumName = sumName;
            _champID = champID;
        }

        private async void ChampPlayer_MouseOver(object sender, MouseEventArgs e)
        {
            if (!KnownPar)
                return;


            if (_stats == null)
            {
                _stats = new PlayerStatisticsChampSelect();
                var summoner = await Client.PVPNet.GetSummonerByName(_sumName);
                if (summoner.SummonerLevel < 30)
                    _stats.Rank.Content = "Level " + summoner.SummonerLevel;
                else
                {
                   SummonerLeaguesDTO playerLeagues = await Client.PVPNet.GetAllLeaguesForPlayer(summoner.SummonerId);
                   _stats.Rank.Content = playerLeagues.SummonerLeagues;

                    foreach (LeagueListDTO x in playerLeagues.SummonerLeagues.Where(x => x.Queue == "RANKED_SOLO_5x5"))
                        _stats.Rank.Content = x.Tier + " " + x.RequestorsRank;

                    if (String.IsNullOrEmpty(_stats.Rank.Content.ToString()))
                        _stats.Rank.Content = "Unranked";
                }
                if (summoner == null || _stats == null || summoner.InternalName.Contains("bot"))
                {
                    _stats = null;
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
                        _stats.MostPlayed.Source = champions.GetChampion((int)topChampions[0].ChampionId).icon;
                        _stats.Champion1.Content = champions.GetChampion((int)topChampions[0].ChampionId).displayName +
                                                   " - Games: " + topChampions[0].TotalGamesPlayed;
                        var wins = 0.0;
                        var total = 0.0;
                        kda = new GetKDA((int)summoner.AcctId, (int)topChampions[0].ChampionId);
                        kda.stats.ChampID = (int)topChampions[0].ChampionId;

                        string[] xz = await kda.stats.Load((int)summoner.AcctId);
                        
                        _stats.MPChamp.Content = xz[0];
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
                            _stats.Champ1ProgressBar.Value = wins / total * 100.0;
                    }
                    else if (topChampions[1].ChampionId != 0 && topChampions.Length > 1)
                    {
                        if (topChampions[1].ChampionId != 0)
                        {
                            try
                            {

                                _stats.MostPlayed.Source = champions.GetChampion((int)topChampions[1].ChampionId).icon;
                                _stats.Champion1.Content = champions.GetChampion((int)topChampions[1].ChampionId).displayName +
                                                        " - Games: " + topChampions[1].TotalGamesPlayed;
                                var wins = 0.0;
                                var total = 0.0;
                                kda = new GetKDA((int)summoner.AcctId, (int)topChampions[1].ChampionId);
                                kda.stats.ChampID = (int)topChampions[1].ChampionId;
                                string[] x = await kda.stats.Load((int)summoner.AcctId);
                                _stats.MPChamp.Content = x[0];
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
                                    _stats.Champ1ProgressBar.Value = wins / total * 100.0;
                                else
                                    _stats.Champ1ProgressBar.Visibility = Visibility.Hidden;
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        _stats.Champ1ProgressBar.Visibility = Visibility.Hidden;
                        _stats.Champion1.Visibility = Visibility.Hidden;
                    }

                    kda = new GetKDA((int)summoner.AcctId, 103);
                    kda.stats.ChampID = 103;
                    string[] xm = await kda.stats.Load((int)summoner.AcctId);
                    try
                    {
                        _stats.Overall.Content = xm[1];
                        _stats.Champ3ProgressBar.Value = kda.stats.WinLossRatio;
                    }
                    catch { }

                    //
                    if (_champID != 0)
                    {
                        kda = new GetKDA((int)summoner.AcctId, _champID);
                        kda.stats.ChampID = _champID;
                        string[] xmt = await kda.stats.Load((int)summoner.AcctId);
                        if (!String.IsNullOrEmpty(xmt[0]))
                        {
                            try
                            {

                                _stats.Champ2ProgressBar.Value = kda.stats.WinLossChampRatio;
                                _stats.CurrentChamp.Content = xmt[0];
                            }
                            catch
                            {
                                try
                                {
                                    _stats.Champ2ProgressBar.Visibility = Visibility.Hidden;
                                    _stats.CurrentChamp.Content = "NO GAMES!";
                                }
                                catch { }
                            }
                        }
                        else
                        {
                            try
                            {
                                _stats.Champ2ProgressBar.Value = 0;
                                _stats.CurrentChamp.Content = "NO GAMES!";
                            }
                            catch { }
                        }
                    }


                    Client.MainGrid.Children.Add(_stats);
                }
                try
                {

                    var mouseLocation = e.GetPosition(Client.MainGrid);
                    var yMargin = mouseLocation.Y - 25;
                    if (Mouse.GetPosition(Client.MainGrid).X < 200)
                    {
                        _stats.HorizontalAlignment = HorizontalAlignment.Left;
                        _stats.VerticalAlignment = VerticalAlignment.Top;
                        _stats.Margin = new Thickness(142, yMargin, 0, 0);
                    }
                    else
                    {
                        _stats.HorizontalAlignment = HorizontalAlignment.Right;
                        _stats.VerticalAlignment = VerticalAlignment.Top;
                        _stats.Margin = new Thickness(0, yMargin, 155, 0);
                    }
                }
                catch { }
            }
        }

        private void ChampPlayer_MouseLeave(object sender, MouseEventArgs e)
        {
            Client.MainGrid.Children.Remove(_stats);
        }
    }
}