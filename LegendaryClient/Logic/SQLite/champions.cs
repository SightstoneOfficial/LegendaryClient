#region

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;

#endregion

namespace LegendaryClient.Logic.SQLite
{
    public class Champions
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Title { get; set; }

        public string IconPath { get; set; }

        public BitmapImage Icon { get; set; }

        public string PortraitPath { get; set; }

        public string SplashPath { get; set; }

        public string DanceVideoPath { get; set; }

        public string Tags { get; set; }

        public string Description { get; set; }

        public string Quote { get; set; }

        public string QuoteAuthor { get; set; }

        public double Range { get; set; }

        public double MoveSpeed { get; set; }

        public double ArmorBase { get; set; }

        public double ArmorLevel { get; set; }

        public double ManaBase { get; set; }

        public double ManaLevel { get; set; }

        public double CriticalChanceBase { get; set; }

        public double CriticalChanceLevel { get; set; }

        public double ManaRegenBase { get; set; }

        public double ManaRegenLevel { get; set; }

        public double HealthRegenBase { get; set; }

        public double HealthRegenLevel { get; set; }

        public double MagicResistBase { get; set; }

        public double MagicResistLevel { get; set; }

        public double HealthBase { get; set; }

        public double HealthLevel { get; set; }

        public double AttackBase { get; set; }

        public double AttackLevel { get; set; }

        public int RatingDefense { get; set; }

        public int RatingMagic { get; set; }

        public int RatingDifficulty { get; set; }

        public int RatingAttack { get; set; }

        public string Tips { get; set; }

        public string OpponentTips { get; set; }

        public string SelectSoundPath { get; set; }

        public bool IsFavourite { get; set; }

        #region DDragon Data

        public string Lore { get; set; }
        public string ResourceType { get; set; }
        public ArrayList Skins { get; set; }
        public List<Spell> Spells { get; set; }

        #endregion

        public static Champions GetChampion(int id)
        {
            return Client.Champions.FirstOrDefault(c => c.Id == id);
        }

        public static Champions GetChampion(string name)
        {
            return Client.Champions.FirstOrDefault(c => c.Name == name);
        }
    }
}