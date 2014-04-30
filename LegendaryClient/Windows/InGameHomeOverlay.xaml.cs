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

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for InGameHomeOverlay.xaml
    /// </summary>
    public partial class InGameHomeOverlay : Page
    {
        public InGameHomeOverlay()
        {
            InitializeComponent();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Client.SwitchPage(new InGame());
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            Client.SwitchPage(new MainPage());
        }
    }
}
