#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using LegendaryClient.Logic.SQLite;

#endregion

namespace LegendaryClient.Logic.JSON
{
    public static class Masteries
    {
        public static List<masteries> PopulateMasteries()
        {
            var masteryList = new List<masteries>();

            string masteryJson =
                File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "Assets", "data", "en_US", "mastery.json"));
            var serializer = new JavaScriptSerializer();
            var deserializedJson = serializer.Deserialize<Dictionary<string, object>>(masteryJson);
            var masteryData = deserializedJson["data"] as Dictionary<string, object>;
            var treeData = deserializedJson["tree"] as Dictionary<string, object>;

            if (masteryData != null)
                foreach (var mastery in masteryData)
                {
                    var singularMasteryData = mastery.Value as Dictionary<string, object>;
                    var newMastery = new masteries {id = Convert.ToInt32(mastery.Key)};
                    if (singularMasteryData != null)
                    {
                        newMastery.name = singularMasteryData["name"] as string;
                        newMastery.description = singularMasteryData["description"] as ArrayList;
                        newMastery.ranks = (int) singularMasteryData["ranks"];
                        newMastery.prereq = Convert.ToInt32(singularMasteryData["prereq"]);

                        var imageData = singularMasteryData["image"] as Dictionary<string, object>;
                        if (imageData != null)
                        {
                            string uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "mastery",
                                (string) imageData["full"]);
                            newMastery.icon = Client.GetImage(uriSource);
                        }
                    }

                    masteryList.Add(newMastery);
                }

            if (treeData == null)
                return masteryList;

            foreach (var tree in treeData)
            {
                var list = (ArrayList) tree.Value;
                int i;
                for (i = 0; i < list.Count; i++)
                {
                    foreach (masteries tempMastery in from Dictionary<string, object> x in (ArrayList) list[i]
                        where x != null
                        select Convert.ToInt32(x["masteryId"])
                        into masteryId
                        select masteryList.Find(y => y.id == masteryId))
                    {
                        tempMastery.treeRow = i;
                        tempMastery.tree = tree.Key;
                    }
                }
            }

            return masteryList;
        }
    }
}