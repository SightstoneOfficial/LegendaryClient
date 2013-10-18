using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect.RiotObjects.Platform.Catalog.Champion;
using PVPNetConnect.RiotObjects.Platform.Game;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for ChampSelectPage.xaml
    /// </summary>
    public partial class ChampSelectPage : Page
    {
        bool BanningPhase = false;
        ChampionDTO[] Champions;
        List<ChampionDTO> MyChamps = new List<ChampionDTO>();

        public ChampSelectPage()
        {
            InitializeComponent();
            StartChampSelect();
        }

        private async void StartChampSelect()
        {
            Client.PVPNet.OnMessageReceived += ChampSelect_OnMessageReceived;
            Champions = await Client.PVPNet.GetAvailableChampions();
            await Client.PVPNet.SetClientReceivedGameMessage(Client.GameID, "CHAMP_SELECT_CLIENT");
            GameDTO latestDTO = await Client.PVPNet.GetLatestGameTimerState(Client.GameID, Client.ChampSelectDTO.GameState, Client.ChampSelectDTO.PickTurn);
            ChampSelect_OnMessageReceived(this, latestDTO);

            foreach (ChampionDTO champ in Champions)
            {
                if (champ.Owned || champ.FreeToPlay)
                {
                    MyChamps.Add(champ);

                    //Add to ListView
                    ListViewItem item = new ListViewItem();
                    Image champImage = new Image();
                    champImage.Height = 58;
                    champImage.Width = 58;
                    var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champions.GetChampion(champ.ChampionId).iconPath), UriKind.Absolute);
                    champImage.Source = new BitmapImage(uriSource);
                    item.Content = champImage;
                    item.Tag = champ.ChampionId;
                    ChampionSelectListView.Items.Add(item);
                }
            }
        }

        private void ChampSelect_OnMessageReceived(object sender, object message)
        {
            if (message.GetType() == typeof (GameDTO))
            {
                #region In Champion Select
                GameDTO ChampDTO = message as GameDTO;
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    if (ChampDTO.GameState == "TEAM_SELECT")
                    {
                        Client.ClearPage(this);
                        CustomGameLobbyPage page = new CustomGameLobbyPage();
                        Client.SwitchPage(page, "");
                        return;
                    }
                    else if (ChampDTO.GameState == "PRE_CHAMP_SELECT")
                    {
                        //Banning
                    }
                    else if (ChampDTO.GameState == "CHAMP_SELECT")
                    {
                        ;
                    }
                    else if (ChampDTO.GameState == "POST_CHAMP_SELECT")
                    {

                    }
                    else if (ChampDTO.GameState == "START_REQUESTED")
                    {
                        DodgeButton.IsEnabled = false; //Cannot dodge past this point!
                    }
                    else
                    {
                        return;
                    }
                    #region Update Players REFACTOR THIS INSTANTLY
                    foreach (Participant participant in ChampDTO.TeamOne) 
                    {
                        try
                        {
                            PlayerParticipant player = participant as PlayerParticipant;
                            foreach (PlayerChampionSelectionDTO selection in ChampDTO.PlayerChampionSelections)
                            {
                                if (selection.SummonerInternalName == player.SummonerInternalName)
                                {
                                    var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champions.GetChampion(selection.ChampionId).iconPath), UriKind.Absolute); 
                                    switch (player.PickTurn)
                                    {
                                        case 1:
                                            BluePlayer1.ChampionImage.Source = new BitmapImage(uriSource);
                                            BluePlayer1.PlayerName.Content = player.SummonerName;
                                            BluePlayer1.Visibility = Visibility.Visible;
                                            break;
                                        case 2:
                                            BluePlayer2.ChampionImage.Source = new BitmapImage(uriSource);
                                            BluePlayer2.PlayerName.Content = player.SummonerName;
                                            BluePlayer2.Visibility = Visibility.Visible;
                                            break;
                                        case 3:
                                            BluePlayer3.ChampionImage.Source = new BitmapImage(uriSource);
                                            BluePlayer3.PlayerName.Content = player.SummonerName;
                                            BluePlayer3.Visibility = Visibility.Visible;
                                            break;
                                        case 4:
                                            BluePlayer4.ChampionImage.Source = new BitmapImage(uriSource);
                                            BluePlayer4.PlayerName.Content = player.SummonerName;
                                            BluePlayer4.Visibility = Visibility.Visible;
                                            break;
                                        case 5:
                                            BluePlayer5.ChampionImage.Source = new BitmapImage(uriSource);
                                            BluePlayer5.PlayerName.Content = player.SummonerName;
                                            BluePlayer5.Visibility = Visibility.Visible;
                                            break;
                                    }
                                    //node.Nodes.Add(SummonerSpell.GetSpellName((int)selection.Spell1Id));
                                    //node.Nodes.Add(SummonerSpell.GetSpellName((int)selection.Spell2Id));
                                }
                            }
                        }
                        catch
                        {
                            //Robert
                        }
                    }
                    foreach (Participant participant in ChampDTO.TeamTwo)
                    {
                        try
                        {
                            PlayerParticipant player = participant as PlayerParticipant;
                            foreach (PlayerChampionSelectionDTO selection in ChampDTO.PlayerChampionSelections)
                            {
                                if (selection.SummonerInternalName == player.SummonerInternalName)
                                {
                                    var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champions.GetChampion(selection.ChampionId).iconPath), UriKind.Absolute);
                                    switch (player.PickTurn)
                                    {
                                        case 1:
                                            PurplePlayer1.ChampionImage.Source = new BitmapImage(uriSource);
                                            PurplePlayer1.PlayerName.Content = player.SummonerName;
                                            PurplePlayer1.Visibility = Visibility.Visible;
                                            break;
                                        case 2:
                                            PurplePlayer2.ChampionImage.Source = new BitmapImage(uriSource);
                                            PurplePlayer2.PlayerName.Content = player.SummonerName;
                                            PurplePlayer2.Visibility = Visibility.Visible;
                                            break;
                                        case 3:
                                            PurplePlayer3.ChampionImage.Source = new BitmapImage(uriSource);
                                            PurplePlayer3.PlayerName.Content = player.SummonerName;
                                            PurplePlayer3.Visibility = Visibility.Visible;
                                            break;
                                        case 4:
                                            PurplePlayer4.ChampionImage.Source = new BitmapImage(uriSource);
                                            PurplePlayer4.PlayerName.Content = player.SummonerName;
                                            PurplePlayer4.Visibility = Visibility.Visible;
                                            break;
                                        case 5:
                                            PurplePlayer5.ChampionImage.Source = new BitmapImage(uriSource);
                                            PurplePlayer5.PlayerName.Content = player.SummonerName;
                                            PurplePlayer5.Visibility = Visibility.Visible;
                                            break;
                                    }
                                    //node.Nodes.Add(SummonerSpell.GetSpellName((int)selection.Spell1Id));
                                    //node.Nodes.Add(SummonerSpell.GetSpellName((int)selection.Spell2Id));
                                }
                            }
                        }
                        catch
                        {
                            //Robert
                        }
                    }
                    #endregion
                }));
                #endregion
            }
            else if (message.GetType() == typeof(PlayerCredentialsDto))
            {
                #region Launching Game
                PlayerCredentialsDto dto = message as PlayerCredentialsDto;
                Client.CurrentGame = dto;
                Client.PVPNet.OnMessageReceived -= ChampSelect_OnMessageReceived;
                #endregion
            }
        }

        private async void ListViewItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null)
            {
                if (!BanningPhase)
                {
                    if (item.Tag != null)
                    {
                        await Client.PVPNet.SelectChampion((int)item.Tag);
                        DoubleAnimation fadingAnimation = new DoubleAnimation();
                        fadingAnimation.From = 0.4;
                        fadingAnimation.To = 0;
                        fadingAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.2));
                        fadingAnimation.Completed += (eSender, eArgs) =>
                        {
                            var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champions.GetChampion((int)item.Tag).splashPath), UriKind.Absolute);
                            BackgroundSplash.Source = new BitmapImage(uriSource);
                            fadingAnimation = new DoubleAnimation();
                            fadingAnimation.From = 0;
                            fadingAnimation.To = 0.4;
                            fadingAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));

                            BackgroundSplash.BeginAnimation(Image.OpacityProperty, fadingAnimation);
                        };

                        BackgroundSplash.BeginAnimation(Image.OpacityProperty, fadingAnimation);
                        /*SkinComboBox.Items.Clear();
                        SkinComboBox.Text = "";
                        foreach (ChampionDTO champ in Champions)
                        {
                            if (champ.ChampionId == (int)e.Item.Tag)
                            {
                                foreach (ChampionSkinDTO skin in champ.ChampionSkins)
                                {
                                    if (skin.Owned)
                                    {
                                        SkinComboBox.Items.Add(Skins.GetSkinName(skin.SkinId).Replace("_", " "));
                                    }
                                }
                            }
                        }*/
                    }
                }
                else
                {
                    if (item.Tag != null)
                    {
                        await Client.PVPNet.BanChampion((int)item.Tag);
                    }
                }
            }
        }

        private async void DodgeButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO - add messagebox
            await Client.PVPNet.QuitGame();
            Client.PVPNet.OnMessageReceived -= ChampSelect_OnMessageReceived;
            CustomGameLobbyPage clearPage = new CustomGameLobbyPage(); //Clear pages
            Client.ClearPage(clearPage);
            CreateCustomGamePage clearPage2 = new CreateCustomGamePage();
            Client.ClearPage(clearPage2);
            ChampSelectPage clearPage3 = new ChampSelectPage();
            Client.ClearPage(clearPage3);

            MainPage MainPage = new MainPage();
            Client.SwitchPage(MainPage, "");
        }

        private async void LockInButton_Click(object sender, RoutedEventArgs e)
        {
            if (ChampionSelectListView.SelectedItems.Count > 0)
            {
                await Client.PVPNet.ChampionSelectCompleted();
            }
        }
    }
}
