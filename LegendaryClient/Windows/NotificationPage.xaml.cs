#region

using System.Globalization;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Properties;
using Newtonsoft.Json;
using Timer = System.Timers.Timer;

#endregion

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for NotificationPage.xaml
    /// </summary>
    public partial class NotificationPage
    {
        //ChatListView
        private const bool started = false;
        private readonly Timer timer = new Timer();

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
            bool x = Settings.Default.DarkTheme;
            if (x)
                TheGrid.Background = (Brush) bc.ConvertFrom("#E5000000");
            else
                TheGrid.Background = (Brush) bc.ConvertFrom("#E5B4B4B4");
        }

        private void LoadTimer()
        {
            timer.Interval = 10000; //10 second int
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }

        private void UpdateData()
        {
            foreach (InviteInfo data in Client.InviteData.Select(info => info.Value))
            {
                InviteInfo data2 = data;
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    ChatListView.Items.Clear();
                    var notification = new InvitesNotification();
                    InviteInfo data1 = data2;
                    notification.Accept.Click += (s, e) =>
                    {
                        Client.SwitchPage(new TeamQueuePage(data1.stats.InvitationId));
                        Client.PVPNet.Accept(data1.stats.InvitationId);
                        Client.InviteData.Remove(data1.stats.InvitationId);
                    };
                    notification.Decline.Click += (s, e) =>
                    {
                        Client.PVPNet.Decline(data2.stats.InvitationId);
                        Client.InviteData.Remove(data2.stats.InvitationId);
                    };
                    notification.TitleLabel.Content = "Game Invite";
                    notification.BodyTextbox.Text = data2.stats.Inviter + " has invited you to a game";

                    var m = JsonConvert.DeserializeObject<invitationRequest>(data2.stats.GameMetaData);

                    string mapName;

                    CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                    TextInfo textInfo = cultureInfo.TextInfo;
                    string gameModeLower = textInfo.ToTitleCase(string.Format(m.gameMode.ToLower()));
                    string gameTypeLower = textInfo.ToTitleCase(string.Format(m.gameType.ToLower()));
                    //Why do I have to do this Riot?
                    string gameTypeRemove = gameTypeLower.Replace("_game", "");
                    string removeAllUnder = gameTypeRemove.Replace("_", " ");

                    notification.BodyTextbox.Text += "Mode: " + gameModeLower;
                    switch (m.mapId)
                    {
                        case 1:
                            mapName = "Summoners Rift";
                            break;
                        case 10:
                            mapName = "The Twisted Treeline";
                            break;
                        case 12:
                            mapName = "Howling Abyss";
                            break;
                        case 8:
                            mapName = "The Crystal Scar";
                            break;
                        default:
                            mapName = "Unknown Map";
                            break;
                    }
                    notification.BodyTextbox.Text += "Map: " + mapName;
                    notification.BodyTextbox.Text += "Type: " + removeAllUnder;
                    ChatListView.Items.Add(notification);
                }));
            }
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
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