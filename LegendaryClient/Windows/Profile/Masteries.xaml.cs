using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect.RiotObjects.Platform.Summoner.Masterybook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    /// Interaction logic for Masteries.xaml
    /// </summary>
    public partial class Masteries : Page
    {
        private MasteryBookPageDTO SelectedBook;
        private LargeChatPlayer PlayerItem;
        private int UsedPoints = 0;
        private int OffenseUsedPoints = 0;
        private int DefenseUsedPoints = 0;
        private int UtilityUsedPoints = 0;
        private List<double> MasteryPageOrder = new List<double>();
        public Masteries()
        {
            InitializeComponent();
            MasteryPageListView.Items.Clear();
            for (int i = 1; i <= Client.LoginPacket.AllSummonerData.MasteryBook.BookPages.Count; i++)
                MasteryPageListView.Items.Add(i);
            double SelectedPageId = 0;
            foreach (MasteryBookPageDTO MasteryPage in Client.LoginPacket.AllSummonerData.MasteryBook.BookPages)
            {
                MasteryPageOrder.Add(MasteryPage.PageId);
                if (MasteryPage.Current)
                {
                    SelectedPageId = MasteryPage.PageId;
                }
            }
            MasteryPageOrder.Sort();
            MasteryPageListView.SelectedIndex = MasteryPageOrder.IndexOf(SelectedPageId);
        }

        public void ChangeBook()
        {
            MasteryTextBox.Text = SelectedBook.Name;
            foreach (TalentEntry talent in SelectedBook.TalentEntries)
            {
                foreach (masteries Mastery in Client.Masteries)
                {
                    if (Mastery.id == talent.TalentId)
                    {
                        Mastery.selectedRank = talent.Rank;
                    }
                }
            }
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
            foreach (masteries Mastery in Client.Masteries)
            {
                bool IsOffense = false;
                bool IsDefense = false;
                bool IsUtility = false;

                MasteryItem item = new MasteryItem();
                item.RankLabel.Content = "0/" + Mastery.ranks;
                item.MasteryImage.Source = Mastery.icon;
                item.MasteryImage.Opacity = 0.4;
                item.Margin = new Thickness(2, 2, 2, 2);

                if (Mastery.selectedRank > 0)
                {
                    item.MasteryImage.Opacity = 1;
                    item.RankLabel.Content = Mastery.selectedRank + "/" + Mastery.ranks;
                }

                UsedPoints += Mastery.selectedRank;

                switch (Mastery.tree)
                {
                    case "Offense":
                        OffenseUsedPoints += Mastery.selectedRank;
                        IsOffense = true;
                        OffenseListView.Items.Add(item);
                        break;
                    case "Defense":
                        DefenseUsedPoints += Mastery.selectedRank;
                        IsDefense = true;
                        DefenseListView.Items.Add(item);
                        break;
                    default:
                        UtilityUsedPoints += Mastery.selectedRank;
                        IsUtility = true;
                        UtilityListView.Items.Add(item);
                        break;
                }

                //Add spaces
                if (Mastery.id == 4152 ||
                    Mastery.id == 4222 ||
                    Mastery.id == 4253 ||
                    Mastery.id == 4314 ||
                    Mastery.id == 4344 ||
                    Mastery.id == 4353)
                {
                    Rectangle rect = new Rectangle();
                    rect.Width = 64;
                    rect.Height = 64;
                    rect.Margin = new Thickness(2, 2, 2, 2);
                    if (IsOffense)
                        OffenseListView.Items.Add(rect);
                    if (IsDefense)
                        DefenseListView.Items.Add(rect);
                    if (IsUtility)
                        UtilityListView.Items.Add(rect);
                }

                item.Tag = Mastery;
                item.MouseWheel += item_MouseWheel;
                item.MouseLeftButtonDown += item_MouseLeftButtonDown;
                item.MouseRightButtonDown += item_MouseRightButtonDown;
                item.MouseMove += item_MouseMove;
                item.MouseLeave += item_MouseLeave;
            }

            UsedLabel.Content = "Points Used: " + UsedPoints;
            FreeLabel.Content = "Points Free: " + (Client.LoginPacket.AllSummonerData.SummonerTalentsAndPoints.TalentPoints - UsedPoints);
            OffenseLabel.Content = "Offense: " + OffenseUsedPoints;
            DefenseLabel.Content = "Defense: " + DefenseUsedPoints;
            UtilityLabel.Content = "Utility: " + UtilityUsedPoints;
        }

        void item_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                item_MouseLeftButtonDown(sender, null);
            else
                item_MouseRightButtonDown(sender, null);
        }

        void item_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MasteryItem item = (MasteryItem)sender;
            masteries playerItem = (masteries)item.Tag;
            if (playerItem.selectedRank == 0)
                return;

            //Temp check - make it so you can remove masteries even if they are above mastery if enough points in tree
            List<masteries> FilteredMasteries = Client.Masteries.FindAll(x => x.tree == playerItem.tree && x.treeRow > playerItem.treeRow);
            foreach (masteries checkMastery in FilteredMasteries)
            {
                if (checkMastery.selectedRank > 0)
                    return;
            }
            playerItem.selectedRank -= 1;
            foreach (masteries talent in Client.Masteries)
                if (playerItem.id == talent.id)
                    talent.selectedRank = playerItem.selectedRank;
            RenderMasteries();
        }

        void item_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MasteryItem item = (MasteryItem)sender;
            masteries playerItem = (masteries)item.Tag;
            //Max rank
            if (playerItem.selectedRank == playerItem.ranks)
                return;
            //Has enough points in tree
            switch (playerItem.tree)
            {
                case "Offense":
                    if (OffenseUsedPoints < playerItem.treeRow * 4)
                        return;
                    break;
                case "Defense":
                    if (DefenseUsedPoints < playerItem.treeRow * 4)
                        return;
                    break;
                default:
                    if (UtilityUsedPoints < playerItem.treeRow * 4)
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
            foreach (masteries talent in Client.Masteries)
                if (playerItem.id == talent.id)
                    talent.selectedRank = playerItem.selectedRank;
            RenderMasteries();
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
            MasteryItem item = (MasteryItem)sender;
            masteries playerItem = (masteries)item.Tag;
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

                PlayerItem.PlayerWins.Content = "Requires " + playerItem.treeRow * 4 + " points in " + playerItem.tree;

                bool IsAtRequirement = true;
                switch (playerItem.tree)
                {
                    case "Offense":
                        if (OffenseUsedPoints < playerItem.treeRow * 4)
                            IsAtRequirement = false;
                        break;
                    case "Defense":
                        if (DefenseUsedPoints < playerItem.treeRow * 4)
                            IsAtRequirement = false;
                        break;
                    default:
                        if (UtilityUsedPoints < playerItem.treeRow * 4)
                            IsAtRequirement = false;
                        break;
                }

                if (IsAtRequirement)
                {
                    if (playerItem.prereq != 0)
                    {
                        masteries prereqMastery = Client.Masteries.Find(x => playerItem.prereq == x.id);
                        PlayerItem.PlayerWins.Content = "Requires " + prereqMastery.ranks + " points in " + prereqMastery.name;
                    }
                }

                PlayerItem.PlayerLeague.Content = playerItem.id;
                PlayerItem.LevelLabel.Content = playerItem.selectedRank + "/" + playerItem.ranks;
                PlayerItem.UsingLegendary.Visibility = System.Windows.Visibility.Hidden;

                int SelectedRank = playerItem.selectedRank;
                if (SelectedRank == 0)
                    SelectedRank = 1;
                PlayerItem.PlayerStatus.Text = ((string)playerItem.description[SelectedRank - 1]).Replace("<br>", Environment.NewLine);

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

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (masteries mastery in Client.Masteries)
            {
                mastery.selectedRank = 0;
            }
            RenderMasteries();
        }

        private List<TalentEntry> GetCurrentTalentEntries()
        {
            List<TalentEntry> talentEntries = new List<TalentEntry>();
            foreach (masteries mastery in Client.Masteries)
            {
                if (mastery.selectedRank > 0)
                {
                    TalentEntry talentEntry = new TalentEntry();
                    talentEntry.Rank = mastery.selectedRank;
                    talentEntry.TalentId = mastery.id;
                    talentEntries.Add(talentEntry);
                }
            }
            return talentEntries;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (MasteryBookPageDTO MasteryPage in Client.LoginPacket.AllSummonerData.MasteryBook.BookPages)
            {
                if (MasteryPage.Current)
                {
                    MasteryPage.TalentEntries = GetCurrentTalentEntries();
                    MasteryPage.Name = MasteryTextBox.Text;
                }
            }
            await Client.PVPNet.SaveMasteryBook(Client.LoginPacket.AllSummonerData.MasteryBook);
        }

        private void MasteryPageListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (masteries mastery in Client.Masteries)
            {
                mastery.selectedRank = 0;
            }
            foreach (MasteryBookPageDTO MasteryPage in Client.LoginPacket.AllSummonerData.MasteryBook.BookPages)
            {
                if (MasteryPage.Current)
                {
                    MasteryPage.Current = false;
                }
                if (MasteryPage.PageId == MasteryPageOrder[MasteryPageListView.SelectedIndex])
                {
                    MasteryPage.Current = true;
                    SelectedBook = MasteryPage;
                }
            }
            ChangeBook();
            RenderMasteries();
        }

        private void RevertButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (masteries mastery in Client.Masteries)
            {
                mastery.selectedRank = 0;
            }
            foreach (TalentEntry savedMastery in SelectedBook.TalentEntries)
            {
                foreach (masteries mastery in Client.Masteries)
                {
                    if (mastery.id == savedMastery.TalentId)
                        mastery.selectedRank = savedMastery.Rank;
                }
            }
        }
    }
}