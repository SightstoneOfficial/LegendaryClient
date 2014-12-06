using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
