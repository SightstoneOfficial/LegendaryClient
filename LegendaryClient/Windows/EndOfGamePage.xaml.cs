﻿#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using jabber;
using jabber.connection;
using jabber.protocol.client;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.PlayerSpell;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Properties;
using PVPNetConnect.RiotObjects.Platform.Statistics;

#endregion

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for EndOfGamePage.xaml
    /// </summary>
    public partial class EndOfGamePage
    {
        private readonly Room _newRoom;
        private string MatchStatsOnline;
        
        public EndOfGamePage(EndOfGameStats statistics)
        {
            InitializeComponent();
            Change();

            RenderStats(statistics);
            uiLogic.UpdateMainPage();
            Client.runonce = false;

            string jid = Client.GetChatroomJID(statistics.RoomName, statistics.RoomPassword, false);
            _newRoom = Client.ConfManager.GetRoom(new JID(jid));
            _newRoom.Nickname = Client.LoginPacket.AllSummonerData.Summoner.Name;
            _newRoom.OnRoomMessage += newRoom_OnRoomMessage;
            _newRoom.OnParticipantJoin += newRoom_OnParticipantJoin;
            _newRoom.Join(statistics.RoomPassword);
        }

        public void Change()
        {
            var themeAccent = new ResourceDictionary
            {
                Source = new Uri(Settings.Default.Theme)
            };
            Resources.MergedDictionaries.Add(themeAccent);
        }

        private void newRoom_OnParticipantJoin(Room room, RoomParticipant participant)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                {
                    Text = participant.Nick + " joined the room." + Environment.NewLine
                };
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);

                ChatText.ScrollToEnd();
            }));
        }

        private void newRoom_OnRoomMessage(object sender, Message msg)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (msg.Body == "This room is not anonymous")
                    return;

                var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                {
                    Text = msg.From.Resource + ": "
                };
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Turquoise);
                tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                if (Client.Filter)
                    tr.Text = msg.InnerText.Replace("<![CDATA[", "").Replace("]]>", "").Filter() +
                              Environment.NewLine;
                else
                    tr.Text = msg.InnerText.Replace("<![CDATA[", "").Replace("]]>", "") + Environment.NewLine;

                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);

                ChatText.ScrollToEnd();
            }));
        }

        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
            {
                Text = Client.LoginPacket.AllSummonerData.Summoner.Name + ": "
            };
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
            tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
            if (Client.Filter)
                tr.Text = ChatTextBox.Text.Filter() + Environment.NewLine;
            else
                tr.Text = ChatTextBox.Text + Environment.NewLine;

            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
            if (String.IsNullOrEmpty(ChatTextBox.Text))
                return;

            _newRoom.PublicMessage(ChatTextBox.Text);
            ChatTextBox.Text = "";
            ChatText.ScrollToEnd();
        }

        private void RenderStats(EndOfGameStats statistics)
        {
            TimeSpan t = TimeSpan.FromSeconds(statistics.GameLength);
            TimeLabel.Content = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
            ModeLabel.Content = statistics.GameMode;
            TypeLabel.Content = statistics.GameType;
            // Add Garena TW match history
            if (Client.Garena && !String.IsNullOrEmpty(Settings.Default.DefaultGarenaRegion) && Settings.Default.DefaultGarenaRegion == "TW")
                MatchStatsOnline = String.Format("http://lol.moa.tw/summoner/show/{0}#tabs-recentgame2", statistics.SummonerName.Replace(" ", "_"));
            else
                MatchStatsOnline = "http://matchhistory.na.leagueoflegends.com/en/#match-details/" + Client.Region.InternalName + "/" + statistics.ReportGameId + "/" + statistics.UserId;

            GainedIP.Content = "+" + statistics.IpEarned + " IP";
            TotalIP.Content = statistics.IpTotal.ToString(CultureInfo.InvariantCulture).Replace(".0", "") + " IP Total";
            string game = " XP";
            var allParticipants =
                new List<PlayerParticipantStatsSummary>(statistics.TeamPlayerParticipantStats.ToArray());
            allParticipants.AddRange(statistics.OtherTeamPlayerParticipantStats);
            foreach (PlayerParticipantStatsSummary summary in allParticipants)
            {
                var playerStats = new EndOfGamePlayer(summary.UserId, summary.GameId, summary.SummonerName, statistics.TeamPlayerParticipantStats.Contains(summary));
                champions champ = champions.GetChampion(summary.SkinName); //Misleading variable name
                playerStats.ChampImage.Source = champ.icon;
                playerStats.ChampLabel.Content = champ.name;
                playerStats.PlayerLabel.Content = summary.SummonerName;
                if (File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName((int)summary.Spell1Id))))
                {
                    var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName((int)summary.Spell1Id)), UriKind.Absolute);
                    playerStats.Spell1Image.Source = new BitmapImage(uriSource);
                }
                else
                    Client.Log(SummonerSpell.GetSpellImageName((int)summary.Spell1Id) + " is missing");
                if (File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName((int)summary.Spell2Id))))
                {
                    var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName((int)summary.Spell2Id)), UriKind.Absolute);
                    playerStats.Spell2Image.Source = new BitmapImage(uriSource);
                }
                else
                    Client.Log(SummonerSpell.GetSpellImageName((int)summary.Spell2Id) + " is missing");
                double championsKilled = 0;
                double assists = 0;
                double deaths = 0;
                bool victory = false;
                foreach (RawStatDTO stat in summary.Statistics.Where(stat => stat.StatTypeName.ToLower() == "win"))
                {
                    if (summary.SummonerName == Client.LoginPacket.AllSummonerData.Summoner.Name)
                    {
                        victory = true;
                        GameResultLabel.Content = "Victory";
                    }
                        
                }

                if (statistics.Ranked)
                {
                    game = " LP";
                    GainedXP.Content = (victory ? "+" : "-") + statistics.ExperienceEarned + game;
                    TotalXP.Content = statistics.ExperienceTotal + game;
                }
                else
                {
                    if (Client.LoginPacket.AllSummonerData.SummonerLevel.Level < 30)
                    {
                        GainedXP.Content = "+" + statistics.ExperienceEarned + game;
                        TotalXP.Content = statistics.ExperienceTotal + game;
                    }
                    else
                    {
                        GainedXP.Visibility = Visibility.Hidden;
                        TotalXP.Visibility = Visibility.Hidden;
                    }
                }

                foreach (RawStatDTO stat in summary.Statistics)
                {
                    if (stat.StatTypeName.StartsWith("ITEM") && Math.Abs(stat.Value) > 0)
                    {
                        var item = new Image();
                        if (File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "item", stat.Value + ".png")))
                        {
                            var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "item", stat.Value + ".png"), UriKind.Absolute);
                            item.Source = new BitmapImage(uriSource);
                        }
                        else
                            Client.Log(stat.Value + ".png is missing");
                        playerStats.ItemsListView.Items.Add(item);
                    }
                    switch (stat.StatTypeName)
                    {
                        case "GOLD_EARNED":
                            if (stat.Value > 0)
                            {
                                playerStats.GoldLabel.Content = string.Format("{0:N1}k", stat.Value/1000);
                            }
                            break;

                        case "MINIONS_KILLED":
                            playerStats.CsLabel.Content = stat.Value;
                            break;

                        case "LEVEL":
                            playerStats.LevelLabel.Content = stat.Value;
                            break;

                        case "CHAMPIONS_KILLED":
                            championsKilled = stat.Value;
                            break;

                        case "ASSISTS":
                            assists = stat.Value;
                            break;

                        case "NUM_DEATHS":
                            deaths = stat.Value;
                            break;
                    }
                }
                playerStats.ScoreLabel.Content = championsKilled + "/" + deaths + "/" + assists;
                PlayersListView.Items.Add(playerStats);                
            }
            PlayersListView.Items.Insert(allParticipants.Count/2, new Separator());
            championSkins skin = championSkins.GetSkin(statistics.SkinIndex);
            try
            {
                if (skin == null)
                    return;

                var skinSource =
                    new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", skin.splashPath),
                        UriKind.Absolute);
                SkinImage.Source = new BitmapImage(skinSource);
            }
            catch (Exception)
            {
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _newRoom.Leave(null);
            Client.OverlayContainer.Visibility = Visibility.Hidden;
            Client.ClearPage(typeof(EndOfGamePage));
        }

        private void OnlineHistory_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(MatchStatsOnline);
        }
    }
}
