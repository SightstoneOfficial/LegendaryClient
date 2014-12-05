namespace ReplayHandler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 2)
                return;

            string gameId = args[0];
            string region = args[1];

            var server = new ReplayServer(gameId, region);
        }
    }
}