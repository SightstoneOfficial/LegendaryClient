using Sightstone.Logic.JSON.Object;
using Sightstone.Logic.MultiUser;
using Sightstone.Logic.SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace Sightstone.Logic.JSON
{
    public static class ParseItems
    {
        public static List<items> PopulateItems()
        {
            var itemList = new List<items>();

            string itemJson =
                File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "items.json"));
            var serializer = new JavaScriptSerializer();
            var deserializedJson = serializer.Deserialize<Items.Rootobject>(itemJson);
            var itemData = deserializedJson.items;

            if (itemData == null)
                return itemList;

            foreach (var item in itemData)
            {
                var singularItemData = item;

                var newItem = new items
                {
                    id = Convert.ToInt32(item.id),
                    name = item.name,
                    description = item.description
                };

                var goldData = item.gold;
                if (goldData != null)
                {
                    newItem.price = goldData.total;
                    newItem.sellprice = goldData.sell;
                }

                newItem.iconPath = item.icon.Replace(".dds", ".bmp");

                itemList.Add(newItem);
            }

            return itemList;
        }
    }
}