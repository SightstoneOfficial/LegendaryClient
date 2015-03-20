#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;

#endregion

namespace ReplayHandler
{
    public class ReplayServer
    {
        private readonly int endStartupChunkId;
        private readonly string gameFolder;
        private readonly int lastChunk;
        private readonly HttpListener listener;
        private readonly int startGameChunkId;
        private int latestChunk;
        private int latestKeyframe;
        private int nextChunk;
        private DateTime timeLastChunkServed;

        public ReplayServer(string gameId, string region)
        {
            string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (directoryName != null)
            {
                string dir = directoryName.Replace("file:\\", "");
                if (!Directory.Exists(Path.Combine(dir, "cabinet", gameId + "-" + region)))
                {
                    Console.WriteLine("Cannot find replay in: " + dir + " " + gameId + " " + region);
                    Console.ReadLine();
                    return;
                }

                latestChunk = 1;
                nextChunk = 2;
                latestKeyframe = 1;
                gameFolder = Path.Combine(dir, "cabinet", gameId + "-" + region);
            }

            var di = new DirectoryInfo(gameFolder);
            FileInfo[] files = di.GetFiles("chunk-*");

            foreach (
                int chunkId in
                    files.Select(f => Convert.ToInt32(f.Name.Replace("chunk-", "")))
                        .Where(chunkId => chunkId > lastChunk))
                lastChunk = chunkId;

            files = di.GetFiles("key-*");

            foreach (
                int keyId in
                    files.Select(f => Convert.ToInt32(f.Name.Replace("key-", "")))
                        .Where(keyId => keyId < latestKeyframe))
                latestKeyframe = keyId;

            string token = File.ReadAllText(Path.Combine(gameFolder, "token"));
            var serializer = new JavaScriptSerializer();
            var deserializedJson = serializer.Deserialize<Dictionary<string, object>>(token);
            endStartupChunkId = Convert.ToInt32(deserializedJson["endStartupChunkId"]);
            startGameChunkId = Convert.ToInt32(deserializedJson["startGameChunkId"]);


            Console.WriteLine("Starting replay server... Close this window after your replay has finished");
            listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:5651/observer-mode/rest/consumer/");
            listener.Start();

            Console.WriteLine("Listening...");
            timeLastChunkServed = DateTime.Now;

            while (true)
            {
                HandleQuery();
            }
        }

        public void HandleQuery()
        {
            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            string requestedUrl = request.RawUrl.Replace("/observer-mode/rest/consumer/", "");
            Console.WriteLine("Requested: " + requestedUrl);

            bool shutdownAfterQuery = false;
            var buffer = new byte[0];

            if (requestedUrl == "version")
            {
                buffer = File.ReadAllBytes(Path.Combine(gameFolder, "version"));
            }
            else if (requestedUrl == "end")
            {
                buffer = Encoding.UTF8.GetBytes("done");
                shutdownAfterQuery = true;
            }

            string[] Params = requestedUrl.Split('/');
            if (Params.Length > 0)
            {
                if (Params[0] == "getGameMetaData")
                {
                    buffer = File.ReadAllBytes(Path.Combine(gameFolder, "token"));
                }
                else if (Params[0] == "getLastChunkInfo")
                {
                    var totalSecondsToChunk =
                        (int) (-((DateTime.Now - timeLastChunkServed.AddSeconds(30)).TotalMilliseconds));
                    if (totalSecondsToChunk < 0)
                        totalSecondsToChunk = 0;

                    //Quicker loading
                    if (latestChunk < lastChunk/2)
                        totalSecondsToChunk = 1;

                    string chunkInfo = "{\"chunkId\":" + latestChunk +
                                       ",\"availableSince\":" +
                                       (int) (DateTime.Now - timeLastChunkServed).TotalMilliseconds +
                                       ",\"nextAvailableChunk\":" + totalSecondsToChunk +
                                       ",\"keyFrameId\":" + latestKeyframe +
                                       ",\"nextChunkId\":" + nextChunk +
                                       ",\"endStartupChunkId\":" + endStartupChunkId +
                                       ",\"startGameChunkId\":" + startGameChunkId +
                                       ",\"endGameChunkId\":" + (lastChunk - 1) +
                                       ",\"duration\":30000}";

                    buffer = Encoding.UTF8.GetBytes(chunkInfo);
                }
                else if (Params[0] == "getGameDataChunk")
                {
                    if (Convert.ToInt32(Params[3]) == (lastChunk - 1))
                        Params[3] = lastChunk.ToString(CultureInfo.InvariantCulture);

                    string file = Path.Combine(gameFolder, "chunk-" + Params[3]);

                    buffer = File.Exists(file) ? File.ReadAllBytes(file) : new byte[0];

                    if (Convert.ToInt32(Params[3]) >= latestChunk)
                    {
                        latestChunk += 1;
                        nextChunk += 1;

                        if (latestChunk >= 7)
                        {
                            if (latestChunk%2 == 0)
                                latestKeyframe += 1;
                        }
                    }

                    timeLastChunkServed = DateTime.Now;
                }
                else if (Params[0] == "getKeyFrame")
                {
                    string file = Path.Combine(gameFolder, "key-" + Params[3]);

                    buffer = File.Exists(file) ? File.ReadAllBytes(file) : new byte[0];
                }
            }

            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);

            output.Close();

            if (!shutdownAfterQuery)
                return;

            listener.Stop();
            Environment.Exit(0);
        }
    }
}