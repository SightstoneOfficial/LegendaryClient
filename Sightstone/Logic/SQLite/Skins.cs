using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sightstone.Logic.SQLite
{
    public class Skins
    {
        /// <summary>
        /// The skin id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The skin number (starts at one and accends as there are more skins)
        /// </summary>
        public int Num { get; set; }

        /// <summary>
        /// The skin's name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// I guess that this is if you can use /toggle
        /// </summary>
        public bool Chromas { get; set; }

        /// <summary>
        /// Is it the default skin
        /// </summary>
        public bool IsBase { get; set; }

        /// <summary>
        /// Portrait path
        /// </summary>
        public string PortraitPath { get; set; }

        /// <summary>
        /// Splash path
        /// </summary>
        public string SplashPath { get; set; }
    }
}
