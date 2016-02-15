﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using Sightstone.Logic.SQLite;
using Sightstone.Logic.MultiUser;

namespace Sightstone.Logic.JSON
{
    public static class Runes
    {
        public static List<runes> PopulateRunes(string runeJson)
        {
            var runeList = new List<runes>();
            
            var serializer = new JavaScriptSerializer();
            var deserializedJson = serializer.Deserialize<Dictionary<string, object>>(runeJson);
            var runeData = deserializedJson["data"] as Dictionary<string, object>;

            if (runeData == null)
                return runeList;

            foreach (var rune in runeData)
            {
                var singularRuneData = rune.Value as Dictionary<string, object>;
                if (singularRuneData == null)
                    continue;

                var newRune = new runes
                {
                    id = Convert.ToInt32(rune.Key),
                    name = singularRuneData["name"] as string,
                    description = singularRuneData["description"] as string
                };
                newRune.description = newRune.description.Replace("(", "\n");
                newRune.description = newRune.description.Replace(")", string.Empty);
                newRune.stats = singularRuneData["stats"] as Dictionary<string, object>;
                newRune.tags = singularRuneData["tags"] as ArrayList;

                var imageData = singularRuneData["image"] as Dictionary<string, object>;
                if (imageData != null)
                {
                    var UriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "rune",
                        (string) imageData["full"]);
                    newRune.icon = Client.GetImage(UriSource);
                }

                runeList.Add(newRune);
            }
            return runeList;
        }
    }
}