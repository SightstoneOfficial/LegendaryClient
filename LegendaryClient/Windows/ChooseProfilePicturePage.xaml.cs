using LegendaryClient.Logic;
using PVPNetConnect.RiotObjects.Platform.Catalog.Icon;
using PVPNetConnect.RiotObjects.Platform.Summoner.Icon;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for ChooseProfilePicturePage.xaml
    /// </summary>
    public partial class ChooseProfilePicturePage : Page
    {
        public ChooseProfilePicturePage()
        {
            InitializeComponent();
            GetIcons();
        }

        private async void GetIcons()
        {
            SummonerIconInventoryDTO PlayerIcons = await Client.PVPNet.GetSummonerIconInventory(Client.LoginPacket.AllSummonerData.Summoner.SumId);
            foreach (Icon ic in PlayerIcons.SummonerIcons)
            {
                Image champImage = new Image();
                champImage.Height = 64;
                champImage.Width = 64;
                champImage.Margin = new Thickness(5, 5, 5, 5);
                var uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", ic.IconId + ".png");
                champImage.Source = Client.GetImage(uriSource);
                champImage.Tag = ic.IconId;
                SummonerIconListView.Items.Add(champImage);
            }
            for (int i = 0; i < 29; i++)
            {
                Image champImage = new Image();
                champImage.Height = 64;
                champImage.Width = 64;
                champImage.Margin = new Thickness(5, 5, 5, 5);
                var uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", i + ".png");
                champImage.Source = Client.GetImage(uriSource);
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
                Image m = (Image)SummonerIconListView.SelectedItem;
                int SummonerIcon = Convert.ToInt32(m.Tag);
                await Client.PVPNet.UpdateProfileIconId(SummonerIcon);
                Client.LoginPacket.AllSummonerData.Summoner.ProfileIconId = SummonerIcon;
                Client.SetChatHover();
                var uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", SummonerIcon + ".png");
                Client.MainPageProfileImage.Source = Client.GetImage(uriSource);
            }
            Client.OverlayContainer.Visibility = Visibility.Hidden;
        }
    }
}