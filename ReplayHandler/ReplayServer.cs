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
        private readonly int _endStartupChunkId;
        private readonly string _gameFolder;
        private readonly int _lastChunk;
        private readonly HttpListener _listener;
        private readonly int _startGameChunkId;
        private int _latestChunk;
        private int _latestKeyframe;
        private int _nextChunk;
        private DateTime _timeLastChunkServed;

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

                _latestChunk = 1;
                _nextChunk = 2;
                _latestKeyframe = 1;
                _gameFolder = Path.Combine(dir, "cabinet", gameId + "-" + region);
            }

            var di = new DirectoryInfo(_gameFolder);
            FileInfo[] files = di.GetFiles("chunk-*");

            foreach (
                int chunkId in
                    files.Select(f => Convert.ToInt32(f.Name.Replace("chunk-", "")))
                        .Where(chunkId => chunkId > _lastChunk))
                _lastChunk = chunkId;

            files = di.GetFiles("key-*");

            foreach (
                int keyId in
                    files.Select(f => Convert.ToInt32(f.Name.Replace("key-", "")))
                        .Where(keyId => keyId < _latestKeyframe))
                _latestKeyframe = keyId;

            String token = File.ReadAllText(Path.Combine(_gameFolder, "token"));
            var serializer = new JavaScriptSerializer();
            var deserializedJson = serializer.Deserialize<Dictionary<string, object>>(token);
            _endStartupChunkId = Convert.ToInt32(deserializedJson["endStartupChunkId"]);
            _startGameChunkId = Convert.ToInt32(deserializedJson["startGameChunkId"]);


            Console.WriteLine("Starting replay server... Close this window after your replay has finished");
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://127.0.0.1:5651/observer-mode/rest/consumer/");
            _listener.Start();

            Console.WriteLine("Listening...");
            _timeLastChunkServed = DateTime.Now;

            while (true)
            {
                HandleQuery();
            }
        }

        public void HandleQuery()
        {
            HttpListenerContext context = _listener.GetContext();
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            string requestedUrl = request.RawUrl.Replace("/observer-mode/rest/consumer/", "");
            Console.WriteLine("Requested: " + requestedUrl);

            bool shutdownAfterQuery = false;
            var buffer = new byte[0];

            if (requestedUrl == "version")
            {
                buffer = File.ReadAllBytes(Path.Combine(_gameFolder, "version"));
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
                    buffer = File.ReadAllBytes(Path.Combine(_gameFolder, "token"));
                }
                else if (Params[0] == "getLastChunkInfo")
                {
                    var totalSecondsToChunk =
                        (int) (-((DateTime.Now - _timeLastChunkServed.AddSeconds(30)).TotalMilliseconds));
                    if (totalSecondsToChunk < 0)
                        totalSecondsToChunk = 0;

                    //Quicker loading
                    if (_latestChunk < _lastChunk/2)
                        totalSecondsToChunk = 1;

                    string chunkInfo = "{\"chunkId\":" + _latestChunk +
                                       ",\"availableSince\":" +
                                       (int) (DateTime.Now - _timeLastChunkServed).TotalMilliseconds +
                                       ",\"nextAvailableChunk\":" + totalSecondsToChunk +
                                       ",\"keyFrameId\":" + _latestKeyframe +
                                       ",\"nextChunkId\":" + _nextChunk +
                                       ",\"endStartupChunkId\":" + _endStartupChunkId +
                                       ",\"startGameChunkId\":" + _startGameChunkId +
                                       ",\"endGameChunkId\":" + (_lastChunk - 1) +
                                       ",\"duration\":30000}";

                    buffer = Encoding.UTF8.GetBytes(chunkInfo);
                }
                else if (Params[0] == "getGameDataChunk")
                {
                    if (Convert.ToInt32(Params[3]) == (_lastChunk - 1))
                        Params[3] = _lastChunk.ToString(CultureInfo.InvariantCulture);

                    String file = Path.Combine(_gameFolder, "chunk-" + Params[3]);

                    buffer = File.Exists(file) ? File.ReadAllBytes(file) : new byte[0];

                    if (Convert.ToInt32(Params[3]) >= _latestChunk)
                    {
                        _latestChunk += 1;
                        _nextChunk += 1;

                        if (_latestChunk >= 7)
                        {
                            if (_latestChunk%2 == 0)
                                _latestKeyframe += 1;
                        }
                    }

                    _timeLastChunkServed = DateTime.Now;
                }
                else if (Params[0] == "getKeyFrame")
                {
                    String file = Path.Combine(_gameFolder, "key-" + Params[3]);

                    buffer = File.Exists(file) ? File.ReadAllBytes(file) : new byte[0];
                }
            }

            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);

            output.Close();

            if (!shutdownAfterQuery)
                return;

            _listener.Stop();
            Environment.Exit(0);
        }
    }
}