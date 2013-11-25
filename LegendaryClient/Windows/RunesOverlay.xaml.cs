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
using LegendaryClient.Logic;
using LegendaryClient.Windows.Profile;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for RunesOverlay.xaml
    /// </summary>
    public partial class RunesOverlay : Page
    {
        public RunesOverlay()
        {
            InitializeComponent();
            Container.Content = new Runes().Content;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Visibility = Visibility.Hidden;
        }
    }
}
