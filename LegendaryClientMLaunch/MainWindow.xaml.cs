#region



#endregion

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
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}