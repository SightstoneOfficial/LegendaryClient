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
                RunePageListView.Items.Add(i);
            Client.LoginPacket.AllSummonerData.SpellBook.BookPages.Sort((x, y) => x.PageId.CompareTo(y.PageId));
            GetAvailableRunes();
        }
        private void UpdateStatList()
        {
            Dictionary<String, double> statList = new Dictionary<String, double>();
            List<RuneItem> runeCollection = new List<RuneItem>();
            foreach (RuneItem rune in RedListView.Items)
            {
                runeCollection.Add(rune);
            }
            foreach (RuneItem rune in YellowListView.Items)
            {
                runeCollection.Add(rune);
            }
            foreach (RuneItem rune in BlueListView.Items)
            {
                runeCollection.Add(rune);
            }
            foreach (RuneItem rune in BlackListView.Items)
            {
                runeCollection.Add(rune);
            }
            foreach (RuneItem rune in runeCollection)
            {
                foreach (KeyValuePair<string, object> stat in ((runes)rune.Tag).stats)
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
            foreach (runes Rune in Client.Runes)
            {
                bool filteredRune = true;
                if (RuneFilterComboBox.SelectedIndex == 0)
                    filteredRune = false;
                else
                {
                    foreach (string filter in Rune.tags)
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
                        if (Rune.id == rune.RuneId)
                        {
                            RuneItem item = new RuneItem();
                            item.RuneImage.Source = Rune.icon;
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
<<<<<<< HEAD
            PVPNetConnect.RiotObjects.Platform.Summoner.Runes.SummonerRuneInventory runeInven =
=======
            PVPNetConnect.RiotObjects.Platform.Summoner.Runes.SummonerRuneInventory runeInven = 
>>>>>>> master
                await Client.PVPNet.GetSummonerRuneInventory(Client.LoginPacket.AllSummonerData.Summoner.SumId);
            runes = runeInven.SummonerRunes;
            runes.Sort((x, y) => x.Rune.Name.CompareTo(y.Rune.Name));
            RuneFilterComboBox.SelectedIndex = 0;
            RunePageListView.SelectedIndex = Client.LoginPacket.AllSummonerData.SpellBook.BookPages.IndexOf(
                Client.LoginPacket.AllSummonerData.SpellBook.BookPages.Find(x => x.Current == true));
        }

        public void RenderRunes()
        {
            RedListView.Items.Clear();
            YellowListView.Items.Clear();
            BlueListView.Items.Clear();
            BlackListView.Items.Clear();
            if (SelectedBook != null)
            {
                foreach (SlotEntry RuneSlot in SelectedBook.SlotEntries)
                {
                    foreach (var obj in AvailableRuneList.Items)
                    {
                        try
                        {
                            if (((runes)((RuneItem)obj).Tag).id == RuneSlot.RuneId)
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
                    foreach (runes Rune in Client.Runes)
                    {
                        if (RuneSlot.RuneId == Rune.id)
                        {
                            RuneItem item = new RuneItem();
                            item.RuneImage.Source = Rune.icon;
                            item.Margin = new Thickness(2, 2, 2, 2);
                            item.Tag = Rune;
                            item.MouseRightButtonDown += item_MouseRightButtonDown;
                            item.MouseMove += item_MouseMove;
                            item.MouseLeave += item_MouseLeave;
                            if (Rune.name.Contains("Mark"))
                            {
                                RedListView.Items.Add(item);
                            }
                            else if (Rune.name.Contains("Seal"))
                            {
                                YellowListView.Items.Add(item);
                            }
                            else if (Rune.name.Contains("Glyph"))
                            {
                                BlueListView.Items.Add(item);
                            }
                            else if (Rune.name.Contains("Quint"))
                            {
                                BlackListView.Items.Add(item);
                            }

                        }
                    }
                }
            }
        }

        private void item_MouseRightButtonDown(object sender, MouseEventArgs e)
        {
            ((ListView)((RuneItem)sender).Parent).Items.Remove(sender);

            foreach (var obj in AvailableRuneList.Items)
            {
                try
                {
                    if (((runes)((RuneItem)obj).Tag).id == ((runes)((RuneItem)sender).Tag).id)
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
            runes playerItem = (runes)((RuneItem)sender).Tag;
            if (PlayerItem == null)
            {
                PlayerItem = new LargeChatPlayer();
                Client.MainGrid.Children.Add(PlayerItem);

                Panel.SetZIndex(PlayerItem, 4);

                //Only load once
                PlayerItem.ProfileImage.Source = playerItem.icon;
                PlayerItem.PlayerName.Content = playerItem.name;

                PlayerItem.PlayerName.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                if (PlayerItem.PlayerName.DesiredSize.Width > 250) //Make title fit in item
                    PlayerItem.Width = PlayerItem.PlayerName.DesiredSize.Width;
                else
                    PlayerItem.Width = 250;
                PlayerItem.PlayerLeague.Content = playerItem.id;
                PlayerItem.UsingLegendary.Visibility = System.Windows.Visibility.Hidden;

                PlayerItem.PlayerWins.Content = ((string)playerItem.description.Replace("<br>", Environment.NewLine));
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

        private void RunePageListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (SpellBookPageDTO SpellPage in Client.LoginPacket.AllSummonerData.SpellBook.BookPages)
            {
                if (SpellPage.Current)
                {
                    SpellPage.Current = false;
                }
            }
            Client.LoginPacket.AllSummonerData.SpellBook.BookPages[RunePageListView.SelectedIndex].Current = true;
            SelectedBook = Client.LoginPacket.AllSummonerData.SpellBook.BookPages[RunePageListView.SelectedIndex];
            RuneTextBox.Text = SelectedBook.Name;
            RefreshAvailableRunes();
            RenderRunes();
        }

        private void AvailableRuneList_DoubleClickOrRightClick(object sender, MouseButtonEventArgs e)
        {
            if (((RuneItem)((ListView)sender).SelectedItem).Used <= 0)
                return;
            runes Rune = ((runes)((RuneItem)((ListView)sender).SelectedItem).Tag);
            RuneItem item = new RuneItem();
            item.RuneImage.Source = Rune.icon;
            item.Margin = new Thickness(2, 2, 2, 2);
            item.Tag = Rune;
            item.MouseRightButtonDown += item_MouseRightButtonDown;
            item.MouseMove += item_MouseMove;
            item.MouseLeave += item_MouseLeave;
            ListView tempRuneListView = new ListView();
            double tempAvailCount = 0;
            if (Rune.name.Contains("Mark"))
            {
                tempAvailCount = RedRunesAvail;
                tempRuneListView = RedListView;
            }
            if (Rune.name.Contains("Seal"))
            {
                tempAvailCount = YellowRunesAvail;
                tempRuneListView = YellowListView;
            }
            if (Rune.name.Contains("Glyph"))
            {
                tempAvailCount = BlueRunesAvail;
                tempRuneListView = BlueListView;
            }
            if (Rune.name.Contains("Quint"))
            {
                tempAvailCount = BlackRunesAvail;
                tempRuneListView = BlackListView;
            }
            if (tempRuneListView.Items.Count < tempAvailCount)
            {
                tempRuneListView.Items.Add(item);
                UpdateStatList();
                ((RuneItem)((ListView)sender).SelectedItem).Used--;
                AvailableRuneList.Items.Refresh();
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
            foreach (RuneItem rune in RedListView.Items)
            {
                SlotEntry slotEntry = new SlotEntry();
                slotEntry.RuneId = ((runes)rune.Tag).id;
                slotEntry.RuneSlotId = count++;
                slotEntries.Add(slotEntry);
            }
            count = 10;
            foreach (RuneItem rune in YellowListView.Items)
            {
                SlotEntry slotEntry = new SlotEntry();
                slotEntry.RuneId = ((runes)rune.Tag).id;
                slotEntry.RuneSlotId = count++;
                slotEntries.Add(slotEntry);
            }
            count = 19;
            foreach (RuneItem rune in BlueListView.Items)
            {
                SlotEntry slotEntry = new SlotEntry();
                slotEntry.RuneId = ((runes)rune.Tag).id;
                slotEntry.RuneSlotId = count++;
                slotEntries.Add(slotEntry);
            }
            count = 28;
            foreach (RuneItem rune in BlackListView.Items)
            {
                SlotEntry slotEntry = new SlotEntry();
                slotEntry.RuneId = ((runes)rune.Tag).id;
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
            RedListView.Items.Clear();
            YellowListView.Items.Clear();
            BlueListView.Items.Clear();
            BlackListView.Items.Clear();
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