using CommonDB;
using CommonLib;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskService.Models
{

    /// <summary>
    /// Класс для работы с запросами к сервису БД
    /// </summary>
    public class DBWorker
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        #region Private

        private static BaseTask ReadBaseTaskItem(NpgsqlDataReader reader)
        {
            BaseTask foundTask = new BaseTask();

            foundTask.Id = (Guid)reader["Id"];
            foundTask.Type = (BaseTaskType)reader["Type"];
            foundTask.Name = (string)reader["Name"];
            foundTask.Description = (string)reader["Description"];
            foundTask.Cost = (int)reader["Cost"];
            foundTask.Penalty = (int)reader["Penalty"];
            foundTask.AvailableUntil = (DateTime)reader["AvailableUntil"];
            foundTask.SolutionTime = (TimeSpan)reader["SolutionTime"];
            foundTask.SpeedBonus = (int)reader["SpeedBonus"];
            foundTask.OwnerGroup = (Guid)reader["OwnerGroup"];
            foundTask.AvailableFor = (Guid[])reader["AvailableFor"];
            foundTask.Creator = (Guid)reader["Creator"];
            foundTask.Executor = (Guid)reader["Executor"];
            foundTask.Status = (BaseTaskStatus)reader["Status"];
            foundTask.CreationDate = (DateTime)reader["CreationDate"];
            foundTask.CompletionDate = (DateTime)reader["CompletionDate"];
            foundTask.ModificationTime = (DateTime)reader["ModificationTime"];

            return foundTask;
        }

        private static bool CheckTaskExist(Guid taskId)
        {
            try
            {
                Dictionary<string, object> searchParam = new Dictionary<string, object>();
                searchParam.Add("ID", taskId.ToString());
                return 0 != GetTasksCount(searchParam);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw ex;
            }
        }
        #endregion

        /// <summary>
        /// Добавление строки с новой задачей в таблицу с задачами.
        /// </summary>
        /// <param name="newTask"></param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="Exception">Ошибка при работе с БД</exception>
        public static void AddTask(BaseTask newTask)
        {
            string query = "";
            if (newTask == null)
                throw new ArgumentNullException("newTask");

            try
            {
                NpgsqlCommand npgSqlCommand;

                if (!QueriesInfo.Queries.TryGetValue("AddTask", out query))
                {
                    throw new Exception("Ошибка формирования запроса. ");
                }

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@id", newTask.Id);
                    npgSqlCommand.Parameters.AddWithValue("@Type", (int)newTask.Type);
                    npgSqlCommand.Parameters.AddWithValue("@Name", newTask.Name);
                    npgSqlCommand.Parameters.AddWithValue("@Description", newTask.Description);
                    npgSqlCommand.Parameters.AddWithValue("@Cost", newTask.Cost);
                    npgSqlCommand.Parameters.AddWithValue("@Penalty", newTask.Penalty);
                    npgSqlCommand.Parameters.AddWithValue("@AvailableUntil", newTask.AvailableUntil);
                    npgSqlCommand.Parameters.AddWithValue("@SolutionTime", newTask.SolutionTime);
                    npgSqlCommand.Parameters.AddWithValue("@SpeedBonus", newTask.SpeedBonus);
                    npgSqlCommand.Parameters.AddWithValue("@OwnerGroup", newTask.OwnerGroup);
                    npgSqlCommand.Parameters.AddWithValue("@AvailableFor", newTask.AvailableFor);
                    npgSqlCommand.Parameters.AddWithValue("@Creator", newTask.Creator);
                    npgSqlCommand.Parameters.AddWithValue("@Executor", newTask.Executor);
                    npgSqlCommand.Parameters.AddWithValue("@Status", (int)newTask.Status);
                    npgSqlCommand.Parameters.AddWithValue("@CreationDate", newTask.CreationDate);
                    npgSqlCommand.Parameters.AddWithValue("@CompletionDate", newTask.CompletionDate);
                    npgSqlCommand.Parameters.AddWithValue("@ModificationTime", newTask.ModificationTime);

                    logger.Trace($"npgSqlCommand: {npgSqlCommand.CommandText}");
                    int result = npgSqlCommand.ExecuteNonQuery();

                    if (result == 0)
                        throw new Exception("Запрос не затронул ни одной строки. ");

                    logger.Trace($"Задача '{newTask.Id}' создана");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка выполнения запроса. SQL: {query}", ex);
            }
        }

        /// <summary>
        /// Получение задачи по ее идентификатору.
        /// </summary>
        /// <param name="taskIds">Идентификатор задачи</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="Exception">Ошибка при работе с БД</exception>
        public static List<BaseTask> GetTasks(List<Guid> taskIds)
        {
            string query = "";
            List<BaseTask> foundTasksList = new List<BaseTask>();

            if (taskIds == null || taskIds.Count == 0)
                throw new ArgumentException("Необходимо задать минимум 1 идентификатор для поиска!");

            try
            {
                foreach (var taskId in taskIds)
                {
                    Dictionary<string, object> searchParam = new Dictionary<string, object>();
                    searchParam.Add("ID", taskId);
                    var foundTask = GetTasks(searchParam);
                    foundTasksList.AddRange(foundTask);
                }

                logger.Trace($"Найдено задач: {foundTasksList.Count}");

                return foundTasksList;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка выполнения запроса. SQL: {query}", ex);
            }
        }

        /// <summary>
        /// Поиск задач по значениям полей.
        /// </summary>
        /// <param name="searchParams">Идентификатор задачи</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="Exception">Ошибка при работе с БД</exception>
        public static int GetTasksCount(Dictionary<string, object> searchParams)
        {
            string query = "";
            List<BaseTask> foundTasksList = new List<BaseTask>();

            if (searchParams == null || searchParams.Count == 0)
                throw new ArgumentException("Необходимо задать минимум 1 параметр для поиска!");

            try
            {
                NpgsqlCommand npgSqlCommand;

                if (!QueriesInfo.Queries.TryGetValue("CountTaskTemplate", out query))
                {
                    throw new Exception("Ошибка формирования запроса. ");
                }

                //Формирование списка параметров и новых значений для запроса
                string queryParams = DBHelper.MakeSQLParamString(searchParams, "AND");

                //Склейка запроса и перечня параметров
                query = string.Format(query, queryParams);

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    foreach (var param in searchParams)
                    {
                        npgSqlCommand.Parameters.AddWithValue($"{@param.Key}", param.Value);
                    }

                    logger.Trace($"npgSqlCommand: {npgSqlCommand.CommandText}");

                    int taskCount = (int)npgSqlCommand.ExecuteScalar();

                    logger.Trace($"Найдено задач: {taskCount}");

                    return taskCount;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка выполнения запроса. SQL: {query}", ex);
            }
        }

        public static Int64 GetAllTasksCount(Guid groupId)
        {
            string query = "";

            try
            {
                NpgsqlCommand npgSqlCommand;

                if (!QueriesInfo.Queries.TryGetValue("GetAllTaskCount", out query))
                {
                    throw new Exception("Ошибка формирования запроса. ");
                }

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@OwnerGroup", groupId);                   
                    npgSqlCommand.Parameters.AddWithValue("@StatusDeleted", (int)BaseTaskStatus.Deleted);

                    logger.Trace($"npgSqlCommand: {npgSqlCommand.CommandText}");

                    Int64 taskCount = (Int64)npgSqlCommand.ExecuteScalar();

                    logger.Trace($"Найдено задач: {taskCount}");

                    return taskCount;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка выполнения запроса. SQL: {query}", ex);
            }
        }

        public static Int64 GetActiveTasksCount(Guid groupId)
        {
            string query = "";

            try
            {
                NpgsqlCommand npgSqlCommand;

                if (!QueriesInfo.Queries.TryGetValue("GetActiveTaskCount", out query))
                {
                    throw new Exception("Ошибка формирования запроса. ");
                }

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@OwnerGroup", groupId);
                    npgSqlCommand.Parameters.AddWithValue("@StatusAssigned", (int)BaseTaskStatus.Assigned);
                    npgSqlCommand.Parameters.AddWithValue("@StatusAccepted", (int)BaseTaskStatus.Accepted);
                    npgSqlCommand.Parameters.AddWithValue("@StatusInProgress", (int)BaseTaskStatus.InProgress);
                    npgSqlCommand.Parameters.AddWithValue("@StatusCompleted", (int)BaseTaskStatus.Completed);
                    npgSqlCommand.Parameters.AddWithValue("@StatusPendingReview", (int)BaseTaskStatus.PendingReview);

                    logger.Trace($"npgSqlCommand: {npgSqlCommand.CommandText}");

                    Int64 taskCount = (Int64)npgSqlCommand.ExecuteScalar();

                    logger.Trace($"Найдено задач: {taskCount}");

                    return taskCount;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка выполнения запроса. SQL: {query}", ex);
            }
        }

        /// <summary>
        /// Поиск задач по параметрам
        /// </summary>
        /// <param name="searchParams">Набор параметров для поиска</param>
        /// <returns>Список найденных задач</returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="Exception">Ошибка при работе с БД</exception>
        public static List<BaseTask> GetTasks(Dictionary<string, object> searchParams)
        {
            string query = "";
            List<BaseTask> foundTasksList = new List<BaseTask>();

            if (searchParams == null || searchParams.Count == 0)
                throw new ArgumentException("Необходимо задать минимум 1 параметр для поиска!");

            try
            {
                NpgsqlCommand npgSqlCommand;

                if (!QueriesInfo.Queries.TryGetValue("SearchTaskTemplate", out query))
                {
                    throw new Exception("Ошибка формирования запроса. ");
                }

                //Формирование списка параметров и новых значений для запроса
                string queryParams = DBHelper.MakeSQLParamString(searchParams, " AND ", false);

                //Склейка запроса и перечня параметров
                query = string.Format(query, queryParams);

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    foreach (var param in searchParams)
                    {
                        npgSqlCommand.Parameters.AddWithValue($"{@param.Key}", param.Value);
                    }

                    logger.Trace($"npgSqlCommand: {npgSqlCommand.CommandText}");

                    using (var reader = npgSqlCommand.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var task = ReadBaseTaskItem(reader);
                                foundTasksList.Add(task);
                            }
                        }

                        logger.Trace($"Найдено задач: {foundTasksList.Count}");
                        return foundTasksList;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка выполнения запроса. SQL: {query}", ex);
            }
        }

        /// <summary>
        /// Поиск задач по параметрам
        /// </summary>
        /// <param name="searchParams">Набор параметров для поиска</param>
        /// <returns>Список найденных задач</returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="Exception">Ошибка при работе с БД</exception>
        public static List<BaseTask> GetUserTasks(FQRequestInfo ri, Dictionary<string, object> searchParams)
        {
            string query = "";
            List<BaseTask> foundTasksList = new List<BaseTask>();

            if (searchParams == null || searchParams.Count == 0)
                throw new ArgumentException("Необходимо задать минимум 1 параметр для поиска!");

            try
            {
                NpgsqlCommand npgSqlCommand;

                if (!QueriesInfo.Queries.TryGetValue("SearchUserTaskTemplate", out query))
                {
                    throw new Exception("Ошибка формирования запроса. ");
                }

                //Формирование списка параметров и новых значений для запроса
                string queryParams = DBHelper.MakeSQLParamString(searchParams, " AND ", false);

                //Склейка запроса и перечня параметров
                //Пришлось вынести эту часть запроса в код
                //Иначе string.Format не отрабатывает из-за AvailableFor='{}'
                var queryLastPart = "AND {0} ORDER BY ModificationTime DESC;";
                queryLastPart = string.Format(queryLastPart, queryParams);
                query += queryLastPart;

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@StatusCreated", (int)BaseTaskStatus.Created);
                    npgSqlCommand.Parameters.AddWithValue("@StatusClosed", (int)BaseTaskStatus.Closed);
                    npgSqlCommand.Parameters.AddWithValue("@StatusDeleted", (int)BaseTaskStatus.Deleted);
                    npgSqlCommand.Parameters.AddWithValue("@CurrentUser", ri._User.Id);

                    foreach (var param in searchParams)
                    {
                        npgSqlCommand.Parameters.AddWithValue($"{@param.Key}", param.Value);
                    }

                    logger.Trace($"npgSqlCommand: {npgSqlCommand.CommandText}");
                    using (var reader = npgSqlCommand.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var task = ReadBaseTaskItem(reader);
                                foundTasksList.Add(task);
                            }
                        }

                        logger.Trace($"Найдено задач: {foundTasksList.Count}");
                        return foundTasksList;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка выполнения запроса. SQL: {query}", ex);
            }
        }

        /// <summary>
        /// Получение IDшников всех задач, где удаляемы юзер Executor или единственный AvailableFor
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="removingUserId"></param>
        /// <returns></returns>
        public static List<Guid> GetRelatedTasks(Guid groupId, Guid removingUserId)
        {
            string query = "";
            List<Guid> relatedTasksIds = new List<Guid>();

            try
            {
                NpgsqlCommand npgSqlCommand;

                if (!QueriesInfo.Queries.TryGetValue("SearchRelatedTasks", out query))
                {
                    throw new Exception("Ошибка формирования запроса. ");
                }

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@OwnerGroup", groupId);
                    npgSqlCommand.Parameters.AddWithValue("@RemovingUserId", removingUserId);

                    logger.Trace($"npgSqlCommand: {npgSqlCommand.CommandText}");

                    using (var reader = npgSqlCommand.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var relatedTaskId = (Guid)reader["Id"];
                                relatedTasksIds.Add(relatedTaskId);
                            }
                        }

                        logger.Trace($"Найдено задач: {relatedTasksIds.Count}");
                        return relatedTasksIds;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка выполнения запроса. SQL: {query}", ex);
            }
        }

        /// <summary>
        /// Получение IDшников всех InProgress задач, c истекшим SolutionTime        
        /// <returns></returns>
        public static List<BaseTask> GetExpiredTasks()
        {
            string query = "";
            List<BaseTask> expiredTasks = new List<BaseTask>();

            try
            {
                NpgsqlCommand npgSqlCommand;

                if (!QueriesInfo.Queries.TryGetValue("SearchTaskTemplate", out query))
                {
                    throw new Exception("Ошибка формирования запроса. ");
                }

                //TODO: darkmagic AvailableUntil
                string queryParams = "";
                queryParams += "(Status = @Status_Assigned OR Status = @Status_InProgress)";
                queryParams += " AND AvailableUntil != '1970-01-01 00:00:00'"; //дефолтное значение не трогаем
                queryParams += " AND AvailableUntil < @AvailableUntil";

                query = string.Format(query, queryParams);

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@Status_Assigned", (int)BaseTaskStatus.Assigned);
                    npgSqlCommand.Parameters.AddWithValue("@Status_InProgress", (int)BaseTaskStatus.InProgress);
                    npgSqlCommand.Parameters.AddWithValue("@AvailableUntil", DateTime.UtcNow);

                    using (var reader = npgSqlCommand.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var task = ReadBaseTaskItem(reader);
                                expiredTasks.Add(task);
                            }
                        }

                        return expiredTasks;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка выполнения запроса. SQL: {query}", ex);
            }
        }

        //TODO: зарезервировано - не удалять
        //public static void UpdateRelatedTasks_Executor(Guid groupId, Guid removingUserId)
        //{
        //    string query = "";

        //    try
        //    {
        //        NpgsqlCommand npgSqlCommand;

        //        if (!QueriesInfo.Queries.TryGetValue("UpdateRelatedTasks_Executor", out query))
        //        {
        //            throw new Exception("Ошибка формирования запроса. ");
        //        }

        //        using (NpgsqlConnection dbConnection = DBManager.GetConnection())
        //        using (npgSqlCommand = new NpgsqlCommand(query, dbConnection))
        //        {
        //            npgSqlCommand.Parameters.AddWithValue("@OwnerGroup", groupId);
        //            npgSqlCommand.Parameters.AddWithValue("@RemovingUserId", removingUserId);
        //            npgSqlCommand.Parameters.AddWithValue("@ExecutorGuidEmpty", Guid.Empty);
        //            npgSqlCommand.Parameters.AddWithValue("@ModificationTime", DateTime.UtcNow);

        //            logger.Trace($"npgSqlCommand: {npgSqlCommand.CommandText}");

        //            var rowAffected = npgSqlCommand.ExecuteNonQuery();

        //            logger.Trace($"Обновлено задач: {rowAffected}");

        //            return;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Ошибка выполнения запроса. SQL: {query}", ex);
        //    }
        //}

        //public static void UpdateRelatedTasks_AvailableFor(Guid groupId, Guid removingUserId)
        //{
        //    string query = "";

        //    try
        //    {
        //        NpgsqlCommand npgSqlCommand;

        //        if (!QueriesInfo.Queries.TryGetValue("UpdateRelatedTasks_AvailableFor", out query))
        //        {
        //            throw new Exception("Ошибка формирования запроса. ");
        //        }

        //        using (NpgsqlConnection dbConnection = DBManager.GetConnection())
        //        using (npgSqlCommand = new NpgsqlCommand(query, dbConnection))
        //        {
        //            npgSqlCommand.Parameters.AddWithValue("@OwnerGroup", groupId);
        //            npgSqlCommand.Parameters.AddWithValue("@RemovingUserId", removingUserId);
        //            npgSqlCommand.Parameters.AddWithValue("@ModificationTime", DateTime.UtcNow);

        //            logger.Trace($"npgSqlCommand: {npgSqlCommand.CommandText}");

        //            var rowAffected = npgSqlCommand.ExecuteNonQuery();

        //            logger.Trace($"Обновлено задач: {rowAffected}");

        //            return;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Ошибка выполнения запроса. SQL: {query}", ex);
        //    }
        //}

        /// <summary>
        /// Удаление удаляемого юзера из всех AvailableFor
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="removingUserId"></param>
        public static void UpdateRelatedTasks(Guid groupId, Guid removingUserId)
        {
            string query = "";

            try
            {
                NpgsqlCommand npgSqlCommand;

                if (!QueriesInfo.Queries.TryGetValue("UpdateRelatedTasks", out query))
                {
                    throw new Exception("Ошибка формирования запроса. ");
                }

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@OwnerGroup", groupId);
                    npgSqlCommand.Parameters.AddWithValue("@RemovingUserId", removingUserId);
                    npgSqlCommand.Parameters.AddWithValue("@ModificationTime", DateTime.UtcNow);

                    logger.Trace($"npgSqlCommand: {npgSqlCommand.CommandText}");

                    var rowAffected = npgSqlCommand.ExecuteNonQuery();

                    logger.Trace($"Обновлено задач: {rowAffected}");

                    return;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка выполнения запроса. SQL: {query}", ex);
            }
        }

        /// <summary>
        /// Обновление полей задачи
        /// </summary>
        /// <param name="targetTaskId">Идентификатор обновляемой задачи</param>
        /// <param name="newParams">Набор полей для обновления</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="Exception">Ошибка при работе с БД</exception>
        public static void UpdateTask(Guid targetTaskId, Guid groupId, Dictionary<string, object> newParams)
        {
            string query = "";

            if (newParams == null || newParams.Count == 0)
                throw new ArgumentException("Необходимо задать минимум 1 параметр для обновления!");

            // Изменение ID недопустимо
            if (newParams.ContainsKey("Id"))
            {
                newParams.Remove("Id");
            }

            if (newParams.ContainsKey("OwnerGroup"))
            {
                newParams.Remove("OwnerGroup");
            }


            try
            {
                NpgsqlCommand npgSqlCommand;

                if (!QueriesInfo.Queries.TryGetValue("UpdateTaskTemplate", out query))
                {
                    throw new Exception("Ошибка формирования запроса. ");
                }

                //Формирование списка параметров и новых значений для запроса                
                string setParams = DBHelper.MakeSQLParamString(newParams, ", ", false);

                //Склейка запроса и перечня параметров
                query = string.Format(query, setParams);

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@id", targetTaskId);
                    npgSqlCommand.Parameters.AddWithValue("@OwnerGroup", groupId);

                    foreach (var param in newParams)
                    {
                        npgSqlCommand.Parameters.AddWithValue($"@{param.Key}", param.Value);
                    }

                    logger.Trace($"npgSqlCommand: {npgSqlCommand.CommandText}");

                    var rowAffected = npgSqlCommand.ExecuteNonQuery();
                    if (rowAffected == 0)
                        throw new Exception($"Не удалось обновить задачу ${targetTaskId}");

                    logger.Trace($"Обновлено задач: {rowAffected}");

                    return;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка выполнения запроса. SQL: {query}", ex);
            }
        }


        /// <summary>
        /// Удаление задач по параметрам
        /// </summary>
        /// <param name="searchParams">Параметры поиска удаляемых задач</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="Exception">Ошибка при работе с БД</exception>
        public static void DeleteTasks(Dictionary<string, object> searchParams)
        {
            string query = "";

            if (searchParams == null || searchParams.Count == 0)
                throw new ArgumentException("Необходимо задать минимум 1 поле поиска для удаления!");

            try
            {
                NpgsqlCommand npgSqlCommand;

                if (!QueriesInfo.Queries.TryGetValue("DeleteTaskTemplate", out query))
                {
                    throw new Exception("Ошибка формирования запроса. ");
                }

                //Формирование списка параметров и новых значений для запроса
                string queryParams = DBHelper.MakeSQLParamString(searchParams, "AND");

                //Склейка запроса и перечня параметров
                query = string.Format(query, queryParams);

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    foreach (var param in searchParams)
                    {
                        npgSqlCommand.Parameters.AddWithValue($"@{param.Key}", param.Value);
                    }

                    logger.Trace($"npgSqlCommand: {npgSqlCommand.CommandText}");

                    var rowAffected = npgSqlCommand.ExecuteNonQuery();
                    if (rowAffected == 0)
                        throw new Exception("Не удалось выполнить удаление задач по шаблону");

                    logger.Trace($"Удалено задач: {rowAffected}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка выполнения запроса. SQL: {query}", ex);
            }
        }

        /// <summary>
        /// Удаление задач по их идентификаторам
        /// </summary>
        /// <param name="targetTaskIdList">Список идентификаторов задач для удаления</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="Exception">Ошибка при работе с БД</exception>
        public static void DeleteTasks(Guid groupId, List<Guid> targetTaskIdList)
        {
            string query = "";

            if (targetTaskIdList == null || targetTaskIdList.Count == 0)
                throw new ArgumentException("Необходимо задать минимум 1 задачу для удаления!");

            try
            {
                NpgsqlCommand npgSqlCommand;

                if (!QueriesInfo.Queries.TryGetValue("DeleteTask", out query))
                {
                    throw new Exception("Ошибка формирования запроса. ");
                }

                foreach (var taskId in targetTaskIdList)
                {
                    using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                    using (npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                    {
                        npgSqlCommand.Parameters.AddWithValue("@id", taskId);
                        npgSqlCommand.Parameters.AddWithValue("@OwnerGroup", groupId); 

                        logger.Trace($"npgSqlCommand: {npgSqlCommand.CommandText}");

                        var rowAffected = npgSqlCommand.ExecuteNonQuery();
                        if (rowAffected == 0)
                            logger.Error($"Не удалось удалить задачу ${taskId}");


                        logger.Trace($"Задача {taskId} удалена");
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка выполнения запроса. SQL: {query}", ex);
            }
        }
    }
}
