using System;
using System.Collections.Generic;
using System.IO;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Security;

namespace LegendaryClient.Logic.Crypto
{
	class ChatEncryptionWrapper
	{
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
		
	}
	class ChatEncryptionInfo
	{
		public static AsymmetricCipherKeyPair keyPair = null;

	}
}
