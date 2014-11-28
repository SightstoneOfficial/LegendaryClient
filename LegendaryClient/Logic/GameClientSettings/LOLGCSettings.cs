using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryClient.Logic.GameClientSettings
{
    /// <summary>
    /// All of League of Legends Settings for use with Phraser 
    /// </summary>
    public class LOLGCSettings
    {
        #region GeneralSettings
        [General("UserSetResolution")]
        public int userSetResolution { get; set; }
        [General("bindSysKeys")]
        public int bindSysKeys { get; set; }
        [General("SnapCameraOnRespawn")]
        public int snapCameraOnRespawn { get; set; }
        [General("OSXMouseAcceleration")]
        public int OSXMouseAcceleration { get; set; }
        [General("AutoAcquireTarget")]
        public int autoAcquireTarget { get; set; }
        [General("EnableLightFx")]
        public int enableLightFx { get; set; }
        [General("WindowMode")]
        public int windowMode { get; set; }
        [General("ShowTurretRangeIndicators")]
        public int showTurrentRangeIndicators { get; set; }
        [General("PredictMovement")]
        public int predictMovement { get; set; }
        [General("WaitForVerticalSync")]
        public int waitForVerticalSync { get; set; }
        [General("Colors")]
        public int colors { get; set; }
        [General("Height")]
        public int height { get; set; }
        [General("Width")]
        public int width { get; set; }
        [General("SystemMouseSpeed")]
        public int systemMouseSpeed { get; set; }
        [General("CfgVersion")]
        public string cfgVersion { get; set; }
        #endregion

        #region HUD
        [HUD("ShowTimestamps")]
        public int showTimestamps { get; set; }
        [HUD("ChatScale")]
        [HUD("NewAggroIndicator")]
        [HUD("NameTagDisplay")]
        [HUD("ShowChampionIndicator")]
        [HUD("NameTagDisplay")]
        [HUD("ShowChampionIndicator")]
        [HUD("MinimapMoveSelf")]
        [HUD("ShowSummonerNames")]
        [HUD("ScrollSmoothingEnabled")]
        [HUD("MiddleMouseScrollSpeed")]
        [HUD("MapScrollSpeed")]
        [HUD("ItemShopPrevY")]
        [HUD("ItemShopPrevX")]
        [HUD("ItemShopPrevResizeHeight")]
        [HUD("ItemShopPrevResizeWidth")]
        [HUD("ShowAllChannelChat")]
        [HUD("ShowAttackRadius")]
        [HUD("NumericCooldownFormat")]
        [HUD("SmartCastOnKeyRelease")]
        [HUD("EnableSnowEffect")]
        [HUD("EnableLineMissileVis")]
        [HUD("FlipMiniMap")]
        [HUD("LockCamera")]
        [HUD("FlashScreenWhenStunned")]
        [HUD("ItemShopResizeHeight")]
        [HUD("ItemShopResizeWidth")]
        [HUD("ItemShopItemDisplayMode")]
        [HUD("ItemShopStartPane")]
        public string ItemShopStartPane { get; set; }
        #endregion
    }
}
