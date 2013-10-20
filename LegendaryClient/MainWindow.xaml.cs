using LegendaryClient.Forms;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LegendaryClient.Windows;
using LegendaryClient.Logic;
using PVPNetConnect;
using System.Runtime.InteropServices;

namespace LegendaryClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            #region Initialize Custom Window
            TitleBar.MouseLeftButtonDown += (o, e) => DragMove();
            new WindowResizer(this,
                new WindowBorder(BorderPosition.TopLeft, topLeft),
                new WindowBorder(BorderPosition.Top, top),
                new WindowBorder(BorderPosition.TopRight, topRight),
                new WindowBorder(BorderPosition.Right, right),
                new WindowBorder(BorderPosition.BottomRight, bottomRight),
                new WindowBorder(BorderPosition.Bottom, bottom),
                new WindowBorder(BorderPosition.BottomLeft, bottomLeft),
                new WindowBorder(BorderPosition.Left, left));
            Client.EnableButtons = new List<Button>();

            Client.EnableButtons.Add(LogoutButton);
            Client.EnableButtons.Add(PlayButton);
            Client.EnableButtons.Add(ProfileButton);
            Client.EnableButtons.Add(ShopButton);
            Client.EnableButtons.Add(SettingsButton);
            //Buttons not implemented yet
            //Client.EnableButtons.Add(ChatButton);
            //Client.EnableButtons.Add(ReplayButton);

            Client.ExecutingDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            #endregion
            
            Client.PVPNet = new PVPNetConnection();
            Client.PVPNet.OnError += Client.PVPNet_OnError;

            PatcherPage Patch = new PatcherPage();
            Client.Pages = new List<Page>();
            Client.Container = Container;
            Client.SwitchPage(Patch, "PATCH");
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        #region Navigation Bar Buttons

        private void LogoButton_Click(object sender, RoutedEventArgs e)
        {
            if (Client.IsLoggedIn)
            {
                MainPage MainPage = new MainPage();
                Client.SwitchPage(MainPage, "Snowl");
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (Client.IsLoggedIn)
            {
                foreach (Button b in Client.EnableButtons)
                {
                    BrushConverter bc = new BrushConverter();
                    Brush brush = (Brush)bc.ConvertFrom("#FFAAAAAA");
                    b.Foreground = brush;
                    if ((string)b.Content == "LOGOUT")
                    {
                        //If it is the LOGIN button convert back to original
                        bc = new BrushConverter();
                        brush = (Brush)bc.ConvertFrom("#FFFFFF");
                        b.Foreground = brush;
                        b.Content = "LOGIN";
                    }
                }
                Client.PVPNet.Disconnect();
                LoginPage LoginPage = new LoginPage();
                Client.SwitchPage(LoginPage, "LOGIN");
                Client.IsLoggedIn = false;
            }
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (Client.IsLoggedIn)
            {
                ProfilePage ProfilePage = new ProfilePage();
                Client.SwitchPage(ProfilePage, "PROFILE");
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (Client.IsLoggedIn)
            {
                PlayPage PlayPage = new PlayPage();
                Client.SwitchPage(PlayPage, "PLAY");
            }
        }
        #endregion

        private void ShopButton_Click(object sender, RoutedEventArgs e)
        {
            if (Client.IsLoggedIn)
            {
                ShopPage ShopPage = new ShopPage();
                Client.SwitchPage(ShopPage, "SHOP");
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (Client.IsLoggedIn)
            {
                SettingsPage SettingsPage = new SettingsPage();
                Client.SwitchPage(SettingsPage, "SETTINGS");
            }
        }
    }
}
