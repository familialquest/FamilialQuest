using Assets.Code.Models.REST.CommonTypes;
using Code.Controllers.MessageBox;
using Code.Models;
using Code.Models.REST;
using Code.Models.REST.Group;
using Code.Models.REST.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;

namespace Assets.Code.Controllers
{
    public static class GroupController
    {       
        public static RSG.IPromise<DataModelOperationResult> UpdateGroup(string groupName, string groupImg)
        {
            var promise = DataModel.Instance.GroupInfo.UpdateGroup(groupName, groupImg)
                .Then((result) =>
                {
                    Debug.Log($"status: {result.status}");
                    if (result.result)
                    {
                        var prom_UpdateGroupItem = DataModel.Instance.GroupInfo.UpdateGroupItem()
                            .Then((result_UpdateGroupItem) =>
                            {
                                return result_UpdateGroupItem;
                            });

                        return prom_UpdateGroupItem;
                    }
                    else
                    {
                        //Exception пустой потому что просто для возврата промиса
                        throw new Exception();
                    }
                });

            return promise;
        }

        public static RSG.IPromise<DataModelOperationResult> ProccessPurchase(string productId, string purchaseToken)
        {
            var promise = DataModel.Instance.GroupInfo.ProccessPurchase(productId, purchaseToken)
                .Then((result) =>
                {
                    Debug.Log($"status: {result.status}");
                    if (result.result)
                    {
                        //var prom_UpdateGroupItem = DataModel.Instance.GroupInfo.UpdateGroupItem()
                        //    .Then((result_UpdateGroupItem) =>
                        //    {
                        //        return result_UpdateGroupItem;
                        //    });

                        //return prom_UpdateGroupItem;
                    }
                    else
                    {          
                        //Для ошибок таких типов необходима дополнительная обработка в PurchaseWorker-е
                        if (Enum.TryParse(result.ParsedResponse.ri.ResponseData, out FQServiceExceptionType _exType))
                        {
                            if (_exType == FQServiceExceptionType.PurchaseStateIsCanceled ||
                                _exType == FQServiceExceptionType.AcknowledgementStateIsAcknowledged ||
                                _exType == FQServiceExceptionType.PurchaseIsAlreadyExists)
                            {
                                throw new FQServiceException(_exType);
                            }
                        }

                        //Exception пустой потому что просто для возврата промиса
                        throw new Exception();
                    }

                    return result;
                });

            return promise;
        }
    }
}
