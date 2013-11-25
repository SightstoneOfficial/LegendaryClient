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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LegendaryClient.Logic;
using LegendaryClient.Logic.PlayerSpell;
using Path = System.IO.Path;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for SelectSummonerSpells.xaml
    /// </summary>
    public partial class SelectSummonerSpells : Page
    {
        int SelectedSpell1 = 0;
        public SelectSummonerSpells(string GameMode)
        {
            InitializeComponent();
            var values = Enum.GetValues(typeof(NameToImage));
            foreach (NameToImage Spell in values)
            {
                if (!SummonerSpell.CanUseSpell((int)Spell, Client.LoginPacket.AllSummonerData.SummonerLevel.Level, GameMode))
                    continue;
                ListViewItem item = new ListViewItem();
                Image champImage = new Image();
                champImage.Height = 64;
                champImage.Width = 64;
                champImage.Margin = new Thickness(5, 5, 5, 5);
                var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", "Summoner" + Spell.ToString() + ".png"), UriKind.Absolute);
                champImage.Source = new BitmapImage(uriSource);
                item.Content = champImage;
                item.Tag = (int)Spell;
                SummonerSpellListView.Items.Add(item);
            }
        }

        private async void SummonerSpellListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SummonerSpellListView.SelectedIndex != -1)
            {
                ListViewItem item = (ListViewItem)SummonerSpellListView.SelectedItem;
                int spellId = Convert.ToInt32(item.Tag);
                NameToImage spellName = (NameToImage)spellId;
                var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", "Summoner" + spellName + ".png"), UriKind.Absolute);
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
}
