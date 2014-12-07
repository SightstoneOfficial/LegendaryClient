#region

using System.Windows;

#endregion

namespace LegendaryClient.Controls
{
    /// <summary>
    ///     Interaction logic for NotifyPlayerPopup.xaml
    /// </summary>
    public partial class NotifyPlayerPopup
    {
        public NotifyPlayerPopup(string title, string content)
        {
            InitializeComponent();
            NotificationTypeLabel.Content = title;
            NotificationTextBox.Text = content;
        }

        private void OkayButton_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
        }
    }
}