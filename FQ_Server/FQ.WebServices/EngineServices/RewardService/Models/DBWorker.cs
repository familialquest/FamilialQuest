using CommonDB;
using CommonLib;
using CommonTypes;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using static CommonTypes.Reward;

namespace RewardService.Models
{
    /// <summary>
    /// Класс для работы с запросами к сервису БД
    /// </summary>
    public class DBWorker
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        /// <summary>
        /// Добавление новой награды
        /// </summary>
        /// <param name="inputReward"></param>
        public static void AddReward(Reward inputReward)
        {
            try
            {
                logger.Trace("AddReward started.");

                logger.Trace($"id: {inputReward.id.ToString()}");
                logger.Trace($"groupId: {inputReward.groupId.ToString()}");
                logger.Trace($"title: {inputReward.Title}");
                logger.Trace($"description: {inputReward.Description}");
                logger.Trace($"cost: {inputReward.Cost.ToString()}");
                logger.Trace($"Image: {inputReward.Image}");
                logger.Trace($"creator: {inputReward.creator}");
                logger.Trace($"availableFor: {inputReward.availableFor}");
                logger.Trace($"Status: {inputReward.Status.ToString()}");
                logger.Trace($"creationDate: {inputReward.CreationDate.ToString()}");
                logger.Trace($"purchaseDate: {inputReward.PurchaseDate.ToString()}");
                logger.Trace($"handedDate: {inputReward.HandedDate.ToString()}");

                var query = QueriesInfo.GetQueryTemplate("AddReward");

                logger.Trace($"Query: {query}");

                using(NpgsqlConnection dbConnection = DBManager.GetConnection())
				using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@ID", inputReward.id);
                    npgSqlCommand.Parameters.AddWithValue("@GroupID", inputReward.groupId);
                    npgSqlCommand.Parameters.AddWithValue("@Title", inputReward.Title);
                    npgSqlCommand.Parameters.AddWithValue("@Description", inputReward.Description);
                    npgSqlCommand.Parameters.AddWithValue("@Cost", inputReward.Cost);
                    npgSqlCommand.Parameters.AddWithValue("@Img", inputReward.Image);
                    npgSqlCommand.Parameters.AddWithValue("@Creator", inputReward.creator);
                    npgSqlCommand.Parameters.AddWithValue("@AvailableFor", inputReward.availableFor);
                    npgSqlCommand.Parameters.AddWithValue("@Status", (int)inputReward.Status);
                    npgSqlCommand.Parameters.AddWithValue("@CreationDate", inputReward.CreationDate);
                    npgSqlCommand.Parameters.AddWithValue("@PurchaseDate", inputReward.PurchaseDate);
                    npgSqlCommand.Parameters.AddWithValue("@HandedDate", inputReward.HandedDate);
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
                logger.Trace("AddReward leave.");
            }
        }


        /// <summary>
        /// Обновление награды
        /// </summary>
        /// <param name="inputReward"></param>
        public static void UpdateReward(Reward inputReward)
        {
            try
            {
                logger.Trace("UpdateReward started.");

                logger.Trace($"id: {inputReward.id.ToString()}");
                logger.Trace($"groupId: {inputReward.groupId.ToString()}");
                logger.Trace($"title: {inputReward.Title}");
                logger.Trace($"description: {inputReward.Description}");
                logger.Trace($"cost: {inputReward.Cost.ToString()}");
                logger.Trace($"Image: {inputReward.Image}");               
                logger.Trace($"Status: {inputReward.Status.ToString()}");
                logger.Trace($"creationDate: {inputReward.CreationDate.ToString()}");
                logger.Trace($"purchaseDate: {inputReward.PurchaseDate.ToString()}");
                logger.Trace($"handedDate: {inputReward.HandedDate.ToString()}");

                var query = QueriesInfo.GetQueryTemplate("UpdateReward");

                logger.Trace($"Query: {query}");

                using(NpgsqlConnection dbConnection = DBManager.GetConnection())
				using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@Title", inputReward.Title);
                    npgSqlCommand.Parameters.AddWithValue("@Description", inputReward.Description);
                    npgSqlCommand.Parameters.AddWithValue("@Cost", inputReward.Cost);
                    npgSqlCommand.Parameters.AddWithValue("@Img", inputReward.Image);
                    npgSqlCommand.Parameters.AddWithValue("@Status", (int)inputReward.Status);
                    npgSqlCommand.Parameters.AddWithValue("@CreationDate", inputReward.CreationDate);
                    npgSqlCommand.Parameters.AddWithValue("@PurchaseDate", inputReward.PurchaseDate);
                    npgSqlCommand.Parameters.AddWithValue("@HandedDate", inputReward.HandedDate);
                    npgSqlCommand.Parameters.AddWithValue("@ModificationTime", DateTime.UtcNow);
                    npgSqlCommand.Parameters.AddWithValue("@ID", inputReward.id);
                    npgSqlCommand.Parameters.AddWithValue("@GroupID", inputReward.groupId);

                    var changedRows = npgSqlCommand.ExecuteNonQuery();

                    if (changedRows != 1)
                    {
                        throw new Exception($"changedRows: {changedRows}.");
                    }
                }
            }
            finally
            {
                logger.Trace("UpdateReward leave.");
            }
        }

        /// <summary>
        /// Удаление награды
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="rewardsIds"></param>
        public static void RemoveReward(Guid groupId, Guid rewardId)
        {
            try
            {
                logger.Trace("RemoveReward started.");

                logger.Trace($"rewardId: {rewardId.ToString()}");

                var query = QueriesInfo.GetQueryTemplate("RemoveReward");

                logger.Trace($"Query: {query}");
                
                using(NpgsqlConnection dbConnection = DBManager.GetConnection())
				using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@ID", rewardId);
                    npgSqlCommand.Parameters.AddWithValue("@GroupID", groupId);
                    npgSqlCommand.Parameters.AddWithValue("@RemovedStatus", (int)RewardStatuses.Deleted);
                    npgSqlCommand.Parameters.AddWithValue("@ModificationTime", DateTime.UtcNow);

                    var removedRows = npgSqlCommand.ExecuteNonQuery();

                    if (removedRows != 1)
                    {
                        throw new Exception($"removedRows != 1. removedRows: {removedRows}.");
                    }
                }
            }
            finally
            {
                logger.Trace("RemoveReward leave.");
            }
        }

        /// <summary>
        /// Удаление награды
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="rewardsIds"></param>
        public static List<Guid> RemoveRelatedRewards(Guid groupId, Guid removingUserId)
        {
            try
            {
                logger.Trace("RemoveRelatedReward started.");

                logger.Trace($"removingUserId: {removingUserId.ToString()}");

                List<Guid> relatedRewards = new List<Guid>();

                var query = QueriesInfo.GetQueryTemplate("GetRelatedReward");
                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@RemovingUser", removingUserId);
                    npgSqlCommand.Parameters.AddWithValue("@GroupID", groupId);
                    npgSqlCommand.Parameters.AddWithValue("@RemovedStatus", (int)RewardStatuses.Deleted);

                    using (NpgsqlDataReader DR = npgSqlCommand.ExecuteReader())
                    {
                        while (DR.Read())
                        {
                            Guid relatedRewardId = DR.GetGuid(0);
                           
                            logger.Trace($"relatedRewardId: {relatedRewardId.ToString()}");

                            relatedRewards.Add(relatedRewardId);
                        }
                    }
                }


                query = QueriesInfo.GetQueryTemplate("RemoveRelatedReward");
                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@RemovingUser", removingUserId);
                    npgSqlCommand.Parameters.AddWithValue("@GroupID", groupId);
                    npgSqlCommand.Parameters.AddWithValue("@RemovedStatus", (int)RewardStatuses.Deleted);
                    npgSqlCommand.Parameters.AddWithValue("@ModificationTime", DateTime.UtcNow);

                    var removedRows = npgSqlCommand.ExecuteNonQuery();                    
                }

                return relatedRewards;
            }
            finally
            {
                logger.Trace("RemoveRelatedReward leave.");
            }
        }

        /// <summary>
        /// Получение всех наград
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static List<Reward> GetAllRewards(Guid groupId)
        {
            try
            {
                logger.Trace("GetAllRewards started.");

                logger.Trace($"groupId: {groupId.ToString()}");

                var query = QueriesInfo.GetQueryTemplate("GetAllRewards");
                logger.Trace($"Query: {query}");

                using(NpgsqlConnection dbConnection = DBManager.GetConnection())
				using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@GroupID", groupId);
                    npgSqlCommand.Parameters.AddWithValue("@RemovedStatus", (int)RewardStatuses.Deleted);

                    using (NpgsqlDataReader DR = npgSqlCommand.ExecuteReader())
                    {
                        List<Reward> allRewards = new List<Reward>();

                        while (DR.Read())
                        {
                            Reward selectedReward = new Reward();

                            selectedReward.id = DR.GetGuid(0);
                            selectedReward.groupId = DR.GetGuid(1);
                            selectedReward.Title = DR.GetString(2);
                            selectedReward.Description = DR.GetString(3);
                            selectedReward.Cost = DR.GetInt32(4);
                            selectedReward.Image = DR.GetString(5);
                            selectedReward.creator = DR.GetGuid(6);
                            selectedReward.availableFor = DR.GetGuid(7);
                            selectedReward.Status = (RewardStatuses)DR.GetInt32(8);  
                            selectedReward.CreationDate = DR.GetDateTime(9);
                            selectedReward.PurchaseDate = DR.GetDateTime(10);
                            selectedReward.HandedDate = DR.GetDateTime(11);

                            logger.Trace($"id: {selectedReward.id.ToString()}");
                            logger.Trace($"groupId: {selectedReward.groupId.ToString()}");
                            logger.Trace($"title: {selectedReward.Title}");
                            logger.Trace($"description: {selectedReward.Description}");
                            logger.Trace($"cost: {selectedReward.Cost.ToString()}");
                            logger.Trace($"Image: {selectedReward.Image}");
                            logger.Trace($"creator: {selectedReward.creator}");
                            logger.Trace($"availableFor: {selectedReward.availableFor}");
                            logger.Trace($"Status: {selectedReward.Status.ToString()}");
                            logger.Trace($"creationDate: {selectedReward.CreationDate.ToString()}");
                            logger.Trace($"purchaseDate: {selectedReward.PurchaseDate.ToString()}");
                            logger.Trace($"handedDate: {selectedReward.HandedDate.ToString()}");

                            allRewards.Add(selectedReward);
                        }

                        return allRewards;
                    }
                }
            }
            finally
            {
                logger.Trace("GetAllRewards leave.");
            }
        }

        /// <summary>
        /// Получение указанных наград
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="rewardsId"></param>
        /// <returns></returns>
        public static List<Reward> GetRewardsById(Guid groupId, List<Guid> rewardsId)
        {
            try
            {
                logger.Trace("GetRewardsById started.");

                logger.Trace($"groupId: {groupId.ToString()}");
                logger.Trace($"rewardsId.Count: {rewardsId.Count.ToString()}");

                foreach (var rewardId in rewardsId)
                {
                    logger.Trace($"rewardId: {rewardId.ToString()}");
                }

                var query = QueriesInfo.GetQueryTemplate("GetRewardsById");
                var additionalParams = DBHelper.MakeSQLMultipleParamString("ID", rewardsId.Count, " OR ", false);
                query = string.Format(query, additionalParams);
                logger.Trace($"Query: {query}");

                using(NpgsqlConnection dbConnection = DBManager.GetConnection())
				using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@GroupID", groupId);

                    for (int i = 0; i < rewardsId.Count; i++)
                    {
                        npgSqlCommand.Parameters.AddWithValue(string.Format("ID{0}", i), rewardsId[i]);
                    }

                    using (NpgsqlDataReader DR = npgSqlCommand.ExecuteReader())
                    {
                        List<Reward> selectedRewards = new List<Reward>();

                        if (DR.HasRows)
                        {
                            while (DR.Read())
                            {
                                Reward selectedReward = new Reward();

                                selectedReward.id = DR.GetGuid(0);
                                selectedReward.groupId = DR.GetGuid(1);
                                selectedReward.Title = DR.GetString(2);
                                selectedReward.Description = DR.GetString(3);
                                selectedReward.Cost = DR.GetInt32(4);
                                selectedReward.Image = DR.GetString(5);
                                selectedReward.creator = DR.GetGuid(6);
                                selectedReward.availableFor = DR.GetGuid(7);
                                selectedReward.Status = (RewardStatuses)DR.GetInt32(8);
                                selectedReward.CreationDate = DR.GetDateTime(9);
                                selectedReward.PurchaseDate = DR.GetDateTime(10);
                                selectedReward.HandedDate = DR.GetDateTime(11);

                                logger.Trace($"id: {selectedReward.id.ToString()}");
                                logger.Trace($"groupId: {selectedReward.groupId.ToString()}");
                                logger.Trace($"title: {selectedReward.Title}");
                                logger.Trace($"description: {selectedReward.Description}");
                                logger.Trace($"cost: {selectedReward.Cost.ToString()}");
                                logger.Trace($"Image: {selectedReward.Image}");
                                logger.Trace($"creator: {selectedReward.creator}");
                                logger.Trace($"availableFor: {selectedReward.availableFor}");
                                logger.Trace($"Status: {selectedReward.Status.ToString()}");
                                logger.Trace($"creationDate: {selectedReward.CreationDate.ToString()}");
                                logger.Trace($"purchaseDate: {selectedReward.PurchaseDate.ToString()}");
                                logger.Trace($"handedDate: {selectedReward.HandedDate.ToString()}");

                                selectedRewards.Add(selectedReward);
                            }
                        }

                        return selectedRewards;
                    }
                }
            }
            finally
            {
                logger.Trace("GetRewardsById leave.");
            }
        }

        public static Int64 GetRewardsCount(Guid groupId)
        {
            try
            {
                logger.Trace("GetRewardsCount started.");

                logger.Trace($"groupId: {groupId.ToString()}");

                var query = QueriesInfo.GetQueryTemplate("GetRewardsCount");
                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@GroupID", groupId);
                    npgSqlCommand.Parameters.AddWithValue("@StatusDeleted", (int)RewardStatuses.Deleted);

                    var count = (Int64)npgSqlCommand.ExecuteScalar();

                    return count;
                }
            }
            finally
            {
                logger.Trace("GetRewardsCount leave.");
            }
        }

        public static Int64 GetAvailableRewardsCount(Guid groupId)
        {
            try
            {
                logger.Trace("GetAvailableRewardsCount started.");

                logger.Trace($"groupId: {groupId.ToString()}");

                var query = QueriesInfo.GetQueryTemplate("GetAvailableRewardsCount");
                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@GroupID", groupId);
                    npgSqlCommand.Parameters.AddWithValue("@StatusAvailable", (int)RewardStatuses.Available);

                    var count = (Int64)npgSqlCommand.ExecuteScalar();

                    return count;
                }
            }
            finally
            {
                logger.Trace("GetAvailableRewardsCount leave.");
            }
        }
    }
}
