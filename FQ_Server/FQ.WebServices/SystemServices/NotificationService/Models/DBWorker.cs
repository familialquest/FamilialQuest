using CommonDB;
using CommonLib;
using CommonTypes;
using Npgsql;
using System;
using System.Collections.Generic;
using static CommonTypes.HistoryEvent;

namespace NotificationService.Models
{
    public class DBWorker
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        public static bool AddDeviceToUser(NotifiedDevice deviceInfo)
        {
            try
            {
                logger.Trace("AddDeviceToUser started.");

                logger.Trace($"UserId: {deviceInfo.UserId.ToString()}\n" +
                $"DeviceId: {deviceInfo.DeviceId.ToString()}\n" + 
                $"RegToken: {deviceInfo.RegToken}");

                var query = QueriesInfo.GetQueryTemplate("AddDeviceToUser");

                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@UserID", deviceInfo.UserId);
                    npgSqlCommand.Parameters.AddWithValue("@DeviceID", deviceInfo.DeviceId);
                    npgSqlCommand.Parameters.AddWithValue("@RegToken", deviceInfo.RegToken);

                    var insertedRows = npgSqlCommand.ExecuteNonQuery();

                    if (insertedRows == 1)
                    {
                        logger.Debug($"Device '{deviceInfo.DeviceId}' with token '{deviceInfo.RegToken}' was added for user '{deviceInfo.UserId}'");
                    }
                    else
                    {
                        throw new Exception($"Could not add device '{deviceInfo.DeviceId}' for user '{deviceInfo.UserId}'");
                    }

                    return true;
                }
            }
            finally
            {
                logger.Trace("AddDeviceToUser leave.");
            }
        }

        public static void ClearTokensForDeviceAndUser(NotifiedDevice deviceInfo)
        {
            try
            {
                logger.Trace("ClearTokensForDeviceAndUser started.");

                logger.Trace($"UserId: {deviceInfo.UserId.ToString()}\n" +
                             $"DeviceId: {deviceInfo.DeviceId.ToString()}\n");

                var query = QueriesInfo.GetQueryTemplate("ClearTokensForDeviceAndUser");

                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@UserID", deviceInfo.UserId);
                    npgSqlCommand.Parameters.AddWithValue("@DeviceID", deviceInfo.DeviceId);

                    npgSqlCommand.ExecuteNonQuery();                   
                }
            }
            finally
            {
                logger.Trace("ClearTokensForDeviceAndUser leave.");
            }
        }

        public static void ClearTokensForUser(Guid userId)
        {
            try
            {
                logger.Trace("ClearTokensForUser started.");

                logger.Trace($"UserId: {userId}");

                var query = QueriesInfo.GetQueryTemplate("ClearTokensForUser");

                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@UserID", userId);

                    npgSqlCommand.ExecuteNonQuery();
                }
            }
            finally
            {
                logger.Trace("ClearTokensForUser leave.");
            }
        }

        public static bool RemoveDeviceForUser(NotifiedDevice deviceInfo)
        {
            try
            {
                logger.Trace("RemoveDeviceForUser started.");

                logger.Trace($"UserId: {deviceInfo.UserId.ToString()}\n" +
                $"DeviceId: {deviceInfo.DeviceId.ToString()}\n" +
                $"RegToken: {deviceInfo.RegToken}");

                var query = QueriesInfo.GetQueryTemplate("RemoveDeviceFromUser");

                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@UserID", deviceInfo.UserId);
                    npgSqlCommand.Parameters.AddWithValue("@DeviceID", deviceInfo.DeviceId);

                    var deletedRows = npgSqlCommand.ExecuteNonQuery();

                    if (deletedRows == 0)
                    {
                        logger.Debug($"There is no device '{deviceInfo.DeviceId}' registered for user '{deviceInfo.UserId}'");
                    }
                    else if (deletedRows == 1)
                    {
                        logger.Debug($"Device '{deviceInfo.DeviceId}' for user '{deviceInfo.UserId}' was removed");
                    }
                    else
                    {
                        throw new Exception($"Device '{deviceInfo.DeviceId}' was deleted more then 1 time for user '{deviceInfo.UserId}'");
                    }

                    return true;
                }
            }
            finally
            {
                logger.Trace("RemoveDeviceForUser leave.");
            }
        }

        private static NotifiedDevice ReadNotifiedDeviceItem(NpgsqlDataReader reader)
        {
            NotifiedDevice foundNotifiedDevice = new NotifiedDevice();

            foundNotifiedDevice.UserId= (Guid)reader["UserID"];
            foundNotifiedDevice.DeviceId = (string)reader["DeviceID"];
            foundNotifiedDevice.RegToken = (string)reader["RegToken"];

            return foundNotifiedDevice;
        }


        public static List<NotifiedDevice> GetDeviceForUser(Guid userId)
        {
            try
            {
                logger.Trace("GetDeviceForUser started.");

                logger.Trace($"userId: {userId}");

                //Формирование запроса
                var query = QueriesInfo.GetQueryTemplate("GetDeviceForUser");

                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@UserID", userId);

                    //Выполнение запроса
                    using (NpgsqlDataReader DR = npgSqlCommand.ExecuteReader())
                    {
                        List<NotifiedDevice> allDevices = new List<NotifiedDevice>();

                        while (DR.Read())
                        {
                            var device = ReadNotifiedDeviceItem(DR);
                            allDevices.Add(device);
                        }

                        return allDevices;
                    }
                }

            }
            finally
            {
                logger.Trace("GetEvents leave.");
            }
        }

        public static bool ChangeSubForUser(Guid userId, bool isUserSubsribed )
        {
            try
            {
                logger.Trace("ChangeSubForUser started.");

                logger.Trace($"userId: {userId}, isUserSubsribed: {isUserSubsribed}");

                //Формирование запроса
                var query = QueriesInfo.GetQueryTemplate("SetUserSubscription");

                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@UserID", userId);
                    npgSqlCommand.Parameters.AddWithValue("@IsSubscribed", isUserSubsribed);

                    //Выполнение запроса
                    var rowAffected = npgSqlCommand.ExecuteNonQuery();
                    
                    if (rowAffected == 0)
                    {
                        throw new Exception($"Could not set subscription to '{isUserSubsribed}' for user '{userId}'");
                    }
                    else
                    {
                        logger.Debug($"Subscription was successfully set to '{isUserSubsribed}' for user '{userId}'");
                    }
                }
                return isUserSubsribed;
            }
            finally
            {
                logger.Trace("ChangeSubForUser leave.");
            }
        }
    }
}
