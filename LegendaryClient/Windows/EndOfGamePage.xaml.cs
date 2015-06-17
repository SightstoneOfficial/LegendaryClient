using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.PlayerSpell;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Properties;
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
using LegendaryClient.Logic.Riot.Platform;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.x.muc;

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for EndOfGamePage.xaml
    /// </summary>
    public partial class EndOfGamePage
    {
        private readonly MucManager newRoom;
        private string MatchStatsOnline;
        private readonly string RoomJid;
        
        public EndOfGamePage(EndOfGameStats statistics)
        {
            InitializeComponent();
            RenderStats(statistics);
            Client.SwitchPage(Client.MainPage);
            Client.runonce = false;
            Client.ChampId = -1;
            RoomJid = Client.GetChatroomJid(statistics.RoomName, statistics.RoomPassword, false);
            
            newRoom = new MucManager(Client.XmppConnection);
            Client.XmppConnection.OnMessage += XmppConnection_OnMessage;
            newRoom.OnParticipantJoin += newRoom_OnParticipantJoin;
            newRoom.AcceptDefaultConfiguration(new Jid(RoomJid));
            newRoom.JoinRoom(new Jid(RoomJid), Client.LoginPacket.AllSummonerData.Summoner.Name);
        }

        void XmppConnection_OnMessage(object sender, Message msg)
        {
            if (msg.To.Bare != RoomJid)
                return;

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
                    tr.Text = msg.Body.Replace("<![CDATA[", "").Replace("]]>", "").Filter() +
                              Environment.NewLine;
                else
                    tr.Text = msg.Body.Replace("<![CDATA[", "").Replace("]]>", "") + Environment.NewLine;

                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);

                ChatText.ScrollToEnd();
            }));
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
            if (string.IsNullOrEmpty(ChatTextBox.Text))
                return;

            Client.XmppConnection.Send(new Message(new Jid(RoomJid), ChatTextBox.Text));
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
            if (Client.Garena && !string.IsNullOrEmpty(Settings.Default.DefaultGarenaRegion) && Settings.Default.DefaultGarenaRegion == "TW")
                MatchStatsOnline = string.Format("http://lol.moa.tw/summoner/show/{0}#tabs-recentgame2", statistics.SummonerName.Replace(" ", "_"));
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
                    var UriSource = new System.Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName((int)summary.Spell1Id)), UriKind.Absolute);
                    playerStats.Spell1Image.Source = new BitmapImage(UriSource);
                }
                else
                    Client.Log(SummonerSpell.GetSpellImageName((int)summary.Spell1Id) + " is missing");
                if (File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName((int)summary.Spell2Id))))
                {
                    var UriSource = new System.Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName((int)summary.Spell2Id)), UriKind.Absolute);
                    playerStats.Spell2Image.Source = new BitmapImage(UriSource);
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
                            var UriSource = new System.Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "item", stat.Value + ".png"), UriKind.Absolute);
                            item.Source = new BitmapImage(UriSource);
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
                    new System.Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", skin.splashPath),
                        UriKind.Absolute);
                SkinImage.Source = new BitmapImage(skinSource);
            }
            catch (Exception)
            {
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            newRoom.LeaveRoom(new Jid(RoomJid), Client.LoginPacket.AllSummonerData.Summoner.Name);
            Client.OverlayContainer.Visibility = Visibility.Hidden;
            Client.ClearPage(typeof(EndOfGamePage));
        }

        private void OnlineHistory_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(MatchStatsOnline);
        }
    }
}
