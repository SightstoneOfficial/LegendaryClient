using agsXMPP.protocol.iq.roster;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace LegendaryClient.Logic
{
    class RosterManagment
    {
        private static List<RosterItem> rawRoster { get; set; }

        internal static void XmppConnection_OnRosterStart(object sender)
        {
            rawRoster = new List<RosterItem>();
            Client.XmppConnection.OnRosterItem += XmppConnection_OnRosterItem;
            Client.XmppConnection.OnRosterEnd += XmppConnection_OnRosterEnd;
        }

        private static void XmppConnection_OnRosterEnd(object sender)
        {
            Client.loadedGroups = false;
            Client.Groups.Add(new Group("Online"));
            foreach (var item in rawRoster)
            {
                string Parse = item.ToString();
                string temp;
                if (!Parse.Contains("</item>"))
                    temp = Parse + "</item>";
                else
                    temp = Parse;
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(temp);
                string PlayerJson = JsonConvert.SerializeXmlNode(xmlDocument).Replace("#", "").Replace("@", "");
                try
                {
                    if (PlayerJson.Contains(":{\"priority\":"))
                    {
                        RootObject root = JsonConvert.DeserializeObject<RootObject>(PlayerJson);

                        if (!string.IsNullOrEmpty(root.item.name) && !string.IsNullOrEmpty(root.item.note))
                            Client.PlayerNote.Add(root.item.name, root.item.note);

                        if (root.item.group.text != "**Default" && Client.Groups.Find(e => e.GroupName == root.item.group.text) == null && root.item.group.text != null)
                            Client.Groups.Add(new Group(root.item.group.text));
                    }
                    else
                    {
                        RootObject2 root = JsonConvert.DeserializeObject<RootObject2>(PlayerJson);

                        if (!string.IsNullOrEmpty(root.item.name) && !string.IsNullOrEmpty(root.item.note))
                            Client.PlayerNote.Add(root.item.name, root.item.note);

                        if (root.item.group != "**Default" && Client.Groups.Find(e => e.GroupName == root.item.group) == null && root.item.group != null)
                            Client.Groups.Add(new Group(root.item.group));
                    }
                }
                catch
                {
                    Client.Log("Can't load friends", "ERROR");
                }
            }

            Client.Groups.Add(new Group("Offline"));
            Client.SetChatHover();
            Client.loadedGroups = true;
            AddChatPlayers();
        }

        internal static void AddChatPlayers()
        {
            foreach(var item in rawRoster)
            {
                Client.UpdatePlayers = true;
                if (Client.AllPlayers.ContainsKey(item.Jid.User))
                    return;

                var player = new ChatPlayerItem
                {
                    Id = item.Jid.User,
                    Group = "Online"
                };

                //using (XmlReader reader = XmlReader.Create(new StringReader(ri.OuterXml)))
                using (XmlReader reader = XmlReader.Create(new StringReader(item.ToString())))
                {
                    while (reader.Read())
                    {
                        if (!reader.IsStartElement())
                            continue;

                        switch (reader.Name)
                        {
                            case "group":
                                reader.Read();
                                string TempGroup = reader.Value;
                                if (TempGroup != "**Default")
                                    player.Group = TempGroup;
                                break;
                        }
                    }
                }
                player.Username = item.Name;
                Client.AllPlayers.Add(item.Jid.User, player);
            }
        }

        internal static void XmppConnection_OnRosterItem(object sender, agsXMPP.protocol.iq.roster.RosterItem item)
        {
            rawRoster.Add(item);
        }
    }
}
