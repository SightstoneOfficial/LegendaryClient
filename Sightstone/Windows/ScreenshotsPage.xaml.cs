using Sightstone.Controls;
using Sightstone.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private UserClient user;

        public ScreenshotsPage()
        {
            InitializeComponent();
            Load();
        }

        private void DevButton_Click(object sender, RoutedEventArgs e)
        {
            if (!user.Dev) return;
            Client.OverlayContainer.Content = new ScreenshotViewer().Content;
            Client.OverlayContainer.Visibility = Visibility.Visible;
        }

        private void Load()
        {
            user = (UserList.Users[Client.CurrentServer])[Client.CurrentUser];
            if(user.Garena)
            {
                ScreenshotFolder = Path.Combine(Client.Location, "screenshots");
                ScreenshotsPath.Text = ScreenshotFolder;
                if(!user.Dev)
                {
                    DevButton.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                // TODO : Add logic for non-Garena servers.
            }
            // TODO : Add control for each screenshot
        }

        private void OpenScreenshotsPathButton_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(ScreenshotFolder))
            {
                Process.Start(ScreenshotFolder);
            }
            else
            {
                var overlay = new MessageOverlay { MessageTitle = { Content = "Error" }, MessageTextBox = { Text = ScreenshotFolder + " does not exist." } };
                Client.OverlayContainer.Content = overlay.Content;
                Client.OverlayContainer.Visibility = Visibility.Visible;
            }
        }
    }
}