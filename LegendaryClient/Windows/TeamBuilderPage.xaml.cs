using jabber.connection;
using jabber.protocol.client;
using LegendaryClient.Controls.TeamBuilder;
using LegendaryClient.Logic;
using LegendaryClient.Logic.PlayerSpell;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect.RiotObjects.Platform.Catalog.Champion;
using PVPNetConnect.RiotObjects.Platform.Summoner.Masterybook;
using PVPNetConnect.RiotObjects.Platform.Summoner.Spellbook;
using PVPNetConnect.RiotObjects.Platform.ServiceProxy.Dispatch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using Newtonsoft.Json;
using System.Timers;
using LegendaryClient.Controls;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for TeamBuilder.xaml
    /// </summary>
    public partial class TeamBuilderPage : Page
    {
        private Room newRoom;
        internal int ChampionId = 0;

        /// <summary>
        /// Possible roles:
        /// MAGE
        /// SUPPORT
        /// ASSASSIN
        /// MARKSMAN
        /// FIGHTER
        /// TANK
        /// </summary>
        internal string role;

        /// <summary>
        /// TOP
        /// MIDDLE
        /// BOTTOM
        /// JUNGLE
        /// </summary>
        internal string position;       
        
        internal int skinId;

        internal bool connectedToChat = false;

        internal int spell1;
        internal int spell2;

        private List<ChampionDTO> ChampList;
        private MasteryBookDTO MyMasteries;
        private SpellBookDTO MyRunes;    
        

        internal string teambuilderGroupId;
        internal int teambuilderSlotId;
        internal int teambuilderCandidateAutoQuitTimeout;
                
        //TeamBuilder is just a little insane. This code is very messy too. :P
        public TeamBuilderPage(bool iscreater)
        {
            InitializeComponent();
            if (iscreater == false)
            {
                Invite.IsEnabled = false;
            }
            //Start teambuilder
            CallWithArgs(Guid.NewGuid().ToString(), "cap", "retrieveFeatureToggles", "{}");
            MyMasteries = Client.LoginPacket.AllSummonerData.MasteryBook;
            MyRunes = Client.LoginPacket.AllSummonerData.SpellBook;
            //StartTeambuilder();
            LoadStats();
            
            Client.PVPNet.OnMessageReceived += PVPNet_OnMessageReceived;
            AddPlayer();
        }

        /// <summary>
        /// Use this to connect to chat
        /// </summary>
        /// <param name="ChatJID"></param>
        /// <param name="Pass"></param>
        private void ConenctToChat(string ChatJID, string Pass)
        {
            string JID = Client.GetChatroomJID(ChatJID, Pass, false);
            newRoom = Client.ConfManager.GetRoom(new jabber.JID(JID));
            newRoom.Nickname = Client.LoginPacket.AllSummonerData.Summoner.Name;
            newRoom.OnRoomMessage += newRoom_OnRoomMessage;
            newRoom.OnParticipantJoin += newRoom_OnParticipantJoin;
            newRoom.Join(Pass);
            connectedToChat = true;
        }
        private void LeaveChat()
        {
            newRoom.Leave("Player Quit TeamBuilder Lobby");
            //We no longer want to receive messages from teambuilder chat lobby if we want to leave that team
            newRoom.OnRoomMessage -= newRoom_OnRoomMessage;
            newRoom.OnParticipantJoin -= newRoom_OnParticipantJoin;
        }
        
        private void PVPNet_OnMessageReceived(object sender, object message)
        {
            if(message.GetType() == typeof(LcdsServiceProxyResponse))
            {
                LcdsServiceProxyResponse ProxyResponse = message as LcdsServiceProxyResponse;
                HandleProxyResponse(ProxyResponse);
            }
        }

        /// <summary>
        /// Used to start queuing For SOLO [yay riot]
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartQueue(object sender, RoutedEventArgs e)
        {
            string roleUp = string.Format(role.ToUpper());
            string posUp = string.Format(position.ToUpper());
            string Json = string.Format("\"skinId\":{0},\"position\":\"{1}\",\"role\":\"{2}\",\"championId\":{3},\"spell2Id\":{4},\"queueId\":61,\"spell1Id\":{5}", skinId, posUp, roleUp, ChampionId, spell2, spell1);
            string JsonWithBrackets = "{" + Json + "}";
            CallWithArgs(Guid.NewGuid().ToString(), "cap", "createSoloQueryV4", JsonWithBrackets);
        }

        private System.Timers.Timer CountdownTimer;

        private int TimeLeft = 10;

        private void HandleProxyResponse(LcdsServiceProxyResponse Response)
        {
            //Received an acceptance for a teambuilder group. Daymn I'm so smart to not include V2 or whatever it is [so this will work later on]
            if(Response.MethodName == "acceptedByGroupV2")
            {
                TimeLeft = 10;
                ReceivedGroupId m = JsonConvert.DeserializeObject<ReceivedGroupId>(Response.Payload);
                teambuilderCandidateAutoQuitTimeout = m.candidateAutoQuitTimeout;
                teambuilderGroupId = m.groupId;
                teambuilderSlotId = m.slotId;
                MatchFoundGrid.Visibility = Visibility.Visible;

                TimeLeft = teambuilderCandidateAutoQuitTimeout;
                
                CountdownTimer = new System.Timers.Timer(1000);
                CountdownTimer.Elapsed += new ElapsedEventHandler(QueueElapsed);
                CountdownTimer.Enabled = true;
            }
        }

        private void QueueElapsed(object sender, ElapsedEventArgs e)
        {
            if (TimeLeft > 0)
            {

            }
        }

        //This is something you don't know exists
        private void RenderLegenadryClientPlayerSumSpellIcons()
        {
            if (ChampionId != 0)
            {
                TeamPlayer.Champion.Source = champions.GetChampion(ChampionId).icon;
            }
            string uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName((int)spell1));
            TeamPlayer.SummonerSpell1Image.Source = Client.GetImage(uriSource);
            uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName((int)spell2));
            TeamPlayer.SummonerSpell2Image.Source = Client.GetImage(uriSource);
        }

        
        TeamBuilderChoose TeamPlayer = new TeamBuilderChoose();
        private void AddPlayer(bool inNotInTeam = true)
        {
            ///WHY ARE THERE SO MANY ROLES NOW

            TeamPlayer.Position.Items.Add(new Item("Mage"));
            TeamPlayer.Position.Items.Add(new Item("Support"));
            TeamPlayer.Position.Items.Add(new Item("Assassin"));
            TeamPlayer.Position.Items.Add(new Item("Marksman"));
            TeamPlayer.Position.Items.Add(new Item("Fighter"));
            TeamPlayer.Position.Items.Add(new Item("Tank"));

            TeamPlayer.Role.Items.Add(new Item("Top"));
            TeamPlayer.Role.Items.Add(new Item("Middle"));
            TeamPlayer.Role.Items.Add(new Item("Bottom"));
            TeamPlayer.Role.Items.Add(new Item("Jungle"));


            //So many calls. We have to use this instead of putting the control right in because we need to put the player into the correct possition
            TeamPlayer.EditMasteries.Click += EditMasteriesButton_Click;
            TeamPlayer.EditRunes.Click += EditRunesButton_Click;

            TeamPlayer.SummonerSpell1.Click += SummonerSpell_Click;
            TeamPlayer.SummonerSpell2.Click += SummonerSpell_Click;

            TeamPlayer.Position.SelectionChanged += TeamPlayer_SelectionChanged;
            TeamPlayer.Role.SelectionChanged += TeamRole_SelectionChanged;
            TeamPlayer.RunePage.SelectionChanged += RunePage_SelectionChanged;
            TeamPlayer.MasteryPage.SelectionChanged += MasteryPage_SelectionChanged;

            TeamPlayer.SelectChampion.MouseDown += Champion_MouseDown;


            

            int i = 0;
            foreach (MasteryBookPageDTO MasteryPage in MyMasteries.BookPages)
            {
                string MasteryPageName = MasteryPage.Name;
                //Stop garbage mastery names
                if (MasteryPageName.StartsWith("@@"))
                {
                    MasteryPageName = "Mastery Page " + ++i;
                }
                TeamPlayer.MasteryPage.Items.Add(MasteryPageName);
                if (MasteryPage.Current)
                    TeamPlayer.MasteryPage.SelectedValue = MasteryPageName;
            }
            i = 0;
            foreach (SpellBookPageDTO RunePage in MyRunes.BookPages)
            {
                string RunePageName = RunePage.Name;
                //Stop garbage rune names
                if (RunePageName.StartsWith("@@"))
                {
                    RunePageName = "Rune Page " + ++i;
                }
                TeamPlayer.RunePage.Items.Add(RunePageName);
                if (RunePage.Current)
                    TeamPlayer.RunePage.SelectedValue = RunePageName;
            }

            PlayerListView.Items.Add(TeamPlayer);

            if(ChampionId != 0)
            {
                StartTeambuilder();
            }
        }


        private void Champion_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ChampAndSkinGrid.Visibility = Visibility.Visible;
        }

        private void LockIn_Click(object sender, RoutedEventArgs e)
        {
            StartTeambuilder();
            ChampAndSkinGrid.Visibility = Visibility.Hidden;
        }

        private void SelectSkin(int skin)
        {
            skinId = skin;
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
                        //await Client.PVPNet.SelectChampionSkin(skin.championId, skin.id);
                        skinId = skin.id;
                        TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                        tr.Text = "Selected " + skin.displayName + " as skin" + Environment.NewLine;
                        tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                    }
                }
            }
        }

        private void SummonerSpell_Click(object sender, RoutedEventArgs e)
        {
            SelectSummonerSpells("");
            SpellsGrid.Visibility = Visibility.Visible;
        }

        private void AcceptGame_Click(object sender, RoutedEventArgs e)
        {
            //Super Op Code
            string LastArg = string.Format("{\"acceptance\":{0},\"slotId\":{1},\"groupId\":\"{2}\"}", true, teambuilderSlotId, teambuilderGroupId);
            CallWithArgs(Guid.NewGuid().ToString(), "cap", "indicateGroupAcceptanceAsCandidateV1", LastArg);
            MatchFoundGrid.Visibility = Visibility.Hidden;
        }
        
        private void TeamPlayer_SelectionChanged(object sender, EventArgs e)
        {            
            Item itm = (Item)TeamPlayer.Position.SelectedItem;
            position = itm.ComboRole;
            SelectedAllChamps();
        }

        bool QuickLoad = true;

        private async void MasteryPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!QuickLoad) //Make loading quicker
                return;

            bool HasChanged = false;
            int i = 0;
            MasteryBookDTO bookDTO = new MasteryBookDTO();
            bookDTO.SummonerId = Client.LoginPacket.AllSummonerData.Summoner.SumId;
            bookDTO.BookPages = new List<MasteryBookPageDTO>();
            foreach (MasteryBookPageDTO MasteryPage in MyMasteries.BookPages)
            {
                string MasteryPageName = MasteryPage.Name;
                //Convert garbage to readable so we get the proper mastery page
                if (MasteryPageName.StartsWith("@@"))
                {
                    MasteryPageName = "Mastery Page " + ++i;
                }
                MasteryPage.Current = false;
                if (MasteryPageName == (string)TeamPlayer.MasteryPage.SelectedItem)
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

        private async void RunePage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!QuickLoad) //Make loading quicker
                return;

            SpellBookPageDTO SelectedRunePage = new SpellBookPageDTO();
            int i = 0;
            bool HasChanged = false;
            foreach (SpellBookPageDTO RunePage in MyRunes.BookPages)
            {
                string RunePageName = RunePage.Name;
                if (RunePageName.StartsWith("@@"))
                {
                    RunePageName = "Rune Page " + ++i;
                }
                RunePage.Current = false;
                if (RunePageName == (string)TeamPlayer.RunePage.SelectedItem)
                {
                    RunePage.Current = true;
                    SelectedRunePage = RunePage;
                    HasChanged = true;
                    TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                    tr.Text = "Selected " + RunePageName + " as Rune Page" + Environment.NewLine;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                }
            }
            if (HasChanged)
            {
                await Client.PVPNet.SelectDefaultSpellBookPage(SelectedRunePage);
            }
        }

        private void TeamRole_SelectionChanged(object sender, EventArgs e)
        {
            Item itm = (Item)TeamPlayer.Role.SelectedItem;
            role = itm.ComboRole;
            SelectedAllChamps();
        }

        private void SelectedAllChamps()
        {
            /*
            //We only want this to be called when selected champs and role and position have a set value
            if(role != null && position != null && ChampionId != 0)
            {
                //Lazyist way to do this, but is probably is the shortest
                string roleUp = string.Format(role.ToUpper());
                string posUp = string.Format(position.ToUpper());
                string Json = string.Format("{\"role\":\"{0}\",\"position\":\"{1}\",\"queueId\":61,\"championId\":{2}", roleUp, posUp, ChampionId);
                CallWithArgs(Guid.NewGuid().ToString(), "cap", "retrieveEstimatedWaitTimeV2", Json);
            }//*/
        }

        private void StartTeambuilder()
        {            
            ListViewItem item = new ListViewItem();
            Image skinImage = new Image();
            ChampList = new List<ChampionDTO>(Client.PlayerChampions);
            champions Champion = champions.GetChampion(ChampionId);

            string uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "champions", Champion.portraitPath);

            //Retrieve masteries and runes
            MyMasteries = Client.LoginPacket.AllSummonerData.MasteryBook;
            MyRunes = Client.LoginPacket.AllSummonerData.SpellBook;

            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                //Allow all champions to be selected (reset our modifications)
                ListViewItem[] ChampionArray = new ListViewItem[ChampionSelectListView.Items.Count];
                ChampionSelectListView.Items.CopyTo(ChampionArray, 0);
                foreach (ListViewItem y in ChampionArray)
                {
                    y.IsHitTestVisible = true;
                    y.Opacity = 1;
                }
            }));

            foreach (ChampionDTO champ in ChampList)
            {
                if (champ.ChampionId == ChampionId)
                {
                    foreach (ChampionSkinDTO skin in champ.ChampionSkins)
                    {
                        if (skin.Owned)
                        {
                            item = new ListViewItem();
                            skinImage = new Image();
                            uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "champions", championSkins.GetSkin(skin.SkinId).portraitPath);
                            skinImage.Source = Client.GetImage(uriSource);
                            skinImage.Width = 191;
                            skinImage.Stretch = Stretch.UniformToFill;
                            item.Tag = skin.SkinId;
                            item.Content = skinImage;
                            SkinSelectListView.Items.Add(item);
                        }
                    }
                }
            }

            skinImage.Source = Client.GetImage(uriSource);
            skinImage.Width = 191;
            skinImage.Stretch = Stretch.UniformToFill;
            item.Tag = "0:" + ChampionId; //Hack
            item.Content = skinImage;
            SkinSelectListView.Items.Add(item);
        }

        private void LoadStats()
        {
            string hi = "hi";
            ChampionSelectListView.Items.Clear();
            if (hi == "hi")
            {
                ChampList = new List<ChampionDTO>(Client.PlayerChampions);
                foreach (ChampionDTO champ in ChampList)
                {
                    champions getChamp = champions.GetChampion(champ.ChampionId);
                    if ((champ.Owned || champ.FreeToPlay))
                    {
                        //Add to ListView
                        ListViewItem item = new ListViewItem();
                        ChampionImage championImage = new ChampionImage();
                        championImage.ChampImage.Source = champions.GetChampion(champ.ChampionId).icon;
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
        }

        private void SelectChamp(int Championid)
        {
            ChampionId = Championid;
        }        

        private void ListViewItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            SelectChamp((int)item.Tag);

            //TODO: Fix stupid animation glitch on left hand side
            DoubleAnimation fadingAnimation = new DoubleAnimation();
            fadingAnimation.From = 0.4;
            fadingAnimation.To = 0;
            fadingAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.2));
            fadingAnimation.Completed += (eSender, eArgs) =>
            {
                string uriSource = System.IO.Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champions.GetChampion((int)item.Tag).splashPath);
                //BackgroundSplash.Source = Client.GetImage(uriSource);
                fadingAnimation = new DoubleAnimation();
                fadingAnimation.From = 0;
                fadingAnimation.To = 0.4;
                fadingAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            };
        }

        public async void CallWithArgs(String UUID, String GameMode, String ProcedureCall, String Parameters)
        {
            await Client.PVPNet.Call(UUID, GameMode, ProcedureCall, Parameters);
        }

        private void newRoom_OnParticipantJoin(Room room, RoomParticipant participant)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = participant.Nick + " joined the room." + Environment.NewLine;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
            }));
        }

        private void newRoom_OnRoomMessage(object sender, Message msg)
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

        
        /// <summary>
        /// Chat
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            if(connectedToChat == true)
            {
                TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = Client.LoginPacket.AllSummonerData.Summoner.Name + ": ";
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = ChatTextBox.Text + Environment.NewLine;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                newRoom.PublicMessage(ChatTextBox.Text);
                ChatTextBox.Text = "";
            }
            else if (connectedToChat == false)
            {
                TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = "You are not connected to chat! Join a teambuilder lobby to connect to chat.";
            }
        }

        public void SelectSummonerSpells(string GameMode = "CLASSIC")
        {
            InitializeComponent();
            var values = Enum.GetValues(typeof(NameToImage));
            foreach (NameToImage Spell in values)
            {
                if (!SummonerSpell.CanUseSpell((int)Spell, Client.LoginPacket.AllSummonerData.SummonerLevel.Level, GameMode))
                    continue;
                Image champImage = new Image();
                champImage.Height = 64;
                champImage.Width = 64;
                champImage.Margin = new Thickness(5, 5, 5, 5);
                var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", "Summoner" + Spell.ToString() + ".png"), UriKind.Absolute);
                champImage.Source = new BitmapImage(uriSource);
                champImage.Tag = (int)Spell;
                SummonerSpellListView.Items.Add(champImage);
            }
        }

        private int SelectedSpell1 = 0;
        //....................................................................................................................
        //Riot why. WHY CAN'T I USE THE NORMAL WAY WITH THE POPUP. WHY DO YOU HAVE TO SEND ALL THE DATA WITH QUEUEING
        //This is the smartest thing riot has done.
        private void SummonerSpellListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SummonerSpellListView.SelectedIndex != -1)
            {
                Image item = (Image)SummonerSpellListView.SelectedItem;
                int spellId = Convert.ToInt32(item.Tag);
                NameToImage spellName = (NameToImage)spellId;
                var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", "Summoner" + spellName + ".png"), UriKind.Absolute);
                if (SelectedSpell1 == 0)
                {
                    SummonerSpell1.Source = new BitmapImage(uriSource);
                    SummonerSpellListView.Items.Remove(item);
                    SelectedSpell1 = spellId;
                }
                else
                {
                    SummonerSpell2.Source = new BitmapImage(uriSource);
                    SelectSpells(SelectedSpell1, spellId);
                    SpellsGrid.Visibility = Visibility.Hidden;
                }
                RenderLegenadryClientPlayerSumSpellIcons();
            }
        }
        
        private void SelectSpells(int Spell1, int Spell2)
        {
            spell1 = Spell1;
            spell2 = Spell2;
        }
        private class Item
        {
            public string ComboRole { get; set; }
            public Item(string Strings)
            {
                this.ComboRole = Strings;
            }
            public override string ToString() 
            {
                return ComboRole;
            }
        }
        private class ReceivedGroupId
        {
            public string groupId { get; set; }
            public int slotId { get; set; }
            public int candidateAutoQuitTimeout { get; set; }
        }
    }
}
