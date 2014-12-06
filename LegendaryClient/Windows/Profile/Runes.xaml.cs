using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect.RiotObjects.Platform.Summoner.Spellbook;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    /// Interaction logic for Runes.xaml
    /// </summary>
    public partial class Runes : Page
    {
        private SpellBookPageDTO SelectedBook;
        private LargeChatPlayer PlayerItem;
        public List<PVPNetConnect.RiotObjects.Platform.Summoner.Runes.SummonerRune> runes =
            new List<PVPNetConnect.RiotObjects.Platform.Summoner.Runes.SummonerRune>();
        private double RedRunesAvail = 0;
        private double YellowRunesAvail = 0;
        private double BlueRunesAvail = 0;
        private double BlackRunesAvail = 0;
        public Runes()
        {
            InitializeComponent();
            BlackRunesAvail = Math.Floor(Client.LoginPacket.AllSummonerData.SummonerLevel.Level / 10.0f);
            RedRunesAvail = BlackRunesAvail * 3 + Math.Ceiling((Client.LoginPacket.AllSummonerData.SummonerLevel.Level - BlackRunesAvail * 10) / 3.0f);
            YellowRunesAvail = BlackRunesAvail * 3 + Math.Ceiling((Client.LoginPacket.AllSummonerData.SummonerLevel.Level - BlackRunesAvail * 10 - 1) / 3.0f);
            BlueRunesAvail = BlackRunesAvail * 3 + Math.Ceiling((Client.LoginPacket.AllSummonerData.SummonerLevel.Level - BlackRunesAvail * 10 - 2) / 3.0f);
            for (int i = 1; i <= Client.LoginPacket.AllSummonerData.SpellBook.BookPages.Count; i++)
                RunePageListBox.Items.Add(i);
            Client.LoginPacket.AllSummonerData.SpellBook.BookPages.Sort((x, y) => x.PageId.CompareTo(y.PageId));
            GetAvailableRunes();
        }
        private void UpdateStatList()
        {
            Dictionary<String, double> statList = new Dictionary<String, double>();
            List<RuneItem> runeCollection = new List<RuneItem>();
            foreach (RuneItem rune in RedListBox.Items)
            {
                runeCollection.Add(rune);
            }
            foreach (RuneItem rune in YellowListBox.Items)
            {
                runeCollection.Add(rune);
            }
            foreach (RuneItem rune in BlueListBox.Items)
            {
                runeCollection.Add(rune);
            }
            foreach (RuneItem rune in BlackListBox.Items)
            {
                runeCollection.Add(rune);
            }
            foreach (RuneItem rune in runeCollection)
            {
                foreach (KeyValuePair<string, object> stat in ((Logic.SQLite.Runes)rune.Tag).Stats)
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
            }
            String finalStats = "";
            foreach (KeyValuePair<String, double> stat in statList)
            {
                Double statValue = stat.Value;
                String statStringValue = statValue.ToString();
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
                    statStringValue = "@1, " + statValue + "\n" + new string(' ', statName.Length + 3) + "@18, " + (statValue * 18);
                }
                finalStats += statName + " : " + statStringValue + "\n";
            }
            StatsLabel.Content = finalStats;
        }
        private void RefreshAvailableRunes()
        {
            AvailableRuneList.Items.Clear();
            foreach (Logic.SQLite.Runes Rune in Client.Runes)
            {
                bool filteredRune = true;
                if (RuneFilterComboBox.SelectedIndex == 0)
                    filteredRune = false;
                else
                {
                    foreach (string filter in Rune.Tags)
                    {
                        if (filter.ToLower().Contains(((Label)RuneFilterComboBox.SelectedItem).Content.ToString().ToLower()))
                        {
                            filteredRune = false;
                        }
                    }
                }
                if (!filteredRune)
                {
                    foreach (PVPNetConnect.RiotObjects.Platform.Summoner.Runes.SummonerRune rune in runes)
                    {
                        if (Rune.Id == rune.RuneId)
                        {
                            RuneItem item = new RuneItem();
                            item.RuneImage.Source = Rune.Icon;
                            item.Margin = new Thickness(2, 2, 2, 2);
                            item.Tag = Rune;
                            item.Owned = rune.Quantity;
                            item.Used = rune.Quantity;
                            AvailableRuneList.Items.Add(item);
                            item.MouseMove += item_MouseMove;
                            item.MouseLeave += item_MouseLeave;
                        }
                    }
                }
            }
        }
        private async void GetAvailableRunes()
        {
            PVPNetConnect.RiotObjects.Platform.Summoner.Runes.SummonerRuneInventory runeInven =
                await Client.PVPNet.GetSummonerRuneInventory(Client.LoginPacket.AllSummonerData.Summoner.SumId);
            runes = runeInven.SummonerRunes;
            runes.Sort((x, y) => x.Rune.Name.CompareTo(y.Rune.Name));
            RuneFilterComboBox.SelectedIndex = 0;
            RunePageListBox.SelectedIndex = Client.LoginPacket.AllSummonerData.SpellBook.BookPages.IndexOf(
                Client.LoginPacket.AllSummonerData.SpellBook.BookPages.Find(x => x.Current == true));
        }

        public void RenderRunes()
        {
            RedListBox.Items.Clear();
            YellowListBox.Items.Clear();
            BlueListBox.Items.Clear();
            BlackListBox.Items.Clear();
            if (SelectedBook != null)
            {
                foreach (SlotEntry RuneSlot in SelectedBook.SlotEntries)
                {
                    foreach (var obj in AvailableRuneList.Items)
                    {
                        try
                        {
                            if (((Logic.SQLite.Runes)((RuneItem)obj).Tag).Id == RuneSlot.RuneId)
                            {
                                ((RuneItem)obj).Used--;
                            }
                        }
                        catch
                        {
                        }
                    }
                    AvailableRuneList.Items.Refresh();
                    UpdateStatList();
                    foreach (Logic.SQLite.Runes Rune in Client.Runes)
                    {
                        if (RuneSlot.RuneId == Rune.Id)
                        {
                            RuneItem item = new RuneItem();
                            item.RuneImage.Source = Rune.Icon;
                            item.RuneName.Content = Rune.Name;
                            item.RuneEffect.Content = Rune.Description;
                            item.Margin = new Thickness(2, 2, 2, 2);
                            item.Tag = Rune;
                            item.MouseRightButtonDown += item_MouseRightButtonDown;
                            item.MouseMove += item_MouseMove;
                            item.MouseLeave += item_MouseLeave;
                            if (Rune.Name.Contains("Mark"))
                            {
                                RedListBox.Items.Add(item);
                            }
                            else if (Rune.Name.Contains("Seal"))
                            {
                                YellowListBox.Items.Add(item);
                            }
                            else if (Rune.Name.Contains("Glyph"))
                            {
                                BlueListBox.Items.Add(item);
                            }
                            else if (Rune.Name.Contains("Quint"))
                            {
                                BlackListBox.Items.Add(item);
                            }

                        }
                    }
                }
            }
        }

        private void item_MouseRightButtonDown(object sender, MouseEventArgs e)
        {
            ((ListBox)((RuneItem)sender).Parent).Items.Remove(sender);

            foreach (var obj in AvailableRuneList.Items)
            {
                try
                {
                    if (((Logic.SQLite.Runes)((RuneItem)obj).Tag).Id == ((Logic.SQLite.Runes)((RuneItem)sender).Tag).Id)
                    {
                        ((RuneItem)obj).Used++;
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
            if (PlayerItem != null)
            {
                Client.MainGrid.Children.Remove(PlayerItem);
                PlayerItem = null;
            }
        }

        private void item_MouseMove(object sender, MouseEventArgs e)
        {
            Logic.SQLite.Runes playerItem = (Logic.SQLite.Runes)((RuneItem)sender).Tag;
            if (PlayerItem == null)
            {
                PlayerItem = new LargeChatPlayer();
                Client.MainGrid.Children.Add(PlayerItem);

                Panel.SetZIndex(PlayerItem, 4);

                //Only load once
                PlayerItem.ProfileImage.Source = playerItem.Icon;
                PlayerItem.PlayerName.Content = playerItem.Name;

                PlayerItem.PlayerName.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                if (PlayerItem.PlayerName.DesiredSize.Width > 250) //Make title fit in item
                    PlayerItem.Width = PlayerItem.PlayerName.DesiredSize.Width;
                else
                    PlayerItem.Width = 250;
                PlayerItem.PlayerLeague.Content = playerItem.Id;
                PlayerItem.UsingLegendary.Visibility = System.Windows.Visibility.Hidden;

                PlayerItem.PlayerWins.Content = ((string)playerItem.Description.Replace("<br>", Environment.NewLine));
                PlayerItem.PlayerStatus.Text = "";
                PlayerItem.LevelLabel.Content = "";
                PlayerItem.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                PlayerItem.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            }

            Point MouseLocation = e.GetPosition(Client.MainGrid);

            double YMargin = MouseLocation.Y;

            double XMargin = MouseLocation.X;
            if (XMargin + PlayerItem.Width + 10 > Client.MainGrid.ActualWidth)
                XMargin = Client.MainGrid.ActualWidth - PlayerItem.Width - 10;

            PlayerItem.Margin = new Thickness(XMargin + 5, YMargin + 5, 0, 0);
        }

        private void RunePageListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (SpellBookPageDTO SpellPage in Client.LoginPacket.AllSummonerData.SpellBook.BookPages)
            {
                if (SpellPage.Current)
                {
                    SpellPage.Current = false;
                }
            }
            Client.LoginPacket.AllSummonerData.SpellBook.BookPages[RunePageListBox.SelectedIndex].Current = true;
            SelectedBook = Client.LoginPacket.AllSummonerData.SpellBook.BookPages[RunePageListBox.SelectedIndex];
            RuneTextBox.Text = SelectedBook.Name;
            RefreshAvailableRunes();
            RenderRunes();
        }

        private void AvailableRuneList_DoubleClickOrRightClick(object sender, MouseButtonEventArgs e)
        {
            if (((ListBox)sender).SelectedItem != null) //Fix crash if you double click scrollbar (no item selected)
            {
                if (((RuneItem)((ListBox)sender).SelectedItem).Used <= 0)
                    return;
                Logic.SQLite.Runes Rune = ((Logic.SQLite.Runes)((RuneItem)((ListBox)sender).SelectedItem).Tag);
                RuneItem item = new RuneItem();
                item.RuneImage.Source = Rune.Icon;
                item.Margin = new Thickness(2, 2, 2, 2);
                item.Tag = Rune;
                item.RuneName.Content = Rune.Name;
                item.RuneEffect.Content = Rune.Description;
                item.MouseRightButtonDown += item_MouseRightButtonDown;
                item.MouseMove += item_MouseMove;
                item.MouseLeave += item_MouseLeave;
                ListBox tempRuneListBox = new ListBox();
                double tempAvailCount = 0;
                if (Rune.Name.Contains("Mark"))
                {
                    tempAvailCount = RedRunesAvail;
                    tempRuneListBox = RedListBox;
                }
                if (Rune.Name.Contains("Seal"))
                {
                    tempAvailCount = YellowRunesAvail;
                    tempRuneListBox = YellowListBox;
                }
                if (Rune.Name.Contains("Glyph"))
                {
                    tempAvailCount = BlueRunesAvail;
                    tempRuneListBox = BlueListBox;
                }
                if (Rune.Name.Contains("Quint"))
                {
                    tempAvailCount = BlackRunesAvail;
                    tempRuneListBox = BlackListBox;
                }
                if (tempRuneListBox.Items.Count < tempAvailCount)
                {
                    tempRuneListBox.Items.Add(item);
                    UpdateStatList();
                    ((RuneItem)((ListBox)sender).SelectedItem).Used--;
                    AvailableRuneList.Items.Refresh();
                }
            }
        }

        private void RuneFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshAvailableRunes();
            RenderRunes();
        }

        private List<SlotEntry> GetCurrentSlotEntries()
        {
            List<SlotEntry> slotEntries = new List<SlotEntry>();
            int count = 1;
            foreach (RuneItem rune in RedListBox.Items)
            {
                SlotEntry slotEntry = new SlotEntry();
                slotEntry.RuneId = ((Logic.SQLite.Runes)rune.Tag).Id;
                slotEntry.RuneSlotId = count++;
                slotEntries.Add(slotEntry);
            }
            count = 10;
            foreach (RuneItem rune in YellowListBox.Items)
            {
                SlotEntry slotEntry = new SlotEntry();
                slotEntry.RuneId = ((Logic.SQLite.Runes)rune.Tag).Id;
                slotEntry.RuneSlotId = count++;
                slotEntries.Add(slotEntry);
            }
            count = 19;
            foreach (RuneItem rune in BlueListBox.Items)
            {
                SlotEntry slotEntry = new SlotEntry();
                slotEntry.RuneId = ((Logic.SQLite.Runes)rune.Tag).Id;
                slotEntry.RuneSlotId = count++;
                slotEntries.Add(slotEntry);
            }
            count = 28;
            foreach (RuneItem rune in BlackListBox.Items)
            {
                SlotEntry slotEntry = new SlotEntry();
                slotEntry.RuneId = ((Logic.SQLite.Runes)rune.Tag).Id;
                slotEntry.RuneSlotId = count++;
                slotEntries.Add(slotEntry);
            }
            return slotEntries;
        }

        private async void SaveRunes_Click(object sender, RoutedEventArgs e)
        {
            foreach (SpellBookPageDTO RunePage in Client.LoginPacket.AllSummonerData.SpellBook.BookPages)
            {
                if (RunePage.Current)
                {
                    RunePage.SlotEntries = GetCurrentSlotEntries();
                    RunePage.Name = RuneTextBox.Text;
                }
            }
            await Client.PVPNet.SaveSpellBook(Client.LoginPacket.AllSummonerData.SpellBook);
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
                    ((RuneItem)obj).Used = ((RuneItem)obj).Owned;
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