using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect.RiotObjects.Platform.Catalog.Champion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for SelectChampOverlay.xaml
    /// </summary>
    public partial class SelectChampOverlay : Page
    {
        private List<ChampionDTO> ChampList;
        private TeamQueuePage tqp;

        public SelectChampOverlay(TeamQueuePage tqp)
        {
            InitializeComponent();
            this.tqp = tqp;
            ChampionSelectListView.Items.Clear();
            if (true)
            {
                ChampList = new List<ChampionDTO>(Client.PlayerChampions);
                ChampList.Sort((x, y) => champions.GetChampion(x.ChampionId).displayName.CompareTo(champions.GetChampion(y.ChampionId).displayName));

                foreach (ChampionDTO champ in ChampList)
                {
                    champions getChamp = champions.GetChampion(champ.ChampionId);
                    if ((champ.Owned || champ.FreeToPlay))
                    {
                        //Add to ListView
                        ListViewItem item = new ListViewItem();
                        ChampionImage championImage = new ChampionImage();
                        championImage.ChampImage.Source = champions.GetChampion(champ.ChampionId).icon;
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

        private void ListViewItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            Client.SelectChamp = ((int)item.Tag);
            Client.usingInstaPick = true;

            tqp.CreateText("You will attempt to auto select: " + champions.GetChampion((int)item.Tag).displayName, Brushes.OrangeRed);

            Client.OverlayContainer.Visibility = Visibility.Hidden;
            this.Visibility = Visibility.Hidden;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChampionSelectListView.Items.Clear();

            List<ChampionDTO> tempList = ChampList.ToList();

            if (SearchTextBox.Text != "Search" && !String.IsNullOrEmpty(SearchTextBox.Text))
            {
                tempList = tempList.Where(x => champions.GetChampion(x.ChampionId).displayName.ToLower().Contains(SearchTextBox.Text.ToLower())).ToList();
            }

            foreach (ChampionDTO champ in tempList)
            {
                champions getChamp = champions.GetChampion(champ.ChampionId);
                if ((champ.Owned || champ.FreeToPlay))
                {
                    //Add to ListView
                    ListViewItem item = new ListViewItem();
                    ChampionImage championImage = new ChampionImage();
                    championImage.ChampImage.Source = champions.GetChampion(champ.ChampionId).icon;
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
}
