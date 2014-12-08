using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect.RiotObjects.Platform.Catalog.Champion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    /// Interaction logic for Skins.xaml
    /// </summary>
    public partial class Skins : Page
    {
        private List<ChampionDTO> ChampionList;

        public Skins()
        {
            InitializeComponent();
        }

        public async void Update()
        {
            try
            {

                ChampionDTO[] champList = await Client.PVPNet.GetAvailableChampions();

                ChampionList = new List<ChampionDTO>(champList);

                ChampionList.Sort((x, y) => champions.GetChampion(x.ChampionId).displayName.CompareTo(champions.GetChampion(y.ChampionId).displayName));

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
            SkinSelectListView.Items.Clear();

            List<ChampionDTO> tempList = ChampionList.ToList();
            List<ChampionSkinDTO> skinList = new List<ChampionSkinDTO>();

            foreach (ChampionDTO champion in tempList)
            {
                skinList.AddRange(champion.ChampionSkins);
            }

            if (!String.IsNullOrEmpty(SearchTextBox.Text) && !LimitedSkinCheckBox.IsChecked.Value)
            {
                skinList = skinList.Where(x => championSkins.GetSkin(x.SkinId).displayName.ToLower().Contains(SearchTextBox.Text.ToLower())).ToList();
            }

            foreach (ChampionSkinDTO skin in skinList)
            {
                if (LimitedSkinCheckBox.IsChecked.Value ? !skin.StillObtainable && skin.Owned : skin.Owned)
                {
                    ProfileSkinImage skinImage = new ProfileSkinImage();
                    championSkins championSkin = championSkins.GetSkin(skin.SkinId);
                    var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", championSkins.GetSkin(skin.SkinId).portraitPath), UriKind.Absolute);
                    skinImage.SkinImage.Source = new BitmapImage(uriSource);
                    if (!skin.StillObtainable)
                        skinImage.LimitedLabel.Visibility = System.Windows.Visibility.Visible;
                    skinImage.SkinName.Content = championSkin.displayName;
                    skinImage.Margin = new System.Windows.Thickness(5, 0, 5, 0);
                    SkinSelectListView.Items.Add(skinImage);
                }
            }
        }

        private void LimitedSkinCheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            FilterSkins();
        }
    }
}