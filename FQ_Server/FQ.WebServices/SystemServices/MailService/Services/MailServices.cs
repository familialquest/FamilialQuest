using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using CommonLib;
using CommonTypes;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using MailService.Models;
using static CommonLib.FQServiceException;
using System.Threading;

namespace MailService.Services
{
    /// <summary>
    /// Сервис управления проектами
    /// </summary>
    public class MailServices : IMailServices
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        private static int _maxSendRatePerMin = CommonLib.Settings.Current[CommonLib.Settings.Name.Mail.OutgoingSendRate, 100];
        private int _messageCounter = _maxSendRatePerMin;        

        System.Timers.Timer _timerEveryMinuteResetMessageCounter = new System.Timers.Timer()
        {
            AutoReset = true,
            Enabled = true,
            Interval = TimeSpan.FromMinutes(1).TotalMilliseconds
        };

        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Default constructor with HTTPContext
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        public MailServices(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            _timerEveryMinuteResetMessageCounter.Elapsed += _timerResetEveryMinuteMessageCounter_Elapsed;
        }

        private void _timerResetEveryMinuteMessageCounter_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Interlocked.Exchange(ref _messageCounter, _maxSendRatePerMin); // обнуляем счетчик раз в минуту
        }

        public void SendMessage(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("SendMessage started.");

                if (Interlocked.CompareExchange(ref _messageCounter, 0, 0) == 0)
                {
                    logger.Warn($"MailService achieved limit (_maxSendRatePerMin = {_maxSendRatePerMin}.");
                    return;
                }

                Mail sendingMail = JsonConvert.DeserializeObject<Mail>(ri.RequestData.postData.ToString());

                if (String.IsNullOrEmpty(sendingMail.Address) ||
                    String.IsNullOrEmpty(sendingMail.MessageType) ||
                    String.IsNullOrEmpty(sendingMail.ConfirmCode))
                {
                    throw new Exception("Не заполнены обязательные поля.");
                }

                if (MessageTemplates.messageTemplates.TryGetValue(sendingMail.MessageType, out Dictionary<string, string> messageTemplate))
                {
                    Interlocked.Decrement(ref _messageCounter); // считаем любую попытку отправить сообщение

                    string messageTitle = string.Format(messageTemplate["title"], sendingMail.ConfirmCode.ToUpper());
                    string messageBody = string.Format(messageTemplate["body"], sendingMail.ConfirmCode.ToUpper(), sendingMail.Address);
                    MailWorker.SendMail(sendingMail.Address, messageTitle, messageBody);
                }
                else
                {
                    throw new Exception("Запрашиваемый шаблон сообщения отсутствует.");
                }
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                logger.Trace("SendMessage leave.");
            }
        }
    }
}
