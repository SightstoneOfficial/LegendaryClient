namespace Sightstone.Logic.Player
{
    public class GetKDA
    {
        public ChampStats stats { get; set; }

        public GetKDA(string playerName, string ChampName)
        {
            stats = new ChampStats(ChampName, playerName);
        }

        public GetKDA(int playerID, string ChampName)
        {
            stats = new ChampStats(ChampName, playerID);
        }

        public GetKDA(string playerName, int ChampID)
        {
            stats = new ChampStats(ChampID, playerName);
        }

        public GetKDA(int playerID, int ChampID)
        {
            stats = new ChampStats(ChampID, playerID);
        }
    }
}
