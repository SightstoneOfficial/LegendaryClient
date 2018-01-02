﻿using Sightstone.Logic;
using Sightstone.Logic.PlayerSpell;
using Sightstone.Properties;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Sightstone.Logic.Riot;
using Sightstone.Logic.MultiUser;

namespace Sightstone.Windows
{
    /// <summary>
    ///     Interaction logic for SelectSummonerSpells.xaml
    /// </summary>
    public partial class SelectSummonerSpells
    {
        private int SelectedSpell1;
        static UserClient UserClient = (UserList.Users[Client.CurrentServer])[Client.CurrentUser];

        public SelectSummonerSpells(string gameMode)
        {
            InitializeComponent();

            Array values = Enum.GetValues(typeof (NameToImage));
            foreach (NameToImage spell in values)
            {
                if (
                    !SummonerSpell.CanUseSpell((int) spell, UserClient.LoginPacket.AllSummonerData.SummonerLevel.Level,
                        gameMode))
                    continue;

                var champImage = new Image
                {
                    Height = 64,
                    Width = 64,
                    Margin = new Thickness(5, 5, 5, 5)
                };
                var UriSource = Client.GetSpellIconUri(spell);
                champImage.Source = new BitmapImage(UriSource);
                champImage.Tag = (int) spell;
                SummonerSpellListView.Items.Add(champImage);
            }
        }

        private async void SummonerSpellListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SummonerSpellListView.SelectedIndex == -1)
                return;

            var item = (Image) SummonerSpellListView.SelectedItem;
            int spellId = Convert.ToInt32(item.Tag);
            var spellName = (NameToImage) spellId;
            var UriSource = Client.GetSpellIconUri(spellName);
            if (SelectedSpell1 == 0)
            {
                SummonerSpell1.Source = new BitmapImage(UriSource);
                SummonerSpellListView.Items.Remove(item);
                SelectedSpell1 = spellId;
            }
            else
            {
                SummonerSpell2.Source = new BitmapImage(UriSource);
                await UserClient.calls.SelectSpells(SelectedSpell1, spellId);
                Client.OverlayContainer.Visibility = Visibility.Hidden;
            }
        }
    }
}