using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sightstone.Logic.SWF
{
    public class Images
    {

        public int index { set; get; }

        public string name { set; get; }

        public byte[] data { set; get; }

        public string type { set; get; }

        public ushort height { set; get; }

        public ushort width { get; set; }
    }
}
