using LegendaryClient.Logic.SQLite;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace LegendaryClient.Logic.JSON
{
    public static class Champions
    {
        public static void InsertExtraChampData(champions Champ)
        {
            string champJSON;
            champJSON = File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "Assets", "data", "en_US", "champion", Champ.name + ".json"));
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> deserializedJSON = serializer.Deserialize<Dictionary<string, object>>(champJSON);
            //Dictionary<string, object> deserializedJSON = JsonConvert.DeserializeObject<Dictionary<string, object>>(champJSON);
            Dictionary<string, object> temp = deserializedJSON["data"] as Dictionary<string, object>;
            Dictionary<string, object> champData = temp[Champ.name] as Dictionary<string, object>;
            //Dictionary<string, object> champData = serializer.Deserialize<Dictionary<string, object>>(temp2);

            Champ.Lore = champData["lore"] as string;
            Champ.ResourceType = champData["partype"] as string;
            Champ.Skins = champData["skins"] as ArrayList;
            ArrayList Spells = (ArrayList)champData["spells"];
            Champ.Spells = new List<Spell>();

            foreach (Dictionary<string, object> champSpells in Spells)
            {
                Spell NewSpell = new Spell();
                NewSpell.ID = champSpells["id"] as string;
                NewSpell.Name = champSpells["name"] as string;
                NewSpell.Description = champSpells["description"] as string;
                NewSpell.Tooltip = champSpells["tooltip"] as string;
                NewSpell.MaxRank = (int)champSpells["maxrank"];
                Dictionary<string, object> Image = (Dictionary<string, object>)champSpells["image"];
                NewSpell.Image = Image["full"] as string;
                foreach (Dictionary<string, object> x in (ArrayList)champSpells["vars"])
                {
                    string Type = x["link"] as string;
                    Type = Type.Replace("spelldamage", "Ability Power");
                    Type = Type.Replace("bonusattackdamage", "Bonus Attack Damage");
                    Type = Type.Replace("attackdamage", "Total Attack Damage");
                    Type = Type.Replace("armor", "Armor");
                    NewSpell.Tooltip = NewSpell.Tooltip.Replace("{{ " + x["key"] + " }}", Convert.ToString(x["coeff"]) + " " + Type);
                }

                int i = 0;
                foreach (ArrayList x in (ArrayList)champSpells["effect"])
                {
                    string Scaling = "";
                    if (x == null)
                        continue;

                    foreach (var y in x)
                        Scaling += y + "/";

                    Scaling = Scaling.Substring(0, Scaling.Length - 1);

                    i++;
                    NewSpell.Tooltip = NewSpell.Tooltip.Replace("{{ e" + i + " }}", Scaling);
                }

                NewSpell.Tooltip = NewSpell.Tooltip.Replace("<br>", Environment.NewLine);
                NewSpell.Tooltip = Regex.Replace(NewSpell.Tooltip, "<.*?>", string.Empty);

                Champ.Spells.Add(NewSpell);
            }
        }
    }
}
