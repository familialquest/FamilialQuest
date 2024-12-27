using Assets.Code.Models.REST.CommonTypes;
using Proyecto26;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;

namespace Code.Models.REST.Administrative
{
    /// <summary>
    /// Аутентификация по паролю
    /// </summary>
    public class AuthRequest
    {
        public FQRequestInfo request;

        public AuthRequest()
        {

        }

        /// <summary>
        /// Формирование запроса
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <param name="passwordHash">Хэш пароля пользователя</param>
        public AuthRequest(string login, string passwordHash, string token)
        {
            if (string.IsNullOrEmpty(login) || 
                (string.IsNullOrEmpty(passwordHash) && string.IsNullOrEmpty(token)))
            {
                throw new FQServiceException(FQServiceExceptionType.EmptyRequiredField);
            }

            request = new FQRequestInfo(true);
            request.RequestData.actionName = "Auth";
            request.Credentials.Login = login;

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Credentials.tokenB64 = token;
            }
            else
            {
                request.Credentials.PasswordHash = passwordHash;
            }
            
            request.Credentials.DeviceId = SystemInfo.deviceUniqueIdentifier;
        }
    }

    /// <summary>
    /// Результат запроса
    /// </summary>
    public class AuthResponse : FQResponse
    {
        public AuthResponse(ResponseHelper response) : base(response)
        {
            try
            {
                Debug.Log($"Response: {response.Text}");

                FQResponse parsedResponse = new FQResponse(response);

                CredentialHandler.Instance.Credentials.tokenB64 = parsedResponse.ri.ActualToken;
                CredentialHandler.Instance.Credentials.userId = parsedResponse.ri.UserId;
            }
            catch
            {
                throw;
            }
        }
    }
}
