using LegendaryClient.Logic.SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace LegendaryClient.Logic.JSON
{
    class LegendaryUpdate
    {

        public static List<UpdateData> PopulateItems()
        {
            var Json = new WebClient().DownloadString("http://eddy5641.github.io/LegendaryClient/updateData");
            List<UpdateData> ItemList = new List<UpdateData>();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> deserializedJSON = serializer.Deserialize<Dictionary<string, object>>(Json);
            Dictionary<string, object> updateData = deserializedJSON["updateData"] as Dictionary<string, object>;

            foreach (KeyValuePair<string, object> LegendaryClientUpdateData in updateData)
            {
                UpdateData newItem = new UpdateData();
                Dictionary<string, object> singularUpdateData = LegendaryClientUpdateData.Value as Dictionary<string, object>;
                newItem.version = singularUpdateData["version"] as string;
                newItem.active = singularUpdateData["active"] as bool?;
                newItem.isPreRelease = singularUpdateData["isPreRelease"] as bool?;
                newItem.downloadLink = singularUpdateData["downloadLink"] as string;
            }

            return ItemList;
        }
    }
}
