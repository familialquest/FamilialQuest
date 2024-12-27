using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Code.Models.REST;
using Code.Models.REST.Administrative;
using Proyecto26;
using UnityEngine;

namespace Code.Models
{
    public class AuthModel
    {
        public AuthModel() { }

        public RSG.IPromise<DataModelOperationResult> Auth(string login, string password, string token)
        {
            string passwordHash = AuthModel.GetPasswordHash(login, password);

            AuthRequest req = new AuthRequest(login, passwordHash, token);

            var prom = RestClientEx.PostEx(req.request)
               .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new AuthResponse(res.RawResponse)))
               .Catch((ex) => DataModelOperationResult.Wrap(ex));

            return prom;
        }

        public RSG.IPromise<DataModelOperationResult> Logout()
        {
            LogoutRequest req = new LogoutRequest(CredentialHandler.Instance.Credentials.Login, CredentialHandler.Instance.Credentials.tokenB64);

            var prom = RestClientEx.PostEx(req.request)
               .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new FQResponse(res.RawResponse)))
               .Catch((ex) => DataModelOperationResult.Wrap(ex));

            return prom;
        }

        public static string GetPasswordHash(string login, string password)
        {
            string passwordHash = string.Empty;

            byte[] saltBytes = Encoding.UTF8.GetBytes(login.ToLower());
            byte[] passBytes = Encoding.UTF8.GetBytes(password);
            byte[] secondSaltBytes = Encoding.UTF8.GetBytes("[TODO]");

            byte[] plainTextWithSaltBytes =
                new byte[passBytes.Length + saltBytes.Length + secondSaltBytes.Length];

            // Copy plain text bytes into resulting array.
            for (int i = 0; i < passBytes.Length; i++)
                plainTextWithSaltBytes[i] = passBytes[i];

            // Append salt bytes to the resulting array.
            for (int i = 0; i < saltBytes.Length; i++)
                plainTextWithSaltBytes[passBytes.Length + i] = saltBytes[i];

            // Append salt bytes to the resulting array.
            for (int i = 0; i < secondSaltBytes.Length; i++)
                plainTextWithSaltBytes[passBytes.Length + saltBytes.Length + i] = secondSaltBytes[i];

            var hash = new SHA256Managed();

            // Compute hash value of our plain text with appended salt.
            byte[] hashBytes = hash.ComputeHash(plainTextWithSaltBytes);

            passwordHash = Convert.ToBase64String(hashBytes);

            return passwordHash;
        }
    }
}
