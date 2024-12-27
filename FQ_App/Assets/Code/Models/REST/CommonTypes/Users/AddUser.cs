using Assets.Code.Models.REST.CommonTypes;
using Code.Models.RoleModel;
using Newtonsoft.Json;
using Proyecto26;
using System;
using System.Collections.Generic;
using System.Text;
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;

namespace Code.Models.REST.Users
{
    /// <summary>
    /// Добавление пользователя в группу
    /// </summary>
    public class AddUserRequest
    {
        public FQRequestInfo request;

        public AddUserRequest()
        {

        }

        /// <summary>
        /// Формирование запроса
        /// </summary>
        /// <param name="newUserLogin">Логин добавляемого пользователя</param>
        /// <param name="newUserpasswordHash">Хэш пароля добавляемого пользователя</param>
        /// <param name="newUserName">Отображаемое имя добавляемого пользователя</param>
        /// <param name=""></param>
        public AddUserRequest(
            string newUserLogin,
            string newUserPasswordHash,
            User newUserInfo
            )
        {
            var login = CredentialHandler.Instance.Credentials.Login;
            var sessionToken = CredentialHandler.Instance.Credentials.tokenB64;

            string newUserName = newUserInfo.Name;
            RoleTypes newUserRole = newUserInfo.Role;
            string newUserTitle = newUserInfo.Title;
            string newUserImage = newUserInfo.Image;

            if (string.IsNullOrEmpty(newUserLogin) ||
                string.IsNullOrEmpty(newUserPasswordHash) ||
                string.IsNullOrWhiteSpace(newUserName))
            {
                throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
            }

            request = new FQRequestInfo(true);
            request.RequestData.actionName = "AddUserToGroup";
            request.Credentials.Login = login;
            request.Credentials.tokenB64 = sessionToken;
                        
            Dictionary<string, string> accountAndUser = new Dictionary<string, string>();
            accountAndUser.Add("login", newUserLogin);
            accountAndUser.Add("passwordHash", newUserPasswordHash);
            accountAndUser.Add("name", newUserName);
            accountAndUser.Add("role", ((int)newUserRole).ToString());

            if (!string.IsNullOrWhiteSpace(newUserTitle))
            {
                accountAndUser.Add("title", newUserTitle);
            }
            if (!string.IsNullOrEmpty(newUserImage))
            {
                accountAndUser.Add("image", newUserImage);
            }
            
            request.RequestData.postData = JsonConvert.SerializeObject(accountAndUser);
        }
    }

    /// <summary>
    /// Ответ
    /// </summary>
    public class AddUserResponse : FQResponse
    {
        public AddUserResponse(ResponseHelper response) : base(response)
        { }
    }
}
