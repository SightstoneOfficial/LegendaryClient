using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PVPNetConnect
{
   public enum ErrorType
   {
      Password,
      AuthKey,
      Handshake,
      Connect,
      Login,
      Invoke,
      Receive,
      General
   }
   public class Error
   {
      public ErrorType Type;
      public string Message = "";
      public string ErrorCode = "";
   }
}
