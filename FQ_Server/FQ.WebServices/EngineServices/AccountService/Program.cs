using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AccountService.Models;
using CommonLib;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace AccountService
{
    public class Program
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            PeriodicTasks();
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                })
                .UseNLog();  // NLog: setup NLog for Dependency injection;

        /// <summary>
        /// Таск периодических операций
        /// </summary>
        /// <returns></returns>
        private static async Task PeriodicTasks()
        {
            while (true)
            {
                try
                {
                    DBWorker.ResetAllFailedLoginTryings();

                    //TODO: переделать на один запрос с передачей всего списка, либо выдать права на доступ к таблице сервису
                    var usersWithOldTokens = DBWorker.GetUsersWithOldTokens();
                    foreach (var u in usersWithOldTokens)
                    {
                        //RemoveRelatedPushSubscriptions
                        FQRequestInfo ri_RemoveRelatedPushSubscriptions = new FQRequestInfo(true);
                        ri_RemoveRelatedPushSubscriptions.RequestData.actionName = "UnregisterDeviceInner";

                        CommonTypes.NotifiedDevice notifiedDevice = new CommonTypes.NotifiedDevice()
                        {
                            UserId = u,
                            DeviceId = string.Empty,
                            RegToken = string.Empty
                        };

                        ri_RemoveRelatedPushSubscriptions.RequestData.postData = notifiedDevice.Serialize();
                        CommonRoutes.RouteInfo.RouteToService(ri_RemoveRelatedPushSubscriptions);
                    }

                    DBWorker.RemoveOldTokens();
                    DBWorker.RemoveOldTempAccounts();                    
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }

                await Task.Delay(1000 * Settings.Current[Settings.Name.Account.resetAllFailedLoginTryingsPeriod, CommonData.resetAllFailedLoginTryingsPeriod]);
            }
        }
    }
}
