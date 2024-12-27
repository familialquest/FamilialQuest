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
    /// Получение указаных наград по Id
    /// </summary>
    public class GetRewardsByIdRequest
    {
        public FQRequestInfo request;

        public GetRewardsByIdRequest()
        {

        }

        /// <summary>
        /// Формирование запроса
        /// </summary>
        public GetRewardsByIdRequest(
            List<Guid> selectedRewards)
        {
            var login = CredentialHandler.Instance.Credentials.Login;
            var sessionToken = CredentialHandler.Instance.Credentials.tokenB64;

            if (selectedRewards == null || selectedRewards.Count == 0 || selectedRewards.Contains(Guid.Empty))
            {
                throw new FQServiceException(FQServiceExceptionType.DefaultError);
            }

            request = new FQRequestInfo(true);
            request.RequestData.actionName = "GetRewardsById";
            request.Credentials.Login = login;
            request.Credentials.tokenB64 = sessionToken;
            request.RequestData.postData = JsonConvert.SerializeObject(selectedRewards);
        }
    }

    /// <summary>
    /// Ответ
    /// </summary>
    public class GetRewardsByIdResponse : FQResponse
    {
        public List<Reward> Rewards
        {
            get
            {
                return this.ri.DeserializeResponseData<List<Reward>>();
            }
        }

        public GetRewardsByIdResponse(ResponseHelper response) : base(response)
        { }
    }
}
