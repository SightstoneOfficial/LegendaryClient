﻿using Sightstone.Controls;
using Sightstone.Logic;
using Sightstone.Logic.SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Sightstone.Logic.Riot;
using Sightstone.Logic.Riot.Platform;
using Sightstone.Logic.MultiUser;
using System.Windows.Input;

namespace Sightstone.Windows.Profile
{
    /// <summary>
    ///     Interaction logic for Skins.xaml
    /// </summary>
    public partial class Skins
    {
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollviewer = sender as ScrollViewer;
            if (e.Delta > 0)
                scrollviewer.LineLeft();
            else
                scrollviewer.LineRight();
            e.Handled = true;
        }

        private List<ChampionDTO> championList;

        public Skins()
        {
            InitializeComponent();
        }

        public async void Update()
        {
            try
            {
                var champList = await (UserList.Users[Client.CurrentServer])[Client.CurrentUser].calls.GetAvailableChampions();

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
                    var UriSource =
                        new System.Uri(
                            Path.Combine(Client.ExecutingDirectory, "Assets", "images", "champions",
                                championSkins.GetSkin(skin.SkinId).portraitPath), UriKind.Absolute);
                    skinImage.SkinImage.Source = new BitmapImage(UriSource);
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