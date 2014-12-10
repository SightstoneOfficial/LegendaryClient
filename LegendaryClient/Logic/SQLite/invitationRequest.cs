namespace LegendaryClient.Logic.SQLite
{
    public class invitationRequest
    {
        public int queueId { get; set; }
        public bool isRanked { get; set; }
        public string rankedTeamName { get; set; }
        public int mapId { get; set; }
        public int gameTypeConfigId { get; set; }
        public string gameMode { get; set; }
        public string gameType { get; set; }
    }
}