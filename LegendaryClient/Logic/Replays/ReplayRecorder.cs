#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Web.Script.Serialization;

#endregion

namespace LegendaryClient.Logic.Replays
{
    public class ReplayRecorder
    {
        public delegate void OnGotChunkHandler(int chunkId);

        public delegate void OnReplayRecordedHandler();

        public int GameId;


        public int LastChunkNumber = 0;
        public bool Recording = true;
        public string Region;

        public string Server;

        public ReplayRecorder(string server, int gameId, string region, string key)
        {
            try
            {
                GameId = gameId;
                Region = region;
                Server = "http://" + server;
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "cabinet", gameId + "-" + region));

                File.WriteAllText(Path.Combine(Client.ExecutingDirectory, "cabinet", gameId + "-" + region, "key"), key);

                int lastChunk;
                using (var client = new WebClient())
                {
                    client.DownloadFile(
                        String.Format("{0}/consumer/version", Server + "/observer-mode/rest"),
                        Path.Combine(Client.ExecutingDirectory, "cabinet", gameId + "-" + region, "version"));

                    string token = client.DownloadString(
                        String.Format("{0}/consumer/{1}/{2}/{3}/token",
                            Server + "/observer-mode/rest",
                            "getGameMetaData",
                            region,
                            gameId));

                    using (
                        var outfile =
                            new StreamWriter(Path.Combine(Client.ExecutingDirectory, "cabinet", gameId + "-" + region,
                                "token")))
                    {
                        outfile.Write(token);
                    }

                    var serializer = new JavaScriptSerializer();
                    var deserializedJson = serializer.Deserialize<Dictionary<string, object>>(token);

                    lastChunk = Convert.ToInt32(deserializedJson["endStartupChunkId"]);
                }
                Client.curentlyRecording.Add(gameId);

                ThreadPool.QueueUserWorkItem(delegate
                {
                    if (lastChunk != 0)
                    {
                        using (var client = new WebClient())
                        {
                            for (int i = 1; i < lastChunk + 1; i++)
                            {
                                client.DownloadFile(
                                    String.Format("{0}/consumer/{1}/{2}/{3}/{4}/token",
                                        Server + "/observer-mode/rest",
                                        "getGameDataChunk",
                                        region,
                                        gameId,
                                        i),
                                    Path.Combine(Client.ExecutingDirectory, "cabinet", gameId + "-" + region,
                                        "chunk-" + i));

                                if (OnGotChunk != null)
                                    OnGotChunk(i);

                                LastChunkNumber = i;
                            }
                        }
                    }
                    while (Recording)
                    {
                        GetChunk();
                    }
                });
            }
            catch (WebException e)
            {
                Client.curentlyRecording.RemoveAll(id => id == gameId);
                Client.Log(e.Message);
            }
        }

        public event OnReplayRecordedHandler OnReplayRecorded;

        public event OnGotChunkHandler OnGotChunk;


        private void GetChunk()
        {
            using (var client = new WebClient())
            {
                string token = client.DownloadString(
                    String.Format("{0}/consumer/{1}/{2}/{3}/0/token", Server + "/observer-mode/rest",
                        "getLastChunkInfo",
                        Region,
                        GameId));

                var serializer = new JavaScriptSerializer();
                var deserializedJson = serializer.Deserialize<Dictionary<string, object>>(token);

                var chunkId = (Int32) deserializedJson["chunkId"];
                if (chunkId == 0)
                {
                    //Try get chunk once avaliable
                    return;
                }

                if (LastChunkNumber == chunkId)
                {
                    //Sometimes chunk 1 isn't retrieved so get it again... it's like 7 kb so np
                    client.DownloadFile(
                        String.Format("{0}/consumer/{1}/{2}/{3}/{4}/token", Server + "/observer-mode/rest",
                            "getGameDataChunk",
                            Region,
                            GameId,
                            1),
                        Path.Combine(Client.ExecutingDirectory, "cabinet", GameId + "-" + Region, "chunk-" + 1));

                    client.DownloadFile(
                        String.Format("{0}/consumer/{1}/{2}/{3}/token", Server + "/observer-mode/rest",
                            "endOfGameStats",
                            Region,
                            GameId),
                        Path.Combine(Client.ExecutingDirectory, "cabinet", GameId + "-" + Region, "endOfGameStats"));

                    if (OnReplayRecorded != null)
                        OnReplayRecorded();
                    Client.curentlyRecording.RemoveAll(id => id == GameId);

                    Recording = false;
                    return;
                }

                //Get keyframe
                if (chunkId%2 == 0)
                {
                    int keyFrameId = Convert.ToInt32(deserializedJson["keyFrameId"]);
                    if (keyFrameId != 0)
                    {
                        client.DownloadFile(
                            String.Format("{0}/consumer/{1}/{2}/{3}/{4}/token", Server + "/observer-mode/rest",
                                "getKeyFrame",
                                Region,
                                GameId,
                                keyFrameId),
                            Path.Combine(Client.ExecutingDirectory, "cabinet", GameId + "-" + Region,
                                "key-" + keyFrameId));
                    }
                }

                client.DownloadFile(
                    String.Format("{0}/consumer/{1}/{2}/{3}/{4}/token", Server + "/observer-mode/rest",
                        "getGameDataChunk",
                        Region,
                        GameId,
                        chunkId),
                    Path.Combine(Client.ExecutingDirectory, "cabinet", GameId + "-" + Region, "chunk-" + chunkId));

                if (OnGotChunk != null)
                    OnGotChunk(chunkId);

                LastChunkNumber = chunkId;

                Thread.Sleep(Convert.ToInt32(deserializedJson["nextAvailableChunk"]));
            }
        }
    }
}