using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.PlayerSpell;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect.RiotObjects.Platform.Statistics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for EndOfGamePage.xaml
    /// </summary>
    public partial class EndOfGamePage : Page
    {
        public EndOfGamePage(EndOfGameStats Statistics)
        {
            InitializeComponent();
            RenderStats(Statistics);
            Client.SwitchPage(new MainPage());
        }

        private void RenderStats(EndOfGameStats Statistics)
        {
            TimeSpan t = TimeSpan.FromSeconds(Statistics.GameLength);
            TimeLabel.Content = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
            ModeLabel.Content = Statistics.GameMode;
            TypeLabel.Content = Statistics.GameType;

            List<PlayerParticipantStatsSummary> AllParticipants = new List<PlayerParticipantStatsSummary>(Statistics.TeamPlayerParticipantStats.ToArray());
            AllParticipants.AddRange(Statistics.OtherTeamPlayerParticipantStats);

            foreach (PlayerParticipantStatsSummary summary in AllParticipants)
            {
                EndOfGamePlayer playerStats = new EndOfGamePlayer();
                champions Champ = champions.GetChampion(summary.SkinName); //Misleading variable name
                playerStats.ChampImage.Source = Champ.icon;
                playerStats.ChampLabel.Content = Champ.name;
                playerStats.PlayerLabel.Content = summary.SummonerName;
                var uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName((int)summary.Spell1Id));
                playerStats.Spell1Image.Source = Client.GetImage(uriSource);
                uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName((int)summary.Spell2Id));
                playerStats.Spell2Image.Source = Client.GetImage(uriSource);

                double ChampionsKilled = 0;
                double Assists = 0;
                double Deaths = 0;

                foreach (RawStatDTO stat in summary.Statistics)
                {
                    if (stat.StatTypeName.StartsWith("ITEM") && stat.Value != 0)
                    {
                        Image item = new Image();
                        uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "item", stat.Value + ".png");
                        item.Source = Client.GetImage(uriSource);
                        playerStats.ItemsListView.Items.Add(item);
                    }

                    switch (stat.StatTypeName)
                    {
                        case "GOLD_EARNED":
                            if (stat.Value > 0)
                            {
                                playerStats.GoldLabel.Content = string.Format("{0:N1}k", stat.Value / 1000);
                            }
                            break;

                        case "MINIONS_KILLED":
                            playerStats.CSLabel.Content = stat.Value;
                            break;

                        case "LEVEL":
                            playerStats.LevelLabel.Content = stat.Value;
                            break;

                        case "CHAMPIONS_KILLED":
                            ChampionsKilled = stat.Value;
                            break;

                        case "ASSISTS":
                            Assists = stat.Value;
                            break;

                        case "NUM_DEATHS":
                            Deaths = stat.Value;
                            break;

                        default:
                            break;
                    }
                }

                playerStats.ScoreLabel.Content = ChampionsKilled + "/" + Deaths + "/" + Assists;

                PlayersListView.Items.Add(playerStats);
            }

            PlayersListView.Items.Insert(AllParticipants.Count / 2, new Separator());

            /*championSkins Skin = championSkins.GetSkin(Statistics.SkinIndex);
            var skinSource = Path.Combine(Client.ExecutingDirectory, "Assets", "champions", Skin.splashPath);
            SkinImage.Source = Client.GetImage(skinSource);*/
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Visibility = Visibility.Hidden;
        }
    }
}