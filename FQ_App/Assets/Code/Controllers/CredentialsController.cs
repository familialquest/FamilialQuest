using System;
using System.Collections.Generic;
using Code.Controllers.MessageBox;
using Code.Models;
using Code.Models.REST;
using Code.Models.REST.Users;
using Code.ViewControllers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Controllers
{
    public static class CredentialsController
    {
        public static RSG.IPromise<DataModelOperationResult> AddUser(string newUserLogin, string newUserPasswordHash, User newUserInfo)
        {
            var promise = DataModel.Instance.Credentials.AddUser(newUserLogin, newUserPasswordHash, newUserInfo)
                .Then((result) =>
                {
                    Debug.Log($"status: {result.status}");

                    if (result.result)
                    {
                        var prom_UpdateAllCredentials = DataModel.Instance.Credentials.UpdateAllCredentials()
                            .Then((result_UpdateAllCredentials) =>
                            {
                                return result_UpdateAllCredentials;
                            });

                        return prom_UpdateAllCredentials;
                    }
                    else
                    {
                        //Exception пустой потому что просто для возврата промиса
                        throw new Exception();
                    }                  
                });

            return promise;
        }

        public static RSG.IPromise<DataModelOperationResult> GetFQTag(string newUserLogin)
        {
            var promise = DataModel.Instance.Credentials.GetFQTag(newUserLogin)
                .Then((result) =>
                {
                    Debug.Log($"GetFQTagstatus: {result.status}");

                    return result;
                });

            return promise;
        }

        public static RSG.IPromise<DataModelOperationResult> UpdateUser(User newUserInfo)
        {
            var promise = DataModel.Instance.Credentials.UpdateUser(newUserInfo)
                 .Then((result) =>
                 {
                     Debug.Log($"status: {result.status}");

                     if (result.result)
                     {
                         var prom_UpdateAllCredentials = DataModel.Instance.Credentials.UpdateAllCredentials()
                             .Then((result_UpdateAllCredentials) =>
                             {
                                 return result_UpdateAllCredentials;
                             });

                         return prom_UpdateAllCredentials;
                     }
                     else
                     {
                         //Exception пустой потому что просто для возврата промиса
                         throw new Exception();
                     }
                 });

            return promise;
        }

        public static RSG.IPromise<DataModelOperationResult> RemoveUser(Guid userId)
        {
            var promise = DataModel.Instance.Credentials.RemoveUser(userId)
                .Then((result) =>
                {
                    Debug.Log($"status: {result.status}");
                    if (result.result)
                    {
                        var prom_UpdateAllCredentials = DataModel.Instance.Credentials.UpdateAllCredentials()
                            .Then((result_UpdateAllCredentials) =>
                            {
                                return result_UpdateAllCredentials;
                            });

                        return prom_UpdateAllCredentials;
                    }
                    else
                    {
                        //Exception пустой потому что просто для возврата промиса
                        throw new Exception();
                    }
                });

            return promise;
        }

        public static RSG.IPromise<DataModelOperationResult> ChangeSelfPassword(string passwordCurrent, string passwordNew)
        {
            var promise = DataModel.Instance.Credentials.ChangeSelfPassword(passwordCurrent, passwordNew)
                .Then((result) =>
                {
                    Debug.Log($"status: {result.status}");
                    
                    //Нет смыслы проверять result в целях запуска апдейта - это изменение визульно не отображается,
                    //а мсгбокс покажется выше по стеку
                    //Просто возвращаем result

                    return result;
                });

            return promise;
        }
        
        public static RSG.IPromise<DataModelOperationResult> ChangeGroupUserPassword(Guid userId, string passwordHashCurrent, string passwordHashNew)
        {
            var promise = DataModel.Instance.Credentials.ChangeGroupUserPassword(userId, passwordHashCurrent, passwordHashNew)
                .Then((result) =>
                {
                    Debug.Log($"status: {result.status}");

                    //Нет смыслы проверять result в целях запуска апдейта - это изменение визульно не отображается,
                    //а мсгбокс покажется выше по стеку
                    //Просто возвращаем result

                    return result;
                });

            return promise;
        }         
    }
}
