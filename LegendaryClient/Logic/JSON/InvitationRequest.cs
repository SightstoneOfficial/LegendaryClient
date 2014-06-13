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
    public static class InvitationRequest
    {
        public static List<invitationRequest> PopulateGameInviteJson()
        {
            List<invitationRequest> ItemList = new List<invitationRequest>();
            PVPNetConnect.RiotObjects.Gameinvite.Contract.InvitationRequest Invite = new PVPNetConnect.RiotObjects.Gameinvite.Contract.InvitationRequest();
            if (Invite.GameMetaData != null)
            {
                string itemJSON = File.ReadAllText(Invite.GameMetaData);
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> deserializedJSON = serializer.Deserialize<Dictionary<string, object>>(itemJSON);
                Dictionary<string, object> itemData = deserializedJSON["data"] as Dictionary<string, object>;

                foreach (KeyValuePair<string, object> item in itemData)
                {
                    invitationRequest newItem = new invitationRequest();
                    Dictionary<string, object> singularItemData = item.Value as Dictionary<string, object>;
                    newItem.queueId = singularItemData["queueId"] as int?;
                    newItem.isRanked = singularItemData["isRanked"] as bool?;
                    newItem.rankedTeamName = singularItemData["rankedTeamName"] as string;
                    newItem.mapId = singularItemData["mapId"] as int?;
                    newItem.gameTypeConfigId = singularItemData["gameTypeConfigId"] as int?;
                    newItem.gameMode = singularItemData["gameMode"] as string;
                    newItem.gameType = singularItemData["gameType"] as string;

                    ItemList.Add(newItem);
                }
            }
            

            return ItemList;
        }
    }
}
