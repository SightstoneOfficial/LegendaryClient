using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace LegendaryClient.Logic.JSON
{
    class FreeToPlayChampions
    {
        private static FreeToPlayChampions _instance;
        private List<FreeToPlayChampion> champions;
        private bool isLoaded;

        private FreeToPlayChampions()
        {
            isLoaded = load();
        }

        public static FreeToPlayChampions GetInstance()
        {
            if (_instance == null)
                _instance = new FreeToPlayChampions();

            if (_instance.champions == null)
                return null;
            return _instance;
        }

        private bool load()
        {
            try
            {
                WebClient client = new WebClient();
                string json = client.DownloadString("http://cdn.leagueoflegends.com/patcher/data/regions/na/champData/freeToPlayChamps.json");
                champions = new JavaScriptSerializer().Deserialize<List<FreeToPlayChampion>>(json);
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
                return load();
            if (champions != null)
                return true;

            return false;
        }

        public bool IsLoaded()
        {
            return isLoaded;
        }

        public bool IsFreeToPlay(int id)
        {
            return champions.FirstOrDefault(x => x.id == id) != null;
        }

        public bool IsFreeToPlay(SQLite.champions champion)
        {
            return IsFreeToPlay(champion.id);
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
