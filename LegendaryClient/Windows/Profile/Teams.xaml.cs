using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Properties;
using PVPNetConnect.RiotObjects.Platform.Harassment;
using PVPNetConnect.RiotObjects.Platform.Statistics;
using PVPNetConnect.RiotObjects.Team.Dto;
using PVPNetConnect.RiotObjects.Team.Stats;
using PVPNetConnect;

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
            Change();
        }

        public void Change()
        {
            var themeAccent = new ResourceDictionary
            {
                Source = new Uri(Settings.Default.Theme)
            };
            Resources.MergedDictionaries.Add(themeAccent);
        }

        private void TabContainer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        public void Update(PVPNetConnect.RiotObjects.Team.Dto.PlayerDTO result)
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

        private void GamesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
