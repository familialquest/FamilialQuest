#if !UNITY_WEBGL

using UnityEngine;

using Firebase;
using Firebase.Messaging;

using Code.Models.REST.Notifications;
using Code.Models.REST;
using Code.Models;
using System;
using System.Threading.Tasks;
using Code.Controllers.MessageBox;
using Code.Models.REST.HistoryEvent;

namespace Code.Controllers
{
    public class NotificationController : ScriptableObject
    {
        private static NotificationController _instance = null;
        public static NotificationController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = ScriptableObject.CreateInstance<NotificationController>();

                return _instance;
            }
        }

        //Для апдейта UI после получения токена при первом входе, 
        //т.к. процесс может произойти уже после прогрузки UI
        public delegate void UpdateUIButton();
        public UpdateUIButton updateUIButtonDelegate = null;

        private bool _isFirebaseInitialized = false;
        public string CurrentRegToken
        {
            get => PlayerPrefs.GetString("Push_RegToken", string.Empty);
            set => PlayerPrefs.SetString("Push_RegToken", value);
        }
        public bool CurrentSubscriptionStatus
        {
            get => PlayerPrefs.GetInt("Push_SubscriptionStatus", 0) != 0;
            set => PlayerPrefs.SetInt("Push_SubscriptionStatus", value ? 1 : 0);
        }

        public bool Init()
        {
            Debug.Log("Init");
            var dependencyStatus = FirebaseApp.CheckDependencies();
            if (dependencyStatus == DependencyStatus.Available)
            {                         
                InitializeFirebase();               
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");

                // Firebase Unity SDK is not safe to use here.
                _isFirebaseInitialized = false;
            }

            Debug.Log($"Firebase Messaging init status: {_isFirebaseInitialized}");
            Debug.Log($"CurrentSubscriptionStatus: {CurrentSubscriptionStatus}");
            Debug.Log($"CurrentRegToken: {CurrentRegToken}");

            Debug.Log("Init leave");
            return _isFirebaseInitialized;
        }
        private bool InitializeFirebase()
        {
            Debug.Log("InitializeFirebase");
            if (_isFirebaseInitialized)
            {
                Debug.Log("Already inited");
                return _isFirebaseInitialized;
            }
            FirebaseMessaging.MessageReceived += OnMessageReceived;
            FirebaseMessaging.TokenReceived += OnTokenReceived;

            //Firebase.Messaging.FirebaseMessaging.Subscribe("TestTopic");

            _isFirebaseInitialized = true;
            Debug.Log("InitializeFirebase leave");
            return _isFirebaseInitialized;
        }

        public bool Uninit()
        {
            Debug.Log("Uninit");

            UnititializeFirebase();


            Debug.Log($"Firebase Messaging init status: {_isFirebaseInitialized}");
            Debug.Log($"CurrentSubscriptionStatus: {CurrentSubscriptionStatus}");
            Debug.Log($"CurrentRegToken: {CurrentRegToken}");

            Debug.Log("Uninit leave");
            return _isFirebaseInitialized;
        }        
        private bool UnititializeFirebase()
        {
            Debug.Log("UnititializeFirebase");
            if (!_isFirebaseInitialized)
            {
                Debug.Log("Already uninited");
                return _isFirebaseInitialized;
            }
            FirebaseMessaging.MessageReceived -= OnMessageReceived;
            FirebaseMessaging.TokenReceived -= OnTokenReceived;

            //Firebase.Messaging.FirebaseMessaging.Subscribe("TestTopic");

            _isFirebaseInitialized = false;
            Debug.Log("UnititializeFirebase leave");
            return _isFirebaseInitialized;
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            FirebaseMessaging.MessageReceived -= OnMessageReceived;
            FirebaseMessaging.TokenReceived -= OnTokenReceived;

            //FirebaseApp.DefaultInstance.Dispose();
        }

        private bool IsNotInit() => !_isFirebaseInitialized;

        public virtual void OnTokenReceived(object sender, TokenReceivedEventArgs token)
        {
            Debug.Log("OnTokenReceived");
            if (token.Token.Length > 10000)
            {
                Debug.LogError("token.Token is too long!");
                throw new Exception();
            }

            Debug.Log("Received Registration Token: " + token.Token);

            // запомним токен
            CurrentRegToken = token.Token;

            if (updateUIButtonDelegate != null)
            {
                updateUIButtonDelegate();
            }            

            Debug.Log("OnTokenReceived leave");
        }

        public virtual void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                if (e.Message == null)
                {
                    Debug.Log($"Received a new NULL message");
                    return;
                }

                if (e.Message.Notification == null)
                {
                    Debug.Log($"Received a new message with NULL notification");
                }
                else
                {
                    //В ситуации, когда аппка активна, для предотвращения дубляжа (обработки нотификационного пуша)
                    Debug.Log($"Received a new message with notification");
                    return;
                }

                if (e.Message.Data == null) Debug.Log($"Received a new message with NULL Data");
                if (e.Message.NotificationOpened == true)
                {
                    Debug.Log($"We come from notification center");
                    return;
                }

                Debug.Log($"Received a new message ({e.Message.MessageId}) " +
                    $"from: {e.Message.From}. " +
                    $"\nRaw message: {e.Message.Notification?.Title} - {e.Message.Notification?.Body}" +
                    $"\n ({e.Message.RawData})");

                if (e.Message.Data != null && e.Message.Data.ContainsKey("HistoryEvent"))
                {
                    Debug.Log("PUSH contains HistoryEvent");
                    Code.Models.REST.HistoryEvent.HistoryEvent historyEvent = Code.Models.REST.HistoryEvent.HistoryEvent.Deserialize(e.Message.Data["HistoryEvent"]);

                    UpdateCorrespondingList(historyEvent);

                    // TODO: вызвать показ иконки оповещения на флажке

                    DataModel.Instance.HistoryEvents.AddPushCount(1);

                    //Global_MessageBoxHandlerController.ShowMessageBox(
                    //    "Свежие вести!",
                    //    $"{historyEvent.EventTitle}\n...\n{historyEvent.EventText}",
                    //    MessageBoxType.Information,
                    //    MessageBoxButtonsType.Ok);
                }
                else if (e.Message.Notification != null)
                {
                    // TODO: вызвать показ иконки оповещения на флажке

                    DataModel.Instance.HistoryEvents.AddPushCount(1);

                    //Global_MessageBoxHandlerController.ShowMessageBox("Свежие вести!",
                    //    $"<b>{e.Message.Notification.Title}</b>" +
                    //    "\n...\n" +
                    //    $"{e.Message.Notification.Body}",
                    //    MessageBoxType.Information,
                    //    MessageBoxButtonsType.Ok);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        private static void UpdateCorrespondingList(HistoryEvent historyEvent)
        {
            Debug.Log("List to update: " + Enum.GetName(typeof(Models.REST.HistoryEvent.HistoryEvent.ItemTypeEnum), historyEvent.ItemType));
            switch (historyEvent.ItemType)
            {
                case Models.REST.HistoryEvent.HistoryEvent.ItemTypeEnum.Default:
                    break;
                case Models.REST.HistoryEvent.HistoryEvent.ItemTypeEnum.Task:
                    DataModel.Instance.Tasks.UpdateTasks();
                    DataModel.Instance.Credentials.UpdateAllCredentials();
                    break;
                case Models.REST.HistoryEvent.HistoryEvent.ItemTypeEnum.Reward:
                    DataModel.Instance.Rewards.UpdateAllRewardsItems();
                    DataModel.Instance.Credentials.UpdateAllCredentials();
                    break;
                case Models.REST.HistoryEvent.HistoryEvent.ItemTypeEnum.User:
                    DataModel.Instance.Credentials.UpdateAllCredentials();
                    break;
            }            
        }

        public RSG.IPromise<DataModelOperationResult> RegisterDevice()
        {
            Debug.Log("RegisterDevice");
            // зарегистрировать токен для текущего пользователя и устройства в NotificationService
            var promise = DataModel.Instance.Notifications.RegisterDevice(Guid.Empty, SystemInfo.deviceUniqueIdentifier, CurrentRegToken)
                .Then((result) =>
                {
                    Debug.Log($"status: {result.status}");

                    if (result.result)
                    {
                        Debug.Log($"This device reg status now is '{((RegisterDeviceResponse)result.ParsedResponse).Registered}'");

                        CurrentSubscriptionStatus = true;
                    }
                    else
                    {
                        //Exception пустой потому что просто для возврата промиса
                        throw new Exception();
                    }
                    return result;
                });

            return promise;
        }

        public RSG.IPromise<DataModelOperationResult> UnregisterDevice()
        {
            Debug.Log("UnregisterDevice");

            string deviceId = SystemInfo.deviceUniqueIdentifier;

            // удалить текущий девайс для текущего пользователя в NotificationService
            var promise = DataModel.Instance.Notifications.UnregisterDevice(Guid.Empty, deviceId, string.Empty)
                .Then((result) =>
                {
                    Debug.Log($"status: {result.status}");

                    if (result.result)
                    {
                        Debug.Log($"This device reg status now is '{((UnregisterDeviceResponse)result.ParsedResponse).Registered}'");

                        CurrentSubscriptionStatus = false;
                    }
                    else
                    {
                        //Exception пустой потому что просто для возврата промиса
                        throw new Exception();
                    }
                    return result;
                });

            return promise;
        }

        //TODO: зарезервирован на будущее
        //public RSG.IPromise<DataModelOperationResult> ChangeSubscription(bool isSubscribed)
        //{
        //    Debug.Log("ChangeSubscription");

        //    Debug.Log("isSubscribed: " + isSubscribed.ToString());

        //    CurrentSubscriptionStatus = isSubscribed;

        //    RSG.IPromise<DataModelOperationResult> promise = new RSG.Promise<DataModelOperationResult>();
            
        //    // пробуем запросить права на оповещения
        //    Init();

        //    // поменять статус подписки для текущего пользователя в NotificationService
        //    promise = DataModel.Instance.Notifications.SetSubscription(CredentialHandler.Instance.CurrentUser.Id, isSubscribed)
        //        .Then((result) =>
        //        {
        //            Debug.Log($"status: {result.status}");

        //            if (result.result)
        //            {
        //                Debug.Log($"This user subscription status now is '{((SetSubscriptionForUserResponse)result.ParsedResponse).IsSubscribed}'");

        //                // если подписка разрешилась, то регистрируем устройство
        //                if (true == CurrentSubscriptionStatus)
        //                {
        //                    Init();
        //                    _ = GetPermissionToNotificate();
        //                }
        //                else
        //                {
        //                    Uninit();
        //                }
        //            }
        //            else
        //            {
        //                //Exception пустой потому что просто для возврата промиса
        //                throw new Exception();
        //            }
        //            return result;
        //        });


        //    Debug.Log("ChangeSubscription leave");
        //    return promise;
        //}

        public Task GetPermissionToNotificate()
        {
            Debug.Log("GetPermissionToNotificate");
            if (IsNotInit()) 
                return null;

            // This will display the prompt to request permission to receive
            // notifications if the prompt has not already been displayed before. (If
            // the user already responded to the prompt, thier decision is cached by
            // the OS and can be changed in the OS settings).
            var task = FirebaseMessaging.RequestPermissionAsync();
            return task;
        }
    }
}
#else

using UnityEngine;
using Code.Models;
using System.Threading.Tasks;

namespace Code.Controllers
{
    public class NotificationController : ScriptableObject
    {
        private static NotificationController _instance = null;
        public static NotificationController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = ScriptableObject.CreateInstance<NotificationController>();

                return _instance;
            }
        }
        public bool CurrentSubscriptionStatus => false;

        public bool Init()
        {
            return true;
        }
        public bool Uninit()
        {
            return true;
        }

        private bool IsNotInit() => true;
        
        public RSG.IPromise<DataModelOperationResult> RegisterDevice()
        {
            return null;
        }
        public RSG.IPromise<DataModelOperationResult> UnregisterDevice()
        {
            return null;
        }

        public RSG.IPromise<DataModelOperationResult> ChangeSubscription(bool isSubscribed)
        {
            return null;
        }

        public Task GetPermissionToNotificate()
        {
            return null;
        }
    }
}
#endif