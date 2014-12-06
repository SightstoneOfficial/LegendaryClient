using System.Linq;

namespace LegendaryClient.Logic.SQLite
{
    public class Items
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string IconPath { get; set; }

        public int Price { get; set; }

        public int SellPrice { get; set; }

        public double FlatHpPoolMod { get; set; }

        public double FlatMpPoolMod { get; set; }

        public double PercentHpPoolMod { get; set; }

        public double PercentMpPoolMod { get; set; }

        public double FlatHpRegenMod { get; set; }

        public double PercentHpRegenMod { get; set; }

        public double FlatMpRegenMod { get; set; }

        public double PercentMpRegenMod { get; set; }

        public double FlatArmorMod { get; set; }

        public double PercentArmorMod { get; set; }

        public double FlatAttackDamageMod { get; set; }

        public double PercentAttackDamageMod { get; set; }

        public double FlatAbilityPowerMod { get; set; }

        public double PercentAbilityPowerMod { get; set; }

        public double FlatMovementSpeedMod { get; set; }

        public double PercentMovementSpeedMod { get; set; }

        public double FlatAttackSpeedMod { get; set; }

        public double PercentAttackSpeedMod { get; set; }

        public double FlatDodgeMod { get; set; }

        public double PercentDodgeMod { get; set; }

        public double FlatCritChanceMod { get; set; }

        public double PercentCritChanceMod { get; set; }

        public double FlatCritDamageMod { get; set; }

        public double PercentCritDamageMod { get; set; }

        public double FlatMagicResistMod { get; set; }

        public double PercentMagicResistMod { get; set; }

        public double FlatExpBonus { get; set; }

        public double PercentExpBonus { get; set; }

        public double Epicness { get; set; }

        public static Items GetItem(int id)
        {
            return Client.Items.FirstOrDefault(c => c.Id == id);
        }
    }
}