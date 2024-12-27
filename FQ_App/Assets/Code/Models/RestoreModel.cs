using System;
using System.Collections.Generic;

using Code.Models.REST;
using Code.Models.REST.Administrative;
using Proyecto26;
using UnityEngine;

namespace Code.Models
{
    public class RestoreModel
    {
        public string LoginName;

        public RestoreModel() { }

        public RSG.IPromise<DataModelOperationResult> Restore(string login, string passwordNew)
        {
            string passwordHash = AuthModel.GetPasswordHash(login, passwordNew);

            ResetPasswordRequest req = new ResetPasswordRequest(login, passwordHash);

            var prom = RestClientEx.PostEx(req.request)
                .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new ResetPasswordResponse(res.RawResponse)))
                .Catch((ex) => DataModelOperationResult.Wrap(ex));

            return prom;
        }

        public RSG.IPromise<DataModelOperationResult> RestoreConfirm(string login, string confirmCode)
        {
            ResetPasswordConfirmRequest req = new ResetPasswordConfirmRequest(login, confirmCode);

            var prom = RestClientEx.PostEx(req.request)
                .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new ResetPasswordConfirmResponse(res.RawResponse)))
                .Catch((ex) => DataModelOperationResult.Wrap(ex));

            return prom;
        }       
    }
}
