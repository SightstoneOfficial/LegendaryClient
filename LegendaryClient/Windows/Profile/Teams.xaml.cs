using PVPNetConnect;
using PVPNetConnect.RiotObjects.Team.Dto;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    /// Interaction logic for Teams.xaml
    /// </summary>
    public partial class Teams
    {
        public Teams()
        {
            InitializeComponent();
        }

        public void Update(PlayerDTO result)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                Team2.Visibility = Visibility.Collapsed;
                Team3.Visibility = Visibility.Collapsed;
                Team4.Visibility = Visibility.Collapsed;
                Team5.Visibility = Visibility.Collapsed;
                int i = 0;
                foreach (var item in result.TeamsSummary)
                {
                    
                    
                    TeamDTO team = new TeamDTO((TypedObject)item);
                    switch (i)
                    {
                        case 0: Team1.Header = team.Name;
                            break;
                        case 1: Team2.Header = team.Name;
                            Team2.Visibility = Visibility.Visible;
                            break;
                        case 2: Team3.Header = team.Name;
                            Team3.Visibility = Visibility.Visible;
                            break;
                        case 3: Team4.Header = team.Name;
                            Team4.Visibility = Visibility.Visible;
                            break;
                        case 4: Team5.Header = team.Name;
                            Team5.Visibility = Visibility.Visible;
                            break;
                    }
                    i++;
                }

            }));
        }
    }
}
