namespace LegendaryClient.Logic.SQLite
{
    public class Spell
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Tooltip { get; set; }
        public int MaxRank { get; set; }
        public int CooldownBurn { get; set; }
        public int InitialCooldown { get; set; }
        public int CostBurn { get; set; }
        public int InitalCost { get; set; }
        public string Image { get; set; }
        public int Range { get; set; }
        public string Resource { get; set; }
    }
}