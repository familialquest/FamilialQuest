using System;
using UnityEngine;
using Ricimi;
using Code.Controllers.MessageBox;
using static Assets.Code.Models.Reward.BaseReward;
using Assets.Code.Models.Reward;
using Assets.Code.Controllers;
using Code.Models.REST;
using Code.Models;
using Code.Models.RoleModel;
using Assets.Code.Models.REST.CommonTypes;

public class RewardDetailsController : MonoBehaviour
{
    public DescriptionValuesController DescriptionValues;
    public TaskStatusIconController TaskStatusIcon;
    public ButtonGroupController ButtonGroupController;
    public HistoryGroupController HistoryGroupController;

    public RewardStatusIconController RewardStatus;

    public GameObject CircleProgressBar;

    private TextFieldsFiller m_textFieldsFiller;
    private Popup m_thisPopup;
    // Start is called before the first frame update
    void Start()
    {
        try
        {
            m_thisPopup = GetComponent<Popup>();
            m_textFieldsFiller = GetComponent<TextFieldsFiller>();

            var currentStatus = (BaseRewardStatus)Enum.Parse(typeof(BaseRewardStatus), m_textFieldsFiller.TextData["Status"]);

            RewardStatus.SetStatus(currentStatus, m_textFieldsFiller);

            if (CredentialHandler.Instance.CurrentUser.Role == RoleTypes.User)
            {
                if (currentStatus == BaseRewardStatus.Registered)
                {
                    HistoryGroupController.ShowStatus("CreationDate");

                    if (CanPurchaseFromString(m_textFieldsFiller.TextData["CanPurchase"].ToString()))
                    {
                        ButtonGroupController.ShowButton("Yes");
                    }
                    else
                    {
                        ButtonGroupController.ShowButton("No");
                    }
                }
                else
                {
                    ButtonGroupController.ShowButton("Understand");
                }

                if (currentStatus == BaseRewardStatus.Purchase)
                {
                    HistoryGroupController.ShowStatus("CreationDate");
                    HistoryGroupController.ShowStatus("PurchaseDate");
                }

                if (currentStatus == BaseRewardStatus.Handed)
                {
                    HistoryGroupController.ShowStatus("CreationDate");
                    HistoryGroupController.ShowStatus("PurchaseDate");
                    HistoryGroupController.ShowStatus("HandedDate");
                }
            }

            if (CredentialHandler.Instance.CurrentUser.Role == RoleTypes.Administrator)
            {
                if (currentStatus == BaseRewardStatus.Registered)
                {
                    HistoryGroupController.ShowStatus("CreationDate");

                    ButtonGroupController.ShowButton("Remove");
                }

                if (currentStatus == BaseRewardStatus.Purchase)
                {
                    HistoryGroupController.ShowStatus("CreationDate");
                    HistoryGroupController.ShowStatus("PurchaseDate");

                    ButtonGroupController.ShowButton("ConfirmHanded");
                }

                if (currentStatus == BaseRewardStatus.Handed)
                {
                    HistoryGroupController.ShowStatus("CreationDate");
                    HistoryGroupController.ShowStatus("PurchaseDate");
                    HistoryGroupController.ShowStatus("HandedDate");

                    ButtonGroupController.ShowButton("Remove");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    public void OnButton_Purchase()
    {
        try
        {
            Global_MessageBoxHandlerController.ShowMessageBox("Приобретение сокровища", string.Format("Будут потрачены монеты: <b>{0}</b>.\n\nПродолжить?", m_textFieldsFiller.TextData["CostLabel"]), MessageBoxType.Information, MessageBoxButtonsType.OkCancel)
                .Then((dialogRes) =>
                {
                    if (dialogRes == MessageBoxResult.Ok)
                    {
                        CircleProgressBar.SetActive(true);

                        var rewardId = Guid.Parse(m_textFieldsFiller.TextData["Id"]);

                        RewardController.PurchaseReward(rewardId)
                           .Then((res) =>
                           {
                               Debug.Log($"status: {res.status}");

                               if (res.result)
                               {
                                   Global_MessageBoxHandlerController.ShowMessageBox("Приобретение сокровища", "Сокровище ожидает вручения!");
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

    public void OnButton_ConfirmHanded()
    {
        try
        {
            Global_MessageBoxHandlerController.ShowMessageBox("Подтверждение вручения", "Будет подтверждено вручение сокровища Герою.\n\nПродолжить?", MessageBoxType.Information, MessageBoxButtonsType.OkCancel)
                .Then((dialogRes) =>
                {
                    if (dialogRes == MessageBoxResult.Ok)
                    {
                        CircleProgressBar.SetActive(true);

                        var rewardId = Guid.Parse(m_textFieldsFiller.TextData["Id"]);

                        RewardController.GiveReward(rewardId)
                            .Then((res) =>
                            {
                                Debug.Log($"status: {res.status}");

                                if (res.result)
                                {
                                    Global_MessageBoxHandlerController.ShowMessageBox("Подтверждение вручения", "Вручение подтверждено!");
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

    public void OnButton_Remove()
    {
        try
        {
            Global_MessageBoxHandlerController.ShowMessageBox("Удаление сокровища", "Будет выполнено удаление объявленного сокровища.\n\nПродолжить?", MessageBoxType.Information, MessageBoxButtonsType.OkCancel)
                .Then((dialogRes) =>
                {
                    if (dialogRes == MessageBoxResult.Ok)
                    {
                        CircleProgressBar.SetActive(true);

                        var rewardId = Guid.Parse(m_textFieldsFiller.TextData["Id"]);

                        RewardController.RemoveReward(rewardId)
                            .Then((res) =>
                            {
                                Debug.Log($"status: {res.status}");

                                if (res.result)
                                {
                                    Global_MessageBoxHandlerController.ShowMessageBox("Удаление сокровища", "Сокровище удалено!");
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
}
