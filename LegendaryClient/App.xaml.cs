using LegendaryClient.Logic;
using System.Linq;
using System.Threading;

namespace LegendaryClient
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private void Application_Startup(object sender, System.Windows.StartupEventArgs e)
        {
            Thread.Sleep(100);
            if (e.Args.Any())
            {
                if (e.Args[0] == "EnableGarena=true")
                    Client.Garena = true;
                Client.args = e.Args;
                new MainWindow(e).Show();
            }
            else
                new MainWindow(e).Show();
        }
    }
}