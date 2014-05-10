/**
 * A very basic RTMPS client
 *
 * @author Gabriel Van Eyck
 */
/////////////////////////////////////////////////////////////////////////////////
//
//Ported to C# by Ryan A. LaSarre
//
/////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace PVPNetConnect
{
    public class ClassDefinition
    {
        public string type;
        public bool externalizable = false;
        public bool dynamic = false;
        public List<string> members = new List<string>();
    }
}