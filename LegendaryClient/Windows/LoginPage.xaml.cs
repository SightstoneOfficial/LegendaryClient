using jabber.client;
using jabber.connection;
using LegendaryClient.Logic;
using LegendaryClient.Logic.JSON;
using LegendaryClient.Logic.Region;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Logic.SWF;
using LegendaryClient.Logic.SWF.SWFTypes;
using PVPNetConnect.RiotObjects.Platform.Clientfacade.Domain;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();

            //Get client data after patcher completed
            Client.SQLiteDatabase = new SQLite.SQLiteConnection("gameStats_en_US.sqlite");
            Client.Champions = (from s in Client.SQLiteDatabase.Table<champions>()
                                orderby s.name
                                select s).ToList();
            foreach (champions c in Client.Champions)
            {
                var Source = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", c.iconPath), UriKind.Absolute);
                c.icon = new BitmapImage(Source);
            }
            Client.ChampionSkins = (from s in Client.SQLiteDatabase.Table<championSkins>()
                                    orderby s.name
                                    select s).ToList();
            Client.ChampionAbilities = (from s in Client.SQLiteDatabase.Table<championAbilities>()
                                        orderby s.name
                                        select s).ToList();
            Client.SearchTags = (from s in Client.SQLiteDatabase.Table<championSearchTags>()
                                 orderby s.id
                                 select s).ToList();
            Client.Keybinds = (from s in Client.SQLiteDatabase.Table<keybindingEvents>()
                               orderby s.id
                               select s).ToList();
            Client.Items = Items.PopulateItems();

            //Retrieve latest client version
            SWFReader reader = new SWFReader("ClientLibCommon.dat");
            foreach (Tag tag in reader.Tags)
            {
                if (tag is DoABC)
                {
                    DoABC abcTag = (DoABC)tag;
                    if (abcTag.Name.Contains("riotgames/platform/gameclient/application/Version"))
                    {
                        var str = System.Text.Encoding.Default.GetString(abcTag.ABCData);
                        //Ugly hack ahead - turn back now! (http://pastebin.com/yz1X4HBg)
                        string[] firstSplit = str.Split((char)6);
                        string[] secondSplit = firstSplit[0].Split((char)19);
                        Client.Version = secondSplit[1];
                    }
                }
            }

            if (!String.IsNullOrWhiteSpace(Properties.Settings.Default.SavedUsername))
            {
                RememberUsernameCheckbox.IsChecked = true;
                LoginUsernameBox.Text = Properties.Settings.Default.SavedUsername;
            }
            if (!String.IsNullOrWhiteSpace(Properties.Settings.Default.SavedPassword))
            {
                RememberPasswordCheckbox.IsChecked = true;
                LoginPasswordBox.Password = Properties.Settings.Default.SavedPassword;
            }
            if (!String.IsNullOrWhiteSpace(Properties.Settings.Default.Region))
            {
                RegionComboBox.SelectedValue = Properties.Settings.Default.Region;
            }
            var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champions.GetChampion(Client.LatestChamp).splashPath), UriKind.Absolute);
            LoginImage.Source = new BitmapImage(uriSource);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (RememberPasswordCheckbox.IsChecked == true)
                Properties.Settings.Default.SavedPassword = LoginPasswordBox.Password;
            else
                Properties.Settings.Default.SavedPassword = "";

            if (RememberUsernameCheckbox.IsChecked == true)
                Properties.Settings.Default.SavedUsername = LoginUsernameBox.Text;
            else
                Properties.Settings.Default.SavedUsername = "";
            Properties.Settings.Default.Region = (string)RegionComboBox.SelectedValue;
            Properties.Settings.Default.Save();

            HideGrid.Visibility = Visibility.Hidden;
            ErrorTextBox.Visibility = Visibility.Hidden;
            LoggingInLabel.Visibility = Visibility.Visible;
            LoggingInProgressRing.Visibility = Visibility.Visible;
            Client.PVPNet.OnError += PVPNet_OnError;
            Client.PVPNet.OnLogin += PVPNet_OnLogin;
            Client.PVPNet.OnMessageReceived += Client.OnMessageReceived;
            BaseRegion SelectedRegion = BaseRegion.GetRegion((string)RegionComboBox.SelectedValue);
            Client.Region = SelectedRegion;
            Client.PVPNet.Connect(LoginUsernameBox.Text, LoginPasswordBox.Password, SelectedRegion.PVPRegion, Client.Version);
        }

        private void PVPNet_OnLogin(object sender, string username, string ipAddress)
        {
            Client.PVPNet.GetLoginDataPacketForUser(new LoginDataPacket.Callback(GotLoginPacket));
        }

        private void PVPNet_OnError(object sender, PVPNetConnect.Error error)
        {
            //Display error message
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                HideGrid.Visibility = Visibility.Visible;
                ErrorTextBox.Visibility = Visibility.Visible;
                LoggingInProgressRing.Visibility = Visibility.Hidden;
                LoggingInLabel.Visibility = Visibility.Hidden;
                ErrorTextBox.Text = error.Message;
            }));
        }

        #pragma warning disable 4014 //Code does not need to be awaited
        private void GotLoginPacket(LoginDataPacket packet)
        {
            Client.LoginPacket = packet;
            Client.PVPNet.OnError -= PVPNet_OnError;
            Client.GameConfigs = packet.GameTypeConfigs;
            Client.PVPNet.Subscribe("bc", packet.AllSummonerData.Summoner.AcctId);
            Client.PVPNet.Subscribe("cn", packet.AllSummonerData.Summoner.AcctId);
            Client.PVPNet.Subscribe("gn", packet.AllSummonerData.Summoner.AcctId);
            Client.IsLoggedIn = true;

            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                Client.NotificationContainer.Visibility = System.Windows.Visibility.Visible;
                Client.Container.Margin = new Thickness(0, 0, 0, 50);

                Client.ChatClient = new JabberClient(); //Setup chat
                Client.ChatClient.AutoReconnect = 30;
                Client.ChatClient.KeepAlive = 10;
                Client.ChatClient.NetworkHost = "chat." + Client.Region.ChatName + ".lol.riotgames.com";
                Client.ChatClient.Port = 5223;
                Client.ChatClient.Server = "pvp.net";
                Client.ChatClient.SSL = true;
                Client.ChatClient.User = LoginUsernameBox.Text;
                Client.ChatClient.Password = "AIR_" + LoginPasswordBox.Password;
                Client.ChatClient.OnInvalidCertificate += Client.ChatClient_OnInvalidCertificate;
                Client.ChatClient.OnMessage += Client.ChatClient_OnMessage;
                Client.ChatClient.Connect();

                Client.RostManager = new RosterManager();
                Client.RostManager.Stream = Client.ChatClient;
                Client.RostManager.AutoSubscribe = true;
                Client.RostManager.AutoAllow = jabber.client.AutoSubscriptionHanding.AllowAll;
                Client.RostManager.OnRosterItem += Client.RostManager_OnRosterItem;
                Client.RostManager.OnRosterEnd += new bedrock.ObjectHandler(Client.ChatClientConnect);

                Client.PresManager = new PresenceManager();
                Client.PresManager.Stream = Client.ChatClient;
                Client.PresManager.OnPrimarySessionChange += Client.PresManager_OnPrimarySessionChange;

                Client.ConfManager = new ConferenceManager();
                Client.ConfManager.Stream = Client.ChatClient;

                Client.SwitchPage(new MainPage());
                Client.ClearPage(this);
            }));
        }
    }
}