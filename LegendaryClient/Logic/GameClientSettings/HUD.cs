using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryClient.Logic.GameClientSettings
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    class HUD : Attribute
    {
        public string name { get; set; }
        public bool isHUD { get; set; }
        public HUD(string name)
        {
            this.name = name;
            this.isHUD = true;
        }
    }
}
