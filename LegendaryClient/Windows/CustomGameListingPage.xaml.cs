using LegendaryClient.Logic;
using LegendaryClient.Logic.Maps;
using PVPNetConnect.RiotObjects.Platform.Game.Practice;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for CustomGameListingPage.xaml
    /// </summary>
    public partial class CustomGameListingPage : Page
    {
        private List<GameItem> allItems = new List<GameItem>();

        public CustomGameListingPage()
        {
            InitializeComponent();
            GetGames();
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
            PracticeGameSearchResult[] Games = await Client.PvpNet.ListAllPracticeGames();
            foreach (PracticeGameSearchResult game in Games)
            {
                GameItem item = new GameItem
                {
                    GameName = game.Name,
                    GameOwner = game.Owner.SummonerName,
                    Map = BaseMap.GetMap(game.GameMapId).DisplayName,
                    Private = game.PrivateGame.ToString().Replace("True", "Y").Replace("False", ""),
                    Slots = (game.Team1Count + game.Team2Count) + "/" + game.MaxNumPlayers,
                    Spectators = game.SpectatorCount,
                    Type = game.GameModeString.Replace("ODIN", "DOMINION"),
                    Id = game.Id
                };
                CustomGameListView.Items.Add(item);
                allItems.Add(item);
            }
            LimitGames();
        }

        private void LimitGames()
        {
            List<GameItem> tempItems = new List<GameItem>();
            foreach (GameItem item in allItems)
            {
                if (item.GameName.ToLower().Contains(SearchTextBox.Text.ToLower()))
                {
                    if (PrivateCheckbox.IsChecked != true)
                    {
                        if (item.Private == "Y")
                        {
                            continue;
                        }
                    }
                    tempItems.Add(item);
                }
            }
            CustomGameListView.Items.Clear(); // clear list items before adding
            foreach (GameItem item in tempItems)
            {
                CustomGameListView.Items.Add(item);
            }
        }

        private void PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!String.IsNullOrEmpty(PasswordTextBox.Text))
            {
                JoinGameButton.IsEnabled = true;
            }
            else
            {
                JoinGameButton.IsEnabled = false;
            }
        }

        #pragma warning disable 4014 //Code does not need to be awaited
        private void JoinGameButton_Click(object sender, RoutedEventArgs e)
        {
            double GameID = 0;
            string GameName = "";
            foreach (GameItem item in CustomGameListView.SelectedItems)
            {
                GameID = item.Id;
                GameName = item.GameName;
            }
            if (!String.IsNullOrEmpty(PasswordTextBox.Text))
                Client.PvpNet.JoinGame(GameID, PasswordTextBox.Text);
            else
                Client.PvpNet.JoinGame(GameID);

            Client.GameId = GameID;
            Client.GameName = GameName;

            Client.SwitchPage(new CustomGameLobbyPage());
        }

        private void CustomGameListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CustomGameListView.SelectedIndex != -1)
            {
                JoinGameButton.IsEnabled = false;
                PasswordTextBox.IsReadOnly = true;
                PasswordTextBox.Text = "";
                bool isPrivate = false;
                if (CustomGameListView.SelectedItems.Count == 0)
                    return;
                foreach (GameItem gitem in CustomGameListView.SelectedItems)
                {
                    if (gitem.Private == "Y")
                    {
                        isPrivate = true;
                    }
                }
                if ((!isPrivate) || (isPrivate && !String.IsNullOrEmpty(PasswordTextBox.Text)))
                {
                    JoinGameButton.IsEnabled = true;
                }
                else
                {
                    if (isPrivate)
                    {
                        PasswordTextBox.IsReadOnly = false;
                    }
                }
            }
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