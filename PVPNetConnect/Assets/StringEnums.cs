using System;
using System.Reflection;

namespace PVPNetConnect
{
    /// <summary>
    /// Game Modes enumerator.
    /// </summary>
    public enum GameMode
    {
        [StringValue("CLASSIC")]
        Classic,

        [StringValue("ODIN")]
        Dominion,

        [StringValue("ARAM")]
        Aram,

        [StringValue("TUTORIAL")]
        Tutorial,
    }

    /// <summary>
    /// Seasons enumerator.
    /// </summary>
    public enum CompetitiveSeason
    {
        [StringValue("CURRENT")]
        Current,

        [StringValue("ONE")]
        One,

        [StringValue("TWO")]
        Two
    }

    /// <summary>
    /// Game types enumerator.
    /// </summary>
    public enum GameType
    {
        [StringValue("RANKED_TEAM_GAME")]
        RankedTeamGame,

        [StringValue("RANKED_GAME")]
        RankedGame,

        [StringValue("NORMAL_GAME")]
        NormalGame,

        [StringValue("GROUPFINDER")]
        TeamBuilder,

        [StringValue("CUSTOM_GAME")]
        CustomGame,

        [StringValue("TUTORIAL_GAME")]
        TutorialGame,

        [StringValue("PRACTICE_GAME")]
        PracticeGame,

        [StringValue("RANKED_GAME_SOLO")]
        RankedGameSolo,

        [StringValue("COOP_VS_AI")]
        CoopVsAi,

        [StringValue("RANKED_GAME_PREMADE")]
        RankedGamePremade
    }

    /// <summary>
    /// Queue types Enumeartor.
    /// </summary>
    public enum QueueType
    {
        [StringValue("GROUPFINDER")]
        TeamBuilder,

        [StringValue("RANKED_TEAM3x3")]
        RankedTeam3x3,

        [StringValue("RANKED_SOLO_3x3")]
        RankedSolo3x3,

        [StringValue("RANKED_SOLO_5x5")]
        RankedSolo5x5,

        [StringValue("RANKED_TEAM_5x5")]
        RankedTeam5x5,

        [StringValue("ODIN_UNRANKED")]
        DominionUnranked,

        [StringValue("RANKED_PREMADE_3x3")]
        RankedPremade3x3,

        [StringValue("NORMAL_3x3")]
        Normal3x3,

        [StringValue("RANKED_PREMADE_5x5")]
        RankedPremade5x5,

        [StringValue("ODIN_RANKED_PREMADE")]
        DominionRankedPremade,

        [StringValue("BOT_3x3")]
        Bot3x3,

        [StringValue("ODIN_RANKED_SOLO")]
        DominionRankedSolo,

        [StringValue("NORMAL")]
        Normal,

        [StringValue("BOT")]
        Bot,

        [StringValue("ARAM_UNRANKED_1x1")]
        AramUnranked1x1,

        [StringValue("ARAM_UNRANKED_3x3")]
        AramUnranked3x3,

        [StringValue("NONE")]
        None,

        [StringValue("ARAM_UNRANKED_5x5")]
        AramUnranked5x5,

        [StringValue("ARAM_UNRANKED_2x2")]
        AramUnranked2x2,

        [StringValue("ARAM_UNRANKED_6x6")]
        AramUnranked6x6,

        [StringValue("RANKED_SOLO_1x1")]
        RankedSolo1x1
    }

    public enum AllowSpectators
    {
        [StringValue("ALL")]
        All = 1,

        [StringValue("LOBBYONLY")]
        LobbyOnly = 2,

        [StringValue("DROPINONLY")]
        DropInOnly = 3,

        [StringValue("NONE")]
        None = 0
    }

    /// <summary>
    /// The StringEnum value with GetStringValue method
    /// </summary>
    public static class StringEnum
    {
        /// <summary>
        /// Gets the string value from Atrribute.
        /// </summary>
        /// <param name="value">Enum value.</param>
        /// <returns></returns>
        public static string GetStringValue(Enum value)
        {
            string output = null;
            Type type = value.GetType();

            //Check first in our cached results...

            //Look for our 'StringValueAttribute'

            //in the field's custom attributes

            FieldInfo fi = type.GetField(value.ToString());
            StringValue[] attrs =
                fi.GetCustomAttributes(typeof(StringValue),
                    false) as StringValue[];
            if (attrs.Length > 0)
            {
                output = attrs[0].Value;
            }

            return output;
        }
    }

    public class StringValue : System.Attribute
    {
        private string _value;

        public StringValue(string value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }
    }
}