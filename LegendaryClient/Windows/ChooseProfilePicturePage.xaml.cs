#region

using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using LegendaryClient.Logic;
using LegendaryClient.Properties;
using PVPNetConnect.RiotObjects.Platform.Catalog.Icon;
using PVPNetConnect.RiotObjects.Platform.Summoner.Icon;

#endregion

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for ChooseProfilePicturePage.xaml
    /// </summary>
    public partial class ChooseProfilePicturePage
    {
        public ChooseProfilePicturePage()
        {
            InitializeComponent();
            Change();

            GetIcons();
        }

        public void Change()
        {
            var themeAccent = new ResourceDictionary
            {
                Source = new Uri(Settings.Default.Theme)
            };
            Resources.MergedDictionaries.Add(themeAccent);
        }

        private async void GetIcons()
        {
            SummonerIconInventoryDTO playerIcons =
                await Client.PVPNet.GetSummonerIconInventory(Client.LoginPacket.AllSummonerData.Summoner.SumId);
            foreach (Icon ic in playerIcons.SummonerIcons)
            {
                var champImage = new Image
                {
                    Height = 64,
                    Width = 64,
                    Margin = new Thickness(5, 5, 5, 5)
                };
                var uriSource =
                    new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", ic.IconId + ".png"),
                        UriKind.Absolute);
                champImage.Source = new BitmapImage(uriSource);
                champImage.Tag = ic.IconId;
                SummonerIconListView.Items.Add(champImage);
            }
            for (int i = 0; i < 29; i++)
            {
                var champImage = new Image
                {
                    Height = 64,
                    Width = 64,
                    Margin = new Thickness(5, 5, 5, 5)
                };
                var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", i + ".png"),
                    UriKind.Absolute);
                champImage.Source = new BitmapImage(uriSource);
                champImage.Tag = i;
                SummonerIconListView.Items.Add(champImage);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Visibility = Visibility.Hidden;
        }

        private async void SetButton_Click(object sender, RoutedEventArgs e)
        {
            if (SummonerIconListView.SelectedItem != null)
            {
                var m = (Image) SummonerIconListView.SelectedItem;
                int summonerIcon = Convert.ToInt32(m.Tag);
                await Client.PVPNet.UpdateProfileIconId(summonerIcon);
                Client.LoginPacket.AllSummonerData.Summoner.ProfileIconId = summonerIcon;
                Client.SetChatHover();
                var uriSource =
                    new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", summonerIcon + ".png"),
                        UriKind.RelativeOrAbsolute);
                Client.MainPageProfileImage.Source = new BitmapImage(uriSource);
            }
            Client.OverlayContainer.Visibility = Visibility.Hidden;
            Client.done = true;
        }
    }
}