using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Game
{
    public class StartChampSelectDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.game.StartChampSelectDTO";

        public StartChampSelectDTO()
        {
        }

        public StartChampSelectDTO(Callback callback)
        {
            this.callback = callback;
        }

        public StartChampSelectDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(StartChampSelectDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("invalidPlayers")]
        public List<object> InvalidPlayers { get; set; }
    }
}