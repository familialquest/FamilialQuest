using Assets.Code.Models.REST.CommonTypes;
using Proyecto26;
using System;
using System.Collections.Generic;
using System.Text;
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;

namespace Code.Models.REST.Administrative
{
    /// <summary>
    /// Запрос на сброс пароля
    /// </summary>
    class ResetPasswordRequest
    {
        public FQRequestInfo request;

        public ResetPasswordRequest()
        {

        }

        /// <summary>
        /// Формирование запроса
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <param name="passwordHash">Хэш нового пароля пользователя</param>
        public ResetPasswordRequest(string login, string passwordHashNew)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(passwordHashNew))
            {
                throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
            }

            request = new FQRequestInfo(true);
            request.RequestData.actionName = "ResetPassword";
            request.Credentials.Login = login;
            request.Credentials.PasswordHash = passwordHashNew;
        }
    }


    /// <summary>
    /// Ответ
    /// </summary>
    public class ResetPasswordResponse : FQResponse
    {
        public ResetPasswordResponse(ResponseHelper response) : base(response)
        { }
    }
}
