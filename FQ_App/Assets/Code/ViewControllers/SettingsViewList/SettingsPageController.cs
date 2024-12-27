using Code.Models;
using System;
using TMPro;
using UnityEngine;
using Code.ViewControllers.TList;
using Code.Models.REST;
using UnityEngine.UI;
using Assets.Code.Models.REST.CommonTypes;
using Code.Controllers.MessageBox;
using Ricimi;
using Assets.Code.Models.REST.CommonTypes.Common;
using Assets.Code.Controllers;
using UnityEngine.SceneManagement;
using Code.Models.REST.Users;
using System.Collections.Generic;

namespace Code.ViewControllers
{
    public class SettingsPageController : MonoBehaviour
    {
        private static SettingsPageController staticThis;

        public GameObject thisForm;

        public PurchaseWorker thisPurchaseWorker;

        public PopupOpener HelloWorldPopup;

        public GameObject CircleProgressBar_Subscription;
        public GameObject CircleProgressBar_General;

        public TextMeshProUGUI Name;
        public TextMeshProUGUI Title;

        public GameObject ScrollRect_Params;
        public GameObject ScrollRect_Subscription;
        public GameObject ScrollRect_Info;

        public BaseSettingsListFilter TypeChooser_SettingsListFilter;

        public GameObject SoundOn;
        public GameObject SoundOff;
        public GameObject AlertOn;
        public GameObject AlertOff;

        public RectTransform Content;

        public TextMeshProUGUI SubscriptionStatus;
        public TextMeshProUGUI SubscriptionDetails;

        public GameObject DebugConsole;
        private int clickCounter = 0;

        /// <summary>
        /// Сколько нужно проскроллить вниз, чтоб запустить обновление списка
        /// </summary>
        public int SwipeDownLength = 200;

        private bool SoundStatus = true;

        // чтоб не обновлять одновременно
        bool isOnUpdate = false;

        void Awake()
        {
            try
            {
                Code.Controllers.NotificationController.Instance.updateUIButtonDelegate = UpdatePushStatusUI;

                TypeChooser_SettingsListFilter.OnFilterChanged += TypeChooser_SettingsListFilter_OnFilterChanged;

                //Для корректного выставления статуса настройки "Получать уведомления"
                if (Code.Controllers.NotificationController.Instance.CurrentRegToken != string.Empty)
                {
                    UpdatePushStatusUI();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                FQServiceException.ShowExceptionMessage(ex);
            }

            thisPurchaseWorker.InitializePurchasing();

            UpdateSubscriptionInfo();

            thisForm.SetActive(false);

            //isFirstLogin
            if (PlayerPrefs.GetInt(CredentialHandler.Instance.Credentials.Login, 0) == 0)
            {
                HelloWorldPopup.OpenPopup(out var popup);
            }

            staticThis = this;

            UpdateUserDetails();
        }

        private void Update()
        {
            try
            {
                if (!isOnUpdate && Content.transform.position.y < Content.parent.transform.position.y - SwipeDownLength)
                {            
                    UpdateSubscriptionInfo();                                     
                }

                if (Content.transform.position.y >= Content.parent.transform.position.y - SwipeDownLength)
                {
                    isOnUpdate = false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);

                isOnUpdate = false;
                CircleProgressBar_Subscription.SetActive(false);

                FQServiceException.ShowExceptionMessage(ex);
            }
        }

        //Выставление корректного статуса настройки "Получать уведомления"
        public void UpdatePushStatusUI()
        {
            Debug.Log("Init");
            if (Code.Controllers.NotificationController.Instance.CurrentSubscriptionStatus ||
               PlayerPrefs.GetInt("Push_SubscriptionStatus", 123) == 123) //нужно отличить первое включение от дизейбла
            {
                Code.Controllers.NotificationController.Instance.RegisterDevice()
                .Then((result) =>
                {
                    CircleProgressBar_General.SetActive(false);

                    AlertOn.SetActive(true);
                    AlertOff.SetActive(false);
                })
                .Catch((ex) =>
                {
                    Debug.LogError(ex);

                    AlertOn.SetActive(Code.Controllers.NotificationController.Instance.CurrentSubscriptionStatus);
                    AlertOff.SetActive(!Code.Controllers.NotificationController.Instance.CurrentSubscriptionStatus);

                    CircleProgressBar_General.SetActive(false);
                });
            }
            else
            {
                Code.Controllers.NotificationController.Instance.UnregisterDevice()
                    .Then((result) =>
                    {
                        CircleProgressBar_General.SetActive(false);

                        AlertOn.SetActive(false);
                        AlertOff.SetActive(true);
                    })
                    .Catch((ex) =>
                    {
                        Debug.LogError(ex);

                        AlertOn.SetActive(Code.Controllers.NotificationController.Instance.CurrentSubscriptionStatus);
                        AlertOff.SetActive(!Code.Controllers.NotificationController.Instance.CurrentSubscriptionStatus);

                        CircleProgressBar_General.SetActive(false);
                    });
            }
        }

        public void UpdateSubscriptionInfo()
        {
            try
            {
                isOnUpdate = true;

                CircleProgressBar_Subscription.SetActive(true);

                DataModel.Instance.GroupInfo.UpdateGroupItem()
                    .Then((result_UpdateGroupItem) =>
                    {
                        if (result_UpdateGroupItem.result)
                        {
                            if (DataModel.Instance.GroupInfo.MyGroup.SubscriptionIsActive)
                            {
                                SubscriptionStatus.text = "<b>Включен</b>";

                                //Возможна маловероятная ситуация, когда на сервере не удастся получить значение
                                //Пользователю необходимо просто обновить, и все будет ок
                                if (DataModel.Instance.GroupInfo.MyGroup.SubscriptionExpiredDate != CommonData.dateTime_FQDB_MinValue)
                                {                                   
                                    SubscriptionDetails.text = string.Format("{0}",
                                            DateTimePickerController.GetTextFromDate(DataModel.Instance.GroupInfo.MyGroup.SubscriptionExpiredDate.ToLocalTime(), true));
                                }
                                else
                                {
                                    SubscriptionDetails.text = "в обработке";
                                }
                                
                                SubscriptionDetails.text += "\n1000";
                                SubscriptionDetails.text += "\n1000";
                                SubscriptionDetails.text += "\n6";
                            }
                            else
                            {
                                SubscriptionStatus.text = "Отключен";
                                SubscriptionDetails.text = "без ограничений";
                                SubscriptionDetails.text += "\nне более 1";
                                SubscriptionDetails.text += "\nне более 1";
                                SubscriptionDetails.text += "\nне более 2";
                            }
                        }
                        else
                        {                            
                            CircleProgressBar_Subscription.SetActive(false);
                        }

                        // сбросить прокрутку списка
                        Content.transform.position = Content.parent.transform.position;

                        isOnUpdate = false;
                        CircleProgressBar_Subscription.SetActive(false);

                        return result_UpdateGroupItem;
                    })
                    .Catch((ex) =>
                    {
                        //Если сюда попали, то ошибку уже обработали в DataModel и показали уведомление
                        //Нужно только убрать индикацию загрузки
                        CircleProgressBar_Subscription.SetActive(false);
                    });
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);

                CircleProgressBar_Subscription.SetActive(false);

                FQServiceException.ShowExceptionMessage(ex);
            }
        }

        //TODO: чтобы не прокидывать эвент
        public static void UpdateUserDetails()
        {
            if (staticThis != null)
            {
                staticThis.Name.text = CredentialHandler.Instance.CurrentUser.Name;
                staticThis.Title.text = CredentialHandler.Instance.CurrentUser.Title;
            }
        }

        private void TypeChooser_SettingsListFilter_OnFilterChanged(object sender, EventArgs e)
        {
            ScrollRect_Params.SetActive(TypeChooser_SettingsListFilter.CurrentActiveFilter == BaseSettingsFilter.Params);
            ScrollRect_Subscription.SetActive(TypeChooser_SettingsListFilter.CurrentActiveFilter == BaseSettingsFilter.Subscription);
            ScrollRect_Info.SetActive(TypeChooser_SettingsListFilter.CurrentActiveFilter == BaseSettingsFilter.Info);
        }

        private void OnDestroy()
        {
            try
            {               
                // отпишемся от обновлений фильтра
                TypeChooser_SettingsListFilter.OnFilterChanged -= TypeChooser_SettingsListFilter_OnFilterChanged;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }        

        //public void ShowWizard_Parents()
        //{
        //    WizardBG.SetActive(true);
        //    Wizard_Parents.SetActive(true);           
        //}

        //public void ShowWizard_Childrens()
        //{
        //    WizardBG.SetActive(true);
        //    Wizard_Childrens.SetActive(true);
        //}

        public void OnClick_Change_SoundStatus()
        {
            if (SoundStatus)
            {
                SoundStatus = false;
                SoundOn.SetActive(false);
                SoundOff.SetActive(true);
            }
            else
            {
                SoundStatus = true;
                SoundOn.SetActive(true);
                SoundOff.SetActive(false);
            }
        }

        public void OnClick_Change_AlertStatus()
        {
            try
            {
                CircleProgressBar_General.SetActive(true);

                if (!Code.Controllers.NotificationController.Instance.CurrentSubscriptionStatus)
                {
                    Code.Controllers.NotificationController.Instance.RegisterDevice()
                        .Then((result) =>
                        {
                            CircleProgressBar_General.SetActive(false);

                            AlertOn.SetActive(true);
                            AlertOff.SetActive(false);
                        })
                        .Catch((ex) =>
                        {
                            Debug.LogError(ex);

                            AlertOn.SetActive(Code.Controllers.NotificationController.Instance.CurrentSubscriptionStatus);
                            AlertOff.SetActive(!Code.Controllers.NotificationController.Instance.CurrentSubscriptionStatus);

                            CircleProgressBar_General.SetActive(false);
                        });
                }
                else
                {
                    Code.Controllers.NotificationController.Instance.UnregisterDevice()
                        .Then((result) =>
                        {
                            CircleProgressBar_General.SetActive(false);
                            
                            AlertOn.SetActive(false);
                            AlertOff.SetActive(true);
                        })
                        .Catch((ex) =>
                        {
                            Debug.LogError(ex);

                            AlertOn.SetActive(Code.Controllers.NotificationController.Instance.CurrentSubscriptionStatus);
                            AlertOff.SetActive(!Code.Controllers.NotificationController.Instance.CurrentSubscriptionStatus);

                            CircleProgressBar_General.SetActive(false);
                        });
                }

                //Code.Controllers.NotificationController.Instance.ChangeSubscription(!AlertStatus)
                //    .Then((result) =>
                //    {
                //        CircleProgressBar_General.SetActive(false);
                //        AlertStatus = !AlertStatus;
                //        AlertOn.SetActive(AlertStatus);
                //        AlertOff.SetActive(!AlertStatus);
                //    })
                //    .Catch((ex) =>
                //    {
                //        Debug.LogError(ex);
                //        CircleProgressBar_General.SetActive(false);
                //    });

            }
            catch (Exception ex)
            {
                Debug.LogError(ex);

                CircleProgressBar_General.SetActive(false);

                FQServiceException.ShowExceptionMessage(ex);
            }

            //if (AlertStatus)
            //{
            //    AlertStatus = false;
            //    AlertOn.SetActive(false);
            //    AlertOff.SetActive(true);

            //    Code.Controllers.NotificationController.Instance.ChangeSubscription(false);
            //}
            //else
            //{
            //    AlertStatus = true;
            //    AlertOn.SetActive(true);
            //    AlertOff.SetActive(false);

            //    Code.Controllers.NotificationController.Instance.ChangeSubscription(true);
            //}
        }

        public void OnButton_OpenPurchaseMenu(PopupOpener subscriptionDetailsPopupOpener)
        {
            try
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    subscriptionDetailsPopupOpener.OpenPopup(out var popup);
                    SubscriptionDetailsController subscriptionDetailsController = popup.GetComponent<SubscriptionDetailsController>();
                    subscriptionDetailsController.purchaseWorker = thisPurchaseWorker;
                    subscriptionDetailsController.SetData();
                }
                else
                {
                    Global_MessageBoxHandlerController.ShowMessageBox("Премиум-доступ", "К сожалению, в данный момент покупки возможны только с устройства Android.", MessageBoxType.Information);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public void OnClick_ButtonLogout()
        {
            try
            {
                Global_MessageBoxHandlerController.ShowMessageBox("Выход", "Будет выполнен выход из аккаунта пользователя.\n\nПродолжить?", MessageBoxType.Information, MessageBoxButtonsType.OkCancel)
                    .Then((dialogRes) =>
                    {
                        if (dialogRes == MessageBoxResult.Ok)
                        {
                            CircleProgressBar_General.SetActive(true);

                            try
                            {
                                Controllers.NotificationController.Instance.Uninit();
                            }
                            catch
                            {
                                ;
                            }

                            AuthController.Logout()
                                .Then((res) =>
                                {
                                    Debug.Log($"status: {res.status}");
                                    DataModel.Instance.Credentials.Users = null;
                                    DataModel.Instance.GroupInfo.MyGroup = null;
                                    DataModel.Instance.Rewards.Rewards = null;
                                    DataModel.Instance.Tasks.Tasks = null;
                                    DataModel.Instance.HistoryEvents.HistoryEvents = null;

                                    CredentialHandler.Instance.Credentials = new UserCredentials();
                                    CredentialHandler.Instance.CurrentUser = new User();

                                    PlayerPrefs.SetString("Login", string.Empty);
                                    PlayerPrefs.SetString("AuthToken", string.Empty);

                                    SceneManager.LoadScene("StartPages", LoadSceneMode.Single);
                                })
                                .Catch((ex) =>
                                {
                                    Debug.LogError(ex);

                                    DataModel.Instance.Credentials.Users = null;
                                    DataModel.Instance.GroupInfo.MyGroup = null;
                                    DataModel.Instance.Rewards.Rewards = null;
                                    DataModel.Instance.Tasks.Tasks = null;
                                    DataModel.Instance.HistoryEvents.HistoryEvents = null;

                                    CredentialHandler.Instance.Credentials = new UserCredentials();
                                    CredentialHandler.Instance.CurrentUser = new User();

                                    PlayerPrefs.SetString("AuthToken", string.Empty);

                                    SceneManager.LoadScene("StartPages", LoadSceneMode.Single);
                                });
                        }
                    })
                    .Catch((ex) =>
                    {
                        Debug.LogError(ex);

                        DataModel.Instance.Credentials.Users = null;
                        DataModel.Instance.GroupInfo.MyGroup = null;
                        DataModel.Instance.Rewards.Rewards = null;
                        DataModel.Instance.Tasks.Tasks = null;
                        DataModel.Instance.HistoryEvents.HistoryEvents = null;

                        CredentialHandler.Instance.Credentials = new UserCredentials();
                        CredentialHandler.Instance.CurrentUser = new User();

                        PlayerPrefs.SetString("AuthToken", string.Empty);

                        try
                        {
                            Controllers.NotificationController.Instance.Uninit();
                        }
                        catch
                        {
                            ;
                        }

                        SceneManager.LoadScene("StartPages", LoadSceneMode.Single);
                    });
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);

                CircleProgressBar_General.SetActive(false);

                FQServiceException.ShowExceptionMessage(ex);
            }
        }

        public void OnClick_ButtonOpenDebugConsole()
        {
            clickCounter++;

            if (clickCounter >= 20)
            {
                clickCounter = 0;

                if (DebugConsole != null)
                {
                    DebugConsole.SetActive(true);
                }
            }
        }

        public void OnClick_OpenUserDetails(PopupOpener opener)
        {
            try
            {
                opener.OpenPopup(out GameObject popup);

                var item = popup.GetComponent<TextFieldsFiller>();

                var list = new List<User>();
                list.Add(CredentialHandler.Instance.CurrentUser);

                item.SetData(CredentialsModel.ToListOfDictionary<User>(list)[0]);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }        
    }
}