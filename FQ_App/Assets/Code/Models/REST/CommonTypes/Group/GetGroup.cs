using Assets.Code.Models.REST.CommonTypes;
using Proyecto26;
using System;
using System.Collections.Generic;
using System.Text;
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;

namespace Code.Models.REST.Group
{
    /// <summary>
    /// Запрос на получение группы
    /// </summary>
    class GetGroupRequest
    {
        public FQRequestInfo request;
        
        /// <summary>
        /// Формирование запроса
        /// </summary>     
        public GetGroupRequest()
        {
            var login = CredentialHandler.Instance.Credentials.Login;
            var sessionToken = CredentialHandler.Instance.Credentials.tokenB64;

            request = new FQRequestInfo(true);
            request.RequestData.actionName = "GetGroup";
            request.Credentials.Login = login;
            request.Credentials.tokenB64 = sessionToken;
        }
    }

    /// <summary>
    /// Ответ
    /// </summary>
    public class GetGroupResponse : FQResponse
    {
        public Group MyGroup
        {
            get
            {
                return this.ri.DeserializeResponseData<Group>();
            }
        }

        public GetGroupResponse(ResponseHelper response) : base(response)
        { }
    }
}
