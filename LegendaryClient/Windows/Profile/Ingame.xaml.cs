using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.PlayerSpell;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect.RiotObjects.Platform.Game;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    /// Interaction logic for Ingame.xaml
    /// </summary>
    public partial class Ingame : Page
    {
        public Ingame()
        {
            InitializeComponent();
        }

        public void Update(PlatformGameLifecycleDTO CurrentGame)
        {
            foreach (PlayerChampionSelectionDTO championSelect in CurrentGame.Game.PlayerChampionSelections)
            {
                ChampSelectPlayer control = new ChampSelectPlayer();
                var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champions.GetChampion(championSelect.ChampionId).iconPath), UriKind.Absolute);
                control.ChampionImage.Source = new BitmapImage(uriSource);
                uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName(Convert.ToInt32(championSelect.Spell1Id))), UriKind.Absolute);
                control.SummonerSpell1.Source = new BitmapImage(uriSource);
                uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName(Convert.ToInt32(championSelect.Spell2Id))), UriKind.Absolute);
                control.SummonerSpell2.Source = new BitmapImage(uriSource);
            }
            try
            {
                string mmrJSON = "";
                string url = Client.Region.SpectatorLink + "consumer/getGameMetaData/" + Client.Region.InternalName + "/" + CurrentGame.Game.Id + "/token";
                using (WebClient client = new WebClient())
                {
                    mmrJSON = client.DownloadString(url);
                }
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> deserializedJSON = serializer.Deserialize<Dictionary<string, object>>(mmrJSON);
                MMRLabel.Content = "≈" + deserializedJSON["interestScore"];
            }
            catch { MMRLabel.Content = "N/A"; }
        }
    }
}