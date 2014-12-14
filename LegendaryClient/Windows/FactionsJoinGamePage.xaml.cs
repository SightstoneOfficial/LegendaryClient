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
    ///     Interaction logic for FactionsJoinGamePage.xaml
    /// </summary>
    public partial class FactionsJoinGamePage
    {
        private readonly List<GameItem> allItems = new List<GameItem>();

        public FactionsJoinGamePage()
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
            }).Where(item => item.GameName.ToLower().Contains("factions")))
            {
                CustomGameListView.Items.Add(item);
                allItems.Add(item);
            }
            LimitGames();
        }

        private void LimitGames()
        {
            List<GameItem> tempItems =
                allItems.Where(item => item.GameName.ToLower().Contains(SearchTextBox.Text.ToLower())).ToList();
            CustomGameListView.Items.Clear(); // clear list items before adding
            foreach (GameItem item in tempItems)
                CustomGameListView.Items.Add(item);
        }

        private void PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            JoinGameButton.IsEnabled = !String.IsNullOrEmpty("factions");
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
            if (!String.IsNullOrEmpty("factions"))
                Client.PVPNet.JoinGame(gameId, "factions");
            else
                Client.PVPNet.JoinGame(gameId);

            Client.GameID = gameId;
            Client.GameName = gameName;

            Client.SwitchPage(new FactionsGameLobbyPage());
        }

        private void CustomGameListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CustomGameListView.SelectedIndex == -1)
                return;

            JoinGameButton.IsEnabled = false;
            bool isPrivate = false;
            if (CustomGameListView.SelectedItems.Count == 0)
                return;

            foreach (
                GameItem gitem in CustomGameListView.SelectedItems.Cast<GameItem>().Where(gitem => gitem.Private == "Y")
                )
                isPrivate = true;

            if ((!isPrivate) || (isPrivate && !String.IsNullOrEmpty("factions")))
                JoinGameButton.IsEnabled = true;
        }

        private void PrivateCheckbox_Click(object sender, RoutedEventArgs e)
        {
            LimitGames();
        }
    }
}