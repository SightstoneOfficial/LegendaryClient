#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Properties;
using PVPNetConnect.RiotObjects.Platform.Catalog.Champion;

#endregion

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    ///     Interaction logic for Champions.xaml
    /// </summary>
    public partial class Champions
    {
        private List<ChampionDTO> ChampionList;
        private bool NoFilterOnLoad;
        //private static readonly ILog log = LogManager.GetLogger(typeof(Champions));

        public Champions()
        {
            InitializeComponent();
            Change();
        }

        public void Change()
        {
            var themeAccent = new ResourceDictionary
            {
                Source = new Uri(Settings.Default.Theme)
            };
            Resources.MergedDictionaries.Add(themeAccent);
        }

        public async void Update()
        {
            ChampionDTO[] champList = await Client.PVPNet.GetAvailableChampions();

            ChampionList = new List<ChampionDTO>(champList);

            ChampionList.Sort(
                (x, y) =>
                    String.Compare(champions.GetChampion(x.ChampionId)
                        .displayName, champions.GetChampion(y.ChampionId).displayName, StringComparison.Ordinal));

            FilterChampions();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterChampions();
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!NoFilterOnLoad) //Don't filter when content is first loaded
            {
                NoFilterOnLoad = true;
                return;
            }
            FilterChampions();
        }

        private void FilterChampions()
        {
            ChampionSelectListView.Items.Clear();

            List<ChampionDTO> tempList = ChampionList.ToList();

            if (!String.IsNullOrEmpty(SearchTextBox.Text))
            {
                tempList =
                    tempList.Where(
                        x =>
                            champions.GetChampion(x.ChampionId)
                                .displayName.ToLower()
                                .Contains(SearchTextBox.Text.ToLower())).ToList();
            }

            bool allChampions = false;
            bool ownedChampions = false;
            bool notOwnedChampions = false;
            bool avaliableChampions = false;

            switch ((string) FilterComboBox.SelectedValue)
            {
                case "All":
                    allChampions = true;
                    break;

                case "Owned":
                    ownedChampions = true;
                    break;

                case "Not Owned":
                    notOwnedChampions = true;
                    break;

                default:
                    avaliableChampions = true;
                    break;
            }

            foreach (ChampionDTO champ in tempList)
            {
                if ((!avaliableChampions || (!champ.Owned && !champ.FreeToPlay)) && (!allChampions) &&
                    (!ownedChampions || !champ.Owned) && (!notOwnedChampions || champ.Owned))
                    continue;

                var championImage = new ProfileChampionImage();
                champions champion = champions.GetChampion(champ.ChampionId);
                championImage.ChampImage.Source = champion.icon;
                if (champ.FreeToPlay)
                    championImage.FreeToPlayLabel.Visibility = Visibility.Visible;

                championImage.ChampName.Content = champion.displayName;
                if (!champ.Owned && !champ.FreeToPlay)
                    championImage.ChampImage.Opacity = 0.5;

                championImage.Tag = champ.ChampionId;
                ChampionSelectListView.Items.Add(championImage);
            }
        }

        /*private void ChampionSelectListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChampionSelectListView.SelectedIndex == -1)
                return;

            var selectedChampion = (ProfileChampionImage) ChampionSelectListView.SelectedItem;
            Client.OverlayContainer.Content = new ChampionDetailsPage((int) selectedChampion.Tag).Content;
            Client.OverlayContainer.Visibility = Visibility.Visible;
        }*/
    }
}