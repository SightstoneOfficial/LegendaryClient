﻿using Sightstone.Logic;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Sightstone.Logic.Riot;
using Sightstone.Logic.Riot.Platform;
using Sightstone.Logic.MultiUser;

namespace Sightstone.Windows
{
    /// <summary>
    ///     Interaction logic for ChooseProfilePicturePage.xaml
    /// </summary>
    public partial class ChooseProfilePicturePage
    {
        static UserClient UserClient = (UserList.Users[Client.CurrentServer])[Client.CurrentUser];
        public ChooseProfilePicturePage()
        {
            InitializeComponent();
            GetIcons();
        }

        private async void GetIcons()
        {
            SummonerIconInventoryDTO playerIcons =
                await UserClient.calls.GetSummonerIconInventory(UserClient.LoginPacket.AllSummonerData.Summoner.SumId);
            foreach (Icon ic in playerIcons.SummonerIcons)
            {
                var champImage = new Image
                {
                    Height = 64,
                    Width = 64,
                    Margin = new Thickness(5, 5, 5, 5)
                };
                
                champImage.Source = new BitmapImage(Client.GetIconUri(ic.IconId.ToInt()));
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
                champImage.Source = new BitmapImage(Client.GetIconUri(i));
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
                await UserClient.calls.UpdateProfileIconId(summonerIcon);
                UserClient.LoginPacket.AllSummonerData.Summoner.ProfileIconId = summonerIcon;
                UserClient.SetChatHover();
                var UriSource =
                    new System.Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", summonerIcon + ".png"),
                        UriKind.RelativeOrAbsolute);
                Client.UserTitleBarImage.Source = new BitmapImage(UriSource);
                Client.MainPageProfileImage.Source = new BitmapImage(UriSource);            
            }
            Client.OverlayContainer.Visibility = Visibility.Hidden;
            UserClient.done = true;
        }
    }
}