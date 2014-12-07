﻿#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using LegendaryClient.Logic.SQLite;

#endregion

namespace LegendaryClient.Logic.JSON
{
    public static class Champions
    {
        public static void InsertExtraChampData(champions Champ)
        {
            string champJSON =
                File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "Assets", "data", "en_US", "champion",
                    Champ.name + ".json"));
            var serializer = new JavaScriptSerializer();
            var deserializedJson = serializer.Deserialize<Dictionary<string, object>>(champJSON);
            //Dictionary<string, object> deserializedJSON = JsonConvert.DeserializeObject<Dictionary<string, object>>(champJSON);
            var temp = deserializedJson["data"] as Dictionary<string, object>;
            var champData = temp[Champ.name] as Dictionary<string, object>;
            //Dictionary<string, object> champData = serializer.Deserialize<Dictionary<string, object>>(temp2);

            Champ.Lore = champData["lore"] as string;
            Champ.ResourceType = champData["partype"] as string;
            Champ.Skins = champData["skins"] as ArrayList;
            var spells = (ArrayList) champData["spells"];
            Champ.Spells = new List<Spell>();

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
                    type = type.Replace("spelldamage", "Ability Power");
                    type = type.Replace("bonusattackdamage", "Bonus Attack Damage");
                    type = type.Replace("attackdamage", "Total Attack Damage");
                    type = type.Replace("armor", "Armor");
                    newSpell.Tooltip = newSpell.Tooltip.Replace("{{ " + x["key"] + " }}",
                        Convert.ToString(x["coeff"]) + " " + type);
                }

                int i = 0;
                foreach (ArrayList x in (ArrayList) champSpells["effect"])
                {
                    if (x == null)
                        continue;

                    string scaling = x.Cast<object>().Aggregate("", (current, y) => current + (y + "/"));

                    scaling = scaling.Substring(0, scaling.Length - 1);

                    i++;
                    newSpell.Tooltip = newSpell.Tooltip.Replace("{{ e" + i + " }}", scaling);
                }

                newSpell.Tooltip = newSpell.Tooltip.Replace("<br>", Environment.NewLine);
                newSpell.Tooltip = Regex.Replace(newSpell.Tooltip, "<.*?>", string.Empty);

                Champ.Spells.Add(newSpell);
            }
        }
    }
}