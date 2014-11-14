using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryClient.Logic.GameClientSettings
{
    public class LOLGCSettings
    {
        //General
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
    }
}
