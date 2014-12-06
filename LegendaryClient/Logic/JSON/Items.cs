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
        public static List<items> PopulateItems()
        {
            var itemList = new List<items>();

            string itemJson =
                File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "Assets", "data", "en_US", "item.json"));
            var serializer = new JavaScriptSerializer();
            var deserializedJson = serializer.Deserialize<Dictionary<string, object>>(itemJson);
            var itemData = deserializedJson["data"] as Dictionary<string, object>;

            if (itemData == null)
                return itemList;

            foreach (var item in itemData)
            {
                var newItem = new items();
                var singularItemData = item.Value as Dictionary<string, object>;
                newItem.id = Convert.ToInt32(item.Key);
                newItem.name = singularItemData["name"] as string;
                newItem.description = singularItemData["description"] as string;

                var goldData = singularItemData["gold"] as Dictionary<string, object>;
                newItem.price = Convert.ToInt32(goldData["total"]);
                newItem.sellprice = Convert.ToInt32(goldData["sell"]);

                var imageData = singularItemData["image"] as Dictionary<string, object>;
                newItem.iconPath = imageData["full"] as string;

                itemList.Add(newItem);
            }

            return itemList;
        }
    }
}