using Assets.Code.Models.REST.CommonTypes;
using Proyecto26;
using System;
using System.Collections.Generic;
using System.Text;
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;

namespace Code.Models.REST.Users
{
    /// <summary>
    /// Изменение своего пароля
    /// </summary>
    public class ChangeSelfPasswordRequest
    {
        public FQRequestInfo request;

        public ChangeSelfPasswordRequest()
        {

        }

        /// <summary>
        /// Формирование запроса
        /// </summary>
        /// <param name="passwordHashCurrent">Текущий хэш пароля владельца группы (для подтверждения операции)</param>
        /// <param name="passwordHashNew">Новый хэш пароля владельца группы</param>
        public ChangeSelfPasswordRequest(
            string passwordHashCurrent,
            string passwordHashNew)
        {
            var login = CredentialHandler.Instance.Credentials.Login;
            var sessionToken = CredentialHandler.Instance.Credentials.tokenB64;

            if (string.IsNullOrEmpty(passwordHashCurrent) ||
                string.IsNullOrEmpty(passwordHashNew))
            {
                throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
            }
           
            request = new FQRequestInfo(true);
            request.RequestData.actionName = "ChangeSelfPassword";
            request.Credentials.Login = login;
            request.Credentials.tokenB64 = sessionToken;

            Account updatedAccount = new Account();
            updatedAccount.PasswordHashCurrent = passwordHashCurrent;
            updatedAccount.PasswordHashNew = passwordHashNew;

            request.RequestData.postData = updatedAccount;
        }
    }

    /// <summary>
    /// Ответ
    /// </summary>
    public class ChangeSelfPasswordResponse : FQResponse
    {
        public ChangeSelfPasswordResponse(ResponseHelper response) : base(response)
        { }
    }
}


