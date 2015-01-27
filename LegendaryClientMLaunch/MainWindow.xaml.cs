#region



#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace LegendaryClientMLaunch
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    ///     Used to show options that users can use (replays without login)
    ///     * Also allows users to change key bindings, sound volume + more without login
    ///     * Alow Garnea Users to use LegendaryClient
    ///     * Designed not to be in standard installs (Little UI thought, UNORGANIZED CODE).
    ///     * Only certain users should need this (Garnea = Required).
    ///     ONLY IN ADVANCED INSTALLS. DO NOT ATTACH IN NORMAL BINARY
    /// </summary>
    public partial class MainWindow
    {
        private string arg;
        public MainWindow(StartupEventArgs e)
        {
            InitializeComponent();
            if (e.Args.Any())
                arg = e.Args[e.Args.Count() - 1];
            var option = new Option { Label = { Content = "Launch LegendaryClient (Garena Enabled)" } };
            Options.Items.Add(
                e.Args.Count() > 1
                    ? option
                    : new Option { Label = { Content = "Launch LegendaryClient" } });
            Options.SelectedItem = option;
            Options.Items.Add(new Option { Label = { Content = "View and watch replays (DISABLED)" } });
            regionComboBox.ItemsSource = new[] { "PH", "SG", "SGMY", "TH", "TW", "VN" };
            regionComboBox.SelectedItem = "PH";
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("LegendaryClient");
            if (key == null)
                return;
            var fileLocation = (string)key.GetValue("LCLocation");

            var start = new ProcessStartInfo
            {
                FileName = Path.Combine(fileLocation, "Client", "LegendaryClient.exe"),
                Arguments = "\"EnableGarena=true\" \"" + regionComboBox.SelectedItem + "\" \"" + arg + "\""
            };
            Process.Start(start);
            key.Close();
        }
    }
}