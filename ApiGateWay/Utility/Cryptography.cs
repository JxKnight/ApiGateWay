using System;
using NLog;
using System.Security.Cryptography;
using System.Text;
using ApiGateWay.Model;

namespace ApiGateWay.Utility
{
	public class Cryptography : AppSetting
	{
        private string Key = string.Empty, IV = string.Empty;
        public Cryptography(string mode,Keys keys)
        {
            if (mode.Equals("ACCESS") && keys==null)
            {
                keys = new Keys(mode);
            }
            Key = keys.Key1;
            IV = keys.Key2;
        }
        public Logger logger = LogManager.GetCurrentClassLogger();
        public string EncryptString(string plainText)
        {
            try
            {
                byte[] array;

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(Key);
                    aes.IV = Encoding.UTF8.GetBytes(IV);
                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                            {
                                streamWriter.Write(plainText);
                            }

                            array = memoryStream.ToArray();
                        }
                    }
                }
                return Convert.ToBase64String(array);
            }
            catch (Exception ex)
            {
                logger.Error($"Encrypt Failed for this value {plainText} with using key {Key}, Error = {ex.Message} Line = {ex.Source}.");
                return null;
            }
        }
        public string DecryptString(string cipherText)
        {
            try
            {
                byte[] buffer = Convert.FromBase64String(cipherText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(Key);
                    aes.IV = Encoding.UTF8.GetBytes(IV);
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream memoryStream = new MemoryStream(buffer))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                            {
                                return streamReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Decrypt Failed for this value {cipherText} with using key {Key}, Error = {ex}.");
                return null;
            }

        }
        public string ComputeSha256Hash(string rawData)
        {
            try
            {
                // Create a SHA256
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    // ComputeHash - returns byte array
                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                    // Convert byte array to a string
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));
                    }
                    return builder.ToString();
                }
            }
            catch (Exception ex)
            {
                logger.Error($"256Hashing {rawData} Failed. Error = {ex}");
                return null;
            }
        }
    }
}

