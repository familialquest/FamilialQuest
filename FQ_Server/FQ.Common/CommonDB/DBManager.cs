using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.IO;

namespace CommonDB
{
    public static class DBManager
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        private static readonly IConfiguration configuration = new ConfigurationBuilder()
                                                                    .SetBasePath(Directory.GetCurrentDirectory())
                                                                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                                                    .AddEnvironmentVariables()
                                                                    .Build();

        private static string m_connectionString = "";

        static DBManager()
        {
            configuration.GetReloadToken().RegisterChangeCallback(OnJsonConfigChange, null);
        }
        
        private static void OnJsonConfigChange(object state)
        {
            string connString = "";
            connString = FQDBConnectionString();
            if (string.IsNullOrEmpty(connString))
                connString = PGConnection();

            m_connectionString = connString;
        }
        private static string FQDBConnectionString()
        {
            string connString = "";
            if (configuration.GetSection("FQDB_CONNECTION_STRING").Exists())
            {
                connString = configuration.GetSection("FQDB_CONNECTION_STRING").Value;
                if (!string.IsNullOrWhiteSpace(connString) && !string.Equals(m_connectionString, connString))
                {
                    logger.Debug($"connString from configuration env: {connString}");
                }
            }
            return connString;
        }
        private static string FQDBConnectionStringEnv()
        {
            string connString = "";
            connString = Environment.GetEnvironmentVariable("FQDB_CONNECTION_STRING");
            if (!string.IsNullOrWhiteSpace(connString) && !string.Equals(m_connectionString, connString))
            {
                logger.Debug($"connString from Environment: {connString}");
            }
            return connString;
        }
        private static string PGConnection()
        {
            string connString = "";
            connString = configuration.GetConnectionString("PGConnection");
            if (!string.IsNullOrWhiteSpace(connString) && !string.Equals(m_connectionString, connString))
            {
                logger.Debug($"connString from configuration file: {connString}");
            }
            return connString;
        }

        private static string ConnectionString
        {
            get
            {
                // получаем строку подключения один раз при инициализации
                // далее только при обновлении appsettings.json
                // в этом случае переменная окружения уже игнорируется (см. OnJsonConfigChange)
                if (!string.IsNullOrEmpty(m_connectionString))
                    return m_connectionString;

                string connString = "";
                connString = FQDBConnectionStringEnv();

                if (string.IsNullOrEmpty(connString))
                {
                    connString = FQDBConnectionString();
                }

                if (string.IsNullOrEmpty(connString))
                    connString = PGConnection();

                m_connectionString = connString;

                return m_connectionString;
            }
        }

        /// <summary>
        /// Создается новый NpgsqlConnection. Диспозить или использовать using-директиву.
        /// </summary>
        /// <returns></returns>
        public static NpgsqlConnection GetConnection()
        {
            var newDBConnection = new NpgsqlConnection(ConnectionString);
            try
            {
                newDBConnection.Open();
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
                newDBConnection = null;
            }
            return newDBConnection;
        }
    }
}
