#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using LegendaryClient.Logic.SQLite;

#endregion

namespace LegendaryClient.Logic.JSON
{
    public static class Runes
    {
        public static List<SQLite.Runes> PopulateRunes()
        {
            var runeList = new List<SQLite.Runes>();

            string runeJson =
                File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "Assets", "data", "en_US", "rune.json"));
            var serializer = new JavaScriptSerializer();
            var deserializedJson = serializer.Deserialize<Dictionary<string, object>>(runeJson);
            var runeData = deserializedJson["data"] as Dictionary<string, object>;

            if (runeData == null)
                return runeList;

            foreach (var rune in runeData)
            {
                var newRune = new SQLite.Runes();
                var singularRuneData = rune.Value as Dictionary<string, object>;
                newRune.Id = Convert.ToInt32(rune.Key);
                newRune.Name = singularRuneData["name"] as string;
                newRune.Description = singularRuneData["description"] as string;
                newRune.Description = newRune.Description.Replace("(", "\n");
                newRune.Description = newRune.Description.Replace(")", "");
                newRune.Stats = singularRuneData["stats"] as Dictionary<string, object>;
                newRune.Tags = singularRuneData["tags"] as ArrayList;
                var imageData = singularRuneData["image"] as Dictionary<string, object>;
                string uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "rune", (string) imageData["full"]);
                newRune.Icon = Client.GetImage(uriSource);

                runeList.Add(newRune);
            }
            return runeList;
        }
    }
}