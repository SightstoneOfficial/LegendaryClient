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
        public static List<SQLite.Masteries> PopulateMasteries()
        {
            var masteryList = new List<SQLite.Masteries>();

            string masteryJson =
                File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "Assets", "data", "en_US", "mastery.json"));
            var serializer = new JavaScriptSerializer();
            var deserializedJson = serializer.Deserialize<Dictionary<string, object>>(masteryJson);
            var masteryData = deserializedJson["data"] as Dictionary<string, object>;
            var treeData = deserializedJson["tree"] as Dictionary<string, object>;

            if (masteryData != null)
                foreach (var mastery in masteryData)
                {
                    var newMastery = new SQLite.Masteries();
                    var singularMasteryData = mastery.Value as Dictionary<string, object>;
                    newMastery.Id = Convert.ToInt32(mastery.Key);
                    newMastery.Name = singularMasteryData["name"] as string;
                    newMastery.Description = singularMasteryData["description"] as ArrayList;
                    newMastery.Ranks = (int) singularMasteryData["ranks"];
                    newMastery.Prereq = Convert.ToInt32(singularMasteryData["prereq"]);

                    var imageData = singularMasteryData["image"] as Dictionary<string, object>;
                    string uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "mastery",
                        (string) imageData["full"]);
                    newMastery.Icon = Client.GetImage(uriSource);

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
                    foreach (SQLite.Masteries tempMastery in from Dictionary<string, object> x in (ArrayList) list[i]
                        where x != null
                        select Convert.ToInt32(x["masteryId"])
                        into masteryId
                        select masteryList.Find(y => y.Id == masteryId))
                    {
                        tempMastery.TreeRow = i;
                        tempMastery.Tree = tree.Key;
                    }
                }
            }

            return masteryList;
        }
    }
}