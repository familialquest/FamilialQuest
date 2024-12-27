using System;
using System.Net;
using System.Net.Http;
using CommonLib;
using CommonRoutes;
using CommonTypes;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using static CommonLib.FQServiceException;

namespace RouteService.Services
{
    /// <summary>
    /// RouteServices реализует первичную верефикацию и маршрутизацию
    /// запросов клиента
    /// </summary>
    public class RouteServices : IRouteServices
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Default constructor with HTTPContext
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        public RouteServices(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Первичная проверка корректности запроса,
        /// аутентификация пользователя и дальнейший роутинг к соответствующему,
        /// или роутинг неавторизированного пользователя по вопросам регистрации.
        /// </summary>
        /// <param name="ri"></param>
        /// <returns>Результат обработки запроса</returns>
        public FQResponseInfo Route(FQRequestInfo ri)
        {
            logger.Trace("Route started.");

            FQResponseInfo response = new FQResponseInfo(true);

            try
            {              
                logger.Trace($"Login: {ri.Credentials.Login}.");
                logger.Trace($"tokenB64: {ri.Credentials.tokenB64}.");
                logger.Trace($"PasswordHash: {ri.Credentials.PasswordHash}.");
                logger.Trace($"actionName: {ri.RequestData.actionName}.");
                logger.Trace($"ClientVersion: {ri.ClientVersion}.");

                //Проверка версии клиента
                if (ri.ClientVersion < Settings.Current[Settings.Name.Route.minimalClientVersion, CommonData.minimalClientVersion])
                {
                    throw new FQServiceException(FQServiceExceptionType.UnsupportedClientVersion);
                }

                ri.Credentials.Login = ri.Credentials.Login.ToLower();

                if (!Account.VerifyLoginAsEmail(ri.Credentials.Login) &&
                    !Account.VerifyLoginAsFQInnerLogin(ri.Credentials.Login))
                {
                    throw new FQServiceException(FQServiceExceptionType.IncorrectLoginFormat);
                }

                //TODO: проверка по маске

                //Проверка информации о сервисе, обрабатывающем запрашиваемый Action
                if (RouteInfo.routes.ContainsKey(ri.RequestData.actionName))
                {
                    //Проверка: запрашиваемый actionName по вопросу регистрации\восстановлению или нет
                    if (IsRegOrResetAction(ri.RequestData.actionName))
                    {
                        //Запрос по регистрации итд - аутентификация не требуется
                        RouteInfo.RouteToService(ri, _httpContextAccessor);
                    }
                    else
                    {
                        //Запрос к сервису - требуется аутентификация
                        FQRequestInfo ri_AuthRequest = ri.Clone();
                        ri_AuthRequest.RequestData.actionName = "Auth";
                        var ri_AuthResponse = RouteInfo.RouteToService(ri_AuthRequest, _httpContextAccessor);

                        FQRequestInfo authorizedUser = JsonConvert.DeserializeObject<FQRequestInfo>(ri_AuthResponse);

                        if (authorizedUser._Account.userId != Guid.Empty && 
                            !string.IsNullOrEmpty(authorizedUser._Account.Token))
                        {
                            //Аутентификация выполнена успешно
                            //Перенаправление к соответствующему Action-у сервису

                            //Контекст пользователя прокидываем запрашиваемому сервису
                            ri._Account = authorizedUser._Account;
                            ri._User = authorizedUser._User;
                            ri._Group = authorizedUser._Group;

                            //Сразу в ответ внесем идентификатор пользователя и валидный токен для следующей операции
                            //Только если не логаут
                            if (!ri.RequestData.actionName.Equals("Logout"))
                            {
                                response.ActualToken = ri._Account.Token;
                                response.UserId = ri._Account.userId;
                            }

                            //Если запрашена не только аутентификация - роут к нужному сервису
                            if (!ri.RequestData.actionName.Equals("Auth"))
                            {
                                response.ResponseData = RouteInfo.RouteToService(ri, _httpContextAccessor);
                            }
                            else
                            {
                                //Детектим первый вход в сервис и отправим привет аппке
                                if (ri._Account.LastAction == CommonData.dateTime_FQDB_MinValue)
                                {
                                    response.FirstLogin = true;
                                }
                            }
                        }
                        else
                        {                            
                            throw new FQServiceException(FQServiceExceptionType.AuthError);
                        }
                    }
                    
                    //Возврат ответа
                    response.Successfuly = true;
                    return response; 
                }
                else
                {
                    throw new Exception("Неизвестный action.");
                }
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);

                FQServiceExceptionType fqExType = FQServiceExceptionType.DefaultError;

                try
                {
                    fqExType = (FQServiceExceptionType)Enum.Parse(typeof(FQServiceExceptionType), fqEx.Message);
                }
                catch
                {
                    fqExType = FQServiceExceptionType.DefaultError;
                }

                //Возврат кода ошибки
                response.Successfuly = false;
                response.ResponseData = ((int)fqExType).ToString();

                return response;
            }
            catch (Exception ex)
            {
                logger.Error(ex);

                //Возврат дефолтного кода ошибки
                response.Successfuly = false;
                response.ResponseData = FQServiceExceptionType.DefaultError.ToString();

                return response;
            }
            finally
            {
                logger.Trace("Route leave.");
            }
        }

        /// <summary>
        /// Проверка, относится ли запрос к вопросам регистрации
        /// </summary>
        /// <param name="actionName">Запрашиваемый пользователем Action</param>
        /// <returns>Результат проверки: true/false</returns>
        private bool IsRegOrResetAction(string actionName)
        {
            try
            {
                logger.Trace("IsRegRoute started.");

                if (actionName == "CreateTempAccount" ||
                    actionName == "ConfirmTempAccount" ||
                    actionName == "ResetPassword" ||
                    actionName == "ConfirmResetPassword")
                {
                    return true;
                }
                else
                {
                    return false;
                }                
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                logger.Trace("IsRegRoute leave.");
            }
        }
    }
}
