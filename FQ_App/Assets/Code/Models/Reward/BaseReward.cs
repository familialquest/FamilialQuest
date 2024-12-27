using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Code.Models.Reward
{
    public static class BaseReward
    {
        public enum BaseRewardStatus : int
        {
            Registered = 0,
            Purchase = 1,
            Handed = 2,
            None = -1
        }

        public static BaseRewardStatus StatusFromString(string statusString)
        {
            // если там цифровое представление
            int intStatus = 0;
            //int.TryParse(statusString, out intStatus);        
            
            if(statusString == "Доступна")
            {
                intStatus = 0;
            }

            if (statusString == "Ожидает вручения")
            {
                intStatus = 1;
            }

            if (statusString == "Получена")
            {
                intStatus = 2;
            }

            return (BaseRewardStatus)intStatus;
        }

        public static bool CanPurchaseFromString(string canPurchaseString)
        {
            bool result = false;        

            if (canPurchaseString == "true")
            {
                result = true;
            }            

            return result;
        }
    }
}
