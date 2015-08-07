using Sightstone.Controls;
using Sightstone.Logic;
using Sightstone.Logic.MultiUser;
using Sightstone.Logic.SQLite;
using Sightstone.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Sightstone.Windows
{
    /// <summary>
    ///     Interaction logic for ChampionDetailsPage.xaml
    /// </summary>
    public partial class ChampionDetailsPage
    {
        internal champions TheChamp;
        private Point currentLocation;
        private Vector moveOffset;

        public ChampionDetailsPage(int championId)
        {
            InitializeComponent();
            RenderChampions(championId);
        }

        public ChampionDetailsPage(int championId, int skinId)
        {
            InitializeComponent();
            RenderChampions(championId);

            championSkins skin = championSkins.GetSkin(skinId);
            SkinName.Content = skin.displayName;
            string UriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "champions", skin.splashPath);
            ChampionImage.Source = Client.GetImage(UriSource);
        }

        public void RenderChampions(int championId)
        {
            champions champ = champions.GetChampion(championId);
            TheChamp = champ;
            FavoUriteLabel.Content = TheChamp.IsFavoUrite ? "UnfavoUrite" : "FavoUrite";
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
                foreach (var skins in champ.Skins)
                {
                    int skin = Convert.ToInt32(skins.Id);
                    var item = new ListViewItem();
                    var skinImage = new Image();
                    string UriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "champions", skins.PortraitPath);
                    skinImage.Source = Client.GetImage(UriSource);
                    skinImage.Width = 96.25;
                    skinImage.Height = 175;
                    skinImage.Stretch = Stretch.UniformToFill;
                    item.Tag = skin;
                    item.Content = skinImage;
                    SkinSelectListView.Items.Add(item);
                }

            if (champ.Spells != null)
                foreach (ChampionDetailAbility detailAbility in from sp in champ.Spells
                    let UriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "spell", sp.Image)
                    select new ChampionDetailAbility
                    {
                        AbilityImage = {Source = Client.GetImage(UriSource)},
                        AbilityName = {Content = sp.Name},
                        AbilityDescription = {Text = sp.Description.Replace("<br>", Environment.NewLine)}
                    })
                    AbilityListView.Items.Add(detailAbility);

            ChampionImage.Source =
                Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champ.splashPath));

            if (champ.description != null)
                LoreText.Text = champ.description;

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
                string UriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "champions",
                    skin.splashPath);
                ChampionImage.Source = Client.GetImage(UriSource);
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

        private void FavoUriteButton_Click(object sender, RoutedEventArgs e)
        {
            TheChamp.IsFavoUrite = !TheChamp.IsFavoUrite;
            if (Settings.Default.FavoUriteChamps == null)
            {
                var noNull = new List<int> {0};
                Settings.Default.FavoUriteChamps = noNull.ToArray();
                Settings.Default.Save();
            }

            var tempList = new List<int>(Settings.Default.FavoUriteChamps);
            if (tempList.Contains(TheChamp.id))
                tempList.Remove(TheChamp.id);
            else
                tempList.Add(TheChamp.id);

            Settings.Default.FavoUriteChamps = tempList.ToArray();
            Settings.Default.Save();

            FavoUriteLabel.Content = TheChamp.IsFavoUrite ? "UnfavoUrite" : "FavoUrite";
        }

        //Cool way to allow user to drag the grid arround

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
                return;

            currentLocation = Mouse.GetPosition(MouseGrid);
            moveOffset = new Vector(tt.X, tt.Y);
            Grid.CaptureMouse();
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (!Grid.IsMouseCaptured)
                return;

            Vector offset = Point.Subtract(e.GetPosition(MouseGrid), currentLocation);

            tt.X = moveOffset.X + offset.X;
            tt.Y = moveOffset.Y + offset.Y;
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Grid.ReleaseMouseCapture();
        }
    }
}