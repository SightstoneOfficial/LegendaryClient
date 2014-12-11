namespace LegendaryClient.Logic.SQLite
{
    public class UpdateData
    {
        public string version { get; set; }
        public bool? active { get; set; }
        public bool? isPreRelease { get; set; }
        public string downloadLink { get; set; }
        public string fileName { get; set; }
    }
}