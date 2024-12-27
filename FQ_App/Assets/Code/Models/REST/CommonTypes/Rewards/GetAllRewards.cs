using Assets.Code.Models.REST.CommonTypes;
using Proyecto26;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.SceneManagement;
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;

namespace Code.Models.REST.Rewards
{
    /// <summary>
    /// Получение всех наград
    /// </summary>
    public class GetAllRewardsRequest
    {
        public FQRequestInfo request;

        /// <summary>
        /// Формирование запроса
        /// </summary>
        public GetAllRewardsRequest()
        {
            var login = CredentialHandler.Instance.Credentials.Login;
            var sessionToken = CredentialHandler.Instance.Credentials.tokenB64;

            request = new FQRequestInfo(true);
            request.RequestData.actionName = "GetAllRewards";
            request.Credentials.Login = login;
            request.Credentials.tokenB64 = sessionToken;
        }
    }

    /// <summary>
    /// Ответ
    /// </summary>
    public class GetAllRewardsResponse : FQResponse
    {
        public List<Reward> Rewards
        {
            get
            {
                return this.ri.DeserializeResponseData<List<Reward>>();
            }
        }

        public GetAllRewardsResponse(ResponseHelper response) : base(response)
        { }
    }
}
