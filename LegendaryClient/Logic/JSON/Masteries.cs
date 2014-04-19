using LegendaryClient.Logic.SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Media.Imaging;

namespace LegendaryClient.Logic.JSON
{
    public static class Masteries
    {
        public static List<masteries> PopulateMasteries()
        {
            List<masteries> MasteryList = new List<masteries>();

            string masteryJSON = File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "Assets", "data", "en_US", "mastery.json"));
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> deserializedJSON = serializer.Deserialize<Dictionary<string, object>>(masteryJSON);
            Dictionary<string, object> masteryData = deserializedJSON["data"] as Dictionary<string, object>;
            Dictionary<string, object> treeData = deserializedJSON["tree"] as Dictionary<string, object>;

            foreach (KeyValuePair<string, object> mastery in masteryData)
            {
                masteries newMastery = new masteries();
                Dictionary<string, object> singularMasteryData = mastery.Value as Dictionary<string, object>;
                newMastery.id = Convert.ToInt32(mastery.Key);
                newMastery.name = singularMasteryData["name"] as string;
                newMastery.description = singularMasteryData["description"] as ArrayList;
                newMastery.ranks = (int)singularMasteryData["ranks"];
                newMastery.prereq = Convert.ToInt32(singularMasteryData["prereq"]);

                Dictionary<string, object> imageData = singularMasteryData["image"] as Dictionary<string, object>;
                var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "mastery", (string)imageData["full"]), UriKind.Absolute);
                newMastery.icon = new BitmapImage(uriSource);

                MasteryList.Add(newMastery);
            }

            int i = 0;
            foreach (KeyValuePair<string, object> tree in treeData)
            {
                ArrayList list = (ArrayList)tree.Value;
                for (i = 0; i < list.Count; i++)
                {
                    foreach (Dictionary<string, object> x in (ArrayList)list[i])
                    {
                        if (x != null)
                        {
                            int MasteryId = Convert.ToInt32(x["masteryId"]);
                            masteries tempMastery = MasteryList.Find(y => y.id == MasteryId);
                            tempMastery.treeRow = i;
                            tempMastery.tree = tree.Key;
                        }
                    }
                }
            }

            return MasteryList;
        }
    }
}
