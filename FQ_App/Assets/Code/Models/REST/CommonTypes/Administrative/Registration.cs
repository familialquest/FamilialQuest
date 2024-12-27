using Assets.Code.Models.REST.CommonTypes;
using Proyecto26;
using System;
using System.Collections.Generic;
using System.Text;
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;

namespace Code.Models.REST.Administrative
{
    /// <summary>
    /// Запрос на регистрацию пользователя
    /// </summary>
    public class RegistrationRequest
    {
        public FQRequestInfo request;

        public RegistrationRequest()
        {

        }

        /// <summary>
        /// Формирование запроса
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <param name="passwordHash">Хэш пароля пользователя</param>
        public RegistrationRequest(string login, string passwordHash)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(passwordHash))
            {
                throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
            }

            request = new FQRequestInfo(true);
            request.RequestData.actionName = "CreateTempAccount";
            request.Credentials.Login = login;
            request.Credentials.PasswordHash = passwordHash;            
        }
    }


    /// <summary>
    /// Ответ
    /// </summary>
    public class RegistrationResponse : FQResponse
    {
        public RegistrationResponse(ResponseHelper response) : base(response)
        { }
    }
}
