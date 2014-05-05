using LegendaryClient.Logic;
using LegendaryClient.Logic.AutoReplays;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for InGame.xaml
    /// </summary>
    public partial class InGame : Page
    {
        public InGame()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Client.LaunchGame();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Client.SwitchPage(new InGameHomeOverlay());
        }
    }
}
