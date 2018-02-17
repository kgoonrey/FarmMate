using System.Linq;
using System.Net.Mail;
using System.Data;


namespace WebPortalHelper
{
    public class Email
    {
        public static void EmailTimeSheets(int tradingEntity)
        {
            var tradingEntityRow = new Data.DatabaseTableAdapters.TradingEntityTableAdapter().GetDataById(tradingEntity).FirstOrDefault();
            MailMessage mail = new MailMessage(tradingEntityRow.SMTPEmailAddress, "kyle.b.goonrey@hotmail.com");
            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(tradingEntityRow.SMTPEmailAddress, ":assw0rd");
            client.EnableSsl = true;
            client.Host = "smtp.gmail.com";
            mail.Subject = "this is a test email.";
            mail.Body = "Apple bottom jeans";
            client.Send(mail);
        }

        public static string EmailTest()
        {
            return "apple bottoms";
        }
    }
}
