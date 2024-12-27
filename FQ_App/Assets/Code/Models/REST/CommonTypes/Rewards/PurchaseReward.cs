using Assets.Code.Models.REST.CommonTypes;
using Newtonsoft.Json;
using Proyecto26;
using System;
using System.Collections.Generic;
using System.Text;
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;

namespace Code.Models.REST.Rewards
{
    /// <summary>
    /// Покупка награды
    /// </summary>
    public class PurchaseRewardRequest
    {
        public FQRequestInfo request;

        public PurchaseRewardRequest()
        {

        }

        /// <summary>
        /// Формирование запроса
        /// </summary>
        /// <param name="rewardId">Идентификатор обновляемой награды</param>
        public PurchaseRewardRequest(
            Guid rewardId
            )
        {
            var login = CredentialHandler.Instance.Credentials.Login;
            var sessionToken = CredentialHandler.Instance.Credentials.tokenB64;

            if (rewardId == Guid.Empty)
            {
                throw new FQServiceException(FQServiceExceptionType.DefaultError);
            }

            request = new FQRequestInfo(true);
            request.RequestData.actionName = "PurchaseReward";
            request.Credentials.Login = login;
            request.Credentials.tokenB64 = sessionToken;

            request.RequestData.postData = JsonConvert.SerializeObject(rewardId);
        }
    }

    /// <summary>
    /// Ответ
    /// </summary>
    public class PurchaseRewardResponse : FQResponse
    {
        public PurchaseRewardResponse(ResponseHelper response) : base(response)
        { }
    }
}
