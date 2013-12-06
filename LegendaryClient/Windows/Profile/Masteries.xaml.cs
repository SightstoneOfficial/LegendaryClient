using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect.RiotObjects.Platform.Summoner.Masterybook;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    /// Interaction logic for Masteries.xaml
    /// </summary>
    public partial class Masteries : Page
    {
        private MasteryBookPageDTO SelectedBook;
        private LargeChatPlayer PlayerItem;

        public Masteries()
        {
            InitializeComponent();
            MasteryPageListView.Items.Clear();
            foreach (var MasteryPage in Client.LoginPacket.AllSummonerData.MasteryBook.BookPages)
            {
                if (MasteryPage.Current)
                    SelectedBook = MasteryPage;
            }

            if (SelectedBook == null)
            {
                SelectedBook = Client.LoginPacket.AllSummonerData.MasteryBook.BookPages[0];
            }
            ChangeBook();
            RenderMasteries();
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

            int UsedPoints = 0;
            foreach (masteries Mastery in Client.Masteries)
            {
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
                        OffenseListView.Items.Add(item);
                        break;
                    case "Defense":
                        DefenseListView.Items.Add(item);
                        break;
                    default:
                        UtilityListView.Items.Add(item);
                        break;
                }

                item.Tag = Mastery;

                item.MouseLeftButtonDown += item_MouseLeftButtonDown;
                item.MouseRightButtonDown += item_MouseRightButtonDown;
                item.MouseMove += item_MouseMove;
                item.MouseLeave += item_MouseLeave;
            }

            UsedLabel.Content = "Points Used: " + UsedPoints;
            FreeLabel.Content = "Points Free: " + (Client.LoginPacket.AllSummonerData.SummonerTalentsAndPoints.TalentPoints - UsedPoints);
        }

        void item_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MasteryItem item = (MasteryItem)sender;
            masteries playerItem = (masteries)item.Tag;
            if (playerItem.selectedRank == 0)
                return;
            playerItem.selectedRank -= 1;
            RenderMasteries();
        }

        void item_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MasteryItem item = (MasteryItem)sender; 
            masteries playerItem = (masteries)item.Tag;
            if (playerItem.selectedRank == playerItem.ranks)
                return;
            playerItem.selectedRank += 1;
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

                //Only load once
                PlayerItem.ProfileImage.Source = playerItem.icon;
                PlayerItem.PlayerName.Content = playerItem.name;

                PlayerItem.PlayerName.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                if (PlayerItem.PlayerName.DesiredSize.Width > 250) //Make title fit in item
                    PlayerItem.Width = PlayerItem.PlayerName.DesiredSize.Width;
                else
                    PlayerItem.Width = 250;

                PlayerItem.PlayerWins.Content = "Requires " + playerItem.treeRow * 4 + " points in " + playerItem.tree;
                PlayerItem.PlayerLeague.Content = "";
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
    }
}