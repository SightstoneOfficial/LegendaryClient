#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
        private LargeChatPlayer _playerItem;
        private SpellBookPageDTO _selectedBook;

        public List<SummonerRune> runes =
            new List<SummonerRune>();

        private readonly double _blackRunesAvail;
        private readonly double _blueRunesAvail;
        private readonly double _redRunesAvail;
        private readonly double _yellowRunesAvail;

        public Runes()
        {
            InitializeComponent();
            Change();

            _blackRunesAvail = Math.Floor(Client.LoginPacket.AllSummonerData.SummonerLevel.Level/10.0f);
            _redRunesAvail = _blackRunesAvail*3 +
                             Math.Ceiling((Client.LoginPacket.AllSummonerData.SummonerLevel.Level - _blackRunesAvail*10)/
                                          3.0f);
            _yellowRunesAvail = _blackRunesAvail*3 +
                                Math.Ceiling((Client.LoginPacket.AllSummonerData.SummonerLevel.Level -
                                              _blackRunesAvail*10 -
                                              1)/3.0f);
            _blueRunesAvail = _blackRunesAvail*3 +
                              Math.Ceiling((Client.LoginPacket.AllSummonerData.SummonerLevel.Level - _blackRunesAvail*10 -
                                            2)/3.0f);
            for (var i = 1; i <= Client.LoginPacket.AllSummonerData.SpellBook.BookPages.Count; i++)
                RunePageListBox.Items.Add(i);

            Client.LoginPacket.AllSummonerData.SpellBook.BookPages.Sort((x, y) => x.PageId.CompareTo(y.PageId));
            GetAvailableRunes();
            if (!Directory.Exists(Client.ExecutingDirectory + "\\RunePages\\"))
                Directory.CreateDirectory(Client.ExecutingDirectory + "\\RunePages\\");
            Client.LocalRunePages = Client.LeagueSettingsReader(Client.ExecutingDirectory + "\\RunePages\\" + Client.LoginPacket.AllSummonerData.Summoner.Name);
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
            if (_selectedBook == null)
            {
                return;
            }

            foreach (var runeSlot in _selectedBook.SlotEntries)
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
            if (_playerItem == null)
            {
                return;
            }

            Client.MainGrid.Children.Remove(_playerItem);
            _playerItem = null;
        }

        private void item_MouseMove(object sender, MouseEventArgs e)
        {
            var playerItem = (runes) ((RuneItem) sender).Tag;
            if (_playerItem == null)
            {
                _playerItem = new LargeChatPlayer();
                Client.MainGrid.Children.Add(_playerItem);

                Panel.SetZIndex(_playerItem, 4);

                //Only load once
                _playerItem.ProfileImage.Source = playerItem.icon;
                _playerItem.PlayerName.Content = playerItem.name;

                _playerItem.PlayerName.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                _playerItem.Width = _playerItem.PlayerName.DesiredSize.Width > 250
                    ? _playerItem.PlayerName.DesiredSize.Width
                    : 250;
                _playerItem.PlayerLeague.Content = playerItem.id;
                _playerItem.UsingLegendary.Visibility = Visibility.Hidden;

                _playerItem.PlayerWins.Content = playerItem.description.Replace("<br>", Environment.NewLine);
                _playerItem.PlayerStatus.Text = string.Empty;
                _playerItem.LevelLabel.Content = string.Empty;
                _playerItem.HorizontalAlignment = HorizontalAlignment.Left;
                _playerItem.VerticalAlignment = VerticalAlignment.Top;
            }

            var mouseLocation = e.GetPosition(Client.MainGrid);

            var yMargin = mouseLocation.Y;

            var xMargin = mouseLocation.X;
            if (xMargin + _playerItem.Width + 10 > Client.MainGrid.ActualWidth)
            {
                xMargin = Client.MainGrid.ActualWidth - _playerItem.Width - 10;
            }

            _playerItem.Margin = new Thickness(xMargin + 5, yMargin + 5, 0, 0);
        }

        private void RunePageListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (
                var spellPage in
                    Client.LoginPacket.AllSummonerData.SpellBook.BookPages.Where(spellPage => spellPage.Current))
            {
                spellPage.Current = false;
            }

            Client.LoginPacket.AllSummonerData.SpellBook.BookPages[RunePageListBox.SelectedIndex].Current = true;
            _selectedBook = Client.LoginPacket.AllSummonerData.SpellBook.BookPages[RunePageListBox.SelectedIndex];
            RuneTextBox.Text = _selectedBook.Name;
            RefreshAvailableRunes();
            RenderRunes();
        }

        private void AvailableRuneList_DoubleClickOrRightClick(object sender, MouseButtonEventArgs e)
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
                tempAvailCount = _redRunesAvail;
                tempRuneListBox = RedListBox;
            }
            if (rune.name.Contains("Seal"))
            {
                tempAvailCount = _yellowRunesAvail;
                tempRuneListBox = YellowListBox;
            }
            if (rune.name.Contains("Glyph"))
            {
                tempAvailCount = _blueRunesAvail;
                tempRuneListBox = BlueListBox;
            }
            if (rune.name.Contains("Quint"))
            {
                tempAvailCount = _blackRunesAvail;
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
            if (LocalName.Text == string.Empty)
                return;
            else if (LocalName.Text.Contains('='))
            {
                var pop = new NotifyPlayerPopup("Error", "Local rune page name can't contain = .")
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom
                };
                Client.NotificationGrid.Children.Add(pop);
                return;
            }
            List<string> local = new List<string>();
            foreach(var id in GetCurrentSlotEntries())
            {
                local.Add(id.RuneId.ToString());
            }
            if (Client.LocalRunePages.ContainsKey(LocalName.Text))
                Client.LocalRunePages.Remove(LocalName.Text);

            Client.LocalRunePages.Add(LocalName.Text, string.Join(",", local)); //Make to League setting like string
            List<string> saveString = new List<string>();

            foreach(var item in Client.LocalRunePages)
            {
                saveString.Add(item.Key + "=" + item.Value);
            }
            try
            {
                File.WriteAllLines(Client.ExecutingDirectory + "\\RunePages\\" + Client.LoginPacket.AllSummonerData.Summoner.Name, saveString);
            }
            catch
            {
                Client.LocalRunePages.Remove(LocalName.Text);
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