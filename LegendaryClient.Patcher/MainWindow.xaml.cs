using LegendaryClient.Patcher.Logic;
using LegendaryClient.Patcher.Pages;
using MahApps.Metro.Controls;
using System;
using System.IO;
using System.Reflection;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace LegendaryClient.Patcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
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
            waitAnimation.Completed += (o, e) =>
            {
                Container.Content = new PatcherPage().Content;
            };
            Container.BeginAnimation(ContentControl.OpacityProperty, waitAnimation);
        }

        //Contains a progress for the future
        public void SlideGrid(object sender, RoutedEventArgs e)
        {
            if(OverlayGrid.Visibility == Visibility.Hidden)
            {
                OverlayGrid.Visibility = Visibility.Visible;
                /*
                Storyboard sb = new Storyboard();

                DoubleAnimation slide = new DoubleAnimation();
                slide.To = 3000.0;
                slide.From = 0;
                
                slide.Duration = new Duration(TimeSpan.FromMilliseconds(40.0));

                Storyboard.SetTarget(slide, OverlayGrid);
                Storyboard.SetTargetProperty(slide, new PropertyPath("RenderTransform.(TranslateTransform.X)"));
                
                sb.Children.Add(slide);
                sb.Begin();
                //*/
            }
            else
            {
                OverlayGrid.Visibility = Visibility.Hidden;

                /*
                Storyboard sb = new Storyboard();

                DoubleAnimation slide = new DoubleAnimation();
                slide.From = 3000.0;
                slide.To = 0;
                slide.Duration = new Duration(TimeSpan.FromMilliseconds(40.0));

                Storyboard.SetTarget(slide, OverlayGrid);
                Storyboard.SetTargetProperty(slide, new PropertyPath("RenderTransform.(TranslateTransform.X)"));
                
                sb.Children.Add(slide);
                sb.Begin();
                //*/
            }
        }
    }
}
