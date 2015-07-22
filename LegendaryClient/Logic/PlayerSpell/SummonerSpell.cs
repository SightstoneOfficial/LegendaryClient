using LegendaryClient.Logic.MultiUser;
using System;
using System.Linq;

namespace LegendaryClient.Logic.PlayerSpell
{
    public static class SummonerSpell
    {
        static UserClient UserClient = (UserList.Users[Client.CurrentServer])[Client.CurrentUser];
        public static string GetSpellName(int spellId)
        {
            var spell = (SummonerSpells) spellId;
            return Enum.GetName(typeof (SummonerSpells), spell);
        }

        public static string GetSpellImageName(int spellId)
        {
            var spell = (NameToImage) spellId;
            return "Summoner" + Enum.GetName(typeof (NameToImage), spell) + ".png";
        }

        public static SummonerSpells GetSpell(string spellName)
        {
            return (SummonerSpells) Enum.Parse(typeof (SummonerSpells), spellName, true);
        }

        public static bool CanUseSpell(int spellId, double level, string gameMode)
        {
            if (UserClient.LoginPacket.ClientSystemStates.inactiveSpellIdList.Contains(spellId))
                return false;

            switch (gameMode)
            {
                case "CLASSIC":
                    if (UserClient.LoginPacket.ClientSystemStates.inactiveClassicSpellIdList.Contains(spellId))
                        return false;
                    break;

                case "ARAM":
                    if (UserClient.LoginPacket.ClientSystemStates.inactiveAramSpellIdList.Contains(spellId))
                        return false;
                    break;

                case "ODIN":
                    if (UserClient.LoginPacket.ClientSystemStates.inactiveOdinSpellIdList.Contains(spellId))
                        return false;
                    break;
            }

            switch (spellId)
            {
                case 1:
                    return level >= 6;

                case 2:
                    return level >= 8;

                case 3:
                    return level >= 4;

                case 4:
                    return level >= 8;

                case 11:
                    return level >= 10;

                case 12:
                    return level >= 6;

                case 14:
                    return level >= 10;

                case 21:    
                    return level >= 4;

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
        OdinGarrison = 17,
        Barrier = 21,
        Snowball = 32
    }

    public enum SummonerSpells
    {
        None = 0,
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
        Garrison = 17,
        Barrier = 21,
        Mark = 32
    }
}