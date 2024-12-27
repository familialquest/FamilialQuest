using System;
using System.Collections.Generic;

using Code.Models.REST;
using Code.Models.REST.Administrative;
using Proyecto26;
using UnityEngine;

namespace Code.Models
{
    public class RegistrationModel
    {
        public RegistrationModel() { }

        public RSG.IPromise<DataModelOperationResult> Registration(string login, string password)
        {
            string passwordHash = AuthModel.GetPasswordHash(login, password);

            RegistrationRequest req = new RegistrationRequest(login, passwordHash);

            var prom = RestClientEx.PostEx(req.request)
               .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new RegistrationResponse(res.RawResponse)))
               .Catch((ex) => DataModelOperationResult.Wrap(ex));

            return prom;
        }

        public RSG.IPromise<DataModelOperationResult> RegistrationConfirm(string login, string confirmCode)
        {
            RegistrationConfirmRequest req = new RegistrationConfirmRequest(login, confirmCode);

            var prom = RestClientEx.PostEx(req.request)
               .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new RegistrationConfirmResponse(res.RawResponse)))
               .Catch((ex) => DataModelOperationResult.Wrap(ex));

            return prom;
        }
    }
}
