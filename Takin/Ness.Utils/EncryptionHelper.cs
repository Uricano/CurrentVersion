using System.Text.RegularExpressions;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System;

namespace Ness.Utils
{
    public static class EncryptionHelper
    {
        private static string _sharedSecret = "P@ssw0rd!@#$%general";

        public enum encriptionType
        {
            MD5 = 1,
            SHA1 = 2
        }

        public static Tuple<string, string> GetSaltHashData(string password, string passwordSalt = null)
        {
            if (string.IsNullOrEmpty(passwordSalt))
            {
                passwordSalt = System.Web.Security.Membership.GeneratePassword(6, 3);
            }
            string passwordHash = EncryptionHelper.GetHashData(password, passwordSalt, EncryptionHelper.encriptionType.MD5);

            return new Tuple<string, string>(passwordSalt, passwordHash);
        }

        /// <summary>
        /// take any string and encrypt it by encryption key,
        /// using the requested encription type
        /// then return the encrypted data 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="key"></param>
        /// <param name="encType"></param>
        /// <returns></returns>
        public static string GetHashData(string message, string key, encriptionType encType)
        {

            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(key);

            HMACMD5 hmacmd5 = new HMACMD5(keyByte);
            HMACSHA1 hmacsha1 = new HMACSHA1(keyByte);

            byte[] messageBytes = encoding.GetBytes(message);
            byte[] hashmessage;

            switch (encType)
            {
                case encriptionType.MD5:
                    hashmessage = hmacmd5.ComputeHash(messageBytes);
                    break;
                case encriptionType.SHA1:
                    hashmessage = hmacsha1.ComputeHash(messageBytes);
                    break;
                default:
                    hashmessage = hmacmd5.ComputeHash(messageBytes);
                    break;
            }

            return ByteToString(hashmessage);

        }
        /// <summary>
        /// encrypt input text by encryption key with requested encription type
        /// and compare it with the stored encrypted text
        /// </summary>
        /// <param name="inputData"></param>
        /// <param name="storedHashData"></param>
        /// <param name="key"></param>
        /// <param name="encType"></param>
        /// <returns></returns>
        public static bool ValidateHashData(string inputData, string storedHashData, string key, encriptionType encType)
        {
            //hash input text and save it string variable
            string getHashInputData = GetHashData(inputData, key, encType);

            if (string.Compare(getHashInputData, storedHashData) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private static string ByteToString(byte[] buff)
        {
            string sbinary = "";

            for (int i = 0; i < buff.Length; i++)
            {
                sbinary += buff[i].ToString("X2"); // hex format
            }
            return (sbinary);
        }

        public static string EncryptAES(string value, string sharedSecret = null)
        {
            var encription = new AESEncription();
            string key = sharedSecret ?? _sharedSecret;
            return encription.EncryptStringAESEncodeBase64(value, key);
        }

        public static string DecryptAES(string encyptedValue, string sharedSecret = null)
        {
            var encription = new AESEncription();
            string key = sharedSecret ?? _sharedSecret;
            return encription.DecryptStringAESDecodeBase64(encyptedValue, key);
        }

    }
}
