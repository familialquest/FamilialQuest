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
    class UpdateGroupRequest
    {
        public FQRequestInfo request;

        public UpdateGroupRequest()
        {

        }


        /// <summary>
        /// Формирование запроса
        /// </summary>
        /// <param name="newGroupName">Новое имя группы (если не требует обновления - null)</param>
        /// <param name="newGroupImage">Новое аватарка группы (если не требует обновления - null)</param>
        public UpdateGroupRequest(
            string newGroupName,
            string newGroupImage)
        {
            var login = CredentialHandler.Instance.Credentials.Login;
            var sessionToken = CredentialHandler.Instance.Credentials.tokenB64;

            if (newGroupName != null && string.IsNullOrWhiteSpace(newGroupName))
            {
                throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
            }

            request = new FQRequestInfo(true);
            request.RequestData.actionName = "UpdateGroup";
            request.Credentials.Login = login;
            request.Credentials.tokenB64 = sessionToken;

            Dictionary<string, string> updatedParametrs = new Dictionary<string, string>();

            if (newGroupName != null)
            {
                updatedParametrs.Add("name", newGroupName);
            }

            if (newGroupImage != null)
            {
                updatedParametrs.Add("image", newGroupImage);
            }

            request.RequestData.postData = JsonConvert.SerializeObject(updatedParametrs);
        }
    }

    /// <summary>
    /// Ответ
    /// </summary>
    public class UpdateGroupResponse : FQResponse
    {
        public UpdateGroupResponse(ResponseHelper response) : base(response)
        { }
    }
}
