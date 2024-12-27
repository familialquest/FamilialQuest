using Assets.Code.Models.REST.CommonTypes;
using Newtonsoft.Json;
using Proyecto26;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;

namespace Code.Models.REST.Rewards
{
    /// <summary>
    /// Добавление новой награды
    /// </summary>
    public class CreateRewardRequest
    {
        public FQRequestInfo request;

        public CreateRewardRequest()
        {

        }

        /// <summary>
        /// Формирование запроса
        /// </summary>
        /// <param name="newRewardTitle">Отображаемое имя нагрды</param>
        /// <param name="newRewardDescription">Описание (не обязательное)</param>
        /// <param name="newRewardCost">Стоимость</param>
        /// <param name="newRewardImage">Иконка</param>
        public CreateRewardRequest(
            Reward inputReward,
            List<Guid> availableFor
            )
        {
            var login = CredentialHandler.Instance.Credentials.Login;
            var sessionToken = CredentialHandler.Instance.Credentials.tokenB64;

            if (string.IsNullOrEmpty(inputReward.Title) ||
                inputReward.Cost <= 0 ||
                availableFor.Where(x => x != Guid.Empty).Count() == 0
                )
            {
                throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
            }

            request = new FQRequestInfo(true);
            request.RequestData.actionName = "AddReward";
            request.Credentials.Login = login;
            request.Credentials.tokenB64 = sessionToken;

            Dictionary<string, object> postData = new Dictionary<string, object>();
            postData.Add("reward", inputReward);
            postData.Add("availableFor", availableFor);

            request.RequestData.postData = JsonConvert.SerializeObject(postData);
        }
    }

    /// <summary>
    /// Ответ
    /// </summary>
    public class CreateRewardResponse : FQResponse
    {
        public CreateRewardResponse(ResponseHelper response) : base(response)
        { }
    }
}
