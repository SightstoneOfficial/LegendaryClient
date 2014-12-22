using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;

namespace LegendaryClient.Patcher.Logic.Data
{
    public class Message
    {
        public int id { get; set; }
        public string author { get; set; }
        public string content { get; set; }
        public string severity { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public List<object> translations { get; set; }
    }

    public class ServerStatus
    {
        public string status { get; set; }
        public List<Message> messages { get; set; }
    }
    public class GetServerStatus
    {
        public ServerStatus Status;
        public GetServerStatus(Uri DownloadLink)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string status = client.DownloadString(DownloadLink);
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    Status = serializer.Deserialize<ServerStatus>(status);
                }
            }
            catch
            {
                MessageBox.Show("Unable to get server status from riot. Please ensure you are connected to the internet!",
                    "Connection Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.None);

                Environment.Exit(0);
            }
        }
    }
}
