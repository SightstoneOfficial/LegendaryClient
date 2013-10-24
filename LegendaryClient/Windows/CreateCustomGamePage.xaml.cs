using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LegendaryClient.Logic;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Game.Map;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for CreateCustomGamePage.xaml
    /// </summary>
    public partial class CreateCustomGamePage : Page
    {
        public CreateCustomGamePage()
        {
            InitializeComponent();

            NameTextBox.Text = Client.LoginPacket.AllSummonerData.Summoner.Name + "'s game";
        }

        private void CreateGameButton_Click(object sender, RoutedEventArgs e)
        {
            NameInvalidLabel.Visibility = Visibility.Hidden;
            PracticeGameConfig gameConfig = new PracticeGameConfig();
            gameConfig.GameName = NameTextBox.Text;
            gameConfig.GamePassword = PasswordTextBox.Text;
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
                case "Blind Duplicate Pick (5X vs 5Y)":
                    gameConfig.GameTypeConfig = 14;
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
                    if (gameConfig.MaxNumPlayers > 6)
                    {
                        NameInvalidLabel.Content = "Team size must be lower or equal to 3";
                        NameInvalidLabel.Visibility = Visibility.Visible;
                        return;
                    }
                    break;
                default:
                    gameConfig.GameMap = GameMap.SummonersRift;
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
            Client.PVPNet.CreatePracticeGame(gameConfig, new GameDTO.Callback(CreatedGame));
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
                    Client.InGame = true;
                    Client.GameID = result.Id;
                    Client.GameName = result.Name;
                    Client.GameLobbyDTO = result;
                    Client.SwitchPage(new CustomGameLobbyPage());
                }
            }));
        }

    }
}
