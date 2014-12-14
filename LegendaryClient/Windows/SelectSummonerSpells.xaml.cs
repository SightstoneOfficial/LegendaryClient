#region

using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using LegendaryClient.Logic;
using LegendaryClient.Logic.PlayerSpell;
using LegendaryClient.Properties;

#endregion

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for SelectSummonerSpells.xaml
    /// </summary>
    public partial class SelectSummonerSpells
    {
        private int SelectedSpell1;

        public SelectSummonerSpells(string gameMode)
        {
            InitializeComponent();
            Change();

            Array values = Enum.GetValues(typeof (NameToImage));
            foreach (NameToImage spell in values)
            {
                if (
                    !SummonerSpell.CanUseSpell((int) spell, Client.LoginPacket.AllSummonerData.SummonerLevel.Level,
                        gameMode))
                    continue;

                var champImage = new Image
                {
                    Height = 64,
                    Width = 64,
                    Margin = new Thickness(5, 5, 5, 5)
                };
                var uriSource =
                    new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", "Summoner" + spell + ".png"),
                        UriKind.Absolute);
                champImage.Source = new BitmapImage(uriSource);
                champImage.Tag = (int) spell;
                SummonerSpellListView.Items.Add(champImage);
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

        private async void SummonerSpellListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SummonerSpellListView.SelectedIndex == -1)
                return;

            var item = (Image) SummonerSpellListView.SelectedItem;
            int spellId = Convert.ToInt32(item.Tag);
            var spellName = (NameToImage) spellId;
            var uriSource =
                new Uri(
                    Path.Combine(Client.ExecutingDirectory, "Assets", "spell", "Summoner" + spellName + ".png"),
                    UriKind.Absolute);
            if (SelectedSpell1 == 0)
            {
                SummonerSpell1.Source = new BitmapImage(uriSource);
                SummonerSpellListView.Items.Remove(item);
                SelectedSpell1 = spellId;
            }
            else
            {
                SummonerSpell2.Source = new BitmapImage(uriSource);
                await Client.PVPNet.SelectSpells(SelectedSpell1, spellId);
                Client.OverlayContainer.Visibility = Visibility.Hidden;
            }
        }
    }
}