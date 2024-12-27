using Assets.Code.Models.REST.CommonTypes;
using Proyecto26;
using System;
using System.Collections.Generic;
using System.Text;
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;

namespace Code.Models.REST.Users
{
    /// <summary>
    /// Получение FQInnerLogin для пользователей
    /// </summary>
    public class GetFQTagRequest
    {
        public FQRequestInfo request;

        public GetFQTagRequest()
        {

        }

        /// <summary>
        /// Формирование запроса
        /// </summary>
        public GetFQTagRequest(
            string newUserLogin)
        {
            var login = CredentialHandler.Instance.Credentials.Login;
            var sessionToken = CredentialHandler.Instance.Credentials.tokenB64;            

            if (string.IsNullOrEmpty(newUserLogin))
            {
                throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
            }

            request = new FQRequestInfo(true);
            request.RequestData.actionName = "GetFQTag";
            request.Credentials.Login = login;
            request.Credentials.tokenB64 = sessionToken;           

            request.RequestData.postData = newUserLogin;
        }
    }

    /// <summary>
    /// Ответ
    /// </summary>
    public class GetFQTagResponse : FQResponse
    {
        public string FQTag
        {
            get
            {
                return this.ri.DeserializeResponseData<string>();
            }
        }

        public GetFQTagResponse(ResponseHelper response) : base(response)
        { }
    }
}
