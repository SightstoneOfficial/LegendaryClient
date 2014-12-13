#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Properties;
using PVPNetConnect.RiotObjects.Platform.Summoner.Runes;
using PVPNetConnect.RiotObjects.Platform.Summoner.Spellbook;

#endregion

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    ///     Interaction logic for Runes.xaml
    /// </summary>
    public partial class Runes
    {
        private readonly double BlackRunesAvail;
        private readonly double BlueRunesAvail;
        private readonly double RedRunesAvail;
        private readonly double YellowRunesAvail;
        private LargeChatPlayer PlayerItem;
        private SpellBookPageDTO SelectedBook;

        public List<SummonerRune> runes =
            new List<SummonerRune>();

        public Runes()
        {
            InitializeComponent();
            Change();

            BlackRunesAvail = Math.Floor(Client.LoginPacket.AllSummonerData.SummonerLevel.Level/10.0f);
            RedRunesAvail = BlackRunesAvail*3 +
                            Math.Ceiling((Client.LoginPacket.AllSummonerData.SummonerLevel.Level - BlackRunesAvail*10)/
                                         3.0f);
            YellowRunesAvail = BlackRunesAvail*3 +
                               Math.Ceiling((Client.LoginPacket.AllSummonerData.SummonerLevel.Level - BlackRunesAvail*10 -
                                             1)/3.0f);
            BlueRunesAvail = BlackRunesAvail*3 +
                             Math.Ceiling((Client.LoginPacket.AllSummonerData.SummonerLevel.Level - BlackRunesAvail*10 -
                                           2)/3.0f);
            for (int i = 1; i <= Client.LoginPacket.AllSummonerData.SpellBook.BookPages.Count; i++)
                RunePageListBox.Items.Add(i);

            Client.LoginPacket.AllSummonerData.SpellBook.BookPages.Sort((x, y) => x.PageId.CompareTo(y.PageId));
            GetAvailableRunes();
        }

        public void Change()
        {
            var themeAccent = new ResourceDictionary
            {
                Source = new Uri(Settings.Default.Theme)
            };
            Resources.MergedDictionaries.Add(themeAccent);
        }

        private void UpdateStatList()
        {
            var statList = new Dictionary<String, double>();
            List<RuneItem> runeCollection = RedListBox.Items.Cast<RuneItem>().ToList();
            runeCollection.AddRange(YellowListBox.Items.Cast<RuneItem>());
            runeCollection.AddRange(BlueListBox.Items.Cast<RuneItem>());
            runeCollection.AddRange(BlackListBox.Items.Cast<RuneItem>());
            foreach (var stat in runeCollection.SelectMany(rune => ((runes) rune.Tag).stats))
            {
                if (statList.ContainsKey(stat.Key))
                    statList[stat.Key] += Convert.ToDouble(stat.Value);
                else
                    statList.Add(stat.Key, Convert.ToDouble(stat.Value));
            }
            String finalStats = "";
            foreach (var stat in statList)
            {
                Double statValue = stat.Value;
                String statStringValue = statValue.ToString(CultureInfo.InvariantCulture);
                String statName = stat.Key.Replace("Mod", "");
                statName = statName.Replace("Flat", "");
                if (statName.Substring(0, 1).Contains("r"))
                    statName = statName.Substring(1);

                if (statName.Contains("Percent"))
                {
                    statName = statName.Replace("Percent", "");
                    statValue *= 100;
                    statStringValue = statValue + "%";
                }
                if (statName.Contains("PerLevel"))
                {
                    statName = statName.Replace("PerLevel", "");
                    statStringValue = "@1, " + statValue + "\n" + new string(' ', statName.Length + 3) + "@18, " +
                                      (statValue*18);
                }
                finalStats += statName + " : " + statStringValue + "\n";
            }
            StatsLabel.Content = finalStats;
        }

        private void RefreshAvailableRunes()
        {
            AvailableRuneList.Items.Clear();
            foreach (runes Rune in Client.Runes)
            {
                bool filteredRune = true;
                if (RuneFilterComboBox.SelectedIndex == 0)
                    filteredRune = false;
                else
                {
                    foreach (string filter in Rune.tags.Cast<string>().Where(filter => filter.ToLower()
                        .Contains(((Label) RuneFilterComboBox.SelectedItem).Content.ToString().ToLower())))
                    {
                        filteredRune = false;
                    }
                }
                if (filteredRune)
                    continue;

                foreach (SummonerRune rune in runes)
                {
                    if (Rune.id != rune.RuneId)
                        continue;

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
            SummonerRuneInventory runeInven =
                await Client.PVPNet.GetSummonerRuneInventory(Client.LoginPacket.AllSummonerData.Summoner.SumId);
            runes = runeInven.SummonerRunes;
            runes.Sort((x, y) => String.Compare(x.Rune.Name, y.Rune.Name, StringComparison.Ordinal));
            RuneFilterComboBox.SelectedIndex = 0;
            RunePageListBox.SelectedIndex = Client.LoginPacket.AllSummonerData.SpellBook.BookPages.IndexOf(
                Client.LoginPacket.AllSummonerData.SpellBook.BookPages.Find(x => x.Current));
        }

        public void RenderRunes()
        {
            RedListBox.Items.Clear();
            YellowListBox.Items.Clear();
            BlueListBox.Items.Clear();
            BlackListBox.Items.Clear();
            if (SelectedBook != null)
            {
                foreach (SlotEntry runeSlot in SelectedBook.SlotEntries)
                {
                    foreach (object obj in AvailableRuneList.Items)
                    {
                        try
                        {
                            if (((runes) ((RuneItem) obj).Tag).id == runeSlot.RuneId)
                                ((RuneItem) obj).Used--;
                        }
                        catch
                        {
                        }
                    }
                    AvailableRuneList.Items.Refresh();
                    UpdateStatList();
                    foreach (runes rune in Client.Runes)
                    {
                        if (runeSlot.RuneId != rune.id)
                            continue;

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
                            RedListBox.Items.Add(item);
                        else if (rune.name.Contains("Seal"))
                            YellowListBox.Items.Add(item);
                        else if (rune.name.Contains("Glyph"))
                            BlueListBox.Items.Add(item);
                        else if (rune.name.Contains("Quint"))
                            BlackListBox.Items.Add(item);
                    }
                }
            }
        }

        private void item_MouseRightButtonDown(object sender, MouseEventArgs e)
        {
            ((ListBox) ((RuneItem) sender).Parent).Items.Remove(sender);

            foreach (object obj in AvailableRuneList.Items)
            {
                try
                {
                    if (((runes) ((RuneItem) obj).Tag).id == ((runes) ((RuneItem) sender).Tag).id)
                        ((RuneItem) obj).Used++;
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
            if (PlayerItem == null)
                return;

            Client.MainGrid.Children.Remove(PlayerItem);
            PlayerItem = null;
        }

        private void item_MouseMove(object sender, MouseEventArgs e)
        {
            var playerItem = (runes) ((RuneItem) sender).Tag;
            if (PlayerItem == null)
            {
                PlayerItem = new LargeChatPlayer();
                Client.MainGrid.Children.Add(PlayerItem);

                Panel.SetZIndex(PlayerItem, 4);

                //Only load once
                PlayerItem.ProfileImage.Source = playerItem.icon;
                PlayerItem.PlayerName.Content = playerItem.name;

                PlayerItem.PlayerName.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                PlayerItem.Width = PlayerItem.PlayerName.DesiredSize.Width > 250
                    ? PlayerItem.PlayerName.DesiredSize.Width
                    : 250;
                PlayerItem.PlayerLeague.Content = playerItem.id;
                PlayerItem.UsingLegendary.Visibility = Visibility.Hidden;

                PlayerItem.PlayerWins.Content = playerItem.description.Replace("<br>", Environment.NewLine);
                PlayerItem.PlayerStatus.Text = "";
                PlayerItem.LevelLabel.Content = "";
                PlayerItem.HorizontalAlignment = HorizontalAlignment.Left;
                PlayerItem.VerticalAlignment = VerticalAlignment.Top;
            }

            Point mouseLocation = e.GetPosition(Client.MainGrid);

            double yMargin = mouseLocation.Y;

            double xMargin = mouseLocation.X;
            if (xMargin + PlayerItem.Width + 10 > Client.MainGrid.ActualWidth)
                xMargin = Client.MainGrid.ActualWidth - PlayerItem.Width - 10;

            PlayerItem.Margin = new Thickness(xMargin + 5, yMargin + 5, 0, 0);
        }

        private void RunePageListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (
                SpellBookPageDTO spellPage in
                    Client.LoginPacket.AllSummonerData.SpellBook.BookPages.Where(SpellPage => SpellPage.Current))
                spellPage.Current = false;

            Client.LoginPacket.AllSummonerData.SpellBook.BookPages[RunePageListBox.SelectedIndex].Current = true;
            SelectedBook = Client.LoginPacket.AllSummonerData.SpellBook.BookPages[RunePageListBox.SelectedIndex];
            RuneTextBox.Text = SelectedBook.Name;
            RefreshAvailableRunes();
            RenderRunes();
        }

        private void AvailableRuneList_DoubleClickOrRightClick(object sender, MouseButtonEventArgs e)
        {
            if (((ListBox) sender).SelectedItem == null)
                return;

            if (((RuneItem) ((ListBox) sender).SelectedItem).Used <= 0)
                return;

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
                tempAvailCount = RedRunesAvail;
                tempRuneListBox = RedListBox;
            }
            if (rune.name.Contains("Seal"))
            {
                tempAvailCount = YellowRunesAvail;
                tempRuneListBox = YellowListBox;
            }
            if (rune.name.Contains("Glyph"))
            {
                tempAvailCount = BlueRunesAvail;
                tempRuneListBox = BlueListBox;
            }
            if (rune.name.Contains("Quint"))
            {
                tempAvailCount = BlackRunesAvail;
                tempRuneListBox = BlackListBox;
            }
            if (!(tempRuneListBox.Items.Count < tempAvailCount))
                return;

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
            int count = 1;
            List<SlotEntry> slotEntries = (from RuneItem rune in RedListBox.Items
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
                SpellBookPageDTO runePage in
                    Client.LoginPacket.AllSummonerData.SpellBook.BookPages.Where(runePage => runePage.Current))
            {
                runePage.SlotEntries = GetCurrentSlotEntries();
                runePage.Name = RuneTextBox.Text;
            }

            await Client.PVPNet.SaveSpellBook(Client.LoginPacket.AllSummonerData.SpellBook);
        }

        private void ClearRunes_Click(object sender, RoutedEventArgs e)
        {
            RedListBox.Items.Clear();
            YellowListBox.Items.Clear();
            BlueListBox.Items.Clear();
            BlackListBox.Items.Clear();
            foreach (object obj in AvailableRuneList.Items)
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
    }
}