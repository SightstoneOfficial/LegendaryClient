namespace LegendaryClient.Logic.GameClientSettings
{
    /// <summary>
    ///     All of League of Legends Settings for use with Phraser
    /// </summary>
    public class LOLGCSettings
    {
        #region GeneralSettings

        [General("UserSetResolution")]
        public int UserSetResolution { get; set; }

        [General("bindSysKeys")]
        public int BindSysKeys { get; set; }

        [General("SnapCameraOnRespawn")]
        public int SnapCameraOnRespawn { get; set; }

        [General("OSXMouseAcceleration")]
        public int OSXMouseAcceleration { get; set; }

        [General("AutoAcquireTarget")]
        public int AutoAcquireTarget { get; set; }

        [General("EnableLightFx")]
        public int EnableLightFx { get; set; }

        [General("WindowMode")]
        public int WindowMode { get; set; }

        [General("ShowTurretRangeIndicators")]
        public int ShowTurrentRangeIndicators { get; set; }

        [General("PredictMovement")]
        public int PredictMovement { get; set; }

        [General("WaitForVerticalSync")]
        public int WaitForVerticalSync { get; set; }

        [General("Colors")]
        public int Colors { get; set; }

        [General("Height")]
        public int Height { get; set; }

        [General("Width")]
        public int Width { get; set; }

        [General("SystemMouseSpeed")]
        public int SystemMouseSpeed { get; set; }

        [General("CfgVersion")]
        public string CfgVersion { get; set; }

        #endregion

        #region HUD

        [HUD("ShowTimestamps")]
        public int ShowTimestamps { get; set; }

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