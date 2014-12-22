#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Properties;

#endregion

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for ChampionDetailsPage.xaml
    /// </summary>
    public partial class ChampionDetailsPage
    {
        internal champions TheChamp;
        private Point _currentLocation;
        private Vector _moveOffset;


        public ChampionDetailsPage(int championId)
        {
            InitializeComponent();
            Change();

            RenderChampions(championId);
        }

        public ChampionDetailsPage(int championId, int skinId)
        {
            InitializeComponent();
            RenderChampions(championId);

            championSkins skin = championSkins.GetSkin(skinId);
            SkinName.Content = skin.displayName;
            string uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "champions", skin.splashPath);
            ChampionImage.Source = Client.GetImage(uriSource);
        }

        public void Change()
        {
            var themeAccent = new ResourceDictionary
            {
                Source = new Uri(Settings.Default.Theme)
            };
            Resources.MergedDictionaries.Add(themeAccent);
        }

        public void RenderChampions(int championId)
        {
            champions champ = champions.GetChampion(championId);
            TheChamp = champ;
            FavouriteLabel.Content = TheChamp.IsFavourite ? "Unfavourite" : "Favourite";
            ChampionName.Content = champ.displayName;
            ChampionTitle.Content = champ.title;
            ChampionProfileImage.Source = champ.icon;
            AttackProgressBar.Value = champ.ratingAttack;
            DefenseProgressBar.Value = champ.ratingDefense;
            AbilityProgressBar.Value = champ.ratingMagic;
            DifficultyProgressBar.Value = champ.ratingDifficulty;

            HPLabel.Content = string.Format("HP: {0} (+{1} per level)", champ.healthBase, champ.healthLevel);
            ResourceLabel.Content = string.Format("{0}: {1} (+{2} per level)", champ.ResourceType, champ.manaBase,
                champ.manaLevel);
            HPRegenLabel.Content = string.Format("HP/5: {0} (+{1} per level)", champ.healthRegenBase,
                champ.healthRegenLevel);
            ResourceRegenLabel.Content = string.Format("{0}/5: {1} (+{2} per level)", champ.ResourceType,
                champ.manaRegenBase, champ.manaRegenLevel);
            MagicResistLabel.Content = string.Format("MR: {0} (+{1} per level)", champ.magicResistBase,
                champ.magicResistLevel);
            ArmorLabel.Content = string.Format("Armor: {0} (+{1} per level)", champ.armorBase, champ.armorLevel);
            AttackDamageLabel.Content = string.Format("AD: {0} (+{1} per level)", champ.attackBase, champ.attackLevel);
            RangeLabel.Content = string.Format("Range: {0}", champ.range);
            MovementSpeedLabel.Content = string.Format("Speed: {0}", champ.moveSpeed);

            if (champ.Skins != null)
                foreach (Dictionary<string, object> skins in champ.Skins)
                {
                    int skin = Convert.ToInt32(skins["id"]);
                    var item = new ListViewItem();
                    var skinImage = new Image();
                    string uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "champions",
                        championSkins.GetSkin(skin).portraitPath);
                    skinImage.Source = Client.GetImage(uriSource);
                    skinImage.Width = 96.25;
                    skinImage.Height = 175;
                    skinImage.Stretch = Stretch.UniformToFill;
                    item.Tag = skin;
                    item.Content = skinImage;
                    SkinSelectListView.Items.Add(item);
                }

            if (champ.Spells != null)
                foreach (ChampionDetailAbility detailAbility in from sp in champ.Spells
                    let uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "spell", sp.Image)
                    select new ChampionDetailAbility
                    {
                        AbilityImage = {Source = Client.GetImage(uriSource)},
                        AbilityName = {Content = sp.Name},
                        AbilityDescription = {Text = sp.Description.Replace("<br>", Environment.NewLine)}
                    })
                    AbilityListView.Items.Add(detailAbility);

            ChampionImage.Source =
                Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champ.splashPath));

            if (champ.Lore != null)
                LoreText.Text = champ.Lore.Replace("<br>", Environment.NewLine);

            TipsText.Text = string.Format("Tips while playing {0}:{1}{2}{2}{2}Tips while playing aginst {0}:{3}",
                champ.displayName, champ.tips.Replace("*", Environment.NewLine + "*"), Environment.NewLine,
                champ.opponentTips.Replace("*", Environment.NewLine + "*"));
        }

        private void SkinSelectListView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item == null)
                return;

            if (item.Tag == null)
                return;

            championSkins skin = championSkins.GetSkin((int) item.Tag);
            SkinName.Content = skin.displayName;
            var fadingAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(0.2))
            };
            fadingAnimation.Completed += (eSender, eArgs) =>
            {
                string uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "champions",
                    skin.splashPath);
                ChampionImage.Source = Client.GetImage(uriSource);
                fadingAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5))
                };

                ChampionImage.BeginAnimation(OpacityProperty, fadingAnimation);
            };

            ChampionImage.BeginAnimation(OpacityProperty, fadingAnimation);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Visibility = Visibility.Hidden;
        }

        private void FavouriteButton_Click(object sender, RoutedEventArgs e)
        {
            TheChamp.IsFavourite = !TheChamp.IsFavourite;
            if (Settings.Default.FavouriteChamps == null)
            {
                var noNull = new List<int> {0};
                Settings.Default.FavouriteChamps = noNull.ToArray();
                Settings.Default.Save();
            }

            var tempList = new List<int>(Settings.Default.FavouriteChamps);
            if (tempList.Contains(TheChamp.id))
                tempList.Remove(TheChamp.id);
            else
                tempList.Add(TheChamp.id);

            Settings.Default.FavouriteChamps = tempList.ToArray();
            Settings.Default.Save();

            FavouriteLabel.Content = TheChamp.IsFavourite ? "Unfavourite" : "Favourite";
        }

        //Cool way to allow user to drag the grid arround

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
                return;

            _currentLocation = Mouse.GetPosition(MouseGrid);
            _moveOffset = new Vector(tt.X, tt.Y);
            Grid.CaptureMouse();
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (!Grid.IsMouseCaptured)
                return;

            Vector offset = Point.Subtract(e.GetPosition(MouseGrid), _currentLocation);

            tt.X = _moveOffset.X + offset.X;
            tt.Y = _moveOffset.Y + offset.Y;
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Grid.ReleaseMouseCapture();
        }
    }
}