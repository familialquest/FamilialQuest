using Assets.Code.Models.REST.CommonTypes;
using Proyecto26;
using System;
using System.Collections.Generic;
using System.Text;
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;

namespace Code.Models.REST.Users
{
    /// <summary>
    /// Получение указанных пользователей
    /// </summary>
    public class GetUsersByIdRequest
    {
        public FQRequestInfo request;

        public GetUsersByIdRequest()
        {

        }

        /// <summary>
        /// Формирование запроса
        /// </summary>
        /// <param name="selectedUsers">Список идентификаторов запрашиваемых пользователей</param>
        public GetUsersByIdRequest(
            List<Guid> selectedUsers)
        {
            var login = CredentialHandler.Instance.Credentials.Login;
            var sessionToken = CredentialHandler.Instance.Credentials.tokenB64;

            if (selectedUsers == null || selectedUsers.Count == 0 || selectedUsers.Contains(Guid.Empty))
            {
                throw new FQServiceException(FQServiceExceptionType.DefaultError);
            }

            request = new FQRequestInfo(true);
            request.RequestData.actionName = "GetUsersById";
            request.Credentials.Login = login;
            request.Credentials.tokenB64 = sessionToken;

            request.RequestData.postData = selectedUsers;
        }
    }

    /// <summary>
    /// Ответ
    /// </summary>
    public class GetUsersByIdResponse : FQResponse
    {
        public List<User> Users
        {
            get
            {
                return this.ri.DeserializeResponseData<List<User>>();
            }
        }

        public GetUsersByIdResponse(ResponseHelper response) : base(response)
        { }
    }
}
