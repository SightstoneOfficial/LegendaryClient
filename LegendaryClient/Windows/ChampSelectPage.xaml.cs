using jabber.connection;
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
        private bool HasLaunchedGame = false;
        private Room Chatroom;

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
                    MasteryComboBox.SelectedItem = MasteryPageName + " "; //Dont waste two calls at the start of champ select
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
                    RuneComboBox.SelectedItem = RunePageName + " ";
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
            configType = Client.LoginPacket.GameTypeConfigs.Find(x => x.Id == latestDTO.GameTypeConfigId);
            counter = configType.MainPickTimerDuration - 5; //Seems to be a 5 second inconsistancy with riot and what they actually provide
            CountdownTimer = new System.Windows.Forms.Timer();
            CountdownTimer.Tick += new EventHandler(CountdownTimer_Tick);
            CountdownTimer.Interval = 1000; // 1 second
            CountdownTimer.Start();

            ChampSelect_OnMessageReceived(this, latestDTO);

            LatestDto = latestDTO;

            string JID = Client.GetChatroomJID(latestDTO.RoomName.Replace("@sec", ""), latestDTO.RoomPassword, false);
            Chatroom = Client.ConfManager.GetRoom(new jabber.JID(JID));
            Chatroom.Nickname = Client.LoginPacket.AllSummonerData.Summoner.Name;
            Chatroom.OnRoomMessage += Chatroom_OnRoomMessage;
            Chatroom.OnParticipantJoin += Chatroom_OnParticipantJoin;
            Chatroom.Join(latestDTO.RoomPassword);

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
                    ListViewItem[] ChampionArray = new ListViewItem[ChampionSelectListView.Items.Count];
                    ChampionSelectListView.Items.CopyTo(ChampionArray, 0);
                    foreach (ListViewItem y in ChampionArray)
                    {
                        y.IsHitTestVisible = true;
                        y.Opacity = 1;
                    }

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
                        counter = configType.BanTimerDuration;

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

                            foreach (ListViewItem y in ChampionArray)
                            {
                                if ((int)y.Tag == x.ChampionId)
                                {
                                    ChampionSelectListView.Items.Remove(y);
                                }
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
                    bool PurpleSide = false;
                    foreach (Participant participant in AllParticipants)
                    {
                        i++;
                        ChampSelectPlayer control = new ChampSelectPlayer();
                        if (participant is PlayerParticipant)
                        {
                            PlayerParticipant player = participant as PlayerParticipant;
                            foreach (PlayerChampionSelectionDTO selection in ChampDTO.PlayerChampionSelections)
                            {
                                #region Disable picking selected champs
                                foreach (ListViewItem y in ChampionArray)
                                {
                                    if ((int)y.Tag == selection.ChampionId)
                                    {
                                        y.IsHitTestVisible = false;
                                        y.Opacity = 0.5;
                                        if (configType != null)
                                        {
                                            if (configType.DuplicatePick)
                                            {
                                                y.IsHitTestVisible = true;
                                                y.Opacity = 1;
                                            }
                                        }
                                    }
                                }
                                #endregion

                                if (selection.SummonerInternalName == player.SummonerInternalName)
                                {
                                    control = RenderPlayer(selection, player);
                                    if (HasLockedIn && selection.SummonerInternalName == Client.LoginPacket.AllSummonerData.Summoner.InternalName)
                                    {
                                        RenderLockInGrid(selection);
                                    }
                                }
                            }
                        }
                        else if (participant is ObfuscatedParticipant)
                        {
                            control.PlayerName.Content = "Summoner " + i;
                        }
                        else if (participant is BotParticipant)
                        {
                            BotParticipant bot = participant as BotParticipant;
                            string botChamp = bot.SummonerName.Split(' ')[0]; //Why is this internal name rito?
                            champions botSelectedChamp = champions.GetChampion(botChamp);
                            PlayerParticipant part = new PlayerParticipant();
                            PlayerChampionSelectionDTO selection = new PlayerChampionSelectionDTO();
                            selection.ChampionId = botSelectedChamp.id;
                            part.SummonerName = botSelectedChamp.displayName + " bot";
                            control = RenderPlayer(selection, part);
                        }
                        else
                        {
                            control.PlayerName.Content = "Unknown Summoner";
                        }
                        if (i > ChampDTO.TeamOne.Count)
                        {
                            i = 0;
                            PurpleSide = true;
                        }

                        if (!PurpleSide)
                        {
                            BlueListView.Items.Add(control);
                        }
                        else
                        {
                            PurpleListView.Items.Add(control);
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

                if (!HasLaunchedGame)
                {
                    HasLaunchedGame = true;
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        Client.SwitchPage(new MainPage());
                        if (CountdownTimer != null)
                        {
                            CountdownTimer.Stop();
                        }
                        Client.PVPNet.OnMessageReceived -= ChampSelect_OnMessageReceived;
                        Client.ClearPage(this);
                    }));
                    Client.LaunchGame();
                }
                #endregion Launching Game
            }
        }

        internal void RenderLockInGrid(PlayerChampionSelectionDTO selection)
        {
            ChampionSelectListView.Visibility = Visibility.Hidden;
            AfterChampionSelectGrid.Visibility = Visibility.Visible;

            LockInButton.Content = "Locked In";

            champions Champion = champions.GetChampion(selection.ChampionId);

            SkinSelectListView.Items.Clear();

            //Render default skin
            ListViewItem item = new ListViewItem();
            Image skinImage = new Image();
            var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", Champion.portraitPath), UriKind.Absolute);
            skinImage.Source = new BitmapImage(uriSource);
            skinImage.Width = 191;
            skinImage.Stretch = Stretch.UniformToFill;
            item.Tag = "0:" + Champion.id; //Hack
            item.Content = skinImage;
            SkinSelectListView.Items.Add(item);

            List<championAbilities> Abilities = championAbilities.GetAbilities(selection.ChampionId);
            foreach (championAbilities ability in Abilities)
            {
                ChampionAbility championAbility = new ChampionAbility();
                uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "abilities", ability.iconPath), UriKind.Absolute);
                championAbility.AbilityImage.Source = new BitmapImage(uriSource);
                championAbility.AbilityHotKey.Content = ability.hotkey;
                championAbility.AbilityName.Content = ability.name;
                championAbility.AbilityDescription.Text = ability.description;
                championAbility.Width = 375;
                championAbility.Height = 75;
                AbilityListView.Items.Add(championAbility);
            }

            foreach (ChampionDTO champ in Champions)
            {
                if (champ.ChampionId == selection.ChampionId)
                {
                    foreach (ChampionSkinDTO skin in champ.ChampionSkins)
                    {
                        if (skin.Owned)
                        {
                            item = new ListViewItem();
                            skinImage = new Image();
                            uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", championSkins.GetSkin(skin.SkinId).portraitPath), UriKind.Absolute);
                            skinImage.Source = new BitmapImage(uriSource);
                            skinImage.Width = 191;
                            skinImage.Stretch = Stretch.UniformToFill;
                            item.Tag = skin.SkinId;
                            item.Content = skinImage;
                            SkinSelectListView.Items.Add(item);
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
            var item = sender as ListViewItem;
            if (item != null)
            {
                if (item.Tag != null)
                {
                    if (item.Tag is string)
                    {
                        string[] splitItem = ((string)item.Tag).Split(':');
                        int championId = Convert.ToInt32(splitItem[1]);
                        champions Champion = champions.GetChampion(championId);
                        await Client.PVPNet.SelectChampionSkin(championId, 0);
                        TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                        tr.Text = "Selected Default " + Champion.name + " as skin" + Environment.NewLine;
                        tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                    }
                    else
                    {
                        championSkins skin = championSkins.GetSkin((int)item.Tag);
                        await Client.PVPNet.SelectChampionSkin(skin.championId, skin.id);
                        TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                        tr.Text = "Selected " + skin.name + " as skin" + Environment.NewLine;
                        tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                    }
                }
            }
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

        private async void MasteryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool HasChanged = false;
            int i = 0;
            MasteryBookDTO bookDTO = new MasteryBookDTO();
            bookDTO.SummonerId = Client.LoginPacket.AllSummonerData.Summoner.SumId;
            bookDTO.BookPages = new List<MasteryBookPageDTO>();
            foreach (MasteryBookPageDTO MasteryPage in MyMasteries.BookPages)
            {
                string MasteryPageName = MasteryPage.Name;
                if (MasteryPageName.StartsWith("@@"))
                {
                    MasteryPageName = "Mastery Page " + ++i;
                }
                MasteryPage.Current = false;
                if (MasteryPageName == (string)MasteryComboBox.SelectedItem)
                {
                    MasteryPage.Current = true;
                    HasChanged = true;
                    TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                    tr.Text = "Selected " + MasteryPageName + " as Mastery Page" + Environment.NewLine;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                }
                bookDTO.BookPages.Add(MasteryPage);
            }
            if (HasChanged)
            {
                await Client.PVPNet.SaveMasteryBook(bookDTO);
            }
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
                TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = "DEV MODE: " + DevMode;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
            }
            else
            {
                if (DevMode)
                {
                    if (ChatTextBox.Text == "!~champ")
                    {
                        ChampionSelectListView.Visibility = Visibility.Visible;
                        AfterChampionSelectGrid.Visibility = Visibility.Hidden;
                    }
                    else if (ChatTextBox.Text == "!~skin")
                    {
                        ChampionSelectListView.Visibility = Visibility.Hidden;
                        AfterChampionSelectGrid.Visibility = Visibility.Visible;
                    }
                    return;
                }
                TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = Client.LoginPacket.AllSummonerData.Summoner.Name + ": ";
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = ChatTextBox.Text + Environment.NewLine;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                Chatroom.PublicMessage(ChatTextBox.Text);
                ChatTextBox.Text = "";
            }
        }

        void Chatroom_OnRoomMessage(object sender, jabber.protocol.client.Message msg)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (msg.Body != "This room is not anonymous")
                {
                    TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                    tr.Text = msg.From.Resource + ": ";
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);
                    tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                    tr.Text = msg.InnerText.Replace("<![CDATA[", "").Replace("]]>", "") + Environment.NewLine;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                }
            }));
        }

        void Chatroom_OnParticipantJoin(Room room, RoomParticipant participant)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = participant.Nick + " joined the room." + Environment.NewLine;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
            }));
        }
    }
}