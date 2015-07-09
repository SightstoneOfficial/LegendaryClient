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

        internal static void AddUser(string user, string pass, string internalname, BaseRegion region, string encrypt)
        {
            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers", "encrypt")))
            {                
                string data = File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "LCUsers", "encrypt"));
                if (data.Split('|')[0] != encrypt.ToSHA1())
                    return;
            }
            else
            {
                File.Create(Path.Combine(Client.ExecutingDirectory, "LCUsers", "encrypt")).Close();
                TextWriter t = new StreamWriter(Path.Combine(Client.ExecutingDirectory, "LCUsers", internalname));
                t.Write(encrypt.ToSHA1() + "|");
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
            tw.Close();
            
        }
        
        internal static void RemoveUser(string internalname)
        {
            if (Directory.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers")))
                if (File.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers", internalname)))
                    File.Delete(Path.Combine(Client.ExecutingDirectory, "LCUsers", internalname));
        }

        internal static bool verifyEncrypt(string input)
        {
            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers", "encrypt")))
            {
                string data = File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "LCUsers", "encrypt"));
                if (data.Split('|')[0] != input.ToSHA1())
                    return false;
                else
                    return true;
            }
            else
            {
                File.Create(Path.Combine(Client.ExecutingDirectory, "LCUsers", "encrypt")).Close();
                TextWriter t = new StreamWriter(Path.Combine(Client.ExecutingDirectory, "LCUsers", "encrypt"));
                t.Write(input.ToSHA1() + "|");
                t.Close();
                return true;
            }
        }

        internal static List<LoginData> GetAllUsers(string encrypt)
        {
            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers", "encrypt")))
            {
                string data = File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "LCUsers", "encrypt"));
                if (data.Split('|')[0] != encrypt.ToSHA1())
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
                    string user = text[0].DecryptStringAES(encrypt);
                    string pass = text[1].DecryptStringAES(encrypt);
                    BaseRegion region = BaseRegion.GetRegion(text[2].DecryptStringAES(encrypt));
                    LoginData lgn = new LoginData()
                    {
                        User = user,
                        Pass = pass,
                        Region = region
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
        public BaseRegion Region { get; set; }
    }
}
