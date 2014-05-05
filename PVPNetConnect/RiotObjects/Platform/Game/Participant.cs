namespace PVPNetConnect.RiotObjects.Platform.Game
{
    public class Participant : RiotGamesObject
    {
        public Participant()
        {
        }

        public delegate void Callback(Participant result);

        private Callback callback;

        public Participant(Callback callback)
        {
            this.callback = callback;
        }

        public Participant(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }
    }
}