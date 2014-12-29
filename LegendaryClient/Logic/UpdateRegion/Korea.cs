using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryClient.Logic.UpdateRegion
{
    public sealed class Korea : BaseUpdateRegion
    {
        public override string AirListing
        {
            get { return "http://legendspatch-lol.x-cdn.com/KR_CBT/projects/lol_air_client/releases/releaselisting_KR"; }
        }

        public override string AirManifest
        {
            get { return "http://legendspatch-lol.x-cdn.com/KR_CBT/projects/lol_air_client/"; }
        }

        public override string GameListing
        {
            get { return "http://legendspatch-lol.x-cdn.com/KR_CBT/projects/lol_game_client/releases/releaselisting_KR"; }
        }

        public override string SolutionListing
        {
            get { return "http://legendspatch-lol.x-cdn.com/KR_CBT/solutions/lol_game_client_sln/releases/releaselisting_KR"; }
        }

        public override string SolutionManifest
        {
            get { return "http://legendspatch-lol.x-cdn.com/KR_CBT/solutions/lol_game_client_sln/"; }
        }

        public override string BaseLink
        {
            get { return "http://legendspatch-lol.x-cdn.com/KR_CBT"; }
        }
    }
}
