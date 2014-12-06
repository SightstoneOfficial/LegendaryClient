#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using LegendaryClient.Logic.SQLite;

#endregion

namespace LegendaryClient.Logic.JSON
{
    public static class Items
    {
        public static List<SQLite.Items> PopulateItems()
        {
            var itemList = new List<SQLite.Items>();

            string itemJson =
                File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "Assets", "data", "en_US", "item.json"));
            var serializer = new JavaScriptSerializer();
            var deserializedJson = serializer.Deserialize<Dictionary<string, object>>(itemJson);
            var itemData = deserializedJson["data"] as Dictionary<string, object>;

            if (itemData == null)
                return itemList;

            foreach (var item in itemData)
            {
                var newItem = new SQLite.Items();
                var singularItemData = item.Value as Dictionary<string, object>;
                newItem.Id = Convert.ToInt32(item.Key);
                newItem.Name = singularItemData["name"] as string;
                newItem.Description = singularItemData["description"] as string;

                var goldData = singularItemData["gold"] as Dictionary<string, object>;
                newItem.Price = Convert.ToInt32(goldData["total"]);
                newItem.SellPrice = Convert.ToInt32(goldData["sell"]);

                var imageData = singularItemData["image"] as Dictionary<string, object>;
                newItem.IconPath = imageData["full"] as string;

                itemList.Add(newItem);
            }

            return itemList;
        }
    }
}