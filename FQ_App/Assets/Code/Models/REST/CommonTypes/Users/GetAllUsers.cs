using Assets.Code.Models.REST.CommonTypes;
using Proyecto26;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.SceneManagement;
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;

namespace Code.Models.REST.Users
{
    /// <summary>
    /// Получение всех пользователей
    /// </summary>
    public class GetAllUsersRequest
    {
        public FQRequestInfo request;

        /// <summary>
        /// Формирование запроса
        /// </summary>
        public GetAllUsersRequest()
        {
            var login = CredentialHandler.Instance.Credentials.Login;
            var sessionToken = CredentialHandler.Instance.Credentials.tokenB64;       

            request = new FQRequestInfo(true);
            request.RequestData.actionName = "GetAllUsers";
            request.Credentials.Login = login;
            request.Credentials.tokenB64 = sessionToken;                              
        }
    }

    /// <summary>
    /// Ответ
    /// </summary>
    public class GetAllUsersResponse : FQResponse
    {
        public List<User> Users
        {
            get
            {
                return this.ri.DeserializeResponseData<List<User>>();
            }
        }

        public GetAllUsersResponse(ResponseHelper response) : base(response)
        { }
    }
}
