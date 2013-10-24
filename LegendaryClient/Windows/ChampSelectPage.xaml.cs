using System;
using System.Linq;
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
using LegendaryClient.Controls;

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
        GameTypeConfigDTO configType;
        System.Windows.Forms.Timer CountdownTimer;
        int counter;
        bool HasLockedIn = false;

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
            if (latestDTO.GameTypeConfigId < 1) //Invalid config... abort!
            {
                Client.PVPNet.OnMessageReceived -= ChampSelect_OnMessageReceived;
                return;
            }
            configType = Client.LoginPacket.GameTypeConfigs[latestDTO.GameTypeConfigId - 1];
            counter = configType.MainPickTimerDuration - 5; //Seems to be a 5 second inconsistancy with riot and what they actually provide
            CountdownTimer = new System.Windows.Forms.Timer();
            CountdownTimer.Tick += new EventHandler(CountdownTimer_Tick);
            CountdownTimer.Interval = 1000; // 1 second
            CountdownTimer.Start();

            ChampSelect_OnMessageReceived(this, latestDTO);
            ChatText.Text = "test" + Environment.NewLine + "yolo";


            List<ChampionDTO> champList = new List<ChampionDTO>(Champions);

            foreach (ChampionDTO champ in champList)
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

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            counter--;
            if (counter <= 0)
                return;
            LobbyTimeLabel.Content = counter;
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
                        if (CountdownTimer != null)
                        {
                            CountdownTimer.Stop();
                        }
                        Client.PVPNet.OnMessageReceived -= ChampSelect_OnMessageReceived;
                        Client.ClearPage(this);
                        Client.SwitchPage(new CustomGameLobbyPage());
                        return;
                    }
                    else if (ChampDTO.GameState == "PRE_CHAMP_SELECT")
                    {
                        //Banning
                    }
                    else if (ChampDTO.GameState == "CHAMP_SELECT")
                    {
                        if (!HasLockedIn)
                        {
                            GameStatusLabel.Content = "Your turn to pick!";
                        }
                        else
                        {
                            GameStatusLabel.Content = "Waiting for others to pick...";
                        }
                    }
                    else if (ChampDTO.GameState == "POST_CHAMP_SELECT")
                    {
                        GameStatusLabel.Content = "All players have picked!";
                        counter = 10;
                    }
                    else if (ChampDTO.GameState == "START_REQUESTED")
                    {
                        GameStatusLabel.Content = "The game is about to start!";
                        DodgeButton.IsEnabled = false; //Cannot dodge past this point!
                    }

                    #region Display players
                    BlueListView.Items.Clear();
                    PurpleListView.Items.Clear();
                    foreach (Participant participant in ChampDTO.TeamOne.ToArray()) //Clone array so it doesn't get modified
                    {
                        try
                        {
                            bool DisplayedPlayer = false;
                            PlayerParticipant player = participant as PlayerParticipant;
                            foreach (PlayerChampionSelectionDTO selection in ChampDTO.PlayerChampionSelections)
                            {
                                if (selection.SummonerInternalName == player.SummonerInternalName)
                                {
                                    DisplayedPlayer = true;
                                    ChampSelectPlayer control = new ChampSelectPlayer();
                                    if (selection.ChampionId != 0)
                                    {
                                        var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champions.GetChampion(selection.ChampionId).iconPath), UriKind.Absolute);
                                        control.ChampionImage.Source = new BitmapImage(uriSource);
                                    }
                                    control.PlayerName.Content = player.SummonerName;
                                    BlueListView.Items.Add(control);
                                }
                            }
                            if (!DisplayedPlayer)
                            {
                                DisplayedPlayer = true;
                                ChampSelectPlayer control = new ChampSelectPlayer();
                                control.PlayerName.Content = player.SummonerName;
                                BlueListView.Items.Add(control);
                            }
                        }
                        catch
                        {
                            //Robert
                        }
                    }

                    foreach (Participant participant in ChampDTO.TeamTwo.ToArray()) //Clone array so it doesn't get modified
                    {
                        try
                        {
                            bool DisplayedPlayer = false;
                            PlayerParticipant player = participant as PlayerParticipant;
                            foreach (PlayerChampionSelectionDTO selection in ChampDTO.PlayerChampionSelections)
                            {
                                if (selection.SummonerInternalName == player.SummonerInternalName)
                                {
                                    DisplayedPlayer = true;
                                    ChampSelectPlayer control = new ChampSelectPlayer();
                                    if (selection.ChampionId != 0)
                                    {
                                        var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champions.GetChampion(selection.ChampionId).iconPath), UriKind.Absolute);
                                        control.ChampionImage.Source = new BitmapImage(uriSource);
                                    }
                                    control.PlayerName.Content = player.SummonerName;
                                    PurpleListView.Items.Add(control);
                                }
                            }
                            if (!DisplayedPlayer)
                            {
                                DisplayedPlayer = true;
                                ChampSelectPlayer control = new ChampSelectPlayer();
                                control.PlayerName.Content = player.SummonerName;
                                BlueListView.Items.Add(control);
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
            Client.ClearPage(new CustomGameLobbyPage()); //Clear pages
            Client.ClearPage(new CreateCustomGamePage());
            Client.ClearPage(new ChampSelectPage());

            Client.SwitchPage(new MainPage());
        }

        private async void LockInButton_Click(object sender, RoutedEventArgs e)
        {
            if (ChampionSelectListView.SelectedItems.Count > 0)
            {
                await Client.PVPNet.ChampionSelectCompleted();
                HasLockedIn = true;
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
