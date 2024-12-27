using Assets.Code.Models.REST.CommonTypes;
using Proyecto26;
using System;
using System.Collections.Generic;
using System.Text;
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;

namespace Code.Models.REST.Users
{
    /// <summary>
    /// Удаление пользователя
    /// </summary>
    public class RemoveUserRequest
    {
        public FQRequestInfo request;

        public RemoveUserRequest()
        {

        }

        /// <summary>
        /// Формирование запроса
        /// </summary>
        /// <param name="removedUserId">Идентификатор удаляемого пользователя</param>
        /// <param name=""></param>
        public RemoveUserRequest(
            Guid removedUserId)
        {
            var login = CredentialHandler.Instance.Credentials.Login;
            var sessionToken = CredentialHandler.Instance.Credentials.tokenB64;

            if (removedUserId == Guid.Empty)
            {
                throw new FQServiceException(FQServiceExceptionType.DefaultError);
            }

            request = new FQRequestInfo(true);
            request.RequestData.actionName = "RemoveUser";
            request.Credentials.Login = login;
            request.Credentials.tokenB64 = sessionToken; 
            
            request.RequestData.postData = removedUserId.ToString();
        }
    }

    /// <summary>
    /// Ответ
    /// </summary>
    public class RemoveUserResponse : FQResponse
    {
        public RemoveUserResponse(ResponseHelper response) : base(response)
        { }
    }
}



