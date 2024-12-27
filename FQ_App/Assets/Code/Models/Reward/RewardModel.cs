using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Code.Controllers;
using Code.Controllers.MessageBox;
using Code.Models.REST;
using Code.Models.REST.Administrative;
using Code.Models.REST.Group;
using Code.Models.REST.Rewards;
using Code.Models.REST.Users;
using Newtonsoft.Json;
using Proyecto26;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Assets.Code.Models.Reward.BaseReward;
using static Code.Models.REST.Rewards.Reward;

namespace Code.Models
{
    public class RewardModel
    {
        public enum BaseRewardFilter
        {
            Available = 0,
            Obtained
        }
        public enum AvailableRewardFilter
        {
            All = 0,
            CanBeBought,
            CanNotBeBought
        }
        public enum ObtainedRewardFilter
        {
            All = 0,
            Received,
            Handled
        }

        public event EventHandler<EventArgs> OnListChanged;

        protected virtual void ListChanged(EventArgs e)
        {
            EventHandler<EventArgs> handler = OnListChanged;
            handler?.Invoke(this, e);
        }

        //Метод для принудительного обновления UI страницы извне
        public void UpdatePageUI()
        {
            ListChanged(EventArgs.Empty);
        }

        public RewardModel() { }

        private List<Reward> rewards = null;
        public List<Reward> Rewards { get => rewards; set => rewards = value; }
        public List<Reward> AvailableRewards
        {
            get
            {
                if (Rewards != null)
                {
                    return Rewards.FindAll((item) => (item.Status == BaseRewardStatus.Registered));
                }
                else
                {
                    return new List<Reward>();
                }
            }
        }
        public List<Reward> ObtainedRewards
        {
            get
            {
                if (Rewards != null)
                {
                    return Rewards.FindAll((item) => (item.Status != BaseRewardStatus.Registered));
                }
                else
                {
                    return new List<Reward>();
                }                
            }
        }

        /// <summary>
        /// Функция получает список наград списком диктов. Нужно для отображения в списке. 
        /// Списки мои заточены под списки диктов.
        /// </summary>
        /// <param name="reward">Награда</param>
        /// <returns></returns>
        public static List<Dictionary<string, object>> ToListOfDictionary<T>(List<T> objectList)
        {
            if (objectList == null)
            {
                return new List<Dictionary<string, object>>();
            }

            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            foreach (var item in objectList)
            {
                list.Add(item.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(item, null)));
            }
            return list;
        }

        public RSG.IPromise<DataModelOperationResult> GetAllRewards()
        {
            GetAllRewardsRequest req = new GetAllRewardsRequest();

            var prom = RestClientEx.PostEx(req.request)
                .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new GetAllRewardsResponse(res.RawResponse)))
                .Catch((ex) => DataModelOperationResult.Wrap(ex));

            return prom;                        
        }

        public RSG.IPromise<DataModelOperationResult> UpdateAllRewardsItems()
        {
            var prom = GetAllRewards()
                .Then((res) =>
                {
                    if (res.result)
                    {
                        Debug.Log(((GetAllRewardsResponse)res.ParsedResponse).Rewards?.Count);
                        Rewards = ((GetAllRewardsResponse)res.ParsedResponse).Rewards;
                        ListChanged(EventArgs.Empty);
                    }
                    
                    return res;
                });

            return prom;
        }

        public RSG.IPromise<DataModelOperationResult> CreateReward(Reward inputReward, List<Guid> availableFor)
        {
            CreateRewardRequest req = new CreateRewardRequest(inputReward, availableFor);

            var prom = RestClientEx.PostEx(req.request)
               .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new CreateRewardResponse(res.RawResponse)))
               .Catch((ex) => DataModelOperationResult.Wrap(ex));

            return prom;
        }

        public RSG.IPromise<DataModelOperationResult> RemoveReward(Guid removeRewardId)
        {
            RemoveRewardRequest req = new RemoveRewardRequest(removeRewardId);

            var prom = RestClientEx.PostEx(req.request)
               .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new RemoveRewardResponse(res.RawResponse)))
               .Catch((ex) => DataModelOperationResult.Wrap(ex));

            return prom;
        }

        public RSG.IPromise<DataModelOperationResult> PurchaseReward(Guid purchaseRewardId)
        {
            PurchaseRewardRequest req = new PurchaseRewardRequest(purchaseRewardId);

            var prom = RestClientEx.PostEx(req.request)
              .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new PurchaseRewardResponse(res.RawResponse)))
              .Catch((ex) => DataModelOperationResult.Wrap(ex));

            return prom;
        }

        public RSG.IPromise<DataModelOperationResult> GiveReward(Guid giveRewardId)
        {
            GiveRewardRequest req = new GiveRewardRequest(giveRewardId);

            var prom = RestClientEx.PostEx(req.request)
              .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new GiveRewardResponse(res.RawResponse)))
              .Catch((ex) => DataModelOperationResult.Wrap(ex));

            return prom;
        }
    }
}
