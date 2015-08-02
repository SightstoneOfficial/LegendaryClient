using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;
using Sightstone.Logic;
using Sightstone.Logic.MultiUser;

namespace Sightstone.Windows
{
    /// <summary>
    ///     Interaction logic for ScreenshotViewer.xaml
    /// </summary>
    public partial class ScreenshotViewer
    {
        private List<string> screenshots;
        private string current;
        private int index;

        public ScreenshotViewer(List<string> screenshots, string current = null)
        {
            InitializeComponent();
            this.screenshots = screenshots;
            if (current != null)
                this.current = current;
            else
                this.current = screenshots[0];
            Load();
        }

        private void Load()
        {
            CheckButtonAvailability();
            ScreenshotImage.Source = new BitmapImage(new Uri(current));
            index = screenshots.FindIndex(o => o.Equals(current));
        }

        private void CheckButtonAvailability()
        {
            if (current == screenshots[0]) PrevScreenshotButton.IsEnabled = false;
            if (current == screenshots[screenshots.Count - 1]) NextScreenshotButton.IsEnabled = false;
        }

        private void UpdateImage()
        {
            current = screenshots[index];
            ScreenshotImage.Source = new BitmapImage(new Uri(current));
        }

        private void Previous()
        {
            index--;
            UpdateImage();
            CheckButtonAvailability();
        }

        private void Next()
        {
            index++;
            UpdateImage();
            CheckButtonAvailability();
        }
        
        private void Delete()
        {
            // TODO : Implement screenshot deletion
        }

        private void Explore()
        {
            Process.Start("explorer.exe", "/select, \"" + current +"\"");
        }

        #region Control Events
        private void CloseScreenshotsVierwerButton_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Visibility = Visibility.Hidden;
        }

        private void DeleteScreenshotButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OpenScreenshotPathButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PrevScreenshotButton_Click(object sender, RoutedEventArgs e)
        {
            Previous();
        }

        private void NextScreenshotButton_Click(object sender, RoutedEventArgs e)
        {
            Next();
        }

        #endregion
    }
}