#region



#endregion

using System.Windows;

namespace LegendaryClientMLaunch
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        //StartupUri="MainWindow.xaml"
        void App_Startup(object sender, StartupEventArgs e)
        {
            var mainWindow = new MainWindow(e);
            mainWindow.Show();
        }
    }
}