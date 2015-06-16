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
            if (newsettings)
                SettingsLabel.Content = "Please configure your settings before using LegendaryClient.Patcher!";

            if (Properties.Settings.Default.UseGithub)
                Version.SelectedItem = Github;
            else
                Version.SelectedItem = Appveyor;

            if (Properties.Settings.Default.OnlyLOL)
                UpdateSettings.SelectedItem = OnlyLoL;
            else
                UpdateSettings.SelectedItem = LegendaryClient;

            LOLP2P.IsChecked = Properties.Settings.Default.LOLP2P;
            LCP2P.IsChecked = Properties.Settings.Default.LCP2P;
            LCPP2P.IsChecked = Properties.Settings.Default.LCPP2P;
            AlwaysUpdate.IsChecked = Properties.Settings.Default.AlwaysUpdate;
            PatcherVolume.Value = Properties.Settings.Default.Volume;
        }

        private void Version_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Version.SelectedItem == Github)
                Properties.Settings.Default.UseGithub = true;
            else
                Properties.Settings.Default.UseGithub = false;
            Properties.Settings.Default.Save();
        }

        private void Setting_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (UpdateSettings.SelectedItem == OnlyLoL)
                Properties.Settings.Default.OnlyLOL = true;
            else
                Properties.Settings.Default.OnlyLOL = false;
            Properties.Settings.Default.Save();
        }

        private void Slider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            Properties.Settings.Default.Volume = (int)PatcherVolume.Value;
            Properties.Settings.Default.Save();
        }

        private void LOLP2P_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            Properties.Settings.Default.LOLP2P = (bool)LOLP2P.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void LCP2P_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            Properties.Settings.Default.LCP2P = (bool)LCP2P.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void LCPP2P_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            Properties.Settings.Default.LCPP2P = (bool)LCPP2P.IsChecked;
            Properties.Settings.Default.Save();
        }
    }
}