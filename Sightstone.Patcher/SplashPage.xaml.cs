using Sightstone.Patcher.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Sightstone.Patcher
{
    /// <summary>
    /// Interaction logic for SplashPage.xaml
    /// </summary>
    public partial class SplashPage
    {
        public SplashPage()
        {
            InitializeComponent();
            Client.SplashPage = this;
            var text = new List<string>
            {
                "Starting up",
                "Warming up",
                "Looking for important files",
                "Detecting files",
                "Prepping the installer",
                "Almost ready",
                "Almost there",
                "Cleaning up dirty startup work"
            };
            var timer = new System.Timers.Timer { Interval = 2500 };
            timer.Start();
            var count = 0;
            timer.Elapsed += (obj, sender) =>
            {
                count++;
                Debugger.Log(count, count.ToString(), count + Environment.NewLine);
                if (count < 8)
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() => Current.Content = text[count]));
            };
        }
    }
}
