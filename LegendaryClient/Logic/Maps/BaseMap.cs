#region

using System;

#endregion

namespace LegendaryClient.Logic.Maps
{
    public abstract class BaseMap
    {
        public abstract string DisplayName { get; }

        public static BaseMap GetMap(int requestedMap)
        {
            Type t = Type.GetType("LegendaryClient.Logic.Maps.Map" + requestedMap);

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