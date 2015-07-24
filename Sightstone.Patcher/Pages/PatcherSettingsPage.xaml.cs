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

            if (Properties.Settings.Default.UseGithub)
                Version.SelectedItem = Github;
            else
                Version.SelectedItem = Appveyor;

            if (Properties.Settings.Default.OnlyLOL)
                UpdateSettings.SelectedItem = OnlyLoL;
            else
                UpdateSettings.SelectedItem = Sightstone;

            LOLP2P.IsChecked = Properties.Settings.Default.LOLP2P;
            LCP2P.IsChecked = Properties.Settings.Default.LCP2P;
            LCPP2P.IsChecked = Properties.Settings.Default.LCPP2P;
            AlwaysUpdate.IsChecked = Properties.Settings.Default.AlwaysUpdate;
            PatcherVolume.Value = Properties.Settings.Default.Volume;            

            var regions = Client.GetInstances<MainRegion>();
            foreach (MainRegion region in regions)
            {
                if (region.RegionType == RegionType.Riot)
                    riot.Add(region);
                else if (region.RegionType == RegionType.PBE)
                    pbe = region;
                else if (region.RegionType == RegionType.KR)
                    kr = region;
                else if (region.RegionType == RegionType.Garena)
                    garena.Add(region);
            }

            Region.Items.Add("Riot");
            Region.Items.Add("PBE");
            Region.Items.Add("Korea");
            Region.Items.Add("Garena");
            Region.SelectedIndex = -1;
            if (!string.IsNullOrEmpty(Properties.Settings.Default.RegionType))
            {
                Region.SelectedItem = Properties.Settings.Default.RegionType;
                RegionName.Visibility = System.Windows.Visibility.Visible;
                if (Region.SelectedItem.ToString() == "Riot")
                {
                    foreach (MainRegion region in riot)
                    {
                        RegionName.Items.Add(region.RegionName);
                    }
                    if (!string.IsNullOrEmpty(Properties.Settings.Default.RegionName))
                        RegionName.SelectedItem = Properties.Settings.Default.RegionName;
                }
                else if (Region.SelectedItem.ToString() == "PBE")
                {
                    RegionName.Items.Add("PBE");
                    RegionName.SelectedItem = "PBE";
                    RegionName.IsEnabled = false;
                }
                else if (Region.SelectedItem.ToString() == "Korea")
                {
                    RegionName.Items.Add("KR");
                    RegionName.SelectedItem = "KR";
                    RegionName.IsEnabled = false;
                }
                else if (Region.SelectedItem.ToString() == "Garena")
                {
                    foreach (MainRegion region in garena)
                    {
                        RegionName.Items.Add(region.RegionName);
                    }
                    if (!string.IsNullOrEmpty(Properties.Settings.Default.RegionName))
                        RegionName.SelectedItem = Properties.Settings.Default.RegionName;
                }
                else
                {
                    RegionName.SelectedIndex = -1;
                    RegionName.Visibility = System.Windows.Visibility.Hidden;
                }
            }
        }

        private void Version_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Version.SelectedItem == Github)
                Properties.Settings.Default.UseGithub = true;
            else
                Properties.Settings.Default.UseGithub = false;
            Properties.Settings.Default.Save();
        }

        private void Setting_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (UpdateSettings.SelectedItem == OnlyLoL)
                Properties.Settings.Default.OnlyLOL = true;
            else
                Properties.Settings.Default.OnlyLOL = false;
            Properties.Settings.Default.Save();
        }

        private void LOLP2P_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            Properties.Settings.Default.LOLP2P = (bool)LOLP2P.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void LCP2P_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            Properties.Settings.Default.LCP2P = (bool)LCP2P.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void LCPP2P_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            Properties.Settings.Default.LCPP2P = (bool)LCPP2P.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void AlwaysUpdate_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
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
            if (Region.SelectedItem.ToString() == "Riot")
            {
                foreach (MainRegion region in riot)
                {
                    RegionName.Items.Add(region.RegionName);
                }
                if (!string.IsNullOrEmpty(Properties.Settings.Default.RegionName))
                    RegionName.SelectedItem = Properties.Settings.Default.RegionName;
            }
            else if (Region.SelectedItem.ToString() == "PBE")
            {
                RegionName.Items.Add("PBE");
                RegionName.SelectedItem = "PBE";
                RegionName.IsEnabled = false;
            }
            else if (Region.SelectedItem.ToString() == "Korea")
            {
                RegionName.Items.Add("KR");
                RegionName.SelectedItem = "KR";
                RegionName.IsEnabled = false;
            }
            else if (Region.SelectedItem.ToString() == "Garena")
            {
                foreach (MainRegion region in garena)
                {
                    RegionName.Items.Add(region.RegionName);
                }
                if (!string.IsNullOrEmpty(Properties.Settings.Default.RegionName))
                    RegionName.SelectedItem = Properties.Settings.Default.RegionName;
            }
            else
            {
                RegionName.SelectedIndex = -1;
                RegionName.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void RegionName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RegionName.SelectedIndex == -1)
                return;
            Properties.Settings.Default.RegionName = RegionName.SelectedItem.ToString();
            Properties.Settings.Default.Save();

            var region = Properties.Settings.Default.RegionName;
            if (!string.IsNullOrEmpty(region))
                Client.regionLabel.Content = "Connected to: " + region;
            else
                Client.regionLabel.Content = "Not connected to any regions";
        }
    }
}