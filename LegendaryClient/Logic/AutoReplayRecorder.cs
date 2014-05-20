using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Web.Script.Serialization;
using LegendaryClient.Logic;
using LegendaryClient.Logic.Region;

namespace LegendaryClient.Logic.AutoReplayRecorder
{
    public class AutoReplayRecorder
    {

        BaseRegion region;
        
        public int LastChunkNumber = 0;
       
        
        public string Server;
        public bool Recording = true;

        public delegate void OnReplayRecordedHandler();
        public event OnReplayRecordedHandler OnReplayRecorded;

        public delegate void OnGotChunkHandler(int ChunkId);
        public event OnGotChunkHandler OnGotChunk;

        
       // public ReplayRecorder(string Server, int GameId, string Region, string Key)
        
        public AutoReplayRecorder()
        {
     
            //Dictionary<string, object> SpectatorGame = objectGame as Dictionary<string, object>;
            
            //GameId = Client.GameID;
            //this.GameId = GameId;
            //this.Region = Region;
            //this.Server = "http://" + Server;
            
            Directory.CreateDirectory(Path.Combine("cabinet", Client.GameID + "-" + region.SpectatorIpAddress));

            //File.WriteAllText(Path.Combine("cabinet", Client.GameID + "-" + region.SpectatorIpAddress, "key"), Key);

            int ChunkTimeInterval;
            int LastChunk = 0;
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(
                    String.Format("{0}/consumer/version", this.Server + "/observer-mode/rest"),
                Path.Combine("cabinet", Client.GameID + "-" + region.SpectatorIpAddress, "version"));

                string token = client.DownloadString(
                    String.Format("{0}/consumer/{1}/{2}/{3}/token", this.Server + "/observer-mode/rest",
                    "getGameMetaData",
                    region.SpectatorIpAddress,
                    Client.GameID));

                using (StreamWriter outfile = new StreamWriter(Path.Combine("cabinet", Client.GameID + "-" + region.SpectatorIpAddress, "token")))
                {
                    outfile.Write(token);
                }

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> deserializedJSON = serializer.Deserialize<Dictionary<string, object>>(token);

                ChunkTimeInterval = Convert.ToInt32(deserializedJSON["chunkTimeInterval"]);
                LastChunk = Convert.ToInt32(deserializedJSON["endStartupChunkId"]);
            }

            ThreadPool.QueueUserWorkItem(delegate
            {
                if (LastChunk != 0)
                {
                    using (WebClient client = new WebClient())
                    {
                        for (int i = 1; i < LastChunk + 1; i++)
                        {
                            client.DownloadFile(
                                String.Format("{0}/consumer/{1}/{2}/{3}/{4}/token", this.Server + "/observer-mode/rest",
                                "getGameDataChunk",
                                region.SpectatorIpAddress,
                                Client.GameID,
                                i),
                                Path.Combine("cabinet", Client.GameID + "-" + region.SpectatorIpAddress, "chunk-" + i));

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

        void GetChunk()
        {
            using (WebClient client = new WebClient())
            {
                string token = client.DownloadString(
                    String.Format("{0}/consumer/{1}/{2}/{3}/0/token", Server + "/observer-mode/rest",
                    "getLastChunkInfo",
                    region.SpectatorIpAddress,
                    Client.GameID));

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> deserializedJSON = serializer.Deserialize<Dictionary<string, object>>(token);

                int ChunkId = Convert.ToInt32(deserializedJSON["chunkId"]);
                if (ChunkId == 0)
                {
                    //Try get chunk once avaliable
                    return;
                }

                if (LastChunkNumber == ChunkId)
                {
                    //Sometimes chunk 1 isn't retrieved so get it again... it's like 7 kb so np
                    client.DownloadFile(
                            String.Format("{0}/consumer/{1}/{2}/{3}/{4}/token", Server + "/observer-mode/rest",
                            "getGameDataChunk",
                            region.SpectatorIpAddress,
                            Client.GameID,
                            1),
                        Path.Combine("cabinet", Client.GameID + "-" + region.SpectatorIpAddress, "chunk-" + 1));

                    client.DownloadFile(
                        String.Format("{0}/consumer/{1}/{2}/{3}/token", Server + "/observer-mode/rest",
                        "endOfGameStats",
                        region.SpectatorIpAddress,
                        Client.GameID),
                    Path.Combine("cabinet", Client.GameID + "-" + region.SpectatorIpAddress, "endOfGameStats"));

                    if (OnReplayRecorded != null)
                        OnReplayRecorded();

                    Recording = false;
                    return;
                }

                //Get keyframe
                if (ChunkId % 2 == 0)
                {
                    int KeyFrameId = Convert.ToInt32(deserializedJSON["keyFrameId"]);
                    if (KeyFrameId != 0)
                    {
                        client.DownloadFile(
                            String.Format("{0}/consumer/{1}/{2}/{3}/{4}/token", Server + "/observer-mode/rest",
                            "getKeyFrame",
                            region.SpectatorIpAddress,
                            Client.GameID,
                            KeyFrameId),
                        Path.Combine("cabinet", Client.GameID + "-" + region.SpectatorIpAddress, "key-" + KeyFrameId));
                    }
                }

                client.DownloadFile(
                        String.Format("{0}/consumer/{1}/{2}/{3}/{4}/token", Server + "/observer-mode/rest",
                        "getGameDataChunk",
                        region.SpectatorIpAddress,
                        Client.GameID,
                        ChunkId),
                    Path.Combine("cabinet", Client.GameID + "-" + region.SpectatorIpAddress, "chunk-" + ChunkId));

                if (OnGotChunk != null)
                    OnGotChunk(ChunkId);

                LastChunkNumber = ChunkId;

                Thread.Sleep(Convert.ToInt32(deserializedJSON["nextAvailableChunk"]));
            }
        }
    }
}