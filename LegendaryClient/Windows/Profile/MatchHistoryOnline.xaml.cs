using LeagueMatchHistory;
using LeagueMatchHistory.MatchHistory;
using LeagueMatchHistory.MatchHistory.Games;
using LeagueMatchHistory.MatchHistory.Games.Timeline;
using LegendaryClient.Logic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    /// Interaction logic for MatchHistoryOnline.xaml
    /// </summary>
    public partial class MatchHistoryOnline : Page
    {
        string SumName;
        public MatchHistoryOnline(String Name = "")
        {
            InitializeComponent();
            //Started work on
            if (String.IsNullOrEmpty(Name))
            {
                Name = Client.LoginPacket.AllSummonerData.Summoner.Name;
            }
            SumName = Name;
            GetData();
        }
        public async void GetData()
        {
            PlayerMatchHistory history = new PlayerMatchHistory();
            HistoryAccount acc = await history.GetPlayerAsync(Client.Region.InternalName, SumName);
            PlayerListGames games = await history.GetGamesAsync(acc);
            
            //This is the first game
            Game game = await history.GetFullGameDataAsync(games.games.games[0]);

            //This is the IndepthTimeline
            IndepthTimeline timeline = await history.GetGameTimelineAsync(games.games.games[0]);
        }
    }
}
