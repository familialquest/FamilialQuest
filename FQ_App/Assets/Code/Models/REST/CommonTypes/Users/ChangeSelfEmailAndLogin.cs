using Assets.Code.Models.REST.CommonTypes;
using Proyecto26;
using System;
using System.Collections.Generic;
using System.Text;
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;

namespace Code.Models.REST.Users
{
    /// <summary>
    /// Изменение свого Email и Login-а
    /// </summary>
    public class ChangeSelfEmailAndLoginRequest
    {
        public FQRequestInfo request;

        public ChangeSelfEmailAndLoginRequest()
        {

        }

        /// <summary>
        /// Формирование запроса
        /// </summary>
        /// <param name="newEmail">Новый адрес почты владельца группы</param>
        /// <param name="passwordHashCurrent">Текущий хэш пароля владельца группы (для подтверждения операции)</param>
        public ChangeSelfEmailAndLoginRequest(
            string newEmail,
            string passwordHashCurrent)
        {
            var login = CredentialHandler.Instance.Credentials.Login;
            var sessionToken = CredentialHandler.Instance.Credentials.tokenB64;

            if (string.IsNullOrEmpty(newEmail) ||
                string.IsNullOrEmpty(passwordHashCurrent))
            {
                throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
            }

            request = new FQRequestInfo(true);
            request.RequestData.actionName = "ChangeEmail";
            request.Credentials.Login = login;
            request.Credentials.tokenB64 = sessionToken;

            Account updatedAccount = new Account();
            updatedAccount.Email = newEmail;
            updatedAccount.PasswordHashCurrent = passwordHashCurrent;

            request.RequestData.postData = updatedAccount;
        }
    }

    /// <summary>
    /// Ответ
    /// </summary>
    public class ChangeSelfEmailAndLoginResponse : FQResponse
    {
        public ChangeSelfEmailAndLoginResponse(ResponseHelper response) : base(response)
        { }
    }
}
