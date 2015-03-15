#region

using PVPNetConnect;
using PVPNetConnect.RiotObjects.Leagues.Pojo;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Script.Serialization;

#endregion

namespace LegendaryClient.Logic.JSON
{
    internal class LeaguePromote
    {
        public static PromoteItem LeaguesPromote(string json)
        {
            var newItem = new PromoteItem();
            var serializer = new JavaScriptSerializer();
            var deserializedJson = serializer.Deserialize<TypedObject>(json);
            var leagueItem = deserializedJson["leagueItem"] as Dictionary<string, object>;
            if (leagueItem == null)
                return newItem;

            var asd = new TypedObject();
            newItem.notifyReason = deserializedJson["notifyReason"] as string;
            newItem.LeaguePointsDelta = deserializedJson["leaguePointsDelta"] as int?;
            newItem.LeaguePointsDelta = deserializedJson["gameId"] as int?;
            foreach (var item in leagueItem)
            {
                asd.Add(item.Key,item.Value);
            }
            newItem.leagueItem = new LeagueItemDTO(asd);
            return newItem;
        }
    }
}