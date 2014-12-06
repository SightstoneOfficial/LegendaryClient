namespace LegendaryClient.Logic.SQLite
{
    public class InvitationRequest
    {
        public int QueueId { get; set; }
        public bool IsRanked { get; set; }
        public string RankedTeamName { get; set; }
        public int MapId { get; set; }
        public int GameTypeConfigId { get; set; }
        public string GameMode { get; set; }
        public string GameType { get; set; }
    }
}