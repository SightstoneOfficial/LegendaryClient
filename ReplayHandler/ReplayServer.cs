using System;
using System.IO;
using System.Net;
using System.Text;

namespace ReplayHandler
{
    public class ReplayServer
    {
        HttpListener Listener;
        DateTime TimeLastChunkServed;
        int LatestChunk;
        int NextChunk;
        int LatestKeyframe;
        int LastChunk;
        string GameFolder;

        public ReplayServer(string GameId, string Region)
        {
            var dir = (System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)).ToString().Replace("file:\\", "");
            if (!Directory.Exists(Path.Combine(dir, "cabinet", GameId + "-" + Region)))
            {
                Console.WriteLine("Cannot find replay in: " + dir + " " + GameId + " " + Region);
                Console.ReadLine();
                return;
            }

            LatestChunk = 1;
            NextChunk = 2;
            LatestKeyframe = 1;
            GameFolder = Path.Combine(dir, "cabinet", GameId + "-" + Region);

            DirectoryInfo di = new DirectoryInfo(GameFolder);
            FileInfo[] files = di.GetFiles("chunk-*");

            foreach (FileInfo f in files)
            {
                int ChunkId = Convert.ToInt32(f.Name.Replace("chunk-", ""));
                if (ChunkId > LastChunk)
                    LastChunk = ChunkId;
            }

            files = di.GetFiles("key-*");

            foreach (FileInfo f in files)
            {
                int KeyId = Convert.ToInt32(f.Name.Replace("key-", ""));
                if (KeyId < LatestKeyframe)
                    LatestKeyframe = KeyId;
            }

            Console.WriteLine("Starting replay server... Close this window after your replay has finished");
            Listener = new HttpListener();
            Listener.Prefixes.Add("http://127.0.0.1:5651/observer-mode/rest/consumer/");
            Listener.Start();

            Console.WriteLine("Listening...");
            TimeLastChunkServed = DateTime.Now;

            while (true)
            {
                HandleQuery();
            }
        }

        public void HandleQuery()
        {
            HttpListenerContext context = Listener.GetContext();
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            string RequestedURL = request.RawUrl.Replace("/observer-mode/rest/consumer/", "");
            Console.WriteLine("Requested" + RequestedURL);

            bool ShutdownAfterQuery = false;
            byte[] buffer = new byte[0];

            if (RequestedURL == "version")
            {
                buffer = File.ReadAllBytes(Path.Combine(GameFolder, "version"));
            }
            else if (RequestedURL == "end")
            {
                buffer = Encoding.UTF8.GetBytes("done");
                ShutdownAfterQuery = true;
            }

            string[] Params = RequestedURL.Split('/');
            if (Params.Length > 0)
            {
                if (Params[0] == "getGameMetaData")
                {
                    buffer = File.ReadAllBytes(Path.Combine(GameFolder, "token"));
                }
                else if (Params[0] == "getLastChunkInfo")
                {
                    int TotalSecondsToChunk = (int)(-((DateTime.Now - TimeLastChunkServed.AddSeconds(30)).TotalMilliseconds));
                    if (TotalSecondsToChunk < 0)
                        TotalSecondsToChunk = 0;

                    //Quicker loading
                    if (LatestChunk < LastChunk / 2)
                        TotalSecondsToChunk = 1;

                    string ChunkInfo = "{\"chunkId\":" + LatestChunk +
                        ",\"availableSince\":" + (int)(DateTime.Now - TimeLastChunkServed).TotalMilliseconds +
                        ",\"nextAvailableChunk\":" + TotalSecondsToChunk +
                        ",\"keyFrameId\":" + LatestKeyframe +
                        ",\"nextChunkId\":" + NextChunk + ",\"endStartupChunkId\":5,\"startGameChunkId\":7,\"endGameChunkId\":" + (LastChunk - 1) +
                        ",\"duration\":30000}";

                    buffer = Encoding.UTF8.GetBytes(ChunkInfo);
                }
                else if (Params[0] == "getGameDataChunk")
                {
                    if (Convert.ToInt32(Params[3]) == (LastChunk - 1))
                        Params[3] = LastChunk.ToString();
                    buffer = File.ReadAllBytes(Path.Combine(GameFolder, "chunk-" + Params[3]));
                    if (Convert.ToInt32(Params[3]) >= LatestChunk)
                    {
                        LatestChunk += 1;
                        NextChunk += 1;

                        if (LatestChunk >= 7)
                        {
                            if (LatestChunk % 2 == 0)
                                LatestKeyframe += 1;
                        }
                    }

                    TimeLastChunkServed = DateTime.Now;
                }
                else if (Params[0] == "getKeyFrame")
                {
                    buffer = File.ReadAllBytes(Path.Combine(GameFolder, "key-" + Params[3]));
                }
            }

            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);

            output.Close();

            if (ShutdownAfterQuery)
            {
                Listener.Stop();
                Environment.Exit(0);
            }
        }
    }
}