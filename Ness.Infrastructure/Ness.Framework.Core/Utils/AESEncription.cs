using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core.Utils
{
    public class AESEncription
    {

        private static byte[] DEFAULT_SALT = Encoding.ASCII.GetBytes("n0s4l7!!");

        private static Dictionary<byte, string> _accountTypes = null;

        /// <summary>
        /// Gets a string description for a value on an enumerator making it friendly for
        /// display purposes. This method was derived an MSDN blog found at:
        /// http://blogs.msdn.com/b/abhinaba/archive/2005/10/20/483000.aspx
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        //public string GetDescription(string name, Type t)
        //{
        //    MemberInfo[] memberInfo = t.GetMember(name);
        //    if (memberInfo != null && memberInfo.Length > 0)
        //    {
        //        object[] attrs = memberInfo[0].GetCustomAttributes(
        //            typeof(DescriptionAttribute), false);

        //        if (attrs != null && attrs.Length > 0)
        //            return ((DescriptionAttribute)attrs[0]).Description;
        //    }

        //    return name;
        //}

        /// <summary>
        /// Encrypt the given string using AES. This code has been adapted from a blog post from StackOverflow
        /// which adapted the encryption method from another Rijndael example.
        /// Source: http://stackoverflow.com/questions/202011/encrypt-decrypt-string-in-net
        /// </summary>
        /// <param name="plainText">The text to encrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for encryption.</param>
        /// <remarks>
        /// This call uses a default salt value and should only be used for less-secure encryption.
        /// </remarks>
        private string EncryptStringAES(string plainText, string sharedSecret)
        {
            return EncryptStringAES(plainText, sharedSecret, DEFAULT_SALT);
        }

        public string EncryptStringAESEncodeBase64(string plainText, string sharedSecret)
        {
            var encrypted = EncryptStringAES(plainText, sharedSecret, DEFAULT_SALT);

            byte[] toBytes = Encoding.ASCII.GetBytes(encrypted);

            return Convert.ToBase64String(toBytes);
        }

        /// <summary>
        /// Encrypt the given string using AES. This code has been adapted from a blog post from StackOverflow
        /// which adapted the encryption method from another Rijndael example.
        /// Source: http://stackoverflow.com/questions/202011/encrypt-decrypt-string-in-net
        /// </summary>
        /// <param name="plainText">The text to encrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for encryption.</param>
        /// <param name="salt">The salt to use for encryption.</param>
        private string EncryptStringAES(string plainText, string sharedSecret, byte[] salt)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException("plainText");
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException("sharedSecret");

            string outStr = null;
            RijndaelManaged aesAlg = null;

            try
            {
                // Generate the key from the shared secret and the salt.
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, salt);

                // Create a RijndaelManaged object with the specified key and IV.
                aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

                // Create a decryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        swEncrypt.Write(plainText);

                    outStr = Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            return outStr;
        }

        /// <summary>
        /// Decrypt the given string. This code has been adapted from a blog post from StackOverflow
        /// which adapted the encryption method from another Rijndael example.
        /// Source: http://stackoverflow.com/questions/202011/encrypt-decrypt-string-in-net
        /// </summary>
        /// <param name="cipherText">The text to decrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for decryption.</param>
        /// <remarks>
        /// In order for decryption to succeed the cipherText must have been encrypted using 
        /// the exact same sharedSecret and salt as it was encrypted with. This method uses
        /// a default salt value.
        /// </remarks>
        private string DecryptStringAES(string cipherText, string sharedSecret)
        {
            return DecryptStringAES(cipherText, sharedSecret, DEFAULT_SALT);
        }

        public string DecryptStringAESDecodeBase64(string cipherText, string sharedSecret)
        {
            var array = Convert.FromBase64String(cipherText);

            var res = Encoding.ASCII.GetString(array);

            return DecryptStringAES(res, sharedSecret, DEFAULT_SALT);
        }

        /// <summary>
        /// Decrypt the given string. This code has been adapted from a blog post from StackOverflow
        /// which adapted the encryption method from another Rijndael example.
        /// Source: http://stackoverflow.com/questions/202011/encrypt-decrypt-string-in-net
        /// </summary>
        /// <param name="cipherText">The text to decrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for decryption.</param>
        /// <param name="salt">The salt to use for decryption.</param>
        /// <remarks>
        /// In order for decryption to succeed the cipherText must have been encrypted using 
        /// the exact same sharedSecret and salt as it was encrypted with.
        /// </remarks>
        private string DecryptStringAES(string cipherText, string sharedSecret, byte[] salt)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException("cipherText");
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException("sharedSecret");
            if (salt == null || salt.Length == 0)
                throw new ArgumentNullException("salt");

            RijndaelManaged aesAlg = null;
            string plaintext = null;

            try
            {
                // Generate the key from the shared secret and the salt
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, salt);

                // Create a RijndaelManaged object with the specified key and IV.
                aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.                
                byte[] bytes = Convert.FromBase64String(cipherText);
                using (MemoryStream msDecrypt = new MemoryStream(bytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        plaintext = srDecrypt.ReadToEnd();
                }
            }
            finally
            {
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            return plaintext;
        }

    }
}
