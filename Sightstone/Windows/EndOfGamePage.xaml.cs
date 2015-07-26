using Sightstone.Controls;
using Sightstone.Logic;
using Sightstone.Logic.PlayerSpell;
using Sightstone.Logic.SQLite;
using Sightstone.Properties;
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
using Sightstone.Logic.Riot.Platform;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.x.muc;
using Sightstone.Logic.Riot.Platform.Messaging.Persistence;
using Newtonsoft.Json;
using Sightstone.Logic.JSON;
using agsXMPP.Collections;
using Sightstone.Logic.MultiUser;

namespace Sightstone.Windows
{
    /// <summary>
    ///     Interaction logic for EndOfGamePage.xaml
    /// </summary>
    public partial class EndOfGamePage
    {
        private readonly MucManager newRoom;
        private string MatchStatsOnline;
        private readonly string RoomJid;
        static UserClient UserClient = (UserList.Users[Client.CurrentServer])[Client.CurrentUser];
        
        public EndOfGamePage(EndOfGameStats statistics)
        {
            InitializeComponent();
            RenderStats(statistics);
            Client.SwitchPage(Client.MainPage);
            Client.runonce = false;
            UserClient.ChampId = -1;
            RoomJid = Client.GetChatroomJid(statistics.RoomName, statistics.RoomPassword, false);
            
            newRoom = new MucManager(UserClient.XmppConnection);
            UserClient.XmppConnection.OnMessage += XmppConnection_OnMessage;
            UserClient.XmppConnection.OnPresence += XmppConnection_OnPresence;
            UserClient.RiotConnection.MessageReceived += RiotConnection_MessageReceived;
            newRoom.AcceptDefaultConfiguration(new Jid(RoomJid));
            newRoom.JoinRoom(new Jid(RoomJid), UserClient.LoginPacket.AllSummonerData.Summoner.Name);
        }

        void RiotConnection_MessageReceived(object sender, RtmpSharp.Messaging.MessageReceivedEventArgs e)
        {
            if (e.Body is SimpleDialogMessage)
            {
                var item = e.Body as SimpleDialogMessage;
                if (item.Type == "championMastery")
                {
                    var mastery = JsonConvert.DeserializeObject<ChampionMastery>(item.Params.ToString());
                    GotChampionMasteryPoints(mastery);
                }
            }
        }

        private void GotChampionMasteryPoints(ChampionMastery item)
        {
            int gainedChampionMasteryPoints = item.championPointsUntilNextLevelAfterGame - item.championPointsUntilNextLevelBeforeGame;
            TotalChampionXP.Content = (item.championPointsBeforeGame + gainedChampionMasteryPoints).ToString() + "Total CP";
            GainedChampionXP.Content = "+" + gainedChampionMasteryPoints + " CP";
            NextLvlChampionXP.Content = item.championPointsUntilNextLevelAfterGame.ToString() + " to Next Lvl";
            ChampionMasteryGrid.Visibility = Visibility.Visible;
        }

        void XmppConnection_OnPresence(object sender, Presence pres)
        {
            if (pres.To.Bare != RoomJid)
                return;

            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                {
                    Text = pres.From.Resource + " joined the room." + Environment.NewLine
                };
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);

                ChatText.ScrollToEnd();
            }));
        }

        void XmppConnection_OnMessage(object sender, Message msg)
        {
            if (RoomJid.Contains(msg.From.User))
                return;

            if (msg.From.Resource == UserClient.LoginPacket.AllSummonerData.Summoner.Name)
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
                if (UserClient.Filter)
                    tr.Text = msg.Body.Replace("<![CDATA[", "").Replace("]]>", "").Filter() +
                              Environment.NewLine;
                else
                    tr.Text = msg.Body.Replace("<![CDATA[", "").Replace("]]>", "") + Environment.NewLine;

                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);

                ChatText.ScrollToEnd();
            }));
        }

        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
            {
                Text = UserClient.LoginPacket.AllSummonerData.Summoner.Name + ": "
            };
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
            tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
            if (UserClient.Filter)
                tr.Text = ChatTextBox.Text.Filter() + Environment.NewLine;
            else
                tr.Text = ChatTextBox.Text + Environment.NewLine;

            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
            if (string.IsNullOrEmpty(ChatTextBox.Text))
                return;

            UserClient.XmppConnection.Send(new Message(new Jid(RoomJid), MessageType.groupchat, ChatTextBox.Text));
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
            if (UserClient.Garena && !string.IsNullOrEmpty(Settings.Default.DefaultGarenaRegion) && Settings.Default.DefaultGarenaRegion == "TW")
                MatchStatsOnline = string.Format("http://lol.moa.tw/summoner/show/{0}#tabs-recentgame2", statistics.SummonerName.Replace(" ", "_"));
            else
                MatchStatsOnline = "http://matchhistory.na.leagueoflegends.com/en/#match-details/" + UserClient.Region.InternalName + "/" + statistics.ReportGameId + "/" + statistics.UserId;

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
                    if (summary.SummonerName == UserClient.LoginPacket.AllSummonerData.Summoner.Name)
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
                    if (UserClient.LoginPacket.AllSummonerData.SummonerLevel.Level < 30)
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
            UserClient.RiotConnection.MessageReceived -= RiotConnection_MessageReceived;
            newRoom.LeaveRoom(new Jid(RoomJid), UserClient.LoginPacket.AllSummonerData.Summoner.Name);
            Client.OverlayContainer.Visibility = Visibility.Hidden;
            Client.ClearPage(typeof(EndOfGamePage));
        }

        private void OnlineHistory_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(MatchStatsOnline);
        }
    }
}
