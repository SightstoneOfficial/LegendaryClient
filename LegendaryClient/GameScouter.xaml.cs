using LegendaryClient.Logic;
using MahApps.Metro.Controls;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Summoner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using LegendaryClient.Logic.PlayerSpell;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Controls.GameScouter;
using LegendaryClient.Windows.Profile;
using PVPNetConnect.RiotObjects.Platform.Statistics;
using System.Reflection;

namespace LegendaryClient
{
    /// <summary>
    /// Interaction logic for GameScouter.xaml
    /// </summary>
    public partial class GameScouter : MetroWindow
    {
        public GameScouter()
        {
            InitializeComponent();
            Client.win = this;
            LoadScouter();
        }
        public void LoadScouter(string User = null)
        {
            if (String.IsNullOrEmpty(User))
                User = Client.LoginPacket.AllSummonerData.Summoner.Name;
            if (Convert.ToBoolean(IsUserValid(User)))
                LoadStats(User);
            else
            {
                Client.win.Visibility = Visibility.Hidden;
                Client.win.Close();
            }
        }
        private static async Task<bool> IsUserValid(string Username)
        {
            PublicSummoner sum = await Client.PVPNet.GetSummonerByName(Username);
            if (String.IsNullOrEmpty(sum.Name))
                return false;
            else
                return true;
        }

        private async void LoadStats(string user)
        {
            PlatformGameLifecycleDTO n = await Client.PVPNet.RetrieveInProgressSpectatorGameInfo(user);
            if (n.GameName != null)
            {
                Load(new List<Participant>(n.Game.TeamOne.ToArray()), n, BlueTeam);
                Load(new List<Participant>(n.Game.TeamTwo.ToArray()), n, PurpleTeam);
            }
            else
            {
                Client.win.Visibility = Visibility.Hidden;
                Client.win.Close();
            }
        }
        List<MatchStats> GameStats = new List<MatchStats>();
        private async void Load(List<Participant> allParticipants,PlatformGameLifecycleDTO n, ListView list)
        {
            foreach (Participant par in allParticipants)
            {
                if (par is PlayerParticipant)
                {
                    var participant = par as PlayerParticipant;
                    foreach (PlayerChampionSelectionDTO championSelect in n.Game.PlayerChampionSelections.Where(championSelect =>
                        championSelect.SummonerInternalName == participant.SummonerInternalName))
                    {
                        GameScouterPlayer control = new GameScouterPlayer();
                        control.Username.Content = championSelect.SummonerInternalName;
                        control.ChampIcon.Source = champions.GetChampion(championSelect.ChampionId).icon;
                        var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName(Convert.ToInt32(championSelect.Spell1Id))),
                            UriKind.Absolute);
                        control.SumIcon1.Source = new BitmapImage(uriSource);
                        uriSource =
                            new Uri(
                                Path.Combine(Client.ExecutingDirectory, "Assets", "spell",
                                    SummonerSpell.GetSpellImageName(Convert.ToInt32(championSelect.Spell2Id))),
                                UriKind.Absolute);
                        control.SumIcon2.Source = new BitmapImage(uriSource);
                        GameStats.Clear();
                        try
                        {
                            PublicSummoner summoner = await Client.PVPNet.GetSummonerByName(championSelect.SummonerInternalName);
                            RecentGames result = await Client.PVPNet.GetRecentGames(summoner.AcctId);
                            result.GameStatistics.Sort((s1, s2) => s2.CreateDate.CompareTo(s1.CreateDate));
                            foreach (PlayerGameStats game in result.GameStatistics)
                            {
                                game.GameType =
                                    Client.TitleCaseString(game.GameType.Replace("_GAME", "").Replace("MATCHED", "NORMAL"));
                                var match = new MatchStats();

                                foreach (RawStat stat in game.Statistics)
                                {
                                    Type type = typeof (MatchStats);
                                    string fieldName = Client.TitleCaseString(stat.StatType.Replace('_', ' ')).Replace(" ", "");
                                    FieldInfo f = type.GetField(fieldName);
                                    f.SetValue(match, stat.Value);
                                }
                                match.Game = game;
                                GameStats.Add(match);
                            }
                        }
                        catch
                        {

                        }

                        list.Items.Add(control);
                    }
                }
            }
        }
    }
}
