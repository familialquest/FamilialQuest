using CommonDB;
using CommonLib;
using CommonTypes;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CommonTypes.HistoryEvent;

namespace EventService.Models
{
    public class DBWorker
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        public static void CreateHistoryEvent(HistoryEvent historyEvent)
        {
            try
            {
                logger.Trace("CreateHistoryEvent started.");

                logger.Trace($"Id: {historyEvent.Id.ToString()}");
                logger.Trace($"GroupId: {historyEvent.GroupId.ToString()}");
                logger.Trace($"ItemType: {historyEvent.ItemType.ToString()}");
                logger.Trace($"MessageType: {historyEvent.MessageType.ToString()}");
                logger.Trace($"Visability: {historyEvent.Visability.ToString()}");
                logger.Trace($"TargetItem: {historyEvent.TargetItem.ToString()}");
                logger.Trace($"AvailableFor: {historyEvent.AvailableFor.Count.ToString()}");
                logger.Trace($"Doer: {historyEvent.Doer.ToString()}");

                var query = QueriesInfo.GetQueryTemplate("CreateHistoryEvent");

                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@ID", historyEvent.Id);
                    npgSqlCommand.Parameters.AddWithValue("@GroupID", historyEvent.GroupId);
                    npgSqlCommand.Parameters.AddWithValue("@CreationDate", DateTime.UtcNow);
                    npgSqlCommand.Parameters.AddWithValue("@ItemType", (Int32)historyEvent.ItemType);
                    npgSqlCommand.Parameters.AddWithValue("@MessageType", (Int32)historyEvent.MessageType);
                    npgSqlCommand.Parameters.AddWithValue("@Visability", (Int32)historyEvent.Visability);
                    npgSqlCommand.Parameters.AddWithValue("@AvailableFor", historyEvent.AvailableFor.ToArray());
                    npgSqlCommand.Parameters.AddWithValue("@TargetItem", historyEvent.TargetItem);
                    npgSqlCommand.Parameters.AddWithValue("@Doer", historyEvent.Doer);

                    var insertedRows = npgSqlCommand.ExecuteNonQuery();

                    if (insertedRows != 1)
                    {
                        throw new Exception($"insertedRows: {insertedRows}.");
                    }
                }
            }
            finally
            {
                logger.Trace("CreateHistoryEvent leave.");
            }
        }

        public static List<HistoryEvent> GetEvents(FQRequestInfo ri, HistoryEvent conditionHistoryEvent = null, int count = 0, DateTime? toDate = null)
        {
            try
            {
                logger.Trace("GetEvents started.");

                logger.Trace($"Role: {ri._User.Role.ToString()}");

                if (conditionHistoryEvent != null)
                {
                    logger.Trace($"Id: {conditionHistoryEvent.Id.ToString()}");
                    logger.Trace($"GroupId: {conditionHistoryEvent.GroupId.ToString()}");
                    logger.Trace($"CreationDate: {conditionHistoryEvent.CreationDate.ToString()}");
                    logger.Trace($"ItemType: {conditionHistoryEvent.ItemType.ToString()}");
                    logger.Trace($"Visability: {conditionHistoryEvent.Visability.ToString()}");
                    logger.Trace($"MessageType: {conditionHistoryEvent.MessageType.ToString()}");                    
                    logger.Trace($"TargetItem: {conditionHistoryEvent.TargetItem.ToString()}");
                    logger.Trace($"Doer: {conditionHistoryEvent.Doer.ToString()}");
                }

                logger.Trace($"ToDate: {toDate.ToString()}");
                logger.Trace($"Count: {count.ToString()}");

                //Формирование запроса
                var query = string.Empty;

                if (ri._User.Role == User.RoleTypes.Parent)
                {
                    query = QueriesInfo.GetQueryTemplate("GetHistoryEvents_Parent");
                }
                else
                {
                    if (ri._User.Role == User.RoleTypes.Children)
                    {
                        query = QueriesInfo.GetQueryTemplate("GetHistoryEvents_Children");
                    }
                    else
                    {
                        throw new Exception("Ошибка: неизвестная роль пользователя.");
                    }
                }                               

                if (conditionHistoryEvent != null)
                {
                    //Если указан Id - другие условия игнорируем и пытаемся получить конкретную запись.
                    //В противном случае - формируем условие.
                    if (conditionHistoryEvent.Id == Guid.Empty)
                    {
                        if (conditionHistoryEvent.ItemType != HistoryEvent.ItemTypeEnum.Default)
                        {
                            query += " AND ItemType = @ItemType";                                
                        }

                        //TODO: ДО ЗАПИЛА СТАТИСТИКИ - ИГНОРИРУЕМ.
                        //conditionHistoryEvent.MessageType
                        //conditionHistoryEvent.Visability
                        //conditionHistoryEvent.TargetItem
                        //conditionHistoryEvent.Doer

                        if (conditionHistoryEvent.CreationDate != CommonData.dateTime_FQDB_MinValue)
                        {
                            query += " AND CreationDate >= @CreationDate";
                        }
                    }
                    else
                    {
                        query += " AND ID = @ID";
                    }
                }

                if (toDate != null)
                {
                    query += " AND CreationDate <= @ToDate";
                }

                //Всегда нужно в обратном порядке по дате регистрации события (от новых к старым)
                query += " ORDER BY CreationDate DESC";

                if (count != 0)
                {
                    query += " LIMIT @ItemsCount";
                }

                query += ";";

                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {                    
                    npgSqlCommand.Parameters.AddWithValue("@GroupID", ri._User.GroupId);

                    //Если запрос в контексте ребенка - покажем, только то, что полагается
                    if (ri._User.Role == User.RoleTypes.Children)
                    {
                        npgSqlCommand.Parameters.AddWithValue("@VisabilityGroup", (int)VisabilityEnum.Group);
                        npgSqlCommand.Parameters.AddWithValue("@VisabilityChildren", (int)VisabilityEnum.Children);
                        npgSqlCommand.Parameters.AddWithValue("@AvailableFor", ri._User.Id);
                    }

                    //Установка параметров в сформированный запрос по тем же условиям, что и выше
                    if (conditionHistoryEvent != null)
                    {                       
                        if (conditionHistoryEvent.Id == Guid.Empty)
                        {
                            if (conditionHistoryEvent.ItemType != HistoryEvent.ItemTypeEnum.Default)
                            {
                                npgSqlCommand.Parameters.AddWithValue("@ItemType", (Int32)conditionHistoryEvent.ItemType);
                            }

                            if (conditionHistoryEvent.CreationDate != CommonData.dateTime_FQDB_MinValue)
                            {
                                npgSqlCommand.Parameters.AddWithValue("@CreationDate", conditionHistoryEvent.CreationDate);
                            }
                        }
                        else
                        {
                            npgSqlCommand.Parameters.AddWithValue("@ID", conditionHistoryEvent.Id);
                        }
                    }

                    if (toDate != null)
                    {
                        npgSqlCommand.Parameters.AddWithValue("@ToDate", toDate);
                    }

                    if (count != 0)
                    {
                        npgSqlCommand.Parameters.AddWithValue("@ItemsCount", count);
                    }

                    //Выполнение запроса
                    using (NpgsqlDataReader DR = npgSqlCommand.ExecuteReader())
                    {
                        List<HistoryEvent> allEvents = new List<HistoryEvent>();

                        while (DR.Read())
                        {
                            HistoryEvent selectedEvent = new HistoryEvent(true);

                            selectedEvent.Id = DR.GetGuid(0);
                            selectedEvent.GroupId = DR.GetGuid(1);
                            selectedEvent.CreationDate = DR.GetDateTime(2);
                            selectedEvent.ItemType = (ItemTypeEnum)DR.GetInt32(3);
                            selectedEvent.MessageType = (MessageTypeEnum)DR.GetInt32(4);
                            selectedEvent.Visability = (VisabilityEnum)DR.GetInt32(5);
                            selectedEvent.AvailableFor = new List<Guid>(DR.GetFieldValue<Guid[]>(6));
                            selectedEvent.TargetItem = DR.GetGuid(7);
                            selectedEvent.Doer = DR.GetGuid(8);

                            logger.Trace($"Id: {selectedEvent.Id.ToString()}");
                            logger.Trace($"GroupId: {selectedEvent.GroupId.ToString()}");
                            logger.Trace($"CreationDate: {selectedEvent.CreationDate.ToString()}");
                            logger.Trace($"ItemType: {selectedEvent.ItemType.ToString()}");
                            logger.Trace($"MessageType: {selectedEvent.MessageType.ToString()}");
                            logger.Trace($"Visability: {selectedEvent.Visability.ToString()}");
                            logger.Trace($"AvailableFor: {selectedEvent.AvailableFor.Count.ToString()}");
                            logger.Trace($"TargetItem: {selectedEvent.TargetItem.ToString()}");
                            logger.Trace($"Doer: {selectedEvent.Doer.ToString()}");

                            allEvents.Add(selectedEvent);
                        }

                        return allEvents;
                    }
                }
                             
            }
            finally
            {
                logger.Trace("GetEvents leave.");
            }
        }
    }
}
