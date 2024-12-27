using Code.Controllers.MessageBox;
using Code.Models;
using UnityEngine;

namespace Code.Controllers
{
    public static class RegistrationController
    {
        public static RSG.IPromise<DataModelOperationResult> Reg(string login, string password)
        {
            RegistrationModel rm = new RegistrationModel();
            var promise = rm.Registration(login, password)
                .Then((result) =>
                {
                    Debug.Log($"status: {result.status}");                    

                    return result;
                });

            return promise;
        }

        public static RSG.IPromise<DataModelOperationResult> RegConfirm(string login, string confirmCode)
        {
            RegistrationModel rm = new RegistrationModel();
            var promise = rm.RegistrationConfirm(login, confirmCode)
                .Then((result) =>
                {
                    Debug.Log($"status: {result.status}");                   

                    return result;
                });

            return promise;
        }
    }
}
