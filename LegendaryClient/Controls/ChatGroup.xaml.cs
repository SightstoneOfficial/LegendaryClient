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

namespace LegendaryClient.Controls
{
    /// <summary>
    /// Interaction logic for ChatGroup.xaml
    /// </summary>
    public partial class ChatGroup : UserControl
    {
        public ChatGroup()
        {
            InitializeComponent();
        }

        private void GroupGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (GroupListView.Visibility == System.Windows.Visibility.Collapsed)
            {
                ExpandLabel.Content = "-";
                GroupListView.Visibility = System.Windows.Visibility.Visible;
                foreach (Group g in Client.Groups)
                {
                    if (g.GroupName == (string)NameLabel.Content)
                        g.IsOpen = true;
                }
            }
            else
            {
                ExpandLabel.Content = "+";
                GroupListView.Visibility = System.Windows.Visibility.Collapsed;
                foreach (Group g in Client.Groups)
                {
                    if (g.GroupName == (string)NameLabel.Content)
                        g.IsOpen = false;
                }
            }
        }
    }
}
