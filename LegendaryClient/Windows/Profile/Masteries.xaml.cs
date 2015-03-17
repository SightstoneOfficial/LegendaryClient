#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect.RiotObjects.Platform.Summoner.Masterybook;

#endregion

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    ///     Interaction logic for Masteries.xaml
    /// </summary>
    public partial class Masteries
    {
        private int _defenseUsedPoints;
        private int _offenseUsedPoints;
        private LargeChatPlayer _playerItem;
        private MasteryBookPageDTO _selectedBook;
        private int _usedPoints;
        private int _utilityUsedPoints;
        private readonly List<double> _masteryPageOrder = new List<double>();
        public Masteries()
        {
            InitializeComponent();
            MasteryPageListView.Items.Clear();
            for (var i = 1; i <= Client.LoginPacket.AllSummonerData.MasteryBook.BookPages.Count; i++)
            {
                MasteryPageListView.Items.Add(i + " ");
            }

            double selectedPageId = 0;
            foreach (var masteryPage in Client.LoginPacket.AllSummonerData.MasteryBook.BookPages)
            {
                _masteryPageOrder.Add(masteryPage.PageId);
                if (masteryPage.Current)
                {
                    selectedPageId = masteryPage.PageId;
                }
            }
            _masteryPageOrder.Sort();
            MasteryPageListView.SelectedIndex = _masteryPageOrder.IndexOf(selectedPageId);
        }

        public void ChangeBook()
        {
            MasteryTextBox.Text = _selectedBook.Name;
            foreach (var talent in _selectedBook.TalentEntries)
            {
                var talent1 = talent;
                foreach (var mastery in Client.Masteries.Where(mastery => mastery.id == talent1.TalentId))
                {
                    mastery.selectedRank = talent.Rank;
                }
            }
        }

        public void RenderMasteries()
        {
            OffenseListView.Items.Clear();
            DefenseListView.Items.Clear();
            UtilityListView.Items.Clear();

            _usedPoints = 0;
            _offenseUsedPoints = 0;
            _defenseUsedPoints = 0;
            _utilityUsedPoints = 0;
            foreach (var mastery in Client.Masteries)
            {
                var isOffense = false;
                var isDefense = false;

                var item = new MasteryItem
                {
                    RankLabel =
                    {
                        Content = "0/" + mastery.ranks
                    },
                    MasteryImage = {Source = mastery.icon, Opacity = 0.4},
                    Margin = new Thickness(2, 2, 2, 2)
                };

                if (mastery.selectedRank > 0)
                {
                    item.MasteryImage.Opacity = 1;
                    item.RankLabel.Content = mastery.selectedRank + "/" + mastery.ranks;
                }

                _usedPoints += mastery.selectedRank;

                switch (mastery.tree)
                {
                    case "Offense":
                        _offenseUsedPoints += mastery.selectedRank;
                        isOffense = true;
                        OffenseListView.Items.Add(item);
                        break;
                    case "Defense":
                        _defenseUsedPoints += mastery.selectedRank;
                        isDefense = true;
                        DefenseListView.Items.Add(item);
                        break;
                    default:
                        _utilityUsedPoints += mastery.selectedRank;
                        UtilityListView.Items.Add(item);
                        break;
                }

                //Add spaces
                if (mastery.id == 4152 ||
                    mastery.id == 4222 ||
                    mastery.id == 4253 ||
                    mastery.id == 4314 ||
                    mastery.id == 4344 ||
                    mastery.id == 4353 ||
                    mastery.id == 4154)
                {
                    var rect = new Rectangle
                    {
                        Width = 64,
                        Height = 64,
                        Margin = new Thickness(2, 2, 2, 2)
                    };
                    if (isOffense)
                    {
                        OffenseListView.Items.Add(rect);
                    }
                    else if (isDefense)
                    {
                        DefenseListView.Items.Add(rect);
                        if (mastery.id == 4253)
                        {
                            DefenseListView.Items.Add(new Rectangle
                            {
                                Width = 64,
                                Height = 64,
                                Margin = new Thickness(2, 2, 2, 2)
                            });
                        }
                    }
                    else
                    {
                        UtilityListView.Items.Add(rect);
                        if (mastery.id == 4353)
                        {
                            UtilityListView.Items.Add(new Rectangle
                            {
                                Width = 64,
                                Height = 64,
                                Margin = new Thickness(2, 2, 2, 2)
                            });
                        }
                    }
                }

                item.Tag = mastery;
                item.MouseWheel += item_MouseWheel;
                item.MouseLeftButtonDown += item_MouseLeftButtonDown;
                item.MouseRightButtonDown += item_MouseRightButtonDown;
                item.MouseMove += item_MouseMove;
                item.MouseLeave += item_MouseLeave;
            }

            UsedLabel.Content = "Points Used: " + _usedPoints;
            FreeLabel.Content = "Points Free: " +
                                (Client.LoginPacket.AllSummonerData.SummonerTalentsAndPoints.TalentPoints - _usedPoints);
            OffenseLabel.Content = "Offense: " + _offenseUsedPoints;
            DefenseLabel.Content = "Defense: " + _defenseUsedPoints;
            UtilityLabel.Content = "Utility: " + _utilityUsedPoints;
        }

        private void item_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                item_MouseLeftButtonDown(sender, null);
            }
            else
            {
                item_MouseRightButtonDown(sender, null);
            }
        }

        private void item_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = (MasteryItem) sender;
            var playerItem = (masteries) item.Tag;
            if (playerItem.selectedRank == 0)
            {
                return;
            }

            //Temp check - make it so you can remove masteries even if they are above mastery if enough points in tree
            var filteredMasteries =
                Client.Masteries.FindAll(x => x.tree == playerItem.tree && x.treeRow > playerItem.treeRow);
            if (filteredMasteries.Any(checkMastery => checkMastery.selectedRank > 0))
            {
                return;
            }

            playerItem.selectedRank -= 1;
            foreach (var talent in Client.Masteries.Where(talent => playerItem.id == talent.id))
            {
                talent.selectedRank = playerItem.selectedRank;
            }

            RenderMasteries();
        }

        private void item_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = (MasteryItem) sender;
            var playerItem = (masteries) item.Tag;
            //Max rank
            if (playerItem.selectedRank == playerItem.ranks)
            {
                return;
            }

            //Has enough points in tree
            switch (playerItem.tree)
            {
                case "Offense":
                    if (_offenseUsedPoints < playerItem.treeRow*4)
                        return;
                    break;
                case "Defense":
                    if (_defenseUsedPoints < playerItem.treeRow*4)
                        return;
                    break;
                default:
                    if (_utilityUsedPoints < playerItem.treeRow*4)
                        return;
                    break;
            }
            //Has enough points overall
            if (_usedPoints >= Client.LoginPacket.AllSummonerData.SummonerTalentsAndPoints.TalentPoints)
            {
                return;
            }

            //If it has a prerequisite mastery, check if points in it
            if (playerItem.prereq != 0)
            {
                var prereqMastery = Client.Masteries.Find(x => playerItem.prereq == x.id);
                if (prereqMastery.selectedRank != prereqMastery.ranks)
                {
                    return;
                }
            }

            playerItem.selectedRank += 1;
            foreach (var talent in Client.Masteries.Where(talent => playerItem.id == talent.id))
            {
                talent.selectedRank = playerItem.selectedRank;
            }

            RenderMasteries();
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
            var item = (MasteryItem) sender;
            var playerItem = (masteries) item.Tag;
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

                _playerItem.PlayerWins.Content = "Requires " + playerItem.treeRow*4 + " points in " + playerItem.tree;

                var isAtRequirement = true;
                switch (playerItem.tree)
                {
                    case "Offense":
                        if (_offenseUsedPoints < playerItem.treeRow*4)
                        {
                            isAtRequirement = false;
                        }
                        break;
                    case "Defense":
                        if (_defenseUsedPoints < playerItem.treeRow*4)
                        {
                            isAtRequirement = false;
                        }
                        break;
                    default:
                        if (_utilityUsedPoints < playerItem.treeRow*4)
                        {
                            isAtRequirement = false;
                        }
                        break;
                }

                if (isAtRequirement)
                {
                    if (playerItem.prereq != 0)
                    {
                        var prereqMastery = Client.Masteries.Find(x => playerItem.prereq == x.id);
                        _playerItem.PlayerWins.Content = "Requires " + prereqMastery.ranks + " points in " +
                                                         prereqMastery.name;
                    }
                }

                _playerItem.PlayerLeague.Content = playerItem.id;
                _playerItem.LevelLabel.Content = playerItem.selectedRank + "/" + playerItem.ranks;
                _playerItem.UsingLegendary.Visibility = Visibility.Hidden;

                var selectedRank = playerItem.selectedRank;
                if (selectedRank == 0)
                {
                    selectedRank = 1;
                }

                _playerItem.PlayerStatus.Text = ((string) playerItem.description[selectedRank - 1]).Replace("<br>",
                    Environment.NewLine);

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

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var mastery in Client.Masteries)
            {
                mastery.selectedRank = 0;
            }

            RenderMasteries();
        }

        private static List<TalentEntry> GetCurrentTalentEntries()
        {
            return (from mastery in Client.Masteries
                where mastery.selectedRank > 0
                select new TalentEntry
                {
                    Rank = mastery.selectedRank,
                    TalentId = mastery.id
                }).ToList();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (
                var MasteryPage in
                    Client.LoginPacket.AllSummonerData.MasteryBook.BookPages.Where(masteryPage => masteryPage.Current))
            {
                MasteryPage.TalentEntries = GetCurrentTalentEntries();
                MasteryPage.Name = MasteryTextBox.Text;
            }

            await Client.PVPNet.SaveMasteryBook(Client.LoginPacket.AllSummonerData.MasteryBook);
        }

        private void MasteryPageListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var mastery in Client.Masteries)
            {
                mastery.selectedRank = 0;
            }

            foreach (var masteryPage in Client.LoginPacket.AllSummonerData.MasteryBook.BookPages)
            {
                if (masteryPage.Current)
                {
                    masteryPage.Current = false;
                }

                if (Math.Abs(masteryPage.PageId - _masteryPageOrder[MasteryPageListView.SelectedIndex]) > .00001)
                {
                    continue;
                }

                masteryPage.Current = true;
                _selectedBook = masteryPage;
            }
            ChangeBook();
            RenderMasteries();
        }

        private void RevertButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var mastery in Client.Masteries)
            {
                mastery.selectedRank = 0;
            }

            foreach (var savedMastery in _selectedBook.TalentEntries)
            {
                var mastery1 = savedMastery;
                foreach (var mastery in Client.Masteries.Where(mastery => mastery.id == mastery1.TalentId))
                {
                    mastery.selectedRank = savedMastery.Rank;
                }
            }
        }

        private async void AddPageButton_Click(object sender, RoutedEventArgs e)
        {
            double pageId = 0;
            foreach (var item in Client.LoginPacket.AllSummonerData.MasteryBook.BookPages)
            {
                if (pageId <= item.PageId)
                {
                    pageId = item.PageId;
                    pageId++;
                }
            }
            MasteryBookPageDTO newPage = new MasteryBookPageDTO();
            newPage.SummonerId = Client.LoginPacket.AllSummonerData.Summoner.SumId;
            newPage.Name = "@@!PaG3!@@" + pageId;
            newPage.PageId = pageId;
            newPage.TalentEntries = new List<TalentEntry>();
            Client.LoginPacket.AllSummonerData.MasteryBook.BookPages.Add(newPage);
            await Client.PVPNet.SaveMasteryBook(Client.LoginPacket.AllSummonerData.MasteryBook);
            _masteryPageOrder.Add(pageId);
            MasteryPageListView.Items.Add(Client.LoginPacket.AllSummonerData.MasteryBook.BookPages.Count + " ");
        }
    }
}