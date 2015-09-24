using System;

namespace Sightstone.Logic.Maps
{
    public abstract class BaseMap
    {
        public abstract string DisplayName { get; }

        public static BaseMap GetMap(int requestedMap)
        {
            Type t = Type.GetType("Sightstone.Logic.Maps.Map" + requestedMap);

            if (t != null)
            {
                return (BaseMap) Activator.CreateInstance(t);
            }
            return new UnknownMap();
        }
    }

    public class UnknownMap : BaseMap
    {
        public override string DisplayName
        {
            get { return "Unknown Map"; }
        }
    }
}