using Assets.Code.Models.REST.CommonTypes;
using Newtonsoft.Json;
using Proyecto26;
using System;
using System.Collections.Generic;
using System.Text;
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;

namespace Code.Models.REST.Group
{
    /// <summary>
    /// Запрос на обновление полей группы
    /// </summary>
    class ProccessPurchaseRequest
    {
        public FQRequestInfo request;

        public ProccessPurchaseRequest()
        {

        }

        public ProccessPurchaseRequest(
            string productId,
            string purchaseToken)
        {
            var login = CredentialHandler.Instance.Credentials.Login;
            var sessionToken = CredentialHandler.Instance.Credentials.tokenB64;

            if (string.IsNullOrEmpty(productId) ||
                string.IsNullOrEmpty(purchaseToken))

            {
                throw new FQServiceException(FQServiceExceptionType.DefaultError);
            }

            request = new FQRequestInfo(true);
            request.RequestData.actionName = "ProcessPurchase";
            request.Credentials.Login = login;
            request.Credentials.tokenB64 = sessionToken;

            Dictionary<string, string> requestParametrs = new Dictionary<string, string>();
            
            requestParametrs.Add("productId", productId);
            requestParametrs.Add("purchaseToken", purchaseToken);

            request.RequestData.postData = JsonConvert.SerializeObject(requestParametrs);
        }
    }

    /// <summary>
    /// Ответ
    /// </summary>
    public class ProccessPurchaseResponse : FQResponse
    {
        public ProccessPurchaseResponse(ResponseHelper response) : base(response)
        { }
    }
}
