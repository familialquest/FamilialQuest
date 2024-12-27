using CommonLib;
using AccountService.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Net.Mail;
using CommonTypes;
using static CommonTypes.User;
using System.Net.Http;
using System.Net;
using CommonRoutes;
using System.Text;
using static CommonLib.FQServiceException;
using Microsoft.AspNetCore.Http;

namespace AccountService.Services
{
    /// <summary>
    /// Вся работа по пользователям и их аккаунтам
    /// </summary>
    public class AccountServices : IAccountServices
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Default constructor with HTTPContext
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        public AccountServices(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Дессериализация Account-а
        /// </summary>
        /// <param name="inputParams">Json объект</param>
        /// <returns>Экземпляр Account</returns>
        public Account GetAccountFromPostData(object inputParams)
        {
            try
            {
                logger.Trace("GetAccountFromPostData started.");

                Account inputAccount = new Account(true);
                inputAccount = JsonConvert.DeserializeObject<Account>(inputParams.ToString());
                
                logger.Trace($"userId: {inputAccount.userId.ToString()}");
                logger.Trace($"failedLoginTryings: {inputAccount.failedLoginTryings.ToString()}");
                logger.Trace($"login: {inputAccount.Login}");
                logger.Trace($"email: {inputAccount.Email}");
                logger.Trace($"isMain: {inputAccount.isMain.ToString()}");
                logger.Trace($"lastAction: {inputAccount.LastAction.ToString()}");
                logger.Trace($"PasswordHashNew: {inputAccount.PasswordHashNew}");
                logger.Trace($"PasswordHashCurrent: {inputAccount.PasswordHashCurrent}");
                logger.Trace($"Token: {inputAccount.Token}");

                return inputAccount;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                logger.Trace("GetAccountFromPostData leave.");
            }
        }

        /// <summary>
        /// Дессериализация User 
        /// </summary>
        /// <param name="inputParams">Json</param>
        /// <returns>User</returns>
        public User GetUserFromPostData(object inputParams)
        {
            try
            {
                logger.Trace("GetUserFromPostData started.");

                User inputUser = new User(true);
                inputUser = JsonConvert.DeserializeObject<User>(inputParams.ToString());

                logger.Trace($"id: {inputUser.Id.ToString()}");
                logger.Trace($"groupId: {inputUser.GroupId.ToString()}");
                logger.Trace($"name: {inputUser.Name}");
                logger.Trace($"title: {inputUser.Title}");
                logger.Trace($"role: {inputUser.Role.ToString()}");
                logger.Trace($"coins: {inputUser.Coins.ToString()}");
                logger.Trace($"Image: {inputUser.Image}");

                return inputUser;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                logger.Trace("GetUserFromPostData leave.");
            }
        }

        #region RegistrationOperations

        /// <summary>
        /// Создание временной учетной записи при запросе на регистрацию.
        /// Возврат ошибки только в случае EmptyRequiredField - 
        /// во всех иных случаях эмуляция корретной обработки, для того чтобы не сообщать клиенту информацию и наличии в системе логинов
        /// </summary>
        /// <param name="ri">Login, PasswordHash</param>
        public void CreateTempAccount(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("CreateTempAccount started.");

                logger.Trace($"Login: {ri.Credentials.Login}");
                logger.Trace($"PasswordHash: {ri.Credentials.PasswordHash}");                

                if (string.IsNullOrEmpty(ri.Credentials.Login) ||
                    string.IsNullOrEmpty(ri.Credentials.PasswordHash))
                {
                    throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
                }

                if (!Account.VerifyLoginAsEmail(ri.Credentials.Login))
                {
                    throw new FQServiceException(FQServiceExceptionType.IncorrectLoginFormat);
                }

                var confirmCode = GenerateConfirmCode();
                logger.Trace($"ConfirmCode: {confirmCode}");

                //Проверка существования аккаунта с таким именем
                if (DBWorker.CheckAccountExist(ri.Credentials.Login))
                {
                    throw new Exception("Ошибка: AccountExist");
                }

                //Проверка существования временного аккаунт с таким именем
                if (DBWorker.CheckTempAccountExist(ri.Credentials.Login))
                {
                    //Удаление временного аккаунта
                    DBWorker.RemoveTempAccount(ri.Credentials.Login);
                }

                //Добавление записи временного аккаунта
                Account tempAccount = new Account(true);
                tempAccount.Email = ri.Credentials.Login;
                tempAccount.PasswordHashNew = GetPasswordHash(ri.Credentials.Login, ri.Credentials.PasswordHash);
                tempAccount.ConfirmCode = confirmCode;
                tempAccount.CreationDate = DateTime.UtcNow;
                DBWorker.InsertTempAccount(tempAccount);

                //Отправка кода подтверждения на почту
                FQRequestInfo ri_SendMessage = ri.Clone();
                Mail message_CreateAccountConfirm = new Mail();
                message_CreateAccountConfirm.MessageType = "CreateAccountConfirm";
                message_CreateAccountConfirm.Address = tempAccount.Email;
                message_CreateAccountConfirm.ConfirmCode = tempAccount.ConfirmCode;
                ri_SendMessage.RequestData.actionName = "SendMessage";
                ri_SendMessage.RequestData.postData = JsonConvert.SerializeObject(message_CreateAccountConfirm);
                RouteInfo.RouteToService(ri_SendMessage, _httpContextAccessor);
            }
            catch (FQServiceException fqEx) //Возвращается только ошибка о незаполненных полях. Остальное пользователю (в рамках этой операции) знать не следует
            {
                logger.Error(fqEx);

                if (fqEx.exType == FQServiceExceptionType.EmptyRequiredField)
                {
                    throw;
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                //Логируем и возвращаем, что всё ок
                logger.Error(ex);
                
                return;
            }
            finally
            {
                logger.Trace("CreateTempAccount leave.");
            }
        }


        /// <summary>
        /// Подтверждение операции регистрации главного аккаунта
        /// Возврат ошибки только в случае EmptyRequiredField - 
        /// во всех иных случаях эмуляция WrongConfirmCode, для того чтобы не сообщать клиенту информацию и наличии в системе логинов
        /// </summary>
        /// <param name="ri">Login, ConfirmCode</param>
        public void ConfirmTempAccount(FQRequestInfo ri)
        {
            //Оъявлены здесь, потому что в catch-е возможно потребуются
            Guid groupId = Guid.Empty;
            Guid userId = Guid.Empty;
            Account tempAcc = null;

            try
            {
                logger.Trace($"Login: {ri.Credentials.Login}.");

                var confirmCode = ri.RequestData.postData.ToString().ToLower();
                logger.Trace($"ConfirmCode: {confirmCode}.");

                if (string.IsNullOrWhiteSpace(ri.Credentials.Login) || 
                    string.IsNullOrWhiteSpace(confirmCode))
                {
                    throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
                }

                if (!Account.VerifyLoginAsEmail(ri.Credentials.Login))
                {
                    throw new FQServiceException(FQServiceExceptionType.IncorrectLoginFormat);
                }

                //Проверка существования временного аккаунт с таким именем
                if (!DBWorker.CheckTempAccountExist(ri.Credentials.Login))
                {
                    throw new Exception("Ошибка. Временный аккаунт не существует.");
                }

                //Получение временной записи регистрируемого аккаунта
                tempAcc = DBWorker.GetTempAccount(ri.Credentials.Login);                
                logger.Trace($"ConfirmCode from tempAcc: {tempAcc.ConfirmCode}.");

                //Проверка соответствия кода подтверждения
                if (!string.IsNullOrEmpty(tempAcc.ConfirmCode) &&
                    tempAcc.ConfirmCode.Equals(confirmCode))
                {
                    //Всё ок
                    //Формирование и отправка запроса на создание новой группы
                    FQRequestInfo ri_CreateGroup = ri.Clone();
                    ri_CreateGroup.RequestData.actionName = "CreateGroup";
                    ri_CreateGroup.RequestData.postData = string.Empty;
                    var responseCreateGroup = RouteInfo.RouteToService(ri_CreateGroup, _httpContextAccessor);

                    //Получение из ответа гуида новой группы
                    if (string.IsNullOrEmpty(responseCreateGroup))
                    {
                        throw new Exception("Ошибка: string.IsNullOrEmpty(responseCreateGroup)");
                    }

                    groupId = JsonConvert.DeserializeObject<Guid>(responseCreateGroup);

                    if (groupId == Guid.Empty)
                    {
                        throw new Exception("Ошибка: groupId == Guid.Empty");
                    }

                    logger.Trace($"groupId: {groupId.ToString()}");

                    //Создание пользователя
                    userId = Guid.NewGuid();
                    var createdUser = new User(true);
                    createdUser.Id = userId;
                    createdUser.GroupId = groupId;
                    createdUser.Name = tempAcc.Email.Split("@")[0];
                    createdUser.Title = "Основатель Королевства";
                    createdUser.Role = RoleTypes.Parent;
                    DBWorker.CreateUser(createdUser);

                    //Создание аккаунта
                    tempAcc.Login = tempAcc.Email;
                    tempAcc.isMain = true;
                    tempAcc.userId = createdUser.Id;
                    tempAcc.CreationDate = DateTime.UtcNow;
                    DBWorker.InsertAccount(tempAcc);
                    DBWorker.RemoveTempAccount(tempAcc.Email);

                    //Запись HistoryEvent-а
                    try
                    {
                        FQRequestInfo ri_CreateHistoryEvent = ri.Clone();

                        ri_CreateHistoryEvent._User.Id = userId;
                        ri_CreateHistoryEvent._User.GroupId = groupId;

                        List<Guid> availableFor = new List<Guid>();

                        HistoryEvent he = new HistoryEvent(HistoryEvent.ItemTypeEnum.Group, HistoryEvent.MessageTypeEnum.Group_Created, HistoryEvent.VisabilityEnum.Parents, groupId, availableFor, userId);

                        ri_CreateHistoryEvent.RequestData.actionName = "CreateHistoryEvent";
                        ri_CreateHistoryEvent.RequestData.postData = JsonConvert.SerializeObject(he);

                        RouteInfo.RouteToService(ri_CreateHistoryEvent, _httpContextAccessor);
                    }
                    catch (Exception)
                    {
                        //Если по какой-то причине не удалось записать HistoryEvent - не повод  прерывать операцию и кидать пользователю Error.
                    }

                    //Предзаполнение - создадим несколько простых заданий, чтобы уже были объявлены для примера                    
                    try
                    {
                        FQRequestInfo ri_CreateStartingTasks = ri.Clone();
                        ri_CreateStartingTasks._Account = tempAcc;
                        ri_CreateStartingTasks._User = createdUser;
                        ri_CreateStartingTasks._Group = new Group(true);
                        ri_CreateStartingTasks._Group.Id = createdUser.GroupId;
                        ri_CreateStartingTasks.RequestData.actionName = "CreateGroupStartingTasks";
                        ri_CreateStartingTasks.RequestData.postData = string.Empty;
                        RouteInfo.RouteToService(ri_CreateStartingTasks, _httpContextAccessor);
                    }
                    catch
                    {
                        //Если по какой-то причине не удалось создать стартовые примеры задач - не повод  прерывать операцию и кидать пользователю Error.
                    }
                }
                else
                {
                    throw new FQServiceException(FQServiceExceptionType.WrongConfirmCode);
                }                                            
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);

                //Если падение, нужно подчистить висяки
                if (groupId != Guid.Empty)
                {
                    //удаление группы
                    try
                    {
                        FQRequestInfo ri_RemoveGroup = ri.Clone();
                        ri_RemoveGroup.RequestData.actionName = "RemoveGroup";
                        ri_RemoveGroup.RequestData.postData = groupId.ToString();
                        RouteInfo.RouteToService(ri_RemoveGroup, _httpContextAccessor);
                    }
                    catch { }                    
                }

                if (userId != Guid.Empty)
                {
                    //удаление пользователя
                    try
                    {
                        DBWorker.RemoveUser(groupId, userId);
                    }
                    catch { }
                }

                if (tempAcc != null)
                {
                    //удаление временного аккаунта
                    try
                    {
                        DBWorker.RemoveTempAccount(tempAcc.Email);
                    }
                    catch { }
                }

                if (fqEx.exType == FQServiceExceptionType.EmptyRequiredField)
                {
                    throw;
                }
                else
                {
                    throw new FQServiceException(FQServiceExceptionType.WrongConfirmCode);
                }
            }
            catch (Exception ex)
            {
                //Если падение, нужно подчистить висяки

                if (groupId != Guid.Empty)
                {
                    //удаление группы
                    try
                    {
                        FQRequestInfo ri_RemoveGroup = ri.Clone();
                        ri_RemoveGroup.RequestData.actionName = "RemoveGroup";
                        ri_RemoveGroup.RequestData.postData = groupId.ToString();
                        RouteInfo.RouteToService(ri_RemoveGroup, _httpContextAccessor);
                    }
                    catch { }
                }

                if (userId != Guid.Empty)
                {
                    //удаление пользователя
                    try
                    {
                        DBWorker.RemoveUser(groupId, userId);
                    }
                    catch { }
                }

                if (tempAcc != null)
                {
                    //удаление временного аккаунта
                    try
                    {
                        DBWorker.RemoveTempAccount(tempAcc.Email);
                    }
                    catch { }
                }
                
                //Логируем и возвращаем, что некорректный код подтверждения. Детали для безопасности клиенту не возвращаются
                logger.Error(ex);

                throw new FQServiceException(FQServiceExceptionType.WrongConfirmCode);
            }
            finally
            {
                logger.Trace("ConfirmTempAccount leave.");
            }
        }

        /// <summary>
        /// Создание временной учетной записи при запросе на сброс пароля
        /// Возврат ошибки только в случае EmptyRequiredField - 
        /// во всех иных случаях эмуляция корретной обработки, для того чтобы не сообщать клиенту информацию и наличии в системе логинов
        /// </summary>
        /// <param name="ri">Login, PasswordHash</param>
        public void ResetPasswordCreateTempAccount(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("ResetPasswordCreateTempAccount started.");

                logger.Trace($"Login: {ri.Credentials.Login}");
                logger.Trace($"PasswordHash: {ri.Credentials.PasswordHash}");

                var confirmCode = GenerateConfirmCode();
                logger.Trace($"ConfirmCode: {confirmCode}");
                
                if (string.IsNullOrEmpty(ri.Credentials.Login) ||
                    string.IsNullOrEmpty(ri.Credentials.PasswordHash))
                {                    
                    throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
                }

                if (!Account.VerifyLoginAsEmail(ri.Credentials.Login))
                {
                    throw new FQServiceException(FQServiceExceptionType.IncorrectLoginFormat);
                }

                //Проверка существования аккаунта с таким именем
                if (!DBWorker.CheckAccountExist(ri.Credentials.Login))
                {
                    throw new FQServiceException(FQServiceExceptionType.ItemNotFound);
                }

                //Получение существующего аккаунта                
                Account account = DBWorker.GetAccount(ri.Credentials.Login);                

                logger.Trace($"userId: {account.userId.ToString()}");
                logger.Trace($"failedLoginTryings: {account.failedLoginTryings.ToString()}");
                logger.Trace($"login: {account.Login}");
                logger.Trace($"email: {account.Email}");
                logger.Trace($"isMain: {account.isMain.ToString()}");
                logger.Trace($"lastAction: {account.LastAction.ToString()}");
                logger.Trace($"PasswordHashNew: {account.PasswordHashNew}");
                logger.Trace($"PasswordHashCurrent: {account.PasswordHashCurrent}");
                logger.Trace($"Token: {account.Token}");

                var selectingUsers = new List<Guid>();
                selectingUsers.Add(account.userId);

                ri._Account = account;

                //Получение целевого пользователя
                User inputAccount_User = GetUsersById(ri, selectingUsers).FirstOrDefault();
                if (inputAccount_User == null || inputAccount_User.Status == UserStatus.Removed)
                {
                    throw new FQServiceException(FQServiceExceptionType.ItemNotFound);
                }


                //Формирование временной записи об аккаунте для сброса пароля
                Account tempAccount = new Account(true);
                tempAccount.Email = ri.Credentials.Login;
                tempAccount.PasswordHashNew = GetPasswordHash(ri.Credentials.Login, ri.Credentials.PasswordHash);
                tempAccount.ConfirmCode = confirmCode;
                tempAccount.CreationDate = DateTime.UtcNow;

                //Проверка существования временного аккаунт с таким именем
                if (DBWorker.CheckTempAccountExist(ri.Credentials.Login))
                {
                    //Удаление временной записи
                    DBWorker.RemoveTempAccount(ri.Credentials.Login);
                }

                //Добавление временной записи для сброса пароля
                DBWorker.InsertTempAccount(tempAccount);

                //Отправка кода подтверждения на почту
                FQRequestInfo ri_SendMessage = ri.Clone();
                Mail message_ResetPasswordConfirm = new Mail();
                message_ResetPasswordConfirm.MessageType = "ResetPasswordConfirm";
                message_ResetPasswordConfirm.Address = tempAccount.Email;
                message_ResetPasswordConfirm.ConfirmCode = tempAccount.ConfirmCode;
                ri_SendMessage.RequestData.actionName = "SendMessage";
                ri_SendMessage.RequestData.postData = JsonConvert.SerializeObject(message_ResetPasswordConfirm);

                RouteInfo.RouteToService(ri_SendMessage, _httpContextAccessor);
            }
            catch (FQServiceException fqEx) //Возвращается только ошибка о незаполненных полях. Остальное пользователю (в рамках этой операции) знать не следует
            {
                logger.Error(fqEx);

                if (fqEx.exType == FQServiceExceptionType.EmptyRequiredField)
                {
                    throw;
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                //Логируем и возвращаем, что всё ок
                logger.Error(ex);

                return;
            }
            finally
            {
                logger.Trace("ResetPasswordCreateTempAccount leave.");
            }
        }

        /// <summary>
        /// Подтверждение операции сброса пароля.
        /// Возврат ошибки только в случае EmptyRequiredField - 
        /// во всех иных случаях эмуляция WrongConfirmCode, для того чтобы не сообщать клиенту информацию и наличии в системе логинов
        /// </summary>
        /// <param name="ri">Login, ConfirmCode</param>
        public void ResetPasswordConfirmTempAccount(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("ResetPasswordConfirmTempAccount started.");

                
                logger.Trace($"Login: {ri.Credentials.Login}");

                var confirmCode = ri.RequestData.postData.ToString().ToLower();
                logger.Trace($"ConfirmCode: {confirmCode}");

                if (string.IsNullOrEmpty(ri.Credentials.Login) ||
                    string.IsNullOrEmpty(confirmCode))
                {
                    throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
                }

                if (!Account.VerifyLoginAsEmail(ri.Credentials.Login))
                {
                    throw new FQServiceException(FQServiceExceptionType.IncorrectLoginFormat);
                }

                //Получение существующего аккаунта
                Account constAcc = DBWorker.GetAccount(ri.Credentials.Login);

                //Проверка типа аккаунта
                if (!constAcc.isMain)
                {
                    throw new Exception("Ошибка: ConstAccNotIsMain");
                }

                //Проверка существования временного аккаунт с таким именем
                if (!DBWorker.CheckTempAccountExist(ri.Credentials.Login))
                {
                    throw new Exception("Ошибка: TempAccountNotExist");
                }

                //Получение временной записи аккаунта
                Account tempAcc = DBWorker.GetTempAccount(ri.Credentials.Login);
                logger.Trace($"ConfirmCode from UserCredentials: {confirmCode}.");
                logger.Trace($"ConfirmCode from tempAcc: {tempAcc.ConfirmCode}.");
                logger.Trace($"Email from tempAcc: {tempAcc.Email}.");
                logger.Trace($"PasswordHashNew from tempAcc: {tempAcc.PasswordHashNew}.");

                //Проверка соответствия кода подтверждения
                if (!string.IsNullOrEmpty(tempAcc.ConfirmCode) &&
                    tempAcc.ConfirmCode.Equals(confirmCode))
                {
                    constAcc.PasswordHashNew = tempAcc.PasswordHashCurrent;
                    constAcc.Token = string.Empty;

                    //Обновленение пароля записи аккаунта
                    DBWorker.ResetPassword(constAcc);

                    DBWorker.RemoveAccessData(constAcc.userId);

                    //Удаление временной записи
                    DBWorker.RemoveTempAccount(tempAcc.Email);
                }
                else
                {
                    throw new FQServiceException(FQServiceExceptionType.WrongConfirmCode);
                }
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);

                if (fqEx.exType == FQServiceExceptionType.EmptyRequiredField)
                {
                    throw;
                }
                else
                {
                    throw new FQServiceException(FQServiceExceptionType.WrongConfirmCode);
                }
            }
            catch (Exception ex)
            {
                //Логируем и возвращаем, что некорректный код подтверждения. Детали для безопасности клиенту не возвращаются
                logger.Error(ex);

                throw new FQServiceException(FQServiceExceptionType.WrongConfirmCode);
            }
            finally
            {
                logger.Trace("ResetPasswordConfirmTempAccount leave.");
            }
        }

        /// <summary>
        /// //Генерация проверочного кода
        /// </summary>
        /// <returns>Проверочный код 6 цифр</returns>
        private string GenerateConfirmCode()
        {
            try
            {
                logger.Trace("GenerateConfirmCode started.");

                string code = "";

                Random rnd = new Random();

                int[] digitNumbers = { 0, 0 };

                digitNumbers[0] = rnd.Next(0, 5);
                digitNumbers[1] = rnd.Next(0, 5);

                for (int i = 0; i < 6; i++)
                {
                    char c;

                    if (i == digitNumbers[0] || i == digitNumbers[1])
                    {
                        c = (char)rnd.Next(48, 57);
                    }
                    else
                    {
                        int value = rnd.Next(65, 90);

                        if (value == 73 || value == 79)
                        {
                            value++;
                        }

                        c = (char)value;
                    }

                    code += c;
                }

                logger.Trace($"code: {code}.");

                System.Threading.Thread.Sleep(10);

                return code.ToLower();
            }           
            finally
            {
                logger.Trace("GenerateConfirmCode leave.");
            }
        }


        /// <summary>
        /// //Генерация FQ-тэга, для уникальной идентификации логина добавленных пользователей
        /// </summary>
        /// <returns>Код numCount цифр</returns>
        private string GenerateFQTag(int numCount)
        {
            try
            {
                logger.Trace("GenerateFQTag started.");

                string code = "";

                Random rnd = new Random();
                                
                for (int i = 0; i < numCount; i++)
                {
                    char c;

                    c = (char)rnd.Next(48, 57);

                    code += c;
                }

                logger.Trace($"code: {code}.");

                return code.ToLower();
            }
            finally
            {
                logger.Trace("GenerateFQTag leave.");
            }
        }

        private string GetFQInnerTag(string newUserLogin)
        {
            try
            {
                logger.Trace("GetFQInnerTag started.");

                string newFQInnerLogin = string.Empty;

                for (int i = 4; i < 7; i++)
                {
                    //Для ускорения возьмем половину: если не попали в прогал, 
                    //перейдем к другому порядку, где шанс попасть на незанятый код выше.
                    //Порядок "забивания" кодов для нас роли не играет.

                    int tryingsCount = Convert.ToInt32(Math.Pow(10, i)) / 2;

                    for (int j = 0; j < tryingsCount; j++)
                    {
                        //Генерим код заданной длины
                        string fqTag = GenerateFQTag(i);

                        newFQInnerLogin = string.Format("{0}#{1}", newUserLogin, fqTag);

                        if (!DBWorker.CheckAccountExist(newFQInnerLogin))
                        {
                            return fqTag;
                        }
                    }
                }

                throw new Exception("GetFQInnerTag: не удалось сгенерировать код.");
            }
            finally
            {
                logger.Trace("GetFQInnerTag leave.");
            }
        }
        #endregion

        #region AuthOperations
        /// <summary>
        /// Аутентификация клиента
        /// </summary>
        /// <param name="ri">FQRequestInfo с Login и PasswordHash или tokenB64</param>
        /// <returns>FQResponseInfo c userId и tokenB64</returns>
        public FQResponseInfo Auth(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("Auth started.");

                logger.Trace($"Login: {ri.Credentials.Login}.");
                logger.Trace($"tokenB64: {ri.Credentials.tokenB64}.");
                logger.Trace($"PasswordHash: {ri.Credentials.PasswordHash}.");
                logger.Trace($"DeviceId: {ri.Credentials.DeviceId}.");

                if (string.IsNullOrWhiteSpace(ri.Credentials.Login) || 
                    (string.IsNullOrWhiteSpace(ri.Credentials.tokenB64) && string.IsNullOrWhiteSpace(ri.Credentials.PasswordHash)) ||
                    string.IsNullOrWhiteSpace(ri.Credentials.DeviceId))
                {
                    throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
                }
                
                FQRequestInfo authorizedUser = new FQRequestInfo();

                //Проверка типа аутентификации
                if (!string.IsNullOrWhiteSpace(ri.Credentials.tokenB64))
                {
                    authorizedUser = VerifyToken(ri);
                }
                else
                {
                    authorizedUser = VerifyPassword(ri);
                }

                //Формирование ответа
                FQResponseInfo response = new FQResponseInfo(authorizedUser);

                return response;
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);

                if (fqEx.exType == FQServiceExceptionType.EmptyRequiredField)
                {
                    throw;
                }
                else
                {
                    throw new FQServiceException(FQServiceExceptionType.AuthError);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                logger.Trace("Auth leave.");
            }
        }

        /// <summary>
        /// Логаут
        /// </summary>
        /// <param name="ri">FQRequestInfo с Login и tokenB64</param>
        /// <returns></returns>
        public void Logout(FQRequestInfo ri)
        {
            try
            {               
                RemoveAccessData(ri, ri._User.Id);
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                logger.Trace("Logout leave.");
            }
        }

        /// <summary>
        /// Проверка токена сессии
        /// </summary>
        /// <param name="ri">Login, token</param>
        /// <returns>FQRequestInfo с заполненным контекстом пользователя</returns>
        private FQRequestInfo VerifyToken(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("VerifyToken started.");

                logger.Trace($"Login: {ri.Credentials.Login}");
                logger.Trace($"tokenB64: {ri.Credentials.tokenB64}");
                logger.Trace($"DeviceId: {ri.Credentials.DeviceId}.");

                if (string.IsNullOrEmpty(ri.Credentials.Login) || 
                    string.IsNullOrEmpty(ri.Credentials.tokenB64) ||
                    string.IsNullOrEmpty(ri.Credentials.DeviceId))
                {
                    throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
                }

                //Получение аккаунта
                var account = DBWorker.GetAccount(ri.Credentials.Login);
                ri._Account = account;

                //Получение пользака
                List<Guid> usersForSelect = new List<Guid>();
                usersForSelect.Add(ri._Account.userId);
                var user = GetUsersById(ri, usersForSelect).FirstOrDefault();

                if (user == null || user.Status == User.UserStatus.Removed)
                {
                    throw new FQServiceException(FQServiceExceptionType.ItemNotFound);
                }

                //Сохранение в FQRequestInfo полученной информации
                FQRequestInfo authorizedUser = ri.Clone();
                authorizedUser._Account = account;
                authorizedUser._User = user;

                //Получение группы
                var request_GetGroup = authorizedUser.Clone();
                request_GetGroup.RequestData.actionName = "GetGroup";
                var response_GetGroup = RouteInfo.RouteToService(request_GetGroup, _httpContextAccessor);
                authorizedUser._Group = JsonConvert.DeserializeObject<Group>(response_GetGroup);

                logger.Trace($"Login: {authorizedUser._Account.Login}");
                logger.Trace($"userId: {authorizedUser._Account.userId}");
                logger.Trace($"tokenB64: {authorizedUser._Account.Token}");
                logger.Trace($"DeviceId: {authorizedUser._Account.DeviceId}");

                var checkingDeviceId = GetDeviceIdHash(ri.Credentials.Login, ri.Credentials.DeviceId);

                //Проверка корректности данных
                if (!string.IsNullOrEmpty(authorizedUser._Account.Login) &&
                    !string.IsNullOrEmpty(authorizedUser._Account.Token) &&
                    authorizedUser._Account.userId != Guid.Empty &&
                    ri.Credentials.tokenB64 == authorizedUser._Account.Token &&
                    checkingDeviceId == authorizedUser._Account.DeviceId &&
                    ri.Credentials.Login == authorizedUser._Account.Login)
                {
                    //Присвоение аккаунту нового токена
                    authorizedUser._Account.Token = CreateToken();                    
                    DBWorker.SetAccessData(authorizedUser._Account);

                    return authorizedUser;
                }
                else
                {
                    throw new FQServiceException(FQServiceExceptionType.AuthError);
                }
            }            
            finally
            {
                logger.Trace("VerifyToken leave.");
            }
        }

        /// <summary>
        /// Проверка хэша пароля
        /// </summary>
        /// <param name="ri"></param>
        /// <returns>FQRequestInfo с заполненным контекстом пользователя</returns>
        private FQRequestInfo VerifyPassword(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("VerifyPassword started.");
               
                logger.Trace($"Login: {ri.Credentials.Login}");
                logger.Trace($"PasswordHash: {ri.Credentials.PasswordHash}");
                logger.Trace($"DeviceId: {ri.Credentials.DeviceId}.");

                if (string.IsNullOrEmpty(ri.Credentials.Login) || 
                    string.IsNullOrEmpty(ri.Credentials.PasswordHash) ||
                    string.IsNullOrEmpty(ri.Credentials.DeviceId))
                {
                    throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
                }

                //Получение аккаунта
                var account = DBWorker.GetAccount(ri.Credentials.Login);
                ri._Account = account;

                //Если было 5 фейлов подряд, выкидываем ошибку авторизации в любом случае
                if (account.failedLoginTryings >= 5)
                {
                    throw new FQServiceException(FQServiceExceptionType.AuthError);
                }

                //Получение пользака
                List<Guid> usersForSelect = new List<Guid>();
                usersForSelect.Add(ri._Account.userId);
                var user = GetUsersById(ri, usersForSelect).FirstOrDefault();

                if (user == null || user.Status == User.UserStatus.Removed)
                {
                    throw new FQServiceException(FQServiceExceptionType.ItemNotFound);
                }

                //Сохранение в FQRequestInfo полученной информации
                FQRequestInfo authorizedUser = ri.Clone();
                authorizedUser._Account = account;
                authorizedUser._User = user;

                //Получение группы
                var request_GetGroup = authorizedUser.Clone();
                request_GetGroup.RequestData.actionName = "GetGroup";
                var response_GetGroup = RouteInfo.RouteToService(request_GetGroup, _httpContextAccessor);
                authorizedUser._Group = JsonConvert.DeserializeObject<Group>(response_GetGroup);

                logger.Trace($"Login: {authorizedUser._Account.Login}");
                logger.Trace($"userId: {authorizedUser._Account.userId}");
                logger.Trace($"tokenB64: {authorizedUser._Account.Token}");

                var checkingPasswordHash = GetPasswordHash(ri.Credentials.Login, ri.Credentials.PasswordHash);
                
                //Проверка корректности данных
                if (!string.IsNullOrEmpty(authorizedUser._Account.Login) &&
                    !string.IsNullOrEmpty(authorizedUser._Account.PasswordHashCurrent) &&
                    authorizedUser._Account.userId != Guid.Empty &&
                    checkingPasswordHash == authorizedUser._Account.PasswordHashCurrent &&                    
                    ri.Credentials.Login == authorizedUser._Account.Login)
                {
                    var newDeviceId = GetDeviceIdHash(ri.Credentials.Login, ri.Credentials.DeviceId);

                    //Присвоение аккаунту нового токена
                    authorizedUser._Account.Token = CreateToken();
                    authorizedUser._Account.DeviceId = newDeviceId;
                    DBWorker.SetAccessData(authorizedUser._Account, true);

                    DBWorker.SetFailedLoginTryings(ri.Credentials.Login, 0);

                    return authorizedUser;
                }
                else
                {
                    DBWorker.SetFailedLoginTryings(ri.Credentials.Login, account.failedLoginTryings + 1);

                    throw new FQServiceException(FQServiceExceptionType.AuthError);
                }
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex) 
            {
                logger.Error(ex);

                //В любом случае возвращаем юзеру ошибку аутентификации, дабы не раскрывать деталей существования\отсутствия аккаунта
                throw new FQServiceException(FQServiceExceptionType.AuthError);
            }
            finally
            {
                logger.Trace("VerifyPassword leave.");
            }
        }

        /// <summary>
        /// Создание нового токена
        /// </summary>
        /// <returns>token base64</returns>
        private string CreateToken()
        {
            try
            {
                logger.Trace("CreateToken started.");

                byte[] token = new Byte[64];

                using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(token);
                }

                return Convert.ToBase64String(token);
            }
            finally
            {
                logger.Trace("CreateToken leave.");
            }
        }
        
        /// <summary>
        /// Сброс токена сессии и идентификатора устройства для указанного пользователя в группе клиента
        /// </summary>
        private void RemoveAccessData(FQRequestInfo ri, Guid userId)
        {
            try
            {
                logger.Trace("RemoveAccessData started.");

                logger.Trace($"userId: {userId}");

                if (userId == Guid.Empty)
                {
                    throw new Exception("Ошибка: не указан идентификатор пользователя.");
                }

                DBWorker.RemoveAccessData(userId);

                //RemoveRelatedPushSubscriptions
                FQRequestInfo ri_RemoveRelatedPushSubscriptions = ri.Clone();
                ri_RemoveRelatedPushSubscriptions.RequestData.actionName = "UnregisterDeviceInner";

                NotifiedDevice notifiedDevice = new NotifiedDevice()
                {
                    UserId = userId,
                    DeviceId = string.Empty,
                    RegToken = string.Empty
                };

                ri_RemoveRelatedPushSubscriptions.RequestData.postData = notifiedDevice.Serialize();
                RouteInfo.RouteToService(ri_RemoveRelatedPushSubscriptions, _httpContextAccessor);
            }
            finally
            {
                logger.Trace("RemoveAccessData leave.");
            }
        }

        #endregion

        #region UserOperations

        /// <summary>
        /// Изменение своего пароля аутентифицированным клиентом
        /// </summary>
        /// <param name="inputAccount">Account с PasswordHashCurrent и PasswordHashNew</param>
        public void ChangeSelfPassword(FQRequestInfo ri, Account inputAccount)
        {
            try
            {
                logger.Trace("ChangeSelfPassword started.");
               
                logger.Trace($"userId: {ri._Account.userId}");                
                logger.Trace($"PasswordHashCurrent: {inputAccount.PasswordHashCurrent}");
                logger.Trace($"PasswordHashNew: {inputAccount.PasswordHashNew}");

                if (ri._Account.userId == Guid.Empty)
                {
                    throw new Exception("Ошибка: не указан идентификатор пользователя.");
                }
                
                if (string.IsNullOrEmpty(inputAccount.PasswordHashNew) || 
                    string.IsNullOrEmpty(inputAccount.PasswordHashCurrent))
                {
                    throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
                }

                inputAccount.PasswordHashCurrent = GetPasswordHash(ri.Credentials.Login, inputAccount.PasswordHashCurrent);
                inputAccount.PasswordHashNew = GetPasswordHash(ri.Credentials.Login, inputAccount.PasswordHashNew);

                //Проверка статуса полльзователя
                //if (ri._User.Role != RoleTypes.Parent)
                //{
                //    throw new FQServiceException(FQServiceExceptionType.NotEnoughRights);
                //}

                //Получение текущего пароля пользователя
                var currentPassHash = DBWorker.GetUserPasswordHash(ri._Account.userId);

                //Проверка корректности текущего пароля
                if (!inputAccount.PasswordHashCurrent.Equals(currentPassHash))
                {
                    throw new FQServiceException(FQServiceExceptionType.WrongPassword);
                }

                //Изменение текущего пароля
                inputAccount.userId = ri._Account.userId;
                DBWorker.ChangePassword(inputAccount);

                //Сброс текущего токена
                RemoveAccessData(ri, ri._Account.userId);
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                logger.Trace("ChangeSelfPassword leave.");
            }
        }

        /// <summary>
        /// Изменение пароля члена группы
        /// </summary>
        /// <param name="inputAccount">Account с userId и PasswordHashNew целевого пользователя</param>
        public void ChangeGroupUserPassword(FQRequestInfo ri, Account inputAccount)
        {
            try
            {
                logger.Trace("ChangeGroupUserPassword started.");

                logger.Trace($"userId: {ri._Account.userId}");
                logger.Trace($"inputAccount.userId: {inputAccount.userId}");
                logger.Trace($"PasswordHashCurrent_MainUser: {inputAccount.PasswordHashCurrent}");
                logger.Trace($"PasswordHashNew_DestinationSecondaryUser: {inputAccount.PasswordHashNew}");

                if (ri._Account.userId == Guid.Empty)
                {
                    throw new Exception("Ошибка: не указан идентификатор пользователя.");
                }

                if (string.IsNullOrEmpty(inputAccount.PasswordHashCurrent) ||
                    inputAccount.userId == Guid.Empty || 
                    string.IsNullOrEmpty(inputAccount.PasswordHashNew))
                {
                    throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
                }                

                //Проверка статуса пользователя
                if (ri._User.Role != RoleTypes.Parent)
                {
                    throw new FQServiceException(FQServiceExceptionType.NotEnoughRights);
                }

                //Получение целевого пользователя
                List<Guid> requestedUsers = new List<Guid>();
                requestedUsers.Add(inputAccount.userId);
                User inputAccount_User = DBWorker.GetUsersById(ri._User.GroupId, requestedUsers).FirstOrDefault();
                if (inputAccount_User == null ||
                    inputAccount_User.Status == UserStatus.Removed)
                {
                    throw new FQServiceException(FQServiceExceptionType.ItemNotFound);
                }

                //Проверка является ли целевой пользователь родителем
                //if (inputAccount_User.Role == RoleTypes.Parent)
                //{
                //    throw new FQServiceException("Ошибка: целевой пользователь является владельцем группы.");
                //}

                //Получение аккаунта целевого пользователя - необходим логин (как соль) для подсчета хэша
                Account targetUserAccount = DBWorker.GetAccountById(inputAccount.userId);

                inputAccount.PasswordHashCurrent = GetPasswordHash(ri.Credentials.Login, inputAccount.PasswordHashCurrent);
                inputAccount.PasswordHashNew = GetPasswordHash(targetUserAccount.Login, inputAccount.PasswordHashNew);

                //Получение текущего пароля инициатора операции
                var currentPassHash = DBWorker.GetUserPasswordHash(ri._Account.userId);

                //Проверка корректности текущего пароля
                if (!inputAccount.PasswordHashCurrent.Equals(currentPassHash))
                {
                    throw new FQServiceException(FQServiceExceptionType.WrongPassword);
                }

                //Изменение текущего пароля целевого пользователя
                DBWorker.ChangePassword(inputAccount);

                //Сброс текущего токена целевого пользователя
                RemoveAccessData(ri, inputAccount.userId);
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                logger.Trace("ChangeGroupUserPassword leave.");
            }
        }

        //TODO: зарезервирован.
        /// <summary>
        /// Изменение своего пароля аутентифицированным клиентом
        /// </summary>
        /// <param name="inputAccount">Account с PasswordHashCurrent и новым Email</param>
        public void ChangeEmail(FQRequestInfo ri, Account inputAccount)
        {
            //TODO: метод зарезервирован.
            //Весь функционал требует актуализации (как минимум в избыточной информации при возврате ошибок)
            try
            {
                logger.Trace("ChangeEmail started.");

                logger.Trace($"userId: {ri._Account.userId}");
                logger.Trace($"Email: {inputAccount.Email}");
                logger.Trace($"PasswordHashCurrent: {inputAccount.PasswordHashCurrent}");

                if (ri._Account.userId == Guid.Empty)
                {
                    throw new Exception("Ошибка: не указан идентификатор пользователя.");
                }

                if (string.IsNullOrEmpty(inputAccount.PasswordHashCurrent) || 
                    string.IsNullOrEmpty(inputAccount.Email))
                {
                    throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
                }

                if (!Account.VerifyLoginAsEmail(inputAccount.Email))
                {
                    throw new FQServiceException(FQServiceExceptionType.IncorrectLoginFormat);
                }

                //Проверка статуса полльзователя
                if (ri._User.Role != RoleTypes.Parent)
                {
                    throw new FQServiceException(FQServiceExceptionType.NotEnoughRights);
                }

                //Получение текущего пароля
                var currentPassHash = DBWorker.GetUserPasswordHash(ri._Account.userId);

                inputAccount.PasswordHashCurrent = GetPasswordHash(ri.Credentials.Login, inputAccount.PasswordHashCurrent);

                //Проверка корректности текущего пароля
                if (!inputAccount.PasswordHashCurrent.Equals(currentPassHash))
                {
                    throw new FQServiceException(FQServiceExceptionType.WrongPassword);
                }

                //Проверка типа аккаунта
                if (!ri._Account.isMain)
                {
                    throw new FQServiceException(FQServiceExceptionType.DefaultError);
                }

                //Проверка существования аккаунта с таким именем
                if (DBWorker.CheckAccountExist(inputAccount.Email) || 
                    DBWorker.CheckTempAccountExist(inputAccount.Email))
                {
                    throw new FQServiceException(FQServiceExceptionType.DefaultError);
                }

                //Изменение текущего email
                inputAccount.userId = ri._Account.userId;
                DBWorker.ChangeEmail(inputAccount);

                //Сброс текущего токена
                RemoveAccessData(ri, ri._Account.userId);
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                logger.Trace("ChangeEmail leave.");
            }
        }

        /// <summary>
        /// Обновление свойств пользователя
        /// </summary>
        /// <param name="ri"></param>
        public void UpdateUser(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("UpdateUser started.");
                
                User inputUser = GetUserFromPostData(ri.RequestData.postData);

                //Для получения списка полей к апдейту.
                //При формировании клиентом запроса следует учитывать регистрозависимость имен параметров (далее)!
                Dictionary<string, string> inputParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(ri.RequestData.postData.ToString());

                logger.Trace($"inputUser.id: {inputUser.Id}");

                if (inputUser.Id == Guid.Empty)
                {
                    throw new Exception("Ошибка: inputUser.id == Guid.Empty.");
                }

                //Проверка статуса полльзователя
                //if (ri._User.Role != RoleTypes.Parent)
                //{
                //    throw new FQServiceException(FQServiceExceptionType.NotEnoughRights);
                //}
                                
                //Получение обновляемого пользователя
                List<Guid> usersId = new List<Guid>();
                usersId.Add(inputUser.Id);
                var selectedUser = DBWorker.GetUsersById(ri._User.GroupId, usersId).FirstOrDefault();

                if (selectedUser == null || selectedUser.Status == UserStatus.Removed)
                {
                    throw new FQServiceException(FQServiceExceptionType.ItemNotFound);
                }

                //Новые значения обновляемых параметров
                if (inputParams.ContainsKey("name"))
                {
                    if (string.IsNullOrWhiteSpace(inputUser.Name))
                    {
                        throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
                    }

                    selectedUser.Name = inputUser.Name;
                }

                if (inputParams.ContainsKey("title"))
                {
                    if (string.IsNullOrWhiteSpace(inputUser.Title))
                    {
                        throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
                    }

                    selectedUser.Title = inputUser.Title;
                }

                //TODO: пока отключим такие возможности
                //if (inputParams.ContainsKey("role"))
                //{
                //    //Чтобы случайно не остаться без уполномоченного пользователя в группе - запрещено менять свою роль.
                //    if (inputUser.Id != ri._Account.userId)
                //    {
                //        if (!Enum.IsDefined(typeof(RoleTypes), inputUser.Role))
                //        {
                //            throw new FQServiceException("Ошибка: указана некорректная роль пользователя.");
                //        }
                        
                //        selectedUser.Role = inputUser.Role;                        
                //    }
                //    else
                //    {
                //        throw new FQServiceException("Ошибка: нельзя изменить собственную роль пользователя.");
                //    }
                //}

                //if (inputParams.ContainsKey("image"))
                //{
                //    selectedUser.Image = inputUser.Image;
                //}

                DBWorker.UpdateUser(selectedUser);
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                logger.Trace("UpdateUser leave.");
            }
        }

        /// <summary>
        /// Списание стоимости награды
        /// </summary>
        /// <param name="ri"></param>
        public void WriteOffCost(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("WritOffCost started.");

                int rewardCost = JsonConvert.DeserializeObject<int>(ri.RequestData.postData.ToString());

                logger.Trace($"rewardCost: {rewardCost}");

                if (rewardCost == 0)
                {
                    throw new Exception("Ошибка: rewardCost == 0");
                }

                //Получение информации о пользователе
                List<Guid> usersId = new List<Guid>();
                usersId.Add(ri._Account.userId);
                var selectedUser = DBWorker.GetUsersById(ri._User.GroupId, usersId).FirstOrDefault();
                if (selectedUser == null || selectedUser.Status == UserStatus.Removed)
                {
                    throw new FQServiceException(FQServiceExceptionType.ItemNotFound);
                }

                //Проверка наличия необходимого количества монет
                if (rewardCost > selectedUser.Coins)
                {
                    throw new FQServiceException(FQServiceExceptionType.NotEnoughCoins);
                }

                //Списание стоимости награды
                selectedUser.Coins -= rewardCost;                
                DBWorker.UpdateUser(selectedUser);
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                logger.Trace("WritOffCost leave.");
            }
        }

        /// <summary>
        /// Списание стоимости награды
        /// </summary>
        /// <param name="ri"></param>
        public void MakePayment(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("MakePayment started.");

                Dictionary<string, string> inputParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(ri.RequestData.postData.ToString());
                
                if (!inputParams.TryGetValue("Id", out string destUserId_str) ||
                    !Guid.TryParse(destUserId_str, out Guid destUserId) ||
                    destUserId == Guid.Empty)
                {
                    throw new Exception("Ошибка: не удалось получить идентификатор целевого пользователя.");
                }

                logger.Trace($"destUserId: {destUserId}");

                if (!inputParams.TryGetValue("Coins", out string coins_str))
                {
                    throw new Exception("Ошибка: не удалось получить стоимость.");
                }

                var coins = Convert.ToInt32(coins_str);                
                logger.Trace($"Coins: {coins}");

                //Получение информации о пользователе
                List<Guid> usersId = new List<Guid>();
                usersId.Add(destUserId);
                var selectedUser = DBWorker.GetUsersById(ri._User.GroupId, usersId).FirstOrDefault();
                if (selectedUser == null || selectedUser.Status == UserStatus.Removed)
                {
                    throw new FQServiceException(FQServiceExceptionType.ItemNotFound);
                }

                //Проверка наличия необходимого количества монет
                //В минус не уходим
                selectedUser.Coins += coins;
                if (selectedUser.Coins < 0)
                {
                    selectedUser.Coins = 0;
                }

                DBWorker.UpdateUser(selectedUser);
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                logger.Trace("MakePayment leave.");
            }
        }

        /// <summary>
        ///  Получение всех пользователей группы
        /// </summary>
        /// <param name="ri"></param>
        /// <returns></returns>
        public List<User> GetAllUsers(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("GetAllUsers started.");

                var users = DBWorker.GetAllUsers(ri._User.GroupId);

                foreach (var u in users)
                {
                    Account acc = DBWorker.GetAccountById(u.Id);  
                    u.Login = acc.Login;

                    if (acc.LastAction == Constants.POSTGRES_DATETIME_MINVALUE)
                    {
                        u.LastAction = -1;
                    }
                    else
                    {
                        u.LastAction = Convert.ToInt32((DateTime.UtcNow - acc.LastAction).TotalMinutes);
                    }                    
                }

                return users;
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                logger.Trace("GetAllUsers leave.");
            }
        }

        /// <summary>
        /// Получение указанных пользователей группы
        /// </summary>
        /// <param name="ri"></param>
        /// <param name="requestedUsers"></param>
        /// <returns></returns>
        public List<User> GetUsersById(FQRequestInfo ri, List<Guid> requestedUsers)
        {
            try
            {
                logger.Trace("GetUsersById started.");

                logger.Trace($"selectedUsers.Count: {requestedUsers.Count}");

                //Проверка запрашиваемых идентификаторов
                var emptyIds = requestedUsers.Where(x => x == Guid.Empty).Count();
                logger.Trace($"emptyIds: {emptyIds}");

                if (requestedUsers.Count == 0 || emptyIds > 0)
                {
                    throw new Exception("Ошибка: selectedUsers.Count == 0 || emptyIds > 0");
                }
                
                //Нужен идентификатор группы запрашивающего пользователя
                Guid groupId = Guid.Empty;

                //В случае авторизации
                if (ri._User == null || ri._User.GroupId == Guid.Empty)
                {
                    groupId = DBWorker.CheckUserIsInGroup(ri._Account.userId);
                }
                else
                {
                    groupId = ri._User.GroupId;
                }

                //Получение пользователей
                var responsedUsers = DBWorker.GetUsersById(groupId, requestedUsers);
                if (responsedUsers.Count != requestedUsers.Count)
                {
                    throw new FQServiceException(FQServiceExceptionType.ItemNotFound);
                }

                return responsedUsers;
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                logger.Trace("GetUsersById leave.");
            }
        }
        
        /// <summary>
        /// Комплексная операция по созданию нового пользователя и аккаунта
        /// </summary>
        /// <param name="ri"></param>
        /// <returns>Идентификатор пользователя</returns>
        public Guid AddNewUserToCurrentGroup(FQRequestInfo ri)
        {
            Guid creatingChild_UserId = Guid.Empty;

            try
            {
                logger.Trace("AddNewUserToCurrentGroup started.");

                //Проверка статуса полльзователя
                if (ri._User.Role != RoleTypes.Parent)
                {
                    throw new FQServiceException(FQServiceExceptionType.NotEnoughRights);
                }

                var requestData = ri.RequestData.postData.ToString();

                if (string.IsNullOrEmpty(requestData))
                {
                    throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
                }

                //Дессериализация запроса
                string newUserLogin = string.Empty;
                string newUserPasswordHash = string.Empty;
                string newUserName = string.Empty;
                string newUsertitle = string.Empty;

                var userInputParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(requestData);

                if ((!userInputParams.TryGetValue("login", out newUserLogin)) ||
                    (!userInputParams.TryGetValue("passwordHash", out newUserPasswordHash)) ||
                    (!userInputParams.TryGetValue("name", out newUserName)) ||
                    (!userInputParams.TryGetValue("title", out newUsertitle)) ||
                    string.IsNullOrEmpty(newUserLogin) ||
                    string.IsNullOrEmpty(newUserPasswordHash) ||                    
                    string.IsNullOrEmpty(newUserName) ||
                    string.IsNullOrEmpty(newUsertitle))
                {
                    throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
                }

                logger.Trace($"newUserLogin: {newUserLogin}.");
                logger.Trace($"newUserPasswordHash: {newUserPasswordHash}.");                
                logger.Trace($"newUserName: {newUserName}.");
                logger.Trace($"newUserTitle: {newUsertitle}.");

                newUserLogin = newUserLogin.ToLower();                

                User creatingUser = new User(true);
                creatingUser.Name = newUserName;
                creatingUser.Title = newUsertitle;

                if ((userInputParams.TryGetValue("role", out string role)) && !(string.IsNullOrEmpty(role)))
                {
                    if (!Enum.IsDefined(typeof(RoleTypes), Convert.ToInt32(role)))
                    {
                        throw new Exception("Ошибка: указана некорректная роль пользователя.");
                    }

                    creatingUser.Role = (RoleTypes)Convert.ToInt32(role);
                }

                ////if ((userInputParams.TryGetValue("image", out string image)) && !(string.IsNullOrEmpty(image)))
                ////{
                ////    creatingUser.Image = image;
                ////}

                //var groupParentUsersCount = DBWorker.GetRoleUsersCount(ri._User.GroupId, RoleTypes.Parent);
                //var groupChildrenUsersCount = DBWorker.GetRoleUsersCount(ri._User.GroupId, RoleTypes.Children);

                //if (ri._Group.SubscriptionIsActive)
                //{
                //    if (creatingUser.Role == RoleTypes.Parent)
                //    {
                //        if (groupParentUsersCount >= CommonLib.Settings.Current[Settings.Name.Account.maxParents_Extension, CommonData.maxParents_Extension])
                //        {
                //            throw new FQServiceException(FQServiceExceptionType.LimitAchieved);
                //        }
                //    }
                //    else
                //    {
                //        if (groupChildrenUsersCount >= CommonLib.Settings.Current[Settings.Name.Account.maxChildrens_Extension, CommonData.maxChildrens_Extension])
                //        {
                //            throw new FQServiceException(FQServiceExceptionType.LimitAchieved);
                //        }
                //    }

                //}
                //else
                //{
                //    if (creatingUser.Role == RoleTypes.Parent)
                //    {
                //        if (groupParentUsersCount >= CommonLib.Settings.Current[Settings.Name.Account.maxParents_NotExtension, CommonData.maxParents_NotExtension])
                //        {
                //            throw new FQServiceException(FQServiceExceptionType.LimitAchieved);
                //        }
                //    }
                //    else
                //    {
                //        if (groupChildrenUsersCount >= CommonLib.Settings.Current[Settings.Name.Account.maxChildrens_NotExtension, CommonData.maxChildrens_NotExtension])
                //        {
                //            throw new FQServiceException(FQServiceExceptionType.LimitAchieved);
                //        }
                //    }
                //}

                var groupUsersCount = DBWorker.GetAllUsersCount(ri._User.GroupId);

                if (ri._Group.SubscriptionIsActive)
                {
                    var totalCounts = CommonLib.Settings.Current[Settings.Name.Account.maxParents_Extension, CommonData.maxParents_Extension] + 
                        CommonLib.Settings.Current[Settings.Name.Account.maxChildrens_Extension, CommonData.maxChildrens_Extension];

                    if (groupUsersCount >= totalCounts)
                    {
                        throw new FQServiceException(FQServiceExceptionType.LimitAchieved);
                    }
                }
                else
                {
                    var totalCounts = CommonLib.Settings.Current[Settings.Name.Account.maxParents_NotExtension, CommonData.maxParents_NotExtension] +
                        CommonLib.Settings.Current[Settings.Name.Account.maxChildrens_NotExtension, CommonData.maxChildrens_NotExtension];

                    if (groupUsersCount >= totalCounts)
                    {
                        throw new FQServiceException(FQServiceExceptionType.LimitAchieved);
                    }
                }
                
                if (!Account.VerifyLoginAsFQInnerLogin(newUserLogin))
                {
                    throw new FQServiceException(FQServiceExceptionType.IncorrectLoginFormat);
                }

                newUserPasswordHash = GetPasswordHash(newUserLogin, newUserPasswordHash);

                //Создание нового юзера в рамках группы
                creatingChild_UserId = AddUserToGroup(ri, creatingUser);

                Account creatingChild_Account = new Account(true);
                creatingChild_Account.Login = newUserLogin;
                creatingChild_Account.PasswordHashCurrent = newUserPasswordHash;
                creatingChild_Account.userId = creatingChild_UserId;
                creatingChild_Account.isMain = false;
                creatingChild_Account.CreationDate = DateTime.UtcNow;

                DBWorker.InsertAccount(creatingChild_Account);                
                
                //Запись HistoryEvent-а
                try
                {
                    FQRequestInfo ri_CreateHistoryEvent = ri.Clone();

                    List<Guid> availableFor = new List<Guid>();
                    availableFor.Add(creatingChild_Account.userId);

                    HistoryEvent.VisabilityEnum visability = HistoryEvent.VisabilityEnum.Children;

                    //Если добавляемый пользователь - родитель, то показ информации об этом событии только родителям
                    if (creatingUser.Role == RoleTypes.Parent)
                    {
                        visability = HistoryEvent.VisabilityEnum.Parents;
                    }

                    HistoryEvent he = new HistoryEvent(HistoryEvent.ItemTypeEnum.User, HistoryEvent.MessageTypeEnum.User_Created, visability, creatingChild_UserId, availableFor, ri._User.Id);
                    
                    ri_CreateHistoryEvent.RequestData.actionName = "CreateHistoryEvent";
                    ri_CreateHistoryEvent.RequestData.postData = JsonConvert.SerializeObject(he);

                    RouteInfo.RouteToService(ri_CreateHistoryEvent, _httpContextAccessor);
                }
                catch (Exception)
                {
                    //Если по какой-то причине не улаось записать HistoryEvent - не повод  прерывать операцию и кидать пользователю Error.
                }

                //Только для ребенка
                if (creatingUser.Role == RoleTypes.Children)
                {                    
                    try
                    {
                        //Создадим персональную задачу завершить обучение
                        FQRequestInfo ri_CreateUserStartingTask = ri.Clone();
                        ri_CreateUserStartingTask.RequestData.actionName = "CreateUserStartingTask";
                        ri_CreateUserStartingTask.RequestData.postData = creatingChild_UserId.ToString();
                        RouteInfo.RouteToService(ri_CreateUserStartingTask, _httpContextAccessor);
                    }
                    catch
                    {
                        //Если по какой-то причине не удалось создать задачу - не повод  прерывать операцию и кидать пользователю Error.
                    }

                    try
                    {
                        //Создадим персональную награду "Время на мобильные\компьютерные\видео игры"
                        FQRequestInfo ri_CreateUserStartingReward = ri.Clone();
                        ri_CreateUserStartingReward.RequestData.actionName = "CreateUserStartingReward";
                        ri_CreateUserStartingReward.RequestData.postData = creatingChild_UserId.ToString();
                        RouteInfo.RouteToService(ri_CreateUserStartingReward, _httpContextAccessor);
                    }
                    catch
                    {
                        //Если по какой-то причине не удалось создать задачу - не повод  прерывать операцию и кидать пользователю Error.
                    }
                }

                return creatingChild_UserId;
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);

                //В такой ситуации выходит, что создался юзер, не привязанный к аккаунту - удалим его
                if (creatingChild_UserId != Guid.Empty)
                {
                    try
                    {
                        DBWorker.RemoveUser(ri._User.GroupId, creatingChild_UserId);
                    }
                    catch (Exception)
                    {
                        
                    }
                }

                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);

                //В такой ситуации выходит, что создался юзер, не привязанный к аккаунту - удалим его
                if (creatingChild_UserId != Guid.Empty)
                {
                    try
                    {
                        DBWorker.RemoveUser(ri._User.GroupId, creatingChild_UserId);
                    }
                    catch (Exception)
                    {

                    }
                }
                                
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                logger.Trace("AddNewUserToCurrentGroup leave.");
            }
        }
                
        public string GetFQTag(FQRequestInfo ri)
        {
            string newFQInnerTag = string.Empty;

            try
            {
                logger.Trace("GetFQTag started.");

                //Проверка статуса полльзователя
                if (ri._User.Role != RoleTypes.Parent)
                {
                    throw new FQServiceException(FQServiceExceptionType.NotEnoughRights);
                }

                var newUserLogin = ri.RequestData.postData.ToString();

                if (string.IsNullOrEmpty(newUserLogin))
                {
                    throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
                }                

                logger.Trace($"newUserLogin: {newUserLogin}.");
               
                newUserLogin = newUserLogin.ToLower();
                
                if (!Account.VerifyLoginAsFQInnerLogin(newUserLogin, true))
                {
                    throw new FQServiceException(FQServiceExceptionType.IncorrectLoginFormat);
                }

                newFQInnerTag = GetFQInnerTag(newUserLogin);

                logger.Trace($"newFQInnerLogin: {newFQInnerTag}.");

                return newFQInnerTag;
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);   
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                logger.Trace("GetFQTag leave.");
            }
        }

        /// <summary>
        /// Комплексная операция по удалению пользователя и аккаунта
        /// </summary>
        /// <param name="ri"></param>
        public void RemoveUserAndAccount(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("RemoveUser started.");

                var requestData = ri.RequestData.postData.ToString();

                if (string.IsNullOrEmpty(requestData) || 
                    !Guid.TryParse(requestData, out Guid removingUserId) || 
                    removingUserId == Guid.Empty)
                {
                    throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
                }

                //Проверка статуса пользователя
                if (ri._User.Role != RoleTypes.Parent)
                {
                    throw new FQServiceException(FQServiceExceptionType.NotEnoughRights);
                }

                List<Guid> removingUsers = new List<Guid>();
                removingUsers.Add(removingUserId);
                User removedUser = DBWorker.GetUsersById(ri._User.GroupId, removingUsers).FirstOrDefault();
                if (removedUser == null || removedUser.Status == UserStatus.Removed)
                {
                    throw new FQServiceException(FQServiceExceptionType.ItemNotFound);
                }

                var removingAccount = DBWorker.GetAccountById(removingUserId);

                if (removingAccount == null || removingAccount.isMain)
                {
                    throw new Exception("Ошибка: аккаунт целевого пользователя не найден или он является основателем группы.");
                }

                RemoveRelatedItems(ri, removingUserId);

                RemoveAccessData(ri, removingUserId);

                DBWorker.RemoveUser(ri._User.GroupId, removingUserId);                

                //Запись HistoryEvent-а
                try
                {
                    FQRequestInfo ri_CreateHistoryEvent = ri.Clone();

                    List<Guid> availableFor = new List<Guid>();

                    HistoryEvent he = new HistoryEvent(HistoryEvent.ItemTypeEnum.User, HistoryEvent.MessageTypeEnum.User_Removed, HistoryEvent.VisabilityEnum.Parents, removedUser.Id, availableFor, ri._User.Id);

                    ri_CreateHistoryEvent.RequestData.actionName = "CreateHistoryEvent";
                    ri_CreateHistoryEvent.RequestData.postData = JsonConvert.SerializeObject(he);

                    RouteInfo.RouteToService(ri_CreateHistoryEvent, _httpContextAccessor);
                }
                catch (Exception)
                {
                    //Если по какой-то причине не улаось записать HistoryEvent - не повод  прерывать операцию и кидать пользователю Error.
                }
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                logger.Trace("RemoveUser leave.");
            }
        }

        /// <summary>
        /// Добавление нового пользователя в группу владельца
        /// </summary>
        /// <param name="ri"></param>
        /// <param name="inputUser"></param>
        /// <returns>Guid созданого пользователя</returns>
        private Guid AddUserToGroup(FQRequestInfo ri, User inputUser)
        {
            try
            {
                logger.Trace("AddUserToGroup started.");

                logger.Trace($"inputUser.name: {inputUser.Name}");

                if (string.IsNullOrEmpty(inputUser.Name))
                {
                    throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
                }

                //Проверка статуса полльзователя
                if (ri._User.Role != RoleTypes.Parent)
                {
                    throw new FQServiceException(FQServiceExceptionType.NotEnoughRights);
                }

                //Создание пользователя
                var newUser = inputUser.Clone();
                newUser.Id = Guid.NewGuid();
                newUser.GroupId = ri._User.GroupId;

                DBWorker.CreateUser(newUser);

                return newUser.Id;
            }
            finally
            {
                logger.Trace("AddUserToGroup leave.");
            }
        }

        /// <summary>
        /// Удаление связаных сущностей
        /// </summary>
        /// <param name="ri"></param>
        /// <param name="removingUserId"></param>
        private void RemoveRelatedItems(FQRequestInfo ri, Guid removingUserId)
        {
            try
            {
                logger.Trace("RemoveRelatedItems started.");

                //RemoveRelatedRewards
                FQRequestInfo ri_RemoveRelatedRewards = ri.Clone();
                ri_RemoveRelatedRewards.RequestData.actionName = "RemoveRelatedRewards";
                ri_RemoveRelatedRewards.RequestData.postData = JsonConvert.SerializeObject(removingUserId);
                RouteInfo.RouteToService(ri_RemoveRelatedRewards, _httpContextAccessor);

                //UpdateRelatedTasks
                FQRequestInfo ri_UpdateRelatedTasks = ri.Clone();
                ri_UpdateRelatedTasks.RequestData.actionName = "UpdateRelatedTasks";
                ri_UpdateRelatedTasks.RequestData.postData = JsonConvert.SerializeObject(removingUserId);
                RouteInfo.RouteToService(ri_UpdateRelatedTasks, _httpContextAccessor);                
            }
            finally
            {
                logger.Trace("RemoveRelatedItems leave.");
            }
        }
        #endregion

        /// <summary>
        /// Подсчет хэша пароля
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private string GetPasswordHash(string login, string password)
        {
            string passwordHash = string.Empty;

            byte[] saltBytes = Encoding.UTF8.GetBytes(login.ToLower());
            byte[] passBytes = Encoding.UTF8.GetBytes(password);
            byte[] secondSaltBytes = Encoding.UTF8.GetBytes("ljdbfgghbdtn ghjktcdtgjhgl");

            byte[] plainTextWithSaltBytes =
                new byte[passBytes.Length + saltBytes.Length + secondSaltBytes.Length];

            // Copy plain text bytes into resulting array.
            for (int i = 0; i < passBytes.Length; i++)
                plainTextWithSaltBytes[i] = passBytes[i];

            // Append salt bytes to the resulting array.
            for (int i = 0; i < saltBytes.Length; i++)
                plainTextWithSaltBytes[passBytes.Length + i] = saltBytes[i];

            // Append salt bytes to the resulting array.
            for (int i = 0; i < secondSaltBytes.Length; i++)
                plainTextWithSaltBytes[passBytes.Length + saltBytes.Length + i] = secondSaltBytes[i];

            var hash = new SHA256Managed();

            // Compute hash value of our plain text with appended salt.
            byte[] hashBytes = hash.ComputeHash(plainTextWithSaltBytes);

            passwordHash = Convert.ToBase64String(hashBytes);

            return passwordHash;
        }

        /// <summary>
        /// Подсчет хэша DeviceId
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private string GetDeviceIdHash(string login, string deviceId)
        {
            string passwordHash = string.Empty;

            byte[] saltBytes = Encoding.UTF8.GetBytes(login.ToLower());
            byte[] passBytes = Encoding.UTF8.GetBytes(deviceId);
            byte[] secondSaltBytes = Encoding.UTF8.GetBytes("iubue90gyijhsr578tw34iut");

            byte[] plainTextWithSaltBytes =
                new byte[passBytes.Length + saltBytes.Length + secondSaltBytes.Length];

            // Copy plain text bytes into resulting array.
            for (int i = 0; i < passBytes.Length; i++)
                plainTextWithSaltBytes[i] = passBytes[i];

            // Append salt bytes to the resulting array.
            for (int i = 0; i < saltBytes.Length; i++)
                plainTextWithSaltBytes[passBytes.Length + i] = saltBytes[i];

            // Append salt bytes to the resulting array.
            for (int i = 0; i < secondSaltBytes.Length; i++)
                plainTextWithSaltBytes[passBytes.Length + saltBytes.Length + i] = secondSaltBytes[i];

            var hash = new SHA256Managed();

            // Compute hash value of our plain text with appended salt.
            byte[] hashBytes = hash.ComputeHash(plainTextWithSaltBytes);

            passwordHash = Convert.ToBase64String(hashBytes);

            return passwordHash;
        }
    }
}
