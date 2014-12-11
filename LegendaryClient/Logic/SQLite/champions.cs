#region

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;

#endregion

namespace LegendaryClient.Logic.SQLite
{
    public class champions
    {
        public int id { get; set; }

        public string name { get; set; }

        public string displayName { get; set; }

        public string title { get; set; }

        public string iconPath { get; set; }

        public BitmapImage icon { get; set; }

        public string portraitPath { get; set; }

        public string splashPath { get; set; }

        public string danceVideoPath { get; set; }

        public string tags { get; set; }

        public string description { get; set; }

        public string quote { get; set; }

        public string quoteAuthor { get; set; }

        public double range { get; set; }

        public double moveSpeed { get; set; }

        public double armorBase { get; set; }

        public double armorLevel { get; set; }

        public double manaBase { get; set; }

        public double manaLevel { get; set; }

        public double criticalChanceBase { get; set; }

        public double criticalChanceLevel { get; set; }

        public double manaRegenBase { get; set; }

        public double manaRegenLevel { get; set; }

        public double healthRegenBase { get; set; }

        public double healthRegenLevel { get; set; }

        public double magicResistBase { get; set; }

        public double magicResistLevel { get; set; }

        public double healthBase { get; set; }

        public double healthLevel { get; set; }

        public double attackBase { get; set; }

        public double attackLevel { get; set; }

        public int ratingDefense { get; set; }

        public int ratingMagic { get; set; }

        public int ratingDifficulty { get; set; }

        public int ratingAttack { get; set; }

        public string tips { get; set; }

        public string opponentTips { get; set; }

        public string selectSoundPath { get; set; }

        public string Lore { get; set; }

        public string ResourceType { get; set; }

        public ArrayList Skins { get; set; }

        public List<Spell> Spells { get; set; }

        public bool IsFavourite { get; set; }

        public static champions GetChampion(int id)
        {
            return Client.Champions.FirstOrDefault(c => c.id == id);
        }

        public static champions GetChampion(string name)
        {
            return Client.Champions.FirstOrDefault(c => c.name == name);
        }
    }
}