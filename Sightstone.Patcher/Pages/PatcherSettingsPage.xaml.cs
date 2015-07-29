#region
using Sightstone.Patcher.Logic;
using Sightstone.Patcher.Logic.Region;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
#endregion

namespace Sightstone.Patcher.Pages
{
    /// <summary>
    ///     Interaction logic for PatcherSettings.xaml
    /// </summary>
    public partial class PatcherSettingsPage
    {
        List<MainRegion> riot = new List<MainRegion>();
        List<MainRegion> garena = new List<MainRegion>();
        MainRegion pbe;
        MainRegion kr;
        public PatcherSettingsPage(bool newsettings = false)
        {
            InitializeComponent();
            if (newsettings)
                SettingsLabel.Content = "Please configure your settings before using Sightstone.Patcher!";

            Version.SelectedItem = Properties.Settings.Default.UseGithub ? Github : Appveyor;

            UpdateSettings.SelectedItem = Properties.Settings.Default.OnlyLOL ? OnlyLoL : Sightstone;

            LOLP2P.IsChecked = Properties.Settings.Default.LOLP2P;
            LCP2P.IsChecked = Properties.Settings.Default.LCP2P;
            LCPP2P.IsChecked = Properties.Settings.Default.LCPP2P;
            AlwaysUpdate.IsChecked = Properties.Settings.Default.AlwaysUpdate;
            PatcherVolume.Value = Properties.Settings.Default.Volume;            

            var regions = Client.GetInstances<MainRegion>();
            foreach (var region in regions)
            {
                switch (region.RegionType)
                {
                    case RegionType.Riot:
                        riot.Add(region);
                        break;
                    case RegionType.PBE:
                        pbe = region;
                        break;
                    case RegionType.KR:
                        kr = region;
                        break;
                    case RegionType.Garena:
                        garena.Add(region);
                        break;
                }
            }

            Region.Items.Add("Riot");
            Region.Items.Add("PBE");
            Region.Items.Add("Korea");
            Region.Items.Add("Garena");
            Region.SelectedIndex = -1;
            if (string.IsNullOrEmpty(Properties.Settings.Default.RegionType))
                return;
            Region.SelectedItem = Properties.Settings.Default.RegionType;
            RegionName.Visibility = System.Windows.Visibility.Visible;
            switch (Region.SelectedItem.ToString()) {
                case "Riot":
                    foreach (var region in riot)
                    {
                        RegionName.Items.Add(region.RegionName);
                    }
                    RegionName.SelectedIndex = 1;
                    if (!string.IsNullOrEmpty(Properties.Settings.Default.RegionName))
                        RegionName.SelectedItem = Properties.Settings.Default.RegionName;
                    break;
                case "PBE":
                    RegionName.Items.Add("PBE");
                    RegionName.SelectedItem = "PBE";
                    RegionName.IsEnabled = false;
                    if (!string.IsNullOrEmpty(Properties.Settings.Default.RegionName))
                        RegionName.SelectedItem = Properties.Settings.Default.RegionName;
                    break;
                case "Korea":
                    RegionName.Items.Add("KR");
                    RegionName.SelectedItem = "KR";
                    RegionName.IsEnabled = false;
                    if (!string.IsNullOrEmpty(Properties.Settings.Default.RegionName))
                        RegionName.SelectedItem = Properties.Settings.Default.RegionName;
                    break;
                case "Garena":
                    foreach (var region in garena)
                    {
                        RegionName.Items.Add(region.RegionName);
                    }
                    RegionName.SelectedIndex = 1;
                    if (!string.IsNullOrEmpty(Properties.Settings.Default.RegionName))
                        RegionName.SelectedItem = Properties.Settings.Default.RegionName;
                    break;
                default:
                    RegionName.SelectedIndex = -1;
                    RegionName.Visibility = System.Windows.Visibility.Hidden;
                    break;
            }
        }

        private void Version_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.UseGithub = Version.SelectedItem == Github;
            Properties.Settings.Default.Save();
        }

        private void Setting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.OnlyLOL = UpdateSettings.SelectedItem == OnlyLoL;
            Properties.Settings.Default.Save();
        }

        private void LOLP2P_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (LOLP2P.IsChecked != null)
                Properties.Settings.Default.LOLP2P = (bool)LOLP2P.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void LCP2P_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (LCP2P.IsChecked != null)
                Properties.Settings.Default.LCP2P = (bool)LCP2P.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void LCPP2P_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (LCPP2P.IsChecked != null)
                Properties.Settings.Default.LCPP2P = (bool)LCPP2P.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void AlwaysUpdate_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (AlwaysUpdate.IsChecked != null)
                Properties.Settings.Default.AlwaysUpdate = (bool)AlwaysUpdate.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void PatcherVolume_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            Properties.Settings.Default.Volume = PatcherVolume.Value;
            Properties.Settings.Default.Save();
            Client.SoundPlayer.Volume = Properties.Settings.Default.Volume / 100;
        }

        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            //Do nothing because we have to edit the LC settings and that we will do later
        }

        private void Region_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Region.SelectedIndex == -1)
                return;
            RegionName.IsEnabled = true;
            Properties.Settings.Default.RegionType = Region.SelectedItem.ToString();
            Properties.Settings.Default.Save();
            RegionName.Items.Clear();
            RegionName.Visibility = System.Windows.Visibility.Visible;
            switch (Region.SelectedItem.ToString()) 
            {
                case "Riot":
                    foreach (var region in riot)
                    {
                        RegionName.Items.Add(region.RegionName);
                    }
                    if (!string.IsNullOrEmpty(Properties.Settings.Default.RegionName))
                        RegionName.SelectedItem = Properties.Settings.Default.RegionName;
                    break;
                case "PBE":
                    RegionName.Items.Add("PBE");
                    RegionName.SelectedItem = "PBE";
                    Properties.Settings.Default.RegionName = "PBE";
                    RegionName_SelectionChanged(null, null);
                    RegionName.IsEnabled = false;
                    break;
                case "Korea":
                    RegionName.Items.Add("KR");
                    RegionName.SelectedItem = "KR";
                    Properties.Settings.Default.RegionName = "KR";
                    RegionName_SelectionChanged(null, null);
                    RegionName.IsEnabled = false;
                    break;
                case "Garena":
                    foreach (MainRegion region in garena)
                    {
                        RegionName.Items.Add(region.RegionName);
                    }
                    if (!string.IsNullOrEmpty(Properties.Settings.Default.RegionName))
                        RegionName.SelectedItem = Properties.Settings.Default.RegionName;
                    break;
                default:
                    RegionName.SelectedIndex = -1;
                    RegionName.Visibility = System.Windows.Visibility.Hidden;
                    break;
            }
            Properties.Settings.Default.Save();
        }

        private void RegionName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RegionName.SelectedIndex == -1)
                return;
            Properties.Settings.Default.RegionName = RegionName.SelectedItem.ToString();
            Properties.Settings.Default.Save();

            var region = Properties.Settings.Default.RegionName;
            Client.Region = MainRegion.GetMainRegion(region);
            if (!string.IsNullOrEmpty(region))
                Client.RegionLabel.Content = "Connected to: " + region;
            else
                Client.RegionLabel.Content = "Not connected to any regions";
        }
    }
}