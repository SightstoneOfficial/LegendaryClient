using LegendaryClient.Logic.SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace LegendaryClient.Logic.JSON
{
    public static class Items
    {
        public static List<items> PopulateItems()
        {
            List<items> ItemList = new List<items>();

            string itemJSON = File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "Assets", "data", "en_US", "item.json"));
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> deserializedJSON = serializer.Deserialize<Dictionary<string, object>>(itemJSON);
            Dictionary<string, object> itemData = deserializedJSON["data"] as Dictionary<string, object>;

            foreach (KeyValuePair<string, object> item in itemData)
            {
                items newItem = new items();
                Dictionary<string, object> singularItemData = item.Value as Dictionary<string, object>;
                newItem.id = Convert.ToInt32(item.Key);
                newItem.name = singularItemData["name"] as string;
                newItem.description = singularItemData["description"] as string;

                Dictionary<string, object> goldData = singularItemData["gold"] as Dictionary<string, object>;
                newItem.price = Convert.ToInt32(goldData["total"]);
                newItem.sellprice = Convert.ToInt32(goldData["sell"]);

                Dictionary<string, object> imageData = singularItemData["image"] as Dictionary<string, object>;
                newItem.iconPath = imageData["full"] as string;

                ItemList.Add(newItem);
            }

            return ItemList;
        }
    }
}
