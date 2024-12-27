using CommonDB;
using CommonLib;
using CommonTypes;
using Npgsql;
using System;
using System.Collections.Generic;
using static CommonTypes.User;

namespace AccountService.Models
{
    /// <summary>
    /// Класс для работы с БД
    /// </summary>
    public class DBWorker
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        /// <summary>
        /// Проверка состоит ли целевой пользователь в группе
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>Guid группы или null</returns>
        public static Guid CheckUserIsInGroup(Guid userId)
        {
            try
            {
                logger.Trace("CheckUserIsInGroup started.");

                logger.Trace($"userId: {userId}");

                var query = QueriesInfo.GetQueryTemplate("GetGroupId");

                logger.Trace($"Query: {query}");

                using(NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@ID", userId);

                    Guid groupId = Guid.Empty;

                    using (NpgsqlDataReader DR = npgSqlCommand.ExecuteReader())
                    {
                        if (!DR.HasRows)
                        {
                            throw new Exception("HasRows: false.");
                        }

                        while (DR.Read())
                        {
                            logger.Trace("Считано.");

                            groupId = DR.GetGuid(0);
                            logger.Trace($"groupId: {groupId}");
                        }
                    }

                    if (groupId == Guid.Empty)
                    {
                        throw new Exception("groupId == Guid.Empty.");
                    }

                    return groupId;
                }
            }
            finally
            {
                logger.Trace("CheckUserIsInGroup leave.");
            }
        }

        /// <summary>
        /// Получение хэша пароля пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns></returns>
        public static string GetUserPasswordHash(Guid userId)
        {
            try
            {
                logger.Trace("GetUserPasswordHash started.");

                logger.Trace($"userId: {userId.ToString()}");

                var query = QueriesInfo.GetQueryTemplate("GetAccountById");

                logger.Trace($"Query: {query}");

                using(NpgsqlConnection dbConnection = DBManager.GetConnection())
				using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@UserID", userId);

                    string actualHash = string.Empty;

                    using (NpgsqlDataReader DR = npgSqlCommand.ExecuteReader())
                    {
                        if (!DR.HasRows)
                        {
                            throw new Exception("HasRows: false.");
                        }

                        while (DR.Read())
                        {
                            actualHash = DR.GetString(1);
                            logger.Trace($"actualHash: {actualHash}");
                        }
                    }

                    if (actualHash == string.Empty)
                    {
                        throw new Exception("actualHash == string.Empty");
                    }

                    return actualHash;
                }
            }
            finally
            {
                logger.Trace("GetUserPasswordHash leave.");
            }
        }

        /// <summary>
        /// Перезапись токена сессии
        /// </summary>
        /// <param name="inputAccount"></param>
        public static void SetAccessData(Account inputAccount, bool setDeviceId = false)
        {
            try
            {
                logger.Trace("SetAccessData started.");

                logger.Trace($"userId: {inputAccount.userId.ToString()}");
                logger.Trace($"tokenB64: {inputAccount.Token}");                

                var query = QueriesInfo.GetQueryTemplate("SetAccessData");                

                if (setDeviceId)
                {
                    logger.Trace($"DeviceId: {inputAccount.DeviceId}");
                    query = string.Format(query, " DeviceID = @DeviceID,");
                }
                else
                {
                    query = string.Format(query, string.Empty);
                }

                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
				using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@UserID", inputAccount.userId);
                    npgSqlCommand.Parameters.AddWithValue("@Token", inputAccount.Token);

                    if(setDeviceId)
                    {
                        npgSqlCommand.Parameters.AddWithValue("@DeviceID", inputAccount.DeviceId);
                    }

                    npgSqlCommand.Parameters.AddWithValue("@LastAction", DateTime.UtcNow);

                    var updatedRows = npgSqlCommand.ExecuteNonQuery();

                    if (updatedRows != 1)
                    {
                        throw new Exception($"updatedRows: {updatedRows}.");
                    }
                }
            }
            finally
            {
                logger.Trace("SetAccessData leave.");
            }
        }

        //Reg
        /// <summary>
        /// Проверка наличия в системе записи об аккаунте
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public static bool CheckAccountExist(string login)
        {
            try
            {
                logger.Trace("CheckAccountExist started.");

                logger.Trace($"login: {login}");

                var query = QueriesInfo.GetQueryTemplate("CheckAccountExist");

                logger.Trace($"Query: {query}");

                using(NpgsqlConnection dbConnection = DBManager.GetConnection())
				using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@Login", login);

                    var count = (Int64)npgSqlCommand.ExecuteScalar();

                    if (count == 1)
                    {
                        return true;
                    }
                    else
                    {
                        if (count == 0)
                        {
                            return false;
                        }
                        else
                        {
                            throw new Exception($"Count: {count}.");
                        }
                    }                    
                }
            }
            finally
            {
                logger.Trace("GetTempAccount leave.");
            }
        }

        /// <summary>
        /// Проверка наличия в системе временной записи об аккаунте
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool CheckTempAccountExist(string email)
        {
            try
            {
                logger.Trace("CheckTempAccountExist started.");

                logger.Trace($"email: {email}");

                var query = QueriesInfo.GetQueryTemplate("CheckTempAccountExist");

                using(NpgsqlConnection dbConnection = DBManager.GetConnection())
				using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@Email", email);

                    logger.Trace("Выполнение запроса."); 
                    var count = (Int64)npgSqlCommand.ExecuteScalar();

                    if (count == 1)
                    {
                        return true;
                    }
                    else
                    {
                        if (count == 0)
                        {
                            return false;
                        }
                        else
                        {
                            throw new Exception($"Count: {count}.");
                        }
                    }
                }
            }
            finally
            {
                logger.Trace("CheckTempAccountExist leave.");
            }
        }

        /// <summary>
        /// Добавление записи об аккаунте
        /// </summary>
        /// <param name="inputAccount"></param>
        public static void InsertAccount(Account inputAccount)
        {
            try
            {
                logger.Trace("InsertAccount started.");

                logger.Trace($"userId: {inputAccount.userId.ToString()}");
                logger.Trace($"failedLoginTryings: {inputAccount.failedLoginTryings.ToString()}");
                logger.Trace($"login: {inputAccount.Login}");
                logger.Trace($"email: {inputAccount.Email}");
                logger.Trace($"isMain: {inputAccount.isMain.ToString()}");
                logger.Trace($"lastAction: {inputAccount.LastAction.ToString()}");
                logger.Trace($"PasswordHashNew: {inputAccount.PasswordHashNew}");
                logger.Trace($"PasswordHashCurrent: {inputAccount.PasswordHashCurrent}");
                logger.Trace($"Token: {inputAccount.Token}");
                logger.Trace($"DeviceId: {inputAccount.DeviceId}");
                logger.Trace($"CreationDate: {inputAccount.CreationDate}");

                var query = QueriesInfo.GetQueryTemplate("InsertAccount");
                
                logger.Trace($"Query: {query}");

                using(NpgsqlConnection dbConnection = DBManager.GetConnection())
				using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@Login", inputAccount.Login);
                    npgSqlCommand.Parameters.AddWithValue("@Email", inputAccount.Email);
                    npgSqlCommand.Parameters.AddWithValue("@HASHPassword", inputAccount.PasswordHashCurrent);
                    npgSqlCommand.Parameters.AddWithValue("@UserID", inputAccount.userId);
                    npgSqlCommand.Parameters.AddWithValue("@FailedLoginTryings", inputAccount.failedLoginTryings);
                    npgSqlCommand.Parameters.AddWithValue("@IsMain", inputAccount.isMain);
                    npgSqlCommand.Parameters.AddWithValue("@Token", inputAccount.Token);
                    npgSqlCommand.Parameters.AddWithValue("@DeviceID", inputAccount.DeviceId);
                    npgSqlCommand.Parameters.AddWithValue("@LastAction", inputAccount.LastAction);
                    npgSqlCommand.Parameters.AddWithValue("@CreationDate", inputAccount.CreationDate);

                    var insertedRows = npgSqlCommand.ExecuteNonQuery();

                    if (insertedRows != 1)
                    {
                        throw new Exception($"insertedRows: {insertedRows}.");
                    }
                }
            }
            finally
            {
                logger.Trace("InsertAccount leave.");
            }
        }

        /// <summary>
        /// Добавление временной записи об аккаунте
        /// </summary>
        /// <param name="inputTempAccount"></param>
        public static void InsertTempAccount(Account inputTempAccount)
        {
            try
            {
                logger.Trace("InsertTempAccount started.");

                logger.Trace($"userId: {inputTempAccount.userId.ToString()}");
                logger.Trace($"failedLoginTryings: {inputTempAccount.failedLoginTryings.ToString()}");
                logger.Trace($"login: {inputTempAccount.Login}");
                logger.Trace($"email: {inputTempAccount.Email}");
                logger.Trace($"isMain: {inputTempAccount.isMain.ToString()}");
                logger.Trace($"lastAction: {inputTempAccount.LastAction.ToString()}");
                logger.Trace($"PasswordHashNew: {inputTempAccount.PasswordHashNew}");
                logger.Trace($"PasswordHashCurrent: {inputTempAccount.PasswordHashCurrent}");
                logger.Trace($"Token: {inputTempAccount.Token}");
                logger.Trace($"Token: {inputTempAccount.ConfirmCode}");

                var query = QueriesInfo.GetQueryTemplate("InsertTempAccount");
                
                logger.Trace($"Query: {query}");

                using(NpgsqlConnection dbConnection = DBManager.GetConnection())
				using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@Email", inputTempAccount.Email);
                    npgSqlCommand.Parameters.AddWithValue("@HASHPassword", inputTempAccount.PasswordHashNew);
                    npgSqlCommand.Parameters.AddWithValue("@ConfirmCode", inputTempAccount.ConfirmCode);
                    npgSqlCommand.Parameters.AddWithValue("@CreationDate", inputTempAccount.CreationDate);

                    var insertedRows = npgSqlCommand.ExecuteNonQuery();

                    if (insertedRows != 1)
                    {
                        throw new Exception($"insertedRows: {insertedRows}.");
                    }
                }
            }
            finally
            {
                logger.Trace("InsertTempAccount leave.");
            }
        }
               
        /// <summary>
        /// Удаление временной записи об аккаунте
        /// </summary>
        /// <param name="email"></param>
        public static void RemoveTempAccount(string email)
        {
            try
            {
                logger.Trace("RemoveTempAccount started.");

                logger.Trace($"email: {email}");

                var query = QueriesInfo.GetQueryTemplate("RemoveTempAccount");
                
                logger.Trace($"Query: {query}");

                using(NpgsqlConnection dbConnection = DBManager.GetConnection())
				using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@Email", email);

                    var removedRows = npgSqlCommand.ExecuteNonQuery();

                    if (removedRows != 1)
                    {
                        throw new Exception($"removedRows: {removedRows}.");
                    }
                }
            }
            finally
            {
                logger.Trace("RemoveTempAccount leave.");
            }
        }

        /// <summary>
        /// Получение записи об аккаунте по логину
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public static Account GetAccount(string login)
        {
            try
            {
                logger.Trace("GetAccount started.");

                logger.Trace($"login: {login}");

                var query = QueriesInfo.GetQueryTemplate("GetAccount");
                
                logger.Trace($"Query: {query}");

                using(NpgsqlConnection dbConnection = DBManager.GetConnection())
				using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@Login", login);

                    Account acc = new Account(true);

                    using (NpgsqlDataReader DR = npgSqlCommand.ExecuteReader())
                    {
                        if (!DR.HasRows)
                        {
                            throw new Exception("HasRows: false.");
                        }

                        while (DR.Read())
                        {
                            acc.Login = DR.GetString(0);
                            acc.PasswordHashCurrent = DR.GetString(1);
                            acc.userId = DR.GetGuid(2);
                            acc.isMain = DR.GetBoolean(3);
                            acc.failedLoginTryings = DR.GetInt32(4);
                            acc.Token = DR.GetString(5);
                            acc.DeviceId = DR.GetString(6);
                            acc.LastAction = DR.GetDateTime(7);
                            acc.Email = DR.GetString(8);
                            acc.CreationDate = DR.GetDateTime(9);

                            logger.Trace($"userId: {acc.userId.ToString()}");
                            logger.Trace($"failedLoginTryings: {acc.failedLoginTryings.ToString()}");
                            logger.Trace($"login: {acc.Login}");
                            logger.Trace($"email: {acc.Email}");
                            logger.Trace($"isMain: {acc.isMain.ToString()}");
                            logger.Trace($"PasswordHashCurrent: {acc.PasswordHashCurrent}");
                            logger.Trace($"Token: {acc.Token}");
                            logger.Trace($"DeviceId: {acc.DeviceId}");
                            logger.Trace($"LastAction: {acc.LastAction.ToString()}");
                            logger.Trace($"CreationDate: {acc.CreationDate}");
                        }
                    }

                    return acc;
                }
            }
            finally
            {
                logger.Trace("GetAccount leave.");
            }
        }

        /// <summary>
        /// Получение записи об аккаунте по идентификатору пользователя
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static Account GetAccountById(Guid userId)
        {
            try
            {
                logger.Trace("GetAccountById started.");

                logger.Trace($"userId: {userId.ToString()}");

                var query = QueriesInfo.GetQueryTemplate("GetAccountById");
                
                logger.Trace($"Query: {query}");

                using(NpgsqlConnection dbConnection = DBManager.GetConnection())
				using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@UserID", userId);

                    Account acc = new Account(true);

                    using (NpgsqlDataReader DR = npgSqlCommand.ExecuteReader())
                    {

                        if (!DR.HasRows)
                        {
                            throw new Exception("HasRows: false.");
                        }

                        while (DR.Read())
                        {
                            acc.Login = DR.GetString(0);
                            acc.PasswordHashCurrent = DR.GetString(1);
                            acc.userId = DR.GetGuid(2);
                            acc.isMain = DR.GetBoolean(3);
                            acc.failedLoginTryings = DR.GetInt32(4);
                            acc.Token = DR.GetString(5);
                            acc.DeviceId = DR.GetString(6);
                            acc.LastAction = DR.GetDateTime(7);
                            acc.Email = DR.GetString(8);
                            acc.CreationDate = DR.GetDateTime(9);

                            logger.Trace($"userId: {acc.userId.ToString()}");
                            logger.Trace($"failedLoginTryings: {acc.failedLoginTryings.ToString()}");
                            logger.Trace($"login: {acc.Login}");
                            logger.Trace($"email: {acc.Email}");
                            logger.Trace($"isMain: {acc.isMain.ToString()}");
                            logger.Trace($"PasswordHashCurrent: {acc.PasswordHashCurrent}");
                            logger.Trace($"Token: {acc.Token}");
                            logger.Trace($"DeviceId: {acc.DeviceId}");
                            logger.Trace($"LastAction: {acc.LastAction.ToString()}");
                            logger.Trace($"CreationDate: {acc.CreationDate}");
                        }
                    }

                    return acc;
                }
            }
            finally
            {
                logger.Trace("GetAccountById leave.");
            }
        }

        /// <summary>
        /// Получение временной записи об аккаунте по логину/email-у
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static Account GetTempAccount(string email)
        {
            try
            {
                logger.Trace("GetTempAccount started.");

                logger.Trace($"userId: {email}");

                var query = QueriesInfo.GetQueryTemplate("GetTempAccount");

                logger.Trace($"Query: {query}");

                using(NpgsqlConnection dbConnection = DBManager.GetConnection())
				using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@Email", email);

                    Account tempAcc = new Account(true);

                    using (NpgsqlDataReader DR = npgSqlCommand.ExecuteReader())
                    {
                        if (!DR.HasRows)
                        {
                            throw new Exception("HasRows: false.");
                        }
                       
                        while (DR.Read())
                        {
                            tempAcc.Email = DR.GetString(0);
                            tempAcc.PasswordHashCurrent = DR.GetString(1);
                            tempAcc.ConfirmCode = DR.GetString(2);
                            logger.Trace($"email: {tempAcc.Email}");
                            logger.Trace($"PasswordHashCurrent: {tempAcc.PasswordHashCurrent}");
                            logger.Trace($"ConfirmCode: {tempAcc.ConfirmCode}");
                        }
                    }

                    return tempAcc;
                }
            }
            finally
            {
                logger.Trace("GetTempAccount leave.");
            }
        }

        /// <summary>
        /// Изменение пароля
        /// </summary>
        /// <param name="inputAccount"></param>
        public static void ChangePassword(Account inputAccount)
        {
            try
            {
                logger.Trace("ChangePassword started.");

                logger.Trace($"userId: {inputAccount.userId.ToString()}");
                logger.Trace($"failedLoginTryings: {inputAccount.failedLoginTryings.ToString()}");
                logger.Trace($"login: {inputAccount.Login}");
                logger.Trace($"email: {inputAccount.Email}");
                logger.Trace($"isMain: {inputAccount.isMain.ToString()}");
                logger.Trace($"lastAction: {inputAccount.LastAction.ToString()}");
                logger.Trace($"PasswordHashNew: {inputAccount.PasswordHashNew}");
                logger.Trace($"PasswordHashCurrent: {inputAccount.PasswordHashCurrent}");
                logger.Trace($"Token: {inputAccount.Token}");

                var query = QueriesInfo.GetQueryTemplate("ChangePassword");

                logger.Trace($"Query: {query}");

                using(NpgsqlConnection dbConnection = DBManager.GetConnection())
				using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@HASHPassword", inputAccount.PasswordHashNew);
                    npgSqlCommand.Parameters.AddWithValue("@UserID", inputAccount.userId);

                    var changedRows = npgSqlCommand.ExecuteNonQuery();

                    if (changedRows != 1)
                    {
                        throw new Exception($"changedRows: {changedRows}.");
                    }
                }
            }
            finally
            {
                logger.Trace("ChangePassword leave.");
            }
        }

        /// <summary>
        /// Cброс пароля
        /// </summary>
        /// <param name="inputAccount"></param>
        public static void ResetPassword(Account inputAccount)
        {
            try
            {
                logger.Trace("ResetPassword started.");

                logger.Trace($"userId: {inputAccount.userId.ToString()}");
                logger.Trace($"failedLoginTryings: {inputAccount.failedLoginTryings.ToString()}");
                logger.Trace($"login: {inputAccount.Login}");
                logger.Trace($"email: {inputAccount.Email}");
                logger.Trace($"isMain: {inputAccount.isMain.ToString()}");
                logger.Trace($"lastAction: {inputAccount.LastAction.ToString()}");
                logger.Trace($"PasswordHashNew: {inputAccount.PasswordHashNew}");
                logger.Trace($"PasswordHashCurrent: {inputAccount.PasswordHashCurrent}");
                logger.Trace($"Token: {inputAccount.Token}");

                var query = QueriesInfo.GetQueryTemplate("ResetPassword");

                logger.Trace($"Query: {query}");

                using(NpgsqlConnection dbConnection = DBManager.GetConnection())
				using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@Login", inputAccount.Login);
                    npgSqlCommand.Parameters.AddWithValue("@HASHPassword", inputAccount.PasswordHashNew);

                    var updatedRows = npgSqlCommand.ExecuteNonQuery();

                    if (updatedRows != 1)
                    {
                        throw new Exception($"updatedRows: {updatedRows}.");
                    }
                }
            }
            finally
            {
                logger.Trace("ResetPassword leave.");
            }
        }

        /// <summary>
        /// Обновление полей записи аккаунта
        /// </summary>
        /// <param name="inputAccount"></param>
        public static void RemoveAccessData(Guid userId)
        {
            try
            {
                logger.Trace("RemoveAccessData started.");

                logger.Trace($"userId: {userId.ToString()}");

                var query = QueriesInfo.GetQueryTemplate("RemoveAccessData");

                logger.Trace($"Query: {query}");

                using(NpgsqlConnection dbConnection = DBManager.GetConnection())
				using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@UserID", userId);

                    var updatedRows = npgSqlCommand.ExecuteNonQuery();

                    if (updatedRows != 1)
                    {
                        throw new Exception($"updatedRows: {updatedRows}.");
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                logger.Trace("RemoveAccessData leave.");
            }
        }

        public static void SetFailedLoginTryings(string login, int value)
        {
            try
            {
                logger.Trace("SetFailedLoginTryings started.");

                logger.Trace($"login: {login}");

                var query = QueriesInfo.GetQueryTemplate("SetFailedLoginTryings");

                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@FailedLoginTryings", value);
                    npgSqlCommand.Parameters.AddWithValue("@Login", login);

                    var updatedRows = npgSqlCommand.ExecuteNonQuery();

                    if (updatedRows != 1)
                    {
                        throw new Exception($"updatedRows: {updatedRows}.");
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                logger.Trace("SetFailedLoginTryings leave.");
            }
        }

        /// <summary>
        /// Изменение email
        /// </summary>
        /// <param name="inputAccount"></param>
        public static void ChangeEmail(Account inputAccount)
        {
            try
            {
                logger.Trace("ChangeEmail started.");

                logger.Trace($"userId: {inputAccount.userId.ToString()}");
                logger.Trace($"failedLoginTryings: {inputAccount.failedLoginTryings.ToString()}");
                logger.Trace($"login: {inputAccount.Login}");
                logger.Trace($"email: {inputAccount.Email}");
                logger.Trace($"isMain: {inputAccount.isMain.ToString()}");
                logger.Trace($"lastAction: {inputAccount.LastAction.ToString()}");
                logger.Trace($"PasswordHashNew: {inputAccount.PasswordHashNew}");
                logger.Trace($"PasswordHashCurrent: {inputAccount.PasswordHashCurrent}");
                logger.Trace($"Token: {inputAccount.Token}");

                var query = QueriesInfo.GetQueryTemplate("ChangeEmail");

                logger.Trace($"Query: {query}");

                using(NpgsqlConnection dbConnection = DBManager.GetConnection())
				using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@Login", inputAccount.Email);
                    npgSqlCommand.Parameters.AddWithValue("@Email", inputAccount.Email);
                    npgSqlCommand.Parameters.AddWithValue("@UserID", inputAccount.userId);

                    var changedRows = npgSqlCommand.ExecuteNonQuery();

                    if (changedRows != 1)
                    {
                        throw new Exception($"changedRows: {changedRows}.");
                    }
                }
            }            
            finally
            {
                logger.Trace("ChangeEmail leave.");
            }
        }

        /// <summary>
        /// Создание нового пользователя
        /// </summary>
        /// <param name="inputUser"></param>
        public static void CreateUser(User inputUser)
        {
            try
            {
                logger.Trace("CreateUser started.");
                logger.Trace($"id: {inputUser.Id.ToString()}");
                logger.Trace($"groupId: {inputUser.GroupId.ToString()}");
                logger.Trace($"name: {inputUser.Name}");
                logger.Trace($"title: {inputUser.Title}");
                logger.Trace($"role: {inputUser.Role.ToString()}");
                logger.Trace($"coins: {inputUser.Coins.ToString()}");
                logger.Trace($"Image: {inputUser.Image}");

                var query = QueriesInfo.GetQueryTemplate("CreateUser");

                logger.Trace($"Query: {query}");

                //Формирование запроса
                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@ID", inputUser.Id);
                    npgSqlCommand.Parameters.AddWithValue("@GroupID", inputUser.GroupId);
                    npgSqlCommand.Parameters.AddWithValue("@Name", inputUser.Name);
                    npgSqlCommand.Parameters.AddWithValue("@Title", inputUser.Title);
                    npgSqlCommand.Parameters.AddWithValue("@Role", (Int32)inputUser.Role);
                    npgSqlCommand.Parameters.AddWithValue("@Coins", inputUser.Coins);
                    npgSqlCommand.Parameters.AddWithValue("@Img", inputUser.Image);
                    npgSqlCommand.Parameters.AddWithValue("@Status", (int)UserStatus.Active);

                    var insertedRows = npgSqlCommand.ExecuteNonQuery();

                    if (insertedRows != 1)
                    {
                        throw new Exception($"insertedRows: {insertedRows}.");
                    }
                }
            }
            finally
            {
                logger.Trace("CreateUser leave.");
            }
        }

        /// <summary>
        /// Обновление информации о пользователях
        /// </summary>
        /// <param name="inputUser"></param>
        public static void UpdateUser(User inputUser)
        {
            try
            {
                logger.Trace("UpdateUser started.");
                logger.Trace($"id: {inputUser.Id.ToString()}");
                logger.Trace($"groupId: {inputUser.GroupId.ToString()}");
                logger.Trace($"name: {inputUser.Name}");
                logger.Trace($"title: {inputUser.Title}");
                logger.Trace($"role: {inputUser.Role.ToString()}");
                logger.Trace($"coins: {inputUser.Coins.ToString()}");
                logger.Trace($"Image: {inputUser.Image}");

                var query = QueriesInfo.GetQueryTemplate("UpdateUser");

                logger.Trace($"Query: {query}");

                //Формирование запроса
                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@Name", inputUser.Name);
                    npgSqlCommand.Parameters.AddWithValue("@Title", inputUser.Title);
                    npgSqlCommand.Parameters.AddWithValue("@Role", (Int32)inputUser.Role);
                    npgSqlCommand.Parameters.AddWithValue("@Coins", inputUser.Coins);
                    npgSqlCommand.Parameters.AddWithValue("@Img", inputUser.Image);
                    npgSqlCommand.Parameters.AddWithValue("@ID", inputUser.Id);
                    npgSqlCommand.Parameters.AddWithValue("@GroupID", inputUser.GroupId);

                    var changedRows = npgSqlCommand.ExecuteNonQuery();

                    if (changedRows != 1)
                    {
                        throw new Exception($"changedRows: {changedRows}.");
                    }
                }
            }
            finally
            {
                logger.Trace("UpdateUser leave.");
            }
        }

        /// <summary>
        /// Удаление пользователей
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        public static void RemoveUser(Guid groupId, Guid userId)
        {
            try
            {
                logger.Trace("RemoveUser started.");
                logger.Trace($"groupId: {groupId.ToString()}");
                logger.Trace($"userId: {userId.ToString()}");

                var query = QueriesInfo.GetQueryTemplate("RemoveUser");

                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@GroupID", groupId);
                    npgSqlCommand.Parameters.AddWithValue("@ID", userId);
                    npgSqlCommand.Parameters.AddWithValue("@RemovedStatus", (int)UserStatus.Removed);

                    var removedRows = npgSqlCommand.ExecuteNonQuery();

                    if (removedRows != 1)
                    {
                        throw new Exception($"removedRows != 1. removedRows: {removedRows}.");
                    }
                }
            }
            finally
            {
                logger.Trace("RemoveUser leave.");
            }
        }

        /// <summary>
        /// Получение всех пользователей группы
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static List<User> GetAllUsers(Guid groupId)
        {
            try
            {
                logger.Trace("GetAllUsers started.");
                logger.Trace($"groupId: {groupId.ToString()}");

                var query = QueriesInfo.GetQueryTemplate("GetAllUsers");

                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@GroupID", groupId);
                    npgSqlCommand.Parameters.AddWithValue("@RemovedStatus", (int)UserStatus.Removed);

                    using (NpgsqlDataReader DR = npgSqlCommand.ExecuteReader())
                    {
                        if (!DR.HasRows)
                        {
                            throw new Exception("HasRows: false.");
                        }

                        List<User> allUsers = new List<User>();

                        while (DR.Read())
                        {
                            User selectedUser = new User();

                            selectedUser.Id = DR.GetGuid(0);
                            selectedUser.GroupId = DR.GetGuid(1);
                            selectedUser.Name = DR.GetString(2);
                            selectedUser.Title = DR.GetString(3);
                            selectedUser.Role = (RoleTypes)DR.GetInt32(4);
                            selectedUser.Coins = DR.GetInt32(5);
                            selectedUser.Image = DR.GetString(6);

                            logger.Trace($"id: {selectedUser.Id}");
                            logger.Trace($"groupId: {selectedUser.GroupId}");
                            logger.Trace($"name: {selectedUser.Name}");
                            logger.Trace($"title: {selectedUser.Title}");
                            logger.Trace($"role: {selectedUser.Role.ToString()}");
                            logger.Trace($"coins: {selectedUser.Coins.ToString()}");
                            logger.Trace($"Image: {selectedUser.Image}");

                            allUsers.Add(selectedUser);
                        }

                        return allUsers;
                    }
                }
            }
            finally
            {
                logger.Trace("GetAllUsers leave.");
            }
        }

        /// <summary>
        /// Получение указанных пользователей
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="usersId"></param>
        /// <returns></returns>
        public static List<User> GetUsersById(Guid groupId, List<Guid> usersId)
        {
            try
            {
                logger.Trace("GetUsersById started.");
                logger.Trace($"groupId: {groupId.ToString()}");
                logger.Trace($"usersId.Count: {usersId.Count.ToString()}");

                foreach (var userId in usersId)
                {
                    logger.Trace($"userId: {userId.ToString()}");
                }

                var query = QueriesInfo.GetQueryTemplate("GetUsersById");

                var additionalParams = DBHelper.MakeSQLMultipleParamString("ID", usersId.Count, " OR ", false);

                query = string.Format(query, additionalParams);

                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@GroupID", groupId);

                    for (int i = 0; i < usersId.Count; i++)
                    {
                        npgSqlCommand.Parameters.AddWithValue(string.Format("ID{0}", i), usersId[i]);
                    }

                    using (NpgsqlDataReader DR = npgSqlCommand.ExecuteReader())
                    {
                        List<User> selectedUsers = new List<User>();

                        if (DR.HasRows)
                        {
                            while (DR.Read())
                            {
                                User selectedUser = new User();

                                selectedUser.Id = DR.GetGuid(0);
                                selectedUser.GroupId = DR.GetGuid(1);
                                selectedUser.Name = DR.GetString(2);
                                selectedUser.Title = DR.GetString(3);
                                selectedUser.Role = (RoleTypes)DR.GetInt32(4);
                                selectedUser.Coins = DR.GetInt32(5);
                                selectedUser.Image = DR.GetString(6);
                                selectedUser.Status = (UserStatus)DR.GetInt32(7);

                                logger.Trace($"id: {selectedUser.Id}");
                                logger.Trace($"groupId: {selectedUser.GroupId}");
                                logger.Trace($"name: {selectedUser.Name}");
                                logger.Trace($"title: {selectedUser.Title}");
                                logger.Trace($"role: {selectedUser.Role.ToString()}");
                                logger.Trace($"coins: {selectedUser.Coins.ToString()}");
                                logger.Trace($"Image: {selectedUser.Image}");

                                selectedUsers.Add(selectedUser);
                            }
                        }

                        return selectedUsers;
                    }
                }
            }
            finally
            {
                logger.Trace("GetUsersById leave.");
            }
        }

        public static Int64 GetRoleUsersCount(Guid groupId, RoleTypes role)
        {
            try
            {
                logger.Trace("GetRoleUsersCount started.");

                logger.Trace($"groupId: {groupId.ToString()}");

                var query = QueriesInfo.GetQueryTemplate("GetRoleUsersCount");
                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@GroupID", groupId);
                    npgSqlCommand.Parameters.AddWithValue("@Role", (int)role);
                    npgSqlCommand.Parameters.AddWithValue("@RemovedStatus", (int)UserStatus.Removed);

                    var count = (Int64)npgSqlCommand.ExecuteScalar();

                    return count;
                }
            }
            finally
            {
                logger.Trace("GetRoleUsersCount leave.");
            }
        }

        public static Int64 GetAllUsersCount(Guid groupId)
        {
            try
            {
                logger.Trace("GetAllUsersCount started.");

                logger.Trace($"groupId: {groupId.ToString()}");

                var query = QueriesInfo.GetQueryTemplate("GetAllUsersCount");
                logger.Trace($"Query: {query}");

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@GroupID", groupId);
                    npgSqlCommand.Parameters.AddWithValue("@RemovedStatus", (int)UserStatus.Removed);

                    var count = (Int64)npgSqlCommand.ExecuteScalar();

                    return count;
                }
            }
            finally
            {
                logger.Trace("GetAllUsersCount leave.");
            }
        }

        #region Административные функции
        /// <summary>
        /// Удаление временных записей с истекшим сроком действия кода подтверждения
        /// </summary>
        public static void RemoveOldTempAccounts()
        {
            try
            {
                var query = QueriesInfo.GetQueryTemplate("RemoveOldTempAccounts");

                var lastDate = DateTime.UtcNow.AddSeconds((-1) * Settings.Current[Settings.Name.Account.TempAccountTTL, CommonData.confirmAccountPeriod]);

                using(NpgsqlConnection dbConnection = DBManager.GetConnection())
				using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@lastDate", lastDate);

                    var removedRows = npgSqlCommand.ExecuteNonQuery();
                }
            }
            finally
            {

            }
        }

        public static List<Guid> GetUsersWithOldTokens()
        {
            try
            {
                logger.Trace("GetUsersWithOldTokens started.");

                var query = QueriesInfo.GetQueryTemplate("GetUsersWithOldTokens");

                logger.Trace($"Query: {query}");

                var lastDate = DateTime.UtcNow.AddSeconds((-1) * Settings.Current[Settings.Name.Account.TokenTTL, CommonData.tokenLifeTimePeriod]);

                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@lastDate", lastDate);

                    using (NpgsqlDataReader DR = npgSqlCommand.ExecuteReader())
                    {
                        List<Guid> usersWithOldTokens = new List<Guid>();

                        while (DR.Read())
                        {
                            Guid selectedUser = Guid.Empty;

                            selectedUser = DR.GetGuid(0);

                            usersWithOldTokens.Add(selectedUser);
                        }

                        return usersWithOldTokens;
                    }
                }
            }
            finally
            {
                logger.Trace("GetUsersWithOldTokens leave.");
            }
        }

        /// <summary>
        /// Очистка истекших токенов-авторизации
        /// </summary>
        public static void RemoveOldTokens()
        {
            try
            {
                
                var query = QueriesInfo.GetQueryTemplate("RemoveOldTokens");
                
                var lastDate = DateTime.UtcNow.AddSeconds((-1) * Settings.Current[Settings.Name.Account.TokenTTL, CommonData.tokenLifeTimePeriod]);


                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
				using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    npgSqlCommand.Parameters.AddWithValue("@lastDate", lastDate);

                    var removedRows = npgSqlCommand.ExecuteNonQuery();                    
                }
            }
            finally
            {
               
            }
        }

        /// <summary>
        /// Обнуление счётчика FailedLoginTryings
        /// </summary>
        public static void ResetAllFailedLoginTryings()
        {
            try
            {
                var query = QueriesInfo.GetQueryTemplate("ResetAllFailedLoginTryings");
                                
                using (NpgsqlConnection dbConnection = DBManager.GetConnection())
                using (NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, dbConnection))
                {
                    var removedRows = npgSqlCommand.ExecuteNonQuery();
                }
            }
            finally
            {

            }
        }
        #endregion
    }
}
