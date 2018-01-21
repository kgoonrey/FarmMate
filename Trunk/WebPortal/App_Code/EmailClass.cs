using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace WebPortal.App_Code
{
    public class EmailClass
    {
        public string SendEmail(string test)
        {
            MailMessage mail = new MailMessage("FarmMateNotifications@gmail.com", "toemailaddress@hotmail.com");
            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("FarmMateNotifications@gmail.com", "******");
            client.EnableSsl = true;
            client.Host = "smtp.gmail.com";
            mail.Subject = "this is a test email.";
            mail.Body = "this is my test email body";
            client.Send(mail);

            return "Success";
        }
    }
}
