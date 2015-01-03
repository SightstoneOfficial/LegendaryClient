using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Properties;
using PVPNetConnect.RiotObjects.Platform.Catalog.Champion;

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for SelectChampOverlay.xaml
    /// </summary>
    public partial class SelectChampOverlay
    {
        private readonly List<ChampionDTO> ChampList;
        private readonly TeamQueuePage tqp;

        public SelectChampOverlay(TeamQueuePage tqp)
        {
            InitializeComponent();
            Change();

            this.tqp = tqp;
            ChampionSelectListView.Items.Clear();
            if (true)
            {
                ChampList = new List<ChampionDTO>(Client.PlayerChampions);
                ChampList.Sort(
                    (x, y) =>
                        String.Compare(champions.GetChampion(x.ChampionId)
                            .displayName, champions.GetChampion(y.ChampionId).displayName, StringComparison.Ordinal));

                foreach (ChampionDTO champ in ChampList)
                {
                    champions getChamp = champions.GetChampion(champ.ChampionId);
                    if ((!champ.Owned && !champ.FreeToPlay))
                        continue;

                    //Add to ListView
                    var item = new ListViewItem();
                    var championImage = new ChampionImage
                    {
                        ChampImage = {Source = champions.GetChampion(champ.ChampionId).icon}
                    };
                    if (champ.FreeToPlay)
                        championImage.FreeToPlayLabel.Visibility = Visibility.Visible;
                    championImage.Width = 64;
                    championImage.Height = 64;
                    item.Tag = champ.ChampionId;
                    item.Content = championImage.Content;
                    ChampionSelectListView.Items.Add(item);
                }
                var items = new ListViewItem();
                var img = new ChampionImage
                {
                    ChampImage = {Source = Client.GetImage("getNone")},
                    Width = 64,
                    Height = 64
                };
                items.Tag = 0;
                items.Content = img.Content;
                ChampionSelectListView.Items.Add(items);
            }
        }

        public void Change()
        {
            var themeAccent = new ResourceDictionary
            {
                Source = new Uri(Settings.Default.Theme)
            };
            Resources.MergedDictionaries.Add(themeAccent);
        }

        private void ListViewItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;

            if (item != null && (int) item.Tag == 0)
            {
                Client.usingInstaPick = false;
                try
                {
                    tqp.CreateText(
                        "You will no longer attempt to auto select: " +
                        champions.GetChampion(Client.SelectChamp).displayName, Brushes.OrangeRed);
                }
                catch
                {
                    tqp.CreateText(
                           "You will not try to auto select any champ you didn't even pick one to instalock. This is not the random button... yet", Brushes.OrangeRed);
                }
                Client.SelectChamp = 0;
                return;
            }
            if (item != null)
            {
                Client.SelectChamp = ((int) item.Tag);
                Client.usingInstaPick = true;

                tqp.CreateText("You will attempt to auto select: " + champions.GetChampion((int) item.Tag).displayName,
                    Brushes.OrangeRed);
            }

            Client.OverlayContainer.Visibility = Visibility.Hidden;
            Visibility = Visibility.Hidden;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChampionSelectListView.Items.Clear();

            List<ChampionDTO> tempList = ChampList.ToList();

            if (SearchTextBox.Text != "Search" && !String.IsNullOrEmpty(SearchTextBox.Text))
            {
                tempList =
                    tempList.Where(
                        x =>
                            champions.GetChampion(x.ChampionId)
                                .displayName.ToLower()
                                .Contains(SearchTextBox.Text.ToLower())).ToList();
            }

            foreach (ChampionDTO champ in tempList)
            {
                champions getChamp = champions.GetChampion(champ.ChampionId);
                if ((!champ.Owned && !champ.FreeToPlay))
                    continue;

                //Add to ListView
                var item = new ListViewItem();
                var championImage = new ChampionImage
                {
                    ChampImage = {Source = champions.GetChampion(champ.ChampionId).icon}
                };
                if (champ.FreeToPlay)
                    championImage.FreeToPlayLabel.Visibility = Visibility.Visible;
                championImage.Width = 64;
                championImage.Height = 64;
                item.Tag = champ.ChampionId;
                item.Content = championImage.Content;
                ChampionSelectListView.Items.Add(item);
            }
        }
    }
}