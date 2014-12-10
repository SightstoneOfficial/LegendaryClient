namespace LegendaryClient.Logic.TempRunes.LcRunes
{
    public abstract class Rune
    {
        public abstract string RuneType { get; }
        public abstract RuneStats Stats { get; }
    }
}