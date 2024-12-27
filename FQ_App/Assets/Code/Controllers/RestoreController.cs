using Code.Controllers.MessageBox;
using Code.Models;
using UnityEngine;

namespace Code.Controllers
{
    public static class RestoreController
    {
        public static RSG.IPromise<DataModelOperationResult> Restore(string login, string passwordNew)
        {
            RestoreModel rm = new RestoreModel();

            var promise = rm.Restore(login, passwordNew)
                .Then((result) =>
                {
                    Debug.Log($"status: {result.status}");
                    
                    return result;
                });

            return promise;
        }

        public static RSG.IPromise<DataModelOperationResult> RestoreConfirm(string login, string confirmCode)
        {
            RestoreModel rm = new RestoreModel();

            var promise = rm.RestoreConfirm(login, confirmCode)
                .Then((result) =>
                {
                    Debug.Log($"status: {result.status}");
                    
                    return result;
                });

            return promise;
        }
    }
}
