using CommonLib;
using CommonTypes;
using RewardService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RewardService.Services
{
    /// <summary>
    /// Интерфейс сервиса
    /// </summary>
    public interface IRewardServices
    {
        Reward GetRewardFromPostData(object inputParams);
       
        Guid AddReward(FQRequestInfo ri, Reward inputReward, List<Guid> availableFor, bool isStartingItem = false);
       
        void PurchaseReward(FQRequestInfo ri);
       
        void GiveReward(FQRequestInfo ri);
        
        void RemoveReward(FQRequestInfo ri, Reward inputReward);

        void RemoveRelatedRewards(FQRequestInfo ri);

        List<Reward> GetAllRewards(FQRequestInfo ri);

        List<Reward> GetRewardsById(FQRequestInfo ri, List<Guid> inputRewards);
    }
}
