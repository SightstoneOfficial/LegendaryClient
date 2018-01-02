﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using Sightstone.Logic.SQLite;
using Sightstone.Logic.MultiUser;

namespace Sightstone.Logic.JSON
{
    public static class Champions
    {
        public static void InsertExtraChampData(champions champ)
        {
            var champJson =
                File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "Assets", "data", "en_US", "champion",
                    champ.name + ".json"));

            var serializer = new JavaScriptSerializer();
            var deserializedJson = serializer.Deserialize<Dictionary<string, object>>(champJson);
            //Dictionary<string, object> deserializedJSON = JsonConvert.DeserializeObject<Dictionary<string, object>>(champJSON);
            var temp = deserializedJson["data"] as Dictionary<string, object>;

            var champData = temp?[champ.name] as Dictionary<string, object>;
            if (champData == null)
                return;
            
            champ.ResourceType = champData["partype"] as string;
            var spells = (ArrayList) champData["spells"];
            champ.Spells = new List<Spell>();
            
            foreach (Dictionary<string, object> champSpells in spells)
            {
                var newSpell = new Spell
                {
                    ID = champSpells["id"] as string,
                    Name = champSpells["name"] as string,
                    Description = champSpells["description"] as string,
                    Tooltip = champSpells["tooltip"] as string,
                    MaxRank = (int) champSpells["maxrank"]
                };

                var image = (Dictionary<string, object>) champSpells["image"];
                newSpell.Image = image["full"] as string;
                foreach (Dictionary<string, object> x in (ArrayList) champSpells["vars"])
                {
                    var type = x["link"] as string;
                    if (type == null)
                        continue;

                    type = type.Replace("spelldamage", "Ability Power");
                    type = type.Replace("bonusattackdamage", "Bonus Attack Damage");
                    type = type.Replace("attackdamage", "Total Attack Damage");
                    type = type.Replace("armor", "Armor");
                    newSpell.Tooltip = newSpell.Tooltip.Replace("{{ " + x["key"] + " }}",
                        Convert.ToString(x["coeff"]) + " " + type);
                }

                var i = 0;
                foreach (var scaling in from ArrayList x in (ArrayList) champSpells["effect"]
                    where x != null
                    select x.Cast<object>().Aggregate("", (current, y) => current + (y + "/"))
                    into scaling
                    select scaling.Substring(0, scaling.Length - 1))
                {
                    i++;
                    newSpell.Tooltip = newSpell.Tooltip.Replace("{{ e" + i + " }}", scaling);
                }

                newSpell.Tooltip = newSpell.Tooltip.Replace("<br>", Environment.NewLine);
                newSpell.Tooltip = Regex.Replace(newSpell.Tooltip, "<.*?>", string.Empty);

                champ.Spells.Add(newSpell);
            }
        }
    }
}