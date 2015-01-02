#region

using System;
using System.Windows;
using System.Windows.Input;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Windows;
using LegendaryClient.Logic.Player;

#endregion

namespace LegendaryClient.Controls
{
    /// <summary>
    ///     Interaction logic for ChampSelectPlayer.xaml
    /// </summary>
    public partial class ChampSelectPlayer
    {
        private PlayerStatisticsChampSelect _stats;

        public ChampSelectPlayer()
        {
            InitializeComponent();
        }

        private async void ChampPlayer_MouseOver(object sender, MouseEventArgs e)
        {
            if (PlayerName.Content.ToString().StartsWith("Summoner "))
                return;

            try
            {
                if (_stats == null)
                {
                    _stats = new PlayerStatisticsChampSelect();
                    var summoner = await Client.PVPNet.GetSummonerByName(PlayerName.Content.ToString());
                    if (summoner == null || _stats == null || summoner.InternalName.Contains("bot"))
                    {
                        _stats = null;
                        return;
                    }
                    var topChampions = await Client.PVPNet.RetrieveTopPlayedChampions(summoner.AcctId, "CLASSIC");
                    _stats.PlayerName.Content = summoner.Name;
                    GetKDA kda;
                    if (topChampions.Length > 0)
                    {
                        _stats.MostPlayed.Source = champions.GetChampion((int) topChampions[0].ChampionId).icon;
                        _stats.Champion1.Content = champions.GetChampion((int) topChampions[0].ChampionId).displayName +
                                                   " - Games: " + topChampions[0].TotalGamesPlayed;
                        var wins = 0.0;
                        var total = 0.0;
                        kda = new GetKDA((int)summoner.AcctId, (int)topChampions[0].ChampionId);
                        _stats.MPChamp.Content = kda.stats.Champkda.KDAToString();
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

                        if ((Math.Abs(wins/total*100.0) > 0) && Math.Abs(total) > 0)
                            _stats.Champ1ProgressBar.Value = wins/total*100.0;
                        else
                            _stats.Champ1ProgressBar.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        _stats.Champ1ProgressBar.Visibility = Visibility.Hidden;
                        _stats.Champion1.Visibility = Visibility.Hidden;
                    }

                    kda = new GetKDA((int)summoner.AcctId, 103);
                    _stats.Overall.Content = kda.stats.OverallKDA.KDAToString();
                    _stats.Champ3ProgressBar.Value = kda.stats.WinLossRatio;

                    //
                    if (this.Tag != null)
                    {
                        kda = new GetKDA((int)summoner.AcctId, (int)this.Tag);
                        if (kda.stats.GamesWithChamp != 0)
                        {
                            _stats.Champ2ProgressBar.Value = kda.stats.WinLossChampRatio;
                            _stats.CurrentChamp.Content = kda.stats.Champkda;
                        }
                        else
                        {
                            _stats.Champ2ProgressBar.Value = 0;
                            _stats.CurrentChamp.Content = "NO GAMES!";
                        }
                    }
                    

                    Client.MainGrid.Children.Add(_stats);
                }
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
            catch
            {
            }
        }

        private void ChampPlayer_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_stats == null)
                return;

            Client.MainGrid.Children.Remove(_stats);
            _stats = null;
        }
    }
}