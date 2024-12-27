using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MailService.Models
{
    public static class MessageTemplates
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        public static Dictionary<string, Dictionary<string, string>> messageTemplates = new Dictionary<string, Dictionary<string, string>>();

        static MessageTemplates()
        {
            loadData();
            CheckChanges();
        }

        /// <summary>
        /// Считывание списка шаблонов сообщений из сопутствующего routes.json
        /// </summary>
        private static void loadData()
        {
            try
            {
                string routesJson = File.ReadAllText("messageTemplates.json");
                messageTemplates = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(routesJson);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }            
        }

        /// <summary>
        /// Таск периодически проверяет триггер readagain.1
        /// </summary>
        /// <returns></returns>
        private static async Task CheckChanges()
        {
            while (true)
            {
                try
                {
                    if (File.Exists("readagain.1"))
                    {
                        loadData();
                        File.Delete("readagain.1");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }

                await Task.Delay(5000);
            }
        }
    }
}
