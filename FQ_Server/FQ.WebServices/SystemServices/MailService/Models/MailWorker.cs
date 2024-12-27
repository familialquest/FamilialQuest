using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using CommonLib;

namespace MailService.Models
{
    /// <summary>
    /// Класс определяющий работу с отправкой сообщений электронной почты
    /// </summary>
    public static class MailWorker
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        public static void SendMail(string address, string title, string body)
        {
            try
            {
                logger.Trace($"address: {address}.");
                logger.Trace($"title: {title}.");
                logger.Trace($"body: {body}.");

                using (var message = new MailMessage())
                {
                    message.To.Add(new MailAddress(address, $"[{address}]"));
                    message.From = new MailAddress(
                        Settings.Current[Settings.Name.Mail.OutgoingMailbox, "testfqtest@gmail.com"],
                        Settings.Current[Settings.Name.Mail.OutgoingMailboxDisplayName, "Famililal Quest"]);
                    message.Subject = title;
                    message.Body = body;
                    message.IsBodyHtml = true;

                    using (var client = new SmtpClient(Settings.Current[Settings.Name.Mail.SmtpServerAddress, "smtp.gmail.com"]))
                    {
                        client.Port = Settings.Current[Settings.Name.Mail.SmtpServerPort, 587];
                        client.Credentials = new NetworkCredential(
                            Settings.Current[Settings.Name.Mail.OutgoingMailbox, "[TODO]"],
                            Settings.Current[Settings.Name.Mail.OutgoingMailboxPassword, "[TODO]"]);
                        client.EnableSsl = (Settings.Current[Settings.Name.Mail.SmtpServerSsl, false]);
                        client.Timeout = Settings.Current[Settings.Name.Mail.SmtpServerTimeout, 30] * 1000;
                        client.Send(message);
                    }
                }
            }
            finally
            {
                logger.Trace("SendMail leave.");
            }
        }
    }
}
