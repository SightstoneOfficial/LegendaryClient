using LegendaryClient.Logic;
using MahApps.Metro.Controls;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Summoner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LegendaryClient
{
    /// <summary>
    /// Interaction logic for GameScouter.xaml
    /// </summary>
    public partial class GameScouter : MetroWindow
    {
        public GameScouter()
        {
            InitializeComponent();
            Client.win = this;
            LoadScouter();
        }
        public static void LoadScouter(string User = null)
        {
            if (String.IsNullOrEmpty(User))
                User = Client.LoginPacket.AllSummonerData.Summoner.Name;
            if (Convert.ToBoolean(IsUserValid(User)))
                LoadStats(User);
            else
            {
                Client.win.Visibility = Visibility.Hidden;
                Client.win.Close();
            }
        }
        private static async Task<bool> IsUserValid(string Username)
        {
            PublicSummoner sum = await Client.PVPNet.GetSummonerByName(Username);
            if (String.IsNullOrEmpty(sum.Name))
                return false;
            else
                return true;
        }

        private static async void LoadStats(string user)
        {
            PlatformGameLifecycleDTO n = await Client.PVPNet.RetrieveInProgressSpectatorGameInfo(user);
            if (n.GameName != null)
            {

            }
            else
            {
                Client.win.Visibility = Visibility.Hidden;
                Client.win.Close();
            }
        }
    }
}
