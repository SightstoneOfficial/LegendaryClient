using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryClient.Logic.TempRunes.LcRunes
{
    public abstract class Rune
    {
        public abstract string RuneType { get; }
        public abstract RuneStats Stats { get; }
    }
}
