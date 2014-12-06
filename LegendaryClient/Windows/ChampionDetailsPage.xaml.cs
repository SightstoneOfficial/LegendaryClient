using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for ChampionDetailsPage.xaml
    /// </summary>
    public partial class ChampionDetailsPage : Page
    {
        internal Champions TheChamp;

        private Point CurrentLocation;


        public ChampionDetailsPage(int ChampionId)
        {
            InitializeComponent();
            RenderChampions(ChampionId);
        }

        public ChampionDetailsPage(int ChampionId, int SkinID)
        {
            InitializeComponent();
            RenderChampions(ChampionId);
            ChampionSkins skin = ChampionSkins.GetSkin(SkinID);
            SkinName.Content = skin.DisplayName;
            string uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "champions", skin.SplashPath);
            ChampionImage.Source = Client.GetImage(uriSource);
        }

        public void RenderChampions(int ChampionId)
        {
            Champions Champ = Champions.GetChampion(ChampionId);
            TheChamp = Champ;
            if (TheChamp.IsFavourite)
                FavouriteLabel.Content = "Unfavourite";
            else
                FavouriteLabel.Content = "Favourite";
            ChampionName.Content = Champ.DisplayName;
            ChampionTitle.Content = Champ.Title;
            ChampionProfileImage.Source = Champ.Icon;
            AttackProgressBar.Value = Champ.RatingAttack;
            DefenseProgressBar.Value = Champ.RatingDefense;
            AbilityProgressBar.Value = Champ.RatingMagic;
            DifficultyProgressBar.Value = Champ.RatingDifficulty;

            HPLabel.Content = string.Format("HP: {0} (+{1} per level)", Champ.HealthBase, Champ.HealthLevel);
            ResourceLabel.Content = string.Format("{0}: {1} (+{2} per level)", Champ.ResourceType, Champ.ManaBase, Champ.ManaLevel);
            HPRegenLabel.Content = string.Format("HP/5: {0} (+{1} per level)", Champ.HealthRegenBase, Champ.HealthRegenLevel);
            ResourceRegenLabel.Content = string.Format("{0}/5: {1} (+{2} per level)", Champ.ResourceType, Champ.ManaRegenBase, Champ.ManaRegenLevel);
            MagicResistLabel.Content = string.Format("MR: {0} (+{1} per level)", Champ.MagicResistBase, Champ.MagicResistLevel);
            ArmorLabel.Content = string.Format("Armor: {0} (+{1} per level)", Champ.ArmorBase, Champ.ArmorLevel);
            AttackDamageLabel.Content = string.Format("AD: {0} (+{1} per level)", Champ.AttackBase, Champ.AttackLevel);
            RangeLabel.Content = string.Format("Range: {0}", Champ.Range);
            MovementSpeedLabel.Content = string.Format("Speed: {0}", Champ.MoveSpeed);

            foreach (Dictionary<string, object> Skins in Champ.Skins)
            {
                int Skin = Convert.ToInt32(Skins["id"]);
                ListViewItem item = new ListViewItem();
                Image skinImage = new Image();
                var uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "champions", ChampionSkins.GetSkin(Skin).PortraitPath);
                skinImage.Source = Client.GetImage(uriSource);
                skinImage.Width = 96.25;
                skinImage.Height = 175;
                skinImage.Stretch = Stretch.UniformToFill;
                item.Tag = Skin;
                item.Content = skinImage;
                SkinSelectListView.Items.Add(item);
            }

            foreach (Spell Sp in Champ.Spells)
            {
                ChampionDetailAbility detailAbility = new ChampionDetailAbility();
                detailAbility.DataContext = Sp;
                var uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "spell", Sp.Image);
                detailAbility.AbilityImage.Source = Client.GetImage(uriSource);
                AbilityListView.Items.Add(detailAbility);
            }

            ChampionImage.Source = Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", Champ.SplashPath));

            LoreText.Text = Champ.Lore.Replace("<br>", Environment.NewLine);
            TipsText.Text = string.Format("Tips while playing {0}:{1}{2}{2}{2}Tips while playing aginst {0}:{3}", Champ.DisplayName, Champ.Tips.Replace("*", Environment.NewLine + "*"), Environment.NewLine, Champ.OpponentTips.Replace("*", Environment.NewLine + "*"));
        }

        private void SkinSelectListView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null)
            {
                if (item.Tag != null)
                {
                    ChampionSkins skin = ChampionSkins.GetSkin((int)item.Tag);
                    SkinName.Content = skin.DisplayName;
                    DoubleAnimation fadingAnimation = new DoubleAnimation();
                    fadingAnimation.From = 1;
                    fadingAnimation.To = 0;
                    fadingAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.2));
                    fadingAnimation.Completed += (eSender, eArgs) =>
                    {
                        string uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "champions", skin.SplashPath);
                        ChampionImage.Source = Client.GetImage(uriSource);
                        fadingAnimation = new DoubleAnimation();
                        fadingAnimation.From = 0;
                        fadingAnimation.To = 1;
                        fadingAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));

                        ChampionImage.BeginAnimation(Image.OpacityProperty, fadingAnimation);
                    };

                    ChampionImage.BeginAnimation(Image.OpacityProperty, fadingAnimation);
                }
            }
        }

        private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Client.OverlayContainer.Visibility = Visibility.Hidden;
        }

        private void FavouriteButton_Click(object sender, RoutedEventArgs e)
        {
            TheChamp.IsFavourite = !TheChamp.IsFavourite;
            if (Properties.Settings.Default.FavouriteChamps == null)
            {
                List<Int32> NoNull = new List<int>();
                NoNull.Add(0);
                Properties.Settings.Default.FavouriteChamps = NoNull.ToArray();
                Properties.Settings.Default.Save();
            }

            List<Int32> TempList = new List<int>(Properties.Settings.Default.FavouriteChamps);
            if (TempList.Contains(TheChamp.Id))
                TempList.Remove(TheChamp.Id);
            else
                TempList.Add(TheChamp.Id);

            Properties.Settings.Default.FavouriteChamps = TempList.ToArray();
            Properties.Settings.Default.Save();

            if (TheChamp.IsFavourite)
                FavouriteLabel.Content = "Unfavourite";
            else
                FavouriteLabel.Content = "Favourite";
        }

        //Cool way to allow user to drag the grid arround
        private Vector MoveOffset;
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                CurrentLocation = Mouse.GetPosition(MouseGrid);
                MoveOffset = new Vector(tt.X, tt.Y);
                Grid.CaptureMouse();
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (Grid.IsMouseCaptured)
            {
                Vector offset = Point.Subtract(e.GetPosition(MouseGrid), CurrentLocation);

                tt.X = MoveOffset.X + offset.X;
                tt.Y = MoveOffset.Y + offset.Y;
            }
        }
        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Grid.ReleaseMouseCapture();
        }
    }
}