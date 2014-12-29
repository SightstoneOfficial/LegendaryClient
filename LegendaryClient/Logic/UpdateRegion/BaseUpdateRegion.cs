using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryClient.Logic.UpdateRegion
{
    public abstract class BaseUpdateRegion
    {
        public abstract string AirListing { get; }

        public abstract string AirManifest { get; }

        public abstract string GameListing { get; }

        public abstract string SolutionListing { get; }

        public abstract string SolutionManifest { get; }

        public abstract string BaseLink { get; }

        public static BaseUpdateRegion GetUpdateRegion(String requestedRegion)
        {
            Type t = Type.GetType("LegendaryClient.Logic.UpdateRegion." + requestedRegion);
            return (BaseUpdateRegion)Activator.CreateInstance(t);
        }
    }
}
