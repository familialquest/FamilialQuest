using CommonLib;
using CommonTypes;
using System.Collections.Generic;

namespace NotificationService.Services
{
    public interface INotificationServices
    {
        bool RegisterDevice(FQRequestInfo inputParams);
        bool UnregisterDevice(FQRequestInfo inputParams);
        bool UnregisterDeviceInner(FQRequestInfo inputParams);

        void NotifyUsers(FQRequestInfo inputParams);
        void NotifyGroup(FQRequestInfo inputParams);
        bool SetSubscriptionForUser(FQRequestInfo inputParams);        
    }
}
