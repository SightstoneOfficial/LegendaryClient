namespace ReplayHandler
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
                return;

            string GameId = args[0];
            string Region = args[1];

            ReplayServer server = new ReplayServer(GameId, Region);
        }
    }
}