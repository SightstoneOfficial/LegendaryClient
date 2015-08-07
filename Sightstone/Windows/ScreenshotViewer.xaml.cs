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
        const string DatePattern = "dd MMMM yyyy, HH:mm";

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
            index = screenshots.FindIndex(o => o.Equals(current));
            UpdateImage();
        }

        private void CheckButtonAvailability()
        {
            if (current == screenshots[0]) PrevScreenshotButton.IsEnabled = false;
            else PrevScreenshotButton.IsEnabled = true;
            if (current == screenshots[screenshots.Count - 1]) NextScreenshotButton.IsEnabled = false;
            else NextScreenshotButton.IsEnabled = true;
        }

        private void UpdateImage()
        {
            current = screenshots[index];
            ScreenshotImage.Source = Client.ImageSourceFromUri(new Uri(current));
            ScreenshotPath.Text = current;
            DateCreatedLabel.Text = File.GetCreationTime(current).ToString(DatePattern);
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
            // TODO : Implement a user friendly screenshot deletion method
        }

        private void Explore()
        {
            Process.Start("explorer.exe", "/select, \"" + current +"\"");
        }

        private void Close()
        {
            var page = new ScreenshotsPage();
            Client.SwitchPage(page);
            Client.ClearPage(typeof(ScreenshotViewer));
        }

        #region Control Events
        private void CloseScreenshotsVierwerButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DeleteScreenshotButton_Click(object sender, RoutedEventArgs e)
        {
            Delete();
        }

        private void OpenScreenshotPathButton_Click(object sender, RoutedEventArgs e)
        {
            Explore();
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