using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPNetConnect.RiotObjects.Platform.Game
{
   public class ObfruscatedParticipant : Participant
   {
      public override string TypeName
      {
         get
         {
            return this.type;
         }
      }

      private string type = "com.riotgames.platform.game.ObfruscatedParticipant";

      public ObfruscatedParticipant()
      {
      }

      public ObfruscatedParticipant(Callback callback)
      {
         this.callback = callback;
      }

      public ObfruscatedParticipant(TypedObject result)
      {
         base.SetFields(this, result);
      }

      public delegate void Callback(ObfruscatedParticipant result);

      private Callback callback;

      public override void DoCallback(TypedObject result)
      {
         base.SetFields(this, result);
         callback(this);
      }

      [InternalName("badges")]
      public Int32 Badges { get; set; }

      [InternalName("index")]
      public Int32 Index { get; set; }

      [InternalName("clientInSynch")]
      public Boolean ClientInSynch { get; set; }

      [InternalName("gameUniqueId")]
      public Int32 GameUniqueId { get; set; }

      [InternalName("pickMode")]
      public Int32 PickMode { get; set; }
   }
}
