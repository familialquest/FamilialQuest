using CommonDB;
using CommonLib;
using CommonTypes;
using Google.Apis.AndroidPublisher.v3.Data;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using static CommonTypes.Group;
using static CommonTypes.Subscription;

namespace GroupService.Models
{
    /// <summary>
    /// Класс для работы с запросами к сервису БД
    /// </summary>
    public class DBWorker
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        /// <summary>
        /// Создание новой группы
        /// </summary>
        /// <param name="inputGroup"></param>
        public static void CreateGroup(Group inputGroup)
        {
            try
            {
                logger.Trace("CreateGroup started.");

                logger.Trace($"id: {inputGroup.Id.ToString()}");
                logger.Trace($"name: {inputGroup.Name}");
                logger.Trace($"Image: {inputGroup.Image}");

                var query = QueriesInfo.GetQueryTemplate("CreateGroup");

                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@ID", inputGroup.Id);
                    npgSqlCommand.Parameters.AddWithValue("@Name", inputGroup.Name);
                    npgSqlCommand.Parameters.AddWithValue("@Img", inputGroup.Image);

                    var insertedRows = npgSqlCommand.ExecuteNonQuery();

                    if (insertedRows != 1)
                    {
                        throw new Exception($"insertedRows: {insertedRows}.");
                    }
                }
            }
            finally
            {
                logger.Trace("CreateGroup leave.");
            }
        }

        /// <summary>
        /// Обновление группы
        /// </summary>
        /// <param name="inputGroup"></param>
        public static void UpdateGroup(Group inputGroup)
        {
            try
            {
                logger.Trace("UpdateGroup started.");

                logger.Trace($"id: {inputGroup.Id.ToString()}");
                logger.Trace($"name: {inputGroup.Name}");
                logger.Trace($"Image: {inputGroup.Image}");

                var query = QueriesInfo.GetQueryTemplate("UpdateGroup");

                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@ID", inputGroup.Id);
                    npgSqlCommand.Parameters.AddWithValue("@Name", inputGroup.Name);
                    npgSqlCommand.Parameters.AddWithValue("@Img", inputGroup.Image);

                    var updatedRows = npgSqlCommand.ExecuteNonQuery();

                    if (updatedRows != 1)
                    {
                        throw new Exception($"insertedRows: {updatedRows}.");
                    }
                }
            }
            finally
            {
                logger.Trace("UpdateGroup leave.");
            }
        }

        /// <summary>
        /// Удаление группы
        /// </summary>
        /// <param name="groupId"></param>
        public static void RemoveGroup(Guid groupId)
        {
            try
            {
                logger.Trace("RemoveGroup started.");
                logger.Trace($"groupId: {groupId.ToString()}");

                var query = QueriesInfo.GetQueryTemplate("RemoveGroup");

                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@ID", groupId);
                    
                    var removedRows = npgSqlCommand.ExecuteNonQuery();

                    if (removedRows != 1)
                    {
                        throw new Exception($"removedRows: {removedRows}.");
                    }
                }
            }
            finally
            {
                logger.Trace("RemoveGroup leave.");
            }
        }

        /// <summary>
        /// Получение инфы о группе
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static Group GetGroup(Guid groupId)
        {
            try
            {
                logger.Trace("GetGroup started.");
                logger.Trace($"groupId: {groupId.ToString()}");

                var query = QueriesInfo.GetQueryTemplate("GetGroup");

                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@ID", groupId);

                    using (NpgsqlDataReader DR = npgSqlCommand.ExecuteReader())
                    {
                        if (!DR.HasRows)
                        {
                            throw new Exception("HasRows: false.");
                        }

                        Group selectedGroup = new Group(true);

                        while (DR.Read())
                        {
                            selectedGroup.Id = DR.GetGuid(0);
                            selectedGroup.Name = DR.GetString(1);
                            selectedGroup.Image = DR.GetString(2);

                            logger.Trace($"name: {selectedGroup.Name}");
                            logger.Trace($"Image: {selectedGroup.Image}");
                        }

                        return selectedGroup;
                    }
                }
            }
            finally
            {
                logger.Trace("GetGroup leave.");
            }
        }

        /// <summary>
        /// Сохранение в БД информации о подписке
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="purchaseToken"></param>
        /// <param name="orderID"></param>
        /// <param name="months"></param>
        public static void SaveSubscription(Guid groupId, string purchaseToken, string orderID, int months)
        {
            try
            {
                logger.Trace("SaveSubscription started.");

                logger.Trace($"groupId: {groupId.ToString()}");
                logger.Trace($"purchaseToken: {purchaseToken}");
                logger.Trace($"orderID: {orderID}");
                logger.Trace($"months: {months}");

                var query = QueriesInfo.GetQueryTemplate("SaveSubscription");

                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@PurchaseToken", purchaseToken);
                    npgSqlCommand.Parameters.AddWithValue("@GroupID", groupId);
                    npgSqlCommand.Parameters.AddWithValue("@OrderID", orderID);
                    npgSqlCommand.Parameters.AddWithValue("@Months", months);
                    npgSqlCommand.Parameters.AddWithValue("@InnerState", (int)InnerState.Purchased);
                    npgSqlCommand.Parameters.AddWithValue("@VoidedSource", 0);
                    npgSqlCommand.Parameters.AddWithValue("@VoidedReason", 0);
                    npgSqlCommand.Parameters.AddWithValue("@ModificationTime", DateTime.UtcNow);

                    var insertedRows = npgSqlCommand.ExecuteNonQuery();

                    if (insertedRows != 1)
                    {
                        throw new Exception($"insertedRows: {insertedRows}.");
                    }
                }
            }
            finally
            {
                logger.Trace("SaveSubscription leave.");
            }
        }

        /// <summary>
        /// Получение списка всех актуальных (не истекших и не отозванных) подписок в статусах Purchased и Acivated
        /// </summary>
        /// <returns></returns>
        public static List<Subscription> GetAllActualSubscriptions()
        {
            try
            {
                logger.Trace("GetAllActualSubscriptions started.");

                List<Subscription> actualSubscriptions = new List<Subscription>();

                var query = QueriesInfo.GetQueryTemplate("GetAllActualSubscriptions");

                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@InnerStatePurchased", (int)InnerState.Purchased);
                    npgSqlCommand.Parameters.AddWithValue("@InnerStateAcivated", (int)InnerState.Acivated);

                    using (NpgsqlDataReader DR = npgSqlCommand.ExecuteReader())
                    {
                        while (DR.Read())
                        {
                            Subscription s = new Subscription(true);

                            s.PurchaseToken = DR.GetString(0);
                            s.Months = DR.GetInt32(1);
                            s.GroupId = DR.GetGuid(2);
                            s.State = (InnerState)DR.GetInt32(3);
                            s.ModificationTime = DR.GetDateTime(4);

                            actualSubscriptions.Add(s);
                        }

                        return actualSubscriptions;
                    }
                }
            }
            finally
            {
                logger.Trace("GetAllActualSubscriptions leave.");
            }
        }

        /// <summary>
        /// Получение списка всех актуальных (не истекших и не отозванных) подписок в статусах Purchased и Acivated 
        /// для указанной группы
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static List<Subscription> GetActualSubscriptionsForGroup(Guid groupId)
        {
            try
            {
                logger.Trace("GetActualSubscriptionsForGroup started.");

                List<Subscription> actualSubscriptions = new List<Subscription>();

                var query = QueriesInfo.GetQueryTemplate("GetActualSubscriptionsForGroup");

                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@InnerStatePurchased", (int)InnerState.Purchased);
                    npgSqlCommand.Parameters.AddWithValue("@InnerStateAcivated", (int)InnerState.Acivated);
                    npgSqlCommand.Parameters.AddWithValue("@GroupID", groupId);

                    using (NpgsqlDataReader DR = npgSqlCommand.ExecuteReader())
                    {
                        while (DR.Read())
                        {
                            Subscription s = new Subscription(true);

                            s.PurchaseToken = DR.GetString(0);
                            s.Months = DR.GetInt32(1);
                            s.GroupId = DR.GetGuid(2);
                            s.State = (InnerState)DR.GetInt32(3);
                            s.ModificationTime = DR.GetDateTime(4);

                            actualSubscriptions.Add(s);
                        }

                        return actualSubscriptions;
                    }
                }
            }
            finally
            {
                logger.Trace("GetActualSubscriptionsForGroup leave.");
            }
        }

        public static Int64 GetThisSubscriptionCountForGroup(Guid groupId, string purchaseToken)
        {
            try
            {
                logger.Trace("GetThisSubscriptionCountForGroup started.");

                logger.Trace($"groupId: {groupId.ToString()}");
                logger.Trace($"purchaseToken: {purchaseToken}");

                var query = QueriesInfo.GetQueryTemplate("GetThisSubscriptionCountForGroup");
                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@GroupID", groupId);
                    npgSqlCommand.Parameters.AddWithValue("@PurchaseToken", purchaseToken);

                    var count = (Int64)npgSqlCommand.ExecuteScalar();

                    return count;
                }
            }
            finally
            {
                logger.Trace("GetThisSubscriptionCountForGroup leave.");
            }
        }

        /// <summary>
        /// Обновление статуса указанной подписки
        /// </summary>
        /// <param name="purchaseToken"></param>
        /// <param name="state"></param>
        public static void UpdateSubscriptionState(string purchaseToken, InnerState state)
        {
            try
            {
                logger.Trace("UpdateSubscriptionState started.");
                logger.Trace($"purchaseToken: {purchaseToken}");
                logger.Trace($"state: {state.ToString()}");

                var query = QueriesInfo.GetQueryTemplate("UpdateSubscriptionState");

                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@InnerState", (int)state);
                    npgSqlCommand.Parameters.AddWithValue("@ModificationTime", DateTime.UtcNow);
                    npgSqlCommand.Parameters.AddWithValue("@PurchaseToken", purchaseToken);

                    var updatedRows = npgSqlCommand.ExecuteNonQuery();

                    if (updatedRows != 1)
                    {
                        throw new Exception($"updatedRows: {updatedRows}.");
                    }
                }
            }
            finally
            {
                logger.Trace("UpdateSubscriptionState leave.");
            }
        }
    }
}
