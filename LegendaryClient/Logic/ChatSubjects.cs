namespace LegendaryClient.Logic
{
    public enum ChatSubjects
    {
        /// <summary>
        /// Hack, doesn't use the enum value
        /// </summary>
        GAME_INVITE_OWNER_CANCELED,

        GAME_INVITE_ALLOW_DISABLED,
        GAME_FULL_INVITE_REJECTED,
        GAME_INVITE_ALLOW_ENABLED,
        GAME_INVITE_ACCEPTED_ACK,
        GAME_INVITE_SUGGESTED,
        GAME_INVITE_REJECTED,
        GAME_INVITE_CANCELED,
        GAME_MSG_OUT_OF_SYNC,
        GAME_INVITE_ACCEPTED,
        GAME_INVITE,
        VERIFY_INVITEE_RESET,
        VERIFY_INVITEE_ACK,
        VERIFY_INVITEE,
        EVENT_INVITE_RECIEVED,
        PRACTICE_GAME_INVITE_ACCEPT_ACK,
        PRACTICE_GAME_INVITE_ACCEPT,
        PRACTICE_GAME_OWNER_CHANGED,
        PRACTICE_GAME_JOIN_ACK,
        PRACTICE_GAME_INVITE,
        PRACTICE_GAME_JOIN,
        RANKED_TEAM_UPDATED,
        CHAMPION_TRADE_REQUESTED,
        INVITE_STATUS_CHANGED,
        personalMessage
    }
}