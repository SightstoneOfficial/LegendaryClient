using Sightstone.Controls;
using Sightstone.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Sightstone.Logic.Riot;
using Sightstone.Logic.Riot.Platform;
using RtmpSharp.IO;
using RtmpSharp.Messaging;
using Sightstone.Logic.MultiUser;

namespace Sightstone.Windows
{
    /// <summary>
    ///     Interaction logic for ScreenshotsPage.xaml
    /// </summary>
    public partial class ScreenshotsPage
    {
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollviewer = sender as ScrollViewer;
            if (e.Delta > 0)
                scrollviewer.LineLeft();
            else
                scrollviewer.LineRight();
            e.Handled = true;
        }

        private string ScreenshotFolder;

        public ScreenshotsPage()
        {
            InitializeComponent();
        }

        private void Load()
        {
            UserClient user = (UserList.Users[Client.CurrentServer])[Client.CurrentUser];
            if(user.Garena)
            {
                ScreenshotFolder = Path.Combine(Client.Location, "screenshots");
            }
            else
            {
                // TODO : Add logic for non-Garena servers.
            }
            // TODO : Add control for each screenshot
        }
    }
}