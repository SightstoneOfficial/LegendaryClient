using System;

namespace LegendaryClient.Logic.Maps
{
    public abstract class BaseMap
    {
        public abstract string DisplayName { get; }

        public static BaseMap GetMap(int RequestedMap)
        {
            Type t = Type.GetType("LegendaryClient.Logic.Maps.Map" + RequestedMap);

            if (t != null)
            {
                return (BaseMap)Activator.CreateInstance(t);
            }
            return null;
        }
    }
}