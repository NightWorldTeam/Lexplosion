using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Lexplosion.Logic
{
    static class Cryptography
    {
        public static string Sha256(string value)
        {
            return Sha256(Encoding.UTF8.GetBytes(value));
        }

        public static string Sha256(byte[] value)
        {
            StringBuilder Sb = new StringBuilder();
            using (SHA256 hash = SHA256Managed.Create())
            {
                byte[] result = hash.ComputeHash(value);

                foreach (byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }

        public static string Sha256(Stream value)
        {
            StringBuilder Sb = new StringBuilder();
            using (SHA256 hash = SHA256Managed.Create())
            {
                byte[] result = hash.ComputeHash(value);

                foreach (byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }

        public static string Sha512(Stream value)
        {
            StringBuilder Sb = new StringBuilder();
            using (SHA512 hash = SHA512Managed.Create())
            {
                byte[] result = hash.ComputeHash(value);

                foreach (byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }

        public static string Sha1(Stream value)
        {
            StringBuilder Sb = new StringBuilder();
            using (SHA1CryptoServiceProvider mySha = new SHA1CryptoServiceProvider())
            {
                byte[] result = mySha.ComputeHash(value);

                foreach (byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }

        public static string Md5(Stream value)
        {
            StringBuilder Sb = new StringBuilder();
            using (MD5CryptoServiceProvider hash = new MD5CryptoServiceProvider())
            {
                byte[] result = hash.ComputeHash(value);

                foreach (byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }

        static public byte[] AesDecode(byte[] data, byte[] Key, byte[] IV)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(data, 0, data.Length);
                        cryptoStream.FlushFinalBlock();

                        return memoryStream.ToArray();
                    }
                }
            }
        }

        static public byte[] CryptoDecode(ICryptoTransform decryptor, byte[] data)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                }

                return memoryStream.ToArray();
            }
        }

        static public byte[] AesEncode(string plainText, byte[] Key, byte[] IV)
        {
            return AesEncode(Encoding.UTF8.GetBytes(plainText), Key, IV);
        }

        static public byte[] AesEncode(byte[] data, byte[] Key, byte[] IV)
        {
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(data, 0, data.Length);
                        cryptoStream.FlushFinalBlock();
                        return memoryStream.ToArray();
                    }
                }
            }
        }

        public static void CreateRsaKeys(out RSAParameters privateKey, out string publicKey)
        {
            var provider = new RSACryptoServiceProvider(2048);
            privateKey = provider.ExportParameters(true);
            RSAParameters parameters = provider.ExportParameters(false);
            publicKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(parameters)));
        }

        public static RSAParameters DecodeRsaParams(string data)
        {
            return JsonConvert.DeserializeObject<RSAParameters>(Encoding.UTF8.GetString(Convert.FromBase64String(data)));
        }

        public static byte[] RsaEncode(byte[] data, RSAParameters key)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(key);
                return rsa.Encrypt(data, false);
            }
        }

        public static byte[] RsaDecode(byte[] data, RSAParameters key)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(key);
                return rsa.Decrypt(data, false);
            }
        }

    }
}
