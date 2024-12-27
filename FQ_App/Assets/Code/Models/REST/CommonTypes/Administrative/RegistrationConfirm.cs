using Assets.Code.Models.REST.CommonTypes;
using Proyecto26;
using System;
using System.Collections.Generic;
using System.Text;
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;

namespace Code.Models.REST.Administrative
{
    /// <summary>
    /// Запрос на подтверждение регистрации пользователя
    /// </summary>
    public class RegistrationConfirmRequest
    {
        public FQRequestInfo request;

        public RegistrationConfirmRequest()
        {

        }

        /// <summary>
        /// Формирование запроса
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <param name="confirmCode">Код подверждения</param>
        public RegistrationConfirmRequest(string login, string confirmCode)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(confirmCode))
            {
                throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
            }

            request = new FQRequestInfo(true);
            request.Credentials.Login = login;
            request.RequestData.actionName = "ConfirmTempAccount";
            request.RequestData.postData = confirmCode;
        }
    }


    /// <summary>
    /// Ответ
    /// </summary>
    public class RegistrationConfirmResponse : FQResponse
    {
        public RegistrationConfirmResponse(ResponseHelper response) : base(response)
        { }
    }
}
