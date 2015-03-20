using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect.RiotObjects.Platform.Catalog.Champion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    ///     Interaction logic for Skins.xaml
    /// </summary>
    public partial class Skins
    {
        private List<ChampionDTO> championList;

        public Skins()
        {
            InitializeComponent();
        }

        public async void Update()
        {
            try
            {
                var champList = await Client.PVPNet.GetAvailableChampions();

                championList = new List<ChampionDTO>(champList);

                championList.Sort(
                    (x, y) =>
                        string.Compare(champions.GetChampion(x.ChampionId)
                            .displayName, champions.GetChampion(y.ChampionId).displayName, StringComparison.Ordinal));

                FilterSkins();
            }
            catch (Exception e)
            {
                Client.Log("Error with skins: " + e.Message, "Error");
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterSkins();
        }

        private void FilterSkins()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                SkinSelectListView.Items.Clear();

                var tempList = championList.ToList();
                var skinList = new List<ChampionSkinDTO>();
                foreach (var champion in tempList)
                {
                    skinList.AddRange(champion.ChampionSkins);
                }

                if (LimitedSkinCheckBox.IsChecked != null &&
                    (!string.IsNullOrEmpty(SearchTextBox.Text) && !LimitedSkinCheckBox.IsChecked.Value))
                {
                    skinList =
                        skinList.Where(
                            x =>
                                championSkins.GetSkin(x.SkinId).displayName.ToLower().Contains(SearchTextBox.Text.ToLower()))
                            .ToList();
                }

                foreach (var skin in skinList)
                {
                    if (LimitedSkinCheckBox.IsChecked == null ||
                        (LimitedSkinCheckBox.IsChecked.Value ? skin.StillObtainable || !skin.Owned : !skin.Owned))
                    {
                        continue;
                    }

                    var skinImage = new ProfileSkinImage();
                    var championSkin = championSkins.GetSkin(skin.SkinId);
                    var uriSource =
                        new Uri(
                            Path.Combine(Client.ExecutingDirectory, "Assets", "champions",
                                championSkins.GetSkin(skin.SkinId).portraitPath), UriKind.Absolute);
                    skinImage.SkinImage.Source = new BitmapImage(uriSource);
                    if (!skin.StillObtainable)
                    {
                        skinImage.LimitedLabel.Visibility = Visibility.Visible;
                    }

                    skinImage.SkinName.Content = championSkin.displayName;
                    skinImage.Margin = new Thickness(5, 0, 5, 0);
                    SkinSelectListView.Items.Add(skinImage);
                }
            }));
        }

        private void LimitedSkinCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            FilterSkins();
        }
    }
}