using agsXMPP.protocol.client;
using LegendaryClient.Logic.Region;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryClient.Logic.MultiUser
{
    public static class UserList
    {
        internal static Dictionary<string, UserClient> users = new Dictionary<string, UserClient>();

        internal static void AddUser(string user, string pass, string internalname, string status, int icon, BaseRegion region, ShowType show, string encrypt)
        {
            if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers")))
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "LCUsers"));
            if (encrypt == string.Empty)
                return;
            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers", "encrypt")))
            {
                string data = File.ReadAllLines(Path.Combine(Client.ExecutingDirectory, "LCUsers", "encrypt"))[0];
                if (data != encrypt.ToSHA1())
                    return;
            }
            else
            {
                var x = File.Create(Path.Combine(Client.ExecutingDirectory, "LCUsers", "encrypt"));
                x.Close();
                TextWriter t = new StreamWriter(Path.Combine(Client.ExecutingDirectory, "LCUsers", internalname));
                t.Write(encrypt.ToSHA1());
                t.Close();
            }
            if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers")))
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "LCUsers"));

            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers", internalname)))
                File.Delete(Path.Combine(Client.ExecutingDirectory, "LCUsers", internalname));

            File.Create(Path.Combine(Client.ExecutingDirectory, "LCUsers", internalname)).Close();

            TextWriter tw = new StreamWriter(Path.Combine(Client.ExecutingDirectory, "LCUsers", internalname));
            tw.WriteLine(user.EncryptStringAES(encrypt));
            tw.WriteLine(pass.EncryptStringAES(encrypt));
            tw.WriteLine(region.RegionName);
            tw.WriteLine(status);
            tw.WriteLine(icon);
            tw.WriteLine(show.ToString());
            tw.Close();
            Client.Log("added user " + internalname);
        }
        
        internal static void RemoveUser(string internalname)
        {
            if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers")))
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "LCUsers"));
            if (Directory.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers")))
                if (File.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers", internalname)))
                    File.Delete(Path.Combine(Client.ExecutingDirectory, "LCUsers", internalname));
        }

        internal static bool verifyEncrypt(string input)
        {
            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers", "encrypt")))
            {
                string data = File.ReadAllLines(Path.Combine(Client.ExecutingDirectory, "LCUsers", "encrypt"))[0];
                if (data != input.ToSHA1())
                    return false;
                else
                    return true;
            }
            else
            {
                if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers")))
                    Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "LCUsers"));
                var x = File.Create(Path.Combine(Client.ExecutingDirectory, "LCUsers", "encrypt"));
                x.Close();
                TextWriter t = new StreamWriter(Path.Combine(Client.ExecutingDirectory, "LCUsers", "encrypt"));
                t.Write(input.ToSHA1());
                t.Close();
                return true;
            }
        }

        internal static List<LoginData> GetAllUsers(string encrypt)
        {
            if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers")))
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "LCUsers"));
            if (encrypt == string.Empty)
                return null;
            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers", "encrypt")))
            {
                string data = File.ReadAllLines(Path.Combine(Client.ExecutingDirectory, "LCUsers", "encrypt"))[0];
                if (data != encrypt.ToSHA1())
                    return null;
            }
            else
                return null;
            List<LoginData> Login = new List<LoginData>();
            foreach (var files in Directory.GetFiles(Path.Combine(Client.ExecutingDirectory, "LCUsers")))
            {
                string[] text = File.ReadAllLines(files);
                try
                {
                    LoginData lgn = new LoginData()
                    {
                        User = text[0].DecryptStringAES(encrypt),
                        Pass = text[1].DecryptStringAES(encrypt),
                        SumName = Path.GetFileName(files),
                        Region = BaseRegion.GetRegion(text[2].DecryptStringAES(encrypt)),
                        Status = text[3],
                        SumIcon = text[4].ToInt(),
                        ShowType = (ShowType)Enum.Parse(typeof(ShowType), text[5])
                    };
                }
                catch { }
            }
            return Login;
        }
    }
    public class LoginData
    {
        public string User { get; set; }
        public string Pass { get; set; }
        public string SumName { get; set; }
        public int SumIcon { get; set; }
        public string Status { get; set; }
        public BaseRegion Region { get; set; }
        public ShowType ShowType { get; set; }
    }
}
