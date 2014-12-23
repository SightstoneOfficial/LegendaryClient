#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Script.Serialization;
using LegendaryClient.Logic.SQLite;

#endregion

namespace LegendaryClient.Logic.JSON
{
    internal class FreeToPlayChampions
    {
        private static FreeToPlayChampions _instance;
        private readonly bool _isLoaded;
        private List<FreeToPlayChampion> _champions;

        private FreeToPlayChampions()
        {
            _isLoaded = Load();
        }

        public static FreeToPlayChampions GetInstance()
        {
            if (_instance == null)
                _instance = new FreeToPlayChampions();

            return _instance._champions == null ? null : _instance;
        }

        private bool Load()
        {
            try
            {
                var client = new WebClient();
                var json =
                    client.DownloadString(
                        "http://cdn.leagueoflegends.com/patcher/data/regions/na/champData/freeToPlayChamps.json");
                _champions = new JavaScriptSerializer().Deserialize<Response>(json).champions.ToList();

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
            if (_champions == null || force)
                return Load();

            return _champions != null;
        }

        public bool IsLoaded()
        {
            return _isLoaded;
        }

        public bool IsFreeToPlay(int id)
        {
            return _champions.FirstOrDefault(x => x.id == id) != null;
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