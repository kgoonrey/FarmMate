using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebPortal.Models;
using System.Net.Mail;
using System.Data;
using System.Data.SqlClient;

namespace WebPortal.Controllers
{
    [Produces("application/json")]
    public class EmailController : Controller
    {
        [HttpPost]
        [Route("api/Email/SendEmail")]
        public string SendEmail([FromBody]Timesheets timesheet)
        {
            try
            {
                var updateExisting = (timesheet.Id != Guid.Empty);
                if (!updateExisting)
                    timesheet.Id = Guid.NewGuid();

                using (var context = new DataModel())
                {
                    if (updateExisting)
                        context.Update(timesheet);
                    else
                        context.Add(timesheet);

                    context.SaveChanges();
                }

                //var tradingEntity = SqlHelper.RunQuery("select * from TradingEntity where id = @Id", new Dictionary<string, object>() { { "@Id", timesheet.TradingEntity } });

                //MailMessage mail = new MailMessage((string)tradingEntity.Rows[0]["SMTPEmailAddress"], (string)tradingEntity.Rows[0]["PayablesEmailAddress"]);
                //SmtpClient client = new SmtpClient();
                //client.Port = (int)tradingEntity.Rows[0]["SMTPPort"];
                //client.DeliveryMethod = SmtpDeliveryMethod.Network;
                //client.UseDefaultCredentials = false;
                //client.Credentials = new System.Net.NetworkCredential((string)tradingEntity.Rows[0]["SMTPEmailAddress"], (string)tradingEntity.Rows[0]["SMTPPassword"]);
                //client.EnableSsl = (bool)tradingEntity.Rows[0]["SMTPUseSSL"];
                //client.Host = (string)tradingEntity.Rows[0]["SMTPHost"];
                //mail.Subject = "this is a test email.";
                //mail.Body = "Apple bottom jeans";
                //client.Send(mail);

                return "Timesheet Submitted Successfully";
            }
            catch { }

            return "Timesheet failed, Please try again.";
        }
    }
}