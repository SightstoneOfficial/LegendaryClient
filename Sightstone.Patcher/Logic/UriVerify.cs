using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sightstone.Patcher.Logic
{
    public class UriVerify
    {
        public static bool VerifyUri(Uri toVerify)
        {
            try
            {
                var request = WebRequest.Create(toVerify);
                request.Method = "HEAD";
                using (var response = (HttpWebResponse)request.GetResponse())
                    return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                return false;
            }
        }

        public static Uri VerifyUri(Uri[] toVerify)
        {
            try
            {
                var request = WebRequest.Create(toVerify[0]);
                request.Method = "HEAD";
                using (var response = (HttpWebResponse)request.GetResponse())
                    return response.StatusCode == HttpStatusCode.OK ? toVerify[0] : toVerify[1];
            }
            catch
            {
                return toVerify[1];
            }
        }
    }
}
