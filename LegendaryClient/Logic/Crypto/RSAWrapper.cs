using System;
using System.Collections.Generic;
using System.IO;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Security;
using System.Text;
using System.Windows.Forms;

namespace LegendaryClient.Logic.Crypto
{
	class ChatEncryptionWrapper
	{
		public static enum ExchangeResult
		{
			Success,
			PartnerHasNoKey,
			FailedUnknown
		}
		public static ExchangeResult exchangeKeys(string otherUser)
		{
			if (ChatEncryptionInfo.keyPair == null)
			{
				return ExchangeResult.FailedUnknown;
			}
			//ToDo: Add key exchanging, maybe via Chat?

		}

		public static void LoadKeys()
		{

			var file = File.OpenRead(Path.Combine(Client.ExecutingDirectory, "Crypto", "priv.key"));
			ChatEncryptionInfo.keyPair.Private = file;
			file.Close();
			file = File.OpenRead(Path.Combine(Client.ExecutingDirectory, "Crypto", "pub.key"));
			ChatEncryptionInfo.keyPair.Public = file;
			file.Close();
		}


		public static void SaveKeys()
		{
			var file = File.OpenWrite(Path.Combine(Client.ExecutingDirectory, "Crypto", "priv.key"));
			var memStream = new MemoryStream(ChatEncryptionInfo.keyPair.Private);
			file.Write(memStream.ToArray(), 0, memStream.Length);
			file.Close();
			memStream.Close();
			var file = File.OpenWrite(Path.Combine(Client.ExecutingDirectory, "Crypto", "pub.key"));
			var memStream = new MemoryStream(ChatEncryptionInfo.keyPair.Public);
			file.Write(memStream.ToArray(), 0, memStream.Length);
			file.Close();
			memStream.Close();
		}

		public static void generateKeys()
		{
			var keyPairGenerator = new RsaKeyPairGenerator();
			keyPairGenerator.Init(new KeyGenerationParameters(new SecureRandom(new CryptoApiRandomGenerator()), 1024));
			ChatEncryptionInfo.keyPair = keyPairGenerator.GenerateKeyPair();
		}
		public static void SaveKeysToFile(string publicKeyPath, string privateKeyPath)
		{
			PrivateKeyInfo pkInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(ChatEncryptionInfo.keyPair.Private);
			String privateKey = Convert.ToBase64String(pkInfo.GetDerEncoded());
			SubjectPublicKeyInfo info = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(ChatEncryptionInfo.keyPair.Public);
			String publicKey = Convert.ToBase64String(info.GetDerEncoded());
			var publicFile = File.OpenWrite(publicKeyPath);
			publicFile.Write(System.Text.Encoding.ASCII.GetBytes(publicKey.ToCharArray()), 0, publicKey.ToCharArray().Length);
			publicFile.Close();
			var privateFile = File.OpenWrite(privateKeyPath);
			privateFile.Write(System.Text.Encoding.ASCII.GetBytes(privateKey.ToCharArray()), 0, privateKey.ToCharArray().Length);
			privateFile.Close();
		}

		public static string DecryptString(string input)
		{
			var bytes = System.Text.Encoding.Unicode.GetBytes(input);
			MemoryStream inStream = new MemoryStream();
			inStream.Write(bytes, 0, bytes.Length);
			inStream = PgpUtilities.GetDecoderStream(inStream);
			PgpObjectFactory pgpFact = new PgpObjectFactory(inStream);
			PgpEncryptedDataList enc = null;
			PgpPublicKeyEncryptedData realenc = null;
			var obj = pgpFact.NextObject();
			if (obj is PgpEncryptedDataList)
			{
				enc = obj as PgpEncryptedDataList;
			}
			else
			{
				enc = pgpFact.NextObject() as PgpEncryptedDataList;
			}
			var enumeration = enc.GetEncryptedDataObjects();
			PgpPrivateKey privKey = ChatEncryptionInfo.keyPair.Private;
			foreach (var x in enumeration)
			{
				enc = x as PgpPublicKeyEncryptedData;
			}
			var clearStream = realenc.GetDataStream(privKey);
			List<byte> correctBytes = new List<byte>();
			byte currByte = null;
			while ((currByte = clearStream.ReadByte()) != -1)
			{
				correctBytes.Add(currByte);
			}
			return System.Text.Encoding.Unicode.GetString(correctBytes.ToArray());
		}

		public static string EncryptString(string message, PgpPublicKey PublicKey)
		{
			var outStream = new MemoryStream();
			PgpLiteralDataGenerator lData = new PgpLiteralDataGenerator();
			var pOut = lData.Open(outStream, PgpLiteralData.Binary, PgpLiteralData.CONSOLE, Encoding.Default.GetBytes(message).Length, DateTime.Now);
			pOut.Write(Encoding.Default.GetBytes(message), 0, Encoding.Default.GetBytes(message).Length);
			lData.Close();
			PgpEncryptedDataGenerator cPK = new PgpEncryptedDataGenerator(Org.BouncyCastle.Bcpg.SymmetricKeyAlgorithmTag.Aes256, false, new SecureRandom());
			cPK.AddMethod(PublicKey);
			var x = cPK.Open(outStream, outStream.ToArray());
			var MemPOut = new MemoryStream();
			pOut.CopyTo(MemPOut);
			x.Write(MemPOut.ToArray(), 0, MemPOut.Length);
			var encryptedBytes = (x as MemoryStream).ToArray();
			return Encoding.Default(encryptedBytes);
		}
		
	}

	class ChatEncryptionInfo
	{
		/// <summary>
		/// Stores all known Keys
		/// </summary>
		public static Dictionary<string, PgpKeyPair> Keys = new Dictionary<string, PgpKeyPair>();
		public static AsymmetricCipherKeyPair keyPair = null;

	}
}
