using System.Linq;
using System.Windows;
using System.Windows.Input;
using Sightstone.Logic;
using Sightstone.Logic.MultiUser;

namespace Sightstone.Controls
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
                foreach (var g in Client.Groups.Where(g => g.groupName == (string) NameLabel.Content))
                    g.isOpen = true;
            }
            else
            {
                ExpandLabel.Content = "+";
                GroupListView.Visibility = Visibility.Collapsed;
                foreach (var g in Client.Groups.Where(g => g.groupName == (string) NameLabel.Content))
                    g.isOpen = false;
            }
        }
    }
}