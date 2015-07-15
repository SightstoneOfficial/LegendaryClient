using agsXMPP.protocol.client;
using LegendaryClient.Logic.Region;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace LegendaryClient.Logic.MultiUser
{
    public static class UserList
    {
        internal static Dictionary<string, UserClient> Users = new Dictionary<string, UserClient>();

        internal static void AddUser(string user, string pass, string internalname, string status, int icon, BaseRegion region, ShowType show, string encrypt)
        {
            if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers")))
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "LCUsers"));
            if (encrypt == string.Empty)
                return;
            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers", "encrypt")))
            {
                var data = File.ReadAllLines(Path.Combine(Client.ExecutingDirectory, "LCUsers", "encrypt"))[0];
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
            tw.WriteLine(EncryptDes(user, encrypt, internalname));
            tw.WriteLine(EncryptDes(pass, encrypt, internalname));
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

        internal static bool VerifyEncrypt(string input)
        {
            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers", "encrypt")))
            {
                var data = File.ReadAllLines(Path.Combine(Client.ExecutingDirectory, "LCUsers", "encrypt"))[0];
                return data == input.ToSHA1();
            }
            if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers")))
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "LCUsers"));
            var x = File.Create(Path.Combine(Client.ExecutingDirectory, "LCUsers", "encrypt"));
            x.Close();
            TextWriter t = new StreamWriter(Path.Combine(Client.ExecutingDirectory, "LCUsers", "encrypt"));
            t.Write(input.ToSHA1());
            t.Close();
            return true;
        }

        internal static List<LoginData> GetAllUsers(string encrypt)
        {
            if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers")))
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "LCUsers"));
            if (encrypt == string.Empty)
                return null;
            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers", "encrypt")))
            {
                var data = File.ReadAllLines(Path.Combine(Client.ExecutingDirectory, "LCUsers", "encrypt"))[0];
                if (data != encrypt.ToSHA1())
                    return null;
            }
            else
                return null;
            var login = new List<LoginData>();
            foreach (var files in Directory.GetFiles(Path.Combine(Client.ExecutingDirectory, "LCUsers")))
            {
                var text = File.ReadAllLines(files);
                if (Path.GetFileName(files) == "encrypt")
                    continue;
                try
                {
                    var lgn = new LoginData()
                    {
                        SumName = Path.GetFileName(files),
                        User = DecryptDes(text[0], encrypt, Path.GetFileName(files)),
                        Pass = DecryptDes(text[1], encrypt, Path.GetFileName(files)),
                        Region = BaseRegion.GetRegion(text[2].DecryptStringAES(encrypt)),
                        Status = text[3],
                        SumIcon = text[4].ToInt(),
                        ShowType = (ShowType)Enum.Parse(typeof(ShowType), text[5])
                    };
                    login.Add(lgn);
                    Client.Log("found account: " + Path.GetFileName(files));
                }
                catch (Exception e) { Client.Log(e); }
            }
            return login;
        }

        /// <summary>
        /// Legacy PVP Login Service
        /// </summary>
        /// <param name="originalString"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string EncryptDes(string originalString, string username, string password)
        {
            var cryptoProvider = new DESCryptoServiceProvider();
            var memoryStream = new MemoryStream();
            var cryptoStream = new CryptoStream(memoryStream,
                cryptoProvider.CreateEncryptor(Encoding.ASCII.GetBytes(username), Encoding.ASCII.GetBytes(password).ToSixteenBytes()), CryptoStreamMode.Write);
            var writer = new StreamWriter(cryptoStream);
            writer.Write(originalString);
            writer.Flush();
            cryptoStream.FlushFinalBlock();
            writer.Flush();
            return Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
        }

        /// <summary>
        /// Legacy PVP Login Service
        /// </summary>
        /// <param name="cryptedString"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string DecryptDes(string cryptedString, string username, string password)
        {
            var cryptoProvider = new DESCryptoServiceProvider();
            var memoryStream = new MemoryStream
                    (Convert.FromBase64String(cryptedString));
            var cryptoStream = new CryptoStream(memoryStream,
                cryptoProvider.CreateDecryptor(Encoding.ASCII.GetBytes(username), Encoding.ASCII.GetBytes(password).ToSixteenBytes()), CryptoStreamMode.Read);
            var reader = new StreamReader(cryptoStream);
            return reader.ReadToEnd();
        }

        public static byte[] ToSixteenBytes(this byte[] inputBytes)
        {
            var sha1 = SHA1.Create();
            var hash = sha1.ComputeHash(inputBytes);
            return hash.Take(16).ToArray();
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
