#region

using System.Linq;
using System.Windows;
using System.Windows.Input;
using LegendaryClient.Logic;

#endregion

namespace LegendaryClient.Controls
{
    /// <summary>
    ///     Interaction logic for ChatGroup.xaml
    /// </summary>
    public partial class ChatGroup
    {
        public ChatGroup()
        {
            InitializeComponent();
        }

        internal void GroupGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (GroupListView.Visibility == Visibility.Collapsed)
            {
                ExpandLabel.Content = "-";
                GroupListView.Visibility = Visibility.Visible;
                foreach (var g in Client.Groups.Where(g => g.GroupName == (string) NameLabel.Content))
                    g.IsOpen = true;
            }
            else
            {
                ExpandLabel.Content = "+";
                GroupListView.Visibility = Visibility.Collapsed;
                foreach (var g in Client.Groups.Where(g => g.GroupName == (string) NameLabel.Content))
                    g.IsOpen = false;
            }
        }
    }
}