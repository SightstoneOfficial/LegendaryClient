#region

using System.Collections.Generic;
using System.Net;
using System.Web.Script.Serialization;
using LegendaryClient.Logic.SQLite;

#endregion

namespace LegendaryClient.Logic.JSON
{
    internal class LegendaryUpdate
    {
        public static List<UpdateData> PopulateItems()
        {
            string json = new WebClient().DownloadString("http://eddy5641.github.io/LegendaryClient/updateData");
            var itemList = new List<UpdateData>();
            var serializer = new JavaScriptSerializer();
            var deserializedJson = serializer.Deserialize<Dictionary<string, object>>(json);
            var updateData = deserializedJson["updateData"] as Dictionary<string, object>;

            if (updateData == null)
                return itemList;

            foreach (var legendaryClientUpdateData in updateData)
            {
                var newItem = new UpdateData();
                var singularUpdateData = legendaryClientUpdateData.Value as Dictionary<string, object>;
                if (singularUpdateData == null)
                    continue;

                newItem.version = singularUpdateData["version"] as string;
                newItem.active = singularUpdateData["active"] as bool?;
                newItem.isPreRelease = singularUpdateData["isPreRelease"] as bool?;
                newItem.downloadLink = singularUpdateData["downloadLink"] as string;
            }

            return itemList;
        }
    }
}