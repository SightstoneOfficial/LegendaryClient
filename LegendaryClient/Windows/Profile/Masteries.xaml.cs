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
        private readonly List<double> MasteryPageOrder = new List<double>();
        private int DefenseUsedPoints;
        private int OffenseUsedPoints;
        private LargeChatPlayer PlayerItem;
        private MasteryBookPageDTO SelectedBook;
        private int UsedPoints;
        private int UtilityUsedPoints;
        //private static readonly ILog log = LogManager.GetLogger(typeof(Masteries));
        public Masteries()
        {
            InitializeComponent();
            MasteryPageListView.Items.Clear();
            for (int i = 1; i <= Client.LoginPacket.AllSummonerData.MasteryBook.BookPages.Count; i++)
                MasteryPageListView.Items.Add(i);

            double selectedPageId = 0;
            foreach (MasteryBookPageDTO masteryPage in Client.LoginPacket.AllSummonerData.MasteryBook.BookPages)
            {
                MasteryPageOrder.Add(masteryPage.PageId);
                if (masteryPage.Current)
                    selectedPageId = masteryPage.PageId;
            }

            MasteryPageOrder.Sort();
            MasteryPageListView.SelectedIndex = MasteryPageOrder.IndexOf(selectedPageId);
        }

        public void ChangeBook()
        {
            MasteryTextBox.Text = SelectedBook.Name;
            foreach (TalentEntry talent in SelectedBook.TalentEntries)
                foreach (masteries mastery in Client.Masteries.Where(mastery => mastery.id == talent.TalentId))
                    mastery.selectedRank = talent.Rank;
        }

        public void RenderMasteries()
        {
            OffenseListView.Items.Clear();
            DefenseListView.Items.Clear();
            UtilityListView.Items.Clear();

            UsedPoints = 0;
            OffenseUsedPoints = 0;
            DefenseUsedPoints = 0;
            UtilityUsedPoints = 0;
            foreach (masteries mastery in Client.Masteries)
            {
                bool isOffense = false;
                bool isDefense = false;
                bool isUtility = false;

                var item = new MasteryItem
                {
                    RankLabel = {Content = "0/" + mastery.ranks},
                    MasteryImage = {Source = mastery.icon, Opacity = 0.4},
                    Margin = new Thickness(2, 2, 2, 2)
                };

                if (mastery.selectedRank > 0)
                {
                    item.MasteryImage.Opacity = 1;
                    item.RankLabel.Content = mastery.selectedRank + "/" + mastery.ranks;
                }

                UsedPoints += mastery.selectedRank;

                switch (mastery.tree)
                {
                    case "Offense":
                        OffenseUsedPoints += mastery.selectedRank;
                        isOffense = true;
                        OffenseListView.Items.Add(item);
                        break;
                    case "Defense":
                        DefenseUsedPoints += mastery.selectedRank;
                        isDefense = true;
                        DefenseListView.Items.Add(item);
                        break;
                    default:
                        UtilityUsedPoints += mastery.selectedRank;
                        isUtility = true;
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
                        OffenseListView.Items.Add(rect);
                    else if (isDefense)
                    {
                        DefenseListView.Items.Add(rect);
                        if (mastery.id == 4253)
                            DefenseListView.Items.Add(new Rectangle
                            {
                                Width = 64,
                                Height = 64,
                                Margin = new Thickness(2, 2, 2, 2)
                            });
                    }
                    else
                    {
                        UtilityListView.Items.Add(rect);
                        if (mastery.id == 4353)
                            UtilityListView.Items.Add(new Rectangle
                            {
                                Width = 64,
                                Height = 64,
                                Margin = new Thickness(2, 2, 2, 2)
                            });
                    }
                }

                item.Tag = mastery;
                item.MouseWheel += item_MouseWheel;
                item.MouseLeftButtonDown += item_MouseLeftButtonDown;
                item.MouseRightButtonDown += item_MouseRightButtonDown;
                item.MouseMove += item_MouseMove;
                item.MouseLeave += item_MouseLeave;
            }

            UsedLabel.Content = "Points Used: " + UsedPoints;
            FreeLabel.Content = "Points Free: " +
                                (Client.LoginPacket.AllSummonerData.SummonerTalentsAndPoints.TalentPoints - UsedPoints);
            OffenseLabel.Content = "Offense: " + OffenseUsedPoints;
            DefenseLabel.Content = "Defense: " + DefenseUsedPoints;
            UtilityLabel.Content = "Utility: " + UtilityUsedPoints;
        }

        private void item_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                item_MouseLeftButtonDown(sender, null);
            else
                item_MouseRightButtonDown(sender, null);
        }

        private void item_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = (MasteryItem) sender;
            var playerItem = (masteries) item.Tag;
            if (playerItem.selectedRank == 0)
                return;

            //Temp check - make it so you can remove masteries even if they are above mastery if enough points in tree
            List<masteries> filteredMasteries =
                Client.Masteries.FindAll(x => x.tree == playerItem.tree && x.treeRow > playerItem.treeRow);
            if (filteredMasteries.Any(checkMastery => checkMastery.selectedRank > 0))
                return;

            playerItem.selectedRank -= 1;
            foreach (masteries talent in Client.Masteries.Where(talent => playerItem.id == talent.id))
                talent.selectedRank = playerItem.selectedRank;

            RenderMasteries();
        }

        private void item_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = (MasteryItem) sender;
            var playerItem = (masteries) item.Tag;
            //Max rank
            if (playerItem.selectedRank == playerItem.ranks)
                return;

            //Has enough points in tree
            switch (playerItem.tree)
            {
                case "Offense":
                    if (OffenseUsedPoints < playerItem.treeRow*4)
                        return;
                    break;
                case "Defense":
                    if (DefenseUsedPoints < playerItem.treeRow*4)
                        return;
                    break;
                default:
                    if (UtilityUsedPoints < playerItem.treeRow*4)
                        return;
                    break;
            }
            //Has enough points overall
            if (UsedPoints >= Client.LoginPacket.AllSummonerData.SummonerTalentsAndPoints.TalentPoints)
                return;

            //If it has a prerequisite mastery, check if points in it
            if (playerItem.prereq != 0)
            {
                masteries prereqMastery = Client.Masteries.Find(x => playerItem.prereq == x.id);
                if (prereqMastery.selectedRank != prereqMastery.ranks)
                    return;
            }

            playerItem.selectedRank += 1;
            foreach (masteries talent in Client.Masteries.Where(talent => playerItem.id == talent.id))
                talent.selectedRank = playerItem.selectedRank;

            RenderMasteries();
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
            var item = (MasteryItem) sender;
            var playerItem = (masteries) item.Tag;
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

                PlayerItem.PlayerWins.Content = "Requires " + playerItem.treeRow*4 + " points in " + playerItem.tree;

                bool isAtRequirement = true;
                switch (playerItem.tree)
                {
                    case "Offense":
                        if (OffenseUsedPoints < playerItem.treeRow*4)
                            isAtRequirement = false;
                        break;
                    case "Defense":
                        if (DefenseUsedPoints < playerItem.treeRow*4)
                            isAtRequirement = false;
                        break;
                    default:
                        if (UtilityUsedPoints < playerItem.treeRow*4)
                            isAtRequirement = false;
                        break;
                }

                if (isAtRequirement)
                {
                    if (playerItem.prereq != 0)
                    {
                        masteries prereqMastery = Client.Masteries.Find(x => playerItem.prereq == x.id);
                        PlayerItem.PlayerWins.Content = "Requires " + prereqMastery.ranks + " points in " +
                                                        prereqMastery.name;
                    }
                }

                PlayerItem.PlayerLeague.Content = playerItem.id;
                PlayerItem.LevelLabel.Content = playerItem.selectedRank + "/" + playerItem.ranks;
                PlayerItem.UsingLegendary.Visibility = Visibility.Hidden;

                int selectedRank = playerItem.selectedRank;
                if (selectedRank == 0)
                    selectedRank = 1;

                PlayerItem.PlayerStatus.Text = ((string) playerItem.description[selectedRank - 1]).Replace("<br>",
                    Environment.NewLine);

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

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (masteries mastery in Client.Masteries)
                mastery.selectedRank = 0;

            RenderMasteries();
        }

        private List<TalentEntry> GetCurrentTalentEntries()
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
            foreach (MasteryBookPageDTO masteryPage in Client.LoginPacket.AllSummonerData.MasteryBook.BookPages)
            {
                if (!masteryPage.Current)
                    continue;

                masteryPage.TalentEntries = GetCurrentTalentEntries();
                masteryPage.Name = MasteryTextBox.Text;
            }

            await Client.PVPNet.SaveMasteryBook(Client.LoginPacket.AllSummonerData.MasteryBook);
        }

        private void MasteryPageListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (masteries mastery in Client.Masteries)
                mastery.selectedRank = 0;

            foreach (MasteryBookPageDTO masteryPage in Client.LoginPacket.AllSummonerData.MasteryBook.BookPages)
            {
                if (masteryPage.Current)
                    masteryPage.Current = false;

                if (masteryPage.PageId != MasteryPageOrder[MasteryPageListView.SelectedIndex])
                    continue;

                masteryPage.Current = true;
                SelectedBook = masteryPage;
            }

            ChangeBook();
            RenderMasteries();
        }

        private void RevertButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (masteries mastery in Client.Masteries)
                mastery.selectedRank = 0;

            foreach (TalentEntry savedMastery in SelectedBook.TalentEntries)
            {
                TalentEntry mastery1 = savedMastery;
                foreach (masteries mastery in Client.Masteries.Where(mastery => mastery.id == mastery1.TalentId))
                    mastery.selectedRank = savedMastery.Rank;
            }
        }
    }
}