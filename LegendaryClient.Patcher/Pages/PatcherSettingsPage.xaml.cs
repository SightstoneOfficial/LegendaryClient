#region

#endregion

namespace LegendaryClient.Patcher.Pages
{
    /// <summary>
    ///     Interaction logic for PatcherSettings.xaml
    /// </summary>
    public partial class PatcherSettingsPage
    {
        public PatcherSettingsPage(bool newsettings = false)
        {
            InitializeComponent();
            if (Properties.Settings.Default.UseGithub)
                Version.SelectedItem = Github;
            else
                Version.SelectedItem = Appveyor;
        }

        private void Version_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Version.SelectedItem == Github)
                Properties.Settings.Default.UseGithub = true;
            else
                Properties.Settings.Default.UseGithub = false;
        }
    }
}