﻿using Sightstone.Controls.GameScouter;
using Sightstone.Logic;
using Sightstone.Logic.PlayerSpell;
using Sightstone.Logic.SQLite;
using Sightstone.Windows.Profile;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Sightstone.Logic.Riot;
using Sightstone.Logic.Riot.Platform;
using Sightstone.Logic.MultiUser;

namespace Sightstone
{
    /// <summary>
    /// Interaction logic for GameScouter.xaml
    /// </summary>
    public partial class GameScouter : MetroWindow
    {
        string GSUsername;
        Point mouseLocation;
        KeyValuePair<bool, TinyRuneMasteryData> mouseEntered;
        static UserClient UserClient;

        public GameScouter()
        {
            InitializeComponent();
            UserClient = (UserList.Users[Client.CurrentServer])[Client.CurrentUser];
            Client.win = this;
        }
        public async void LoadScouter(string User = null)
        {
            if (string.IsNullOrEmpty(User))
                User = UserClient.LoginPacket.AllSummonerData.Summoner.Name;

            GSUsername = User;

            bool x = await IsUserValid(User);
            if (x)
                LoadStats(User);
            else
            {
                Client.win.Visibility = Visibility.Hidden;
                Client.win.Close();
            }
        }
        private static async Task<bool> IsUserValid(string Username)
        {
            PublicSummoner sum = await UserClient.calls.GetSummonerByName(Username);
            if (string.IsNullOrEmpty(sum.Name))
                return false;
            else
                return true;
        }

        private async void LoadStats(string user)
        {
            PlatformGameLifecycleDTO n = await UserClient.calls.RetrieveInProgressSpectatorGameInfo(user);
            if (n != null)
            {
                if (n.GameName != null)
                {
                    LoadPar(new List<Participant>(n.Game.TeamOne.ToArray()), n, BlueTeam);
                    LoadPar(new List<Participant>(n.Game.TeamTwo.ToArray()), n, PurpleTeam);
                }
            }
            else
            {
                Client.win.Visibility = Visibility.Hidden;
                Client.win.Close();
            }
        }
        List<MatchStats> GameStats = new List<MatchStats>();
        Dictionary<double, Brush> color = new Dictionary<double, Brush>();
        int ColorId;
        /// <summary>
        /// Loads the particiapants for the game
        /// </summary>
        /// <param name="allParticipants"></param>
        /// <param name="n"></param>
        /// <param name="list"></param>
        private async void LoadPar(List<Participant> allParticipants,PlatformGameLifecycleDTO n, ListView list)
        {
            bool isYourTeam = false;
            list.Items.Clear();
            list.Items.Refresh();
            try
            {
                string mmrJson;
                string url = UserClient.Region.SpectatorLink + "consumer/getGameMetaData/" + UserClient.Region.InternalName +
                             "/" + n.Game.Id + "/token";
                using (var client = new WebClient())
                    mmrJson = client.DownloadString(url);

                var serializer = new JavaScriptSerializer();
                var deserializedJson = serializer.Deserialize<Dictionary<string, object>>(mmrJson);
                MMRLabel.Content = "Game MMR ≈ " + deserializedJson["interestScore"];
            }
            catch
            {
                MMRLabel.Content = "Unable to calculate Game MMR";
            }

            foreach (Participant par in allParticipants)
            {
                if (par is PlayerParticipant)
                {
                    PublicSummoner scoutersum = await UserClient.calls.GetSummonerByName(GSUsername);
                    if ((par as PlayerParticipant).AccountId == scoutersum.AcctId)
                        isYourTeam = true;
                }
            }
            foreach (Participant par in allParticipants)
            {
                if (par is PlayerParticipant)
                {
                    var participant = par as PlayerParticipant;
                    foreach (PlayerChampionSelectionDTO championSelect in n.Game.PlayerChampionSelections.Where(championSelect =>
                        championSelect.SummonerInternalName == participant.SummonerInternalName))
                    {
                        GameScouterPlayer control = new GameScouterPlayer();
                        control.Tag = championSelect;
                        GameStats = new List<MatchStats>();
                        control.Username.Content = championSelect.SummonerInternalName;
                        //Make it so you can see yourself
                        if (championSelect.SummonerInternalName == GSUsername)
                            control.Username.Foreground = (Brush)(new BrushConverter().ConvertFrom("#FF007A53"));
                        control.ChampIcon.Source = champions.GetChampion(championSelect.ChampionId).icon;
                        if (File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName(Convert.ToInt32(championSelect.Spell1Id)))))
                        {
                            var UriSource = new System.Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName(Convert.ToInt32(championSelect.Spell1Id))), UriKind.Absolute);
                            control.SumIcon1.Source = new BitmapImage(UriSource);
                        }
                        if (File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName(Convert.ToInt32(championSelect.Spell2Id)))))
                        {
                            var UriSource = new System.Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName(Convert.ToInt32(championSelect.Spell2Id))), UriKind.Absolute);
                            control.SumIcon2.Source = new BitmapImage(UriSource);
                        }
                        GameStats.Clear();
                        try
                        {
                            PublicSummoner summoner = await UserClient.calls.GetSummonerByName(championSelect.SummonerInternalName.Replace("summoner", string.Empty));
                            if(File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", summoner.ProfileIconId.ToString() + ".png")))
                                control.ProfileIcon.Source = new BitmapImage(Client.GetIconUri(summoner.ProfileIconId));
                            RecentGames result = await UserClient.calls.GetRecentGames(summoner.AcctId);
                            result.GameStatistics.Sort((s1, s2) => s2.CreateDate.CompareTo(s1.CreateDate));
                            foreach (PlayerGameStats game in result.GameStatistics)
                            {
                                game.GameType = Client.TitleCaseString(game.GameType.Replace("_GAME", "").Replace("MATCHED", "NORMAL"));
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
                            int Kills, ChampKills;
                            int Deaths, ChampDeaths;
                            int Assists, ChampAssists;
                            int GamesPlayed, ChampGamesPlayed;
                            Kills = 0; Deaths = 0; Assists = 0; GamesPlayed = 0; ChampKills = 0; ChampDeaths = 0; ChampAssists = 0; ChampGamesPlayed = 0;
                            //Load average KDA for past 20 games if possible
                            foreach (MatchStats stats in GameStats)
                            {
                                champions gameChamp = champions.GetChampion((int)Math.Round(stats.Game.ChampionId));
                                Kills = Kills + (int)stats.ChampionsKilled;
                                Deaths = Deaths + (int)stats.NumDeaths;
                                Assists = Assists + (int)stats.Assists;
                                GamesPlayed++;
                                if (championSelect.ChampionId == (int)Math.Round(stats.Game.ChampionId))
                                {
                                    ChampKills = ChampKills + (int)stats.ChampionsKilled;
                                    ChampDeaths = ChampDeaths + (int)stats.NumDeaths;
                                    ChampAssists = ChampAssists + (int)stats.Assists;
                                    ChampGamesPlayed++;
                                }
                            }
                            //GetKDA string
                            string KDAString = string.Format("{0}/{1}/{2}",
                                (Kills / GamesPlayed),
                                (Deaths / GamesPlayed),
                                (Assists / GamesPlayed));
                            string ChampKDAString = "";
                            try
                            {
                                ChampKDAString = string.Format("{0}/{1}/{2}",
                                (ChampKills / ChampGamesPlayed),
                                (ChampDeaths / ChampGamesPlayed),
                                (ChampAssists / ChampGamesPlayed));
                                
                            }
                            catch { }
                            
                            if (ChampGamesPlayed == 0)
                                ChampKDAString = "No Games lately";
                            control.AverageKDA.Content = KDAString;
                            control.ChampAverageKDA.Content = ChampKDAString;
                            BrushConverter bc = new BrushConverter();
                            if (isYourTeam)
                            {
                                bc = new BrushConverter();
                                if (ChampKills < ChampDeaths)
                                    control.ChampKDAColor.Fill = (Brush)bc.ConvertFrom("#FFC51C1C");
                                else if (ChampKills == ChampDeaths)
                                    control.ChampKDAColor.Fill = (Brush)bc.ConvertFrom("#FF8D8D8D");
                                else
                                    control.ChampKDAColor.Fill = (Brush)bc.ConvertFrom("#FF1EBF1E");

                                bc = new BrushConverter();
                                if (Kills < Deaths)
                                    control.GameKDAColor.Fill = (Brush)bc.ConvertFrom("#FFC51C1C");
                                else if (Kills == Deaths)
                                    control.GameKDAColor.Fill = (Brush)bc.ConvertFrom("#FF8D8D8D");
                                else
                                    control.GameKDAColor.Fill = (Brush)bc.ConvertFrom("#FF1EBF1E");
                            }
                            else
                            {
                                bc = new BrushConverter();
                                if (ChampKills > ChampDeaths)
                                    control.ChampKDAColor.Fill = (Brush)bc.ConvertFrom("#FFC51C1C");
                                else if (ChampKills == ChampDeaths)
                                    control.ChampKDAColor.Fill = (Brush)bc.ConvertFrom("#FF8D8D8D");
                                else
                                    control.ChampKDAColor.Fill = (Brush)bc.ConvertFrom("#FF1EBF1E");

                                bc = new BrushConverter();
                                if (Kills > Deaths)
                                    control.GameKDAColor.Fill = (Brush)bc.ConvertFrom("#FFC51C1C");
                                else if (Kills == Deaths)
                                    control.GameKDAColor.Fill = (Brush)bc.ConvertFrom("#FF8D8D8D");
                                else
                                    control.GameKDAColor.Fill = (Brush)bc.ConvertFrom("#FF1EBF1E");
                            }
                        }
                        catch
                        {
                            Client.Log("Failed to get stats about player", "GAME_SCOUTER_ERROR");
                        }
                        if (participant.TeamParticipantId != null)
                        {
                            try
                            {
                                Brush myColor = color[(double)participant.TeamParticipantId];
                                control.QueueTeamColor.Fill = myColor;
                                control.QueueTeamColor.Visibility = Visibility.Visible;
                            }
                            catch
                            {
                                BrushConverter bc = new BrushConverter();
                                Brush brush = Brushes.White;
                                //I know that there is a better way in the InGamePage
                                //I find that sometimes the colors (colours) are very hard to distinguish from eachother
                                //This makes sure that each color is easy to see
                                //because of hexa hill I put 12 in just in case
                                switch (ColorId)
                                {
                                    case 0:
                                        //blue
                                        brush = (Brush)bc.ConvertFrom("#FF00E8FF");
                                        break;
                                    case 2:
                                        //Lime Green
                                        brush = (Brush)bc.ConvertFrom("#FF00FF00");
                                        break;
                                    case 3:
                                        //Yellow
                                        brush = (Brush)bc.ConvertFrom("#FFFFFF00");
                                        break;
                                    case 4:
                                        //Blue Green
                                        brush = (Brush)bc.ConvertFrom("#FF007A53");
                                        break;
                                    case 5:
                                        //Purple
                                        brush = (Brush)bc.ConvertFrom("#FF5100FF");
                                        break;
                                    case 6:
                                        //Pink
                                        brush = (Brush)bc.ConvertFrom("#FFCB46C5");
                                        break;
                                    case 7:
                                        //Dark Green
                                        brush = (Brush)bc.ConvertFrom("#FF006409");
                                        break;
                                    case 8:
                                        //Brown
                                        brush = (Brush)bc.ConvertFrom("#FF643200");
                                        break;
                                    case 9: 
                                        //White
                                        brush = (Brush)bc.ConvertFrom("#FFFFFFFF");
                                        break;
                                    case 10:
                                        //Grey
                                        brush = (Brush)bc.ConvertFrom("#FF363636");
                                        break;
                                    case 11:
                                        //Red Pink
                                        brush = (Brush)bc.ConvertFrom("#FF8F4242");
                                        break;
                                    case 12:
                                        //Grey Blue
                                        brush = (Brush)bc.ConvertFrom("#FFFF0000");
                                        break;
                                }
                                color.Add((double)participant.TeamParticipantId, brush);
                                ColorId++;
                                control.QueueTeamColor.Fill = brush;
                                control.QueueTeamColor.Visibility = Visibility.Visible;
                            }
                        }
                        //control.MouseMove += controlMouseEnter;
                        control.MouseEnter += controlMouseEnter;
                        control.MouseLeave += control_MouseLeave;
                        control.MouseDown += control_MouseDown;
                        TinyRuneMasteryData smallData = new TinyRuneMasteryData();
                        //Now store data in the tags so that all of the event handlers work
                        Dictionary<string, object> data = new Dictionary<string, object>();
                        data.Add("MasteryDataControl", smallData);
                        data.Add("RuneData", await GetUserRunesPage(GSUsername));
                        data.Add("MasteryData", await GetUserRunesPage(GSUsername));
                        control.Tag = data;
                        list.Items.Add(control);
                    }
                }
            }
        }

        void control_MouseDown(object sender, MouseButtonEventArgs e)
        {
            GameScouterPlayer control = (GameScouterPlayer)sender;
            object m = ((Dictionary<string, object>)control.Tag)["MasteryDataControl"];
            TinyRuneMasteryData smallData;
            if (m.GetType() == typeof(TinyRuneMasteryData))
            {
                smallData = (TinyRuneMasteryData)m;
            }
            else
            {
                smallData = new TinyRuneMasteryData();
            }
            try
            {
                Mousegrid.Children.Remove(smallData);
            }
            catch { }
            //start the popup
            //TODO

        }

        void control_MouseLeave(object sender, MouseEventArgs e)
        {
            GameScouterPlayer control = (GameScouterPlayer)sender;
            control.Tooltip.Content = "Hover over for info";
            object m = ((Dictionary<string, object>)control.Tag)["MasteryDataControl"];
            TinyRuneMasteryData smallData;
            if (m.GetType() == typeof(TinyRuneMasteryData))
            {
                smallData = (TinyRuneMasteryData)m;
            }
            else
            {
                smallData = new TinyRuneMasteryData();
            }
            if (mouseEntered.Key && mouseEntered.Value == smallData)
                return;
            try
            {
                Mousegrid.Children.Remove(smallData);
            }
            catch { }
        }

        void controlMouseEnter(object sender, MouseEventArgs e)
        {
            GameScouterPlayer control = (GameScouterPlayer)sender;
            object m = ((Dictionary<string, object>)control.Tag)["MasteryDataControl"];
            TinyRuneMasteryData smallData;
            if (m.GetType() == typeof(TinyRuneMasteryData))
            {
                smallData = (TinyRuneMasteryData)m;
            }
            else
            {
                smallData = new TinyRuneMasteryData();
                ((Dictionary<string, object>)control.Tag).Add("MasteryDataControl", smallData);
            }
            smallData.MouseEnter += (o, g) =>
                {
                    mouseEntered = new KeyValuePair<bool, TinyRuneMasteryData>(true, smallData);
                };
            smallData.MouseLeave += (o, g) =>
                {
                    mouseEntered = new KeyValuePair<bool, TinyRuneMasteryData>(false, smallData);
                };
            control.Tooltip.Content = "Click for even more info";
            mouseLocation = e.GetPosition(Mousegrid);
            smallData.HorizontalAlignment = HorizontalAlignment.Left;
            smallData.VerticalAlignment = VerticalAlignment.Top;
            smallData.Margin = new Thickness(mouseLocation.X - smallData.Width, mouseLocation.Y + 20, 0, 0);
            try
            {
                Mousegrid.Children.Add(smallData);
            }
            catch { }
        }
        private async Task<SummonerRuneInventory> GetUserRunesPage(string User)
        {
            PublicSummoner summoner = await UserClient.calls.GetSummonerByName(User);
            SummonerRuneInventory runes = await UserClient.calls.GetSummonerRuneInventory(summoner.SummonerId);
            return runes;


        }
        private async Task<MasteryBookDTO> GetUserMasterPage(string User)
        {
            PublicSummoner summoner = await UserClient.calls.GetSummonerByName(User);
            MasteryBookDTO page = await UserClient.calls.GetMasteryBook(summoner.SummonerId);
            return page;
        }
    }
}
