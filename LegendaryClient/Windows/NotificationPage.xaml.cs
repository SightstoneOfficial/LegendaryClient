using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for NotificationPage.xaml
    /// </summary>
    public partial class NotificationPage : Page
    {
        //ChatListView
        private System.Timers.Timer timer = new System.Timers.Timer();
        private bool started = false;
        public NotificationPage()
        {
            InitializeComponent();
            LoadTimer();
            UpdateData();
            Change();
        }
        public void Change()
        {
            var bc = new BrushConverter();
            bool x = Properties.Settings.Default.DarkTheme;
            if (x)
                TheGrid.Background = (Brush)bc.ConvertFrom("#E5000000");
            else
                TheGrid.Background = (Brush)bc.ConvertFrom("#E5B4B4B4");
        }
        private void LoadTimer()
        {
            if (started)
                return;
            timer.Interval = 10000; //10 second int
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }
        private void UpdateData()
        {
            foreach (KeyValuePair<String, InviteInfo> info in Client.InviteData)
            {
                InviteInfo data = info.Value;
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    ChatListView.Items.Clear();
                    InvitesNotification notification = new InvitesNotification();
                    notification.Accept.Click += (s, e) => { Client.PVPNet.Accept(data.stats.InvitationId); Client.InviteData.Remove(data.stats.InvitationId); Client.SwitchPage(new TeamQueuePage(data.stats.InvitationId)); };
                    notification.Decline.Click += (s, e) => { Client.PVPNet.Decline(data.stats.InvitationId); Client.InviteData.Remove(data.stats.InvitationId); };
                    notification.TitleLabel.Content = "Game Invite";
                    notification.BodyTextbox.Text = data.stats.Inviter + " has invited you to a game";

                    invitationRequest m = JsonConvert.DeserializeObject<invitationRequest>(data.stats.GameMetaData);

                    string MapName;

                    CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                    TextInfo textInfo = cultureInfo.TextInfo;
                    var gameModeLower = textInfo.ToTitleCase(string.Format(m.gameMode.ToLower()));
                    var gameTypeLower = textInfo.ToTitleCase(string.Format(m.gameType.ToLower()));
                    //Why do I have to do this Riot?
                    var gameTypeRemove = gameTypeLower.Replace("_game", "");
                    var removeAllUnder = gameTypeRemove.Replace("_", " ");

                    notification.BodyTextbox.Text += "Mode: " + gameModeLower;
                    if (m.mapId == 1)
                        MapName = "Summoners Rift";
                    else if (m.mapId == 10)
                        MapName = "The Twisted Treeline";
                    else if (m.mapId == 12)
                        MapName = "Howling Abyss";
                    else if (m.mapId == 8)
                        MapName = "The Crystal Scar";
                    else
                        MapName = "Unknown Map";
                    notification.BodyTextbox.Text += "Map: " + MapName;
                    notification.BodyTextbox.Text += "Type: " + removeAllUnder;
                    ChatListView.Items.Add(notification);
                }));
            }
        }
        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            UpdateData();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Client.InviteData.Clear();
            ChatListView.Items.Clear();
        }
    }
}
