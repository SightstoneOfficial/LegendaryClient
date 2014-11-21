using jabber.connection;
using jabber.protocol.client;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.PlayerSpell;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect.RiotObjects.Platform.Statistics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for EndOfGamePage.xaml
    /// </summary>
    public partial class EndOfGamePage : Page
    {
        private Room newRoom;

        public EndOfGamePage(EndOfGameStats Statistics)
        {
            InitializeComponent();
            RenderStats(Statistics);
            Client.SwitchPage(new MainPage());
            Client.runonce = false;

            string ObfuscatedName = Client.GetObfuscatedChatroomName(Statistics.RoomName, ChatPrefixes.Post_Game);
            string JID = Client.GetChatroomJID(Statistics.RoomName, Statistics.RoomPassword, false);
            newRoom = Client.ConfManager.GetRoom(new jabber.JID(JID));
            newRoom.Nickname = Client.LoginPacket.AllSummonerData.Summoner.Name;
            newRoom.OnRoomMessage += newRoom_OnRoomMessage;
            newRoom.OnParticipantJoin += newRoom_OnParticipantJoin;
            newRoom.Join(Statistics.RoomPassword);
        }

        private void newRoom_OnParticipantJoin(Room room, RoomParticipant participant)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = participant.Nick + " joined the room." + Environment.NewLine;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                ChatText.ScrollToEnd();
            }));
        }

        private void newRoom_OnRoomMessage(object sender, Message msg)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {

                if (msg.Body != "This room is not anonymous")
                {
                    TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                    tr.Text = msg.From.Resource + ": ";
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);
                    tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                    if (Client.Filter)
                        tr.Text = msg.InnerText.Replace("<![CDATA[", "").Replace("]]>", "").Filter() + Environment.NewLine;
                    else
                        tr.Text = msg.InnerText.Replace("<![CDATA[", "").Replace("]]>", "") + Environment.NewLine;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                    ChatText.ScrollToEnd();
                }
            }));
        }

        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
            tr.Text = Client.LoginPacket.AllSummonerData.Summoner.Name + ": ";
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
            tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
            if (Client.Filter)
                tr.Text = ChatTextBox.Text.Filter() + Environment.NewLine;
            else
                tr.Text = ChatTextBox.Text + Environment.NewLine;
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
            if (String.IsNullOrEmpty(ChatTextBox.Text))
                return;
            newRoom.PublicMessage(ChatTextBox.Text);
            ChatTextBox.Text = "";
            ChatText.ScrollToEnd();
        }

        private void RenderStats(EndOfGameStats Statistics)
        {
            TimeSpan t = TimeSpan.FromSeconds(Statistics.GameLength);
            TimeLabel.Content = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
            ModeLabel.Content = Statistics.GameMode;
            TypeLabel.Content = Statistics.GameType;

            GainedIP.Content = "+" + Statistics.IpEarned + " IP";
            TotalIP.Content = Statistics.IpTotal.ToString().Replace(".0", "") + " IP Total";

            GainedXP.Content = Statistics.ExperienceEarned;

            List<PlayerParticipantStatsSummary> AllParticipants = new List<PlayerParticipantStatsSummary>(Statistics.TeamPlayerParticipantStats.ToArray());
            AllParticipants.AddRange(Statistics.OtherTeamPlayerParticipantStats);

            foreach (PlayerParticipantStatsSummary summary in AllParticipants)
            {
                EndOfGamePlayer playerStats = new EndOfGamePlayer();
                champions Champ = champions.GetChampion(summary.SkinName); //Misleading variable name
                playerStats.ChampImage.Source = Champ.icon;
                playerStats.ChampLabel.Content = Champ.name;
                playerStats.PlayerLabel.Content = summary.SummonerName;
                var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName((int)summary.Spell1Id)), UriKind.Absolute);
                playerStats.Spell1Image.Source = new BitmapImage(uriSource);
                uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName((int)summary.Spell2Id)), UriKind.Absolute);
                playerStats.Spell2Image.Source = new BitmapImage(uriSource);

                double ChampionsKilled = 0;
                double Assists = 0;
                double Deaths = 0;

                foreach (RawStatDTO stat in summary.Statistics)
                {
                    if (stat.StatTypeName.StartsWith("ITEM") && stat.Value != 0)
                    {
                        System.Windows.Controls.Image item = new System.Windows.Controls.Image();
                        uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "item", stat.Value + ".png"), UriKind.Absolute);
                        item.Source = new BitmapImage(uriSource);
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

            championSkins Skin = championSkins.GetSkin(Statistics.SkinIndex);
            try
            {
                var skinSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", Skin.splashPath), UriKind.Absolute);
                SkinImage.Source = new BitmapImage(skinSource);
            }
            catch
            {

            }
            
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            newRoom.Leave(null);
            Client.OverlayContainer.Visibility = Visibility.Hidden;
        }
    }
}