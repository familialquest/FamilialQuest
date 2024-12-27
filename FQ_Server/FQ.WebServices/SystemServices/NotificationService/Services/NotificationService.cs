using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommonLib;
using CommonRoutes;
using CommonTypes;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NotificationService.Models;
using static CommonLib.FQServiceException;

namespace NotificationService.Services
{
    /// <summary>
    /// Реализация интерфейса сервиса-приёмника запросов клиента
    /// </summary>
    public class NotificationServices : INotificationServices
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        private List<Dictionary<string, object>> targetTasks = new List<Dictionary<string, object>>();
        private List<Reward> targetRewards = new List<Reward>();
        private List<User> targetUsers = new List<User>();

        private Dictionary<HistoryEvent.MessageTypeEnum, Dictionary<string, string>> MessagTypeText_Parent;
        private Dictionary<HistoryEvent.MessageTypeEnum, Dictionary<string, string>> MessagTypeText_Children;

        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Default constructor with HTTPContext
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        public NotificationServices(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        //public void Test_SendNotificationToAll(FQRequestInfo ri)
        //{
        //    try
        //    {
        //        logger.Trace("Test_SendNotificationToAll started");
                
        //        // This registration token comes from the client FCM SDKs.
        //        var registrationToken = "ex63gRZPTbGlZoOYvT-5Ex:APA91bHeSjPSEe_aJ_1PrkTN7p5AD-bBCAWQEkP32qmmzhDJgyLsKYTZelLzwZyIOhELIKtzpiYpp_kBBQsNi2JffFSZZ8JwDEvs9EJQxsJ6uctdWeZ9tpaQc3ZY49D3cqGEuqciTAU-";

        //        // See documentation on defining a message payload.
        //        var message = new Message()
        //        {
        //            Data = new Dictionary<string, string>()
        //            {
        //                { "score", "850" },
        //                { "time", "2:45" },
        //            },
        //            Token = registrationToken,
        //        };

        //        message.Notification = new Notification()
        //        {
        //            Title = "TEST1",
        //            Body = "TESTBODY1 " + DateTime.Now.ToShortTimeString(),
        //            ImageUrl = "http://familialquest.com/testsite/images/faicons/newIcon.png" //info.ImageURL
        //        };

        //        // Send a message to the device corresponding to the provided
        //        // registration token.
        //        var response = FirebaseMessaging.DefaultInstance.SendAsync(message);
        //        response.Wait();
        //        // Response is a message ID string.
        //        logger.Debug("Successfully sent message: " + response.Result);
        //    }
        //    catch (FQServiceException fqEx)
        //    {
        //        logger.Error(fqEx);
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex);
        //        throw new Exception(FQServiceExceptionType.DefaultError.ToString());
        //    }
        //    finally
        //    {
        //        logger.Trace("Test_SendNotificationToAll leave.");
        //    }
        //}
                       
        public bool RegisterDevice(FQRequestInfo inputParams)
        {
            try
            {
                logger.Trace("RegisterDevice started");
                NotifiedDevice deviceInfo = NotifiedDevice.GetNotifiedDeviceFromPostData(inputParams.RequestData.postData.ToString());

                deviceInfo.UserId = inputParams._Account.userId;

                // DBWorker
                DBWorker.ClearTokensForDeviceAndUser(deviceInfo);

                bool isRegisteredNow = DBWorker.AddDeviceToUser(deviceInfo);

                // Response is a message ID string.
                logger.Debug($"Successfully register device: {deviceInfo.DeviceId} with token: {deviceInfo.RegToken}");
                return isRegisteredNow;
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                logger.Trace("RegisterDevice leave.");
            }
        }

        //ID целевого пользователя  - из текущего контекста
        public bool UnregisterDevice(FQRequestInfo inputParams)
        {
            try
            {
                logger.Trace("UnregisterDevice started");
                NotifiedDevice deviceInfo = NotifiedDevice.GetNotifiedDeviceFromPostData(inputParams.RequestData.postData.ToString());

                deviceInfo.UserId = inputParams._Account.userId;

                bool isRegisteredNow = true;

                if (deviceInfo.DeviceId == string.Empty)
                {
                    DBWorker.ClearTokensForUser(deviceInfo.UserId);
                    isRegisteredNow = false;
                }
                else
                {
                    isRegisteredNow = !DBWorker.RemoveDeviceForUser(deviceInfo);
                }

                // Response is a message ID string.
                logger.Debug($"Successfully unregister device: {deviceInfo.DeviceId} with token: {deviceInfo.RegToken} for user {deviceInfo.UserId}");
                return isRegisteredNow;
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                logger.Trace("UnregisterDevice leave.");
            }
        }

        //ID целевого пользователя - из параметра. Не вызывается снаружи
        public bool UnregisterDeviceInner(FQRequestInfo inputParams)
        {
            try
            {
                logger.Trace("UnregisterDeviceInner started");
                NotifiedDevice deviceInfo = NotifiedDevice.GetNotifiedDeviceFromPostData(inputParams.RequestData.postData.ToString());

                bool isRegisteredNow = true;

                if (deviceInfo.DeviceId == string.Empty)
                {
                    DBWorker.ClearTokensForUser(deviceInfo.UserId);
                    isRegisteredNow = false;
                }
                else
                {
                    isRegisteredNow = !DBWorker.RemoveDeviceForUser(deviceInfo);
                }

                // Response is a message ID string.
                logger.Debug($"Successfully unregister device: {deviceInfo.DeviceId} with token: {deviceInfo.RegToken} for user {deviceInfo.UserId}");
                return isRegisteredNow;
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                logger.Trace("UnregisterDeviceInner leave.");
            }
        }

        public bool SetSubscriptionForUser(FQRequestInfo inputParams)
        {
            try
            {
                logger.Trace("SetSubscriptionForUser started");

                var inputDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(inputParams.RequestData.postData.ToString());

                Guid.TryParse(inputDict["userId"], out Guid userId);
                bool.TryParse(inputDict["isSubscribed"], out bool isSubscribed);

                if (userId == null)
                    throw new Exception("Error while parsing input parameters");

                bool isSubscribedNow = DBWorker.ChangeSubForUser(userId, isSubscribed);

                logger.Debug("Successfully set subscription for user");

                return isSubscribedNow;

            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                logger.Trace("SetSubscriptionForUser leave.");
            }
        }

        public void NotifyUsers(FQRequestInfo inputParams)
        {
            try
            {
                logger.Trace("NotifyUsers started");

                logger.Trace($"RawData for sent: {inputParams.RequestData.postData.ToString()}");
                NotificationInfo info = NotificationInfo.Deserialize(inputParams.RequestData.postData.ToString());
                logger.Trace($"Info for sent: \nTitle:{info.Title}, \nBody: {info.Body}");
                var sendTask = SendMessageToUsers(info)
                    .ContinueWith((result) =>
                {                   
                    if (result == null)
                    {
                        logger.Debug($"Messages were not send.");
                        return;
                    }
                    logger.Debug($"Successfully sent multicast message to users. Success: ${result.Result.SuccessCount}, failure: ${result.Result.FailureCount}");
                    if (result.Result.FailureCount != 0)
                    {
                        var failedMsg = result.Result.Responses.Where((response) => { return !response.IsSuccess; });
                        foreach (var failed in failedMsg)
                        {
                            logger.Error($"Id: {failed.MessageId}" +
                                $"\n MessagingErrorCode: {failed.Exception.MessagingErrorCode} " +
                                $"\n HttpResponse: {failed.Exception.HttpResponse?.ToString()} " +
                                $"\n Message: {failed.Exception.Message}");
                        }
                    }
                });

                try
                {
                    sendTask.Wait(10000);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                logger.Trace("NotifyUsers leave.");
            }
        }

        private static Task<BatchResponse> SendMessageToUsers(NotificationInfo info)
        {
            // интепретируем info.TargetsIds как идентификаторы пользователей
            // получаем идентификаторы устройств для указанных пользователей (пока с того, с которого был последний логин)
            List<string> targetTokens = new List<string>();
            foreach (var userId in info.TargetsIds)
            {
                var devices = DBWorker.GetDeviceForUser(userId);
                targetTokens.AddRange(devices.Where((d) => !string.IsNullOrEmpty(d.RegToken)).Select((d) => d.RegToken));
            }
            // если не найдены устройства, на которые можно отправить оповещения, то просто выходим
            if (targetTokens.Count == 0)
            {
                logger.Trace("No device for sending");
                return new Task<BatchResponse>(() => { return null; });
            }
            logger.Trace($"Found device for sending: {targetTokens.Count}");
            // заполняем
            //var registrationTokens = new List<string>()
            //    {
            //        "ex63gRZPTbGlZoOYvT-5Ex:APA91bHeSjPSEe_aJ_1PrkTN7p5AD-bBCAWQEkP32qmmzhDJgyLsKYTZelLzwZyIOhELIKtzpiYpp_kBBQsNi2JffFSZZ8JwDEvs9EJQxsJ6uctdWeZ9tpaQc3ZY49D3cqGEuqciTAU-"
            //    };

            MulticastMessage message = PrepareMessage(info, targetTokens);
            message.Android.Priority = Priority.High;

            MulticastMessage message_Inner = PrepareMessage_Inner(info, targetTokens);

            // Send a message to the device corresponding to the provided
            // registration token.
            FirebaseMessaging.DefaultInstance.SendMulticastAsync(message_Inner);

            // Send a message to the device corresponding to the provided
            // registration token.
            return FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);
        }

        private static MulticastMessage PrepareMessage(NotificationInfo info, List<string> targetTokens)
        {
            try
            {
                logger.Trace("PrepareMessage started.");

                logger.Trace("info.Data.Count: ", info.Data.Count);

                // See documentation on defining a message payload.
                var message = new MulticastMessage()
                {
                    Data = (IReadOnlyDictionary<string, string>)info.Data,
                    Tokens = targetTokens,
                };

                message.Notification = PrepareNotification(info);
                message.Android = PrepareNotificationAndroid(info);
                //message.Apns = PrepareNotificationIOS(info);
                //message.Webpush = PrepareNotificationWeb(info);

                return message;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                logger.Trace("PrepareMessage leave.");
            }
        }

        private static MulticastMessage PrepareMessage_Inner(NotificationInfo info, List<string> targetTokens)
        {
            try
            {
                logger.Trace("PrepareMessage_Inner started.");

                logger.Trace("info.Data.Count: ", info.Data.Count);

                // See documentation on defining a message payload.
                var message = new MulticastMessage()
                {
                    Data = (IReadOnlyDictionary<string, string>)info.Data,
                    Tokens = targetTokens,
                };

                message.Notification = null;
                message.Android = PrepareNotificationAndroid(info);

                return message;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                logger.Trace("PrepareMessage_Inner leave.");
            }
        }

        private static Notification PrepareNotification(NotificationInfo info)
        {
            try
            {
                logger.Trace("PrepareNotification started.");

                // TODO: проработать содержимое оповещения
                string trimmedTitle = Regex.Replace(info.Title, "<.*?>", String.Empty);
                string trimmedBody = Regex.Replace(info.Body, "<.*?>", String.Empty);
                var notification = new Notification();
                notification.Title = trimmedTitle;
                notification.Body = trimmedBody;

                notification.ImageUrl = null;

                return notification;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                logger.Trace("PrepareNotification leave.");
            }
        }
        private static AndroidConfig PrepareNotificationAndroid(NotificationInfo info)
        {
            AndroidConfig androidConfig = new AndroidConfig();

            androidConfig.CollapseKey = "Main";

            return androidConfig;
        }

        private static ApnsConfig PrepareNotificationIOS(NotificationInfo info)
        {
            ApnsConfig apnsConfig = new ApnsConfig();
            
            return apnsConfig;
        }
        private static WebpushConfig PrepareNotificationWeb(NotificationInfo info)
        {
            WebpushConfig webpushConfig = new WebpushConfig();
                        
            return webpushConfig;
        }

        private static string GetImageUrl(HistoryEvent.ItemTypeEnum type)
        {
            string imageUrl = "http://familialquest.com/app/images/icons/";
            switch (type)
            {
                case HistoryEvent.ItemTypeEnum.Default:
                    imageUrl = "";
                    break;
                case HistoryEvent.ItemTypeEnum.Task:
                    imageUrl += "Task.png";
                    break;
                case HistoryEvent.ItemTypeEnum.Reward:
                    imageUrl += "Reward.png";
                    break;
                case HistoryEvent.ItemTypeEnum.User:
                    imageUrl += "User.png";
                    break;
                case HistoryEvent.ItemTypeEnum.Group:
                    imageUrl += "Group.png";
                    break;
            }
            return imageUrl;
        }

        public void NotifyGroup(FQRequestInfo inputParams)
        {
            try
            { 
                logger.Trace("NotifyGroup started");

                NotificationInfo info = NotificationInfo.Deserialize(inputParams.RequestData.postData.ToString());
                List<Task> tasks = new List<Task>();
                foreach (var groupID in info.TargetsIds)
                {
                    // интепретируем info.TargetsIds как идентификаторы групп
                    // получаем идентификаторы пользователей для указанных групп
                    // заполняем info.TargetsIds = 
                    //
                    // вызываем отправку для группы пользователей
                    tasks.Add(SendMessageToUsers(info));
                }

                Task.WaitAll(tasks.ToArray());

                // Response is a message ID string.
                logger.Debug("Successfully sent multicast message to groups");

            }
            catch (FQServiceException fqEx)
            {
                logger.Error(fqEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception(FQServiceExceptionType.DefaultError.ToString());
            }
            finally
            {
                logger.Trace("NotifyUsers leave.");
            }
        }
    }
}
