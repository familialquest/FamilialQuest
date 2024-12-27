using Assets.Code.Models.REST.CommonTypes;
using Newtonsoft.Json;
using Proyecto26;
using System;
using System.Collections.Generic;
using System.Text;
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;

namespace Code.Models.REST.Users
{
    /// <summary>
    /// Обновление полей пользователя
    /// </summary>
    public class UpdateUserRequest
    {
        public FQRequestInfo request;

        public UpdateUserRequest()
        {

        }


        /// <summary>
        /// Формирование запроса
        /// </summary>
        /// <param name="updatedUserId">Идентификатор обновляемого пользователя</param>
        /// <param name="updatedUserName">Отображаемое имя обновляемого пользователя (если не требует обновления - null)</param>
        /// <param name="updatedUserTitle">Титул обновляемого пользователя (если не требует обновления - null)</param>
        /// <param name="updatedUserRole">Роль обновляемого пользователя (если не требует обновления - null)</param>
        /// <param name="updatedUserImage">Аватарка обновляемого пользователя (если не требует обновления - null)</param>
        public UpdateUserRequest(
            User newUserInfo
            )
        {
            var login = CredentialHandler.Instance.Credentials.Login;
            var sessionToken = CredentialHandler.Instance.Credentials.tokenB64;

            Guid updatedUserId = newUserInfo.Id;
            string updatedUserName = newUserInfo.Name;
            string updatedUserTitle = newUserInfo.Title;
            //int? updatedUserRole = newUserInfo.Role;
            //string updatedUserImage = newUserInfo.Image;

            if (updatedUserId == Guid.Empty)
            {
                throw new FQServiceException(FQServiceExceptionType.DefaultError);
            }

            //if (updatedUserRole != null && updatedUserRole != 0 && updatedUserRole != 1)
            //{
            //    throw new Exception("Ошибка: некорректно указана роль пользователя");
            //}

            if (updatedUserName != null && string.IsNullOrWhiteSpace(updatedUserName))
            {
                throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
            }

            request = new FQRequestInfo(true);
            request.RequestData.actionName = "UpdateUser";
            request.Credentials.Login = login;
            request.Credentials.tokenB64 = sessionToken;

            //User updatedUser = new User();
            //updatedUser.Id = updatedUserId;
            //updatedUser.Name = updatedUserName;
            //updatedUser.Title = updatedUserTitle;
            //updatedUser.Role = updatedUserRole;
            //updatedUser.Image = updatedUserImage;

            Dictionary<string, string> updatedUser = new Dictionary<string, string>();
            updatedUser.Add("id", updatedUserId.ToString());

            if (updatedUserName != null)
            {
                updatedUser.Add("name", updatedUserName);
            }

            if (updatedUserTitle != null)
            {
                updatedUser.Add("title", updatedUserTitle);
            }

            //if (updatedUserRole != null)            
            //{
            //    updatedUser.Add("role", updatedUserRole.ToString());
            //}

            //if (updatedUserImage != null)
            //{
            //    updatedUser.Add("image", updatedUserImage);
            //}

            request.RequestData.postData = JsonConvert.SerializeObject(updatedUser);
        }
    }

    /// <summary>
    /// Ответ
    /// </summary>
    public class UpdateUserResponse : FQResponse
    {
        public UpdateUserResponse(ResponseHelper response) : base(response)
        { }
    }
}
