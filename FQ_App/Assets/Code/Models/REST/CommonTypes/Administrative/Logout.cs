using Assets.Code.Models.REST.CommonTypes;
using System;
using System.Collections.Generic;
using System.Text;
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;

namespace Code.Models.REST.Administrative
{
    /// <summary>
    /// Выход из аккаунта
    /// </summary>
    public class LogoutRequest
    {
        public FQRequestInfo request;

        public LogoutRequest()
        {

        }

        /// <summary>
        /// Формирование запроса
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <param name="sessionToken">Токен сессии</param>
        public LogoutRequest(string login, string sessionToken)
        {
            request = new FQRequestInfo(true);
            request.RequestData.actionName = "Logout";
            request.Credentials.Login = login;
            request.Credentials.tokenB64 = sessionToken;
        }
    }


    /// <summary>
    /// Результат запроса
    /// </summary>
    /// [HTTP Status 200] или [HTTP Status 500 + errorMessage]
}
