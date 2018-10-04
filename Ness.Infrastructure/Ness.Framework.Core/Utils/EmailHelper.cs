using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using Framework.Core.Log;
using System.IO.Compression;
using System.Linq;

namespace Ness.Framework.Core.Utils
{
    public static class EmailHelper
    {
        //<configuration>  
        //            <system.net>
        //<smtp from = "eitan.gammer@ness-tech.co.il" deliveryFormat="International" deliveryMethod="Network">
        //    <network defaultCredentials = "false"
        //     host="smtp.gmail.com"
        //     port="587"
        //     enableSsl="true"
        //     userName="nessdev1234@gmail.com"
        //     password="Aa123456!" 
        //     />
        //  </smtp>
        //  </system.net>
        //      </configuration>

        public static void Send(string toEmail, string fromEmail, string bcc, string cc, string subject, string body, IEnumerable<string> attachments = null, bool isBodyHTML = true, bool zipFiles = true)
        {
            MailMessage message = new MailMessage();

            //set the sender address of the mail message
            if (!string.IsNullOrEmpty(fromEmail))
            {
                message.From = new MailAddress(fromEmail);
            }

            //set the recipient address of the mail message
            AddMailAddress(message.To, toEmail);

            //set the blind carbon copy address
            if (!string.IsNullOrEmpty(bcc))
            {
                AddMailAddress(message.Bcc, bcc);
            }

            //set the carbon copy address
            if (!string.IsNullOrEmpty(cc))
            {
                AddMailAddress(message.CC, cc);
            }

            //set the subject of the mail message
            if (string.IsNullOrEmpty(subject))
            {
                message.Subject = "הודעת מערכת";
            }
            else
            {
                message.Subject = subject;
            }

            //set the body of the mail message
            message.Body = body;

            //set the format of the mail message body
            message.IsBodyHtml = isBodyHTML;

            //set the priority
            message.Priority = MailPriority.Normal;

            string zipFile = string.Empty;

            //add any attachments from the filesystem
            if (attachments != null)
            {
                Attachment mailAttachment;

                if (zipFiles)
                {
                    Zip(ref zipFile, attachments);
                    mailAttachment = new Attachment(zipFile);
                    message.Attachments.Add(mailAttachment);
                }
                else
                {
                    foreach (var attachmentPath in attachments)
                    {
                        mailAttachment = new Attachment(attachmentPath);
                        message.Attachments.Add(mailAttachment);
                    }
                }
            }

            try
            {
                //create the SmtpClient instance
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    smtpClient.Send(message);
                    message.Dispose();
                }

                if (!string.IsNullOrEmpty(zipFile))
                {
                    File.Delete(zipFile);
                }

            }
            catch (Exception ex)
            {
                //var log4NetLoggerService = new Log4NetLoggerService();
                //log4NetLoggerService.Error(ex);
                //throw ex;
            }
        }

        public static void SendBinary(string toEmail, string fromEmail,string cc, string subject, string body, IEnumerable<Tuple<string, byte[]>> attachments = null, bool isBodyHTML = true)
        {
            MailMessage message = new MailMessage();

            //set the sender address of the mail message
            if (!string.IsNullOrEmpty(fromEmail))
            {
                message.From = new MailAddress(fromEmail);
            }

            //set the recipient address of the mail message
            AddMailAddress(message.To, toEmail);
            //set the carbon copy address
            if (!string.IsNullOrEmpty(cc))
            {
                AddMailAddress(message.CC, cc);
            }
            //set the subject of the mail message
            if (string.IsNullOrEmpty(subject))
            {
                message.Subject = "הודעת מערכת";
            }
            else
            {
                message.Subject = subject;
            }
            

            //set the body of the mail message
            message.Body = body;

            //set the format of the mail message body
            message.IsBodyHtml = isBodyHTML;

            //set the priority
            message.Priority = MailPriority.Normal;

            string zipFile = string.Empty;

            //add any attachments from the filesystem
            if (attachments != null)
            {
                Attachment mailAttachment;

                
                foreach (var attachment in attachments)
                {
                    MemoryStream file = new MemoryStream(attachment.Item2);
                    mailAttachment = new Attachment(file, attachment.Item1);
                    message.Attachments.Add(mailAttachment);
                }
               
            }

            try
            {
                //create the SmtpClient instance
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    //if (!string.IsNullOrEmpty(password))
                    //{
                    //    smtpClient.EnableSsl = true;
                    //    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    //    if (!string.IsNullOrEmpty(password))
                    //    {
                    //        smtpClient.UseDefaultCredentials = false;
                    //        smtpClient.Credentials = new System.Net.NetworkCredential(fromEmail, password);
                    //    }
                        
                    //}
                    smtpClient.Send(message);
                    message.Dispose();
                }

                if (!string.IsNullOrEmpty(zipFile))
                {
                    File.Delete(zipFile);
                }

            }
            catch (Exception ex)
            {
                //var log4NetLoggerService = new Log4NetLoggerService();
                //log4NetLoggerService.Error(ex);
                //throw ex;
            }
        }

        private static void AddMailAddress(MailAddressCollection mailAddress, string email)
        {
            if (email.Contains(";"))
            {
                var emails = email.Split(';');
                foreach (var address in emails)
                {
                    mailAddress.Add(new MailAddress(address));
                }
                return;
            }

            mailAddress.Add(new MailAddress(email));
        }

        public static void Zip(ref string fileName, IEnumerable<string> files)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                var file = files.FirstOrDefault();
                var zipFolder = Path.GetDirectoryName(file);
                fileName = string.Format("{0}\\{1}_{2}.zip", zipFolder, Guid.NewGuid().ToString("N").Substring(0, 6) ,DateTime.Now.ToString("yyyyMMddHHmmssffff"));                
            }

            // Create and open a new ZIP file
            using (var zip = ZipFile.Open(fileName, ZipArchiveMode.Create))
            {
                foreach (var file in files)
                {
                    // Add the entry for each file
                    zip.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
                }
                
            }
            //// Dispose of the object when we are done
            //zip.Dispose();
        }

    }
}
