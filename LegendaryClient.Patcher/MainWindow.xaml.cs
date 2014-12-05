#region

using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Animation;
using LegendaryClient.Patcher.Logic;
using LegendaryClient.Patcher.Pages;

#endregion

namespace LegendaryClient.Patcher
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Client.ExecutingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Client.MainHolder = Container;
            Client.Win = this;

            Client.OverlayContainer = OverlayContainer;
            Client.OverlayGrid = OverlayGrid;
            OverlayContainer.Content = new PatcherSettingsPage().Content;


            //Wait half a second before starting, makes it look sleek. This is a hack tho (but it's pretty!)
            var waitAnimation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5));
            waitAnimation.Completed += (o, e) => { Container.Content = new PatcherPage().Content; };
            Container.BeginAnimation(OpacityProperty, waitAnimation);
        }

        //Contains a progress for the future
        public void SlideGrid(object sender, RoutedEventArgs e)
        {
            OverlayGrid.Visibility = OverlayGrid.Visibility == Visibility.Hidden
                ? Visibility.Visible
                : Visibility.Hidden;
        }
    }
}