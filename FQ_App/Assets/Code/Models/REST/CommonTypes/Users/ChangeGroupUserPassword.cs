using Assets.Code.Models.REST.CommonTypes;
using Proyecto26;
using System;
using System.Collections.Generic;
using System.Text;
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;

namespace Code.Models.REST.Users
{
    /// <summary>
    /// Изменение пароля зависимого пользователя
    /// </summary>
    public class ChangeGroupUserPasswordRequest
    {
        public FQRequestInfo request;

        public ChangeGroupUserPasswordRequest()
        {

        }

        /// <summary>
        /// Формирование запроса
        /// </summary>
        /// <param name="childUserId">Идентификатор пользователя (ребенка)</param>
        /// <param name="passwordHashCurrent">Текущий хэш пароля владельца группы (для подтверждения операции)</param>
        /// <param name="passwordHashNew">Новый пароль ребенка</param>
        public ChangeGroupUserPasswordRequest(            
            Guid childUserId,
            string passwordHashCurrent,
            string passwordHashNew)
        {
            var login = CredentialHandler.Instance.Credentials.Login;
            var sessionToken = CredentialHandler.Instance.Credentials.tokenB64;

            if (childUserId == Guid.Empty)
            {
                throw new FQServiceException(FQServiceExceptionType.DefaultError);
            }

            if (
                string.IsNullOrEmpty(passwordHashCurrent) ||
                string.IsNullOrEmpty(passwordHashNew))
            {
                throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
            }

            request = new FQRequestInfo(true);
            request.RequestData.actionName = "ChangeGroupUserPassword";
            request.Credentials.Login = login;
            request.Credentials.tokenB64 = sessionToken;

            Account updatedAccount = new Account();
            updatedAccount.userId = childUserId;
            updatedAccount.PasswordHashCurrent = passwordHashCurrent;
            updatedAccount.PasswordHashNew = passwordHashNew;

            request.RequestData.postData = updatedAccount;
        }
    }

    /// <summary>
    /// Ответ
    /// </summary>
    public class ChangeGroupUserPasswordResponse : FQResponse
    {
        public ChangeGroupUserPasswordResponse(ResponseHelper response) : base(response)
        { }
    }
}


