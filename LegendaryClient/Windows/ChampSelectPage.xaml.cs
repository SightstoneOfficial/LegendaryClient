using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.PlayerSpell;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect.RiotObjects.Platform.Catalog.Champion;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Summoner.Masterybook;
using PVPNetConnect.RiotObjects.Platform.Summoner.Spellbook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for ChampSelectPage.xaml
    /// </summary>
    public partial class ChampSelectPage : Page
    {
        private bool BanningPhase = false;
        private GameDTO LatestDto;
        private ChampionDTO[] Champions;
        private MasteryBookDTO MyMasteries;
        private SpellBookDTO MyRunes;
        private List<ChampionDTO> MyChamps = new List<ChampionDTO>();
        private GameTypeConfigDTO configType;
        private System.Windows.Forms.Timer CountdownTimer;
        private int counter;
        private bool HasLockedIn = false;
        private bool DevMode = false;

        public ChampSelectPage()
        {
            InitializeComponent();
            StartChampSelect();
        }

        private async void StartChampSelect()
        {
            Client.FocusClient();
            Client.PVPNet.OnMessageReceived += ChampSelect_OnMessageReceived;
            Champions = await Client.PVPNet.GetAvailableChampions();
            MyMasteries = Client.LoginPacket.AllSummonerData.MasteryBook;
            MyRunes = Client.LoginPacket.AllSummonerData.SpellBook;

            int i = 0;
            foreach (MasteryBookPageDTO MasteryPage in MyMasteries.BookPages)
            {
                string MasteryPageName = MasteryPage.Name;
                if (MasteryPageName.StartsWith("@@"))
                {
                    MasteryPageName = "Mastery Page " + ++i;
                }
                MasteryComboBox.Items.Add(MasteryPageName);
                if (MasteryPage.Current)
                    MasteryComboBox.SelectedItem = MasteryPageName;
            }
            i = 0;
            foreach (SpellBookPageDTO RunePage in MyRunes.BookPages)
            {
                string RunePageName = RunePage.Name;
                if (RunePageName.StartsWith("@@"))
                {
                    RunePageName = "Rune Page " + ++i;
                }
                RuneComboBox.Items.Add(RunePageName);
                if (RunePage.Current)
                    RuneComboBox.SelectedItem = RunePageName;
            }

            await Client.PVPNet.SetClientReceivedGameMessage(Client.GameID, "CHAMP_SELECT_CLIENT");
            GameDTO latestDTO = await Client.PVPNet.GetLatestGameTimerState(Client.GameID, Client.ChampSelectDTO.GameState, Client.ChampSelectDTO.PickTurn);
            if (latestDTO.GameTypeConfigId < 1 || latestDTO.GameTypeConfigId > Client.LoginPacket.GameTypeConfigs.Count) //Invalid config... abort!
            {
                Client.PVPNet.OnMessageReceived -= ChampSelect_OnMessageReceived;
                await Client.PVPNet.QuitGame();
                Client.ClearPage(this);

                Client.SwitchPage(new MainPage());
                MessageOverlay overlay = new MessageOverlay();
                overlay.MessageTextBox.Text = "Invalid Config ID (" + latestDTO.GameTypeConfigId.ToString() + "). Report to Snowl [https://github.com/Snowl/LegendaryClient/issues/new]";
                overlay.MessageTitle.Content = "Invalid Config";
                Client.OverlayContainer.Content = overlay.Content;
                Client.OverlayContainer.Visibility = Visibility.Visible;
                return;
            }
            configType = Client.LoginPacket.GameTypeConfigs[latestDTO.GameTypeConfigId - 1];
            counter = configType.MainPickTimerDuration - 5; //Seems to be a 5 second inconsistancy with riot and what they actually provide
            CountdownTimer = new System.Windows.Forms.Timer();
            CountdownTimer.Tick += new EventHandler(CountdownTimer_Tick);
            CountdownTimer.Interval = 1000; // 1 second
            CountdownTimer.Start();

            ChampSelect_OnMessageReceived(this, latestDTO);

            LatestDto = latestDTO;

            ChampionSelectListView.Visibility = Visibility.Visible;
            AfterChampionSelectGrid.Visibility = Visibility.Hidden;

            List<ChampionDTO> champList = new List<ChampionDTO>(Champions);

            champList.Sort((x, y) => champions.GetChampion(x.ChampionId).displayName.CompareTo(champions.GetChampion(y.ChampionId).displayName));

            foreach (ChampionDTO champ in champList)
            {
                if (champ.Owned || champ.FreeToPlay)
                {
                    MyChamps.Add(champ);

                    //Add to ListView
                    ListViewItem item = new ListViewItem();
                    ChampionImage championImage = new ChampionImage();
                    var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champions.GetChampion(champ.ChampionId).iconPath), UriKind.Absolute);
                    championImage.ChampImage.Source = new BitmapImage(uriSource);
                    if (champ.FreeToPlay)
                        championImage.FreeToPlayLabel.Visibility = Visibility.Visible;
                    championImage.Width = 64;
                    championImage.Height = 64;
                    item.Tag = champ.ChampionId;
                    item.Content = championImage.Content;
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
            if (message.GetType() == typeof(GameDTO))
            {
                #region In Champion Select

                GameDTO ChampDTO = message as GameDTO;
                LatestDto = ChampDTO;
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async () =>
                {
                    List<Participant> AllParticipants = new List<Participant>(ChampDTO.TeamOne.ToArray());
                    AllParticipants.AddRange(ChampDTO.TeamTwo);
                    foreach (Participant p in AllParticipants)
                    {
                        if (p is PlayerParticipant)
                        {
                            PlayerParticipant play = (PlayerParticipant)p;
                            if (play.PickTurn == ChampDTO.PickTurn)
                            {
                                if (play.SummonerId == Client.LoginPacket.AllSummonerData.Summoner.SumId)
                                {
                                    ChampionSelectListView.IsHitTestVisible = true;
                                    ChampionSelectListView.Opacity = 1;
                                    GameStatusLabel.Content = "Your turn to pick!";
                                    break;
                                }
                            }
                        }
                        if (!DevMode)
                        {
                            ChampionSelectListView.IsHitTestVisible = false;
                            ChampionSelectListView.Opacity = 0.5;
                        }
                        GameStatusLabel.Content = "Waiting for others to pick...";
                    }

                    if (ChampDTO.GameState == "TEAM_SELECT")
                    {
                        if (CountdownTimer != null)
                        {
                            CountdownTimer.Stop();
                        }
                        Client.PVPNet.OnMessageReceived -= ChampSelect_OnMessageReceived;
                        Client.ClearPage(this);
                        FakePage fakePage = new FakePage();
                        fakePage.Content = Client.LastPageContent;
                        Client.SwitchPage(fakePage);
                        return;
                    }
                    else if (ChampDTO.GameState == "PRE_CHAMP_SELECT")
                    {
                        BanningPhase = true;
                        PurpleBansLabel.Visibility = Visibility.Visible;
                        BlueBansLabel.Visibility = Visibility.Visible;
                        BlueBanListView.Visibility = Visibility.Visible;
                        PurpleBanListView.Visibility = Visibility.Visible;
                        GameStatusLabel.Content = "Bans are on-going";

                        ChampionBanInfoDTO[] BannedChamps = await Client.PVPNet.GetChampionsForBan();

                        #region Render Bans
                        BlueBanListView.Items.Clear();
                        PurpleBanListView.Items.Clear();
                        foreach (var x in ChampDTO.BannedChampions)
                        {
                            Image champImage = new Image();
                            champImage.Height = 58;
                            champImage.Width = 58;
                            var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champions.GetChampion(x.ChampionId).iconPath), UriKind.Absolute);
                            champImage.Source = new BitmapImage(uriSource);
                            if (x.TeamId == 100)
                            {
                                BlueBanListView.Items.Add(champImage);
                            }
                            else
                            {
                                PurpleBanListView.Items.Add(champImage);
                            }
                        }
                        #endregion
                    }
                    else if (ChampDTO.GameState == "CHAMP_SELECT")
                    {
                        BanningPhase = false;
                    }
                    else if (ChampDTO.GameState == "POST_CHAMP_SELECT")
                    {
                        HasLockedIn = true;
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
                    int i = 0;
                    foreach (Participant participant in ChampDTO.TeamOne.ToArray()) //Clone array so it doesn't get modified
                    {
                        if (participant is PlayerParticipant)
                        {
                            bool DisplayedPlayer = false;
                            PlayerParticipant player = participant as PlayerParticipant;
                            foreach (PlayerChampionSelectionDTO selection in ChampDTO.PlayerChampionSelections)
                            {
                                if (selection.SummonerInternalName == player.SummonerInternalName)
                                {
                                    DisplayedPlayer = true;
                                    ChampSelectPlayer control = RenderPlayer(selection, player);
                                    BlueListView.Items.Add(control);
                                    if (HasLockedIn && selection.SummonerInternalName == Client.LoginPacket.AllSummonerData.Summoner.InternalName)
                                    {
                                        RenderLockInGrid(selection);
                                    }
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
                        else if (participant is ObfuscatedParticipant)
                        {
                            ChampSelectPlayer control = new ChampSelectPlayer();
                            control.PlayerName.Content = "Summoner " + ++i;
                            BlueListView.Items.Add(control);
                        }
                        else
                        {
                            ChampSelectPlayer control = new ChampSelectPlayer();
                            control.PlayerName.Content = "Unknown Summoner";
                            BlueListView.Items.Add(control);
                        }
                    }

                    foreach (Participant participant in ChampDTO.TeamTwo.ToArray()) //Clone array so it doesn't get modified
                    {
                        if (participant is PlayerParticipant)
                        {
                            bool DisplayedPlayer = false;
                            PlayerParticipant player = participant as PlayerParticipant;
                            foreach (PlayerChampionSelectionDTO selection in ChampDTO.PlayerChampionSelections)
                            {
                                if (selection.SummonerInternalName == player.SummonerInternalName)
                                {
                                    DisplayedPlayer = true;
                                    ChampSelectPlayer control = RenderPlayer(selection, player);
                                    PurpleListView.Items.Add(control);
                                    if (HasLockedIn && selection.SummonerInternalName == Client.LoginPacket.AllSummonerData.Summoner.InternalName)
                                    {
                                        RenderLockInGrid(selection);
                                    }
                                }
                            }
                            if (!DisplayedPlayer)
                            {
                                DisplayedPlayer = true;
                                ChampSelectPlayer control = new ChampSelectPlayer();
                                control.PlayerName.Content = player.SummonerName;
                                PurpleListView.Items.Add(control);
                            }
                        }
                        else if (participant is ObfuscatedParticipant)
                        {
                            ChampSelectPlayer control = new ChampSelectPlayer();
                            control.PlayerName.Content = "Summoner " + ++i;
                            BlueListView.Items.Add(control);
                        }
                        else
                        {
                            ChampSelectPlayer control = new ChampSelectPlayer();
                            control.PlayerName.Content = "Unknown Summoner";
                            BlueListView.Items.Add(control);
                        }
                    }

                    #endregion Display players
                }));

                #endregion In Champion Select
            }
            else if (message.GetType() == typeof(PlayerCredentialsDto))
            {
                #region Launching Game

                PlayerCredentialsDto dto = message as PlayerCredentialsDto;
                Client.CurrentGame = dto;
                Client.PVPNet.OnMessageReceived -= ChampSelect_OnMessageReceived;

                #endregion Launching Game
            }
        }

        internal void RenderLockInGrid(PlayerChampionSelectionDTO selection)
        {
            ChampionSelectListView.Visibility = Visibility.Hidden;
            AfterChampionSelectGrid.Visibility = Visibility.Visible;

            champions Champion = champions.GetChampion(selection.ChampionId);

            ChampNameLabel.Content = Champion.displayName;
            var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", Champion.portraitPath), UriKind.Absolute);
            ChampSplashImage.Source = new BitmapImage(uriSource);
            foreach (ChampionDTO champ in Champions)
            {
                if (champ.ChampionId == selection.ChampionId)
                {
                    foreach (ChampionSkinDTO skin in champ.ChampionSkins)
                    {
                        if (skin.Owned)
                        {
                            //SkinComboBox.Items.Add(Skins.GetSkinName(skin.SkinId).Replace("_", " "));
                        }
                    }
                }
            }
        }

        internal ChampSelectPlayer RenderPlayer(PlayerChampionSelectionDTO selection, PlayerParticipant player)
        {
            ChampSelectPlayer control = new ChampSelectPlayer();
            if (selection.ChampionId != 0)
            {
                var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champions.GetChampion(selection.ChampionId).iconPath), UriKind.Absolute);
                control.ChampionImage.Source = new BitmapImage(uriSource);
            }
            if (selection.Spell1Id != 0)
            {
                var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName((int)selection.Spell1Id)), UriKind.Absolute);
                control.SummonerSpell1.Source = new BitmapImage(uriSource);
                uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName((int)selection.Spell2Id)), UriKind.Absolute);
                control.SummonerSpell2.Source = new BitmapImage(uriSource);
            }
            if (player.SummonerName == Client.LoginPacket.AllSummonerData.Summoner.Name)
            {
                var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName((int)selection.Spell1Id)), UriKind.Absolute);
                SummonerSpell1Image.Source = new BitmapImage(uriSource);
                uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName((int)selection.Spell2Id)), UriKind.Absolute);
                SummonerSpell2Image.Source = new BitmapImage(uriSource);
            }
            control.PlayerName.Content = player.SummonerName;
            return control;
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

                        //TODO: Fix stupid animation glitch on left hand side
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

        private async void SkinSelectListView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private async void DodgeButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO - add messagebox
            await Client.PVPNet.QuitGame();
            Client.PVPNet.OnMessageReceived -= ChampSelect_OnMessageReceived;
            Client.ClearPage(new CustomGameLobbyPage());
            Client.ClearPage(new CreateCustomGamePage());
            Client.ClearPage(this);

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

        private void EditMasteriesButton_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Content = new MasteriesOverlay().Content;
            Client.OverlayContainer.Visibility = Visibility.Visible;
        }

        private void EditRunesButton_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Content = new RunesOverlay().Content;
            Client.OverlayContainer.Visibility = Visibility.Visible;
        }

        private void SummonerSpell_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Content = new SelectSummonerSpells(LatestDto.GameMode).Content;
            Client.OverlayContainer.Visibility = Visibility.Visible;
        }

        private void MasteryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void RuneComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            if (ChatTextBox.Text == "!~dev")
            {
                DevMode = !DevMode;
                ChampionSelectListView.IsHitTestVisible = true;
                ChampionSelectListView.Opacity = 1;
            }
            else
            {
                TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = Client.LoginPacket.AllSummonerData.Summoner.Name + ": ";
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = ChatTextBox.Text + Environment.NewLine;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                ChatTextBox.Text = "";
            }
        }
    }
}