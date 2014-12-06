namespace LegendaryClient.Logic.SQLite
{
    public class UpdateData
    {
        public string Version { get; set; }
        public bool? Active { get; set; }
        public bool? IsPreRelease { get; set; }
        public string DownloadLink { get; set; }
        public string FileName { get; set; }
    }
}