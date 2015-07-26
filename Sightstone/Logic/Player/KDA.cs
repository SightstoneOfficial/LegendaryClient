namespace Sightstone.Logic.Player
{
    public class KDA
    {
        public int Kills;
        public int Deaths;
        public int Assists;
        public int Games;

        public string KDAToString(KDA kda)
        {
            return (kda.Kills / kda.Games) + "/" + (kda.Deaths / kda.Games) + "/" + (kda.Assists / kda.Games);
        }
    }
}
