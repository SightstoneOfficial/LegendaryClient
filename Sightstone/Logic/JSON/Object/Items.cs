using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sightstone.Logic.JSON.Object
{
    public class Items
    {
        public class Rootobject
        {
            public Basicitem basicitem { get; set; }
            public Item[] items { get; set; }
            public Itemgroup[] itemgroups { get; set; }
            public Category[] categories { get; set; }
        }

        public class Basicitem
        {
            public bool canBeSold { get; set; }
            public bool consumed { get; set; }
            public int depth { get; set; }
            public object[] from { get; set; }
            public bool hideFromAll { get; set; }
            public bool inStore { get; set; }
            public object[] into { get; set; }
            public object[] sidegrade { get; set; }
            public string itemgroup { get; set; }
            public object[] notes { get; set; }
            public string requiredChampion { get; set; }
            public string requiredSpellName { get; set; }
            public object[] requiredPurchaseIdentities { get; set; }
            public string requiredBuffCurrencyName { get; set; }
            public string disabledDescriptionOverride { get; set; }
            public int requiredLevel { get; set; }
            public string specialRecipe { get; set; }
            public int stacks { get; set; }
            public bool usableInStore { get; set; }
            public bool consumeOnAcquire { get; set; }
            public object[] effectByLevelValues { get; set; }
            public Stats stats { get; set; }
        }

        public class Stats
        {
            public int FlatArmorMod { get; set; }
            public int FlatAttackSpeedMod { get; set; }
            public int FlatCritChanceMod { get; set; }
            public int FlatCritDamageMod { get; set; }
            public int FlatHPPoolMod { get; set; }
            public int FlatHPRegenMod { get; set; }
            public int FlatMPPoolMod { get; set; }
            public int FlatMPRegenMod { get; set; }
            public int FlatMagicDamageMod { get; set; }
            public int FlatMovementSpeedMod { get; set; }
            public int FlatPhysicalDamageMod { get; set; }
            public int FlatSpellBlockMod { get; set; }
            public int PercentArmorMod { get; set; }
            public int PercentAttackSpeedMod { get; set; }
            public int PercentCritDamageMod { get; set; }
            public int PercentEXPBonus { get; set; }
            public int PercentHPPoolMod { get; set; }
            public int PercentHPRegenMod { get; set; }
            public int PercentBaseHPRegenMod { get; set; }
            public int PercentMPPoolMod { get; set; }
            public int PercentMPRegenMod { get; set; }
            public int PercentBaseMPRegenMod { get; set; }
            public int PercentMagicDamageMod { get; set; }
            public int PercentMovementSpeedMod { get; set; }
            public int PercentPhysicalDamageMod { get; set; }
            public int PercentSpellBlockMod { get; set; }
        }

        public class Item
        {
            public string id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string icon { get; set; }
            public Stats1 stats { get; set; }
            public bool consumed { get; set; }
            public bool inStore { get; set; }
            public object[] requiredPurchaseIdentities { get; set; }
            public Gold gold { get; set; }
            public int requiredBuffCurrencyCost { get; set; }
            public string[] tags { get; set; }
            public string itemgroup { get; set; }
            public bool canBeSold { get; set; }
            public string requiredChampion { get; set; }
            public string specialRecipe { get; set; }
            public string[] into { get; set; }
            public string[] sidegrade { get; set; }
            public bool usableInStore { get; set; }
            public bool consumeOnAcquire { get; set; }
            public string requiredBuffCurrencyName { get; set; }
            public int stacks { get; set; }
            public int requiredLevel { get; set; }
            public string[] from { get; set; }
            public int depth { get; set; }
            public string requiredSpellName { get; set; }
            public string disabledDescriptionOverride { get; set; }
            public bool hideFromAll { get; set; }
            public int[] effectByLevelValues { get; set; }
        }

        public class Stats1
        {
            public float PercentBaseMPRegenMod { get; set; }
            public float PercentBaseHPRegenMod { get; set; }
            public int FlatArmorMod { get; set; }
            public int FlatMovementSpeedMod { get; set; }
            public int FlatPhysicalDamageMod { get; set; }
            public int FlatHPPoolMod { get; set; }
            public int FlatMagicDamageMod { get; set; }
            public int FlatMPPoolMod { get; set; }
            public float FlatCritChanceMod { get; set; }
            public float FlatMPRegenMod { get; set; }
            public float FlatHPRegenMod { get; set; }
            public int FlatSpellBlockMod { get; set; }
            public float PercentAttackSpeedMod { get; set; }
            public float PercentMovementSpeedMod { get; set; }
        }

        public class Gold
        {
            public int _base { get; set; }
            public int total { get; set; }
            public int sell { get; set; }
            public int sidegradeCredit { get; set; }
        }

        public class Itemgroup
        {
            public string groupid { get; set; }
            public int maxGroupOwnable { get; set; }
            public int groupPurchaseCooldown { get; set; }
            public int inventorySlotMin { get; set; }
            public int inventorySlotMax { get; set; }
        }

        public class Category
        {
            public string header { get; set; }
            public int sortIndex { get; set; }
            public bool restricted { get; set; }
            public string[] tags { get; set; }
        }
    }
}
