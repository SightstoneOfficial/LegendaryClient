#region

using System;

#endregion

namespace LegendaryClient.Logic.GameClientSettings
{
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Property,
        AllowMultiple = true)]
    public class General : Attribute
    {
        public General(string name)
        {
            Name = name;
            IsGeneral = true;
        }

        public string Name { get; set; }
        public bool IsGeneral { get; set; }
    }
}