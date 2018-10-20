using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using WebPortal.Models;

namespace WebPortal.Controllers
{
    public class Tools
    {
        public static DataTable RunQuery(string query, Dictionary<string, object> parameters)
        {
            var conn = new SqlConnection("Data Source=(local)\\SQLExpress;Initial Catalog=FarmMate;Integrated Security=True");
            System.Data.DataTable dt = new DataTable();
            System.Data.SqlClient.SqlDataAdapter da = new SqlDataAdapter(query, conn);

            foreach (var item in parameters)
                da.SelectCommand.Parameters.AddWithValue(item.Key, item.Value);

            da.Fill(dt);
            return dt;
        }

        public static void RunScalarQuery(string query, Dictionary<string, object> parameters)
        {
            var conn = new SqlConnection("Data Source=(local)\\SQLExpress;Initial Catalog=FarmMate;Integrated Security=True");
            System.Data.DataTable dt = new DataTable();
            System.Data.SqlClient.SqlDataAdapter da = new SqlDataAdapter(query, conn);

            foreach (var item in parameters)
                da.SelectCommand.Parameters.AddWithValue(item.Key, item.Value);

            da.Update(dt);
        }

        public static void SendEmail(TradingEntity tradingEntity, string emailAddress, string subject, string body)
        {
            MailMessage mail = new MailMessage(tradingEntity.SMTPEmailAddress, emailAddress);
            mail.Subject = subject;
            mail.Body = body;

            SmtpClient client = new SmtpClient();
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Port = tradingEntity.SMTPPort;
            client.EnableSsl = tradingEntity.SMTPUseSSL;
            client.Credentials = new System.Net.NetworkCredential(tradingEntity.SMTPEmailAddress, tradingEntity.SMTPPassword);
            client.Host = tradingEntity.SMTPHost;
            client.Send(mail);
        }
    }
}
