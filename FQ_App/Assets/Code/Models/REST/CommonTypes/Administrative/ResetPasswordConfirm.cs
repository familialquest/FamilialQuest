using Assets.Code.Models.REST.CommonTypes;
using Proyecto26;
using System;
using System.Collections.Generic;
using System.Text;
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;

namespace Code.Models.REST.Administrative
{
    /// <summary>
    /// Запрос на подтверждение операции сброса пароля
    /// </summary>
    public class ResetPasswordConfirmRequest
    {
        public FQRequestInfo request;

        public ResetPasswordConfirmRequest()
        {

        }

        /// <summary>
        /// Формирование запроса
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <param name="confirmCode">Код подверждения</param>
        public ResetPasswordConfirmRequest(string login, string confirmCode)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(confirmCode))
            {
                throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
            }

            request = new FQRequestInfo(true);
            request.Credentials.Login = login;
            request.RequestData.actionName = "ConfirmResetPassword";
            request.RequestData.postData = confirmCode;
        }
    }


    /// <summary>
    /// Ответ
    /// </summary>
    public class ResetPasswordConfirmResponse : FQResponse
    {
        public ResetPasswordConfirmResponse(ResponseHelper response) : base(response)
        { }
    }
}
