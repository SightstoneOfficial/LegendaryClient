using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using LegendaryClient.Logic.Riot;
using Timer = System.Timers.Timer;
using LegendaryClient.Logic.MultiUser;

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
        static UserClient UserClient = new UserClient();

        public NotificationPage()
        {
            InitializeComponent();
            if (Client.Current != string.Empty)
                UserClient = UserList.users[Client.Current];
            LoadTimer();
            UpdateData();
        }

        private void LoadTimer()
        {
            timer.Interval = 10000; //10 second int
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }

#pragma warning disable 4014

        private void UpdateData()
        {
            foreach (InviteInfo data in UserClient.InviteData.Select(info => info.Value))
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
                        UserClient.calls.AcceptInvite(data1.stats.InvitationId);
                        UserClient.InviteData.Remove(data1.stats.InvitationId);
                    };
                    notification.Decline.Click += (s, e) =>
                    {
                        UserClient.calls.DeclineInvite(data2.stats.InvitationId);
                        UserClient.InviteData.Remove(data2.stats.InvitationId);
                    };
                    notification.TitleLabel.Content = "Game Invite";
                    string _name;
                    if (data2.stats.Inviter != null)
                        _name = data2.stats.Inviter.SummonerName;
                    else
                        _name = data2.stats.Owner.SummonerName;
                    notification.BodyTextbox.Text = _name + " has invited you to a game" + Environment.NewLine;

                    var m = JsonConvert.DeserializeObject<invitationRequest>(data2.stats.GameMetaData);

                    string mapName;

                    CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                    TextInfo textInfo = cultureInfo.TextInfo;
                    string gameModeLower = textInfo.ToTitleCase(string.Format(m.gameMode.ToLower()));
                    string gameTypeLower = textInfo.ToTitleCase(string.Format(m.gameType.ToLower()));
                    string gameTypeRemove = gameTypeLower.Replace("_game", "");
                    string removeAllUnder = gameTypeRemove.Replace("_", " ");

                    notification.BodyTextbox.Text += "Mode: " + gameModeLower + Environment.NewLine;
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
                    notification.BodyTextbox.Text += "Map: " + mapName + Environment.NewLine;
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
            UserClient.InviteData.Clear();
            ChatListView.Items.Clear();
        }
    }
}