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

        public static bool CanUseSpell(int SpellId, double Level, string GameMode)
        {
            if (Client.LoginPacket.ClientSystemStates.InactiveSpellIdList.Contains(SpellId))
                return false;
            switch (GameMode)
            {
                case "CLASSIC":
                    if (Client.LoginPacket.ClientSystemStates.InactiveClassicSpellIdList.Contains(SpellId))
                        return false;
                    break;
                case "ARAM":
                    if (Client.LoginPacket.ClientSystemStates.InactiveAramSpellIdList.Contains(SpellId))
                        return false;
                    break;
                case "ODIN":
                    if (Client.LoginPacket.ClientSystemStates.InactiveOdinSpellIdList.Contains(SpellId))
                        return false;
                    break;
                default:
                    break;
            }
            switch (SpellId)
            {
                case 1:
                    return Level >= 2;
                case 2:
                    return Level >= 10;
                case 3:
                    return Level >= 8;
                case 4:
                    return Level >= 12;
                case 11:
                    return Level >= 3;
                case 12:
                    return Level >= 2;
                case 14:
                    return Level >= 8;
                case 21:
                    return Level >= 6;
                default:
                    return true;
            }
        }
    }

    public enum NameToImage
    {
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
