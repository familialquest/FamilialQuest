using Code.Controllers.MessageBox;
using Code.Models;
using Code.Models.REST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Code.Controllers
{
    public static class AuthController
    {
        public static RSG.IPromise<DataModelOperationResult> Auth(string login, string password = "", string token = "")
        {
            var promise = DataModel.Instance.Auth.Auth(login, password, token)
                .Then((result) =>
                {
                    Debug.Log($"status: {result.status}");
                    return result;
                });

            return promise;
        }

        public static RSG.IPromise<DataModelOperationResult> Logout()
        {
            var promise = DataModel.Instance.Auth.Logout()
                .Then((result) =>
                {
                    Debug.Log($"status: {result.status}");
                    return result;
                });

            return promise;
        }
    }
}
