using System;
using System.Reflection;

namespace PVPNetConnect
{
    public enum Region
    {
        [ServerValue("prod.na2.lol.riotgames.com")]
        [LoginQueueValue("https://lq.na2.lol.riotgames.com/")]
        [LocaleValue("en_US")]
        [UseGarenaValue(false)]
        [GarenaAuthServerValue(null)]
        NA,

        [ServerValue("prod.euw1.lol.riotgames.com")]
        [LoginQueueValue("https://lq.euw1.lol.riotgames.com/")]
        [LocaleValue("en_GB")]
        [UseGarenaValue(false)]
        [GarenaAuthServerValue(null)]
        EUW,

        [ServerValue("prod.eun1.lol.riotgames.com")]
        [LoginQueueValue("https://lq.eun1.lol.riotgames.com/")]
        [LocaleValue("en_GB")]
        [UseGarenaValue(false)]
        [GarenaAuthServerValue(null)]
        EUN,

        [ServerValue("prod.kr.lol.riotgames.com")]
        [LoginQueueValue("https://lq.kr.lol.riotgames.com/")]
        [LocaleValue("ko_KR")]
        [UseGarenaValue(false)]
        [GarenaAuthServerValue(null)]
        KR,

        [ServerValue("prod.br.lol.riotgames.com")]
        [LoginQueueValue("https://lq.br.lol.riotgames.com/")]
        [LocaleValue("pt_BR")]
        [UseGarenaValue(false)]
        [GarenaAuthServerValue(null)]
        BR,

        [ServerValue("prod.tr.lol.riotgames.com")]
        [LoginQueueValue("https://lq.tr.lol.riotgames.com/")]
        [LocaleValue("pt_BR")]
        [UseGarenaValue(false)]
        [GarenaAuthServerValue(null)]
        TR,

        [ServerValue("prod.ru.lol.riotgames.com")]
        [LoginQueueValue("https://lq.ru.lol.riotgames.com/")]
        [LocaleValue("en_US")]
        [UseGarenaValue(false)]
        [GarenaAuthServerValue(null)]
        RU,

        [ServerValue("prod.la1.lol.riotgames.com")]
        [LoginQueueValue("https://lq.la1.lol.riotgames.com/")]
        [LocaleValue("es_MX")]
        [UseGarenaValue(false)]
        [GarenaAuthServerValue(null)]
        LA1,

        [ServerValue("prod.la2.lol.riotgames.com")]
        [LoginQueueValue("https://lq.la2.lol.riotgames.com/")]
        [LocaleValue("es_MX")]
        [UseGarenaValue(false)]
        [GarenaAuthServerValue(null)]
        LA2,

        [ServerValue("prod.pbe1.lol.riotgames.com")]
        [LoginQueueValue("https://lq.pbe1.lol.riotgames.com/")]
        [LocaleValue("en_US")]
        [UseGarenaValue(false)]
        [GarenaAuthServerValue(null)]
        PBE,

        [ServerValue("prod.lol.garenanow.com")]
        [LoginQueueValue("https://lq.lol.garenanow.com/")]
        [LocaleValue("en_US")]
        [UseGarenaValue(true)]
        [GarenaAuthServerValue("")]
        SG,

        [ServerValue("prod.lol.garenanow.com")]
        [LoginQueueValue("https://lq.lol.garenanow.com/")]
        [LocaleValue("en_US")]
        [UseGarenaValue(true)]
        [GarenaAuthServerValue("")]
        MY,

        [ServerValue("prod.lol.garenanow.com")]
        [LoginQueueValue("https://lq.lol.garenanow.com/")]
        [LocaleValue("en_US")]
        [UseGarenaValue(true)]
        [GarenaAuthServerValue("")]
        SGMY,

        [ServerValue("prodtw.lol.garenanow.com")]
        [LoginQueueValue("https://loginqueuetw.lol.garenanow.com/")]
        [LocaleValue("en_US")]
        [UseGarenaValue(true)]
        [GarenaAuthServerValue("")]
        TW,

        [ServerValue("prodth.lol.garenanow.com")]
        [LoginQueueValue("https://lqth.lol.garenanow.com/")]
        [LocaleValue("en_US")]
        [UseGarenaValue(true)]
        [GarenaAuthServerValue("")]
        TH,

        [ServerValue("prodph.lol.garenanow.com")]
        [LoginQueueValue("https://lqph.lol.garenanow.com/")]
        [LocaleValue("en_US")]
        [UseGarenaValue(true)]
        [GarenaAuthServerValue("lolph.auth.garenanow.com")]
        PH,

        [ServerValue("prodvn.lol.garenanow.com")]
        [LoginQueueValue("https://lqvn.lol.garenanow.com/")]
        [LocaleValue("en_US")]
        [UseGarenaValue(true)]
        [GarenaAuthServerValue("")]
        VN,

        [ServerValue("prod.oc1.lol.riotgames.com")]
        [LoginQueueValue("https://lq.oc1.lol.riotgames.com/")]
        [LocaleValue("en_US")]
        [UseGarenaValue(false)]
        [GarenaAuthServerValue(null)]
        OCE,

        [ServerValue("prod.cs.lol.riotgames.com")]
        [LoginQueueValue("https://lq.cs.lol.riotgames.com/")]
        [LocaleValue("en_US")]
        [UseGarenaValue(false)]
        [GarenaAuthServerValue(null)]
        CS,
    }

    public static class RegionInfo
    {
        public static string GetServerValue(Enum value)
        {
            string output = null;
            Type type = value.GetType();

            FieldInfo fi = type.GetField(value.ToString());
            ServerValue[] attrs =
                fi.GetCustomAttributes(typeof(ServerValue),
                    false) as ServerValue[];
            if (attrs.Length > 0)
            {
                output = attrs[0].Value;
            }
            return output;
        }

        public static string GetLoginQueueValue(Enum value)
        {
            string output = null;
            Type type = value.GetType();

            FieldInfo fi = type.GetField(value.ToString());
            LoginQueueValue[] attrs =
                fi.GetCustomAttributes(typeof(LoginQueueValue),
                    false) as LoginQueueValue[];
            if (attrs.Length > 0)
            {
                output = attrs[0].Value;
            }
            return output;
        }

        public static string GetLocaleValue(Enum value)
        {
            string output = null;
            Type type = value.GetType();

            FieldInfo fi = type.GetField(value.ToString());
            LocaleValue[] attrs =
                fi.GetCustomAttributes(typeof(LocaleValue),
                    false) as LocaleValue[];
            if (attrs.Length > 0)
            {
                output = attrs[0].Value;
            }
            return output;
        }

        public static bool GetUseGarenaValue(Enum value)
        {
            bool output = false;
            Type type = value.GetType();

            FieldInfo fi = type.GetField(value.ToString());
            UseGarenaValue[] attrs =
                fi.GetCustomAttributes(typeof(UseGarenaValue),
                    false) as UseGarenaValue[];
            if (attrs.Length > 0)
            {
                output = attrs[0].Value;
            }
            return output;
        }

        public static string GetGarenaAuthServerValue(Enum value)
        {
            string result = null;
            Type type = value.GetType();
            FieldInfo fi = type.GetField(value.ToString());
            GarenaAuthServerValue[] attrs =
                fi.GetCustomAttributes(typeof(GarenaAuthServerValue),
                    false) as GarenaAuthServerValue[];
            if (attrs.Length > 0)
            {
                result = attrs[0].Server;
            }
            return result;
        }
    }

    public class ServerValue : System.Attribute
    {
        private string _value;

        public ServerValue(string value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }
    }

    public class LoginQueueValue : System.Attribute
    {
        private string _value;

        public LoginQueueValue(string value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }
    }

    public class LocaleValue : System.Attribute
    {
        private string _value;

        public LocaleValue(string value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }
    }

    public class UseGarenaValue : System.Attribute
    {
        private bool _value;

        public UseGarenaValue(bool value)
        {
            _value = value;
        }

        public bool Value
        {
            get { return _value; }
        }
    }

    public class GarenaAuthServerValue : System.Attribute
    {
        private string _server;
        public GarenaAuthServerValue(string server)
        {
            _server = server;
        }
        public string Server
        {
            get { return _server; }
        }
    }
}
