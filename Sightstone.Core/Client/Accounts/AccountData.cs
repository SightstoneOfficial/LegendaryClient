using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sightstone.Core.Client.Accounts
{
    /// <summary>
    /// Contains all data for accounts stored by Sightstone.Core
    /// </summary>
    public class AccountData
    {
        /// <summary>
        /// The loginname used when a user signs into league
        /// </summary>
        public string Loginname { get; set; }

        /// <summary>
        /// The password used when a user signs into league
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// The user's last summoner name when they last logged in
        /// </summary>
        public string SumName { get; set; }

        /// <summary>
        /// The user's last summoner icon when they last logged in
        /// </summary>
        public int SumIcon { get; set; }

        /// <summary>
        /// The user's sum id
        /// </summary>
        public double SumId { get; set; }
    }
}
