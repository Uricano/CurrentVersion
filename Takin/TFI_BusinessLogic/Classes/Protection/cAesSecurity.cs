using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Cherries.TFI.BusinessLogic.Protection
{
    public static class cAesSecurity
    {

        #region Data members

        // Encryption variables
        private static String m_strSalt = "Kosher"; // Salt to encrypt with
        private static String m_strHashAlgorithm = "SHA1"; // Can be either SHA1 or MD5
        private static int m_iPasswordIterations = 2; // Number of iterations
        private static String m_strInitialVector = "OFRna73m*aze01xY"; // Needs to be 16 ASCII characters long
        private static int m_iKeySize = 128; // Can be 128, 192, or 256

        #endregion Data members

        #region Static functions

        public static String getEncryptedMessage(String strPlainText, String strPassword)
        { // Encrypts a string
            if (string.IsNullOrEmpty(strPlainText)) return "";

            byte[] bInitialVectorBytes = Encoding.ASCII.GetBytes(m_strInitialVector);
            byte[] bSaltValueBytes = Encoding.ASCII.GetBytes(m_strSalt);
            byte[] bPlainTextBytes = Encoding.UTF8.GetBytes(strPlainText);
            PasswordDeriveBytes pbDerivedPassword = new PasswordDeriveBytes(strPassword, bSaltValueBytes, m_strHashAlgorithm, m_iPasswordIterations);
            byte[] bKeyBytes = pbDerivedPassword.GetBytes(m_iKeySize / 8);

            RijndaelManaged SymmetricKey = new RijndaelManaged();
            SymmetricKey.Mode = CipherMode.CBC;
            byte[] CipherTextBytes = null;
            using (ICryptoTransform Encryptor = SymmetricKey.CreateEncryptor(bKeyBytes, bInitialVectorBytes))
                using (MemoryStream MemStream = new MemoryStream())
                    using (CryptoStream CryptoStream = new CryptoStream(MemStream, Encryptor, CryptoStreamMode.Write))
                    {
                        CryptoStream.Write(bPlainTextBytes, 0, bPlainTextBytes.Length);
                        CryptoStream.FlushFinalBlock();
                        CipherTextBytes = MemStream.ToArray();

                        MemStream.Close();
                        CryptoStream.Close();
                    }
            SymmetricKey.Clear();
            return Convert.ToBase64String(CipherTextBytes);
        }//getEncryptedMessage

        public static String getDecryptedMessage(String strCipherText, String strPassword)
        { // Decrypts a string
            if (string.IsNullOrEmpty(strCipherText)) return "";

            byte[] bInitialVectorBytes = Encoding.ASCII.GetBytes(m_strInitialVector);
            byte[] bSaltValueBytes = Encoding.ASCII.GetBytes(m_strSalt);
            byte[] bCipherTextBytes = Convert.FromBase64String(strCipherText);
            PasswordDeriveBytes pbDerivedPassword = new PasswordDeriveBytes(strPassword, bSaltValueBytes, m_strHashAlgorithm, m_iPasswordIterations);
            byte[] bKeyBytes = pbDerivedPassword.GetBytes(m_iKeySize / 8);

            RijndaelManaged SymmetricKey = new RijndaelManaged();
            SymmetricKey.Mode = CipherMode.CBC;
            byte[] PlainTextBytes = new byte[bCipherTextBytes.Length];
            int ByteCount = 0;
            using (ICryptoTransform Decryptor = SymmetricKey.CreateDecryptor(bKeyBytes, bInitialVectorBytes))
                using (MemoryStream MemStream = new MemoryStream(bCipherTextBytes))
                    using (CryptoStream CryptoStream = new CryptoStream(MemStream, Decryptor, CryptoStreamMode.Read))
                    {
                        ByteCount = CryptoStream.Read(PlainTextBytes, 0, PlainTextBytes.Length);
                        MemStream.Close();
                        CryptoStream.Close();
                    }
            SymmetricKey.Clear();
            return Encoding.UTF8.GetString(PlainTextBytes, 0, ByteCount);
        }//getDecryptedMessage

        #endregion Static functions

    }//of class
}
