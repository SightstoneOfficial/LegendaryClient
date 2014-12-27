using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryClient.Logic.UpdateRegion
{
    public sealed class PBE : BaseUpdateRegion
    {
        public override string AirListing
        {
            get { return "http://l3cdn.riotgames.com/releases/pbe/projects/lol_air_client/releases/releaselisting_PBE"; }
        }

        public override string AirManifest
        {
            get { return "http://l3cdn.riotgames.com/releases/pbe/projects/lol_air_client/"; }
        }

        public override string GameListing
        {
            get { return "http://l3cdn.riotgames.com/releases/pbe/projects/lol_game_client/releases/releaselisting_PBE"; }
        }

        public override string SolutionListing
        {
            get { return "http://l3cdn.riotgames.com/releases/pbe/solutions/lol_game_client_sln/releases/releaselisting_PBE"; }
        }

        public override string SolutionManifest
        {
            get { return "http://l3cdn.riotgames.com/releases/pbe/solutions/lol_game_client_sln/"; }
        }

        public override string BaseLink
        {
            get { return "http://l3cdn.riotgames.com/releases/pbe"; }
        }
    }
}
