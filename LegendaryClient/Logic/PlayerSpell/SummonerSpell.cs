using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryClient.Logic.PlayerSpell
{
    public static class SummonerSpell
    {
        public static string GetSpellName(int spellID)
        {
            SummonerSpells spell = (SummonerSpells)spellID;
            return Enum.GetName(typeof(SummonerSpells), spell);
        }

        public static string GetSpellImageName(int spellID)
        {
            NameToImage spell = (NameToImage)spellID;
            return "Summoner" + Enum.GetName(typeof(NameToImage), spell) + ".png";
        }

        public static SummonerSpells GetSpell(string spellName)
        {
            return (SummonerSpells)Enum.Parse(typeof(SummonerSpells), spellName, true);
        }
    }

    public enum NameToImage
    {
        NONE = 0,
        Boost = 1,
        Clairvoyance = 2,
        Exhaust = 3,
        Flash = 4,
        Haste = 6,
        Heal = 7,
        Revive = 10,
        Smite = 11,
        Teleport = 12,
        Mana = 13,
        Dot = 14,
        Barrier = 21
    }

    public enum SummonerSpells
    {
        NONE = 0,
        Cleanse = 1,
        Clairvoyance = 2,
        Exhaust = 3,
        Flash = 4,
        Ghost = 6,
        Heal = 7,
        Revive = 10,
        Smite = 11,
        Teleport = 12,
        Clarity = 13,
        Ignite = 14,
        Barrier = 21
    }
}
