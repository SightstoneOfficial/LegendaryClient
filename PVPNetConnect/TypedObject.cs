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

using System;
using System.Collections.Generic;
using System.Text;

namespace PVPNetConnect
{
    public class TypedObject : Dictionary<string, object>
    {
        private static long serialVersionUID = 1244827787088018807L;

        public string type;

        public TypedObject()
        {
            this.type = null;
        }

        public TypedObject(string type)
        {
            this.type = type;
        }

        public static TypedObject MakeArrayCollection(object[] data)
        {
            TypedObject ret = new TypedObject("flex.messaging.io.ArrayCollection");
            ret.Add("array", data);
            return ret;
        }

        public TypedObject GetTO(string key)
        {
            if (this.ContainsKey(key) && this[key] is TypedObject)
                return (TypedObject)this[key];

            return null;
        }

        public string GetString(string key)
        {
            return (string)this[key];
        }

        public int? GetInt(string key)
        {
            object val = this[key];
            if (val == null)
                return null;
            else if (val is int)
                return (int)val;
            else
                return Convert.ToInt32((double)val);
        }

        public double? GetDouble(string key)
        {
            object val = this[key];
            if (val == null)
                return null;
            else if (val is double)
                return (double)val;
            else
                return Convert.ToDouble((int)val);
        }

        public bool GetBool(string key)
        {
            return (bool)this[key];
        }

        public object[] GetArray(string key)
        {
            if (this[key] is TypedObject && GetTO(key).type.Equals("flex.messaging.io.ArrayCollection"))
                return (object[])GetTO(key)["array"];
            else
                return (object[])this[key];
        }

        public override string ToString()
        {
            if (type == null)
                return base.ToString();
            else if (type.Equals("flex.messaging.io.ArrayCollection"))
            {
                StringBuilder sb = new StringBuilder();
                object[] data = (object[])this["array"];
                sb.Append("ArrayCollection[");
                for (int i = 0; i < data.Length; i++)
                {
                    sb.Append(data[i]);
                    if (i < data.Length - 1)
                        sb.Append(", ");
                }
                sb.Append(']');
                return sb.ToString();
            }
            else
                return type + ":" + base.ToString();
        }
    }
}