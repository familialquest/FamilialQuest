using UnityEngine;
using Ricimi;
using System.Collections.Generic;
using System;
using Code.Models.REST;
using Code.Models;
using System.Linq;
using Code.Controllers;
using Code.Controllers.MessageBox;
using Code.Models.RoleModel;
using Assets.Code.Models.REST.CommonTypes;

namespace Code.ViewControllers
{
    public class GroupDetailsController : MonoBehaviour
    {
        public DescriptionValuesController DescriptionValues;
        public TaskStatusIconController TaskStatusIcon;
        public ButtonGroupController ButtonGroupController;
        public GameObject CoinsInfo;
        public GameObject CoinsStub;
        public GameObject AccountInfo;
        public GameObject AccountStub;
        public GameObject OnlineIcon;

        public GameObject CircleProgressBar;

        private TextFieldsFiller m_textFieldsFiller;
        private Popup m_thisPopup;

        // Start is called before the first frame update
        void Start()
        {
            try
            {
                //Отключим. Если разрешено в текущенм контексте - включим ниже.
                AccountInfo.SetActive(false);
                AccountStub.SetActive(true);

                m_thisPopup = GetComponent<Popup>();
                m_textFieldsFiller = GetComponent<TextFieldsFiller>();

                //Онлайн-статус
                if (m_textFieldsFiller.TextData.ContainsKey("LastAction") && Int32.TryParse(m_textFieldsFiller.TextData["LastAction"], out int lastActionMinutesAgo))
                {
                    if (lastActionMinutesAgo >= 0 && lastActionMinutesAgo <= 15)
                    {
                        OnlineIcon.SetActive(true);
                    }
                    else
                    {
                        OnlineIcon.SetActive(false);
                    }
                }
                else
                {
                    OnlineIcon.SetActive(false);
                }

                //Поля и кнопки
                var destinationUser = DataModel.Instance.Credentials.Users.Where(x => x.Id == Guid.Parse(m_textFieldsFiller.TextData["Id"])).FirstOrDefault();

                if (destinationUser != null &&
                    destinationUser.Id != Guid.Empty)
                {
                    //Скрытие показа монет.
                    //Если хватит прав - покажутся ниже
                    CoinsInfo.SetActive(false);
                    CoinsStub.SetActive(true);

                    if (CredentialHandler.Instance.CurrentUser.Role == RoleTypes.User)
                    {
                        //Если пользак смотрит свою инфу - всё показываем
                        if (destinationUser.Id == CredentialHandler.Instance.Credentials.userId)
                        {
                            CoinsInfo.SetActive(true);
                            CoinsStub.SetActive(false);

                            AccountInfo.SetActive(true);
                            AccountStub.SetActive(false);

                            ButtonGroupController.ShowButton("Edit");
                        }
                        else
                        {
                            ButtonGroupController.ShowButton("Understand");
                        }                        
                    }

                    if (CredentialHandler.Instance.CurrentUser.Role == RoleTypes.Administrator)
                    {
                        //Если родитель смотрит инфу ребенка - показываем инфу о монетках
                        if (destinationUser.Role == RoleTypes.User)
                        {
                            CoinsInfo.SetActive(true);
                            CoinsStub.SetActive(false);
                        }

                        AccountInfo.SetActive(true);
                        AccountStub.SetActive(false);

                        if (destinationUser.Id != CredentialHandler.Instance.Credentials.userId)
                        {
                            if (destinationUser.Role == RoleTypes.User)
                            {
                                ButtonGroupController.ShowButton("RemoveRedact");
                            }
                            else //в данный момент - Если 1
                            {
                                ButtonGroupController.ShowButton("Delete");
                            }
                        }
                        else
                        {
                            if (destinationUser != null &&
                            destinationUser.Id != Guid.Empty &&
                            destinationUser.Id == CredentialHandler.Instance.Credentials.userId)
                            {
                                ButtonGroupController.ShowButton("Edit");
                            }
                            else
                            {
                                ButtonGroupController.ShowButton("Understand");
                            }
                        }
                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        

        //var currentStatus = BaseReward.StatusFromString(m_textFieldsFiller.TextData["Status"].ToString());

        //if (CredentialHandler.Instance.CurrentUser.Role == 0)
        //{
        //    if (currentStatus == BaseRewardStatus.Registered)
        //    {
        //        HistoryGroupController.ShowStatus("CreationDate");

        //        if (CanPurchaseFromString(m_textFieldsFiller.TextData["CanPurchase"].ToString()))
        //        {
        //            ButtonGroupController.ShowButton("Yes");
        //        }
        //        else
        //        {
        //            ButtonGroupController.ShowButton("No");
        //        }
        //    }
        //    else
        //    {
        //        ButtonGroupController.ShowButton("Understand");
        //    }

        //    if (currentStatus == BaseRewardStatus.Purchase)
        //    {
        //        HistoryGroupController.ShowStatus("CreationDate");
        //        HistoryGroupController.ShowStatus("PurchaseDate");
        //    }

        //    if (currentStatus == BaseRewardStatus.Handed)
        //    {
        //        HistoryGroupController.ShowStatus("CreationDate");
        //        HistoryGroupController.ShowStatus("PurchaseDate");
        //        HistoryGroupController.ShowStatus("HandedDate");
        //    }
        //}

        //if (CredentialHandler.Instance.CurrentUser.Role == 1)
        //{
        //    if (currentStatus == BaseRewardStatus.Registered)
        //    {
        //        HistoryGroupController.ShowStatus("CreationDate");

        //        ButtonGroupController.ShowButton("Remove");
        //    }

        //    if (currentStatus == BaseRewardStatus.Purchase)
        //    {
        //        HistoryGroupController.ShowStatus("CreationDate");
        //        HistoryGroupController.ShowStatus("PurchaseDate");

        //        ButtonGroupController.ShowButton("ConfirmHanded");
        //    }

        //    if (currentStatus == BaseRewardStatus.Handed)
        //    {
        //        HistoryGroupController.ShowStatus("CreationDate");
        //        HistoryGroupController.ShowStatus("PurchaseDate");
        //        HistoryGroupController.ShowStatus("HandedDate");

        //        ButtonGroupController.ShowButton("Understand");
        //    }
        //}

        public void OnButton_OK()
        {
            try
            {
                m_thisPopup.Close();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }
        public void OnButton_Edit(PopupOpener editPopupOpener)
        {
            try
            {
                editPopupOpener.OpenPopup(out var popup);
                PopupCredentialCreateController editCredentialPageController = popup.GetComponent<PopupCredentialCreateController>();
                editCredentialPageController.SetEditMode(true);
                editCredentialPageController.SetData(m_textFieldsFiller.Data);
                editCredentialPageController.AfterCredentialsChangedDelegate = AfterCredentialsEdited;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public void OnButton_EditAccountInfo(PopupOpener editPopupOpener)
        {
            try
            {
                editPopupOpener.OpenPopup(out var popup);
                PopupCredentialEditAccountController editAccountPageController = popup.GetComponent<PopupCredentialEditAccountController>();
                //editAccountPageController.SetEditMode(true);
                editAccountPageController.SetDestinationUserId(m_textFieldsFiller.TextData["Id"]);
                editAccountPageController.AfterCredentialsChangedDelegate = AfterCredentialsEdited;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        public void OnButton_Delete()
        {
            try
            {
                Global_MessageBoxHandlerController.ShowMessageBox("Удаление пользователя", "Будет выполнено удаление пользователя и всех связанных данных.\n\nПродолжить?", MessageBoxType.Information, MessageBoxButtonsType.OkCancel)
                        .Then((dialogRes) =>
                        {
                            if (dialogRes == MessageBoxResult.Ok)
                            {
                                CircleProgressBar.SetActive(true);

                                var userId = Guid.Parse(m_textFieldsFiller.TextData["Id"]);

                                CredentialsController.RemoveUser(userId)
                                    .Then((res) =>
                                    {
                                        Debug.Log($"status: {res.status}");

                                        if (res.result)
                                        {
                                            DataModel.Instance.Rewards.UpdateAllRewardsItems();
                                            DataModel.Instance.Tasks.UpdateTasks();

                                            Global_MessageBoxHandlerController.ShowMessageBox("Удаление пользователя", $" Пользователь <b>{m_textFieldsFiller.TextData["Name"]}</b> успешно удален.");

                                            m_thisPopup.Close();
                                        }
                                        else
                                        {
                                            CircleProgressBar.SetActive(false);
                                        }
                                    })
                                    .Catch((ex) =>
                                    {
                                        //Если сюда попали, то ошибку уже обработали в DataModel и показали уведомление
                                        //Нужно только убрать индикацию загрузки
                                        CircleProgressBar.SetActive(false);
                                    });
                            }
                        })
                        .Catch((ex) =>
                        {
                            Debug.LogError(ex);

                            CircleProgressBar.SetActive(false);

                            FQServiceException.ShowExceptionMessage(ex);
                        });
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);

                CircleProgressBar.SetActive(false);

                FQServiceException.ShowExceptionMessage(ex);
            }
        }


        private void AfterCredentialsEdited(Dictionary<string, object> newProps)
        {
            try
            {
                m_textFieldsFiller.SetData(newProps);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }
    }

}