using Sightstone.Controls;
using Sightstone.Logic;
using Sightstone.Logic.SQLite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sightstone.Logic.Riot;
using Sightstone.Logic.Riot.Platform;
using Sightstone.Logic.MultiUser;

namespace Sightstone.Windows.Profile
{
    /// <summary>
    ///     Interaction logic for Runes.xaml
    /// </summary>
    public partial class Runes
    {
        private LargeChatPlayer playerItem;
        private SpellBookPageDTO selectedBook;

        public List<SummonerRune> runes =
            new List<SummonerRune>();

        private readonly double blackRunesAvail;
        private readonly double blueRunesAvail;
        private readonly double redRunesAvail;
        private readonly double yellowRunesAvail;
        private static UserClient UserClient;

        public Runes()
        {
            InitializeComponent();
            UserClient = (UserList.Users[Client.CurrentServer])[Client.CurrentUser];
            blackRunesAvail = Math.Floor(UserClient.LoginPacket.AllSummonerData.SummonerLevel.Level / 10.0f);
            redRunesAvail = blackRunesAvail*3 +
                             Math.Ceiling((UserClient.LoginPacket.AllSummonerData.SummonerLevel.Level - blackRunesAvail * 10) /
                                          3.0f);
            yellowRunesAvail = blackRunesAvail*3 +
                                Math.Ceiling((UserClient.LoginPacket.AllSummonerData.SummonerLevel.Level -
                                              blackRunesAvail*10 -
                                              1)/3.0f);
            blueRunesAvail = blackRunesAvail*3 +
                              Math.Ceiling((UserClient.LoginPacket.AllSummonerData.SummonerLevel.Level - blackRunesAvail * 10 -
                                            2)/3.0f);
            for (var i = 1; i <= UserClient.LoginPacket.AllSummonerData.SpellBook.BookPages.Count; i++)
                RunePageListBox.Items.Add(i);

            UserClient.LoginPacket.AllSummonerData.SpellBook.BookPages.Sort((x, y) => x.PageId.CompareTo(y.PageId));
            GetAvailableRunes();
            if (!Directory.Exists(Client.ExecutingDirectory + "\\RunePages\\"))
                Directory.CreateDirectory(Client.ExecutingDirectory + "\\RunePages\\");
            if (!Directory.Exists(Client.ExecutingDirectory + "\\RunePages\\" + UserClient.LoginPacket.AllSummonerData.Summoner.Name))
                Directory.CreateDirectory(Client.ExecutingDirectory + "\\RunePages\\" + UserClient.LoginPacket.AllSummonerData.Summoner.Name);
            try
            {
                foreach (
                    var file in
                        Directory.GetFiles(
                            Path.Combine(
                                Client.ExecutingDirectory,
                                "RunePages", UserClient.LoginPacket.AllSummonerData.Summoner.Name)))
                {
                    //Fix this later, it is very bad (for more than one rune page)
                    foreach (var x in file.LeagueSettingsReader())
                        UserClient.LocalRunePages.Add(x.Key, x.Value);
                }
            }
            catch
            {
                //ignored
            }
        }

        private void UpdateStatList()
        {
            var statList = new Dictionary<string, double>();
            var runeCollection = RedListBox.Items.Cast<RuneItem>().ToList();
            runeCollection.AddRange(YellowListBox.Items.Cast<RuneItem>());
            runeCollection.AddRange(BlueListBox.Items.Cast<RuneItem>());
            runeCollection.AddRange(BlackListBox.Items.Cast<RuneItem>());
            foreach (var stat in runeCollection.SelectMany(rune => ((runes) rune.Tag).stats))
            {
                if (statList.ContainsKey(stat.Key))
                {
                    statList[stat.Key] += Convert.ToDouble(stat.Value);
                }
                else
                {
                    statList.Add(stat.Key, Convert.ToDouble(stat.Value));
                }
            }
            var finalStats = string.Empty;
            foreach (var stat in statList)
            {
                var statValue = stat.Value;
                var statStringValue = statValue.ToString(CultureInfo.InvariantCulture);
                var statName = stat.Key.Replace("Mod", string.Empty);
                statName = statName.Replace("Flat", string.Empty);
                if (statName.Substring(0, 1).Contains("r"))
                {
                    statName = statName.Substring(1);
                }

                if (statName.Contains("Percent"))
                {
                    statName = statName.Replace("Percent", string.Empty);
                    statValue *= 100;
                    statStringValue = statValue + "%";
                }
                if (statName.Contains("PerLevel"))
                {
                    statName = statName.Replace("PerLevel", string.Empty);
                    statStringValue = "@1, " + statValue + Environment.NewLine + new string(' ', statName.Length + 3) +
                                      "@18, " +
                                      (statValue*18);
                }
                finalStats += statName + " : " + statStringValue + Environment.NewLine;
            }
            StatsLabel.Content = finalStats;
        }

        private void RefreshAvailableRunes()
        {
            AvailableRuneList.Items.Clear();
            foreach (var Rune in Client.Runes)
            {
                var filteredRune = true;
                if (RuneFilterComboBox.SelectedIndex == 0)
                {
                    filteredRune = false;
                }
                else
                {
                    foreach (var filter in Rune.tags.Cast<string>().Where(filter => filter.ToLower()
                        .Contains(((Label) RuneFilterComboBox.SelectedItem).Content.ToString().ToLower())))
                    {
                        filteredRune = false;
                    }
                }

                if (filteredRune)
                {
                    continue;
                }

                foreach (var rune in runes)
                {
                    if (Rune.id != rune.RuneId)
                    {
                        continue;
                    }

                    var item = new RuneItem
                    {
                        RuneImage = {Source = Rune.icon},
                        Margin = new Thickness(2, 2, 2, 2),
                        Tag = Rune,
                        Owned = rune.Quantity,
                        Used = rune.Quantity
                    };
                    AvailableRuneList.Items.Add(item);
                    item.MouseMove += item_MouseMove;
                    item.MouseLeave += item_MouseLeave;
                }
            }
        }

        private async void GetAvailableRunes()
        {
            var runeInven =
                await UserClient.calls.GetSummonerRuneInventory(UserClient.LoginPacket.AllSummonerData.Summoner.SumId);
            runes = runeInven.SummonerRunes;
            runes.Sort((x, y) => string.Compare(x.Rune.Name, y.Rune.Name, StringComparison.Ordinal));
            RuneFilterComboBox.SelectedIndex = 0;
            RunePageListBox.SelectedIndex = UserClient.LoginPacket.AllSummonerData.SpellBook.BookPages.IndexOf(
                UserClient.LoginPacket.AllSummonerData.SpellBook.BookPages.Find(x => x.Current));
        }

        public void RenderRunes()
        {
            RedListBox.Items.Clear();
            YellowListBox.Items.Clear();
            BlueListBox.Items.Clear();
            BlackListBox.Items.Clear();
            if (selectedBook == null)
            {
                return;
            }

            foreach (var runeSlot in selectedBook.SlotEntries)
            {
                foreach (var obj in AvailableRuneList.Items)
                {
                    try
                    {
                        if (((runes) ((RuneItem) obj).Tag).id == runeSlot.RuneId)
                        {
                            ((RuneItem) obj).Used--;
                        }
                    }
                    catch
                    {
                    }
                }
                AvailableRuneList.Items.Refresh();
                UpdateStatList();
                foreach (var rune in Client.Runes)
                {
                    if (runeSlot.RuneId != rune.id)
                    {
                        continue;
                    }

                    var item = new RuneItem
                    {
                        RuneImage = {Source = rune.icon},
                        RuneName = {Content = rune.name},
                        RuneEffect = {Content = rune.description},
                        Margin = new Thickness(2, 2, 2, 2),
                        Tag = rune
                    };
                    item.MouseRightButtonDown += item_MouseRightButtonDown;
                    item.MouseMove += item_MouseMove;
                    item.MouseLeave += item_MouseLeave;
                    if (rune.name.Contains("Mark"))
                    {
                        RedListBox.Items.Add(item);
                    }
                    else if (rune.name.Contains("Seal"))
                    {
                        YellowListBox.Items.Add(item);
                    }
                    else if (rune.name.Contains("Glyph"))
                    {
                        BlueListBox.Items.Add(item);
                    }
                    else if (rune.name.Contains("Quint"))
                    {
                        BlackListBox.Items.Add(item);
                    }
                }
            }
        }

        private void item_MouseRightButtonDown(object sender, MouseEventArgs e)
        {
            ((ListBox) ((RuneItem) sender).Parent).Items.Remove(sender);

            foreach (var obj in AvailableRuneList.Items)
            {
                try
                {
                    if (((runes) ((RuneItem) obj).Tag).id == ((runes) ((RuneItem) sender).Tag).id)
                    {
                        ((RuneItem) obj).Used++;
                    }
                }
                catch
                {
                }
            }
            AvailableRuneList.Items.Refresh();
            UpdateStatList();
        }

        private void item_MouseLeave(object sender, MouseEventArgs e)
        {
            if (playerItem == null)
            {
                return;
            }

            Client.MainGrid.Children.Remove(playerItem);
            playerItem = null;
        }

        private void item_MouseMove(object sender, MouseEventArgs e)
        {
            var playerItem = (runes) ((RuneItem) sender).Tag;
            if (this.playerItem == null)
            {
                this.playerItem = new LargeChatPlayer();
                Client.MainGrid.Children.Add(this.playerItem);

                Panel.SetZIndex(this.playerItem, 4);

                //Only load once
                this.playerItem.ProfileImage.Source = playerItem.icon;
                this.playerItem.PlayerName.Content = playerItem.name;

                this.playerItem.PlayerName.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                this.playerItem.Width = this.playerItem.PlayerName.DesiredSize.Width > 250
                    ? this.playerItem.PlayerName.DesiredSize.Width
                    : 250;
                this.playerItem.PlayerLeague.Content = playerItem.id;
                this.playerItem.UsingLegendary.Visibility = Visibility.Hidden;

                this.playerItem.PlayerWins.Content = playerItem.description.Replace("<br>", Environment.NewLine);
                this.playerItem.PlayerStatus.Content = string.Empty;
                this.playerItem.LevelLabel.Content = string.Empty;
                this.playerItem.HorizontalAlignment = HorizontalAlignment.Left;
                this.playerItem.VerticalAlignment = VerticalAlignment.Top;
            }

            var mouseLocation = e.GetPosition(Client.MainGrid);

            var yMargin = mouseLocation.Y;

            var xMargin = mouseLocation.X;
            if (xMargin + this.playerItem.Width + 10 > Client.MainGrid.ActualWidth)
            {
                xMargin = Client.MainGrid.ActualWidth - this.playerItem.Width - 10;
            }

            this.playerItem.Margin = new Thickness(xMargin + 5, yMargin + 5, 0, 0);
        }

        private void RunePageListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (
                var spellPage in
                    UserClient.LoginPacket.AllSummonerData.SpellBook.BookPages.Where(spellPage => spellPage.Current))
            {
                spellPage.Current = false;
            }

            UserClient.LoginPacket.AllSummonerData.SpellBook.BookPages[RunePageListBox.SelectedIndex].Current = true;
            selectedBook = UserClient.LoginPacket.AllSummonerData.SpellBook.BookPages[RunePageListBox.SelectedIndex];
            RuneTextBox.Text = selectedBook.Name;
            RefreshAvailableRunes();
            RenderRunes();
        }

        private void AvailableRuneList_doubleClickOrRightClick(object sender, MouseButtonEventArgs e)
        {
            if (((ListBox) sender).SelectedItem == null)
            {
                return;
            }

            if (((RuneItem) ((ListBox) sender).SelectedItem).Used <= 0)
            {
                return;
            }

            var rune = ((runes) ((RuneItem) ((ListBox) sender).SelectedItem).Tag);
            var item = new RuneItem
            {
                RuneImage = {Source = rune.icon},
                Margin = new Thickness(2, 2, 2, 2),
                Tag = rune,
                RuneName = {Content = rune.name},
                RuneEffect = {Content = rune.description}
            };
            item.MouseRightButtonDown += item_MouseRightButtonDown;
            item.MouseMove += item_MouseMove;
            item.MouseLeave += item_MouseLeave;
            var tempRuneListBox = new ListBox();
            double tempAvailCount = 0;
            if (rune.name.Contains("Mark"))
            {
                tempAvailCount = redRunesAvail;
                tempRuneListBox = RedListBox;
            }
            if (rune.name.Contains("Seal"))
            {
                tempAvailCount = yellowRunesAvail;
                tempRuneListBox = YellowListBox;
            }
            if (rune.name.Contains("Glyph"))
            {
                tempAvailCount = blueRunesAvail;
                tempRuneListBox = BlueListBox;
            }
            if (rune.name.Contains("Quint"))
            {
                tempAvailCount = blackRunesAvail;
                tempRuneListBox = BlackListBox;
            }
            if (!(tempRuneListBox.Items.Count < tempAvailCount))
            {
                return;
            }

            tempRuneListBox.Items.Add(item);
            UpdateStatList();
            ((RuneItem) ((ListBox) sender).SelectedItem).Used--;
            AvailableRuneList.Items.Refresh();
        }

        private void RuneFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshAvailableRunes();
            RenderRunes();
        }

        private List<SlotEntry> GetCurrentSlotEntries()
        {
            var count = 1;
            var slotEntries = (from RuneItem rune in RedListBox.Items
                select new SlotEntry
                {
                    RuneId = ((runes) rune.Tag).id,
                    RuneSlotId = count++
                }).ToList();
            count = 10;
            slotEntries.AddRange(from RuneItem rune in YellowListBox.Items
                select new SlotEntry
                {
                    RuneId = ((runes) rune.Tag).id,
                    RuneSlotId = count++
                });
            count = 19;
            slotEntries.AddRange(from RuneItem rune in BlueListBox.Items
                select new SlotEntry
                {
                    RuneId = ((runes) rune.Tag).id,
                    RuneSlotId = count++
                });
            count = 28;
            slotEntries.AddRange(from RuneItem rune in BlackListBox.Items
                select new SlotEntry
                {
                    RuneId = ((runes) rune.Tag).id,
                    RuneSlotId = count++
                });

            return slotEntries;
        }

        private async void SaveRunes_Click(object sender, RoutedEventArgs e)
        {
            foreach (
                var runePage in
                    UserClient.LoginPacket.AllSummonerData.SpellBook.BookPages.Where(runePage => runePage.Current))
            {
                runePage.SlotEntries = GetCurrentSlotEntries();
                runePage.Name = RuneTextBox.Text;
            }

            await UserClient.calls.SaveSpellBook(UserClient.LoginPacket.AllSummonerData.SpellBook);
        }

        private void ClearRunes_Click(object sender, RoutedEventArgs e)
        {
            RedListBox.Items.Clear();
            YellowListBox.Items.Clear();
            BlueListBox.Items.Clear();
            BlackListBox.Items.Clear();
            foreach (var obj in AvailableRuneList.Items)
            {
                try
                {
                    ((RuneItem) obj).Used = ((RuneItem) obj).Owned;
                }
                catch
                {
                }
            }
            UpdateStatList();
            AvailableRuneList.Items.Refresh();
        }

        private void LocalSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(LocalName.Text))
            {
                var pop = new NotifyPlayerPopup("Error", "Local rune page name can't be empty.")
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom
                };
                Client.NotificationGrid.Children.Add(pop);
                return;
            }
            List<string> local = GetCurrentSlotEntries().Select(id => id.RuneId.ToString()).ToList();
            if (UserClient.LocalRunePages.ContainsKey(LocalName.Text))
                UserClient.LocalRunePages.Remove(LocalName.Text);

            //Client.LocalRunePages.Add(LocalName.Text, string.Join(",", local)); //Make to League setting like string
            List<string> saveString = UserClient.LocalRunePages.Select(item => item.Key + "=" + item.Value).ToList();

            try
            {
                File.WriteAllLines(Path.Combine(Client.ExecutingDirectory, "RunePages", UserClient.LoginPacket.AllSummonerData.Summoner.Name, LocalName.Text), saveString);
            }
            catch
            {
                UserClient.LocalRunePages.Remove(LocalName.Text);
                var pop = new NotifyPlayerPopup("Error", "Unable to save local rune pages.")
                        {
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Bottom
                        };
                Client.NotificationGrid.Children.Add(pop);
            }
        }
    }
}