using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLib
{
    /// <summary>
    /// Класс с общими данными
    /// </summary>
    public static class CommonData
    {
        public static int minimalClientVersion = 2;
        public static int confirmAccountPeriod = 5 * 60; //Время на подтверждение почты
        public static int tokenLifeTimePeriod = 5* 24 * 60 * 60; //Время жизни токена 5д
        public static int closeExpiredTasksPeriod = 60;
        public static int resetAllFailedLoginTryingsPeriod = 30;
        public static int manageSubscriptionsDelay = 40;
        //public static TimeSpan requestTimeOut = TimeSpan.FromSeconds(20);
        public static int requestTimeOut = 20;
        public static int eventBatchLimit = 100;
        public static int maxTasks_Total = 10000;
        public static int maxTasks_Extension = 1000;
        public static int maxTasks_NotExtension = 1;
        public static int maxRewards_Total = 10000;
        public static int maxRewards_Extension = 1000;
        public static int maxRewards_NotExtension = 1;
        public static int maxParents_Extension = 2;
        public static int maxParents_NotExtension = 1;
        public static int maxChildrens_Extension = 4;
        public static int maxChildrens_NotExtension = 1;
        public static DateTime dateTime_FQDB_MinValue = CommonTypes.Constants.POSTGRES_DATETIME_MINVALUE; // TODO: или перейти на CommonTypes.Constants.POSTGRES_DATETIME_MINVALUE
        public static string applicationName = "familialquestapp";
        public static string packageName = "com.familialquest";
        public static string groupKeyFilePath = "/app/familialquestapp-2330f5e56889.json";
        public static string subscriptionProductId1M = "familialquest.pa.1";
        public static string subscriptionProductId3M = "familialquest.pa.3";
        public static string subscriptionProductId12M = "familialquest.pa.12";
    }    
}
