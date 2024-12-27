using Microsoft.Extensions.Configuration;
using System.IO;

namespace CommonLib
{
    public class Settings
    {
        private static Settings instance;

        public static Settings Current
        {
            get
            {
                if (instance == null)
                    instance = new Settings();

                return instance;
            }
        }

        // 1 - переменные окружения (для контейнеров)
        // 2 - appsettings
        private static readonly IConfiguration configuration = new ConfigurationBuilder()
                                                            .SetBasePath(Directory.GetCurrentDirectory())
                                                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                                            .AddEnvironmentVariables("FQ_")
                                                            .Build();

        public string this[string name, string defaultValue]
        {
            get
            {
                return GetString(name, defaultValue);
            }
        }

        public int this[string name, int defaultValue]
        {
            get
            {
                return GetInt(name, defaultValue);
            }
        }

        public bool this[string name, bool defaultValue]
        {
            get
            {
                return GetBool(name, defaultValue);
            }
        }

        public string GetString(string name, string defaultValue)
        {
            string value = defaultValue;

            if (GetString(name, out value))
                return value;

            return defaultValue;
        }

        public int GetInt(string name, int defaultValue)
        {
            int value = defaultValue;

            if (GetString(name, out string strValue))
            {
                if (int.TryParse(strValue, out value))
                    return value;
            }

            return defaultValue;
        }

        public bool GetBool(string name, bool defaultValue)
        {
            bool value = defaultValue;
            

            if (GetString(name, out string strValue))
            {
                if (bool.TryParse(strValue, out value))
                    return value;
                else if (int.TryParse(strValue, out int intValue))
                    return intValue != 0;
            }

            return defaultValue;
        }

        private bool GetString(string name, out string value)
        {
            value = "";
            string settingName = name.ToLower();
            if (configuration.GetSection(settingName).Exists())
            {
                value = configuration.GetSection(settingName).Value;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static class Name
        {
            public static class Account
            {
                private static string prefix = "account_";

                public static string TempAccountTTL = prefix + "temp_account_ttl";
                public static string TokenTTL = prefix + "token_ttl";

                public static string resetAllFailedLoginTryingsPeriod = prefix + "reset_all_failed_login_tryings_period";

                public static string maxParents_Extension = prefix + "max_parents_extension";
                public static string maxParents_NotExtension = prefix + "max_parents_not_extension";
                public static string maxChildrens_Extension = prefix + "max_childrens_extension";
                public static string maxChildrens_NotExtension = prefix + "max_childrens_not_extension";
            }
            public static class Task
            {
                private static string prefix = "task_";

                public static string TempAccountTTL = prefix + "periodic_close_delay";

                public static string maxTasks_Total = prefix + "max_tasks_total";
                public static string maxTasks_Extension = prefix + "max_tasks_extension";
                public static string maxTasks_NotExtension = prefix + "max_tasks_not_extension";
            }
            public static class Reward
            {
                private static string prefix = "reward_";

                public static string maxRewards_Total = prefix + "max_rewards_total";
                public static string maxRewards_Extension = prefix + "max_rewards_extension";
                public static string maxRewards_NotExtension = prefix + "max_rewards_not_extension";
            }
            public static class Route
            {
                private static string prefix = "route_";

                public static string RequestTimeout = prefix + "request_timeout";
                public static string minimalClientVersion = prefix + "minimal_client_version";
            }
            public static class Mail
            {
                private static string prefix = "mail_";

                public static string OutgoingMailbox = prefix + "outgoing_mailbox";
                public static string OutgoingMailboxPassword = prefix + "outgoing_mailbox_password";
                public static string OutgoingMailboxDisplayName = prefix + "outgoing_mailbox_displayname";
                public static string SmtpServerAddress = prefix + "smtp_server_address";
                public static string SmtpServerPort = prefix + "smtp_server_port";
                public static string SmtpServerTimeout = prefix + "smtp_server_timeout";
                public static string SmtpServerSsl = prefix + "smtp_server_ssl";
                public static string OutgoingSendRate = prefix + "outgoing_send_rate";
            }
            public static class Event
            {
                private static string prefix = "event_";

                public static string BatchLimit = prefix + "batch_limit";
            }
            public static class Group
            {
                private static string prefix = "group_";

                public static string applicationName = prefix + "application_name";
                public static string packageName = prefix + "package_name";
                public static string KeyFilePath = prefix + "key_file_path";
                public static string ManageSubscriptionsDelay = prefix + "manage_subscriptions_delay";
                public static string subscriptionProductId1M = prefix + "subscription_productid_1m";
                public static string subscriptionProductId3M = prefix + "subscription_productid_3m";
                public static string subscriptionProductId12M = prefix + "subscription_productid_12m";
            }
        }
    }
}
