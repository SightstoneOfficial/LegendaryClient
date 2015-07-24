using Sightstone.Logic.MultiUser;
using Sightstone.Logic.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Script.Serialization;

namespace Sightstone.Logic.JSON
{
    internal class FreeToPlayChampions
    {
        private static FreeToPlayChampions instance;
        private readonly bool isLoaded;
        private List<FreeToPlayChampion> champions;

        private FreeToPlayChampions()
        {
            isLoaded = Load();
        }

        public static FreeToPlayChampions GetInstance()
        {
            if (instance == null)
                instance = new FreeToPlayChampions();

            return instance.champions == null ? null : instance;
        }

        private bool Load()
        {
            try
            {
                var client = new WebClient();
                var json =
                    client.DownloadString(
                        "http://cdn.leagueoflegends.com/patcher/data/regions/na/champData/freeToPlayChamps.json");
                champions = new JavaScriptSerializer().Deserialize<Response>(json).champions.ToList();

                return true;
            }
            catch (Exception e)
            {
                Client.Log("Error loading free to play champs.", "ERROR");
                Client.Log(e.ToString(), "ERROR");
            }

            return false;
        }

        public bool ReloadChamps(bool force)
        {
            if (champions == null || force)
                return Load();

            return champions != null;
        }

        public bool IsLoaded()
        {
            return isLoaded;
        }

        public bool IsFreeToPlay(int id)
        {
            return champions.FirstOrDefault(x => x.id == id) != null;
        }

        public bool IsFreeToPlay(champions champion)
        {
            return IsFreeToPlay(champion.id);
        }

        private class Response
        {
            public FreeToPlayChampion[] champions { get; set; }
        }

        private class FreeToPlayChampion
        {
            public int id { get; set; }
            public bool active { get; set; }
            public bool botEnabled { get; set; }
            public bool freeToPlay { get; set; }
            public bool botMmEnabled { get; set; }
            public bool rankedPlayEnabled { get; set; }
        }
    }
}