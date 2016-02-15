﻿using Sightstone.Logic;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Sightstone.Logic.Riot;
using Sightstone.Logic.Riot.Platform;
using Sightstone.Logic.MultiUser;

namespace Sightstone.Windows
{
    /// <summary>
    ///     Interaction logic for FactionsCreateGamePage.xaml
    /// </summary>
    public partial class FactionsCreateGamePage
    {
        static UserClient UserClient = (UserList.Users[Client.CurrentServer])[Client.CurrentUser];
        public FactionsCreateGamePage()
        {
            InitializeComponent();
            UserClient.Whitelist = new List<string>();
            NameTextBox.Text = UserClient.LoginPacket.AllSummonerData.Summoner.Name + "'s game";
        }

        private async void CreateGameButton_Click(object sender, RoutedEventArgs e)
        {
            NameInvalidLabel.Visibility = Visibility.Hidden;
            PracticeGameConfig gameConfig = GenerateGameConfig();
            CreatedGame(await UserClient.calls.CreatePracticeGame(gameConfig));
        }

        private PracticeGameConfig GenerateGameConfig()
        {
            NameInvalidLabel.Visibility = Visibility.Hidden;
            var gameConfig = new PracticeGameConfig
            {
                GameName = NameTextBox.Text + "– " + Team1.Text + " vs. " + Team2.Text,
                GamePassword = PasswordTextBox.Text
            };
            if (EasyCreate.IsChecked != null && (bool)EasyCreate.IsChecked)
            {
                gameConfig.GameName = "FACTIONS – " + Team1.Text + " vs. " + Team2.Text;
                gameConfig.GamePassword = "factions";
            }
            gameConfig.MaxNumPlayers = Convert.ToInt32(TeamSizeComboBox.SelectedItem) * 2;
            switch ((string)GameTypeComboBox.SelectedItem)
            {
                case "Blind Pick":
                    gameConfig.GameTypeConfig = 1;
                    break;

                case "No Ban Draft":
                    gameConfig.GameTypeConfig = 3;
                    break;

                case "All Random":
                    gameConfig.GameTypeConfig = 4;
                    break;

                case "Open Pick":
                    gameConfig.GameTypeConfig = 5;
                    break;

                case "Blind Draft":
                    gameConfig.GameTypeConfig = 7;
                    break;

                case "Infinite Time Blind Pick":
                    gameConfig.GameTypeConfig = 11;
                    break;

                case "One for All":
                    gameConfig.GameTypeConfig = 14;
                    break;

                case "Captain Pick":
                    gameConfig.GameTypeConfig = 12;
                    break;

                default: //Tournament Draft
                    gameConfig.GameTypeConfig = 6;
                    break;
            }
            switch ((string)((Label)MapListBox.SelectedItem).Content)
            {
                case "The Crystal Scar":
                    gameConfig.GameMap = GameMap.TheCrystalScar;
                    gameConfig.GameMode = "ODIN";
                    break;

                case "Howling Abyss":
                    gameConfig.GameMap = GameMap.HowlingAbyss;
                    gameConfig.GameMode = "ARAM";
                    break;

                case "The Twisted Treeline":
                    gameConfig.GameMap = GameMap.TheTwistedTreeline;
                    gameConfig.GameMode = "CLASSIC";
                    if (gameConfig.MaxNumPlayers < 3)
                    {
                        NameInvalidLabel.Content = "Team size must be lower or equal to 3";
                        NameInvalidLabel.Visibility = Visibility.Visible;
                        CreateGameButton.IsEnabled = false;
                        return gameConfig;
                    }
                    break;

                default:
                    gameConfig.GameMap = GameMap.NewSummonersRift;
                    gameConfig.GameMode = "CLASSIC";
                    break;
            }
            switch ((string)AllowSpectatorsComboBox.SelectedItem)
            {
                case "None":
                    gameConfig.AllowSpectators = "NONE";
                    break;

                case "Lobby Only":
                    gameConfig.AllowSpectators = "LOBBYONLY";
                    break;

                case "Friends List Only":
                    gameConfig.AllowSpectators = "DROPINONLY";
                    break;

                default:
                    gameConfig.AllowSpectators = "ALL";
                    break;
            }
            CreateGameButton.IsEnabled = true;
            return gameConfig;
        }

        private void CreatedGame(GameDTO result)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (result.Name == null)
                {
                    NameInvalidLabel.Visibility = Visibility.Visible;
                    NameInvalidLabel.Content = "Name is already taken!";
                }
                else
                {
                    UserClient.GameID = result.Id;
                    UserClient.GameName = result.Name;
                    UserClient.GameLobbyDTO = result;
                    Client.ClearPage(typeof(FactionsGameLobbyPage));
                    Client.SwitchPage(new FactionsGameLobbyPage());
                }
            }));
        }

        private void GenerateSpectatorCode()
        {
            PracticeGameConfig gameConfig = GenerateGameConfig();
            TournamentCodeTextbox.Text = "pvpnet://lol/customgame/joinorcreate";
            TournamentCodeTextbox.Text += "/map" + gameConfig.GameMap.MapId;
            TournamentCodeTextbox.Text += "/pick" + gameConfig.GameTypeConfig;
            TournamentCodeTextbox.Text += "/team" + gameConfig.MaxNumPlayers * 0.5;
            TournamentCodeTextbox.Text += "/spec" + gameConfig.AllowSpectators;
            string json;
            if (string.IsNullOrEmpty(gameConfig.GamePassword))
                json = "{ \"name\": \"" + gameConfig.GameName + "\" }";
            else
                json = "{ \"name\": \"" + gameConfig.GameName + "\", \"password\": \"" + gameConfig.GamePassword +
                       "\" }";

            //Also "report" to get riot to send a report back, and "extra" to have data sent so you can identify it (passbackDataPacket)

            byte[] plainTextbytes = Encoding.UTF8.GetBytes(json);
            TournamentCodeTextbox.Text += "/" + Convert.ToBase64String(plainTextbytes);
        }

        private void WhitelistAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(WhiteListTextBox.Text))
                return;

            if (UserClient.Whitelist.Contains(WhiteListTextBox.Text.ToLower()))
                return;

            UserClient.Whitelist.Add(WhiteListTextBox.Text.ToLower());
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                WhitelistListBox.Items.Add(WhiteListTextBox.Text);
                WhiteListTextBox.Text = "";
            }));
        }

        private void WhitelistListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WhitelistListBox.SelectedIndex != -1)
                WhitelistRemoveButton.IsEnabled = true;
        }

        private void WhitelistRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (WhitelistListBox.SelectedIndex == -1)
                return;

            if (UserClient.Whitelist.Count == 1)
                WhitelistRemoveButton.IsEnabled = false;

            UserClient.Whitelist.Remove(WhitelistListBox.SelectedValue.ToString().ToLower());
            Dispatcher.BeginInvoke(DispatcherPriority.Input,
                new ThreadStart(() => WhitelistListBox.Items.Remove(WhitelistListBox.SelectedValue)));
        }

        private void EasyCreate_Checked(object sender, RoutedEventArgs e)
        {
            if (EasyCreate.IsChecked != null && (bool)EasyCreate.IsChecked)
            {
                NameTextBox.IsEnabled = false;
                PasswordTextBox.IsEnabled = false;
            }
            else
            {
                NameTextBox.IsEnabled = true;
                PasswordTextBox.IsEnabled = true;
            }
        }

        #region Spectator Code functions

        private void TeamSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count == 0)
                return;
            GenerateSpectatorCode();
        }

        private void AllowSpectatorsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count == 0)
                return;
            GenerateSpectatorCode();
        }

        private void GameTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count == 0)
                return;
            GenerateSpectatorCode();
        }

        private void MapListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count == 0)
                return;
            GenerateSpectatorCode();
            MapLabel.Content = ((Label)MapListBox.SelectedItem).Content;
        }

        private void PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            GenerateSpectatorCode();
        }

        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            GenerateSpectatorCode();
        }

        #endregion Spectator Code functions
    }
}