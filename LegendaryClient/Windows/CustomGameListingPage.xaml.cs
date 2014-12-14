#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.Maps;
using LegendaryClient.Properties;
using PVPNetConnect.RiotObjects.Platform.Game.Practice;

#endregion

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for CustomGameListingPage.xaml
    /// </summary>
    public partial class CustomGameListingPage
    {
        private readonly List<GameItem> allItems = new List<GameItem>();

        public CustomGameListingPage()
        {
            InitializeComponent();
            Change();

            GetGames();
        }

        public void Change()
        {
            var themeAccent = new ResourceDictionary
            {
                Source = new Uri(Settings.Default.Theme)
            };
            Resources.MergedDictionaries.Add(themeAccent);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            GetGames();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LimitGames();
        }

        private async void GetGames()
        {
            CustomGameListView.Items.Clear();
            allItems.Clear();
            PracticeGameSearchResult[] games = await Client.PVPNet.ListAllPracticeGames();
            foreach (GameItem item in games.Select(game => new GameItem
            {
                GameName = game.Name,
                GameOwner = game.Owner.SummonerName,
                Map = BaseMap.GetMap(game.GameMapId).DisplayName,
                Private = game.PrivateGame.ToString().Replace("True", "Y").Replace("False", ""),
                Slots = (game.Team1Count + game.Team2Count) + "/" + game.MaxNumPlayers,
                Spectators = game.SpectatorCount,
                Type = game.GameModeString.Replace("ODIN", "DOMINION"),
                Id = game.Id
            }))
            {
                CustomGameListView.Items.Add(item);
                allItems.Add(item);
            }
            LimitGames();
        }

        private void LimitGames()
        {
            var tempItems = new List<GameItem>();
            foreach (
                GameItem item in allItems.Where(item => item.GameName.ToLower().Contains(SearchTextBox.Text.ToLower())))
            {
                if (PrivateCheckbox.IsChecked != true)
                    if (item.Private == "Y")
                        continue;

                tempItems.Add(item);
            }
            CustomGameListView.Items.Clear(); // clear list items before adding
            foreach (GameItem item in tempItems)
                CustomGameListView.Items.Add(item);
        }

        private void PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            JoinGameButton.IsEnabled = !String.IsNullOrEmpty(PasswordTextBox.Text);
        }

#pragma warning disable 4014 //Code does not need to be awaited
        private void JoinGameButton_Click(object sender, RoutedEventArgs e)
        {
            double gameId = 0;
            string gameName = "";
            foreach (GameItem item in CustomGameListView.SelectedItems)
            {
                gameId = item.Id;
                gameName = item.GameName;
            }
            if (!String.IsNullOrEmpty(PasswordTextBox.Text))
                Client.PVPNet.JoinGame(gameId, PasswordTextBox.Text);
            else
                Client.PVPNet.JoinGame(gameId);

            Client.GameID = gameId;
            Client.GameName = gameName;

            Client.SwitchPage(new CustomGameLobbyPage());
        }

        private void CustomGameListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CustomGameListView.SelectedIndex == -1)
                return;

            JoinGameButton.IsEnabled = false;
            PasswordTextBox.IsReadOnly = true;
            PasswordTextBox.Text = "";
            bool isPrivate = false;
            if (CustomGameListView.SelectedItems.Count == 0)
                return;

            foreach (
                GameItem gitem in CustomGameListView.SelectedItems.Cast<GameItem>().Where(gitem => gitem.Private == "Y")
                )
                isPrivate = true;

            if ((!isPrivate) || (isPrivate && !String.IsNullOrEmpty(PasswordTextBox.Text)))
                JoinGameButton.IsEnabled = true;
            else if (isPrivate)
                PasswordTextBox.IsReadOnly = false;
        }

        private void PrivateCheckbox_Click(object sender, RoutedEventArgs e)
        {
            LimitGames();
        }
    }

    public class GameItem
    {
        public string GameName { get; set; }

        public string GameOwner { get; set; }

        public string Slots { get; set; }

        public int Spectators { get; set; }

        public double Id { get; set; }

        public string Map { get; set; }

        public string Private { get; set; }

        public string Type { get; set; }
    }
}