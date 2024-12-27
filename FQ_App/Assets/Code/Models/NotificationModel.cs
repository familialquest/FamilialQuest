using Code.Models.REST;
using Code.Models.REST.Notifications;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Models
{
    public class NotificationModel
    {
        readonly string notificationServiceUri = "http://localhost:56006/";

        internal RSG.IPromise<DataModelOperationResult> RegisterDevice(Guid userId, string deviceId, string regToken)
        {
            try
            {
                Debug.Log("RegisterDevice");
                NotifiedDevice notifiedDevice = new NotifiedDevice()
                {
                    UserId = userId,
                    DeviceId = deviceId,
                    RegToken = regToken
                };
                RegisterDeviceRequest registerDeviceRequest = new RegisterDeviceRequest(notifiedDevice);

                var prom = RestClientEx.PostEx(registerDeviceRequest.RequestInfo)
                    .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new RegisterDeviceResponse(res.RawResponse)))
                    .Catch((ex) => DataModelOperationResult.Wrap(ex));

                return prom;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return null;
            }
        }

        internal RSG.IPromise<DataModelOperationResult> UnregisterDevice(Guid userId, string deviceId, string regToken)
        {
            try
            {
                Debug.Log("RegisterDevice");
                NotifiedDevice notifiedDevice = new NotifiedDevice()
                {
                    UserId = userId,
                    DeviceId = deviceId,
                    RegToken = regToken
                };
                UnregisterDeviceRequest registerDeviceRequest = new UnregisterDeviceRequest(notifiedDevice);

                Debug.Log("Be4 PostEx");
                var prom = RestClientEx.PostEx(registerDeviceRequest.RequestInfo)
                    .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new UnregisterDeviceResponse(res.RawResponse)))
                    .Catch((ex) => DataModelOperationResult.Wrap(ex));

                return prom;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return null;
            }
        }
        internal RSG.IPromise<DataModelOperationResult> SetSubscription(Guid userId, bool isSubscribed)
        {
            try
            {
                SetSubscriptionForUserRequest setSubscriptionForUserRequest = new SetSubscriptionForUserRequest(userId, isSubscribed);

                var prom = RestClientEx.PostEx(setSubscriptionForUserRequest.RequestInfo)
                    .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new SetSubscriptionForUserResponse(res.RawResponse)))
                    .Catch((ex) => DataModelOperationResult.Wrap(ex));

                return prom;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return null;
            }
        }
    }
}
