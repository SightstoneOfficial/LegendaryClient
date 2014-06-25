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
        public ChampionDetailsPage(int ChampionId)
        {
            InitializeComponent();
            RenderChampions(ChampionId);
        }

        public ChampionDetailsPage(int ChampionId, int SkinID)
        {
            InitializeComponent();
            RenderChampions(ChampionId);
            championSkins skin = championSkins.GetSkin(SkinID);
            SkinName.Content = skin.displayName;
            string uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "champions", skin.splashPath);
            ChampionImage.Source = Client.GetImage(uriSource);
        }

        public void RenderChampions(int ChampionId)
        {
            champions Champ = champions.GetChampion(ChampionId);
            ChampionName.Content = Champ.displayName;
            ChampionTitle.Content = Champ.title;
            ChampionProfileImage.Source = Champ.icon;
            AttackProgressBar.Value = Champ.ratingAttack;
            DefenseProgressBar.Value = Champ.ratingDefense;
            AbilityProgressBar.Value = Champ.ratingMagic;
            DifficultyProgressBar.Value = Champ.ratingDifficulty;

            HPLabel.Content = string.Format("HP: {0} (+{1} per level)", Champ.healthBase, Champ.healthLevel);
            ResourceLabel.Content = string.Format("{0}: {1} (+{2} per level)", Champ.ResourceType, Champ.manaBase, Champ.manaLevel);
            HPRegenLabel.Content = string.Format("HP/5: {0} (+{1} per level)", Champ.healthRegenBase, Champ.healthRegenLevel);
            ResourceRegenLabel.Content = string.Format("{0}/5: {1} (+{2} per level)", Champ.ResourceType, Champ.manaRegenBase, Champ.manaRegenLevel);
            MagicResistLabel.Content = string.Format("MR: {0} (+{1} per level)", Champ.magicResistBase, Champ.magicResistLevel);
            ArmorLabel.Content = string.Format("Armor: {0} (+{1} per level)", Champ.armorBase, Champ.armorLevel);
            AttackDamageLabel.Content = string.Format("AD: {0} (+{1} per level)", Champ.attackBase, Champ.attackLevel);
            RangeLabel.Content = string.Format("Range: {0}", Champ.range);
            MovementSpeedLabel.Content = string.Format("Speed: {0}", Champ.moveSpeed);

            foreach (Dictionary<string, object> Skins in Champ.Skins)
            {
                int Skin = Convert.ToInt32(Skins["id"]);
                ListViewItem item = new ListViewItem();
                Image skinImage = new Image();
                var uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "champions", championSkins.GetSkin(Skin).portraitPath);
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

            ChampionImage.Source = Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", Champ.splashPath));

            LoreText.Text = Champ.Lore.Replace("<br>", Environment.NewLine);
            TipsText.Text = string.Format("Tips while playing {0}:{1}{2}{2}{2}Tips while playing aginst {0}:{3}", Champ.displayName, Champ.tips.Replace("*", Environment.NewLine + "*"), Environment.NewLine, Champ.opponentTips.Replace("*", Environment.NewLine + "*"));
        }

        private void SkinSelectListView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null)
            {
                if (item.Tag != null)
                {
                    championSkins skin = championSkins.GetSkin((int)item.Tag);
                    SkinName.Content = skin.displayName;
                    DoubleAnimation fadingAnimation = new DoubleAnimation();
                    fadingAnimation.From = 1;
                    fadingAnimation.To = 0;
                    fadingAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.2));
                    fadingAnimation.Completed += (eSender, eArgs) =>
                    {
                        string uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "champions", skin.splashPath);
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
    }
}