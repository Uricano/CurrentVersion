using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web;

namespace Ness.Utils
{
    public class EmailHandler
    {
        private string Smtp;
        private int Port;
        private string UserName;
        private string Password;
        private string From;
        private SmtpClient client;
        public EmailHandler(Boolean isAuthenification)
        {
            // TODO: alex: uncomment after authentication@gocherries.com account fix.

            //if (isAuthenification)
            //{
            //    From = ConfigurationManager.AppSettings["fromAuth"].ToString();
            //    Smtp = ConfigurationManager.AppSettings["smtpserverAuth"].ToString();
            //    Port = int.Parse(ConfigurationManager.AppSettings["smtpportAuth"].ToString());
            //    UserName = ConfigurationManager.AppSettings["usernameAuth"].ToString();
            //    Password = EncryptionHelper.DecryptAES(ConfigurationManager.AppSettings["passwordAuth"].ToString());
            //}
            //else
            //{
                From = ConfigurationManager.AppSettings["fromInfo"].ToString();
                Smtp = ConfigurationManager.AppSettings["smtpserverInfo"].ToString();
                Port = int.Parse(ConfigurationManager.AppSettings["smtpportInfo"].ToString());
                UserName = ConfigurationManager.AppSettings["usernameInfo"].ToString();
                Password = EncryptionHelper.DecryptAES(ConfigurationManager.AppSettings["passwordInfo"].ToString());
            //}
            client = new SmtpClient(Smtp, Port);
            
            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(UserName, Password);
        }

        public void Send(List<string> ToAddress, string Subject, string Body)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(From);
            foreach (var item in ToAddress)
            {
                mail.To.Add(item);
            }
            mail.Subject = Subject;
            mail.IsBodyHtml = true;
            mail.Body = Body;
            client.Send(mail);
        }

        public void SendGreetingsEmail(String name, String email)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(From, "Cherries Team");
            mail.To.Add(email);
            mail.Subject = "Start Investing with Cherries!";
            mail.IsBodyHtml = true;

            AddMailBody(mail, name);

            client.Send(mail);
        }

        private void AddMailBody(MailMessage message, String username)
        {
            StringBuilder bodyBuilder = new StringBuilder();

            LinkedResource logoImg = CreateLinkedImage("~/Content/themes/images/cherries_logo_white.png", "mailLogo");
            LinkedResource headerBgImg = CreateLinkedImage("~/Content/themes/images/email_header_bg.png", "headerBg");
            LinkedResource facebookIcon = CreateLinkedImage("~/Content/themes/images/facebook.png", "facebook");
            LinkedResource linkedinIcon = CreateLinkedImage("~/Content/themes/images/linkedin.png", "linkedin");
            LinkedResource twitterIcon = CreateLinkedImage("~/Content/themes/images/twitter.png", "twitter");
            LinkedResource instagramIcon = CreateLinkedImage("~/Content/themes/images/instagram.png", "instagram");

            bodyBuilder.Append("<html>");
            bodyBuilder.Append("<head>");

            bodyBuilder.Append("<style type=\"text/css\">");
            bodyBuilder.AppendLine(".mail{font-family: Helvetica, Arial, sans-serif; line-height: 28px; font-size: 16px; background-color: #f7f7f7; color: #28364e; direction: ltr;}");
            bodyBuilder.AppendLine(".mail_container{background-color: #fff; max-width: 800px; margin: auto;}");
            bodyBuilder.AppendLine(".mail_header{ text-align: center; overflow: hidden;}");
            bodyBuilder.AppendLine(".mail_header_bg_container{ max-width: 0; max-height: 0; }");
            bodyBuilder.AppendLine(".mail_header_bg{ min-width: 800px; height: 148px; }");
            bodyBuilder.AppendLine(".mail_header_logo{ max-width: 270px; position: relative; margin: 15px auto; }");
            bodyBuilder.AppendLine("@media (max-width: 959px) { .mail_header_logo{ max-width: 150px; } }");
            bodyBuilder.AppendLine(".mail_content{ padding: 0 40px; max-width: 800px; margin: auto; }");
            bodyBuilder.AppendLine(".mail_content p{ margin: 30px 0; color: #28364e; }");
            bodyBuilder.AppendLine(".mail_content a { color: #28364e; }");
            bodyBuilder.AppendLine(".mail_footer { padding-top: 5px; background-color: #e6e6e6; text-align: center; }");
            bodyBuilder.AppendLine(".mail_footer_title { margin-top: 5px; color: #28364e; }");
            bodyBuilder.AppendLine(".mail_footer_links{ margin-top: 5px; font-size: 14px; }");
            bodyBuilder.AppendLine(".mail_footer_link{ display: inline-block; }");
            bodyBuilder.AppendLine("@media (max-width: 599px) { .mail_footer_link{ display: block; } }");
            bodyBuilder.AppendLine(".mail_footer_link a { color: #28364e; text-decoration: none; }");
            bodyBuilder.AppendLine(".mail_footer_link_separator { display: inline-block; padding: 0 10px; }");
            bodyBuilder.AppendLine("@media (max-width: 599px) { .mail_footer_link_separator { display: none; } }");
            bodyBuilder.AppendLine(".mail_footer_social{ padding: 20px 0; }");
            bodyBuilder.AppendLine(".mail_footer_social a{ display: imline-block; margin: 0 16px; }");
            bodyBuilder.AppendLine(".mail_footer_social a img{ width: 20px; }");
            bodyBuilder.Append("</style>");

            bodyBuilder.Append("</head>");

            bodyBuilder.Append("<body>");

            bodyBuilder.Append("<div class=\"mail\" dir=\"ltr\">");
            bodyBuilder.Append("<div class=\"mail_container\">");

            bodyBuilder.Append("<div class=\"mail_header\">");
            bodyBuilder.AppendFormat("<div class=\"mail_header_bg_container\"><img src=cid:{0} class=\"mail_header_bg\" /></div>", headerBgImg.ContentId);
            bodyBuilder.AppendFormat("<img src=cid:{0} class=\"mail_header_logo\" />", logoImg.ContentId);
            bodyBuilder.Append("</div>");

            bodyBuilder.Append("<div class=\"mail_content\">");
            bodyBuilder.AppendFormat("<p>Hi {0},</p>", username);
            bodyBuilder.Append(@"" +
                "<p>Welcome to Cherries - the online optimal stock portfolio builder!</p>" +
                "<p>The address to which this e-mail was sent is your personal username, " +
                "which you will use to access the <a href=\"cherries.gocherries.com\">Cherries platform</a>. " +
                "If you forget your password at any time, you can recover it using " +
                "<a href=\"cherries.gocherries.com/forgetPassword\">this link</a>.</p>" +
                "<p>To get started, visit our <a href=\"https://support.gocherries.com/en/\">Help Center</a> " +
                "and learn how to build, track, and backtest portfolios, test strategies, and define your personal investing style.</p>" +
                "<p>Still have questions? Feel free to <a href=\"https://support.gocherries.com/en/contact\">contact</a> " +
                "us at any time. Our support team is at your service.</p>" +
                "<p>Happy Investing,<br/>The Cherries Team</p>");
            bodyBuilder.Append("</div>");

            bodyBuilder.Append("<div class=\"mail_footer\">");
            bodyBuilder.Append(@"" +
                "<div class=\"mail_footer_links\">" +
                    "<div class=\"mail_footer_link\">Cherries by TFI Ltd</div>" +
                    "<div class=\"mail_footer_link_separator\">&#07;</div>" +
                    "<div class=\"mail_footer_link\">" +
                        "<a href=\"http://www.gocherries.com\">www.gocherries.com</a>" +
                    "</div>" +
                    "<div class=\"mail_footer_link_separator\">&#07;</div>" +
                    "<div class=\"mail_footer_link\">" +
                        "<a href=\"mailto:info@gocherries.com\">info@gocherries.com</a>" +
                    "</div>" +
                "</div>" +
                "<div class=\"mail_footer_social\">" +
                    "<a href=\"https://www.facebook.com/CherriesCo/\">" +
                        "<img src=cid:" + facebookIcon.ContentId + " />" +
                    "</a>" +
                    "<a href=\"https://www.linkedin.com/company/cherriesco/\">" +
                        "<img src=cid:" + linkedinIcon.ContentId + " />" +
                    "</a>" +
                    "<a href=\"https://twitter.com/cherries_co\">" +
                        "<img src=cid:" + twitterIcon.ContentId + " />" +
                    "</a>" +
                    "<a href=\"https://www.instagram.com/cherries_co/\">" +
                        "<img src=cid:" + instagramIcon.ContentId + " />" +
                    "</a>" +
                "</div>");
            bodyBuilder.Append("</div>");
            bodyBuilder.Append("</div>");
            bodyBuilder.Append("</div>");
            bodyBuilder.Append("</body>");
            bodyBuilder.Append("</html>");

            String body = bodyBuilder.ToString();

            AlternateView av = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
            av.LinkedResources.Add(logoImg);
            av.LinkedResources.Add(headerBgImg);
            av.LinkedResources.Add(facebookIcon);
            av.LinkedResources.Add(linkedinIcon);
            av.LinkedResources.Add(twitterIcon);
            av.LinkedResources.Add(instagramIcon);
            
            message.AlternateViews.Add(av);
            message.Body = body;
        }

        private LinkedResource CreateLinkedImage(String imagePath, String imageId)
        {
            LinkedResource linkedImg = new LinkedResource(
                HttpContext.Current.Server.MapPath(imagePath));
            linkedImg.ContentId = imageId;
            linkedImg.ContentType = new ContentType("image/png");
            return linkedImg;
        }
    }
}
