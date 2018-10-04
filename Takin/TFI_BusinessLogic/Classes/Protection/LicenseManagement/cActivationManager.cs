using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net.Mime;
using System.IO;
using System.Reflection;

using LogicNP.CryptoLicensing;

// Used namespaces
using Cherries.TFI_BusinessLogic.General;

namespace Cherries.TFI_BusinessLogic.Protection.LicenseManagement
{
    public static class cActivationManager
    {



        #region Activation methods

        public static void DeactivateLic(String strLicenseFile, String strProjectKey, String strEMail)
        {
            CryptoLicense licence;

            if (File.Exists(strLicenseFile))
            {
                System.Text.ASCIIEncoding ansiEN = new System.Text.ASCIIEncoding();
                StreamReader streamReader = new StreamReader(strLicenseFile, ansiEN);
                string lc = streamReader.ReadToEnd();

                licence = new CryptoLicense(lc, strProjectKey);
                licence.StorageMode = LicenseStorageMode.None;
                streamReader.Close();
                if (licence.Status != LicenseStatus.Valid) return;
            } else return;


            string deactivatedKey = licence.DeactivateLocally();
            //string fName = cProperties.DataFolder + "\\" + Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location) + ".deactivated";
            string fName = strLicenseFile.Replace(".lic", ".deactivated");

            SendMail("Deactivation license", "Deactivation key:\r\n" + deactivatedKey, strEMail);
            using (StreamWriter outfile = new StreamWriter(fName))
            {
                outfile.Write(deactivatedKey);
                outfile.Close();
            }

        }//DeactivateLic

        #endregion Activation methods

        #region User management methods

        public static TFI_CS_Shared.ResultCodes setFinalUserNameOnServer(String strPrevUser, String strNewUsername, String strNewPassword)
        { // Signs up the user + client
            TFI_CS_Shared.ResultCodes enumResults = TFI_CS_Shared.ResultCodes.LoginError;

            string ErrStr = "";
            String strEncryptedPassword = cAesSecurity.getEncryptedMessage(strNewPassword, cProperties.EncryptionPass);

            //enumResults = ClientBiz.cltBiz.setFinalUser(strPrevUser, strNewUsername, strEncryptedPassword, out ErrStr); // Checks if there is an available license
            enumResults = ClientBiz.cltBiz.setFinalUser(strPrevUser, strNewUsername, strNewPassword, out ErrStr); // Checks if there is an available license

            return enumResults;
        }//setFinalUserNameOnServer

        public static TFI_CS_Shared.ResultCodes setUserPasswordOnServer(String strNewPassword, int idUser)
        { // Signs up the user + client
            TFI_CS_Shared.ResultCodes enumResults = TFI_CS_Shared.ResultCodes.LoginError;

            string ErrStr = "";
            String strEncryptedPassword = cAesSecurity.getEncryptedMessage(strNewPassword, cProperties.EncryptionPass);

            //enumResults = ClientBiz.cltBiz.setFinalUser(strPrevUser, strNewUsername, strEncryptedPassword, out ErrStr); // Checks if there is an available license
            enumResults = ClientBiz.cltBiz.setNewPassword(strNewPassword, idUser, out ErrStr); // Checks if there is an available license

            return enumResults;
        }//setUserPasswordOnServer

        public static Boolean isUserAvailableInServer(String strUsername)
        { // Verifies whether the user selected is available
            Boolean isAvailable = false;

            string ErrStr = "";
            TFI_CS_Shared.ResultCodes m_enumResult = ClientBiz.cltBiz.isServerUserAvailable(strUsername, ref isAvailable, out ErrStr); // Checks if there is an available license

            if (m_enumResult != TFI_CS_Shared.ResultCodes.ok)
            { return false; }

            return isAvailable;
        }//isUserExistsInServer

        public static int isUserTemporary(String strUsername)
        { // Verifies whether the user selected is available
            int iTempStatus = 0;

            string ErrStr = "";
            TFI_CS_Shared.ResultCodes m_enumResult = ClientBiz.cltBiz.isServerUserTemporary(strUsername, ref iTempStatus, out ErrStr); // Checks if there is an available license

            if (m_enumResult != TFI_CS_Shared.ResultCodes.ok)
            {  return 0; }

            return iTempStatus;
        }//isUserExistsInServer

        #endregion User management methods

        #region Helper methods

        public static String SendMail(string Subject, string Body, string file, params string[] toEmails)
        { // Sends an e-mail to specified addresses
            String strErrorMsg = "";
            try
            {
                MailMessage mail = new MailMessage();

                mail.From = new MailAddress("Takin.ProductService@gmail.com");
                for (int i = 0; i < toEmails.Length; i++)
                    mail.To.Add(toEmails[i]);
                mail.Subject = Subject;
                mail.Body = Body;
                mail.Attachments.Add(getLicenseCodeAttachment(file)); // Add the file attachment to this e-mail message.

                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                SmtpServer.Port = 587;
                SmtpServer.UseDefaultCredentials = false;
                SmtpServer.Credentials = new System.Net.NetworkCredential("Takin.ProductService", "dtdis646");
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
                strErrorMsg = "";
            } catch (Exception ex) {
                strErrorMsg = ex.Message; // Error while sending e-mail
            }
            return strErrorMsg;
        }//SendMail

        private static Attachment getLicenseCodeAttachment(string file)
        { // Attaches license key file to mail body
            // Create  the file attachment for this e-mail message.
            Attachment data = new Attachment(file, MediaTypeNames.Application.Octet);
            // Add time stamp information for the file.
            ContentDisposition disposition = data.ContentDisposition;
            disposition.CreationDate = System.IO.File.GetCreationTime(file);
            disposition.ModificationDate = System.IO.File.GetLastWriteTime(file);
            disposition.ReadDate = System.IO.File.GetLastAccessTime(file);
            return data;
        }//getLicenseCodeAttachment

        #endregion Helper methods

    }//of class
}
