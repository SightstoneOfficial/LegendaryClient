#region

using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Media.Animation;
using Sightstone.Patcher.Logic;
using Sightstone.Patcher.Pages;

#endregion

namespace Sightstone.Patcher
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        PatcherPage patcherPage;
        public MainWindow()
        {
            InitializeComponent();
            InitializeLanguage();
            Client.MainHolder = Container;
            Visibility = Visibility.Hidden;
            var page = new SplashPage();
            page.Show();
            Client.ExecutingDirectory = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName;
            Client.Win = this;
            Player.Volume = Properties.Settings.Default.Volume / 100;
            Client.SoundPlayer = Player;
            var region = Properties.Settings.Default.RegionName;
            RegionLabel.Content = !string.IsNullOrEmpty(region) ? Client.GetDictText("ConnectedRegion").Replace("{REGION}", region) : Client.GetDictText("NoRegion");
            Client.RegionLabel = RegionLabel;
            Client.OverlayContainer = OverlayContainer;
            Client.OverlayGrid = OverlayGrid;
            patcherPage = new PatcherPage();
            OverlayContainer.Content = new PatcherSettingsPage(Properties.Settings.Default.FirstStart).Content;
            //It is easier to access the menu with this
#if Debug
            Properties.Settings.Default.FirstStart = false;
#endif
            var waitAnimation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5));
            waitAnimation.Completed += (o, e) => { Container.Content = patcherPage.Content; };
            Container.BeginAnimation(OpacityProperty, waitAnimation);
        }
        private static void InitializeLanguage()
        {
            switch (Thread.CurrentThread.CurrentCulture.ToString().Split('-')[0])
            {
                case "en":
                    Client.Dict.Source = new Uri("..\\Logic\\Languages\\English.xaml",
                                  UriKind.Relative);
                    break;
                default:
                    Client.Dict.Source = new Uri("..\\Logic\\Languages\\English.xaml",
                                      UriKind.Relative);
                    break;
            }
            Application.Current.Resources.MergedDictionaries.Add(Client.Dict);
        }


        //Contains a progress for the future
        public void SlideGrid(object sender, RoutedEventArgs e)
        {
            if (!patcherPage.DownloadStarted)
                patcherPage.Download();
            OverlayGrid.Visibility = OverlayGrid.Visibility == Visibility.Hidden
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            //Close pls
            Environment.Exit(0);
        }
    }
}