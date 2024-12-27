using Code.Controllers.MessageBox;
using Code.Models;
using Code.Models.REST;
using Code.Models.REST.Group;
using Code.Models.REST.Rewards;
using Code.Models.REST.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Code.Controllers
{
    public static class RewardController
    {
        public static RSG.IPromise<DataModelOperationResult> CreateReward(Reward inputReward, List<Guid> availableFor)
        {
            var promise = DataModel.Instance.Rewards.CreateReward(inputReward, availableFor)
                .Then((result) =>
                {
                    Debug.Log($"status: {result.status}");

                    if (result.result)
                    {
                        var prom_UpdateAllRewardsItems = DataModel.Instance.Rewards.UpdateAllRewardsItems()
                            .Then((result_UpdateAllRewardsItems) =>
                            {
                                return result_UpdateAllRewardsItems;
                            });

                        return prom_UpdateAllRewardsItems;
                    }
                    else
                    {
                        //Exception пустой потому что просто для возврата промиса
                        throw new Exception();
                    }
                });

            return promise;
        }

        public static RSG.IPromise<DataModelOperationResult> RemoveReward(Guid removeRewardId)
        {
            var promise = DataModel.Instance.Rewards.RemoveReward(removeRewardId)
                .Then((result) =>
                {
                    Debug.Log($"status: {result.status}");

                    if (result.result)
                    {
                        var prom_UpdateAllRewardsItems = DataModel.Instance.Rewards.UpdateAllRewardsItems()
                            .Then((result_UpdateAllRewardsItems) =>
                            {
                                return result_UpdateAllRewardsItems;
                            });

                        return prom_UpdateAllRewardsItems;
                    }
                    else
                    {
                        //Exception пустой потому что просто для возврата промиса
                        throw new Exception();
                    }
                });

            return promise;
        }

        public static RSG.IPromise<DataModelOperationResult> PurchaseReward(Guid purchaseRewardId)
        {
            var promise = DataModel.Instance.Rewards.PurchaseReward(purchaseRewardId)
                .Then((result) =>
                {
                    Debug.Log($"status: {result.status}");

                    if (result.result)
                    {
                        var prom_UpdateAllRewardsItems = DataModel.Instance.Rewards.UpdateAllRewardsItems()
                            .Then((result_UpdateAllRewardsItems) =>
                            {
                                return result_UpdateAllRewardsItems;
                            });

                        DataModel.Instance.Credentials.UpdateAllCredentials();

                        return prom_UpdateAllRewardsItems;
                    }
                    else
                    {
                        //Exception пустой потому что просто для возврата промиса
                        throw new Exception();
                    }

                });

            return promise;
        }

        public static RSG.IPromise<DataModelOperationResult> GiveReward(Guid giveRewardId)
        {
            var promise = DataModel.Instance.Rewards.GiveReward(giveRewardId)
                .Then((result) =>
                {
                    Debug.Log($"status: {result.status}");

                    if (result.result)
                    {
                        var prom_UpdateAllRewardsItems = DataModel.Instance.Rewards.UpdateAllRewardsItems()
                            .Then((result_UpdateAllRewardsItems) =>
                            {
                                return result_UpdateAllRewardsItems;
                            });
                        
                        return prom_UpdateAllRewardsItems;
                    }
                    else
                    {
                        //Exception пустой потому что просто для возврата промиса
                        throw new Exception();
                    }
                });

            return promise;
        }
    }
}
