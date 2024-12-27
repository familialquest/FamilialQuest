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
    public class GiveRewardRequest
    {
        public FQRequestInfo request;

        public GiveRewardRequest()
        {

        }

        /// <summary>
        /// Формирование запроса
        /// </summary>
        /// <param name="rewardId">Идентификатор обновляемой награды</param>
        public GiveRewardRequest(
            Guid rewardId
            )
        {
            if (rewardId == Guid.Empty)
            {
                throw new FQServiceException(FQServiceExceptionType.DefaultError);
            }

            var login = CredentialHandler.Instance.Credentials.Login;
            var sessionToken = CredentialHandler.Instance.Credentials.tokenB64;

            request = new FQRequestInfo(true);
            request.RequestData.actionName = "GiveReward";
            request.Credentials.Login = login;
            request.Credentials.tokenB64 = sessionToken;

            request.RequestData.postData = JsonConvert.SerializeObject(rewardId);
        }
    }

    /// <summary>
    /// Ответ
    /// </summary>
    public class GiveRewardResponse : FQResponse
    {
        public GiveRewardResponse(ResponseHelper response) : base(response)
        { }
    }
}
