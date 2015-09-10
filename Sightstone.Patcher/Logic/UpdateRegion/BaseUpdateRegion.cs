using System;

namespace Sightstone.Patcher.Logic.UpdateRegion
{
    public abstract class BaseUpdateRegion
    {
        public abstract string AirListing { get; }

        public abstract string AirManifest { get; }

        public abstract string GameListing { get; }

        public abstract string SolutionListing { get; }

        public abstract string SolutionManifest { get; }

        public abstract string BaseLink { get; }

        public static BaseUpdateRegion GetUpdateRegion(string requestedRegion)
        {
            Type t = Type.GetType("Sightstone.Patcher.Logic.UpdateRegion." + requestedRegion);
            return (BaseUpdateRegion)Activator.CreateInstance(t);
        }
    }
}
