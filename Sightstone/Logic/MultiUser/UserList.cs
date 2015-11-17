using agsXMPP.protocol.client;
using Sightstone.Logic.Region;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sightstone.Logic.MultiUser
{
    public static class UserList
    {
        internal static Dictionary<string, Dictionary<string, UserClient>> Users = new Dictionary<string, Dictionary<string, UserClient>>();
        private const string Version = "1.0.0.2";

        internal static void AddUser(string user, string pass, string internalname, string status, int icon, BaseRegion region, ShowType show, string encrypt)
        {
            if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers")))
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "LCUsers"));
            if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers", region.InternalName)))
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "LCUsers", region.InternalName));
            if (encrypt == string.Empty)
                encrypt = Client.EncrytKey;
            if (Client.EncrytKey == null || !VerifyEncrypt(encrypt))
            {
                return;
            }
            //TODO: Switch to json or sqlite table
            var filename = Path.Combine(region.InternalName, EncryptDes(region.InternalName + ":" + internalname, encrypt, encrypt).Replace("/", "Rito"));
            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers", "AccountVersion")))
            {
                var version = File.ReadAllLines(Path.Combine(Client.ExecutingDirectory, "LCUsers", "AccountVersion"))[0];
                if (version != Version)
                {
                    Directory.Delete(Path.Combine(Client.ExecutingDirectory, "LCUsers"), true);
                    Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "LCUsers"));
                }
                File.Delete(Path.Combine(Client.ExecutingDirectory, "LCUsers", "AccountVersion"));
            }
            else
            {
                Directory.Delete(Path.Combine(Client.ExecutingDirectory, "LCUsers"), true);
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "LCUsers"));
            }
            if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers")))
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "LCUsers"));
            var stream = File.Create(Path.Combine(Client.ExecutingDirectory, "LCUsers", "AccountVersion"));
            using (TextWriter streamReader = new StreamWriter(stream))
            {
                streamReader.WriteLine(Version);
            }

            if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers", region.InternalName)))
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "LCUsers", region.InternalName));

            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers", filename)))
                File.Delete(Path.Combine(Client.ExecutingDirectory, "LCUsers", filename));
            var x = File.Create(Path.Combine(Client.ExecutingDirectory, "LCUsers", filename));
            using (TextWriter tw = new StreamWriter(x))
            {
                tw.WriteLine(EncryptDes(user, encrypt, internalname));
                tw.WriteLine(EncryptDes(pass, encrypt, internalname));
                tw.WriteLine(region.RegionName);
                tw.WriteLine(status);
                tw.WriteLine(icon);
                tw.WriteLine(show.ToString());
            }
            Client.Log("added user " + region.InternalName + internalname);
        }
        
        internal static void RemoveUser(string internalname)
        {
            if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers")))
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "LCUsers"));
            if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers")))
            {
                return;
            }
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
            if (string.IsNullOrWhiteSpace(input))
                return false; //the pass can not be empty or too eazy
            if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers")))
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "LCUsers"));
            var x = File.Create(Path.Combine(Client.ExecutingDirectory, "LCUsers", "encrypt"));
            using (TextWriter t = new StreamWriter(x))
                t.Write(input.ToSHA1());
            return true;
        }

        internal static List<LoginData> GetAllUsers(string encrypt)
        {
            if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "LCUsers")))
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "LCUsers"));
            if (encrypt == string.Empty)
                encrypt = Client.EncrytKey;
            var login = new List<LoginData>();
            if (!VerifyEncrypt(encrypt)) 
                return login;
            Parallel.ForEach(Directory.GetFiles(Path.Combine(Client.ExecutingDirectory, "LCUsers"), "*", SearchOption.AllDirectories), files =>
                {
                    Client.Log("Found file: " + Path.GetFileName(files));
                    var fileName = Path.GetFileName(files);
                    if (fileName == null || fileName == "AccountVersion" || fileName == "encrypt")
                        return;
                    var text = File.ReadAllLines(files);
                    var region = BaseRegion.GetRegion(text[2]);
                    fileName = fileName.TrimStart(region.InternalName.ToCharArray());
                    fileName = DecryptDes(fileName.Replace("Rito", "/"), encrypt, encrypt).Split(':')[1];
                    var lgn = new LoginData
                    {
                        SumName = fileName,
                        User = DecryptDes(text[0], encrypt, fileName),
                        Pass = DecryptDes(text[1], encrypt, fileName),
                        Region = region,
                        Status = text[3],
                        SumIcon = text[4].ToInt(),
                        ShowType = (ShowType) Enum.Parse(typeof (ShowType), text[5])
                    };
                    login.Add(lgn);
                });
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
                cryptoProvider.CreateEncryptor(Encoding.ASCII.GetBytes(username).ToTwentyBytes().Take(8).ToArray(), Encoding.ASCII.GetBytes(password).ToSixteenBytes()), CryptoStreamMode.Write);
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

            var memoryStream = new MemoryStream(Convert.FromBase64String(cryptedString.Replace(' ', '+')));
            var cryptoStream = new CryptoStream(memoryStream,
                cryptoProvider.CreateDecryptor(Encoding.ASCII.GetBytes(username).ToTwentyBytes().Take(8).ToArray(), Encoding.ASCII.GetBytes(password).ToSixteenBytes()), CryptoStreamMode.Read);
            using (var reader = new StreamReader(cryptoStream))
            {
                var x = reader.ReadToEnd();
                Client.Log(cryptedString + "=" + x);
                cryptoProvider.Dispose();
                memoryStream.Dispose();
                return x;
            }
        }

        public static byte[] ToSixteenBytes(this byte[] inputBytes)
        {
            var md5 = MD5.Create();
            return md5.ComputeHash(inputBytes);
        }

        public static byte[] ToTwentyBytes(this byte[] inputBytes)
        {
            var sha1 = SHA1.Create();
            return sha1.ComputeHash(inputBytes);
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
