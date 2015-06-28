using LegendaryClient.Logic;
using LegendaryClient.Properties;
using MahApps.Metro;
using System;
using System.Linq;
using System.Threading;
using System.Windows;

namespace LegendaryClient
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Any())
            {
                if (e.Args[0] == "EnableGarena=true")
                    Client.Garena = true;
                Client.args = e.Args;
                new MainWindow().Show();
            }
            else
                new MainWindow().Show();
        }
    }
}