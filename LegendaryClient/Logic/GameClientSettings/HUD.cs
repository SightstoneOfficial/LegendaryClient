#region

using System;

#endregion

namespace LegendaryClient.Logic.GameClientSettings
{
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Property,
        AllowMultiple = true)]
    internal class HUD : Attribute
    {
        public HUD(string name)
        {
            Name = name;
            IsHUD = true;
        }

        public string Name { get; set; }
        public bool IsHUD { get; set; }
    }
}