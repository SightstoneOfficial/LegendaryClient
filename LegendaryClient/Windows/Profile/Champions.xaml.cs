using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect.RiotObjects.Platform.Catalog.Champion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    /// Interaction logic for Champions.xaml
    /// </summary>
    public partial class Champions : Page
    {
        private List<ChampionDTO> ChampionList;
        private bool NoFilterOnLoad;

        public Champions()
        {
            InitializeComponent();
        }

        public async void Update()
        {
            ChampionDTO[] champList = await Client.PVPNet.GetAvailableChampions();

            ChampionList = new List<ChampionDTO>(champList);

            ChampionList.Sort((x, y) => champions.GetChampion(x.ChampionId).displayName.CompareTo(champions.GetChampion(y.ChampionId).displayName));

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
                tempList = tempList.Where(x => champions.GetChampion(x.ChampionId).displayName.ToLower().Contains(SearchTextBox.Text.ToLower())).ToList();
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
                    //Add to ListView
                    ListViewItem item = new ListViewItem();
                    ProfileChampionImage championImage = new ProfileChampionImage();
                    champions champion = champions.GetChampion(champ.ChampionId);
                    championImage.ChampImage.Source = champion.icon;
                    if (champ.FreeToPlay)
                        championImage.FreeToPlayLabel.Visibility = System.Windows.Visibility.Visible;
                    championImage.ChampName.Content = champion.displayName;
                    if (!champ.Owned && !champ.FreeToPlay)
                    {
                        championImage.ChampImage.Opacity = 0.5;
                    }
                    item.Tag = champ.ChampionId;
                    item.Content = championImage.Content;
                    ChampionSelectListView.Items.Add(item);
                }
            }
        }
    }
}