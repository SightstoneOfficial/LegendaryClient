using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect.RiotObjects.Platform.Catalog.Champion;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using log4net;

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    /// Interaction logic for Champions.xaml
    /// </summary>
    public partial class Champions : Page
    {
        private List<ChampionDTO> ChampionList;
        private bool NoFilterOnLoad;
        private static readonly ILog log = LogManager.GetLogger(typeof(Champions));

        public Champions()
        {
            InitializeComponent();
        }

        public async void Update()
        {
            ChampionDTO[] champList = await Client.PVPNet.GetAvailableChampions();

            ChampionList = new List<ChampionDTO>(champList);

            ChampionList.Sort((x, y) => Logic.SQLite.Champions.GetChampion(x.ChampionId).DisplayName.CompareTo(Logic.SQLite.Champions.GetChampion(y.ChampionId).DisplayName));

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
                tempList = tempList.Where(x => Logic.SQLite.Champions.GetChampion(x.ChampionId).DisplayName.ToLower().Contains(SearchTextBox.Text.ToLower())).ToList();
            }

            bool AllChampions = false;
            bool OwnedChampions = false;
            bool NotOwnedChampions = false;
            bool AvaliableChampions = false;

            switch ((string)FilterComboBox.SelectedValue)
            {
                case "All":
                    AllChampions = true;
                    break;

                case "Owned":
                    OwnedChampions = true;
                    break;

                case "Not Owned":
                    NotOwnedChampions = true;
                    break;

                default:
                    AvaliableChampions = true;
                    break;
            }

            foreach (ChampionDTO champ in tempList)
            {
                if ((AvaliableChampions && (champ.Owned || champ.FreeToPlay)) ||
                    (AllChampions) ||
                    (OwnedChampions && champ.Owned) ||
                    (NotOwnedChampions && !champ.Owned))
                {
                    ProfileChampionImage championImage = new ProfileChampionImage();
                    Logic.SQLite.Champions champion = Logic.SQLite.Champions.GetChampion(champ.ChampionId);
                    championImage.ChampImage.Source = champion.Icon;
                    if (champ.FreeToPlay)
                        championImage.FreeToPlayLabel.Visibility = System.Windows.Visibility.Visible;
                    championImage.ChampName.Content = champion.DisplayName;
                    if (!champ.Owned && !champ.FreeToPlay)
                    {
                        championImage.ChampImage.Opacity = 0.5;
                    }
                    championImage.Tag = champ.ChampionId;
                    ChampionSelectListView.Items.Add(championImage);
                }
            }
        }

        private void ChampionSelectListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChampionSelectListView.SelectedIndex != -1)
            {
                ProfileChampionImage selectedChampion = (ProfileChampionImage)ChampionSelectListView.SelectedItem;
                Client.OverlayContainer.Content = new ChampionDetailsPage((int)selectedChampion.Tag).Content;
                Client.OverlayContainer.Visibility = Visibility.Visible;
            }
        }
    }
}