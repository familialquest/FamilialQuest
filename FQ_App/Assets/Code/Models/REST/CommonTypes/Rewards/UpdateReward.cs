//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Code.Models.REST.Rewards
//{
//    /// <summary>
//    /// Обновление награды
//    /// </summary>
//    public class UpdateRewardRequest
//    {
//        public FQRequestInfo request;

//        public UpdateRewardRequest()
//        {

//        }

//        /// <summary>
//        /// Формирование запроса
//        /// </summary>
//        /// <param name="login">Логин пользователя</param>
//        /// <param name="sessionToken">Токен сессии</param>
//        /// <param name="rewardId">Идентификатор обновляемой награды</param>
//        /// <param name="newRewardTitle">Отображаемое имя нагрды (если не требует обновления - null)</param>
//        /// <param name="newRewardDescription">Описание (если не требует обновления - null)</param>
//        /// <param name="newRewardCost">Стоимость (если не требует обновления - null)</param>
//        /// <param name="newRewardImage">Иконка (если не требует обновления - null)</param>
//        public UpdateRewardRequest(
//            string login,
//            string sessionToken,
//            Guid rewardId,
//            string newRewardTitle,
//            string newRewardDescription,
//            int? newRewardCost,
//            string newRewardImage)
//        {
//            if (string.IsNullOrEmpty(login) ||
//                string.IsNullOrEmpty(sessionToken) ||
//                rewardId == Guid.Empty)
//            {
//                throw new Exception("Ошибка: не заполнены обязательные поля");
//            }

//            request = new FQRequestInfo(true);
//            request.RequestData.actionName = "UpdateReward";
//            request.Credentials.Login = login;
//            request.Credentials.tokenB64 = sessionToken;

//            Reward reward = new Reward();
//            reward.Title = newRewardTitle;
//            reward.Description = newRewardDescription;
//            reward.Cost = newRewardCost;
//            reward.Image = newRewardImage;

//            request.RequestData.postData = reward;
//        }
//    }

//    /// <summary>
//    /// Ответ
//    /// </summary>
//    public class UpdateRewardResponse
//    {
//        /// <summary>
//        /// response.actualToken  //string base64 (new sessinon token)
//        /// </summary>
//        public FQResponseInfo response;
//    }
//}
