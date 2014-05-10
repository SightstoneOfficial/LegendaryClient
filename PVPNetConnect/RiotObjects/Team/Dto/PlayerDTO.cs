using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Team.Dto
{
    public class PlayerDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.team.dto.PlayerDTO";

        public PlayerDTO()
        {
        }

        public PlayerDTO(Callback callback)
        {
            this.callback = callback;
        }

        public PlayerDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(PlayerDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("playerId")]
        public Double PlayerId { get; set; }

        [InternalName("teamsSummary")]
        public List<object> TeamsSummary { get; set; }

        [InternalName("createdTeams")]
        public List<object> CreatedTeams { get; set; }

        [InternalName("playerTeams")]
        public List<object> PlayerTeams { get; set; }
    }
}