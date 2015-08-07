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
using System.Windows.Media.Imaging;
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
        public string[] Screenshots;
        private UserClient user;

        public ScreenshotsPage()
        {
            InitializeComponent();
            Load();
        }

        private void DevButton_Click(object sender, RoutedEventArgs e)
        {
            if (!user.Dev) return;
            //Client.OverlayContainer.Content = new ScreenshotViewer();
            Client.OverlayContainer.Visibility = Visibility.Visible;
        }

        private void Load()
        {
            user = (UserList.Users[Client.CurrentServer])[Client.CurrentUser];
            if(user.Garena)
                ScreenshotFolder = Path.Combine(Client.Location, "screenshots");
            else
                ScreenshotFolder = Path.Combine(Client.RootLocation, "Screenshots");

            if (!Directory.Exists(ScreenshotFolder))
            {
                Directory.CreateDirectory(ScreenshotFolder);
            }

            Screenshots = Directory.GetFiles(ScreenshotFolder);
            ScreenshotsPath.Text = ScreenshotFolder;
            if (!user.Dev)
            {
                DevButton.Visibility = Visibility.Hidden;
            }

            try
            {
                AddScreenshots();
            }catch(UnauthorizedAccessException e)
            {
                Client.Log(e);
                Client.Log("Unauthorized access to screenshot.");
                Client.ErrorOverlay("Sightstone can't access some files in your screenshot folder.");
            }catch(Exception e)
            {
                Client.Log(e);
                Client.Log("Can't render screenshot for some reason.");
                Client.ErrorOverlay("Sightstone encountered an unexpected error and is unable to view screenshots for you.");
            }
        }

        private void AddScreenshots()
        {
            SSGrid.Children.Clear();
            foreach (var file in Screenshots)
            {
                var image = new Screenshot(file, this);
                image.Source = Client.ImageSourceFromUri(new Uri(file));
                image.Stretch = Stretch.Uniform;
                image.Margin = new Thickness(10, 10, 0, 0);
                SSGrid.Children.Add(image);
            }
        }

        #region Control events
        private void OpenScreenshotsPathButton_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(ScreenshotFolder))
            {
                Process.Start(ScreenshotFolder);
            }
            else
            {
                Client.ErrorOverlay(ScreenshotFolder + " does not exist.");
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Visibility = Visibility.Hidden;
            Client.FullNotificationOverlayContainer.Visibility = Visibility.Hidden;
            Load();
        }
        #endregion
    }
}