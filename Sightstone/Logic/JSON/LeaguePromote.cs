﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Remoting.Contexts;
using System.Web.Script.Serialization;
using Sightstone.Logic.Riot.Leagues;
using RtmpSharp.IO;

namespace Sightstone.Logic.JSON
{
    internal class LeaguePromote
    {
        public static PromoteItem LeaguesPromote(string json)
        {
            var newItem = new PromoteItem();
            var serializer = new JavaScriptSerializer();
            var deserializedJson = serializer.Deserialize<Dictionary<string, object>>(json);
            var leagueItem = deserializedJson["leagueItem"] as Dictionary<string, object>;
            if (leagueItem == null)
                return newItem;

            var asd = new AsObject();
            newItem.notifyReason = deserializedJson["notifyReason"] as string;
            newItem.LeaguePointsDelta = deserializedJson["leaguePointsDelta"] as int?;
            newItem.LeaguePointsDelta = deserializedJson["gameId"] as int?;
            foreach (var item in leagueItem)
            {
                asd.Add(item.Key,item.Value);
            }
            newItem.leagueItem = (LeagueItemDTO)(object)asd;
            return newItem;
        }
    }
}