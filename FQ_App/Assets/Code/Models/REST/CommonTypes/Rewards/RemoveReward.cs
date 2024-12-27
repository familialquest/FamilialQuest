using Assets.Code.Models.REST.CommonTypes;
using Proyecto26;
using System;
using System.Collections.Generic;
using System.Text;
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;

namespace Code.Models.REST.Rewards
{
    /// <summary>
    /// Удаление награды
    /// </summary>
    public class RemoveRewardRequest
    {
        public FQRequestInfo request;

        public RemoveRewardRequest()
        {

        }

        /// <summary>
        /// Формирование запроса
        /// </summary>
        /// <param name="removedRewardId">Идентификатор удаляемой награды</param>
        /// <param name=""></param>
        public RemoveRewardRequest(
            Guid removedRewardId)
        {
            var login = CredentialHandler.Instance.Credentials.Login;
            var sessionToken = CredentialHandler.Instance.Credentials.tokenB64;

            if (removedRewardId == Guid.Empty)
            {
                throw new FQServiceException(FQServiceExceptionType.DefaultError);
            }

            request = new FQRequestInfo(true);
            request.RequestData.actionName = "RemoveReward";
            request.Credentials.Login = login;
            request.Credentials.tokenB64 = sessionToken;

            Reward reward = new Reward();
            reward.Id = removedRewardId;

            request.RequestData.postData = reward;
        }
    }

    /// <summary>
    /// Ответ
    /// </summary>
    public class RemoveRewardResponse : FQResponse
    {
        public RemoveRewardResponse(ResponseHelper response) : base(response)
        { }
    }
}



