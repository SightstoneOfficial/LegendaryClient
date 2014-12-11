#region

using System.Linq;

#endregion

namespace LegendaryClient.Logic.SQLite
{
    public class items
    {
        public int id { get; set; }

        public string name { get; set; }

        public string description { get; set; }

        public string iconPath { get; set; }

        public int price { get; set; }

        public int sellprice { get; set; }

        public double flatHPPoolMod { get; set; }

        public double flatMPPoolMod { get; set; }

        public double percentHPPoolMod { get; set; }

        public double percentMPPoolMod { get; set; }

        public double flatHPRegenMod { get; set; }

        public double percentHPRegenMod { get; set; }

        public double flatMPRegenMod { get; set; }

        public double percentMPRegenMod { get; set; }

        public double flatArmorMod { get; set; }

        public double percentArmorMod { get; set; }

        public double flatAttackDamageMod { get; set; }

        public double percentAttackDamageMod { get; set; }

        public double flatAbilityPowerMod { get; set; }

        public double percentAbilityPowerMod { get; set; }

        public double flatMovementSpeedMod { get; set; }

        public double percentMovementSpeedMod { get; set; }

        public double flatAttackSpeedMod { get; set; }

        public double percentAttackSpeedMod { get; set; }

        public double flatDodgeMod { get; set; }

        public double percentDodgeMod { get; set; }

        public double flatCritChanceMod { get; set; }

        public double percentCritChanceMod { get; set; }

        public double flatCritDamageMod { get; set; }

        public double percentCritDamageMod { get; set; }

        public double flatMagicResistMod { get; set; }

        public double percentMagicResistMod { get; set; }

        public double flatEXPBonus { get; set; }

        public double percentEXPBonus { get; set; }

        public double epicness { get; set; }

        public static items GetItem(int id)
        {
            return Client.Items.FirstOrDefault(c => c.id == id);
        }
    }
}